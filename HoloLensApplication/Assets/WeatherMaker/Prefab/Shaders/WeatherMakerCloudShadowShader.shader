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

Shader "WeatherMaker/WeatherMakerCloudShadowShader"
{
	Properties
	{
	}

	SubShader
	{
		CGINCLUDE

		#pragma target 3.0
		#pragma fragmentoption ARB_precision_hint_fastest
		#pragma glsl_no_auto_normalization
		#pragma multi_compile __ ENABLE_CLOUDS ENABLE_CLOUDS_MASK

		#include "WeatherMakerCloudShader.cginc"

		ENDCG
		
		Pass
		{
			Name "WeatherMakerCloudShadowPass"
			Cull Off Lighting Off ZWrite Off ZTest Off Blend Off

			CGPROGRAM

			#pragma vertex shadowVert
			#pragma fragment shadowFrag

			float3 _CloudShadowCenterPoint;

			v2fShadow shadowVert(appdata_base v)
			{
				v2fShadow o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.worldPos = WorldSpaceVertexPos(v.vertex);
				return o;
			}

			fixed4 shadowFrag(v2fShadow i) : SV_TARGET
			{

#if defined(ENABLE_CLOUDS) || defined(ENABLE_CLOUDS_MASK)

				// shadow strength * sun alpha * shadow multiplier * fbm
				fixed rgb = ComputeCloudFBMOutter(float3(0.0, 0.0, 0.0), float3(i.worldPos.x, _CloudHeight[0], i.worldPos.y), _CloudNoise1, _CloudNoiseMask1, float2(0.0, 0.0));
				fixed shadowMultiplier = (_WeatherMakerSunLightPower.z + _WeatherMakerSunColor.a) * 0.5;
				fixed a = saturate((shadowMultiplier * _CloudShadowMultiplier * rgb * (1.0 + _CloudCover[0])) - _CloudShadowThreshold[0]);
				a = pow(a, _CloudShadowPower[0]);

				// restrict to circle
				// (x - center_x)^2 + (y - center_y)^2 < radius^2.
				float dx = (i.worldPos.x - _CloudShadowCenterPoint.x);
				dx *= dx;
				float dy = (i.worldPos.y - _CloudShadowCenterPoint.z);
				dy *= dy;
				bool inCircle = ((dx + dy) <= _ProjectionParams.z * _ProjectionParams.z);
				a *= inCircle;

				return fixed4(0.0, 0.0, 0.0, a);

#else

				return 1.0;

#endif

			}

			ENDCG
		}	
	}

	Fallback Off
}