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

Shader "WeatherMaker/WeatherMakerAnimated3DTextureShader"
{
	Properties
	{
		_MainTex3D ("Texture", 3D) = "white" {}
		_TintColor ("Tint Color", Color) = (1,1,1,1)
		_AlphaMultiplier ("Alpha Multiplier", Float) = 1.0
		_AlphaMultiplierAnimation ("Alpha Multiplier For Animation", Float) = 1.0
		_AnimationSpeed ("Animation Speed", Vector) = (0.01, 0.01, 1.0, 0.0)
		_InvertColor ("Invert Color", Int) = 0
		_PointSpotLightMultiplier("Point/Spot Light Multiplier", Range(0, 10)) = 1
		_DirectionalLightMultiplier("Directional Light Multiplier", Range(0, 10)) = 1
		_AmbientLightMultiplier("Ambient light multiplier", Range(0, 4)) = 1
		_SrcBlendMode("SrcBlendMode (Source Blend Mode)", Int) = 5 // SrcAlpha
		_DstBlendMode("DstBlendMode (Destination Blend Mode)", Int) = 10 // OneMinusSrcAlpha
		_WorldSpaceSampleScale("World space sample scale", Float) = 0

	}
	SubShader
	{
		Tags{ "RenderType" = "Transparent" "IgnoreProjector" = "True" "Queue" = "Transparent" }
		LOD 100

		Pass
		{
			Blend [_SrcBlendMode][_DstBlendMode]
			ZTest LEqual
			Cull Back

			CGPROGRAM

			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag

			#include "WeatherMakerShader.cginc"
			
			struct v2f
			{
				float4 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float3 worldPos : TEXCOORD1;
			};

			sampler3D _MainTex3D;
			float4 _MainTex3D_ST;
			fixed _AlphaMultiplier;
			fixed _AlphaMultiplierAnimation;
			fixed4 _AnimationSpeed;
			fixed _InvertColor;
			fixed _WorldSpaceSampleScale;
			
			v2f vert (appdata_base v)
			{
				v2f o;
				float2 uv = TRANSFORM_TEX(v.texcoord, _MainTex);
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.worldPos = WorldSpaceVertexPos(v.vertex);
				o.uv = float4((_WorldSpaceSampleScale ? RotateVertexLocalQuaternion(o.worldPos - _WorldSpaceCameraPos, float3(0.0, 1.0, 0.0), _WeatherMakerTime.x * _AnimationSpeed.w).xz * _WorldSpaceSampleScale : uv) + (_AnimationSpeed.xy * _WeatherMakerTime.y), _WeatherMakerTime.y * _AnimationSpeed.z, 0.0);

				/* // Tried to position to match depth and height but running into too many glitches
#if SHADER_TARGET >= 30

				float4 projPos = ComputeScreenPos(o.vertex);
				float3 viewPos = UnityObjectToViewPos(v.vertex);
				float2 screenUV = projPos.xy / projPos.w;
				float depth = length(DECODE_EYEDEPTH(WM_SAMPLE_DEPTH(screenUV)) / normalize(viewPos).z);

				// move vertex in world space to new position based on depth
				float3 worldPosDepth = _WorldSpaceCameraPos + (depth * normalize(o.worldPos - _WorldSpaceCameraPos));
				float3 vertexLocal = v.vertex;
				vertexLocal.y += (worldPosDepth.y - o.worldPos.y);
				o.vertex = UnityObjectToClipPos(vertexLocal);

#endif
				*/

				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col;
				// col = tex3Dlod(_MainTex3D, i.uv);
				col = tex3D(_MainTex3D, i.uv.xyz);
				col.rgb = ((1.0 - col.rgb) * _InvertColor) + ((1.0 - _InvertColor) * col.rgb);
				col.rgb *= (_TintColor.rgb * _TintColor.a);
				col.rgb *= ((_WeatherMakerAmbientLightColor.rgb * _AmbientLightMultiplier) + CalculateLightColorWorldSpace(i.worldPos, float3Zero, 1, 1));
				col.a = min(1.0, col.a * _AlphaMultiplier * _AlphaMultiplierAnimation);
				return col;
			}

			ENDCG
		}
	}
}
