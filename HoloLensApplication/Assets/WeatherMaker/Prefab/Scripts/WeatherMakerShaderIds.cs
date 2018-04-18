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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace DigitalRuby.WeatherMaker
{
    public static class WeatherMakerShaderIds
    {
        private static readonly Color[] tmpColorArray = new Color[4];
        private static readonly float[] tmpFloatArray = new float[4];
        private static readonly float[] tmpFloatArray2 = new float[8];
        private static readonly Vector4[] tmpVectorArray = new Vector4[4];

        public static readonly int ArrayWeatherMakerMoonDirectionUp;
        public static readonly int ArrayWeatherMakerMoonDirectionDown;
        public static readonly int ArrayWeatherMakerMoonLightColor;
        public static readonly int ArrayWeatherMakerMoonLightPower;
        public static readonly int ArrayWeatherMakerMoonTintColor;
        public static readonly int ArrayWeatherMakerMoonTintIntensity;
        public static readonly int ArrayWeatherMakerMoonVar1;

        static WeatherMakerShaderIds()
        {
            ArrayWeatherMakerMoonDirectionUp = Shader.PropertyToID("_WeatherMakerMoonDirectionUp");
            ArrayWeatherMakerMoonDirectionDown = Shader.PropertyToID("_WeatherMakerMoonDirectionDown");
            ArrayWeatherMakerMoonLightColor = Shader.PropertyToID("_WeatherMakerMoonLightColor");
            ArrayWeatherMakerMoonLightPower = Shader.PropertyToID("_WeatherMakerMoonLightPower");
            ArrayWeatherMakerMoonTintColor = Shader.PropertyToID("_WeatherMakerMoonTintColor");
            ArrayWeatherMakerMoonTintIntensity = Shader.PropertyToID("_WeatherMakerMoonTintIntensity");
            ArrayWeatherMakerMoonVar1 = Shader.PropertyToID("_WeatherMakerMoonVar1");
        }

        public static void SetColorArray(Material m, string prop, Color c1, Color c2, Color c3, Color c4)
        {
            tmpColorArray[0] = c1;
            tmpColorArray[1] = c2;
            tmpColorArray[2] = c3;
            tmpColorArray[3] = c4;
            m.SetColorArray(prop, tmpColorArray);
        }

        public static void SetFloatArray(Material m, string prop, float f1, float f2, float f3, float f4)
        {
            tmpFloatArray[0] = f1;
            tmpFloatArray[1] = f2;
            tmpFloatArray[2] = f3;
            tmpFloatArray[3] = f4;
            m.SetFloatArray(prop, tmpFloatArray);
        }

        public static void SetFloatArrayRotation(Material m, string prop, float f1, float f2, float f3, float f4)
        {
            tmpFloatArray2[0] = Mathf.Cos(f1 * Mathf.Deg2Rad);
            tmpFloatArray2[1] = Mathf.Cos(f2 * Mathf.Deg2Rad);
            tmpFloatArray2[2] = Mathf.Cos(f3 * Mathf.Deg2Rad);
            tmpFloatArray2[3] = Mathf.Cos(f4 * Mathf.Deg2Rad);
            tmpFloatArray2[4] = Mathf.Sin(f1 * Mathf.Deg2Rad);
            tmpFloatArray2[5] = Mathf.Sin(f2 * Mathf.Deg2Rad);
            tmpFloatArray2[6] = Mathf.Sin(f3 * Mathf.Deg2Rad);
            tmpFloatArray2[7] = Mathf.Sin(f4 * Mathf.Deg2Rad);
            m.SetFloatArray(prop, tmpFloatArray2);
        }

        public static void SetVectorArray(Material m, string prop, Vector4 v1, Vector4 v2, Vector4 v3, Vector4 v4)
        {
            tmpVectorArray[0] = v1;
            tmpVectorArray[1] = v2;
            tmpVectorArray[2] = v3;
            tmpVectorArray[3] = v4;
            m.SetVectorArray(prop, tmpVectorArray);
        }

        public static void DisableKeywords(this Material m, params string[] keywords)
        {
            foreach (string keyword in keywords)
            {
                m.DisableKeyword(keyword);
            }
        }

        public static void Initialize() { }
    }
}
