// UNITY_SHADER_NO_UPGRADE

// Can't force unity to use posWorld for 5.6+ since this define is overwritten inside unity's CGINC and the old one doesn't use posWorld anymore
//#define UNITY_REQUIRE_FRAG_WORLDPOS 1
// Forced unity to implement posWorld inside VertexOutputForwardBase but doesn't for 5.6+ since unity removed posWorld from it
#if UNITY_VERSION < 560
#ifndef UNITY_SPECCUBE_BOX_PROJECTION
#if !SHADER_API_OPENGL
#define UNITY_SPECCUBE_BOX_PROJECTION 1
#endif
#endif
#endif
#include "UnityStandardCore.cginc"

// TRANSFORMS
fixed _Tiling, _Density, _DensityLow, _DensityHigh, _SphereHorizonStretch;
sampler2D _PerlinNormalMap, _DensityMap;
fixed4 _PerlinNormalMap_ST, _DensityMap_ST;
fixed4 _WindDirection, _CloudTransform, _CloudSpherePosition, _CloudSphereDimensions;
fixed _CloudBaseFlatness;

// TIME
fixed4 _TimeEditor;
fixed _TimeMult, _TimeMultSecondLayer;

// COLOR AND SHADING
fixed4 _VisiblePointLights[64], _VisiblePointLightsColor[64];
int _VisibleLightCount;
fixed4 _BaseColor,  _LightAttenuation, _Shading, _ShadowColor;
fixed _DepthColor, _NormalsIntensity, _Normalized, _IndirectContribution;
fixed _Alpha, _AlphaCut, _DistanceBlend;

// RAYMARCHER
sampler2D _CameraDepthTexture;
fixed4x4 _ToWorldMatrix;
int _MaxSteps;
fixed _StepSize, _StepNearSurface, _StepSkip, _OpacityGain;
int _SkipPixel;
fixed _LodBase, _LodOffset, _DrawDistance, _ShadowDrawDistance;

fixed _SelfShadowBias, _SelfShadowIntensity;

float _OrthographicPerspective;






static UnityLight light;
static fixed3 planePos;






VertexOutputForwardBase vertForwardModified (VertexInput v)
{
	VertexOutputForwardBase o = vertForwardBase(v);

	#if UNITY_VERSION >= 540
		o.tex = UnityObjectToClipPos(v.vertex);
		float3 worldPos = mul(unity_ObjectToWorld, v.vertex);
	#else
		o.tex = mul(UNITY_MATRIX_MVP, float4(v.vertex.xyz, 1.0));
		float3 worldPos = mul(_Object2World, v.vertex);
	#endif

	#if UNITY_VERSION >= 560
	o.tangentToWorldAndPackedData[0].xyz = worldPos;
	#endif
	float3 fakePerspDir = lerp(worldPos - _WorldSpaceCameraPos, UNITY_MATRIX_IT_MV[2].xyz, _OrthographicPerspective * UNITY_MATRIX_P[3][3]);
	o.eyeVec = NormalizePerVertexNormal(fakePerspDir);
	return o;
}



inline float SampleCloudsBase(fixed2 UV, fixed sampleHeight, fixed lod, out fixed4 cloudTexture, out fixed4 cloudTexture2, out fixed density)
{
	fixed2 baseAnimation = (_Time.g + _TimeEditor.g) * 0.001 * _WindDirection.xz;
	fixed2 worldUV = (UV+_CloudTransform.zw)/_Tiling;

	fixed2 newUV = worldUV - (baseAnimation * _TimeMult);
	// offset of half on both axis to avoid any possible overlap between both layers
	fixed2 newUV2 = worldUV + fixed2(0.5, 0.5) - (baseAnimation * _TimeMultSecondLayer);
	
	cloudTexture = tex2Dlod(_PerlinNormalMap, fixed4(newUV, 0, lod));
	cloudTexture2 = tex2Dlod(_PerlinNormalMap, fixed4(newUV2, 0, lod));
	
	density = _Density;
	
#if _DENSITYMAP
	fixed densityMap = tex2Dlod(_DensityMap, fixed4((UV+_DensityMap_ST.zw)/_DensityMap_ST.xy, 0, lod)).r;
	density += lerp(_DensityLow, _DensityHigh, densityMap);
#endif

	return saturate((cloudTexture.a - cloudTexture2.a + density)*_Alpha*sampleHeight);
}

