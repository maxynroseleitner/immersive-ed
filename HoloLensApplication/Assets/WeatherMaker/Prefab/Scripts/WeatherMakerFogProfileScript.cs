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
    [CreateAssetMenu(fileName = "WeatherMakerFogProfile", menuName = "WeatherMaker/Fog Profile", order = 5)]
    public class WeatherMakerFogProfileScript : ScriptableObject
    {
        [Header("Fog appearance")]
        [Tooltip("Fog mode")]
        public WeatherMakerFogMode FogMode = WeatherMakerFogMode.Exponential;

        [Tooltip("Fog density")]
        [Range(0.0f, 1.0f)]
        public float FogDensity = 0.0f;

        [Tooltip("Fog color")]
        public Color FogColor = Color.white;

        [Tooltip("Fog emission color, alpha is intensity")]
        public Color FogEmissionColor = Color.black;

        [Range(0.0f, 10.0f)]
        [Tooltip("Fog light absorption - lower values absorb more light, higher values scatter and intensify light more.")]
        public float FogLightAbsorption = 1.0f;

        [Tooltip("Whether to enable volumetric fog point/spot lights. Fog always uses directional lights. Disable to improve performance.")]
        public bool EnableFogLights = true;

        [Tooltip("Maximum fog factor, where 1 is the maximum allowed fog.")]
        [Range(0.0f, 1.0f)]
        public float MaxFogFactor = 1.0f;

        [Header("Fog noise")]
        [Tooltip("Fog noise scale. Lower values get less tiling. 0 to disable noise.")]
        [Range(0.0f, 1.0f)]
        public float FogNoiseScale = 0.01f;

        [Tooltip("Controls how the noise value is calculated. Negative values allow areas of no noise, higher values increase the intensity of the noise.")]
        [Range(-1.0f, 1.0f)]
        public float FogNoiseAdder = 0.0f;

        [Tooltip("How much the noise effects the fog.")]
        [Range(0.0f, 10.0f)]
        public float FogNoiseMultiplier = 0.0f;

        [Tooltip("Fog noise velocity, determines how fast the fog moves. Not all fog scripts support 3d velocity, some only support 2d velocity (x and y).")]
        public Vector3 FogNoiseVelocity = new Vector3(0.01f, 0.01f, 0.0f);
        private Vector3 fogNoiseVelocityAccum;

        [Tooltip("Number of samples to take for 3D fog. If the player will never enter the fog, this can be a lower value. If the player can move through the fog, 40 or higher is better, but will cost some performance.")]
        [Range(1, 100)]
        public int FogNoiseSampleCount = 40;

        [Tooltip("Dithering level. 0 to disable dithering.")]
        [Range(0.0f, 1.0f)]
        public float DitherLevel = 0.005f;

        [Header("Fog shadows (sun only)")]
        [Tooltip("Number of shadow samples, 0 to disable fog shadows. Requires EnableFogLights be true.")]
        [Range(0, 500)]
        public int FogShadowSampleCount = 0;

        [Tooltip("Max ray length for fog shadows.")]
        [Range(10.0f, 1000.0f)]
        public float FogShadowMaxRayLength = 300.0f;

        [Tooltip("Multiplier for fog shadow lighting. Higher values make brighter light rays.")]
        [Range(0.0f, 32.0f)]
        public float FogShadowMultiplier = 8.0f;

        [Tooltip("Controls how light falls off from the light source. Higher values fall off faster. Setting this to a value that is a power of two is recommended.")]
        [Range(0.0f, 128.0f)]
        public float FogShadowPower = 64.0f;

        [Tooltip("Controls brightness of light in the fog vs in shadow.")]
        [Range(0.01f, 10.0f)]
        public float FogShadowBrightness = 1.0f;

        [Tooltip("Controls how light falls off from the light source. Lower values fall off faster.")]
        [Range(0.0f, 1.0f)]
        public float FogShadowDecay = 0.95f;

        [Tooltip("Fog shadow dither multiplier. Higher values dither more.")]
        [Range(0.0f, 3.0f)]
        public float FogShadowDither = 0.4f;

        [Tooltip("Magic numbers for fog shadow dithering. Tweak if you don't like the dithering appearance.")]
        public Vector4 FogShadowDitherMagic = new Vector4(0.73f, 1.665f, 1024.0f, 1024.0f);

        [Header("Fog volume percentage")]
        [Tooltip("Percentage of bounding volume (if any) that fog should fill - this is dependant on the type of volume and is ignored for full screen fog.")]
        [Range(0.01f, 1.0f)]
        public float FogPercentage = 0.9f;

        /// <summary>
        /// Density of fog for scattering reduction
        /// </summary>
        private float fogScatterReduction = 1.0f;
        public float FogScatterReduction { get { return fogScatterReduction; } }

        /// <summary>
        /// Update a fog material with fog properties from this object
        /// </summary>
        /// <param name="material">Fog material</param>
        /// <param name="camera">Camera</param>
        public virtual void UpdateMaterialProperties(Material material, Camera camera)
        {
            bool gamma = (QualitySettings.activeColorSpace == ColorSpace.Gamma);
            float scatterCover = (WeatherMakerScript.Instance.CloudScript.enabled && WeatherMakerScript.Instance.CloudScript.CloudProfile != null ? WeatherMakerScript.Instance.CloudScript.CloudProfile.CloudCoverTotal : 0.0f);
            material.SetColor("_FogColor", FogColor);
            Color tmp = FogEmissionColor * FogEmissionColor.a;
            tmp.a = FogEmissionColor.a;
            material.SetColor("_FogEmissionColor", tmp);
            material.SetFloat("_FogLightAbsorption", FogLightAbsorption);
            material.SetFloat("_FogNoiseScale", FogNoiseScale);
            material.SetFloat("_FogNoiseAdder", FogNoiseAdder);
            material.SetFloat("_FogNoiseMultiplier", FogNoiseMultiplier);
            material.SetVector("_FogNoiseVelocity", (fogNoiseVelocityAccum += (FogNoiseVelocity * Time.deltaTime * 0.005f)));
            material.SetFloat("_FogNoiseSampleCount", (float)FogNoiseSampleCount);
            material.SetFloat("_FogNoiseSampleCountInverse", 1.0f / (float)FogNoiseSampleCount);
            material.SetFloat("_MaxFogFactor", MaxFogFactor);
            if (material.IsKeywordEnabled("WEATHER_MAKER_FOG_CONSTANT"))
            {
                material.DisableKeyword("WEATHER_MAKER_FOG_CONSTANT");
            }
            else if (material.IsKeywordEnabled("WEATHER_MAKER_FOG_EXPONENTIAL"))
            {
                material.DisableKeyword("WEATHER_MAKER_FOG_EXPONENTIAL");
            }
            else if (material.IsKeywordEnabled("WEATHER_MAKER_FOG_LINEAR"))
            {
                material.DisableKeyword("WEATHER_MAKER_FOG_LINEAR");
            }
            else if (material.IsKeywordEnabled("WEATHER_MAKER_FOG_EXPONENTIAL_SQUARED"))
            {
                material.DisableKeyword("WEATHER_MAKER_FOG_EXPONENTIAL_SQUARED");
            }
            if (FogMode == WeatherMakerFogMode.None || FogDensity <= 0.0f || MaxFogFactor <= 0.001f)
            {
                fogScatterReduction = 1.0f;
            }
            else if (FogMode == WeatherMakerFogMode.Exponential)
            {
                material.EnableKeyword("WEATHER_MAKER_FOG_EXPONENTIAL");
                material.SetFloat("_FogDensityScatter", fogScatterReduction = Mathf.Clamp(1.0f - ((FogDensity + scatterCover) * 1.5f), 0.0f, 1.0f));
            }
            else if (FogMode == WeatherMakerFogMode.Linear)
            {
                material.EnableKeyword("WEATHER_MAKER_FOG_LINEAR");
                material.SetFloat("_FogDensityScatter", fogScatterReduction = Mathf.Clamp((1.0f - ((FogDensity + scatterCover) * 1.2f)), 0.0f, 1.0f));
            }
            else if (FogMode == WeatherMakerFogMode.ExponentialSquared)
            {
                material.EnableKeyword("WEATHER_MAKER_FOG_EXPONENTIAL_SQUARED");
                material.SetFloat("_FogDensityScatter", fogScatterReduction = Mathf.Clamp((1.0f - ((FogDensity + scatterCover) * 2.0f)), 0.0f, 1.0f));
            }
            else if (FogMode == WeatherMakerFogMode.Constant)
            {
                material.EnableKeyword("WEATHER_MAKER_FOG_CONSTANT");
                material.SetFloat("_FogDensityScatter", fogScatterReduction = Mathf.Clamp(1.0f - (FogDensity + scatterCover), 0.0f, 1.0f));
            }
            if (FogNoiseScale > 0.0f && FogNoiseMultiplier > 0.0f && WeatherMakerLightManagerScript.NoiseTexture3DInstance != null)
            {
                if (!material.IsKeywordEnabled("ENABLE_FOG_NOISE"))
                {
                    material.EnableKeyword("ENABLE_FOG_NOISE");
                }
            }
            else if (material.IsKeywordEnabled("ENABLE_FOG_NOISE"))
            {
                material.DisableKeyword("ENABLE_FOG_NOISE");
            }
            if (EnableFogLights)
            {
                if (FogShadowSampleCount > 0 && WeatherMakerScript.Instance.Sun.Light.intensity > 0.0f)
                {
                    float brightness = Mathf.Lerp(0.0f, 1.0f, (WeatherMakerScript.Instance.Sun.Light.intensity - 0.6f) / 0.4f);
                    material.DisableKeyword("ENABLE_FOG_LIGHTS");
                    material.EnableKeyword("ENABLE_FOG_LIGHTS_WITH_SHADOWS");
                    material.SetFloat("_FogLightShadowSampleCount", (float)FogShadowSampleCount);
                    material.SetFloat("_FogLightShadowInvSampleCount", 1.0f / (float)FogShadowSampleCount);
                    material.SetFloat("_FogLightShadowMaxRayLength", FogShadowMaxRayLength);
                    material.SetFloat("_FogLightShadowMultiplier", FogShadowMultiplier);
                    material.SetFloat("_FogLightShadowPower", FogShadowPower);
                    material.SetFloat("_FogLightShadowBrightness", brightness * FogShadowBrightness);
                    material.SetFloat("_FogLightShadowDecay", FogShadowDecay);
                    material.SetFloat("_FogLightShadowDither", FogShadowDither);
                    material.SetVector("_FogLightShadowDitherMagic", FogShadowDitherMagic);
                    if (QualitySettings.shadowCascades < 2)
                    {
                        material.EnableKeyword("SHADOWS_ONE_CASCADE");
                    }
                    else
                    {
                        material.DisableKeyword("SHADOWS_ONE_CASCADE");
                    }
                }
                else
                {
                    material.DisableKeyword("ENABLE_FOG_LIGHTS_WITH_SHADOWS");
                    material.EnableKeyword("ENABLE_FOG_LIGHTS");
                }
            }
            else
            {
                material.DisableKeyword("ENABLE_FOG_LIGHTS");
                material.DisableKeyword("ENABLE_FOG_LIGHTS_WITH_SHADOWS");
            }
            material.SetFloat("_FogDitherLevel", (gamma ? DitherLevel : DitherLevel * 0.5f));
            material.SetFloat("_FogDensity", FogDensity);
        }
    }

    public enum WeatherMakerFogMode
    {
        None,
        Constant,
        Linear,
        Exponential,
        ExponentialSquared
    }
}
