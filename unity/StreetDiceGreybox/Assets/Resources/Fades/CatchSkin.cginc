float4 _SkinGrade;

float CatchSkinMask(float3 c)
{
    // Specific to the warm hand and neutral pavement in the approved clips.
    return smoothstep(0.012, 0.04, c.r - c.g) * smoothstep(0.025, 0.065, c.r - c.b);
}

float3 GradeCatchSkin(float3 source, float mask)
{
    float3 graded = 1.0 - pow(max(0.0001, 1.0 - saturate(source)), _SkinGrade.rgb);
    return lerp(source, graded, mask);
}