inline float3 ComputeNormals(fixed4 tex1, fixed4 tex2, float sampleHeight, float density)
{
	// Remap normals from unit to normalized space 
	tex1.xyz = tex1.xyz*2-1;
	tex2.xyz = tex2.xyz*2-1;
	return normalize(  (tex1.xyz*tex1.a - tex2.xyz*tex2.a) * sampleHeight + float3(0, 0, saturate(density))  );
}

inline fixed4 SampleClouds(fixed2 UV, fixed sampleHeight, fixed lod)
{
	fixed density;
	fixed4 tex1, tex2;
	fixed4 cloud = SampleCloudsBase(UV, sampleHeight, lod, tex1, tex2, density);
	#if _NORMALMAP
	cloud.xyz = ComputeNormals(tex1, tex2, sampleHeight, density);
	#endif
	return cloud;
}

inline fixed SampleCloudsShadow(fixed2 UV, fixed sampleHeight, fixed lod)
{
	fixed density;
	fixed4 tex1, tex2;
	return SampleCloudsBase(UV, sampleHeight, lod, tex1, tex2, density);
}

inline fixed3 IntersectionOnPlane(fixed3 offsetOrthogonalToPlane, fixed3 rayDirection, out bool oob)
{
	fixed dotToSurface = dot(normalize(offsetOrthogonalToPlane), rayDirection);
	if(dotToSurface <= 0.0)
	{
		oob = true;
		return fixed3(0, 0, 0);
	}
	oob = false;
	return rayDirection * length(offsetOrthogonalToPlane) / dotToSurface;
}

inline fixed3 RaySphereIntersection(fixed3 spherePos, fixed3 sphereSize, float sphereRadius, fixed3 rayPos, fixed3 rayDirection, fixed fromOutside, out bool oob)
{
    fixed3 rayPosToSpherePos = (spherePos - rayPos);
    float sphereDirPerpOffset = dot(rayPosToSpherePos, rayDirection);
    fixed3 dirPerpToSpherePos = rayDirection * sphereDirPerpOffset;

    fixed3 centerToPerp = (dirPerpToSpherePos - rayPosToSpherePos);///sphereSize;
	float centerToPerpMag = length(centerToPerp);
    // Are we still inside the sphere ?
    if (centerToPerpMag < sphereRadius)
    {
        float collisionOffset = sqrt(sphereRadius * sphereRadius - centerToPerpMag * centerToPerpMag);
        fixed3 collisionPoint = spherePos + (centerToPerp + rayDirection * collisionOffset * fromOutside);
		
		if (dot(collisionPoint - rayPos, rayDirection) > 0)
        {
            oob = false;
            return collisionPoint;
        }
    }
    oob = true;
    return 0;
}

