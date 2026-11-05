// Generado para la interfaz de Sal y Almas. Transición: la tinta inunda la página. _Cover 0 limpio, 1 cubierto.
Shader "SalYAlmas/UI/Wipe"
{
    Properties
    {
        [PerRendererData] _MainTex ("Textura", 2D) = "white" {}
        _Color ("Tinte", Color) = (1,1,1,1)
        _NoiseTex ("Ruido", 2D) = "gray" {}
        _Cover ("Cubierto", Range(0,1)) = 0
        _EdgeColor ("Filo", Color) = (0.72,0.57,0.23,1)
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
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"
            struct a2v { float4 vertex : POSITION; float4 color : COLOR; float2 uv : TEXCOORD0; };
            struct v2f { float4 pos : SV_POSITION; fixed4 color : COLOR; float2 uv : TEXCOORD0; float4 world : TEXCOORD1; };
            sampler2D _MainTex;
            fixed4 _Color;
            sampler2D _NoiseTex; float _Cover; fixed4 _EdgeColor;
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
                float n = tex2D(_NoiseTex, i.uv * float2(1.8, 1.0)).r * 0.65 + tex2D(_NoiseTex, i.uv * float2(6.0, 3.4)).g * 0.35;
                float h = n * 0.55 + (1.0 - i.uv.x) * 0.45;
                float t = _Cover * 1.3 - 0.15;
                float a = smoothstep(h - 0.02, h + 0.02, t);
                float edge = smoothstep(h - 0.05, h, t) * (1.0 - smoothstep(h, h + 0.05, t));
                fixed4 c = i.color;
                c.a *= a;
                c.rgb = lerp(c.rgb, _EdgeColor.rgb, edge);
                c.a = max(c.a, edge * _EdgeColor.a * i.color.a);
                return c;
            }
            ENDCG
        }
    }
}
