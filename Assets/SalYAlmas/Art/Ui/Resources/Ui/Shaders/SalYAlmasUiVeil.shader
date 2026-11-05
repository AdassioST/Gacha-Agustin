// Generado para la interfaz de Sal y Almas. Velo de vapor a pantalla: ruido que corre lento, con transparencia por altura. Barato: una sola capa.
Shader "SalYAlmas/UI/Veil"
{
    Properties
    {
        [PerRendererData] _MainTex ("Textura", 2D) = "white" {}
        _Color ("Tinte", Color) = (1,1,1,1)
        _NoiseTex ("Ruido", 2D) = "gray" {}
        _Flow ("Corriente", Vector) = (0.012,0.02,-0.018,0.031)
        _Density ("Densidad", Float) = 1
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
            float _UiMotion;
            struct a2v { float4 vertex : POSITION; float4 color : COLOR; float2 uv : TEXCOORD0; };
            struct v2f { float4 pos : SV_POSITION; fixed4 color : COLOR; float2 uv : TEXCOORD0; float4 world : TEXCOORD1; };
            sampler2D _MainTex;
            fixed4 _Color;
            sampler2D _NoiseTex; float4 _Flow; float _Density;
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
                float2 uv = i.uv * float2(1.6, 0.9);
                float n1 = tex2D(_NoiseTex, uv + _Flow.xy * (_Time.y * _UiMotion)).b;
                float n2 = tex2D(_NoiseTex, uv * 1.9 + _Flow.zw * (_Time.y * _UiMotion)).r;
                float v = saturate((n1 * 0.6 + n2 * 0.5 - 0.42) * 2.4);
                fixed4 c = tex2D(_MainTex, i.uv) * i.color;
                c.a *= v * v * _Density;
                return c;
            }
            ENDCG
        }
    }
}
