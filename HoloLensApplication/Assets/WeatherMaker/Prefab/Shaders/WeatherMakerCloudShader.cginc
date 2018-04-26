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

#include "WeatherMakerShader.cginc"

uniform sampler2D _CloudNoise1;
uniform sampler2D _CloudNoise2;
uniform sampler2D _CloudNoise3;
uniform sampler2D _CloudNoise4;
uniform sampler2D _CloudNoiseMask1;
uniform sampler2D _CloudNoiseMask2;
uniform sampler2D _CloudNoiseMask3;
uniform sampler2D _CloudNoiseMask4;
uniform float4 _CloudNoiseScale[4];
uniform float4 _CloudNoiseMultiplier[4];
uniform float _CloudNoiseRotation[8]; // first 4 cos, second 4 sin
uniform float3 _CloudNoiseVelocity[4];
uniform float _CloudNoiseMaskScale[4];
uniform float2 _CloudNoiseMaskOffset[4];
uniform float3 _CloudNoiseMaskVelocity[4];
uniform float _CloudNoiseMaskRotation[8]; // first 4 cos, second 4 sin
uniform fixed4 _CloudColor[4];
uniform fixed4 _CloudEmissionColor[4];
uniform fixed _CloudAmbientMultiplier[4];
uniform float _CloudCover[4];
uniform float _CloudDensity[4];
uniform float _CloudHeight[4];
uniform float _CloudLightAbsorption[4];
uniform float _CloudSharpness[4];
uniform float _CloudShadowThreshold[4];
uniform float _CloudShadowPower[4];
uniform float _CloudShadowMultiplier;
uniform float _CloudRayOffset[4]; // brings clouds down at the horizon at the cost of stretching them over the top

//TODO: Volumetric clouds
//uniform float _CloudSampleCount[4];
//uniform float4 _CloudSampleStepMultiplier[4];
//uniform float4 _CloudSampleDitherMagic[4];
//uniform float _CloudSampleDitherIntensity[4];

// current layer
int _CloudIndex = 0;

#if defined(ENABLE_CLOUDS) || defined(ENABLE_CLOUDS_MASK)

// returns world pos of cloud plane intersect
float3 CloudRaycastWorldPos(float3 ray)
{
	float3 planePos = float3(0, _CloudHeight[_CloudIndex], 0);
	float distanceToPlane;
	float planeMultiplier = RayPlaneIntersect(_WorldSpaceCameraPos, ray, float3(0.0, 1.0, 0.0), planePos, distanceToPlane);
	return planeMultiplier * (_WorldSpaceCameraPos + (ray * distanceToPlane));
}

