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

Shader "WeatherMaker/WeatherMakerFullScreenOverlayShader"
{
	Properties
	{
		_OverlayTexture("Texture", 2D) = "white" {}
		_OverlayIntensity("Overlay Intensity", Float) = 0.0
		_OverlayNormalReducer("Overlay Normal Power", Float) = 0.5
		_OverlayScale("Overlay Scale", Float) = 0.005
		_OverlayOffset("Overlay Offset", Vector) = (0.0, 0.0, 0.0, 0.0)
		_OverlayVelocity("Overlay Velocity", Vector) = (0.0, 0.0, 0.0, 0.0)
		_OverlayColor("Overlay Color", Color) = (1.0, 1.0, 1.0, 1.0)
		_OverlaySpecularColor("Overlay Specular Color", Color) = (1.0, 1.0, 1.0, 1.0)
		_OverlaySpecularIntensity("Overlay Specular Intensity", Float) = 4.0
		_OverlaySpecularPower("Overlay Specular Power", Float) = 4.0
		_OverlayNoiseTexture("Noise Texture", 2D) = "white" {}
		_OverlayNoiseMultiplier("Noise Multiplier", Float) = 1.0
		_OverlayNoisePower("Noise Power", Float) = 1.0
		_OverlayNoiseAdder("Noise Adder", Float) = 0.0
		_OverlayNoiseScale("Noise Scale", Float) = 0.005
		_OverlayNoiseOffset("Noise Offset", Vector) = (0.0, 0.0, 0.0, 0.0)
		_OverlayNoiseVelocity("Noise Velocity", Vector) = (0.0, 0.0, 0.0, 0.0)
		_OverlayMinHeight("Min height", Float) = 0.0
		_OverlayMinHeigtNoiseMultiplier("Min height noise multiplier", Float) = 1.0
		_OverlayMinHeightNoiseScale("Min height noise scale", Float) = 0.04
		_OverlayMinHeightNoiseOffset("Min height noise offset", Vector) = (0.0, 0.0, 0.0, 0.0)
		_OverlayMinHeightNoiseVelocity("Min height noise velocity", Vector) = (0.0, 0.0, 0.0, 0.0)
		_DirectionalLightMultiplier("Directional light multiplier", Float) = 1.5
		_PointSpotLightMultiplier("Point/spot light multiplier", Float) = 1.5
		_AmbientLightMultiplier("Ambient light multiplier", Float) = 0.15
	}
	SubShader
	{
		Tags{ "Queue" = "Geometry+503" "IgnoreProjector" = "True" "RenderType" = "Transparent" "LightMode" = "Always" }
		Cull Off Lighting Off ZWrite Off ZTest [_ZTest] Fog{ Mode Off }
		Blend [_SrcBlendMode][_DstBlendMode]

		Pass
		{
			CGPROGRAM

			#pragma target 3.0
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma glsl_no_auto_normalization
			#pragma vertex full_screen_vertex_shader
			#pragma fragment full_screen_overlay_fragment_shader
			#pragma multi_compile __ ENABLE_OVERLAY_NOISE
			#pragma multi_compile __ ENABLE_OVERLAY_HEIGHT ENABLE_OVERLAY_HEIGHT_NOISE
			#pragma multi_compile __ SHADOWS_ONE_CASCADE
			#pragma multi_compile __ WEATHER_MAKER_DEFERRED_SHADING

			#define WM_IS_FULL_SCREEN_EFFECT

			#include "WeatherMakerOverlayShader.cginc"

			fixed4 full_screen_overlay_fragment_shader(full_screen_fragment i) : SV_Target
			{
				WM_INSTANCE_FRAG(i);

#if defined(WEATHER_MAKER_DEFERRED_SHADING)

				float3 gBufferNormal = tex2Dlod(_CameraGBufferTexture2, float4(i.uv, 0.0, 0.0)).xyz;
				if (gBufferNormal.x == 0.0 && gBufferNormal.y == 0.0 && gBufferNormal.z == 0.0)
				{
					return fixed4Zero;
				}
				// normal is already world space
				float3 normal = normalize((gBufferNormal * 2.0) - 1.0);
				float depth = GetDepth01(i.uv);

#else

				// forward rendering
				float depth;
				float3 normal;
				GetDepthAndNormal(i.uv, depth, normal);
				if (depth > 0.7)
				{
					// normals at far plane are not zero sadly, so we have to exit out to eliminate far plane artifacts
					return fixed4Zero;
				}
				normal = mul((float3x3)_WeatherMakerInverseView[unity_StereoEyeIndex], normal).xyz;
				if ((normal.x == 0.0 && normal.y == 0.0 && normal.z == 0.0))
				{
					return fixed4Zero;
				}

#endif

				float3 wpos = _WorldSpaceCameraPos + (depth * i.forwardLine);
				float3 ray = normalize(i.forwardLine);
				return ComputeOverlayColor(wpos, ray, normal, depth);
			}

			ENDCG
		}
	}
	Fallback Off
}
