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

#ifndef __WEATHER_MAKER_SHADER__
#define __WEATHER_MAKER_SHADER__

#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "AutoLight.cginc"
#include "UnityDeferredLibrary.cginc"
#include "HLSLSupport.cginc"
#include "WeatherMakerNoiseShaderInclude.cginc"
#include "WeatherMakerShadows.cginc"

#if defined(SHADER_API_D3D9)
#define MAX_LIGHT_COUNT 4
#define MAX_MOON_COUNT 2
#else
#define MAX_LIGHT_COUNT 16
#define MAX_MOON_COUNT 8
#endif
#if defined(UNITY_COLORSPACE_GAMMA)
#define GAMMA 2
#define COLOR_2_GAMMA(color) color
#define COLOR_2_LINEAR(color) color * color
#define LINEAR_2_OUTPUT(color) sqrt(color)
#else
#define GAMMA 2.2
#define COLOR_2_GAMMA(color) ((unity_ColorSpaceDouble.r>2.0) ? pow(color,1.0/GAMMA) : color)
#define COLOR_2_LINEAR(color) color
#define LINEAR_2_LINEAR(color) color
#endif

#define float4Zero float4(0.0, 0.0, 0.0, 0.0)
#define float4One float4(1.0, 1.0, 1.0, 1.0)
#define fixed4Zero fixed4(0.0, 0.0, 0.0, 0.0)
#define fixed4One fixed4(1.0, 1.0, 1.0, 1.0)
#define float3Zero float3(0.0, 0.0, 0.0)
#define float3One float3(1.0, 1.0, 1.0)
#define fixed3Zero fixed3(0.0, 0.0, 0.0)
#define fixed3One fixed3(1.0, 1.0, 1.0)
#define ditherMagic fixed3(12.9898, 78.233, 43758.5453)
#define upVector float3(0.0, 1.0, 0.0)

#ifndef PI
#define PI 3.14159263
#endif

#define WM_BASE_VERTEX_INPUT UNITY_VERTEX_INPUT_INSTANCE_ID
#define WM_BASE_VERTEX_TO_FRAG UNITY_VERTEX_INPUT_INSTANCE_ID UNITY_VERTEX_OUTPUT_STEREO

#if defined(UNITY_STEREO_INSTANCING_ENABLED) || defined(UNITY_STEREO_MULTIVIEW_ENABLED)
#define WM_INSTANCE_VERT(v, type, o) type o; UNITY_SETUP_INSTANCE_ID(v); UNITY_TRANSFER_INSTANCE_ID(v, o); UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o); int eyeIndex = o.stereoTargetEyeIndex;
#else
#define WM_INSTANCE_VERT(v, type, o) type o; int eyeIndex = unity_StereoEyeIndex;
#endif

#define WM_INSTANCE_FRAG(i) UNITY_SETUP_INSTANCE_ID(i); UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

// vertex...
struct vertex_only_input_data
{
	float4 vertex : POSITION;
	WM_BASE_VERTEX_INPUT
};

struct vertex_uv_normal
{
	float4 vertex : POSITION;
	float2 uv : TEXCOORD0;
	float3 normal : NORMAL;
	WM_BASE_VERTEX_INPUT
};

struct full_screen_vertex
{
	float4 vertex : POSITION;
	float2 uv : TEXCOORD0;
	WM_BASE_VERTEX_INPUT
};

// fragment...
struct volumetric_data
{
	float4 vertex : POSITION;
	float3 normal : NORMAL;
	float4 projPos : TEXCOORD0;
	float3 rayDir : TEXCOORD1;
	float3 viewPos : TEXCOORD2;
	float3 worldPos : TEXCOORD3;
	WM_BASE_VERTEX_TO_FRAG
};

struct full_screen_fragment
{
	float4 vertex : SV_POSITION;
	float2 uv : TEXCOORD0;
	float3 forwardLine : NORMAL;
	WM_BASE_VERTEX_TO_FRAG
};

struct deferred_fragment
{
	float4 gBuffer0 : SV_Target0;
	float4 gBuffer1 : SV_Target1;
	float4 gBuffer2 : SV_Target2;
	float4 gBuffer3 : SV_Target3;
};

struct frag_out_with_depth
{
	fixed4 color : COLOR;
	float depth : DEPTH;
};

struct v2fShadow
{
	float4 vertex : POSITION;
	float3 worldPos : TEXCOORD0;
};

// globals
#if defined(WM_IS_FULL_SCREEN_EFFECT) && (defined(UNITY_STEREO_INSTANCING_ENABLED) || defined(UNITY_STEREO_MULTIVIEW_ENABLED))

uniform UNITY_DECLARE_SCREENSPACE_TEXTURE(_MainTex);
uniform UNITY_DECLARE_SCREENSPACE_TEXTURE(_MainTex2);
uniform UNITY_DECLARE_SCREENSPACE_TEXTURE(_CameraDepthNormalsTexture);

#else

