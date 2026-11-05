// Generado para la interfaz de Sal y Almas. Perturbacion circular en el campo de altura.
Shader "SalYAlmas/UI/WaterDrop"
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
            float4 _Center;
            float4 frag(v2f_img i) : SV_Target
            {
                float4 c = tex2D(_MainTex, i.uv);
                float2 d = (i.uv - _Center.xy) * float2(1.5, 1.0);
                float g = exp(-dot(d, d) / (_Center.z * _Center.z));
                c.r += g * _Center.w;
                return c;
            }
            ENDCG
        }
    }
}
