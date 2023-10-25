Shader "Unlit/Grass"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Data ("data", 2D) = "black" {}
        _WindSpeed ("Wind Speed", float) = 1
        _WaveLength ("Wave length", float) = 1
        _WaveAmplitude ("Wave Amplitude", float) = 1
    }
    SubShader
    {
        Tags {"Queue" = "Transparent" "RenderMode"="Transparent" }
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

            sampler2D _MainTex;
            sampler2D _Data;
            float4 _MainTex_ST;

            float _WindSpeed;
            float _WaveLength;
            float _WaveAmplitude;

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

                float deltaX = sin(_Time.x * _WindSpeed + i.vertex.x * _WaveLength)
                             * sin(_Time.x * _WindSpeed * 0.3 + i.vertex.x * _WaveLength) * _WaveAmplitude * data.x;

                fixed4 col = tex2D(_MainTex, float2(i.uv.x + deltaX, i.uv.y));

                if (col.a < 0.2) discard;
                return col;
            }
            ENDCG
        }
    }
}
