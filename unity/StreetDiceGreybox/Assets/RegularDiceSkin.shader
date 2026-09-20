Shader "iPlay/Regular Dice Skin"
{
    Properties
    {
        baseColorTexture ("Albedo", 2D) = "white" {}
        normalTexture ("Normal", 2D) = "bump" {}
        metallicRoughnessTexture ("Metallic Roughness", 2D) = "white" {}
        occlusionTexture ("Occlusion", 2D) = "white" {}
        baseColorFactor ("Source Color", Color) = (1,1,1,1)
        normalTexture_scale ("Normal Scale", Float) = 1
        metallicFactor ("Metallic", Range(0,1)) = 1
        roughnessFactor ("Roughness", Range(0,1)) = 1
        occlusionTexture_strength ("Occlusion Strength", Range(0,1)) = 1
        _BodyColor ("Body Color", Color) = (1,1,1,1)
        _PipColor ("Pip Color", Color) = (1,1,1,1)
        _Recolor ("Recolor", Range(0,1)) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Cull Off
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows vertex:vert
        #pragma target 3.0
        #include "UnityStandardUtils.cginc"
        sampler2D baseColorTexture, normalTexture, metallicRoughnessTexture, occlusionTexture;
        fixed4 baseColorFactor, _BodyColor, _PipColor;
        half normalTexture_scale, metallicFactor, roughnessFactor, occlusionTexture_strength, _Recolor;
        struct Input { float2 diceUv; };
        void vert(inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            o.diceUv = v.texcoord.xy;
        }
        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            float2 uv = IN.diceUv;
            fixed3 source = tex2D(baseColorTexture, uv).rgb * baseColorFactor.rgb;
            half luminance = dot(source, half3(0.2126, 0.7152, 0.0722));
            half bodyMask = smoothstep(0.15, 0.65, luminance);
            fixed3 colored = lerp(_PipColor.rgb * lerp(0.8, 1.0, 1.0 - luminance), source * _BodyColor.rgb, bodyMask);
            o.Albedo = lerp(source, colored, _Recolor) * 0.65;
            o.Normal = UnpackScaleNormal(tex2D(normalTexture, uv), normalTexture_scale);
            fixed3 packed = tex2D(metallicRoughnessTexture, uv).rgb;
            // The regular dice are resin, not polished metal.
            o.Metallic = 0;
            o.Smoothness = clamp(1.0 - packed.g * roughnessFactor, 0.12, 0.42);
            o.Occlusion = lerp(1.0, tex2D(occlusionTexture, uv).r, occlusionTexture_strength);
            o.Alpha = 1;
        }
        ENDCG
    }
    FallBack "Standard"
}