uniform sampler2D _MainTex;
uniform sampler2D _MainTex2;
uniform sampler2D _CameraDepthNormalsTexture;

#endif

uniform float4 _MainTex_ST;
uniform float4 _MainTex_TexelSize;
uniform float4 _MainTex2_ST;
uniform float4 _MainTex2_TexelSize;
uniform float4 _CameraDepthTexture_ST;
uniform float4 _CameraDepthTexture_TexelSize;
uniform float4 _CameraDepthNormalsTexture_ST;
uniform float4 _CameraDepthNormalsTexture_TexelSize;

uniform float4 _WeatherMakerTime;
uniform float4 _WeatherMakerTimeSin;
//uniform sampler2D _WeatherMakerDitherTexture;
//uniform float4 _WeatherMakerDitherTexture_ST;
//uniform float4 _WeatherMakerDitherTexture_TexelSize;

// time of day multipliers, all add up to 1
uniform fixed _WeatherMakerDayMultiplier;
uniform fixed _WeatherMakerDawnDuskMultiplier;
uniform fixed _WeatherMakerNightMultiplier;

// ambient light
uniform fixed3 _WeatherMakerAmbientLightColor;
uniform fixed3 _WeatherMakerAmbientLightColorSky;
uniform fixed3 _WeatherMakerAmbientLightColorGround;
uniform fixed3 _WeatherMakerAmbientLightColorEquator;

// dir lights
uniform int _WeatherMakerDirLightCount;
uniform float4 _WeatherMakerDirLightPosition[MAX_LIGHT_COUNT];
uniform fixed4 _WeatherMakerDirLightColor[MAX_LIGHT_COUNT];
uniform float4 _WeatherMakerDirLightAtten[MAX_LIGHT_COUNT];
uniform float4 _WeatherMakerDirLightViewportPosition[MAX_LIGHT_COUNT];

// point lights
uniform int _WeatherMakerPointLightCount;
uniform float4 _WeatherMakerPointLightPosition[MAX_LIGHT_COUNT];
uniform fixed4 _WeatherMakerPointLightColor[MAX_LIGHT_COUNT];
uniform float4 _WeatherMakerPointLightAtten[MAX_LIGHT_COUNT];

// spot lights
uniform int _WeatherMakerSpotLightCount;
uniform float4 _WeatherMakerSpotLightPosition[MAX_LIGHT_COUNT];
uniform fixed4 _WeatherMakerSpotLightColor[MAX_LIGHT_COUNT];
uniform float4 _WeatherMakerSpotLightAtten[MAX_LIGHT_COUNT];
uniform float4 _WeatherMakerSpotLightSpotDirection[MAX_LIGHT_COUNT];
uniform float4 _WeatherMakerSpotLightSpotEnd[MAX_LIGHT_COUNT];

uniform fixed4 _WeatherMakerFogLightFalloff = fixed4(1.2, 0.0, 0.0, 0.0); // spot light radius light falloff, 0, 0, 0
uniform fixed _WeatherMakerFogLightSunIntensityReducer = 0.8;
uniform fixed _WeatherMakerFogDirectionalLightScatterIntensity = 5.0;

uniform float3 _WeatherMakerSunDirectionUp; // direction to sun
uniform float3 _WeatherMakerSunDirectionUp2D; // direction to sun
uniform float3 _WeatherMakerSunDirectionDown; // direction sun is facing
uniform float3 _WeatherMakerSunDirectionDown2D; // direction sun is facing
uniform fixed4 _WeatherMakerSunColor; // sun light color
uniform fixed4 _WeatherMakerSunTintColor; // sun tint color
uniform float3 _WeatherMakerSunPositionNormalized; // sun position in world space, normalized
uniform float3 _WeatherMakerSunPositionWorldSpace; // sun position in world space
uniform float4 _WeatherMakerSunLightPower; // power, multiplier, shadow strength, 1.0 - shadow strength
uniform float4 _WeatherMakerSunVar1; // scale, sun intensity ^ 0.5, sun intensity ^ 0.75, sun intensity ^ 2

uniform int _WeatherMakerMoonCount; // moon count
uniform float3 _WeatherMakerMoonDirectionUp[MAX_MOON_COUNT]; // direction to moon
uniform float3 _WeatherMakerMoonDirectionDown[MAX_MOON_COUNT]; // direction moon is facing
uniform fixed4 _WeatherMakerMoonLightColor[MAX_MOON_COUNT]; // moon light color
uniform float4 _WeatherMakerMoonLightPower[MAX_MOON_COUNT]; // power, multiplier, shadow strength, 1.0 - shadow strength
uniform fixed4 _WeatherMakerMoonTintColor[MAX_MOON_COUNT]; // moon tint color
uniform float4 _WeatherMakerMoonVar1[MAX_MOON_COUNT]; // scale, 0, 0, 0

