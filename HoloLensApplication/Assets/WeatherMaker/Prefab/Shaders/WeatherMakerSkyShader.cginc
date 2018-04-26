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

#ifndef SKYBOX_COLOR_IN_TARGET_COLOR_SPACE
#if defined(SHADER_API_MOBILE)
#define SKYBOX_COLOR_IN_TARGET_COLOR_SPACE 1
#else
#define SKYBOX_COLOR_IN_TARGET_COLOR_SPACE 0
#endif
#endif

struct procedural_sky_info
{
	fixed3 inScatter;
	fixed3 outScatter;
	fixed4 skyColor;
};

struct v2fSky
{
	float4 vertex : SV_POSITION;
	float2 uv : TEXCOORD0;
	float3 ray : TEXCOORD1;
	float3 normal : NORMAL;
	fixed3 inScatter : COLOR0;
	fixed3 outScatter : COLOR1;
	WM_BASE_VERTEX_TO_FRAG
};

uniform sampler2D _DawnDuskTex;
uniform float4 _DawnDuskTex_ST;
uniform sampler2D _NightTex;
uniform float4 _NightTex_ST;
uniform fixed _NightSkyMultiplier;
uniform fixed _NightVisibilityThreshold;
uniform fixed _NightIntensity;
uniform fixed _NightTwinkleSpeed;
uniform fixed _NightTwinkleVariance;
uniform fixed _NightTwinkleMinimum;
uniform fixed _NightTwinkleRandomness;

uniform fixed3 _SkyTintColor;
uniform fixed _WeatherMakerSkySamples = 2.0;
uniform fixed4 _WeatherMakerSkyMieG; // -mieG, mieG * mieG, mieG, sun mieDot
uniform fixed _WeatherMakerSkyAtmosphereThickness = 1.0;
uniform fixed4 _WeatherMakerSkyRadius; // outer, outer * outer, inner, inner * inner
uniform fixed4 _WeatherMakerSkyMie; // x, y, z, w
uniform fixed4 _WeatherMakerSkyLightScattering;
uniform fixed4 _WeatherMakerSkyLightPIScattering;
uniform fixed3 _WeatherMakerSkyTintColor;
uniform fixed4 _WeatherMakerSkyScale; // scale factor, scale depth, scale / scale depth, camera height
uniform fixed4 _WeatherMakerSkyTotalRayleigh; // w = sun fade
uniform fixed4 _WeatherMakerSkyTotalMie; // w = total sun intensity

inline fixed GetMiePhase(fixed size, fixed eyeCos, fixed eyeCos2, fixed power)
{
	fixed temp = 1.0 + _WeatherMakerSkyMieG.y + (2.0 * _WeatherMakerSkyMieG.x * eyeCos);
	temp = max(1.0e-4, smoothstep(0.0, 0.005, temp) * temp);
	fixed mie = saturate(size * _WeatherMakerSkyMie.x * ((1.0 + eyeCos2) / temp));
	return pow(mie, power);
}

inline fixed GetSkyMiePhase(fixed eyeCos, fixed eyeCos2)
{
	return (_WeatherMakerSkyMie.x * (1.0 + eyeCos2) / pow((_WeatherMakerSkyMie.y + _WeatherMakerSkyMie.z * eyeCos), 1.5));
}

inline fixed GetRayleighPhase(fixed eyeCos2)
{
	return 0.75 + 0.75 * eyeCos2;
}

inline fixed GetRayleighPhase(fixed3 light, fixed3 ray)
{
	fixed eyeCos = dot(light, ray);
	return GetRayleighPhase(eyeCos * eyeCos);
}

float GetRayleighPhasePreetham(float cosTheta)
{
	return (3.0 / (16.0 * PI)) * (1.0 + (cosTheta * cosTheta));
}

float GetHgPhasePreetham(float cosTheta)
{
	float inverse = 1.0 / pow(1.0 - 2.0 * _WeatherMakerSkyMieG.z * cosTheta + _WeatherMakerSkyMieG.y, 1.5);
	return (1.0 / PI) * ((1.0 - _WeatherMakerSkyMieG.y) * inverse);
}

inline fixed CalcSunSpot(fixed size, fixed3 vec1, fixed3 vec2)
{
	half3 delta = vec1 - vec2;
	half dist = length(delta);
	half spot = 1.0 - smoothstep(0.0, size, dist);
	return saturate(100 * spot * spot);
}

inline fixed4 GetSunColorFast(float3 sunNormal, fixed4 sunColor, fixed size, float3 ray)
{
	fixed sun = CalcSunSpot(size, sunNormal, ray);
	return (sun * sunColor);
}

