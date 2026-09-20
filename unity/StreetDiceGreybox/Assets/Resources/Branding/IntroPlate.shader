Shader "iPlay/Intro Plate"
{
    Properties
    {
        _MainTex ("iPlay art", 2D) = "white" {}
        _BaseLight ("Base Light", Range(0, 1)) = 0.2
        _BorderReveal ("Border Reveal", Range(0, 1)) = 0
        _SymbolReveal ("Symbol Reveal", Range(0, 1)) = 0
        _Pulse ("Cyan Pulse", Range(0, 2)) = 0
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
            float _BaseLight, _BorderReveal, _SymbolReveal, _Pulse;
            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f { float4 vertex : SV_POSITION; float2 uv : TEXCOORD0; };
            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
            fixed4 frag(v2f i) : SV_Target
            {
                fixed3 color = tex2D(_MainTex, i.uv).rgb;
                fixed cyan = saturate((color.b - color.r - 0.08) * 5.0)
                    * saturate((color.g - color.r - 0.04) * 5.0);
                float2 d = i.uv - 0.5;
                fixed border = step(0.35, max(abs(d.x), abs(d.y)));
                float perimeter = frac((atan2(d.y, d.x) + 4.712389) / 6.283185);
                fixed edgeLight = border * step(perimeter, _BorderReveal);
                fixed symbolLight = (1.0 - border)
                    * (1.0 - smoothstep(_SymbolReveal * 0.40 - 0.04,
                        _SymbolReveal * 0.40 + 0.04, length(d)));
                fixed light = saturate(edgeLight + symbolLight) * cyan;
                fixed3 plate = color * _BaseLight;
                return fixed4(plate + color * light * (1.0 + _Pulse), 1.0);
            }
            ENDCG
        }
    }
}
