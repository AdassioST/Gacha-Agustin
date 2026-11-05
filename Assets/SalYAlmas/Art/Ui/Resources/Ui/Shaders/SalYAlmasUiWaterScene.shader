// Generado para la interfaz de Sal y Almas. El Manantial en perspectiva: cielo, sierras, y agua con reflejo, Fresnel, caustica, luz desde el fondo y brillo solar. La altura viene de la simulacion.
Shader "SalYAlmas/UI/WaterScene"
{
    Properties
    {
        [PerRendererData] _MainTex ("Textura", 2D) = "white" {}
        _Color ("Tinte", Color) = (1,1,1,1)
        _Sim ("Altura", 2D) = "gray" {}
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
        _SkyTint ("Tinte del cielo", Color) = (1,1,1,1)
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" "CanUseSpriteAtlas"="True" }
        Stencil { Ref [_Stencil] Comp [_StencilComp] Pass [_StencilOp] ReadMask [_StencilReadMask] WriteMask [_StencilWriteMask] }
        Cull Off Lighting Off ZWrite Off ZTest [unity_GUIZTestMode]
        Blend One Zero
        ColorMask [_ColorMask]
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"
            float _UiMotion;
            struct a2v { float4 vertex : POSITION; float4 color : COLOR; float2 uv : TEXCOORD0; };
            struct v2f { float4 pos : SV_POSITION; fixed4 color : COLOR; float2 uv : TEXCOORD0; float4 world : TEXCOORD1; };
            sampler2D _MainTex;
            fixed4 _Color;
            sampler2D _Sim, _Sky, _Far, _NoiseTex, _Hero;
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
            }
            v2f vert(a2v v)
            {
                v2f o;
                o.world = v.vertex;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color * _Color;
                return o;
            }
            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;
                float t = (_Time.y * _UiMotion);
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
                return fixed4(col, 1);
            }
            ENDCG
        }
    }
}