uniform float _WeatherMakerSkySphereRadius;
uniform float _WeatherMakerSkySphereRadiusSquared;
uniform fixed4 _WeatherMakerSkyGradientColor = fixed4(1.0, 1.0, 1.0, 1.0);
uniform fixed _WeatherMakerSkyDitherLevel;

// contains un-normalized direction to frustum corners for left and right eye : bottom left, top left, bottom right, top right, use unity_StereoEyeIndex * 4 to index
uniform float3 _WeatherMakerCameraFrustumRays[8];

// inverse view matrix for each eye, use unity_StereoEyeIndex to idnex
uniform float4x4 _WeatherMakerInverseView[2];

uniform fixed4 _TintColor;
uniform fixed3 _EmissiveColor;
uniform fixed _Intensity;
uniform fixed _DirectionalLightMultiplier;
uniform fixed _PointSpotLightMultiplier;
uniform fixed _AmbientLightMultiplier;

#if defined(SOFTPARTICLES_ON)

float _InvFade;

#endif

#define WM_SAMPLE_DEPTH(uv) UNITY_SAMPLE_DEPTH(SAMPLE_DEPTH_TEXTURE_LOD(_CameraDepthTexture, float4((uv).xy, 0.0, 0.0)))
#define WM_SAMPLE_DEPTH_PROJ(uv) UNITY_SAMPLE_DEPTH(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(uv)))

#if defined(WM_IS_FULL_SCREEN_EFFECT) && (defined(UNITY_STEREO_INSTANCING_ENABLED) || defined(UNITY_STEREO_MULTIVIEW_ENABLED))

#define WM_SAMPLE_FULL_SCREEN_TEXTURE(tex, uv) UNITY_SAMPLE_TEX2DARRAY_LOD(tex, float3((uv).xy, (float)unity_StereoEyeIndex), 0.0)

#else

#define WM_SAMPLE_FULL_SCREEN_TEXTURE(tex, uv) tex2Dlod(tex, float4((uv).xy, 0.0, 0.0))

#endif

inline float GetDepth01(float2 uv)
{
	return Linear01Depth(WM_SAMPLE_DEPTH(uv));
}

inline void GetDepthAndNormal(float2 uv, out float depth, out float3 normal)
{
	float4 samp = WM_SAMPLE_FULL_SCREEN_TEXTURE(_CameraDepthNormalsTexture, uv);
	DecodeDepthNormal(samp, depth, normal);
}

inline fixed LerpFade(float4 lifeTime, float timeSinceLevelLoad)
{
	// the vertex will fade in, stay at full color, then fade out
	// x = creation time seconds
	// y = fade time in seconds
	// z = life time seconds

	// debug
	// return 1;

	float peakFadeIn = lifeTime.x + lifeTime.y;
	float startFadeOut = lifeTime.x + lifeTime.z - lifeTime.y;
	float endTime = lifeTime.x + lifeTime.z;
	float lerpMultiplier = saturate(ceil(timeSinceLevelLoad - peakFadeIn));
	float lerp1Scalar = saturate(((timeSinceLevelLoad - lifeTime.x + 0.000001) / max(0.000001, (peakFadeIn - lifeTime.x))));
	float lerp2Scalar = saturate(max(0, ((timeSinceLevelLoad - startFadeOut) / max(0.000001, (endTime - startFadeOut)))));
	float lerp1 = lerp1Scalar * (1.0 - lerpMultiplier);
	float lerp2 = (1.0 - lerp2Scalar) * lerpMultiplier;
	return lerp1 + lerp2;
}

inline float3 WorldSpaceVertexPosNear(float3 vertex)
{
	return mul(unity_ObjectToWorld, float4(vertex.xyz, 0.0)).xyz;
}

inline float3 WorldSpaceVertexPosFar(float3 vertex)
{
	return mul(unity_ObjectToWorld, float4(vertex.xyz, 1.0)).xyz;
}

inline float3 WorldSpaceVertexPos(float4 vertex)
{
	return mul(unity_ObjectToWorld, vertex).xyz;
}

inline deferred_fragment ColorToDeferredFragment(float4 color)
{
	deferred_fragment f;
	f.gBuffer0 = float4(0.0, 0.0, 0.0, 0.0);
	f.gBuffer1 = float4(0.0, 0.0, 0.0, 0.0);
	f.gBuffer2 = float4(0.0, 0.0, 0.0, 1.0);

#if defined(UNITY_HDR_ON)

	f.gBuffer3 = color;

#else

	// TODO: Something is wrong with alpha channel in deferred non-HDR
	f.gBuffer3 = float4(exp2(-color.rgb), color.a);

#endif

	return f;
}

