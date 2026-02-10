Shader "Custom/ButtonVerticalGradient"
{
    Properties
    {
        _MainTex("Base Map", 2D) = "white" {}
        _TopColor("Top Color", Color) = (0.3,0.65,1,1)
        _BottomColor("Bottom Color", Color) = (0.42,0.36,1,1)
        _FlipUV("Flip UV", Float) = 0
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

            sampler2D _MainTex;
            float4 _TopColor;
            float4 _BottomColor;
            float _FlipUV;

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

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float t = i.uv.y;
                if (_FlipUV > 0.5) t = 1 - t;
                fixed4 grad = lerp(_BottomColor, _TopColor, t);
                fixed4 tex = tex2D(_MainTex, i.uv);
                return grad * tex;
            }
            ENDCG
        }
    }
}
