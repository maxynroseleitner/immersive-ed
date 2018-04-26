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

// http://rastergrid.com/blog/2010/09/efficient-gaussian-blur-with-linear-sampling/
// _MainTex must be bilinear

Shader "WeatherMaker/WeatherMakerFullScreenBlurShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "red" {}
	}
	SubShader
	{
		Cull Off ZWrite Off ZTest [_ZTest]
		Blend [_SrcBlendMode][_DstBlendMode]

		CGINCLUDE

		#pragma target 3.0
		#pragma fragmentoption ARB_precision_hint_fastest
		#pragma multi_compile __ BLUR7

		#define WM_IS_FULL_SCREEN_EFFECT

		#include "WeatherMakerShader.cginc"

		struct v2f
		{
			float4 vertex : SV_POSITION;
			float2 uv : TEXCOORD0;

#if defined(BLUR7)

			float2 offsets : TEXCOORD1;

#else

			float4 offsets : TEXCOORD1;

#endif

			WM_BASE_VERTEX_TO_FRAG
		};

		v2f vert (appdata_base v)
		{
			WM_INSTANCE_VERT(v, v2f, o);
			o.vertex = UnityObjectToClipPosFarPlane(v.vertex);
			o.uv = AdjustFullScreenUV(v.texcoord);

#if defined(BLUR7)

			// take top left 3 and bottom right 3 plus center pixel average
			o.offsets = float2(_MainTex_TexelSize.x * 0.333333, _MainTex_TexelSize.y * 0.333333);

#else

			// (0.4,-1.2) , (-1.2,-0.4) , (1.2,0.4) and (-0.4,1.2).
			o.offsets = float4
			(
				_MainTex_TexelSize.x * 0.4,
				_MainTex_TexelSize.x * 1.2,
				_MainTex_TexelSize.y * 0.4,
				_MainTex_TexelSize.y * 1.2
			);

#endif

			return o;
		}

		ENDCG

		// optimized blur
		Pass
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			fixed4 frag(v2f i) : SV_Target
			{
				WM_INSTANCE_FRAG(i);
				fixed4 col = WM_SAMPLE_FULL_SCREEN_TEXTURE(_MainTex, i.uv);

#if defined(BLUR7)

				// 7 tap approximation with 2 texture lookups
				col += WM_SAMPLE_FULL_SCREEN_TEXTURE(_MainTex, float2(i.uv.x - i.offsets.x, i.uv.y - i.offsets.y));
				col += WM_SAMPLE_FULL_SCREEN_TEXTURE(_MainTex, float2(i.uv.x + i.offsets.x, i.uv.y + i.offsets.y));
				col *= 0.333333; // 3 total colors to average

#else

				// 17 tap approximation with 4 texture lookups
				col += WM_SAMPLE_FULL_SCREEN_TEXTURE(_MainTex, float2(i.uv.x + i.offsets.x, i.uv.y - i.offsets.w));
				col += WM_SAMPLE_FULL_SCREEN_TEXTURE(_MainTex, float2(i.uv.x - i.offsets.y, i.uv.y - i.offsets.z));
				col += WM_SAMPLE_FULL_SCREEN_TEXTURE(_MainTex, float2(i.uv.x + i.offsets.y, i.uv.y + i.offsets.z));
				col += WM_SAMPLE_FULL_SCREEN_TEXTURE(_MainTex, float2(i.uv.x - i.offsets.x, i.uv.y + i.offsets.w));
				col *= 0.2; // 5 total colors to average

#endif

				return col;
            }

            ENDCG
		}
	}
}
