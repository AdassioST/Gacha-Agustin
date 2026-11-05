// Generado para la interfaz de Sal y Almas. Emergencia del agua: la imagen aparece desde abajo, comida por la sal, con un filo de luz.
Shader "SalYAlmas/UI/Dissolve"
{
    Properties
    {
        [PerRendererData] _MainTex ("Textura", 2D) = "white" {}
        _Color ("Tinte", Color) = (1,1,1,1)
        _NoiseTex ("Ruido", 2D) = "gray" {}
        _Reveal ("Revelado", Range(0,1)) = 0
        _Edge ("Color de filo", Color) = (1,0.85,0.5,1)
        _EdgeWidth ("Ancho de filo", Float) = 0.07
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
            sampler2D _NoiseTex; float _Reveal, _EdgeWidth; fixed4 _Edge;
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
                fixed4 c = tex2D(_MainTex, i.uv) * i.color;
                float n = tex2D(_NoiseTex, i.uv * 2.3).r * 0.6 + tex2D(_NoiseTex, i.uv * 7.0).g * 0.4;
                float h = i.uv.y * 0.72 + n * 0.28;
                float r = _Reveal * (1.0 + _EdgeWidth * 2.0);
                float edge = 1.0 - saturate((h - (r - _EdgeWidth)) / _EdgeWidth);
                float a = saturate((r - h) / 0.015);
                c.rgb = lerp(_Edge.rgb * 2.0, c.rgb, edge * edge);
                c.a *= a;
                return c;
            }
            ENDCG
        }
    }
}