fixed3 ApplyDirLightWorldSpace(float3 worldViewDir, float4 lightDir, float3 normal, fixed4 lightColor, out float atten)
{
#if defined(ORTHOGRAPHIC_MODE)

	atten = pow(max(0.0, dot(lightDir.xyz, float3(0.0, 0.0, -1.0))), 0.5);

#else

	if (normal.x != 0.0 || normal.y != 0.0 || normal.z != 0.0)
	{
		atten = max(0.0, dot(lightDir, normal));

		// average in a little bit from the direct line of site
		//atten = ((atten * 0.7) + (max(0.0, dot(worldViewDir, -normal)) * 0.3)) * 0.5;
		atten *= lightColor.a * lightColor.a;
	}
	else
	{
		atten = lightColor.a * lightColor.a;
	}
	fixed horizonFade = min(1.0, (lightDir.y + 0.1) * 3.0);
	atten *= horizonFade;

#endif

	return lightColor.rgb * atten * _DirectionalLightMultiplier;
}

fixed3 ApplyPointLightWorldSpace(float4 lightPos, float3 normal, fixed4 lightColor, float4 lightAtten, float3 worldPos, out fixed atten)
{
	float3 toLight = (lightPos.xyz - worldPos);

#if defined(ORTHOGRAPHIC_MODE)

	// ignore view normal and point straight out along z axis
	normal = fixed3(0, 0, -1);
	atten = dot(normal, normalize(toLight));
	return lightColor.rgb * lightColor.a * atten * _PointSpotLightMultiplier;

#else

	float lengthSq = max(0.000001, dot(toLight, toLight));
	atten = (1.0 / (1.0 + (lengthSq * lightAtten.z)));
	float attenFalloff = 1.0 - pow(min(1.0, (lengthSq * lightAtten.w)), 2.0);
	atten *= attenFalloff;
	if (normal.x != 0.0 || normal.y != 0.0 || normal.z != 0.0)
	{
		toLight *= rsqrt(lengthSq);
		atten *= max(0.0, dot(normal, toLight));
	}
	return lightColor.rgb * lightColor.a * atten * _PointSpotLightMultiplier;

#endif

}

fixed3 ApplySpotLightWorldSpace(float4 lightPos, float4 lightDir, float3 normal, fixed4 lightColor, float4 lightAtten, float3 worldPos, out fixed atten)
{
	float3 toLight = (lightPos.xyz - worldPos);

#if defined(ORTHOGRAPHIC_MODE)

	// ignore view normal and point straight out along z axis
	normal = fixed3(0, 0, -1);
	atten = dot(normal, normalize(toLight));
	return lightColor.rgb * lightColor.a * atten * _PointSpotLightMultiplier;

#else

	float lengthSq = max(0.000001, dot(toLight, toLight));
	atten = (1.0 / (1.0 + (lengthSq * lightAtten.z)));
	toLight *= rsqrt(lengthSq);
	if (normal.x != 0.0 || normal.y != 0.0 || normal.z != 0.0)
	{
		atten *= max(0.0, dot(normal, toLight));
	}
	float rho = max(0.0, dot(toLight, -lightDir.xyz));
	atten *= saturate(16.0 * ((rho - lightAtten.x) * lightAtten.y));
	lightColor.rgb *= lightColor.a * atten * _PointSpotLightMultiplier;
	return lightColor;

#endif

}

inline fixed3 CalculateSpecularLight(float3 worldViewDir, float3 normal, float3 lightDir, fixed4 lightColor, float specularPower, fixed4 specularColor, float shadowPower)
{
	fixed3 sharedSpecularColor = specularColor.a * lightColor.a * shadowPower * (lightColor.rgb * specularColor.rgb);

	// calculate direct light coming off surface and into eye without reversing direction
	half3 h = normalize(lightDir - worldViewDir);
	fixed diff, nh, indirectReflectPower;
	fixed3 specularDirect, specularIndirect;

	//if (normal.x != 0.0 || normal.y != 0.0 || normal.z != 0.0)
	{
		diff = max(0, dot(normal, -lightDir));
		nh = max(0, dot(normal, h));

		// calculate direct specular based on dot products
		specularDirect = pow(nh, specularPower) * sharedSpecularColor;

		// calculate indirect color based on reflection of light off surface in opposite direction to eye
		indirectReflectPower = max(0.0, (dot(worldViewDir, -normal) * max(0.0, dot(normal, lightDir))));
		specularIndirect = sharedSpecularColor * indirectReflectPower;
	}
	/*else
	{
	diff = 1.0;
	nh = 1.0;
	indirectReflectPower = 1.0;
	specularIndirect = fixed3Zero;
	specularDirect = sharedSpecularColor * specularPower;// *max(0.0, (dot(worldViewDir, -UNITY_MATRIX_IT_MV[2].xyz)));
	}*/

	// return the sum of light
	return (specularDirect + specularIndirect);
}

inline fixed CalculateDirLightShadowPower(float3 worldPos, int doShadow, int dirIndex)
{
	if (dirIndex == 0 && _WeatherMakerSunColor.a > 0.0 && doShadow > 0 && _WeatherMakerSunLightPower.z > 0.0)
	{
		float4 cascadeWeights = GET_CASCADE_WEIGHTS(worldPos, 0.0);
		float4 samplePos = GET_SHADOW_COORDINATES(float4(worldPos, 1.0), cascadeWeights);
		fixed shadowPower = (doShadow == 1 ? UNITY_SAMPLE_SHADOW(_WeatherMakerSunShadowMapTexture, samplePos) : UNITY_SAMPLE_SHADOW_4(_WeatherMakerSunShadowMapTexture, samplePos));
		return _WeatherMakerSunLightPower.w + (shadowPower * _WeatherMakerSunLightPower.z);
	}
	else
	{
		return 1.0;
	}
}

