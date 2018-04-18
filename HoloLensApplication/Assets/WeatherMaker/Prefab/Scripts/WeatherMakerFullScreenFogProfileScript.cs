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

using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Rendering;

namespace DigitalRuby.WeatherMaker
{
    [CreateAssetMenu(fileName = "WeatherMakerFullScreenFogProfile", menuName = "WeatherMaker/Full Screen Fog Profile", order = 6)]
    public class WeatherMakerFullScreenFogProfileScript : WeatherMakerFogProfileScript
    {
        [Header("Full screen fog - thresholds")]
        [Tooltip("Fog height. Set to 0 for unlimited height.")]
        [Range(0.0f, 5000.0f)]
        public float FogHeight = 0.0f;

        [Header("Full screen fog - screen space sun shafts")]
        [Tooltip("The number of sun shaft samples. Set to 0 to disable sun shafts.")]
        [Range(0, 100)]
        public int SunShaftSampleCount = 0;

        [Tooltip("Sun shaft down sample scale. Down samples camera buffer to this before rendering fog.")]
        [Range(0.01f, 1.0f)]
        public float SunShaftDownSampleScale = 0.5f;

        [Tooltip("Sun shaft spread (0 - 1).")]
        [Range(0.0f, 1.0f)]
        public float SunShaftSpread = 0.65f;

        [Tooltip("Increases the sun shaft brightness")]
        [Range(0.0f, 1.0f)]
        public float SunShaftBrightness = 0.075f;

        [Tooltip("Combined with each ray march, this determines how much light is accumulated each step.")]
        [Range(0.0f, 100.0f)]
        public float SunShaftStepMultiplier = 21.0f;

        [Tooltip("Determines light fall-off from start of sun shaft. Set to 1 for no fall-off.")]
        [Range(0.5f, 1.0f)]
        public float SunShaftDecay = 0.97f;

        [Tooltip("Sun shaft tint color. Alpha value determines tint intensity.")]
        public Color SunShaftTintColor = Color.white;

        [Tooltip("Controls dithering intensity of sun shafts.")]
        [Range(-1.0f, 1.0f)]
        public float SunShaftDither = 0.4f;

        [Tooltip("Controls dithering appearance of sun shafts.")]
        public Vector4 SunShaftDitherMagic = new Vector4(2.34325f, 5.235345f, 1024.0f, 1024.0f);

        /// <summary>
        /// Update full screen fog material properties
        /// </summary>
        /// <param name="material">Fog material</param>
        /// <param name="camera">Camera</param>
        public override void UpdateMaterialProperties(Material material, Camera camera)
        {
            base.UpdateMaterialProperties(material, camera);

            if (FogHeight > 0.0f)
            {
                material.SetFloat("_FogHeight", FogHeight);
                if (!material.IsKeywordEnabled("ENABLE_FOG_HEIGHT"))
                {
                    material.EnableKeyword("ENABLE_FOG_HEIGHT");
                }
            }
            else if (material.IsKeywordEnabled("ENABLE_FOG_HEIGHT"))
            {
                material.DisableKeyword("ENABLE_FOG_HEIGHT");
            }

            // if no sun, then no sun shafts
            if (SunShaftSampleCount > 0 && WeatherMakerScript.Instance.Sun.LightIsOn && WeatherMakerScript.Instance.Sun.ViewportPosition.z > 0.0f)
            {
                if (!material.IsKeywordEnabled("ENABLE_FOG_SUN_SHAFTS"))
                {
                    // Sun is visible
                    material.EnableKeyword("ENABLE_FOG_SUN_SHAFTS");
                }

                bool gamma = (QualitySettings.activeColorSpace == ColorSpace.Gamma);
                float brightness = SunShaftBrightness * (gamma ? 1.0f : 0.33f) * Mathf.Max(0.0f, 1.0f - (WeatherMakerScript.Instance.CloudScript.CloudProfile.CloudCoverTotal * 1.5f));
                material.SetVector("_FogSunShaftsParam1", new Vector4(SunShaftSpread / (float)SunShaftSampleCount, (float)SunShaftSampleCount, brightness, 1.0f / (float)SunShaftSampleCount));
                material.SetVector("_FogSunShaftsParam2", new Vector4(SunShaftStepMultiplier, SunShaftDecay, SunShaftDither, 0.0f));
                material.SetVector("_FogSunShaftsTintColor", new Vector4(SunShaftTintColor.r * SunShaftTintColor.a, SunShaftTintColor.g * SunShaftTintColor.a,
                    SunShaftTintColor.b * SunShaftTintColor.a, SunShaftTintColor.a));
                material.SetVector("_FogSunShaftsDitherMagic", SunShaftDitherMagic);
            }
            else if (material.IsKeywordEnabled("ENABLE_FOG_SUN_SHAFTS"))
            {
                // disable sun shafts
                material.DisableKeyword("ENABLE_FOG_SUN_SHAFTS");
            }
        }
    }
}