inline fixed3 RaySphereIntersectionAlwaysFromInside(fixed3 spherePos, fixed3 sphereSize, float sphereRadius, fixed3 rayPos, fixed3 rayDirection)
{

	fixed3 rayPosToSpherePos = (spherePos - rayPos);
    float sphereDirPerpOffset = dot(rayPosToSpherePos, rayDirection);
    fixed3 dirPerpToSpherePos = rayDirection * sphereDirPerpOffset;

    fixed3 centerToPerp = (dirPerpToSpherePos - rayPosToSpherePos);///sphereSize;
	float centerToPerpMag = length(centerToPerp);
    // Are we still inside the sphere ?
    /*if (centerToPerpMag < sphereRadius)
    {*/
        float collisionOffset = sqrt(sphereRadius * sphereRadius - centerToPerpMag * centerToPerpMag);
        fixed3 collisionPoint = spherePos + (centerToPerp + rayDirection * collisionOffset);
		return collisionPoint;
		/*if (dot(collisionPoint - rayPos, rayDirection) > 0)
        {
            return collisionPoint;
        }*/
    //}
    return 0;

	/*
	fixed3 rayPosToSpherePos = (spherePos - rayPos);
    float sphereDirPerpOffset = dot(rayPosToSpherePos, rayDirection);
    fixed3 dirPerpToSpherePos = rayDirection * sphereDirPerpOffset;

    fixed3 centerToPerp = (dirPerpToSpherePos - rayPosToSpherePos);///sphereSize;
	float centerToPerpMag = length(centerToPerp);

    float collisionOffset = sqrt(sphereRadius * sphereRadius - centerToPerpMag * centerToPerpMag);
    return spherePos + (centerToPerp + rayDirection * collisionOffset);*/
}

inline fixed3 IntersectionInSphere(fixed3 spherePos, fixed3 sphereSize, float sphereRadius, float sphereDepth, fixed3 rayPos, fixed3 rayDirection, out bool oob)
{
    oob = false;

    // Is inside spheres, try to project along dir to closest border inside
    if (length((spherePos - rayPos)/*sphereSize*/) < sphereRadius - sphereDepth)
    {
        return RaySphereIntersection(spherePos, sphereSize, sphereRadius - sphereDepth, rayPos, rayDirection, 1, oob);
    }
    // Is outside spheres, try to project along dir to closest border outside
    else if (length((spherePos - rayPos)/*sphereSize*/) > sphereRadius + sphereDepth)
    {
        return RaySphereIntersection(spherePos, sphereSize, sphereRadius + sphereDepth, rayPos, rayDirection, -1, oob);
    }
	else
	{
		// Is between big sphere's surface and small sphere's surface, do nothing
		oob = false;
		return rayPos;
	}
}








// Offset is the percentage of tile displacement, 1 = a tile, 0.5 = half a tile
inline fixed CreateGrid(fixed2 pixelPos, fixed2 offset, fixed resolution)
{
	fixed2 transform = round(frac((offset*0.5f*resolution + pixelPos) / resolution));
	return distance(transform.r+transform.g, 1.0);
}

inline fixed3 ComputePointLightSimple(fixed3 samplePos, fixed4 transform, fixed3 color)
{
	return (1-min(distance(transform.xyz, samplePos)/transform.w, 1))*color.rgb;
}

inline fixed3 ComputePointLightComplex(fixed3 samplePos, fixed3 sampleNormal, fixed opacity, fixed4 transform, fixed3 color)
{
	float3 delta = transform.xyz - samplePos;
	return (1-min(length(delta)/transform.w, 1))*((dot(normalize(delta), sampleNormal)+1)*0.5) * color.rgb * opacity;
}


inline fixed CloudCenterBounds()
{
	return _CloudTransform.x;
}

inline fixed CloudUpperBounds()
{
	return CloudCenterBounds() + _CloudTransform.y;
}

inline fixed CloudLowerBounds()
{
	return CloudCenterBounds() - _CloudTransform.y;
}



inline fixed4 WorldPosFromDepth(fixed2 uv)
{
	fixed vz = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv));
	vz = min(_DrawDistance, vz);
	// Depth stops at far clipping plane even if there aren't any object there. Extend depth to drawdistance
	if(distance(vz, _ProjectionParams.z) < _ProjectionParams.z/100.0)
		vz = _DrawDistance;
    fixed2 p11_22 = fixed2(unity_CameraProjection._11, unity_CameraProjection._22);
    fixed3 vpos = fixed3((uv * 2 - 1) / p11_22, -1) * vz;
	fixed4 wpos = mul(_ToWorldMatrix, fixed4(vpos, 1));
    return wpos;
}