// doShadow: 0 = none, 1 = 1 tap, 2 = 4 tap
inline fixed3 CalculateLightColorWorldSpaceSpecular(float3 worldPos, float3 normal, int doDirLight, int doShadow, float3 worldViewDir, fixed specularPower, fixed4 specularColor)
{
	fixed3 vertexColor = fixed3(0.0, 0.0, 0.0);
	int dirLightCount = _WeatherMakerDirLightCount * doDirLight;
	UNITY_UNROLL
		for (int dirIndex = 0; dirIndex < dirLightCount; dirIndex++)
		{
			fixed dirAtten;
			fixed3 dirLight = ApplyDirLightWorldSpace(worldViewDir, _WeatherMakerDirLightPosition[dirIndex], normal, _WeatherMakerDirLightColor[dirIndex], dirAtten);
			fixed shadowPower = CalculateDirLightShadowPower(worldPos, doShadow, dirIndex);
			dirLight *= shadowPower;
			if (specularPower > 0.0)
			{
				fixed4 sunColorForSpecular = fixed4(_WeatherMakerSunColor.rgb, _WeatherMakerSunColor.a * dirAtten * shadowPower * shadowPower);
				vertexColor += (CalculateSpecularLight(worldViewDir, normal, _WeatherMakerSunDirectionUp, sunColorForSpecular, specularPower, specularColor, shadowPower * _WeatherMakerSunLightPower.z) * saturate(_WeatherMakerDirLightPosition[dirIndex].y * 2.0));
			}
			vertexColor += dirLight;
		}
	UNITY_LOOP
		for (int pointIndex = 0; pointIndex < _WeatherMakerPointLightCount; pointIndex++)
		{
			fixed pointAtten;
			vertexColor += ApplyPointLightWorldSpace(_WeatherMakerPointLightPosition[pointIndex], normal, _WeatherMakerPointLightColor[pointIndex], _WeatherMakerPointLightAtten[pointIndex], worldPos, pointAtten);
			if (specularPower > 0.0 && pointAtten > 0.0)
			{
				fixed4 pointColorForSpecular = fixed4(_WeatherMakerPointLightColor[pointIndex].rgb, _WeatherMakerPointLightColor[pointIndex].a * pointAtten);
				vertexColor += CalculateSpecularLight(worldViewDir, normal, normalize(_WeatherMakerPointLightPosition[pointIndex].xyz - worldPos), pointColorForSpecular, specularPower, specularColor, 1.0);
			}
		}
	UNITY_LOOP
		for (int spotIndex = 0; spotIndex < _WeatherMakerSpotLightCount; spotIndex++)
		{
			fixed spotAtten;
			vertexColor += ApplySpotLightWorldSpace(_WeatherMakerSpotLightPosition[spotIndex], _WeatherMakerSpotLightSpotDirection[spotIndex], normal, _WeatherMakerSpotLightColor[spotIndex], _WeatherMakerSpotLightAtten[spotIndex], worldPos, spotAtten);
			if (specularPower > 0.0 && spotAtten > 0.0)
			{
				fixed4 spotColorForSpecular = fixed4(_WeatherMakerSpotLightColor[spotIndex].rgb, _WeatherMakerSpotLightColor[spotIndex].a * spotAtten);
				vertexColor += CalculateSpecularLight(worldViewDir, normal, normalize(_WeatherMakerSpotLightPosition[spotIndex].xyz - worldPos), spotColorForSpecular, specularPower, specularColor, 1.0);
			}
		}
	return vertexColor;
}

inline fixed3 CalculateLightColorWorldSpace(float3 worldPos, float3 normal, int doDirLight, int doShadow)
{
	return CalculateLightColorWorldSpaceSpecular(worldPos, normal, doDirLight, doShadow, float3Zero, 0.0, fixed4Zero);
}

inline float3 RotateVertexLocalQuaternion(float3 position, float3 axis, float angle)
{
	float half_angle = angle * 0.5;
	float _sin, _cos;
	sincos(half_angle, _sin, _cos);
	float4 q = float4(axis.xyz * _sin, _cos);
	return position + (2.0 * cross(cross(position, q.xyz) + (q.w * position), q.xyz));
}

inline volumetric_data GetVolumetricData(appdata_base v)
{
	WM_INSTANCE_VERT(v, volumetric_data, o);
	o.vertex = UnityObjectToClipPos(v.vertex);
	o.normal = UnityObjectToWorldNormal(v.normal);
	o.projPos = ComputeScreenPos(o.vertex);
	o.worldPos = WorldSpaceVertexPos(v.vertex);
	o.rayDir = (o.worldPos - _WorldSpaceCameraPos);
	o.viewPos = UnityObjectToViewPos(v.vertex);
	return o;
}

