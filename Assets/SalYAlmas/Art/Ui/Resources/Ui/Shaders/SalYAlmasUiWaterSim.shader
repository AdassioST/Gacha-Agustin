// Generado para la interfaz de Sal y Almas. Paso de la ecuacion de onda. R altura, G velocidad, centradas en 0.5 si el formato no es flotante.
Shader "SalYAlmas/UI/WaterSim"
{
    Properties { _MainTex ("Estado", 2D) = "black" {} }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always Blend Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex;
            float4 _Texel; float _Damp;
            float4 frag(v2f_img i) : SV_Target
            {
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
                return float4(h * edge + 0.5, v * edge + 0.5, 0, 1);
            }
            ENDCG
        }
    }
}