inline fixed DistFromCloudCenterSigned(fixed3 samplePos)
{
#if _SPHEREMAPPED
	half3 dirToSphereCenter = normalize(samplePos - _CloudSpherePosition);
	half3 projectedCenter = _CloudSpherePosition + dirToSphereCenter * CloudCenterBounds();
	half3 deltaToCloudCenter = samplePos - projectedCenter;
	half dist = length(deltaToCloudCenter) * sign(dot(normalize(deltaToCloudCenter), dirToSphereCenter)) / _CloudTransform.y;
#else
	float dist = (samplePos.y - CloudCenterBounds()) / _CloudTransform.y;
#endif
	return lerp(dist, dist/_CloudBaseFlatness, saturate(-dist));
}

inline fixed DistFromCloudCenter(fixed3 samplePos)
{
	return abs(DistFromCloudCenterSigned(samplePos));
}

inline bool IsOutsideCloudBounds(fixed3 samplePos, fixed sampleDir)
{
#if _SPHEREMAPPED
	return distance(samplePos, _CloudSpherePosition) > CloudUpperBounds() + 0.1;
#else
	return samplePos.y > CloudUpperBounds() + 0.01 || samplePos.y < CloudLowerBounds() - 0.01;
#endif
}
#if _SPHEREMAPPED
// put sample pos on the closest point forward in sphere bounds, doesnt work properly for some reason
inline fixed3 IsInsideEmptySphere(fixed3 samplePos, fixed sampleDir)
{
	if(length((samplePos - _CloudSpherePosition)/* * _CloudSphereDimensions*/) < CloudLowerBounds() -0.1)
		return RaySphereIntersectionAlwaysFromInside(_CloudSpherePosition, _CloudSphereDimensions, CloudLowerBounds(), samplePos, sampleDir);

	return samplePos;
}
#endif















inline fixed3 PlaceSamplePosOnBounds(fixed3 samplePos, fixed3 initDir, out bool oob)
{	
#if _SPHEREMAPPED
	oob = true;
	samplePos = IntersectionInSphere(_CloudSpherePosition, _CloudSphereDimensions, CloudCenterBounds(), _CloudTransform.y, samplePos, initDir, oob);
	return samplePos;
#endif
#if !_SPHEREMAPPED
	oob = false;
	if(samplePos.y > CloudUpperBounds())
	{
		fixed3 offsetOrthoToBounds = fixed3(samplePos.x, CloudUpperBounds(), samplePos.z) - samplePos;
		fixed3 offsetToBounds = IntersectionOnPlane(offsetOrthoToBounds, initDir, oob);

		return samplePos += offsetToBounds;
	}
	if(samplePos.y < CloudLowerBounds())
	{
		fixed3 offsetOrthoToBounds = fixed3(samplePos.x, CloudLowerBounds(), samplePos.z) - samplePos;
		fixed3 offsetToBounds = IntersectionOnPlane(offsetOrthoToBounds, initDir, oob);

		return samplePos += offsetToBounds;
	}
	return samplePos;
#endif
}

// Not really precise but works well enough for now
inline fixed3 TransformNormalsAroundSphere(fixed3 normalPlanar, fixed3 samplePosOnSphere)
{
	fixed dotF = dot(samplePosOnSphere, fixed3(0, 0, 1));
    fixed dotU = dot(samplePosOnSphere, fixed3(0, 1, 0));
    fixed3 fwdDir = normalize(fixed3(0, 0, 1) * dotU + fixed3(0, -1, 0) * dotF);
    fixed3 rightDir = normalize(cross(samplePosOnSphere, fwdDir));
	fixed scalar = clamp(samplePosOnSphere.y*2, -1, 1);
	return normalize(normalPlanar.x * rightDir * scalar + normalPlanar.y * samplePosOnSphere + normalPlanar.z * fwdDir * scalar);
}













