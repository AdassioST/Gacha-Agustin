// Generado para la interfaz de Sal y Almas. Vida falsa para los retratos: respiración, vuelo de ropa y luz de borde. Es un puente hasta Live2D o Spine (GDD 19).
Shader "SalYAlmas/UI/Hero"
{
    Properties
    {
        [PerRendererData] _MainTex ("Textura", 2D) = "white" {}
        _Color ("Tinte", Color) = (1,1,1,1)
        _Breath ("Respiración", Float) = 1
        _Sway ("Vuelo de ropa", Float) = 1
        _Flash ("Destello", Color) = (0,0,0,0)
        _Rim ("Luz de contraluz", Color) = (1,0.72,0.55,0.55)
        _RimSide ("Lado del contraluz", Float) = 1
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
            float _Breath, _Sway, _RimSide; fixed4 _Flash, _Rim;
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
                return c * i.color;
            }
            ENDCG
        }
    }
}
