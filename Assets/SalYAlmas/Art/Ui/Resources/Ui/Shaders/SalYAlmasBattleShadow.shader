// Sombra proyectada de un recorte: la misma silueta, tendida en el suelo y alejándose de la luz clave.
// Nítida junto a los pies y cada vez más difusa y tenue hacia la cabeza, como una sombra real.
Shader "SalYAlmas/Battle/Shadow"
{
    Properties
    {
        _MainTex ("Recorte", 2D) = "white" {}
        _Color ("Color y opacidad", Color) = (0.05,0.03,0.08,0.5)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Cull Off ZWrite Off Lighting Off
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;
            fixed4 _Color;

            struct a2v { float4 vertex : POSITION; float2 uv : TEXCOORD0; float4 color : COLOR; };
            struct v2f { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; fixed4 color : COLOR; };

            v2f vert(a2v v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float texH = max(_MainTex_TexelSize.w, 1.0);
                // De 1 % a 6 % de la altura de desenfoque, de los pies a la cabeza.
                float lod = log2(max(texH * lerp(0.01, 0.06, saturate(i.uv.y)), 1.0));
                float a = tex2Dlod(_MainTex, float4(i.uv, 0, lod)).a;
                float fade = 1.0 - smoothstep(0.0, 0.95, i.uv.y);
                return fixed4(i.color.rgb, a * fade * i.color.a);
            }
            ENDCG
        }
    }
}
