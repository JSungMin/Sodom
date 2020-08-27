// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/SpriteLighting"
{
    Properties
    {
        _MainTex ("Diffuse Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
		_RayShapeWidth("Ray Width Ratio", Range(1,10)) = 1
		_RayShapeHeight("Ray Height Ratio", Range(1,10)) = 1
        _ContrastFactor ("Contrast Factor", Float) = 1.0
        _IntensityFactor ("Intensity Variation Factor", Float) = 0.5
        _ParticleFactor ("Particle Factor", Float) = 1.0
        _PivotPoint ("Pivot", Vector) = (0,0,0,0)
		_CenterPoint("Center", Vector) = (0.5,0.5,0,0)
        _Radius ("Light Radius", Float) = 1.5
        _FlickerPeroid ("Flicker Peroid", Float) = 1
		_PixelSize("PixelSize", Float) = 1
    }
     
    SubShader
    {
         Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "ForceNoShadowCasting"="True"
        }
        Pass
        {
			ZWrite Off
            Blend DstColor One // Keep deep black values
             
            CGPROGRAM
              
            #pragma vertex vert
            #pragma fragment frag
              
            #include "UnityCG.cginc"
              
            // User-specified properties
            uniform sampler2D _MainTex;
            uniform float4 _Color;
            uniform float _RayShapeWidth;
			uniform float _RayShapeHeight;
			uniform float _ContrastFactor;
            uniform float _IntensityFactor;

            fixed4 _PivotPoint;
			fixed4 _CenterPoint;
            fixed _Radius;
            fixed _FlickerPeroid;
			fixed _PixelSize;

            struct VertexInput
            {
                float4 vertex : POSITION;
                float4 uv : TEXCOORD0;
                float4 color : COLOR;
            };
              
            struct VertexOutput
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
                float intensity : TEXCOORD1;
                float4 uvgrab : TEXCOORD2;
            };
              
            VertexOutput vert(VertexInput input)
            {
                VertexOutput output;
                output.pos = UnityObjectToClipPos(input.vertex);
				#ifdef PIXELSNAP_ON
				OUT.vertex = UnityPixelSnap(OUT.vertex);
				#endif
                output.uv = input.uv;
                output.color = input.color;
                output.intensity = _IntensityFactor * _ContrastFactor * cos(_Time.z/_FlickerPeroid) * sin(_Time.w/_FlickerPeroid) * _CosTime.w
                                + 1.0 * _ContrastFactor; 
                return output;
            }
              
            float4 frag(VertexOutput input) : COLOR
            {
                float4 diffuseColor = tex2D(_MainTex, input.uv);
                fixed2 uv = input.uv;
				
				uv += _PivotPoint;
				uv.x /= _RayShapeWidth;
				uv.y /= _RayShapeHeight;
				float dis = distance (uv, _CenterPoint.xy);
                float maxDis = distance(_PivotPoint,_CenterPoint.xy);
                // Retrieve color from texture and multiply it by tint color and by sprite color
                // Multiply everything by texture alpha to emulate transparency
                diffuseColor.rgb = diffuseColor.rgb * _Color.rgb * input.color.rgb;
                diffuseColor.rgb *= diffuseColor.a * _Color.a * input.color.a;
                diffuseColor *= input.intensity;
				float delta = _Radius - dis;
    			diffuseColor *= pow (delta, 1.5);
    			if (dis > _Radius )
    			{
    				diffuseColor = float4(0,0,0,0);
    			}
                return diffuseColor;
              }
          
               ENDCG
           }
		Pass
        {
			ZWrite Off
            Blend SrcAlpha One // Add colours to the previous pixels
 
            CGPROGRAM
              
            #pragma vertex vert
            #pragma fragment frag
              
            #include "UnityCG.cginc"
              
            // User-specified properties
            uniform sampler2D _MainTex;
            uniform float4 _Color;
            uniform float _RayShapeWidth;
			uniform float _RayShapeHeight;
			uniform float _ContrastFactor;
            uniform float _IntensityFactor;

            fixed4 _PivotPoint;
			fixed4 _CenterPoint;
            fixed _Radius;
            fixed _FlickerPeroid;
			fixed _PixelSize;
              
            struct VertexInput
            {
                float4 vertex : POSITION;
                float4 uv : TEXCOORD0;
                float4 color : COLOR;
            };
              
            struct VertexOutput
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
                float intensity : TEXCOORD1;
                float4 uvgrab : TEXCOORD2;
            };
              
             VertexOutput vert(VertexInput input)
            {
                VertexOutput output;
                output.pos = UnityObjectToClipPos(input.vertex);
				#ifdef PIXELSNAP_ON
				OUT.vertex = UnityPixelSnap(OUT.vertex);
				#endif
                output.uv = input.uv;
                output.color = input.color;
                output.intensity = _IntensityFactor * _ContrastFactor * cos(_Time.z/_FlickerPeroid) * sin(_Time.w/_FlickerPeroid) * _CosTime.w
                                + 1.0 * _ContrastFactor; 
                return output;
            }
              
           float4 frag(VertexOutput input) : COLOR
            {
                float4 diffuseColor = tex2D(_MainTex, input.uv);
                fixed2 uv = input.uv;
				
				uv += _PivotPoint;
				uv.x /= _RayShapeWidth;
				uv.y /= _RayShapeHeight;
				float dis = distance (uv, _CenterPoint.xy);
                float maxDis = distance(_PivotPoint,_CenterPoint.xy);
                // Retrieve color from texture and multiply it by tint color and by sprite color
                // Multiply everything by texture alpha to emulate transparency
                diffuseColor.rgb = diffuseColor.rgb * _Color.rgb * input.color.rgb;
                diffuseColor.rgb *= diffuseColor.a * _Color.a * input.color.a;
                diffuseColor *= input.intensity;
				float delta = _Radius - dis;
    			diffuseColor *= pow (delta, 1.5);
    			if (dis > _Radius )
    			{
    				diffuseColor = float4(0,0,0,0);
    			}
                return diffuseColor;
              }
          
            ENDCG
        }
    }
}