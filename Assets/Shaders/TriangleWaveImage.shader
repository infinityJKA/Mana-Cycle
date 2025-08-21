Shader "Unlit/TriangleWaveImage"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _TimeScale ("TimeScale", Float) = 1.0
        _Color ("Color", Color) = (1.0, 1.0, 1.0, 1.0)
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha

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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            float _TimeScale;
            float4 _Color;
            float _t;

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);

                float y = abs(((i.uv.y + _Time * _TimeScale) % 0.1) - 0.05) + 0.6;
                col.a = step(1.0, i.uv.x + y);
                col *= _Color;

                return col;
            }
            ENDCG
        }
    }
}
