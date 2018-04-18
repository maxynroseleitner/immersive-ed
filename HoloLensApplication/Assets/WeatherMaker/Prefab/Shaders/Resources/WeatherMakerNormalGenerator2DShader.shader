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

Shader "WeatherMaker/WeatherMakerNormalGenerator2DShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Strength ("Strength", Range(1.0, 100.0)) = 2.0
	}
	SubShader
	{
		Cull Back ZWrite Off ZTest Always Blend Off

		Pass
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma target 4.0
			
			#define _Radius int(5)
			#define _Diameter int(_Radius + _Radius)
			#define _YMinimum 0.6
			#define _YMaximum 1.0
			#define _YPower 2.0
			#define ARRAY_ROW_SIZE (_Diameter + 1)
			#define ARRAY_SIZE (ARRAY_ROW_SIZE * ARRAY_ROW_SIZE)

			sampler2D _MainTex;
			float4 _MainTex_TexelSize;

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			void samplePixels(float2 uv, inout float sampleGrid[ARRAY_SIZE])
			{
				float2 uvTmp;
				fixed a;

				// sample from radius
				for (int x = -_Radius; x <= _Radius; x++)
				{
					for (int y = -_Radius; y <= _Radius; y++)
					{
						uvTmp = uv;
						uvTmp.x += (_MainTex_TexelSize.x * float(x));
						uvTmp.y += (_MainTex_TexelSize.y * float(y));
						a = tex2Dlod(_MainTex, float4(uvTmp, 0.0, 0.0)).a;
						sampleGrid[x + _Radius + ((y + _Radius) * ARRAY_ROW_SIZE)] = 1.0 - a;
					}
				}
			}

			fixed4 averagePixels(float2 uv, inout float sampleGrid[ARRAY_SIZE])
			{
				fixed centerAlpha = tex2Dlod(_MainTex, float4(uv, 0.0, 0.0)).a;
				float wx, wz;
				float xWeight = 0.0;
				float yWeight = 0.0;
				float zWeight = 0.0;

				// fill grid
				for (int x = 0; x <= _Diameter; x++)
				{
					for (int y = 0; y <= _Diameter; y++)
					{
						wx = float(x - _Radius) / float(_Radius);
						wz = float(y - _Radius) / float(_Radius);
						wx = max(1.0 / float(_Radius), (1.0 - abs(wx))) * sign(wx);
						wz = max(1.0 / float(_Radius), (1.0 - abs(wz))) * sign(wz);
						xWeight += (wx * sampleGrid[x + (y * ARRAY_ROW_SIZE)]);
						zWeight += (wz * sampleGrid[x + (y * ARRAY_ROW_SIZE)]);
					}
				}

				fixed y = 0.6;// lerp(_YMaximum, _YMinimum, pow(centerAlpha, _YPower));
				return fixed4(saturate((xWeight + 1.0) * 0.5), y, saturate((zWeight + 1.0) * 0.5), centerAlpha);
			}

			fixed4 frag (v2f i) : SV_Target
			{
				float sampleGrid[ARRAY_SIZE];
				samplePixels(i.uv, sampleGrid);
				return averagePixels(i.uv, sampleGrid);
			}

			ENDCG
		}
	}
	Fallback Off
}
