// Fondo del combate. Doma los brillos que compiten con las figuras, abre un charco de luz donde pisan,
// oscurece las orillas y recibe las mismas luces dinámicas que las figuras, para que el golpe ilumine el suelo.
Shader "SalYAlmas/Battle/Backdrop"
{
    Properties
    {
        _MainTex ("Fondo", 2D) = "white" {}
        _Color ("Tinte", Color) = (1,1,1,1)
        _Blur ("Desenfoque (sesgo de mip)", Float) = 0.6
        _Knee ("Inicio de la compresión de brillos", Float) = 0.62
        _Pool ("Charco de luz: xy centro en pantalla, z radio, w fuerza", Vector) = (0.42, 0.34, 0.42, 0.18)
        _Edge ("Oscuridad de las orillas", Float) = 0.35
    }
    SubShader
    {
        Tags { "Queue"="Geometry" "RenderType"="Opaque" "IgnoreProjector"="True" }
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
            fixed4 _Color;
            float _Blur, _Knee, _Edge;
            float4 _Pool;

            float4 _FigKeyDir;
            float4 _FigFog;
            float4 _FigLightPos[4];
            float4 _FigLightCol[4];

            struct a2v { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; float4 screen : TEXCOORD1; };

            v2f vert(a2v v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.screen = ComputeScreenPos(o.pos);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 t = tex2Dbias(_MainTex, float4(i.uv, 0, _Blur));
                float3 c = t.rgb * _Color.rgb;

                // Los brillos fuertes (bokeh, luciérnagas) se comprimen: dejan de pelear con las figuras.
                float3 over = max(c - _Knee, 0.0);
                c = min(c, _Knee) + over / (1.0 + over * 2.5);

                float aspect = max(_FigFog.w, 0.5);
                float2 suv = i.screen.xy / max(i.screen.w, 0.0001);

                // Charco de luz bajo el aquelarre y los enemigos; las orillas caen a la sombra.
                float2 p = (suv - _Pool.xy) * float2(aspect, 1.6) / max(_Pool.z, 0.01);
                float pool = exp(-dot(p, p));
                float2 v = (suv - 0.5) * float2(1.0, 1.25);
                float rim = smoothstep(0.38, 0.92, length(v));
                c *= (1.0 + pool * _Pool.w) * (1.0 - rim * _Edge);

                // Las luces dinámicas también tocan el suelo, más suaves que en las figuras.
                if (_FigKeyDir.w > 0.5)
                {
                    for (int k = 0; k < 4; k++)
                    {
                        float4 lp = _FigLightPos[k];
                        float2 d = lp.xy - suv;
                        d.x *= aspect;
                        float att = saturate(1.0 - length(d) / max(lp.z * 1.3, 0.001));
                        c += (c * 0.8 + 0.03) * _FigLightCol[k].rgb * (att * att * lp.w * 0.55);
                    }
                }
                return fixed4(c, t.a * _Color.a);
            }
            ENDCG
        }
    }
    Fallback "Sprites/Default"
}
