// Generado para la interfaz de Sal y Almas. Agua termal: las cáusticas se mueven y la imagen ondula.
Shader "SalYAlmas/UI/Water"
{
    Properties
    {
        [PerRendererData] _MainTex ("Textura", 2D) = "white" {}
        _Color ("Tinte", Color) = (1,1,1,1)
        _NoiseTex ("Ruido", 2D) = "gray" {}
        _Strength ("Ondulación", Float) = 0.008
        _Glow ("Color de luz", Color) = (1,0.82,0.65,0.4)
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
            sampler2D _NoiseTex; float _Strength; fixed4 _Glow;
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
                float t = (_Time.y * _UiMotion);
                float2 n = float2(tex2D(_NoiseTex, i.uv * 2.6 + float2(t * 0.025, t * 0.018)).r, tex2D(_NoiseTex, i.uv * 2.6 + float2(-t * 0.02, t * 0.031)).g) - 0.5;
                fixed4 c = tex2D(_MainTex, i.uv + n * _Strength) * i.color;
                float ca = tex2D(_NoiseTex, i.uv * 5.0 + n * 0.5 + t * 0.012).g;
                ca = pow(saturate(1.0 - abs(ca - 0.5) * 7.0), 3.0);
                c.rgb += _Glow.rgb * ca * _Glow.a;
                return c;
            }
            ENDCG
        }
    }
}
