"""Genera los shaders de interfaz. Todos comparten el contrato de uGUI (stencil para máscaras, color de vértice)."""
import os
import sys

OUT = sys.argv[1] if len(sys.argv) > 1 else os.path.join(os.path.dirname(os.path.dirname(os.path.abspath(__file__))), "Assets", "SalYAlmas", "Art", "Ui", "Resources", "Ui", "Shaders")
os.makedirs(OUT, exist_ok=True)

TEMPLATE = """// Generado para la interfaz de Sal y Almas. {doc}
Shader "SalYAlmas/UI/{name}"
{{
    Properties
    {{
        [PerRendererData] _MainTex ("Textura", 2D) = "white" {{}}
        _Color ("Tinte", Color) = (1,1,1,1)
{props}
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
    }}
    SubShader
    {{
        Tags {{ "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" "CanUseSpriteAtlas"="True" }}
        Stencil {{ Ref [_Stencil] Comp [_StencilComp] Pass [_StencilOp] ReadMask [_StencilReadMask] WriteMask [_StencilWriteMask] }}
        Cull Off Lighting Off ZWrite Off ZTest [unity_GUIZTestMode]
        Blend {blend}
        ColorMask [_ColorMask]
        Pass
        {{
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"
            struct a2v {{ float4 vertex : POSITION; float4 color : COLOR; float2 uv : TEXCOORD0; }};
            struct v2f {{ float4 pos : SV_POSITION; fixed4 color : COLOR; float2 uv : TEXCOORD0; float4 world : TEXCOORD1; }};
            sampler2D _MainTex;
            fixed4 _Color;
{decl}
            v2f vert(a2v v)
            {{
                v2f o;
                o.world = v.vertex;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color * _Color;
                return o;
            }}
            fixed4 frag(v2f i) : SV_Target
            {{
{body}
            }}
            ENDCG
        }}
    }}
}}
"""

