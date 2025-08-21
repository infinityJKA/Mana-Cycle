Shader "Unlit/SpookyOutline"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _NoiseTex ("Texture", 2D) = "white" {}
        _TimeScale ("TimeScale", Float) = 1.0
        _InnerColor ("InnerColor", Color) = (1.0, 1.0, 1.0, 1.0)
        _OuterColor1 ("OuterColor1", Color) = (1.0, 1.0, 1.0, 1.0)
        _OuterColor2 ("OuterColor2", Color) = (1.0, 1.0, 1.0, 1.0)
        _Radius ("Radius", Float) = 0.1
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }
    SubShader
    {
        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off         
        ZTest [unity_GUIZTestMode]
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

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
            sampler2D _NoiseTex;
            float _TimeScale;
            float4 _InnerColor;
            float4 _OuterColor1;
            float4 _OuterColor2;
            float _t;
            float _Radius;
            float4 _ClipRect;

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = fixed4(1.0, 1.0, 1.0, 0.0);
                fixed2 deltas[8] = {
                    fixed2(1, 0),
                    fixed2(0, 1),
                    fixed2(-1, 0),
                    fixed2(0, -1),
                    fixed2(1, 1),
                    fixed2(-1, -1),
                    fixed2(1, -1),
                    fixed2(-1, 1),
                };

                float4 noise = tex2D(_NoiseTex, fmod(i.uv + fixed2(sin(_Time.y * _TimeScale * 1.5), -_Time.y * _TimeScale), 1.0));

                float d = 0;

                for (int ind = 0; ind < 8; ind++)
                {
                    float offset = noise.r * 0.015;
                    fixed2 delta = deltas[ind] * _Radius;
                    fixed4 sampleA = tex2D(_MainTex, i.uv + delta + offset);
                    fixed4 sampleB = tex2D(_MainTex, i.uv + offset);
    
                    d += max(sampleA.a - sampleB.a, 0);
                    d += max(sampleB.a - sampleA.a, 0);
                    col = _InnerColor;
                    col.a = sampleB.a * _InnerColor.a;
                }

                col += (lerp(_OuterColor1, _OuterColor2, noise.g) - _InnerColor) * d;

                col.a = min(col.a, 1.0) * 0.47;
                // col = noise;

                return col;
            }
            ENDCG
        }
    }
}