inline float ComputeCloudFBMInner(float3 rayDir, float3 worldPos, sampler2D noiseTex)
{
	//float3 noisePos = (worldPos * _CloudNoiseScale[_CloudIndex].x) + _CloudNoiseVelocity[_CloudIndex];
	//return ((tex2Dlod(noiseTex, float4(noisePos.xz, 0.0, 0.0)).a));

	float fbm = 0.0;
	float hasY = (float)(_CloudNoiseScale[_CloudIndex].y > 0.0 && _CloudNoiseMultiplier[_CloudIndex].y > 0.0);
	float hasZ = (float)(_CloudNoiseScale[_CloudIndex].z > 0.0 && _CloudNoiseMultiplier[_CloudIndex].z > 0.0);
	float hasW = (float)(_CloudNoiseScale[_CloudIndex].w > 0.0 && _CloudNoiseMultiplier[_CloudIndex].w > 0.0);
	float3 noisePos;

	//float sampleCount = _CloudSampleCount[_CloudIndex];
	//float3 step = rayDir * _CloudSampleStepMultiplier[_CloudIndex].x;
	//float3 step2 = rayDir * _CloudSampleStepMultiplier[_CloudIndex].y;
	//float3 step3 = rayDir * _CloudSampleStepMultiplier[_CloudIndex].z;
	//float3 step4 = rayDir * _CloudSampleStepMultiplier[_CloudIndex].w;
	//float i = 0.0;
	//float maxFbm = sampleCount * 0.2;

	//UNITY_LOOP
	//for (; i < sampleCount && fbm < maxFbm; i++)
	//{
		noisePos = ((worldPos/* + (step * i)*/) * _CloudNoiseScale[_CloudIndex].x) + _CloudNoiseVelocity[_CloudIndex];
		fbm += ((tex2Dlod(noiseTex, float4(RotateUV(noisePos.xz, _CloudNoiseRotation[_CloudIndex + 4], _CloudNoiseRotation[_CloudIndex]), 0.0, 0.0)).a) * _CloudNoiseMultiplier[_CloudIndex].x);
		if (hasY)
		{
			noisePos = ((worldPos/* + (step2 * i)*/) * _CloudNoiseScale[_CloudIndex].y) + _CloudNoiseVelocity[_CloudIndex];
			fbm += ((tex2Dlod(noiseTex, float4(RotateUV(noisePos.xz, _CloudNoiseRotation[_CloudIndex + 4], _CloudNoiseRotation[_CloudIndex]), 0.0, 0.0)).a) * _CloudNoiseMultiplier[_CloudIndex].y);
		}
		if (hasZ)
		{
			noisePos = ((worldPos/* + (step3 * i)*/) * _CloudNoiseScale[_CloudIndex].z) + _CloudNoiseVelocity[_CloudIndex];
			fbm += ((tex2Dlod(noiseTex, float4(RotateUV(noisePos.xz, _CloudNoiseRotation[_CloudIndex + 4], _CloudNoiseRotation[_CloudIndex]), 0.0, 0.0)).a) * _CloudNoiseMultiplier[_CloudIndex].z);
		}
		if (hasW)
		{
			noisePos = ((worldPos/* + (step4 * i)*/) * _CloudNoiseScale[_CloudIndex].w) + _CloudNoiseVelocity[_CloudIndex];
			fbm += ((tex2Dlod(noiseTex, float4(RotateUV(noisePos.xz, _CloudNoiseRotation[_CloudIndex + 4], _CloudNoiseRotation[_CloudIndex]), 0.0, 0.0)).a) * _CloudNoiseMultiplier[_CloudIndex].w);
		}
	//}

	return fbm;

}

float ComputeCloudFBMOutter(float3 rayDir, float3 worldPos, sampler2D noiseTex, sampler2D maskTex, float2 screenUV)
{
	// calculate cloud values
	float sharpness = _CloudSharpness[_CloudIndex];
	float cover = _CloudCover[_CloudIndex];
	float fbm = ComputeCloudFBMInner(rayDir, worldPos, noiseTex);
	//fbm = saturate(sharpness > 0.0 ? (1.0 - (pow(_CloudSharpness[_CloudIndex], fbm - (1.0 - pow(cover, 0.5))))) : (fbm * cover));
	if (sharpness > 0.0)
	{
		fbm = min(1.0, (1.0 - (pow(sharpness, (1.5 * fbm) - (1.0 - cover)))));
	}
	else
	{
		fbm = fbm * cover;
	}

#if defined(ENABLE_CLOUDS_MASK)

	float2 maskRotated = RotateUV(worldPos.xz, _CloudNoiseMaskRotation[_CloudIndex + 4], _CloudNoiseMaskRotation[_CloudIndex]);
	float maskNoise = CalculateNoiseXZ(maskTex, float3(maskRotated.x, 0.0, maskRotated.y), _CloudNoiseMaskScale[_CloudIndex], _CloudNoiseMaskOffset[_CloudIndex], _CloudNoiseMaskVelocity[_CloudIndex], 1.0, 0.0);
	fbm *= maskNoise;

#endif

	return fbm;
}