#define TAU 6.28318530717958647692

inline fixed2 ComputeSphereUV(float3 deltaFromCenter)
{
	#if _SPHERICAL_MAPPING
	fixed3 p = normalize(deltaFromCenter);
	return fixed2(atan2(p.x, p.z)/* / TAU + 0.5*/, acos(p.y)) * 1000;
	#else
	return deltaFromCenter.xz * pow((CloudUpperBounds()-abs(deltaFromCenter.y))/CloudUpperBounds(), _SphereHorizonStretch);
	#endif
}

inline fixed GetLODScale(float distanceTravelled)
{
	return _LodBase + sqrt(sqrt(distanceTravelled) * _LodOffset);
}

inline fixed4 RaymarchShadowClouds(fixed3 scenePixelPos, fixed renderedAlpha)
{
	fixed maximumShadowDrawDistance = min(_ProjectionParams.z, _ShadowDrawDistance);
	if(distance(planePos, scenePixelPos) > maximumShadowDrawDistance || renderedAlpha >= .99f)
		return 0.0;
	// Init raymarching data
	// Start after clipping planes
	fixed3 initDir = light.dir;
	fixed3 initPos = scenePixelPos;
	fixed3 samplePos = initPos;
	fixed3 offset = initDir * _StepSize;
	fixed distanceTravelled = 0.0;

	fixed shadow = 0.0;
	
	// PLACE SAMPLEPOS ON CLOUD BOUNDS
	bool oob;
	samplePos = PlaceSamplePosOnBounds(samplePos, initDir, oob);
	if(oob)
		return 0.0;
		
	// RAYMARCH
	#if !SHADER_API_OPENGL && !SHADER_API_GLES3 && !SHADER_API_GLES
	[loop]
	#endif
	for(int i = 0; i < _MaxSteps; i++)
	{
		/*
	#if _SPHEREMAPPED
		samplePos = IsInsideEmptySphere(samplePos, initDir);
	#endif*/

		// If current pos is outside of cloud bounds quit loop
		if(IsOutsideCloudBounds(samplePos, initDir))
			break;
		
		// If distance travelled is greater than draw distance or scene depth, quit loop
		distanceTravelled = distance(samplePos, initPos);
		if(distanceTravelled > maximumShadowDrawDistance)
			break;

		fixed dist = DistFromCloudCenter(samplePos);
	#if _SPHEREMAPPED
		fixed3 deltaFromCenter = samplePos - _CloudSpherePosition;
		fixed textureSample = SampleCloudsShadow(ComputeSphereUV(deltaFromCenter), 1.0-saturate(dist), GetLODScale(distanceTravelled));
		#if _SPHERICAL_MAPPING
			textureSample *= saturate((1-abs(normalize(deltaFromCenter).y))/_SphereHorizonStretch);
		#endif
	#else
		fixed textureSample = SampleCloudsShadow(samplePos.xz, 1.0-saturate(dist), GetLODScale(distanceTravelled));
	#endif
		fixed3 newSampleOffset = offset * (1.0+sqrt(distanceTravelled)*_StepSkip);

		// Is inside cloud volume ?
		if(textureSample > dist)
		{
			// Opacity based on position inside the cloud and ray passthrough
			fixed opacityGain = _OpacityGain * distance(textureSample, dist);
			shadow += opacityGain;
			newSampleOffset = offset * (1.0+sqrt(distanceTravelled)*_StepSkip) * _StepNearSurface * (1.0-textureSample);
			if(shadow >= 1.0)
				break;
		}
		samplePos+=newSampleOffset;
	}
	return fixed4(_ShadowColor.rgb, saturate(shadow)*_ShadowColor.a);
}





