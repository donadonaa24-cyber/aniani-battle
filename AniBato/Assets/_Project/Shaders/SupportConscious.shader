Shader "Hidden/AniBato/SupportConscious"
{
    Properties { _MainTex ("World", 2D) = "white" {} _Phase ("Phase", Float) = 0 }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex;
            float _Phase;
            fixed4 frag(v2f_img input) : SV_Target
            {
                float2 uv=input.uv;
                uv.x+=sin(uv.y*28+_Phase*2.2)*.0025;
                uv.y+=sin(uv.x*21-_Phase*1.4)*.0015;
                fixed4 c=tex2D(_MainTex,saturate(uv));
                c.rgb=lerp(c.rgb,fixed3(.64,.87,1),.16);
                return c;
            }
            ENDCG
        }
    }
}
