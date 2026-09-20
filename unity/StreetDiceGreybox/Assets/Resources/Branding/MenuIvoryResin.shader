Shader "iPlay/Polished Ivory Resin"
{
    Properties
    {
        _MainTex("Worn Ivory", 2D) = "white" {}
        _Color("Tint", Color) = (1, 1, 1, 1)
        _Smoothness("Polish", Range(0, 1)) = 0.76
        _EdgeSheen("Edge Sheen", Range(0, 1)) = 0.18
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        CGPROGRAM
        #pragma surface surf StandardSpecular fullforwardshadows
        #pragma target 3.0

        sampler2D _MainTex;
        fixed4 _Color;
        half _Smoothness;
        half _EdgeSheen;

        struct Input
        {
            float2 uv_MainTex;
            float3 worldNormal;
            float3 viewDir;
        };

        void surf(Input i, inout SurfaceOutputStandardSpecular o)
        {
            fixed3 ivory = tex2D(_MainTex, i.uv_MainTex).rgb * _Color.rgb;
            half brightness = dot(ivory, half3(0.3, 0.59, 0.11));
            half grazing = 1.0 - saturate(dot(normalize(i.worldNormal), normalize(i.viewDir)));
            o.Albedo = ivory;
            o.Specular = half3(0.11, 0.105, 0.095);
            o.Smoothness = saturate(_Smoothness - (1.0 - brightness) * 0.25);
            o.Emission = pow(grazing, 4.0) * _EdgeSheen * half3(1.0, 0.96, 0.90);
            o.Alpha = 1.0;
        }
        ENDCG
    }
    Fallback "Standard"
}