inline fixed3 NormalLighting(fixed3 viewDir, fixed3 normal)
{
	fixed diffuseTerm = dot(normal, light.dir);
	diffuseTerm = saturate(lerp(diffuseTerm, diffuseTerm*0.5+0.5, _Normalized));

	fixed3 IndirectIllumination = ShadeSH12Order(fixed4(normal,1))*_IndirectContribution;

	return lerp(fixed3(1, 1, 1), _BaseColor.xyz * lerp( _Shading.rgb, _Shading.a, (IndirectIllumination.rgb + light.color * diffuseTerm)), _BaseColor.a);
}








inline fixed4 RaymarchClouds(float3 viewDir, float3 startPos, fixed depth)
{
	// Init raymarching data
	fixed3 initDir = viewDir;
	fixed3 initPos = startPos;
	fixed3 samplePos = initPos;
	fixed3 offset = initDir * _StepSize;
	fixed distanceTravelled = 0.0;
	fixed3 normals = 0;

	fixed4 colors = 0.0;

	// PLACE SAMPLEPOS ON CLOUD BOUNDS
	bool oob;
	samplePos = PlaceSamplePosOnBounds(samplePos, initDir, oob);
	if(oob)
		return fixed4(0, 0, 0, 0);

	// RAYMARCH
	fixed distInsideCloud = 0;
	#if !SHADER_API_OPENGL && !SHADER_API_GLES3 && !SHADER_API_GLES
	[loop]
	#endif
	for(int i = 0; i < _MaxSteps; i++)
	{

		/*
	#if _SPHEREMAPPED
		samplePos = IsInsideEmptySphere(samplePos, initDir);
	#endif*/

		// If current pos is outside of cloud bounds quit loop
		if(IsOutsideCloudBounds(samplePos, initDir))
			break;

		// If distance travelled is greater than draw distance or scene depth, quit loop
		distanceTravelled = distance(samplePos, initPos);
		if(distanceTravelled > depth)
			break;
		fixed3 newSampleOffset = offset * (1.0+sqrt(distanceTravelled)*_StepSkip);

		fixed dist = DistFromCloudCenterSigned(samplePos);
		fixed absDist = abs(dist);
	#if _SPHEREMAPPED
		fixed3 deltaFromCenter = samplePos - _CloudSpherePosition;
		fixed4 textureSample = SampleClouds(ComputeSphereUV(deltaFromCenter), 1.0-absDist, GetLODScale(distanceTravelled));
		#if _SPHERICAL_MAPPING
			textureSample.a *= saturate((1-abs(normalize(deltaFromCenter).y))/_SphereHorizonStretch);
		#endif
	#else
		fixed4 textureSample = SampleClouds(samplePos.xz, 1.0-absDist, GetLODScale(distanceTravelled));
	#endif

		// Is inside cloud volume ?
		if(textureSample.w > absDist)
		{
			// Opacity based on position inside the cloud and ray passthrough
			fixed opacityGain = _OpacityGain * distance(textureSample.w, absDist);
			distInsideCloud = opacityGain;
			colors.a += opacityGain;
			fixed3 baseSampleColor = lerp(1.0, dist * 0.5 + 0.5, _DepthColor) * opacityGain;
			#if _NORMALMAP
				normals = normalize(fixed3(textureSample.x*_NormalsIntensity, dist, textureSample.y*_NormalsIntensity));
				#if _SPHEREMAPPED
					normals = TransformNormalsAroundSphere(normals, normalize(deltaFromCenter));
				#endif
				fixed lightAttenuation = saturate((1.0-saturate(distanceTravelled))*distInsideCloud*_LightAttenuation.a*50.0);
				normals = normalize(lerp(normals, -light.dir, lightAttenuation));
				baseSampleColor *= NormalLighting(-initDir, normals);
			#endif
			colors.rgb += baseSampleColor;
			
			
			// Point light contribution
			#if _HQPOINTLIGHT
				for(int j = 0; j < _VisibleLightCount; j++)
				{
					colors.rgb += ComputePointLightComplex(samplePos, normals, opacityGain, _VisiblePointLights[j], _VisiblePointLightsColor[j].rgb);
				}
			#endif

			newSampleOffset = offset * (1.0+sqrt(distanceTravelled)*_StepSkip) * _StepNearSurface * (1.0-textureSample.w);
			
			if(colors.a >= 1.0)
				break;
		}
		samplePos += newSampleOffset;
	}
	#if _RENDERSELFSHADOWS
		fixed3 selfShadowing = saturate(RaymarchShadowClouds(samplePos+light.dir*_SelfShadowBias, 0.0).a/_ShadowColor.a);
		fixed3 shade = _Shading.rgb;
		#if _NORMALMAP
		shade += ShadeSH12Order(fixed4(normals,1)) * _IndirectContribution;
		#endif
		colors.rgb = lerp(colors.rgb, (shade+colors.rgb) * 0.5, _SelfShadowIntensity * selfShadowing * saturate(colors.a));
	#endif

	colors.a = saturate(colors.a)*(1-saturate(pow(distanceTravelled/_DrawDistance,1.0/_DistanceBlend)));
	// Re-equilibrate transparent colors
	colors.rgb += (1.0-colors.a)*colors.rgb;
	// Light attenuation inside clouds
	colors.rgb = lerp(colors.rgb, _LightAttenuation.rgb, saturate((1.0-saturate(distanceTravelled))*distInsideCloud*_LightAttenuation.a*50.0));
	
#if !_HQPOINTLIGHT
	// Point light contribution
	for(int j = 0; j < _VisibleLightCount; j++)
	{
		colors.rgb += ComputePointLightSimple(samplePos, _VisiblePointLights[j], _VisiblePointLightsColor[j].rgb);
	}
#endif

	return colors;
}