SHADERS = {
    "WaterScene": dict(
        doc="El Manantial en perspectiva: cielo, sierras, y agua con reflejo, Fresnel, caustica, luz desde el fondo y brillo solar. La altura viene de la simulacion.",
        blend="One Zero",
        props="""        _Sim ("Altura", 2D) = "gray" {}
        _Sky ("Cielo", 2D) = "black" {}
        _Far ("Sierras", 2D) = "black" {}
        _NoiseTex ("Ruido", 2D) = "gray" {}
        _Hero ("Bruja", 2D) = "black" {}
        _HeroRect ("Rect de la bruja", Vector) = (0,0,0,0)
        _WaterY ("Linea de flotacion", Float) = 0.3
        _HorY ("Horizonte", Float) = 0.56
        _Deep ("Agua profunda", Color) = (0.25,0.05,0.09,1)
        _Shallow ("Agua clara", Color) = (0.85,0.42,0.38,1)
        _Under ("Luz bajo el agua", Color) = (0,0,0,0)
        _UnderPos ("Centro de la luz", Vector) = (0.5,0.3,0.28,0)
        _Sun ("Sol", Color) = (1,0.78,0.55,1)
        _Dim ("Oscurecer cielo", Float) = 0
        _SkyTint ("Tinte del cielo", Color) = (1,1,1,1)""",
        decl="""            sampler2D _Sim, _Sky, _Far, _NoiseTex, _Hero;
            float4 _HeroRect, _UnderPos, _Sim_TexelSize;
            float _WaterY, _HorY, _Dim;
            fixed4 _Deep, _Shallow, _Under, _Sun, _SkyTint;

            float2 PlaneUv(float2 uv)
            {
                float d = saturate((_HorY - uv.y) / _HorY);
                float sx = 0.5 + (uv.x - 0.5) / (1.04 + d * 0.16);
                return float2(sx, 0.03 + 0.94 * pow(d, 0.72));
            }

            fixed3 Backdrop(float2 uv)
            {
                // El horizonte del arte está a 0.29 desde abajo; el de la escena, en _HorY. Sobre él, el cielo sin estirar.
                float2 s = float2(uv.x, saturate(0.29 + (uv.y - _HorY) / (1.0 - _HorY) * 0.71));
                fixed3 c = tex2D(_Sky, s).rgb * _SkyTint.rgb;
                fixed4 f = tex2D(_Far, s);
                c = lerp(c, f.rgb * _SkyTint.rgb, f.a);
                return c * (1.0 - _Dim);
            }""",
        body="""
                float2 uv = i.uv;
                float t = _Time.y;
                if (uv.y > _HorY + 0.002)
                {
                    fixed3 c0 = Backdrop(uv);
                    float2 up = (uv - _UnderPos.xy) / _UnderPos.z;
                    c0 += _Under.rgb * _Under.a * 0.22 * exp(-dot(up, up) * 0.35);
                    return fixed4(c0, 1);
                }
                float2 p = PlaneUv(uv);
                float d = saturate((_HorY - uv.y) / _HorY);
                float2 e = _Sim_TexelSize.xy * 1.5;
                float hx_ = tex2D(_Sim, p + float2(e.x, 0)).r - tex2D(_Sim, p - float2(e.x, 0)).r;
                float hy_ = tex2D(_Sim, p + float2(0, e.y)).r - tex2D(_Sim, p - float2(0, e.y)).r;
                float2 sim = float2(hx_, hy_) * 15.0 * (0.4 + d * 0.8);
                // Oleaje fino de fondo, tambien en perspectiva, para que el agua nunca este muerta.
                // Coordenadas de mundo sobre el plano: la textura se aplasta hacia el horizonte como agua real, y con mipmaps no chispea.
                float zw = 1.0 / (d + 0.10);
                float2 wuv = float2((uv.x - 0.5) * zw * 0.9, zw * 0.36);
                float2 fine = (float2(tex2D(_NoiseTex, wuv * 1.1 + float2(t * 0.014, t * 0.02)).r, tex2D(_NoiseTex, wuv * 1.45 - float2(t * 0.017, -t * 0.01)).g) - 0.5) * 0.22 * saturate(d * 1.6 + 0.08);
                float2 n = sim + fine;
                // Reflejo del cielo con distorsion. Mas lejos, mas plano y mas espejo.
                float2 r = float2(uv.x + n.x * 0.07, _HorY + (_HorY - uv.y) * 0.92 + n.y * 0.16 * (0.3 + d));
                fixed3 refl = Backdrop(r);
                // Reflejo de la bruja: mismo punto reflejado respecto a su linea de flotacion.
                fixed3 heroRefl = 0; float heroA = 0;
                if (_HeroRect.z > 0 && uv.y < _WaterY)
                {
                    float2 hr = float2(uv.x + n.x * 0.09, _WaterY + (_WaterY - uv.y) * 1.02 + n.y * 0.12);
                    float2 huv = (hr - _HeroRect.xy) / (_HeroRect.zw - _HeroRect.xy);
                    if (huv.x > 0 && huv.x < 1 && huv.y > 0 && huv.y < 1)
                    {
                        fixed4 hc = tex2D(_Hero, huv);
                        float fade = saturate(1.0 - (_WaterY - uv.y) * 2.6);
                        heroRefl = hc.rgb * 0.82;
                        heroA = hc.a * fade;
                    }
                }
                // Fresnel: rasante cerca del horizonte, y las olas cambian el angulo.
                float cosv = saturate(0.08 + d * 0.92 + (n.y) * 0.5);
                float fres = 0.05 + 0.95 * pow(1.0 - cosv, 3.0);
                // Fondo: la profundidad y la caustica, con el agua de la Termas turbia y rosada.
                fixed3 body = lerp(_Shallow.rgb, _Deep.rgb, smoothstep(0.0, 0.95, d));
                float ca = tex2D(_NoiseTex, wuv * 2.4 + n * 0.5 + t * 0.012).g;
                ca = pow(saturate(1.0 - abs(ca - 0.5) * 6.0), 3.0);
                body += fixed3(1.0, 0.8, 0.65) * ca * 0.07 * saturate(d * 1.4);
                float2 up2 = (uv - _UnderPos.xy) / float2(_UnderPos.z, _UnderPos.z * 0.42);
                float glow = exp(-dot(up2, up2));
                // La luz viene de abajo: se tiñe el agua y sube lo claro, sin apagar la textura.
                body = lerp(body, body * 0.8 + _Under.rgb * (0.2 + ca * 0.75), glow * _Under.a * 0.55);
                fixed3 col = lerp(body, refl, fres);
                // Sol y luz en las crestas
                float2 fineN = fine * 2.2 + sim * 0.25;
                float2 sv = float2((uv.x - 0.66) * 1.9 + fineN.x * 1.3, d * 0.9 + fineN.y * 0.8);
                float spec = pow(saturate(1.0 - dot(sv, sv) * 3.2), 14.0) * (1.0 - _Dim * 0.75);
                col += _Sun.rgb * spec * 0.8;
                // Lectura de las ondas: un lado de cada cresta recibe luz y el otro no. Es lo que hace ver los anillos.
                float slope = clamp(sim.y * 0.9 + sim.x * 0.35, -1.0, 1.0);
                col += (slope > 0.0 ? fixed3(1.0, 0.86, 0.74) * slope * 0.5 : col * slope * 0.6) * (0.4 + d * 0.6);
                col += _Under.rgb * _Under.a * saturate(abs(slope) - 0.15) * glow * 0.5;
                col = lerp(col, heroRefl, heroA * 0.62);
                // Bruma sobre el horizonte
                col = lerp(col, Backdrop(float2(uv.x, _HorY)), (1.0 - smoothstep(0.0, 0.1, d)) * 0.75);
                return fixed4(col, 1);"""),
    "HeroWater": dict(
        doc="Bruja que sale del agua: se recorta en su linea de flotacion ondulada, moja lo mas cercano y brilla por su contorno.",
        blend="SrcAlpha OneMinusSrcAlpha",
        props="""        _ClipUv ("Corte en uv", Float) = 0
        _NoiseTex ("Ruido", 2D) = "gray" {}
        _Glow ("Contorno", Color) = (1,0.85,0.5,0)
        _Wet ("Humedad", Float) = 1
        _Flash ("Destello", Float) = 0""",
        decl="            float _ClipUv, _Wet, _Flash; sampler2D _NoiseTex; fixed4 _Glow;",
        body="""
                float t = _Time.y;
                float2 uv = i.uv;
                float wob = (tex2D(_NoiseTex, float2(uv.x * 2.0 + t * 0.09, t * 0.05)).r - 0.5) * 0.018 + sin(uv.x * 26.0 + t * 3.4) * 0.002;
                float dy = uv.y - (_ClipUv + wob);
                // Refraccion leve justo sobre el agua, por lo que chorrea.
                uv.x += sin(uv.y * 80.0 + t * 7.0) * 0.0018 * saturate(1.0 - dy * 9.0) * _Wet;
                fixed4 c = tex2D(_MainTex, uv) * i.color;
                float wet = saturate(1.0 - dy * 3.6) * _Wet;
                c.rgb *= 1.0 - wet * 0.28;
                c.rgb += fixed3(1.0, 0.92, 0.85) * pow(saturate(tex2D(_NoiseTex, uv * 7.0).g * 1.6 - 0.72), 2.0) * wet * 1.2;
                float e = 0.0028;
                float a0 = tex2D(_MainTex, uv + float2(e, e)).a * tex2D(_MainTex, uv + float2(-e, e)).a * tex2D(_MainTex, uv - float2(e, e)).a * tex2D(_MainTex, uv + float2(e, -e)).a;
                float rim = saturate(c.a - a0);
                c.rgb += _Glow.rgb * rim * _Glow.a * 0.9;
                c.rgb += _Glow.rgb * _Flash * 0.7;
                c.a *= smoothstep(0.0, 0.006, dy);
                return c;"""),
    "Soft": dict(doc="Alfa normal: vapor y bruma.", blend="SrcAlpha OneMinusSrcAlpha", props="", decl="", body="""
                fixed4 c = tex2D(_MainTex, i.uv) * i.color;
                return c;"""),
    "Additive": dict(doc="Luz: almitas, chispas, rayos.", blend="SrcAlpha One", props="", decl="", body="""
                fixed4 c = tex2D(_MainTex, i.uv) * i.color;
                return c;"""),
    "Hero": dict(
        doc="Vida falsa para los retratos: respiración, vuelo de ropa y luz de borde. Es un puente hasta Live2D o Spine (GDD 19).",
        blend="SrcAlpha OneMinusSrcAlpha",
        props='        _Breath ("Respiración", Float) = 1\n        _Sway ("Vuelo de ropa", Float) = 1\n        _Flash ("Destello", Color) = (0,0,0,0)\n        _Rim ("Luz de contraluz", Color) = (1,0.72,0.55,0.55)\n        _RimSide ("Lado del contraluz", Float) = 1',
        decl="            float _Breath, _Sway, _RimSide; fixed4 _Flash, _Rim;",
        body="""
                float2 uv = i.uv;
                float t = _Time.y;
                // Respiración: el torso se ensancha y sube un poco; los pies no se mueven.
                float chest = exp(-pow((uv.y - 0.68) / 0.20, 2.0));
                float br = sin(t * 1.45) * _Breath;
                uv.y -= chest * br * 0.0035;
                uv.x -= (uv.x - 0.5) * chest * br * 0.006;
                // La ropa vuela desde la cintura, más abajo que arriba, sin arrastrar el suelo.
                float hem = smoothstep(0.02, 0.18, uv.y) * smoothstep(0.62, 0.22, uv.y);
                uv.x += sin(t * 0.9 + uv.y * 9.0) * hem * 0.0038 * _Sway;
                uv.x += sin(t * 1.7 + uv.y * 21.0 + uv.x * 6.0) * hem * 0.0014 * _Sway;
                fixed4 c = tex2D(_MainTex, uv);
                // Contraluz de atardecer: alfa vecino para encontrar el borde, sin normal map.
                float e = 0.006;
                float side = tex2D(_MainTex, uv + float2(e * _RimSide, 0)).a;
                float rim = saturate(c.a - side);
                c.rgb = lerp(c.rgb, c.rgb * 0.55 + _Rim.rgb, rim * _Rim.a);
                c.rgb += _Flash.rgb * _Flash.a;
                return c * i.color;"""),
    "Shimmer": dict(
        doc="Pan de oro: una banda de luz cruza de vez en cuando.",
        blend="SrcAlpha OneMinusSrcAlpha",
        props='        _Band ("Color de banda", Color) = (1,0.95,0.75,1)\n        _Speed ("Velocidad", Float) = 0.35\n        _Width ("Ancho", Float) = 0.09\n        _Pause ("Pausa", Float) = 2.2',
        decl="            fixed4 _Band; float _Speed, _Width, _Pause;",
        body="""
                fixed4 c = tex2D(_MainTex, i.uv) * i.color;
                float p = frac(_Time.y * _Speed) * (1.0 + _Pause) - 0.3;
                float d = (i.uv.x + i.uv.y * 0.4) - p * 1.4;
                float s = exp(-d * d / (_Width * _Width));
                c.rgb += _Band.rgb * s * _Band.a * c.a;
                return c;"""),
    "Dissolve": dict(
        doc="Emergencia del agua: la imagen aparece desde abajo, comida por la sal, con un filo de luz.",
        blend="SrcAlpha OneMinusSrcAlpha",
        props='        _NoiseTex ("Ruido", 2D) = "gray" {}\n        _Reveal ("Revelado", Range(0,1)) = 0\n        _Edge ("Color de filo", Color) = (1,0.85,0.5,1)\n        _EdgeWidth ("Ancho de filo", Float) = 0.07',
        decl="            sampler2D _NoiseTex; float _Reveal, _EdgeWidth; fixed4 _Edge;",
        body="""
                fixed4 c = tex2D(_MainTex, i.uv) * i.color;
                float n = tex2D(_NoiseTex, i.uv * 2.3).r * 0.6 + tex2D(_NoiseTex, i.uv * 7.0).g * 0.4;
                float h = i.uv.y * 0.72 + n * 0.28;
                float r = _Reveal * (1.0 + _EdgeWidth * 2.0);
                float edge = 1.0 - saturate((h - (r - _EdgeWidth)) / _EdgeWidth);
                float a = saturate((r - h) / 0.015);
                c.rgb = lerp(_Edge.rgb * 2.0, c.rgb, edge * edge);
                c.a *= a;
                return c;"""),
    "Wipe": dict(
        doc="Transición: la tinta inunda la página. _Cover 0 limpio, 1 cubierto.",
        blend="SrcAlpha OneMinusSrcAlpha",
        props='        _NoiseTex ("Ruido", 2D) = "gray" {}\n        _Cover ("Cubierto", Range(0,1)) = 0\n        _EdgeColor ("Filo", Color) = (0.72,0.57,0.23,1)',
        decl="            sampler2D _NoiseTex; float _Cover; fixed4 _EdgeColor;",
        body="""
                float n = tex2D(_NoiseTex, i.uv * float2(1.8, 1.0)).r * 0.65 + tex2D(_NoiseTex, i.uv * float2(6.0, 3.4)).g * 0.35;
                float h = n * 0.55 + (1.0 - i.uv.x) * 0.45;
                float t = _Cover * 1.3 - 0.15;
                float a = smoothstep(h - 0.02, h + 0.02, t);
                float edge = smoothstep(h - 0.05, h, t) * (1.0 - smoothstep(h, h + 0.05, t));
                fixed4 c = i.color;
                c.a *= a;
                c.rgb = lerp(c.rgb, _EdgeColor.rgb, edge);
                c.a = max(c.a, edge * _EdgeColor.a * i.color.a);
                return c;"""),
    "Veil": dict(
        doc="Velo de vapor a pantalla: ruido que corre lento, con transparencia por altura. Barato: una sola capa.",
        blend="SrcAlpha OneMinusSrcAlpha",
        props='        _NoiseTex ("Ruido", 2D) = "gray" {}\n        _Flow ("Corriente", Vector) = (0.012,0.02,-0.018,0.031)\n        _Density ("Densidad", Float) = 1',
        decl="            sampler2D _NoiseTex; float4 _Flow; float _Density;",
        body="""
                float2 uv = i.uv * float2(1.6, 0.9);
                float n1 = tex2D(_NoiseTex, uv + _Flow.xy * _Time.y).b;
                float n2 = tex2D(_NoiseTex, uv * 1.9 + _Flow.zw * _Time.y).r;
                float v = saturate((n1 * 0.6 + n2 * 0.5 - 0.42) * 2.4);
                fixed4 c = tex2D(_MainTex, i.uv) * i.color;
                c.a *= v * v * _Density;
                return c;"""),
    "Water": dict(
        doc="Agua termal: las cáusticas se mueven y la imagen ondula.",
        blend="SrcAlpha OneMinusSrcAlpha",
        props='        _NoiseTex ("Ruido", 2D) = "gray" {}\n        _Strength ("Ondulación", Float) = 0.008\n        _Glow ("Color de luz", Color) = (1,0.82,0.65,0.4)',
        decl="            sampler2D _NoiseTex; float _Strength; fixed4 _Glow;",
        body="""
                float t = _Time.y;
                float2 n = float2(tex2D(_NoiseTex, i.uv * 2.6 + float2(t * 0.025, t * 0.018)).r, tex2D(_NoiseTex, i.uv * 2.6 + float2(-t * 0.02, t * 0.031)).g) - 0.5;
                fixed4 c = tex2D(_MainTex, i.uv + n * _Strength) * i.color;
                float ca = tex2D(_NoiseTex, i.uv * 5.0 + n * 0.5 + t * 0.012).g;
                ca = pow(saturate(1.0 - abs(ca - 0.5) * 7.0), 3.0);
                c.rgb += _Glow.rgb * ca * _Glow.a;
                return c;"""),
    "Disc": dict(
        doc="Recorte circular con borde suave, sin el diente de sierra de una máscara de stencil.",
        blend="SrcAlpha OneMinusSrcAlpha",
        props="",
        decl="",
        body="""
                fixed4 c = tex2D(_MainTex, i.uv) * i.color;
                float r = length(i.uv - 0.5) * 2.0;
                c.a *= 1.0 - smoothstep(0.93, 0.97, r);
                return c;"""),
    "Fx": dict(
        doc="Tinte que se pasa a gris, para cartas bloqueadas y brujas que aún no llegan.",
        blend="SrcAlpha OneMinusSrcAlpha",
        props='        _Gray ("Gris", Range(0,1)) = 1\n        _Dark ("Oscuro", Range(0,1)) = 0.5',
        decl="            float _Gray, _Dark;",
        body="""
                fixed4 c = tex2D(_MainTex, i.uv) * i.color;
                float g = dot(c.rgb, float3(0.3, 0.59, 0.11));
                c.rgb = lerp(c.rgb, g.xxx, _Gray) * (1.0 - _Dark);
                return c;"""),
}

