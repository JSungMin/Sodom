// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/WaveDistortion" {
	 Properties {
	     _MainTex ("Base (RGB)", 2D) = "white" {}
		 _Color ("Color", Color) = (1,1,1,1)
	 }
	 
	 SubShader {
	    Tags { 
			"Queue"="Transparent" 
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
		}
	    Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha
		Pass {
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
				 i.uv.x += sin((_Time.x + i.uv.y) * 3.14 * 15) * 0.0005;
			     half4 color = tex2D(_MainTex, i.uv) * _Color;
				 if (color.a <= 0.2)
				 	discard;
			     return color;
			 }
			 ENDCG
		}

	}
 
	Fallback off
}