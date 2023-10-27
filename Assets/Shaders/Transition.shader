Shader "Unlit/Transition"
{
    Properties
    {
        _Color ("Color", Color) = (0, 0, 0, 1)
        _Size ("Size", float) = 1
    }
    SubShader
    {
        Tags {"Queue" = "Transparent" "RenderType"="Transparent" }
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
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            float4 _Color;
            float _Size;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 centered = i.uv - float2(0.5, 0.5);
                centered.x *= _ScreenParams.x / _ScreenParams.y;
                float sqrDist = centered.x * centered.x + centered.y * centered.y;
                
                if (sqrDist < _Size * _Size)
                    discard;

                return _Color;
            }
            ENDCG
        }
    }
}
