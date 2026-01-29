Shader "Custom/VerticalGradient"
{
    Properties
    {
        _TopColor ("Top Color", Color) = (1,1,0,1)
        _BottomColor ("Bottom Color", Color) = (1,1,0,0)
        _FlipUV ("Flip UV", Float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent+10" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Offset -1, -1

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
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            float4 _TopColor;
            float4 _BottomColor;
            float _FlipUV;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float t = i.uv.y;

                // Si _FlipUV == 1 → invertimos el gradiente
                if (_FlipUV > 0.5)
                    t = 1 - t;

                return lerp(_BottomColor, _TopColor, t);
            }
            ENDCG
        }
    }
}
