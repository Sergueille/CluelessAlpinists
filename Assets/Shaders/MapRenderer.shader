Shader "Unlit/MapRenderer"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Cloud ("Cloud", 2D) = "white" {}
        _Color ("Color", Color) = (1, 1, 1, 1)
    }
    SubShader
    {
        Tags {"Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
            };

            sampler2D _MainTex;
            sampler2D _Cloud;
            float4 _MainTex_ST;

            float4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float4 cur = tex2D(_MainTex, i.uv);
                float4 up = tex2D(_MainTex, i.uv + float2(0.001, 0.003));
                
                float attAmount = 10;
                float att = clamp(i.uv.x * attAmount, 0, 1)
                            * clamp(attAmount - i.uv.x * attAmount, 0, 1)
                            * clamp(i.uv.y * attAmount, 0, 1)
                            * clamp(attAmount - i.uv.y * attAmount, 0, 1);

                // float4 cloudTex = tex2D(_Cloud, i.uv);
                // float4 cloudTexUp = tex2D(_Cloud, i.uv + float2(0.001, 0.003));
                // float cloudDiff = abs(cloudTex.r - cloudTexUp.r) * 4;

                float2 centerUV = i.uv - float2(0.5, 0.5);

                // float cloud = 1 - clamp(0.5 - length(i.uv - float2(0.5, 0.5)), 0, 1);
                float att2 = clamp(0.5 - centerUV.x - centerUV.y, 0, 1);

                float val = abs(cur.r - up.r) + abs(cur.g - up.g) + abs(cur.b - up.b);
                val = clamp(sqrt(val) * 1, 0, 1);

                // val += cloud > 0.99 ? 1 : 0;

                val *= att;

                return float4(0, 0, 0, val);
                // return float4(float3(0.878, 0.729, 0.494) * (1 - val), 1);
            }

            ENDCG
        }
    }
}