inline float GetSkyScale(float inCos)
{
	float x = 1.0 - inCos;
#if defined(SHADER_API_N3DS)
	// The polynomial expansion here generates too many swizzle instructions for the 3DS vertex assembler
	// Approximate by removing x^1 and x^2
	return 0.25 * exp(-0.00287 + x * x * x * (-6.80 + x * 5.25));
#else
	return 0.25 * exp(-0.00287 + x * (0.459 + x * (3.83 + x * (-6.80 + x * 5.25))));
#endif

}

procedural_sky_info CalculateScatteringCoefficients(float3 lightDir, fixed3 lightColor, float scale, float3 eyeRay)
{
	procedural_sky_info o;
	eyeRay.y = max(-0.01, eyeRay.y);

	float outerRadius = _WeatherMakerSkyRadius.x;
	float outerRadius2 = _WeatherMakerSkyRadius.y;
	float innerRadius = _WeatherMakerSkyRadius.z;
	float innerRadius2 = _WeatherMakerSkyRadius.w;
	float scaleFactor = _WeatherMakerSkyScale.x * scale;
	float scaleDepth = _WeatherMakerSkyScale.y;
	float scaleFactorOverDepth = _WeatherMakerSkyScale.z;
	float cameraHeight = _WeatherMakerSkyScale.w;

	// the following is copied from Unity procedural sky shader
	float3 cameraPosition = float3(0.0, innerRadius + cameraHeight, 0.0);
	float far = sqrt(outerRadius2 + innerRadius2 * eyeRay.y * eyeRay.y - innerRadius2) - innerRadius * eyeRay.y;
	float startDepth = exp(scaleFactorOverDepth * (-cameraHeight));
	float startAngle = dot(eyeRay, cameraPosition) / (innerRadius + cameraHeight);
	float startOffset = startDepth * GetSkyScale(startAngle);
	float sampleLength = far / _WeatherMakerSkySamples;
	float scaledLength = sampleLength * scaleFactor;
	float3 sampleRay = eyeRay * sampleLength;
	float3 samplePoint = cameraPosition + sampleRay * 0.5;
	float3 color = float3(0.0, 0.0, 0.0);

	// Loop through the sample rays
	for (int i = 0; i < int(_WeatherMakerSkySamples); i++)
	{
		float height = length(samplePoint);
		float invHeight = 1.0 / height;
		float depth = exp(scaleFactorOverDepth * (innerRadius - height));
		float scaleAtten = depth * scaledLength;
		float eyeAngle = dot(eyeRay, samplePoint) * invHeight;
		float lightAngle = dot(lightDir, samplePoint) * invHeight;
		float lightScatter = startOffset + depth * (GetSkyScale(lightAngle) - GetSkyScale(eyeAngle));
		float3 lightAtten = exp(-lightScatter * (_WeatherMakerSkyLightPIScattering.xyz + _WeatherMakerSkyLightPIScattering.w));
		color += (lightAtten * scaleAtten);
		samplePoint += sampleRay;
	}

	o.inScatter = lightColor * color * _WeatherMakerSkyLightScattering.xyz;
	o.outScatter = lightColor * color * _WeatherMakerSkyLightScattering.w;

	return o;
}

procedural_sky_info CalculateScatteringColor(float3 lightDir, fixed3 lightColor, fixed sunSize, float3 eyeRay, fixed3 inScatter, fixed3 outScatter)
{
	float eyeCos = dot(lightDir, eyeRay);
	float eyeCos2 = eyeCos * eyeCos;
	procedural_sky_info o;
	o.inScatter = inScatter;
	o.outScatter = outScatter;

	o.skyColor.rgb = GetRayleighPhase(eyeCos2) * inScatter;
	o.skyColor.rgb += (outScatter * GetSkyMiePhase(eyeCos, eyeCos2));
	
	// draws the sun disc, not used for now
	//o.skyColor.rgb += GetMiePhase(sunSize, eyeCos, eyeCos2, 1.18) * outScatter;
	o.skyColor.a = min(1.0, 128.0 * max(o.skyColor.r, max(o.skyColor.g, o.skyColor.b)));

#if defined(UNITY_COLORSPACE_GAMMA) && SKYBOX_COLOR_IN_TARGET_COLOR_SPACE

	o.skyColor.rgb = sqrt(o.skyColor.rgb);

#endif

	return o;
}

fixed3 Uncharted2Tonemap(fixed3 x)
{
	const float A = 0.15;
	const float B = 0.50;
	const float C = 0.10;
	const float D = 0.20;
	const float E = 0.02;
	const float F = 0.30;
	return ( ( x * ( A * x + C * B ) + D * E ) / ( x * ( A * x + B ) + D * F ) ) - E / F;
}

