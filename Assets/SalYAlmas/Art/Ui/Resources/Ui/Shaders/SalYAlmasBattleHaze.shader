// Aire del combate, delante de todo lo del escenario: neblina baja que cubre la línea de los pies con el color
// del bioma y una viñeta que junta fondo y figuras bajo la misma luz.
Shader "SalYAlmas/Battle/Haze"
{
    Properties
    {
        _MistY ("Altura de la neblina en pantalla", Float) = 0.3
        _MistH ("Grosor de la neblina", Float) = 0.07
        _MistA ("Opacidad de la neblina", Float) = 0.28
        _Vignette ("Viñeta", Float) = 0.42
    }
    SubShader
    {
        Tags { "Queue"="Transparent+50" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Cull Off ZWrite Off ZTest Always Lighting Off
        Blend One OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"

            float _MistY, _MistH, _MistA, _Vignette;
            sampler2D _FigEnvTex;
            float4 _FigEnvTint;
            float4 _FigFog;
            float _FigMotion;

            struct a2v { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f { float4 pos : SV_POSITION; float4 screen : TEXCOORD0; };

            v2f vert(a2v v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.screen = ComputeScreenPos(o.pos);
                return o;
            }

            float Hash(float2 p) { return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453); }
            float Noise(float2 p)
            {
                float2 i = floor(p), f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                return lerp(lerp(Hash(i), Hash(i + float2(1, 0)), f.x), lerp(Hash(i + float2(0, 1)), Hash(i + float2(1, 1)), f.x), f.y);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 suv = i.screen.xy / max(i.screen.w, 0.0001);
                float aspect = max(_FigFog.w, 0.5);
                float t = _Time.y * _FigMotion;

                float dy = (suv.y - _MistY) / max(_MistH, 0.005);
                float band = exp(-dy * dy);
                float2 q = float2(suv.x * aspect, suv.y);
                float n = Noise(q * float2(5.0, 22.0) + float2(t * 0.04, 0.0)) * 0.6 + Noise(q * float2(11.0, 40.0) - float2(t * 0.07, t * 0.01)) * 0.4;
                float mist = band * smoothstep(0.2, 0.85, n) * _MistA;
                float3 mistCol = tex2D(_FigEnvTex, float2(suv.x, _MistY)).rgb * _FigEnvTint.rgb * 1.35 + 0.035;

                float2 v = (suv - 0.5) * float2(1.0, 1.2);
                float vig = smoothstep(0.42, 0.98, length(v)) * _Vignette;

                return fixed4(mistCol * mist, saturate(mist + vig * (1.0 - mist)));
            }
            ENDCG
        }
    }
}
