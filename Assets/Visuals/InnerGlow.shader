Shader "Unlit/InnerGlow"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,0.75)
        _Size ("Size", Range(1.0, 10.0)) = 5.0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

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
            float4 _MainTex_ST;
            float4 _Color;
            float _Size;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);

                fixed4 colL = tex2D(_MainTex, (i.uv - 0.5) * _Size - fixed2(_MainTex_ST.z, 0) + 0.5);
                fixed4 colR = tex2D(_MainTex, (i.uv - 0.5) * _Size + fixed2(_MainTex_ST.z, 0) + 0.5);
                fixed4 colU = tex2D(_MainTex, (i.uv - 0.5) * _Size - fixed2(0, _MainTex_ST.w) + 0.5);
                fixed4 colD = tex2D(_MainTex, (i.uv - 0.5) * _Size + fixed2(0, _MainTex_ST.w) + 0.5);

                float threshold = (colR.a * colL.a * colU.a * colD.a);

                col.rgb = 1;
                if ((threshold < 0.1) && (col.a > 0)) col.a = 1;
                else col.a = 0;

                return col * _Color;
            }
            ENDCG
        }
    }
}