half4 fragForwardClouds (VertexOutputForwardBase i) : SV_Target
{
	// retrieve static variable
#if UNITY_VERSION >= 550
	light = MainLight ();
#else
	// We don't need the ndotl so just put anything in here
	light = MainLight (fixed3(0, 0, 0));
#endif

	i.eyeVec = normalize(i.eyeVec);

	float3 worldPos = 0;
	#if UNITY_VERSION >= 560
	worldPos = i.tangentToWorldAndPackedData[0].xyz;
	#else
	worldPos = i.posWorld;
	#endif
	planePos = worldPos;

	fixed grabSign = abs(_ProjectionParams.x);
	fixed2 screenPos = i.tex.xy / i.tex.w;
    screenPos.y *= _ProjectionParams.x;
    fixed2 sceneUVs = fixed2(1,grabSign)*screenPos.xy*0.5+0.5;
	#if UNITY_VERSION >= 540
	sceneUVs = UnityStereoTransformScreenSpaceTex(sceneUVs);
	#endif
	fixed4 sceneDepthPos = WorldPosFromDepth(sceneUVs);
	fixed sceneDepth = distance(sceneDepthPos, worldPos);
	
	FRAGMENT_SETUP(s)

	fixed4 clouds = 0;
	fixed4 shadows = 0;

	#if X_AXIS
	i.eyeVec = i.eyeVec.yxz;
	worldPos = worldPos.yxz;
	light.dir = light.dir.yxz;
	sceneDepthPos.xyz = sceneDepthPos.yxz;
	#elif(Z_AXIS)
	i.eyeVec = i.eyeVec.xzy;
	worldPos = worldPos.xzy;
	light.dir = light.dir.xzy;
	sceneDepthPos.xyz = sceneDepthPos.xzy;
	#endif

	#if _RENDERSHADOWSONLY == false
		clouds = RaymarchClouds(i.eyeVec, worldPos, sceneDepth);
	#endif
	#if _RENDERSHADOWS
		shadows = RaymarchShadowClouds(sceneDepthPos, clouds.a);
	#endif
	clouds += fixed4(shadows.rgb*shadows.a, shadows.a)*(1-clouds.a);

	if(clouds.a <= _AlphaCut)
		discard;
	return clouds;
}