float3 GetFarPlaneVectorFullScreen(float2 uv, int eyeIndex)
{
	uv.x = (uv.x > 0.5);
	uv.y = (uv.y > 0.5);
	return _WeatherMakerCameraFrustumRays[(eyeIndex * 4) + ((uv.x * 2) + uv.y)];
}

inline float RayBoxIntersect(float3 rayOrigin, float3 rayDir, float rayLength, float3 boxMin, float3 boxMax, out float intersectAmount, out float distanceToBox)
{
	// https://tavianator.com/fast-branchless-raybounding-box-intersections/

	/*
	Aos::Vector3 t1(Aos::mulPerElem(m_min - ray.m_pos, ray.m_invDir));
	Aos::Vector3 t2(Aos::mulPerElem(m_max - ray.m_pos, ray.m_invDir));

	Aos::Vector3 tmin1(Aos::minPerElem(t1, t2));
	Aos::Vector3 tmax1(Aos::maxPerElem(t1, t2));

	float tmin = Aos::maxElem(tmin1);
	float tmax = Aos::minElem(tmax1);

	return tmax >= std::max(ray.m_min, tmin) && tmin < ray.m_max;
	*/

	float3 invRayDir = 1.0 / rayDir;
	float3 t1 = (boxMin - rayOrigin) * invRayDir;
	float3 t2 = (boxMax - rayOrigin) * invRayDir;
	float3 tmin1 = min(t1, t2);
	float3 tmax1 = max(t1, t2);
	float tmin = max(max(tmin1.x, tmin1.y), tmin1.z);
	float tmax = min(min(tmax1.x, tmax1.y), tmax1.z);
	float2 tt0 = max(tmin1.xx, tmin1.yz);
	distanceToBox = max(0.0, max(tt0.x, tt0.y));
	tt0 = min(tmax1.xx, tmax1.yz);
	float tt1 = min(tt0.x, tt0.y);
	tt1 = min(tt1, rayLength);
	intersectAmount = max(0.0, tt1 - distanceToBox);

	return intersectAmount > 0.0001;
}

// spherePosition is x,y,z,radius squared
inline float RaySphereIntersect(float3 rayOrigin, float3 rayDir, float rayLength, float4 spherePosition, out float intersectAmount, out float distanceToSphere)
{
	// optimized version, seems to work as well, but watch out for artifacts
	// https://gamedev.stackexchange.com/questions/96459/fast-ray-sphere-collision-code
	float3 sphereCenter = rayOrigin - spherePosition.xyz;
	float b = dot(rayDir, sphereCenter);
	float c = dot(sphereCenter, sphereCenter) - spherePosition.w;
	float discr = (b * b) - c;
	float t = sqrt(discr * (discr > 0.0));
	b = -b;
	distanceToSphere = clamp(b - t, 0.0, rayLength);
	intersectAmount = clamp(b + t, 0.0, rayLength);
	intersectAmount = intersectAmount - distanceToSphere;

	/* // older version which is known to work in all cases
	// http://www.cosinekitty.com/raytrace/chapter06_sphere.html
	float3 sphereCenter = rayOrigin - spherePosition.xyz;
	float fA = dot(rayDir, rayDir);
	float fB = 2.0 * dot(rayDir, sphereCenter);
	float fC = dot(sphereCenter, sphereCenter) - spherePosition.w;
	float fD = (fB * fB) - (4.0 * fA * fC);
	float recpTwoA = (fD > 0.0) * (0.5 / fA);
	float DSqrt = sqrt(fD);
	fB = -fB;

	// the distance to the front of sphere - will be 0 if in sphere or miss
	distanceToSphere = clamp((fB - DSqrt) * recpTwoA, 0.0, rayLength);

	// total distance to the back of the sphere, will be 0 if miss
	intersectAmount = clamp((fB + DSqrt) * recpTwoA, 0.0, rayLength);

	// get intersect amount - we know that distance to back of sphere is greater than distance to front of sphere at this point
	intersectAmount = intersectAmount - distanceToSphere;
	*/

	return intersectAmount > 0.0001;
}

// return 0 if no intersect
inline float RayPlaneIntersect(float3 rayOrigin, float3 rayDir, float3 planeNormal, float3 planePos, out float distanceToPlane)
{
	float denom = dot(planeNormal, rayDir);
	distanceToPlane = dot(planePos - rayOrigin, planeNormal) / denom;
	return (denom > 0.0001 && distanceToPlane > 0.0001);
}

