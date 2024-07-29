Shader "Unlit/CheckerBackground"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ReplaceColor("Replace Color", Color) = (1.0, 1.0, 1.0, 1.0)
        _Color1("Color 1", Color) = (1.0, 0.0, 0.0, 1.0)
        _Color2("Color 2", Color) = (0.0, 0.0, 1.0, 1.0)
        _Tolerance("Tolerance", Float) = 0.1

        _ScaleX("Scale X", Float) = 2.0
        _ScaleY("Scale Y", Float) = 2.0

        _SpeedX("Speed X", Float) = 1.0
        _SpeedY("Speed Y", Float) = 1.0

    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

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
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _ReplaceColor;
            float4 _Color1;
            float4 _Color2;
            float _Tolerance;

            float _ScaleX;
            float _ScaleY;

            float _SpeedX;
            float _SpeedY;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float aspect = _ScreenParams.x / _ScreenParams.y;
                i.uv.x = i.uv.x * aspect;

                fixed4 col = tex2D(_MainTex, i.uv);
                float x = abs(i.uv.x + _Time * _SpeedX) * _ScaleX % 1.0;
                float y = abs(i.uv.y + _Time * _SpeedY) * _ScaleY % 1.0;

                if (length(col.rgb - _ReplaceColor.rgb) < _Tolerance)
                {
                    col = ( (step(x, 0.5) + step (y, 0.5) < 0.9) || (step(0.5, x) + step(0.5, y) < 0.9) ? _Color1 : _Color2);
                }

                return col;
            }
            ENDCG
        }
    }
}
