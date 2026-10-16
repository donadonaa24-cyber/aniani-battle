Shader "AniBato/PaperStageCutout"
{
 Properties { [PerRendererData] _MainTex("Texture",2D)="white" {} _Color("Tint",Color)=(1,1,1,1) _Cutoff("Paper edge",Range(0,1))=.8 }
 SubShader
 {
  Tags { "Queue"="Transparent" "RenderType"="Transparent" "CanUseSpriteAtlas"="False" }
  Cull Off ZWrite Off Blend SrcAlpha OneMinusSrcAlpha
  Pass
  {
   CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "UnityCG.cginc"
   struct appdata { float4 vertex:POSITION; float2 uv:TEXCOORD0; fixed4 color:COLOR; };
   struct v2f { float4 vertex:SV_POSITION; float2 uv:TEXCOORD0; fixed4 color:COLOR; };
   sampler2D _MainTex; fixed4 _Color; float _Cutoff;
   v2f vert(appdata v){v2f o;o.vertex=UnityObjectToClipPos(v.vertex);o.uv=v.uv;o.color=v.color*_Color;return o;}
   fixed4 frag(v2f i):SV_Target{fixed4 c=tex2D(_MainTex,i.uv);clip(c.a-_Cutoff);return fixed4(c.rgb*i.color.rgb,i.color.a);}
   ENDCG
  }
 }
}
