Shader "iPlay/PavementPhoto"
{
    Properties { _MainTex ("Environment crop", 2D) = "white" {} }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200
        CGPROGRAM
        #pragma surface surf Photo fullforwardshadows noambient novertexlights nolightmap noforwardadd
        sampler2D _MainTex;
        struct Input { float2 uv_MainTex; };
        float2 tileOffset(float2 cell)
        {
            return frac(sin(float2(dot(cell, float2(127.1, 311.7)), dot(cell, float2(269.5, 183.3)))) * 43758.5453) * 7;
        }
        half4 LightingPhoto(SurfaceOutput s, half3 lightDir, half attenuation)
        {
            // The photograph already contains illumination; only live occlusion is added.
            return half4(s.Albedo * lerp(0.45h, 1.0h, saturate(attenuation)), s.Alpha);
        }
        void surf(Input IN, inout SurfaceOutput o)
        {
            float2 p = IN.uv_MainTex;
            float2 cell = floor(p);
            float2 blend = smoothstep(0.15, 0.85, frac(p));
            float3 a = tex2D(_MainTex, p + tileOffset(cell)).rgb;
            float3 b = tex2D(_MainTex, p + tileOffset(cell + float2(1, 0))).rgb;
            float3 c = tex2D(_MainTex, p + tileOffset(cell + float2(0, 1))).rgb;
            float3 d = tex2D(_MainTex, p + tileOffset(cell + float2(1, 1))).rgb;
            o.Albedo = lerp(lerp(a, b, blend.x), lerp(c, d, blend.x), blend.y);
            o.Alpha = 1;
        }
        ENDCG
    }
    Fallback "Diffuse"
}
