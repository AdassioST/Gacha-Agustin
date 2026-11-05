// Luz del combate para los recortes de brujas y enemigos. Ilumina el arte con el bioma en vez de pegarlo encima:
// luz ambiente de cielo y suelo, contraluz del lado de la luz clave, envoltura de luz del fondo en el borde,
// oclusión en los pies, niebla por profundidad y hasta cuatro luces dinámicas (golpes, hechizos, turno).
// Las globales _Fig* las pone BattleLighting a partir del fondo real de la escena.
Shader "SalYAlmas/Battle/Figure"
{
    Properties
    {
        _MainTex ("Recorte", 2D) = "white" {}
        _Color ("Tinte y destello", Color) = (1,1,1,1)
        _FlipX ("Volteado (1 o -1)", Float) = 1
        _Depth ("Profundidad: 0 al frente, 1 al fondo", Float) = 0
        _EdgeRadius ("Ancho del borde en fracción de altura", Float) = 0.022
        _Focus ("Realce de foco", Float) = 0
        _BaseFade ("Desvanecido de la base en fracción de altura", Float) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" "PreviewType"="Plane" }
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
            float _FlipX, _Depth, _EdgeRadius, _Focus, _BaseFade;

            sampler2D _FigEnvTex;
            float4 _FigEnvTint;
            float4 _FigAmbientTop;     // rgb luz del cielo
            float4 _FigAmbientBottom;  // rgb luz rebotada del suelo
            float4 _FigKeyColor;       // rgb color de la luz clave, a fuerza del contraluz
            float4 _FigKeyDir;         // xy hacia la luz en pantalla, z oscuridad de pies, w luces dinámicas (0 o 1)
            float4 _FigGrade;          // x saturación, y contraste, z exposición, w envoltura de luz
            float4 _FigFog;            // x niebla al frente, y niebla al fondo, z negros elevados, w aspecto de pantalla
            float4 _FigLightPos[4];    // xy pantalla 0..1, z radio en alto de pantalla, w intensidad
            float4 _FigLightCol[4];

            struct a2v { float4 vertex : POSITION; float4 color : COLOR; float2 uv : TEXCOORD0; };
            struct v2f { float4 pos : SV_POSITION; fixed4 color : COLOR; float2 uv : TEXCOORD0; float4 screen : TEXCOORD1; };

            v2f vert(a2v v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.screen = ComputeScreenPos(o.pos);
                o.color = v.color * _Color;
                return o;
            }

            float Luma(float3 c) { return dot(c, float3(0.299, 0.587, 0.114)); }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 c = tex2D(_MainTex, i.uv);
                if (c.a < 0.004) discard;

                // Silueta difuminada con los mipmaps: da el borde y una normal falsa sin mapa de normales.
                float texH = max(_MainTex_TexelSize.w, 1.0);
                float aspect = max(_MainTex_TexelSize.z, 1.0) / texH;
                float lod = log2(max(texH * _EdgeRadius, 1.0));
                float2 r = float2(_EdgeRadius / aspect, _EdgeRadius);
                float aC = tex2Dlod(_MainTex, float4(i.uv, 0, lod)).a;
                float aL = tex2Dlod(_MainTex, float4(i.uv - float2(r.x, 0), 0, lod)).a;
                float aR = tex2Dlod(_MainTex, float4(i.uv + float2(r.x, 0), 0, lod)).a;
                float aD = tex2Dlod(_MainTex, float4(i.uv - float2(0, r.y), 0, lod)).a;
                float aU = tex2Dlod(_MainTex, float4(i.uv + float2(0, r.y), 0, lod)).a;
                float2 n = float2(aL - aR, aD - aU);
                n.x *= _FlipX;
                float nl = length(n);
                float2 nd = nl > 0.0001 ? n / nl : float2(0, 0);
                float blur = (aC * 2.0 + aL + aR + aD + aU) / 6.0;
                float edge = smoothstep(0.0, 0.62, saturate(1.0 - blur)) * c.a;

                // El fondo detrás de este píxel y justo afuera de la silueta.
                float2 suv = i.screen.xy / max(i.screen.w, 0.0001);
                float2 outward = nd * float2(0.035 / max(_FigFog.w, 0.5), 0.035);
                float3 envHere = tex2D(_FigEnvTex, suv).rgb * _FigEnvTint.rgb;
                float3 envOut = tex2D(_FigEnvTex, suv + outward).rgb * _FigEnvTint.rgb;

                // 1. El arte entra al rango del fondo: saturación y contraste.
                float3 albedo = c.rgb;
                float l = Luma(albedo);
                albedo = lerp(l.xxx, albedo, _FigGrade.x);
                albedo = max((albedo - 0.5) * _FigGrade.y + 0.5, 0.0);

                // 2. Luz ambiente del bioma: el cielo arriba y el suelo rebotado abajo.
                float h = i.uv.y;
                float3 amb = lerp(_FigAmbientBottom.rgb, _FigAmbientTop.rgb, smoothstep(0.05, 0.85, h));
                float3 lit = albedo * amb * _FigGrade.z;

                // 3. Los pies se oscurecen donde tocan el suelo.
                lit *= lerp(_FigKeyDir.z, 1.0, smoothstep(0.0, 0.18, h));

                // 4. Contraluz del lado de la luz clave (GDD 19.1: luz de contorno).
                float facing = saturate(dot(nd, _FigKeyDir.xy) * 0.8 + 0.2);
                lit += _FigKeyColor.rgb * (edge * facing * _FigKeyColor.a);

                // 5. Envoltura de luz: el fondo se derrama sobre el borde y la silueta deja de verse recortada.
                lit += envOut * (edge * _FigGrade.w);
                lit = lerp(lit, envOut, edge * _FigGrade.w * 0.2);

                // 6. Luces dinámicas: golpes, hechizos, Ascensiones y el foco del turno.
                if (_FigKeyDir.w > 0.5)
                {
                    for (int k = 0; k < 4; k++)
                    {
                        float4 lp = _FigLightPos[k];
                        float2 d = lp.xy - suv;
                        d.x *= _FigFog.w;
                        float dist = length(d);
                        float att = saturate(1.0 - dist / max(lp.z, 0.001));
                        att *= att * lp.w;
                        float2 ld = dist > 0.0001 ? d / dist : float2(0, 0);
                        float wrap = saturate(dot(nd, ld) * 0.6 + 0.4);
                        lit += albedo * _FigLightCol[k].rgb * (att * wrap) + _FigLightCol[k].rgb * (edge * att * 0.6);
                    }
                }

                // 7. Aire del bioma: negros elevados y niebla según la fila.
                lit += envHere * _FigFog.z;
                float fog = lerp(_FigFog.x, _FigFog.y, saturate(_Depth));
                lit = lerp(lit, envHere, fog);

                // 8. Foco: la unidad elegida o en turno recupera un poco de su color original.
                lit = lerp(lit, max(lit, c.rgb * 0.95), saturate(_Focus) * 0.5);

                // 9. Base de los gigantes: el corte recto del dibujo se funde con el suelo.
                float baseA = _BaseFade > 0.0001 ? smoothstep(0.0, _BaseFade, h) : 1.0;
                return fixed4(lit * i.color.rgb, c.a * i.color.a * baseA);
            }
            ENDCG
        }
    }
    Fallback "Sprites/Default"
}
