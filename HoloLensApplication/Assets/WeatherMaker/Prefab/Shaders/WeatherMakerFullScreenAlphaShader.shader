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

Shader "WeatherMaker/WeatherMakerFullScreenAlphaShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Cull Off ZWrite Off ZTest [_ZTest]
		Blend [_SrcBlendMode][_DstBlendMode]

		Pass
		{
			CGPROGRAM

			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag

			#define WM_IS_FULL_SCREEN_EFFECT

			#include "WeatherMakerShader.cginc"

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				WM_BASE_VERTEX_TO_FRAG
			};
	 
			v2f vert (appdata_base v)
			{
				WM_INSTANCE_VERT(v, v2f, o);
				o.vertex = UnityObjectToClipPosFarPlane(v.vertex);
				o.uv = AdjustFullScreenUV(v.texcoord.xy);
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				WM_INSTANCE_FRAG(i);
				return WM_SAMPLE_FULL_SCREEN_TEXTURE(_MainTex, i.uv);
			}

			ENDCG
		}
	}
}