fixed4 CalculateSkyColorPreetham(float3 ray, float3 dirToSun)
{
	static const float whiteScale = 1.0748724675633854;
	static const float rayleighZenithLength = 8.4E3;
	static const float mieZenithLength = 1.25E3;
	static const float luminance = 1.0;
	float sunfade = _WeatherMakerSkyTotalRayleigh.w;
	float sunDot = _WeatherMakerSkyMieG.w;
	float sunE = _WeatherMakerSkyTotalMie.w;

	// extinction (absorbtion + out scattering)
	// rayleigh coefficients
	float3 vBetaR = _WeatherMakerSkyTotalRayleigh.xyz;

	// mie coefficients
	float3 vBetaM = _WeatherMakerSkyTotalMie.xyz;

	// optical length
	float zenithAngle = acos(max(0.0, dot(upVector, ray)));
	float inverse = 1.0 / (cos(zenithAngle) + 0.15 * pow(93.885 - ((zenithAngle * 180.0) / PI), -1.253));
	float sR = rayleighZenithLength * inverse;
	float sM = mieZenithLength * inverse;

	// combined extinction factor
	float3 fex = exp(-(vBetaR * sR + vBetaM * sM));

	// in scattering
	float cosTheta = dot(ray, dirToSun);

	float rPhase = GetRayleighPhasePreetham(cosTheta * 0.5 + 0.5);
	rPhase = max(min(1.0, 1.0 - dirToSun.y - 0.6), rPhase);
	float3 betaRTheta = vBetaR * rPhase;

	float mPhase = GetHgPhasePreetham(cosTheta);
	float3 betaMTheta = vBetaM * mPhase;

	float3 Lin = float3
	(
		pow(sunE * ((betaRTheta.r + betaMTheta.r) / (vBetaR.r + vBetaM.r)) * (1.0 - fex.x), 1.5),
		pow(sunE * ((betaRTheta.g + betaMTheta.g) / (vBetaR.g + vBetaM.g)) * (1.0 - fex.y), 1.5),
		pow(sunE * ((betaRTheta.b + betaMTheta.b) / (vBetaR.b + vBetaM.b)) * (1.0 - fex.z), 1.5)
	);
	Lin *= float3
	(
		lerp(1.0, pow(sunE * ((betaRTheta.r + betaMTheta.r) / (vBetaR.r + vBetaM.r)) * fex.x, 0.5), sunDot),
		lerp(1.0, pow(sunE * ((betaRTheta.g + betaMTheta.g) / (vBetaR.g + vBetaM.g)) * fex.y, 0.5), sunDot),
		lerp(1.0, pow(sunE * ((betaRTheta.b + betaMTheta.b) / (vBetaR.b + vBetaM.b)) * fex.z, 0.5), sunDot)
	);

	fixed4 color = fixed4((max(0.0, Lin) * 0.04) + float3(0.0, 0.0003, 0.00075), 1.0);
	color.rgb = Uncharted2Tonemap((log2(2.0 / pow(luminance, 4.0))) * color.rgb);
	color.rgb *= whiteScale;
	color.a = min(1.0, 128.0 * max(color.r, max(color.g, color.b)));

#if defined(UNITY_COLORSPACE_GAMMA) && SKYBOX_COLOR_IN_TARGET_COLOR_SPACE

	color.rgb = sqrt(color.rgb);

#endif

	return color;
}

fixed3 GetNightColor(float3 ray, float2 uv, float light)
{
	fixed3 nightColor = tex2Dlod(_NightTex, float4(uv, 0.0, 0.0)).rgb * _NightIntensity;
	nightColor *= (nightColor >= _NightVisibilityThreshold);
	fixed maxValue = max(nightColor.r, max(nightColor.g, nightColor.b));

#if defined(ENABLE_NIGHT_TWINKLE)

	fixed twinkleRandom = _NightTwinkleRandomness * RandomFloat(ray * _WeatherMakerTime.y);
	fixed twinkle = (maxValue > _NightTwinkleMinimum) * (twinkleRandom + (_NightTwinkleVariance * sin(_NightTwinkleSpeed * _WeatherMakerTime.y * maxValue)));
	nightColor *= (1.0 + twinkle);

#endif

	return nightColor * _NightIntensity * _NightSkyMultiplier * _WeatherMakerNightMultiplier * _WeatherMakerNightMultiplier * _WeatherMakerNightMultiplier * (1.0 - light);
}