fixed3 ComputeDirectionalLightCloud(fixed4 lightColor, float3 lightDir, float3 rayDir, float fbm, float scatterMultiplier, float powerX, float powerY)
{
	// sunlight scatter - direct line of sight
	float lightDot = max(0.0, dot(lightDir, rayDir));
	lightDot = pow(lightDot, powerX);
	float lightMultiplier = (lightDot * lightColor.a * scatterMultiplier);

	// light indirect + direct
	float indirectLight = lightColor.a;
	lightColor *= lightColor.a;

	indirectLight /= pow(fbm, 0.5);
	//sunLightDot = max(0.0, dot(_WeatherMakerSunDirectionUp, fbmAndNormal.xyz));
	return (indirectLight + (lightColor.a * lightColor.rgb * lightMultiplier * lightMultiplier * powerY));
}

fixed3 ComputeCloudLighting(float fbm, float3 rayDir, float3 worldPos, fixed4 sunColor, fixed alphaAccum)
{
	float cloudDensity = min(1.0, _CloudDensity[_CloudIndex] * 1.5);
	float invCloudDensity = 1.0 - cloudDensity;
	float invFbm = 1.0 - fbm;
	float dirLightReducer = max(0.0, 1.0 - alphaAccum);
	float scatterMultiplier = invCloudDensity * invCloudDensity * (1.0 - alphaAccum);
	fixed3 cloudLight = ComputeDirectionalLightCloud(sunColor, _WeatherMakerSunDirectionUp, rayDir, fbm, scatterMultiplier, _WeatherMakerSunLightPower.x, _WeatherMakerSunLightPower.y);

	// moonlight
	for (int i = 0; i < _WeatherMakerMoonCount; i++)
	{
		cloudLight += ComputeDirectionalLightCloud(_WeatherMakerMoonLightColor[i], _WeatherMakerMoonDirectionUp[i], rayDir, fbm, scatterMultiplier, _WeatherMakerMoonLightPower[i].x, _WeatherMakerMoonLightPower[i].y);
	}

	// reduce directional lights by previous density (higher layers)
	cloudLight *= dirLightReducer;

	// reduce directional light by cloud light absorption factor
	float dirLightDensityFactor = min(1.0, invFbm * _CloudLightAbsorption[_CloudIndex] * 10.0);
	cloudLight *= dirLightDensityFactor;

	// additional lights, probably under or inside the clouds, so reduce as the particle density decreases
	cloudLight +=
	(
		fbm * CalculateLightColorWorldSpace
		(
			worldPos,
			float3Zero,
			0.0,
			0.0
		)
	);

	// ambient, this is assumed to be below the clouds, so reduce as particle density decreases
	cloudLight += (fbm * _WeatherMakerAmbientLightColor.rgb * _AmbientLightMultiplier * _CloudAmbientMultiplier[_CloudIndex]);

	return cloudLight;
}

fixed4 ComputeCloudColor(float3 rayDir, sampler2D noiseTex, sampler2D maskTex, float2 screenUV, inout fixed4 sunColor, out float3 worldPos, inout fixed alphaAccum)
{
	worldPos = CloudRaycastWorldPos(rayDir);

	// miss, exit out
	if (worldPos.y < 1.0)
	{
		return fixed4(0.0, 0.0, 0.0, 0.0);
	}

	float fbm = ComputeCloudFBMOutter(rayDir, worldPos, noiseTex, maskTex, screenUV);
	if (fbm < 0.005)
	{
		// fast out for transparent areas, avoids a lot of light calculations
		return fixed4(0.0, 0.0, 0.0, 0.0);
	}

	// compute lighting
	fixed3 litCloud = ComputeCloudLighting(fbm, rayDir, worldPos, sunColor, alphaAccum);

	// calculate alpha for the particle
	fixed alpha = min(1.0, (fbm + fbm));
	alpha = pow(alpha, 1.0 - alpha);
	alpha *= alpha;
	//alpha = min(1.0, alpha * fbm * fbm * 3.5);

	// calculate directional light reduction for future layers
	alphaAccum = min(1.0, alphaAccum + (alpha * _CloudDensity[_CloudIndex]));

	return fixed4(((_CloudColor[_CloudIndex] * _WeatherMakerSkyGradientColor.rgb * litCloud) +
		(_CloudEmissionColor[_CloudIndex].rgb * _CloudEmissionColor[_CloudIndex].a)), alpha);
}

#endif
