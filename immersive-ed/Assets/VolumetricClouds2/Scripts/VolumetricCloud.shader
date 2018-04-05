Shader "VolumetricCloud"
{
	Properties
	{
	    [NoScaleOffset]_PerlinNormalMap ("Perlin Normal Map", 2D) = "white" {}

		[Header(Colors)]
			_BaseColor ("Base Color", Color) = (1,1,1,1)
			_Shading ("Shading Color", Color) = (0, 0, 0, 1)
			[Toggle(_NORMALMAP)] _NormalmapState ("Use Normalmap", int) = 1
				[ToggleHideDrawer(_NormalmapState)] _NormalsIntensity ("Normals Intensity", float ) = 1
				[ToggleHideDrawer(_NormalmapState)] _IndirectContribution("Indirect Lighting", float) = 1
				[ToggleHideDrawer(_NormalmapState)] _Normalized ("Normalized", float ) = 0
			_DepthColor ("Depth Intensity", float ) = 0
			_LightAttenuation ("Light Attenuation", Color) = (.35,.35,.35,.15)
			_DistanceBlend ("Distance Blend", float ) = 0.6
			[Toggle(_RENDERSELFSHADOWS)] _RSShadows ("Receive Self Shadows", int) = 0
				[ToggleHideDrawer(_RSShadows)] _SelfShadowBias ("Bias", float) = 10
				[ToggleHideDrawer(_RSShadows)] _SelfShadowIntensity ("Intensity", float) = 1
			[Toggle(_RENDERSHADOWS)] _SSShadows ("Screen Space Shadows", int) = 0
				[ToggleHideDrawer(_SSShadows)] _ShadowColor ("Shadow Color", Color) = (0,0,0,.5)
				[ToggleHideDrawer(_SSShadows)] _ShadowDrawDistance ("Draw Distance", float) = 999
				[Toggle(_RENDERSHADOWSONLY)] _RenderShadowsOnly ("Render Shadows Only", int) = 0
				[Toggle(_HQPOINTLIGHT)] _HQPointLight ("High quality point light", int) = 0


        [Header(Shape)]
			_Density ("Density", float ) = 0
			[Toggle(_DENSITYMAP)] _DensityMapOn ("Density Map", int) = 0
				[ToggleHideDrawer(_DensityMapOn)] _DensityMap ("Density Map", 2D) = "white" {}
				[ToggleHideDrawer(_DensityMapOn)] _DensityLow ("Density Low", float) = -1
				[ToggleHideDrawer(_DensityMapOn)] _DensityHigh ("Density High", float) = 1
			_Alpha ("Alpha", float ) = 4
			_AlphaCut ("AlphaCut", float ) = 0.01
        
		[Header(Animation)]
			_TimeMult ("Speed", float ) = 0.1
			_TimeMultSecondLayer ("Speed Second Layer", float ) = 4
			_WindDirection ("Wind Direction", vector) = (1,0,0,0)

		[Header(Dimensions)]
			_CloudTransform("Cloud Transform", vector) =  (100, 20, 0, 0)
			_CloudBaseFlatness("Cloud base flatness", float) = 0.1
			[Toggle(_SPHEREMAPPED)] _SphereMapped ("Spherical", int) = 0
				[ToggleHideDrawer(_SphereMapped)] _CloudSpherePosition("Sphere position", vector) =  (0, 0, 0, 0)
				[ToggleHideDrawer(_SphereMapped)] _SphereHorizonStretch("Sphere stretch horizon", float) = .6
				[ToggleHideDrawer(_SphereMapped, _SPHERICAL_MAPPING)] _SphericalMapping ("Spherical Mapping", int) = 0
				//[ToggleHideDrawer(_SphereMapped)] _CloudSphereDimensions("Cloud Sphere dimensions", vector) =  (1, 1, 1, 0)
			_Tiling ("Tiling", float ) = 1500
			[KeywordEnumFullDrawer(Y_AXIS, X_AXIS, Z_AXIS)] _Alignement("Plane Alignement", int) = 0
			_OrthographicPerspective ("Orthographic Perspective", float) = 0

		[Space(10)]

		[Header(Raymarcher)]
			_DrawDistance("Draw distance", float) = 1000.0
			_MaxSteps("Steps Max", int) = 500
			_StepSize("Step Size", float) = 0.015
			_StepSkip("Step Skip", float) = 10
			_StepNearSurface("Step near surface", float) = 0.5
			_LodBase("Lod Base", float) = 0
			_LodOffset("Lod Offset", float) = 0.7
			_OpacityGain("Opacity Gain", float) = 0.1

		[Space(10)]

		[Header(Debug)]
			_RenderQueue("Render Queue", Range(0, 5000)) = 2501
	}

	SubShader
	{
		Tags
		{
			"RenderType"="Transparent" 
			"PreviewType"="Plane"
			/*"PerformanceChecks"="False"*/
		}
		LOD 300
	
		Pass
		{
			Name "FORWARD" 
			Tags 
			{ 
				"LightMode"="ForwardBase"
				"Queue"="Transparent"
				"RenderType"="Transparent"
				"IgnoreProjector"="True"
			}
			Blend SrcAlpha OneMinusSrcAlpha
			ZWrite Off
			Cull Back
			
			CGPROGRAM
			#pragma target 3.0
			
			// -------------------------------------
					
			#pragma shader_feature _NORMALMAP
			#pragma shader_feature _RENDERSHADOWS
			#pragma shader_feature _RENDERSHADOWSONLY
			#pragma shader_feature _HQPOINTLIGHT
			#pragma shader_feature _DENSITYMAP
			#pragma shader_feature _SPHEREMAPPED
			#pragma shader_feature _SPHERICAL_MAPPING
			#pragma shader_feature _RENDERSELFSHADOWS
			#pragma shader_feature Y_AXIS X_AXIS Z_AXIS

			#pragma multi_compile_fwdbase

			#pragma vertex vertForwardModified
			#pragma fragment fragForwardClouds

			#include "VolumetricCloudsCG.cginc"

			ENDCG
		}
	}
	CustomEditor "VCloudShaderGUI"

}
