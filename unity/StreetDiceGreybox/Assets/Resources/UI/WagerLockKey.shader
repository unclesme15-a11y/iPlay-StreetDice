Shader "iPlay/WagerLockKey"
{
    Properties { _MainTex ("Artwork", 2D) = "white" {} _Crop ("Atlas crop", Vector) = (0,0,1,1) }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex;
            float4 _Crop;
            fixed4 frag(v2f_img i) : SV_Target
            {
                float4 c = tex2D(_MainTex, _Crop.xy + i.uv * _Crop.zw);
                float key = min(c.r, c.b) - c.g;
                float alpha = 1 - smoothstep(0.12, 0.55, key);
                // Suppress chroma spill in partially covered edge pixels.
                c.rb = lerp(c.rb, min(c.rb, c.g + 0.12), 1 - alpha);
                return float4(c.rgb, alpha);
            }
            ENDCG
        }
    }
}
