//
// Weather Maker for Unity
// (c) 2016 Digital Ruby, LLC
// Source code may be used for personal or commercial projects.
// Source code may NOT be redistributed or sold.
// 
// *** A NOTE ABOUT PIRACY ***
// 
// If you got this asset off of leak forums or any other horrible evil pirate site, please consider buying it from the Unity asset store at https ://www.assetstore.unity3d.com/en/#!/content/60955?aid=1011lGnL. This asset is only legally available from the Unity Asset Store.
// 
// I'm a single indie dev supporting my family by spending hundreds and thousands of hours on this and other assets. It's very offensive, rude and just plain evil to steal when I (and many others) put so much hard work into the software.
// 
// Thank you.
//
// *** END NOTE ABOUT PIRACY ***
//

// TODO: Volumetric clouds
// http://bitsquid.blogspot.com/2016/07/volumetric-clouds.html
// https://github.com/greje656/clouds
// http://patapom.com/topics/Revision2013/Revision%202013%20-%20Real-time%20Volumetric%20Rendering%20Course%20Notes.pdf

Shader "WeatherMaker/WeatherMakerFullScreenCloudsShader"
{
	Properties
	{
		_PointSpotLightMultiplier("Point/Spot Light Multiplier", Range(0, 10)) = 1
		_DirectionalLightMultiplier("Directional Light Multiplier", Range(0, 10)) = 1
		_AmbientLightMultiplier("Ambient light multiplier", Range(0, 4)) = 1
	}
	SubShader
	{
		Tags{ "Queue" = "Geometry+503" "IgnoreProjector" = "True" "RenderType" = "Transparent" "LightMode" = "Always" }
		Cull Off Lighting Off ZWrite Off ZTest LEqual Fog { Mode Off }
		Blend [_SrcBlendMode][_DstBlendMode]

		Pass
		{
			CGPROGRAM

			#pragma vertex full_screen_vertex_shader
			#pragma fragment frag
			#pragma multi_compile __ ENABLE_CLOUDS ENABLE_CLOUDS_MASK
			#pragma multi_compile __ UNITY_HDR_ON

			#define WM_IS_FULL_SCREEN_EFFECT
			
			#include "WeatherMakerCloudShader.cginc"

			// fixed4 cloudColor, inout fixed4 finalColor, inout fixed4 sunLightColor)
			#define blendClouds(cloudColor, finalColor, sunLightColor) \
				finalColor.rgb = (cloudColor.rgb * cloudColor.a) + (finalColor.rgb * (1.0 - cloudColor.a)); \
				finalColor.a = max(finalColor.a, cloudColor.a);

			fixed4 frag (full_screen_fragment i) : SV_Target
			{

#if defined(ENABLE_CLOUDS) || defined(ENABLE_CLOUDS_MASK)

				WM_INSTANCE_FRAG(i);
				float3 cloudRay = normalize(i.forwardLine);
				fixed4 sunLightColor = _WeatherMakerSunColor;
				float3 worldPos;
				fixed4 finalColor = fixed4(0.0, 0.0, 0.0, 0.0);
				fixed4 cloudColor;
				fixed alphaAccum = 0.0;

				// top layer
				if (_CloudCover[3] > 0.0)
				{
					_CloudIndex = 3;
					cloudColor = ComputeCloudColor(float3(cloudRay.x, cloudRay.y + _CloudRayOffset[3], cloudRay.z), _CloudNoise4, _CloudNoiseMask4, i.uv, sunLightColor, worldPos, alphaAccum);
					blendClouds(cloudColor, finalColor, sunLightColor);
				}

				// next layer
				if (_CloudCover[2] > 0.0)
				{
					_CloudIndex = 2;
					cloudColor = ComputeCloudColor(float3(cloudRay.x, cloudRay.y + _CloudRayOffset[2], cloudRay.z), _CloudNoise3, _CloudNoiseMask3, i.uv, sunLightColor, worldPos, alphaAccum);
					blendClouds(cloudColor, finalColor, sunLightColor);
				}

				// next layer
				if (_CloudCover[1] > 0.0)
				{
					_CloudIndex = 1;
					cloudColor = ComputeCloudColor(float3(cloudRay.x, cloudRay.y + _CloudRayOffset[1], cloudRay.z), _CloudNoise2, _CloudNoiseMask2, i.uv, sunLightColor, worldPos, alphaAccum);
					blendClouds(cloudColor, finalColor, sunLightColor);
				}

				// bottom layer
				if (_CloudCover[0] > 0.0)
				{
					_CloudIndex = 0;
					cloudColor = ComputeCloudColor(float3(cloudRay.x, cloudRay.y + _CloudRayOffset[0], cloudRay.z), _CloudNoise1, _CloudNoiseMask1, i.uv, sunLightColor, worldPos, alphaAccum);
					blendClouds(cloudColor, finalColor, sunLightColor);
				}

				// reduce color by alpha, which is cloud intensity
				finalColor.rgb *= finalColor.a;
				ApplyDither(finalColor.rgb, i.uv, _WeatherMakerSkyDitherLevel);
				return finalColor;

#else

				return fixed4(0.0, 0.0, 0.0, 0.0);

#endif

			}

			ENDCG
		}
	}
}