// distanceToPlane becomes distance to disc center
// intersectsDisc is true if the plane intersect is within the disc
inline void RayPlaneDiscIntersect(float3 rayOrigin, float3 rayDir, float3 planeCenter, float discRadiusSquared, inout float distanceToPlane, out float intersectsDisc)
{
	float3 planeIntersect = (rayOrigin + (rayDir * distanceToPlane));
	float3 planeIntersectLocal = planeIntersect - planeCenter;
	float distanceFromDiscCenter = dot(planeIntersectLocal, planeIntersectLocal);
	distanceToPlane = distance(planeIntersect, rayOrigin);
	intersectsDisc = (distanceFromDiscCenter < discRadiusSquared);
}

inline float LineLineClosestDistanceSquared(float3 line1Point1, float3 line1Point2, float3 line2Point1, float3 line2Point2, out float3 closePoint1, out float3 closePoint2)
{
	float3 u = line1Point2 - line1Point1;
	float3 v = line2Point2 - line2Point1;
	float3 w = line1Point1 - line2Point1;
	float a = dot(u, u);         // always >= 0
	float b = dot(u, v);
	float c = dot(v, v);         // always >= 0
	float d = dot(u, w);
	float e = dot(v, w);
	float D = a * c - b * b;        // always >= 0
	float sc, tc;

	// compute the line parameters of the two closest points
	if (D < 0.0001)
	{
		// the lines are almost parallel
		sc = 0.0;
		tc = (b > c ? d / b : e / c);    // use the largest denominator
	}
	else
	{
		sc = (b * e - c * d) / D;
		tc = (a * e - b * d) / D;
	}

	// get the difference of the two closest points
	closePoint1 = w + (sc * u);
	closePoint2 = (tc * v);
	float3 dP = closePoint1 - closePoint2; // =  L1(sc) - L2(tc)

										   // return norm(dP);   // return the closest distance
	return dot(dP, dP);
}

// coneposition is xyz, range
// conedir is xyz, end radius squared
// coneatten is cos(angle * 0.5), 1.0 / cos(angle * 0.25), atten, 1.0 / range squared
// coneend is base center xyz, slant squared
inline float RayConeIntersect(float3 rayOrigin, float3 rayDir, float rayLength, float4 conePosition, float4 coneDir, float4 coneEnd, float4 coneAtten, out float intersectAmount, out float distanceToCone)
{
	float2 t;

	// https://www.geometrictools.com/GTEngine/Include/Mathematics/GteIntrLine3Cone3.h
	// https://www.shadertoy.com/view/4s23DR
	// https://github.com/mayank127/raytracer/blob/master/object.cpp
	float3 PmV = rayOrigin - conePosition.xyz;
	float DdU = dot(coneDir.xyz, rayDir);
	float DdPmV = dot(coneDir.xyz, PmV);
	float UdPmV = dot(rayDir, PmV);
	float PmVdPmV = dot(PmV, PmV);
	float halfCosAngle = coneAtten.x * coneAtten.x;
	float c2 = DdU * DdU - halfCosAngle;
	float c1 = DdU * DdPmV - halfCosAngle * UdPmV;
	float c0 = DdPmV * DdPmV - halfCosAngle * PmVdPmV;
	float discr = c1 * c1 - c0 * c2;
	discr *= (discr > 0.0);
	float root = sqrt(discr);
	float invC2 = (1.0 / c2);
	c1 = -c1;
	t.y = (c1 - root) * invC2;
	t.x = (c1 + root) * invC2;

	// zero out negative cone
	t *= (DdPmV + (DdU * t) > 0.0);

	if (t.x == 0.0 && t.y == 0.0)
	{
		intersectAmount = 0.0;
		distanceToCone = 0.0;
		return false;
	}
	else
	{
		// intersect cone base (disc) and subsitute where appropriate
		float distanceToPlane1, distanceToPlane2;
		float hasCap1, hasCap2;

		// case 1: ray passes down through cone disc plane
		// handle case where the ray passes down through the cone disc plane
		float intersectPlane1 = RayPlaneIntersect(rayOrigin, rayDir, coneDir.xyz, coneEnd.xyz, distanceToPlane1);
		RayPlaneDiscIntersect(rayOrigin, rayDir, coneEnd.xyz, coneDir.w, distanceToPlane1, hasCap1);

		// if hasCap, y becomes cap intersect, else y is min of distance to plane or y
		t.y = (intersectPlane1 * ((distanceToPlane1 * hasCap1) + (min(distanceToPlane1, t.y) * !hasCap1))) + (t.y * !intersectPlane1);
		// ---

		// case 2: ray passes up through cone disc plane
		// handle case where the ray passes up through the cone disc plane
		float intersectPlane2 = RayPlaneIntersect(rayOrigin, rayDir, -coneDir.xyz, coneEnd.xyz, distanceToPlane2);
		RayPlaneDiscIntersect(rayOrigin, rayDir, coneEnd.xyz, coneDir.w, distanceToPlane2, hasCap2);

		// if hasCap, x becomes cap intersect, else x is un-modified
		t.x = (intersectPlane2 * ((distanceToPlane2 * hasCap2) + (t.x * !hasCap2))) + (t.x * !intersectPlane2);

		// if the plane intersect is closer than the distance to the back cone intersect, y is unmodified, else y is 0
		t.y *= (!intersectPlane2 || distanceToPlane2 < t.y);
		// ---

		// case 3: ray does not pass through cone disc plane
		// handle case where ray does not intersect the cone disc plane
		// point must be within slant distance, else throw it out (squared distance for performance)
		float3 distanceVector = rayOrigin + (rayDir * t.y) - conePosition.xyz;
		t.y *= (intersectPlane1 || intersectPlane2 || dot(distanceVector, distanceVector) < coneEnd.w);

		// clamp results to ray length, remove negative values
		distanceToCone = clamp(t.x, 0.0, rayLength);
		intersectAmount = clamp(t.y, 0.0, rayLength);
		intersectAmount = max(0.0, intersectAmount - distanceToCone);

		return intersectAmount > 0.0001;
	}
}

