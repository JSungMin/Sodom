Shader "Unlit/MonoColorMaskShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Color ("Color", Color) = (1,1,1,1)
	}
	SubShader
	{
		LOD 100
		Pass {
			Tags { "RenderType"="Opaque" "Queue" = "Geometry"}
		    Stencil {
		       Ref 3
		       Comp Always
		       Pass replace
			}
			 CGPROGRAM
			 #pragma vertex vert
			 #pragma fragment frag
			 #include "UnityCG.cginc"
			 
			 uniform sampler2D _MainTex;
			 float4 _Color;
			 struct v2f {
			     half4 pos : POSITION;
			     half2 uv : TEXCOORD0;
			 };
			 
			 v2f vert(appdata_img v) {
			     v2f o;
			     o.pos = UnityObjectToClipPos (v.vertex);
			     half2 uv = MultiplyUV( UNITY_MATRIX_TEXTURE0, v.texcoord );
			     o.uv = uv;
			     return o;
			 }

			 half4 frag (v2f i) : COLOR {
				 half4 color = tex2D(_MainTex, i.uv) * _Color;
				 if (color.a <= 0.1)
				 	discard;
			     return color;
			 }
			 ENDCG
		}
	}
}
