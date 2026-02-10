Shader "Custom/UIBlur"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BlurSize ("Blur Size", Float) = 1.0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float _BlurSize;

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = fixed4(0,0,0,0);
                float2 uv = i.uv;

                float offset = _BlurSize * _MainTex_TexelSize.xy.x;

                col += tex2D(_MainTex, uv + float2(-offset, -offset)) * 0.0625;
                col += tex2D(_MainTex, uv + float2(0, -offset)) * 0.125;
                col += tex2D(_MainTex, uv + float2(offset, -offset)) * 0.0625;

                col += tex2D(_MainTex, uv + float2(-offset, 0)) * 0.125;
                col += tex2D(_MainTex, uv) * 0.25;
                col += tex2D(_MainTex, uv + float2(offset, 0)) * 0.125;

                col += tex2D(_MainTex, uv + float2(-offset, offset)) * 0.0625;
                col += tex2D(_MainTex, uv + float2(0, offset)) * 0.125;
                col += tex2D(_MainTex, uv + float2(offset, offset)) * 0.0625;

                return col;
            }
            ENDCG
        }
    }
}
