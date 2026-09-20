Shader "iPlay/Catch Video"
{
    Properties
    {
        _MainTex ("Kling frame", 2D) = "black" {}
        _SkinGrade ("Hand grade", Vector) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Cull Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "CatchSkin.cginc"
            sampler2D _MainTex;
            fixed4 frag(v2f_img i) : SV_Target
            {
                fixed4 c = tex2D(_MainTex, i.uv);
                c.rgb = GradeCatchSkin(c.rgb, CatchSkinMask(c.rgb));
                c.a = 1;
                return c;
            }
            ENDCG
        }
    }
}