for name, s in SHADERS.items():
    text = TEMPLATE.format(name=name, doc=s["doc"], blend=s["blend"], props=s["props"], decl=s["decl"], body=s["body"].strip("\n"))
    with open(os.path.join(OUT, f"SalYAlmasUi{name}.shader"), "w", encoding="utf8", newline="\n") as f:
        f.write(text)

PLAIN = """// Generado para la interfaz de Sal y Almas. {doc}
Shader "SalYAlmas/UI/{name}"
{{
    Properties {{ _MainTex ("Estado", 2D) = "black" {{}} }}
    SubShader
    {{
        Cull Off ZWrite Off ZTest Always Blend Off
        Pass
        {{
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex;
{decl}
            float4 frag(v2f_img i) : SV_Target
            {{
{body}
            }}
            ENDCG
        }}
    }}
}}
"""

PLAINS = {
    "WaterSim": dict(doc="Paso de la ecuacion de onda. R altura, G velocidad, centradas en 0.5 si el formato no es flotante.",
        decl="            float4 _Texel; float _Damp;",
        body="""
                float4 c = tex2D(_MainTex, i.uv);
                float h = c.r - 0.5, v = c.g - 0.5;
                float n = tex2D(_MainTex, i.uv + float2(0, _Texel.y)).r;
                float s = tex2D(_MainTex, i.uv - float2(0, _Texel.y)).r;
                float e = tex2D(_MainTex, i.uv + float2(_Texel.x, 0)).r;
                float w = tex2D(_MainTex, i.uv - float2(_Texel.x, 0)).r;
                float avg = (n + s + e + w) * 0.25 - 0.5;
                v += (avg - h) * 0.38;
                v *= _Damp;
                h += v;
                // Difusión mínima: mata el ajedrez de alta frecuencia sin comerse los anillos.
                h = lerp(h, avg, 0.035);
                h *= 0.996;
                // Las orillas absorben: asi no rebota un eco falso desde los bordes de la textura.
                float2 d = min(i.uv, 1.0 - i.uv);
                float edge = saturate(min(d.x, d.y) * 7.0);
                return float4(h * edge + 0.5, v * edge + 0.5, 0, 1);"""),
    "WaterDrop": dict(doc="Perturbacion circular en el campo de altura.",
        decl="            float4 _Center;",
        body="""
                float4 c = tex2D(_MainTex, i.uv);
                float2 d = (i.uv - _Center.xy) * float2(1.5, 1.0);
                float g = exp(-dot(d, d) / (_Center.z * _Center.z));
                c.r += g * _Center.w;
                return c;"""),
}
for name, x in PLAINS.items():
    with open(os.path.join(OUT, f"SalYAlmasUi{name}.shader"), "w", encoding="utf8", newline="\n") as f:
        f.write(PLAIN.format(name=name, doc=x["doc"], decl=x["decl"], body=x["body"].strip("\n")))

print(len(SHADERS) + len(PLAINS), "shaders")
