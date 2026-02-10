Shader "Custom/RimHighlight"
{
    Properties
    {
        _RimColor ("Highlight Color", Color) = (0,1,0,0.5)
        _FillIntensity ("Fill Intensity", Range(0, 1)) = 0.2
        _PulseSpeed ("Pulse Speed", Range(0, 5)) = 1.0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100

        Pass
        {
            Name "Highlight"
            Blend SrcAlpha OneMinusSrcAlpha // Standard transparency
            ZWrite Off
            ZTest LEqual

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 normal : TEXCOORD0;
                float3 viewDir : TEXCOORD1;
            };

            float4 _RimColor;
            float _FillIntensity;
            float _PulseSpeed;
            float _PulseMin;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.normal = UnityObjectToWorldNormal(v.normal);
                o.viewDir = normalize(WorldSpaceViewDir(v.vertex));
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 normal = normalize(i.normal);
                float3 viewDir = normalize(i.viewDir);
                
                // Rim effect
                float rim = 1.0 - saturate(dot(normal, viewDir));
                rim = pow(rim, 3.0);
                
                // Pulse effect: Oscillate smoothly between _PulseMin and 1.0
                float sineWave = 0.5 * (sin(_Time.y * _PulseSpeed) + 1.0); // 0 to 1
                float pulse = lerp(_PulseMin, 1.0, sineWave);
                
                // Combine: Fill + Rim
                // We apply the pulse to the overall alpha to make it "dim" and "brighten"
                float alpha = (_FillIntensity + rim) * _RimColor.a * pulse;
                
                return float4(_RimColor.rgb, saturate(alpha));
            }
            ENDCG
        }
    }
}
