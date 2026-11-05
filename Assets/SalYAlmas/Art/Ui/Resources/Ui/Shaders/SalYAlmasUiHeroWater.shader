// Generado para la interfaz de Sal y Almas. Bruja que sale del agua: se recorta en su linea de flotacion ondulada, moja lo mas cercano y brilla por su contorno.
Shader "SalYAlmas/UI/HeroWater"
{
    Properties
    {
        [PerRendererData] _MainTex ("Textura", 2D) = "white" {}
        _Color ("Tinte", Color) = (1,1,1,1)
        _ClipUv ("Corte en uv", Float) = 0
        _NoiseTex ("Ruido", 2D) = "gray" {}
        _Glow ("Contorno", Color) = (1,0.85,0.5,0)
        _Wet ("Humedad", Float) = 1
        _Flash ("Destello", Float) = 0
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
            float _ClipUv, _Wet, _Flash; sampler2D _NoiseTex; fixed4 _Glow;
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
                return c;
            }
            ENDCG
        }
    }
}