inline float RandomFloat(float3 v)
{
	return (frac(frac(dot(v.xyz, float3(12.9898, 78.233, 45.5432))) * 43758.5453) - 0.5) * 2.0;
	//return frac(sin(dot(v.xyz, float3(12.9898, 78.233, 45.5432))) * 43758.5453);
}

inline void GetFullScreenBoundingBox(float height, out float3 boxMin, out float3 boxMax)
{
	boxMin = float3(_WorldSpaceCameraPos.x - _ProjectionParams.z, -_ProjectionParams.z, _WorldSpaceCameraPos.z - _ProjectionParams.z);
	boxMax = float3(_WorldSpaceCameraPos.x + _ProjectionParams.z, height, _WorldSpaceCameraPos.z + _ProjectionParams.z);
}

inline void GetFullScreenBoundingBox2(float minHeight, float maxHeight, out float3 boxMin, out float3 boxMax)
{
	boxMin = float3(_WorldSpaceCameraPos.x - _ProjectionParams.z, minHeight, _WorldSpaceCameraPos.z - _ProjectionParams.z);
	boxMax = float3(_WorldSpaceCameraPos.x + _ProjectionParams.z, maxHeight, _WorldSpaceCameraPos.z + _ProjectionParams.z);
}

inline float CalculateNoiseXZ(sampler2D noiseTex, float3 worldPos, float scale, float2 offset, float2 velocity, float multiplier, float adder)
{
	float2 noiseUV = float2(worldPos.x * scale, worldPos.z * scale);
	noiseUV += offset + velocity;
	float4 uvlod = float4(noiseUV.x, noiseUV.y, 0, 0);
	return (tex2Dlod(noiseTex, uvlod).a + adder) * multiplier;
}

inline float2 AdjustFullScreenUV(float2 uv)
{
	uv = UnityStereoTransformScreenSpaceTex(uv);
	//uv = UnityStereoScreenSpaceUVAdjust(uv, _MainTex_ST);

#if UNITY_UV_STARTS_AT_TOP

	if (_MainTex_TexelSize.y < 0)
	{
		uv.y = 1.0 - uv.y;
	}

#endif

	return uv;
}

inline float4 UnityObjectToClipPosFarPlane(float4 vertex)
{
	float4 pos = UnityObjectToClipPos(float4(vertex.xyz, 1.0));
	if (UNITY_NEAR_CLIP_VALUE == 1.0)
	{
		// DX
		pos.z = 0.0;
	}
	else
	{
		// OpenGL
		pos.z = pos.w;
	}
	return pos;
}

// only works for wrapping coordinates, sin / cos
inline float2 RotateUV(float2 uv, float s, float c)
{
	return float2((uv.x * c) - (uv.y * s), (uv.x * s) + (uv.y * c));
}

inline void ApplyDither(inout fixed3 rgb, float2 screenUV, fixed l)
{
	fixed3 gradient = frac(cos(dot(screenUV * _WeatherMakerTime.x, ditherMagic.xy)) * ditherMagic.z) * l;
	rgb = max(0, (rgb - gradient));
}

inline fixed GetMieScattering(float cosAngle)
{
	const float MIEGV_COEFF = 0.1;
	const float4 MIEGV = float4(1.0 - (MIEGV_COEFF * MIEGV_COEFF), 1.0 + (MIEGV_COEFF * MIEGV_COEFF), 2.0 * MIEGV_COEFF, 1.0f / (4.0f * 3.14159265358979323846));
	return MIEGV.w * (MIEGV.x / (pow(MIEGV.y - (MIEGV.z * cosAngle), 1.5)));
}

full_screen_fragment full_screen_vertex_shader(full_screen_vertex v)
{
	WM_INSTANCE_VERT(v, full_screen_fragment, o);
	o.vertex = UnityObjectToClipPosFarPlane(v.vertex);
	o.uv = AdjustFullScreenUV(v.uv);
	o.forwardLine = GetFarPlaneVectorFullScreen(v.uv, eyeIndex);
	return o;
}

#endif // __WEATHER_MAKER_SHADER__
