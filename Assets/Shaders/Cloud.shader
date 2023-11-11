Shader "Unlit/Cloud"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _WindSpeed ("Wind Speed", float) = 1
        _Alpha ("Alpha", float) = 1
        _WaveLength ("Wave length", float) = 1
        _WaveAmplitude ("Wave Amplitude", float) = 1
    }
    SubShader
    {
        Tags {"Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100
        Blend SrcAlpha One

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

            sampler2D _MainTex;
            sampler2D _Data;
            float4 _MainTex_ST;

            float _WindSpeed;
            float _WaveLength;
            float _WaveAmplitude;
            float _Alpha;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 data = tex2D(_Data, i.uv);

                float time = _Time.x;
                float deltaX = sin(time * _WindSpeed + i.uv.x * _WaveLength) * sin(time * _WindSpeed * 0.9 + i.uv.y * _WaveLength) * _WaveAmplitude;
                float deltaY = sin(time * _WindSpeed * 0.9 + i.uv.x * _WaveLength * 0.9) * sin(time * _WindSpeed * 0.8 + i.uv.y * _WaveLength * 0.9) * _WaveAmplitude;

                fixed4 col = tex2D(_MainTex, float2(i.uv.x + deltaX, i.uv.y + deltaY));
                col.a *= _Alpha;

                return col;
            }
            ENDCG
        }
    }
}
