Shader "iPlay/Menu Symbol"
{
    Properties
    {
        _MainTex ("iPlay art", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f { float4 vertex : SV_POSITION; float2 uv : TEXCOORD0; };
            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }
            fixed4 frag(v2f i) : SV_Target
            {
                fixed3 color = tex2D(_MainTex, i.uv).rgb;
                fixed cyan = saturate((color.b - color.r - 0.08) * 5.0)
                    * saturate((color.g - color.r - 0.04) * 5.0);
                return fixed4(color, cyan);
            }
            ENDCG
        }
    }
}
