// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "Sprites/Water"
{
    Properties
    {
       	_MainTex ("Main Texture", 2D) = "bump" {}
       	_BumpTex ("Bump Texture", 2D) = "bump" {}
       	_DetailBumpTex ("Detail Texture", 2D) = "bump" {}
       	_HeightDispTex ("Height Displace Map", 2D) = "bump" {}
       	_CollisionTex ("Collision Map", 2D) = "bump" {}

        _Colour ("Colour", Color) = (1,1,1,1)
        _BaseScrollSpeed ("Base Scroll Speed", Float) = 0.05
        _DetailScrollSpeed ("Detail Scroll Speed", Float) = 0.08
        _BaseMagnitude ("Base Magnitude", Range(0,1)) = 0.05
        _DetailMagnitude ("Detail Magnitude", Range(0,1)) = 0.1
        _WaterHeight ("Water Height", Float) = 0.2
        _WaterWaveHeight ("Water Wave Height", Float) = 0.025
        _WaterFreq ("Water Frequency", Float) = 60

		_WaterIncline("Incline", Float) = 2
		_PivotOffset("Water Pivot", Vector) = (0,0.5,0,1)

        [MaterialToggle] UseSparkle ("Use Sparkle", Float) = 1
        _SparkleColour ("Sparkle Colour", Color) = (1,1,1,1)
        _SparkleNum ("Sparkle Number", Range(3,10)) = 5
        _SparkleDistance ("Sparkle Distance", Float) = 0.02
        _SparkleThreshold ("Sparkle Threshold", Float) = 60
        [MaterialToggle] UseGradation ("Use Gradation", Float) = 1
        _BottomTint ("Top Tint", Color) = (1,1,1,1)
        _TopTint ("Bottom Tint", Color) = (1,1,1,1)
        _BottomWeight ("Top Weight", Float) = 1
        _TopWeight ("Bottom Weight", Float) = 1
        _TintWeight ("Gradation Weight", Range(0,1)) = 0.5
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
        [PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
        [PerRendererData] _EnableExternalAlpha ("Enable External Alpha", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent" "IgnoreProjection"="True" "RenderType"="Opaque"
        }
        ZWrite On Lighting Off Cull Off Fog { Mode Off } Blend One Zero
        GrabPass
        {
        	"_BackgroundTexture"
        }

        Pass
        {
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.0
            #pragma multi_compile_instancing
            #pragma multi_compile _ PIXELSNAP_ON
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
            #include "UnityCG.cginc"

            fixed4		_Colour;
            float4		_RendererColor;
            float4		_BumpTex_ST;
			fixed4		_PivotOffset;
			float 		_WaterIncline;
          	float 		_BaseScrollSpeed;
          	float		_DetailScrollSpeed;
            float 		_BaseMagnitude;
            float 		_DetailMagnitude;
            float		_WaterHeight;
            float		_WaterWaveHeight;
            float		_WaterFreq;
            sampler2D 	_MainTex;
            sampler2D 	_BumpTex;
            sampler2D 	_DetailBumpTex;
            sampler2D 	_HeightDispTex;
			sampler2D 	_BackgroundTexture;
			sampler2D	_CollisionTex;

			bool 		UseSparkle;
			float4		_SparkleColour;
			float 		_SparkleDistance;
			float 		_SparkleThreshold;
			float		_SparkleNum;
			bool		UseGradation;
			float4 		_TopTint;
			float4 		_BottomTint;
			float		_TopWeight;
			float		_BottomWeight;
			float 		_TintWeight;
				
			struct appdata_t
			{
			    float4 vertex   : POSITION;
			    float4 color    : COLOR;
			    float2 texcoord : TEXCOORD0;
			    UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
			    float4 vertex   : SV_POSITION;
			    fixed4 color    : COLOR;
			    float2 texcoord : TEXCOORD0;
			    float4 grabPos	: TEXCOORD1;
			    UNITY_VERTEX_OUTPUT_STEREO
			};

			//	HELP FUNCTION BLOCK
			float getAngle (half2 coord01, half2 coord02)
			{
				return acos(dot (normalize(coord01), normalize(coord02))/(length(coord01) * length(coord02))) * 57.3;
			}

			//	END HELP FUNCTION BLOCK

            v2f vert(appdata_t IN)
			{
			    v2f OUT;

			    UNITY_SETUP_INSTANCE_ID (IN);
			    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

			#ifdef UNITY_INSTANCING_ENABLED
			    IN.vertex.xy *= _Flip;
			#endif

			    OUT.vertex = UnityObjectToClipPos(IN.vertex);
			    OUT.texcoord = TRANSFORM_TEX (IN.texcoord, _BumpTex);

			    #ifdef PIXELSNAP_ON
			    OUT.vertex = UnityPixelSnap (OUT.vertex);
			    #endif

			    #if UNITY_UV_STARTS_AT_TOP
			    float scale = -1.0;
			    #else
			    float scale = 1.0;
			    #endif

			    OUT.grabPos.xy = (float2(OUT.vertex.x, (OUT.vertex.y) * scale) + OUT.vertex.w) * 0.5;
			    OUT.grabPos.zw = OUT.vertex.zw;

			    float4 top = UnityObjectToClipPos(_PivotOffset);

			    OUT.grabPos.y = 1 - (OUT.grabPos.y + top.y);

			    return OUT;
			}

            half4 frag(v2f IN) : COLOR
			{
				//IN.grabPos.y = IN.grabPos.y;

				float2 coord = IN.texcoord;
				#if UNITY_UV_STARTS_AT_TOP
				coord.y *= -1;
				#endif
				//	Default Water Process
				half2 perspectiveCorrection = half2(_WaterIncline * (IN.texcoord.x - 0.5) * IN.texcoord.y, 0.0);
				half4 bump = tex2D(_BumpTex, coord + float2(_Time.z * _BaseScrollSpeed,0) + perspectiveCorrection);
				half4 detailBump = tex2D(_DetailBumpTex, coord + float2(_Time.z * _DetailScrollSpeed,0) + perspectiveCorrection);
				half4 waterHeight = tex2D(_HeightDispTex, IN.texcoord);

				half2 distortion = UnpackNormal(bump).rg;
				half2 detailDistortion = UnpackNormal(detailBump).rg;
				half4 cColor = tex2D (_CollisionTex, float2(IN.texcoord.x - 0.5, IN.texcoord.y * 0.25) + (distortion + detailDistortion) * 0.0025);

				half2 basePos = IN.grabPos.xy + distortion * _BaseMagnitude;
				half2 detailPos = IN.grabPos.xy + detailDistortion * _DetailMagnitude;

				half4 bgColor = tex2D(_BackgroundTexture, basePos);
				half4 deColor = tex2D(_BackgroundTexture, detailPos);
				//	Average Result
				half4 result = (bgColor + deColor) * 0.5;

				//	Water Height Process
				float dis = waterHeight.y;
				float thres = (1 - (_WaterHeight + sin(IN.texcoord.x * _WaterFreq + _Time.z) * dis * _WaterWaveHeight));
				clip(IN.texcoord.y >= thres ? -1 : 1);

				//	Gradation Process
				if (UseGradation)
				{
					result *= lerp (_TopTint * _TopWeight, _BottomTint * _BottomWeight, (IN.texcoord.y * _TintWeight * 2));
				}

				//	Collide Process
				if (cColor.w > 0)
				{
					result += cColor;
				}

				//	Sparkle Process
				float angle = getAngle (distortion, detailDistortion);
				if (UseSparkle)
				{
					if (angle >= _SparkleThreshold)
					{

						float sumAngle = 0;
						half4 tmpBump;
						half4 tmpDetailBump;
						half2 tmpDistortion;
						half2 tmpDetailDistortion;
						float tmpAngle;
						int loopNum = floor(_SparkleNum * 0.5);
						[unroll(10)]
						for (int i = -loopNum; i < loopNum; i++)
						{
							tmpBump = tex2D(_BumpTex, coord + float2(_Time.z * _BaseScrollSpeed + _SparkleDistance * i, 0) + perspectiveCorrection);
							tmpDetailBump = tex2D(_DetailBumpTex, coord + float2(_Time.z * _DetailScrollSpeed + _SparkleDistance * i, 0) + perspectiveCorrection);
							tmpDistortion = UnpackNormal (tmpBump).rg;
							tmpDetailDistortion = UnpackNormal (tmpDetailBump).rg;
							tmpAngle = getAngle (tmpDistortion, tmpDetailDistortion);
							sumAngle += tmpAngle;
						}
						if (sumAngle >= _SparkleThreshold * _SparkleNum)
						{
							result = lerp (result, _SparkleColour, 1 - (angle)/180);
							return result;
						}

					}
				}
				return result * _Colour;
			}
		ENDCG
        }
    }
}
