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

namespace DigitalRuby.WeatherMaker
{
    [CreateAssetMenu(fileName = "WeatherMakerCloudProfile", menuName = "WeatherMaker/Cloud Profile", order = 3)]
    public class WeatherMakerCloudProfileScript : ScriptableObject
    {
        [Tooltip("The first, and lowest cloud layer, null for none")]
        public WeatherMakerCloudLayerProfileScript CloudLayer1;

        [Tooltip("The second, and second lowest cloud layer, null for none")]
        public WeatherMakerCloudLayerProfileScript CloudLayer2;

        [Tooltip("The third, and third lowest cloud layer, null for none")]
        public WeatherMakerCloudLayerProfileScript CloudLayer3;

        [Tooltip("The fourth, and highest cloud layer, null for none")]
        public WeatherMakerCloudLayerProfileScript CloudLayer4;

        [Tooltip("How much to multiply directional light intensities by when clouds are showing")]
        [Range(0.0f, 1.0f)]
        public float DirectionalLightIntensityMultiplier = 1.0f;

        [Tooltip("How much to multiply directional light shadow strengths by when clouds are showing")]
        [Range(0.0f, 1.0f)]
        public float DirectionalLightShadowStrengthMultiplier = 1.0f;

        private const float scaleReducer = 0.1f;

        /// <summary>
        /// Checks whether clouds are enabled
        /// </summary>
        public bool CloudsEnabled { get; private set; }

        /// <summary>
        /// Sum of cloud cover, max of 1
        /// </summary>
        public float CloudCoverTotal { get; private set; }

        /// <summary>
        /// Sum of cloud density, max of 1
        /// </summary>
        public float CloudDensityTotal { get; private set; }

        /// <summary>
        /// Sum of cloud light absorption, max of 1
        /// </summary>
        public float CloudLightAbsorptionTotal { get; private set; }

        /// <summary>
        /// A value of 0 to 1 that is a guide on how much to block the direct intensity of directional light, i.e. sun light reflecting off of water that makes the nice bright spots right in line of field of view to the sun
        /// </summary>
        public float CloudDirectionalLightDirectBlock { get; private set; }

        /// <summary>
        /// Whethe any of the cloud layers have shadows
        /// </summary>
        public bool CloudHasShadows { get; private set; }

        private Vector3 cloudNoiseVelocityAccum1;
        private Vector3 cloudNoiseVelocityAccum2;
        private Vector3 cloudNoiseVelocityAccum3;
        private Vector3 cloudNoiseVelocityAccum4;
        private Vector3 cloudNoiseMaskVelocityAccum1;
        private Vector3 cloudNoiseMaskVelocityAccum2;
        private Vector3 cloudNoiseMaskVelocityAccum3;
        private Vector3 cloudNoiseMaskVelocityAccum4;

        public void SetShaderCloudParameters(Material m)
        {
            m.DisableKeyword("ENABLE_CLOUDS");
            m.DisableKeyword("ENABLE_CLOUDS_MASK");

            if (CloudsEnabled)
            {
                m.SetTexture("_CloudNoise1", CloudLayer1.CloudNoise ?? Texture2D.blackTexture);
                m.SetTexture("_CloudNoise2", CloudLayer2.CloudNoise ?? Texture2D.blackTexture);
                m.SetTexture("_CloudNoise3", CloudLayer3.CloudNoise ?? Texture2D.blackTexture);
                m.SetTexture("_CloudNoise4", CloudLayer4.CloudNoise ?? Texture2D.blackTexture);

                //WeatherMakerShaderIds.SetFloatArray(m, "_CloudSampleCount", (float)CloudSampleCount, (float)CloudSampleCount2, (float)CloudSampleCount3, (float)CloudSampleCount4);
                //WeatherMakerShaderIds.SetVectorArray(m, "_CloudSampleStepMultiplier", CloudSampleStepMultiplier, CloudSampleStepMultiplier2, CloudSampleStepMultiplier3, CloudSampleStepMultiplier4);
                //WeatherMakerShaderIds.SetFloatArray(m, "_CloudSampleCount", CloudSampleCount, CloudSampleCount2, CloudSampleCount3, CloudSampleCount4);
                //WeatherMakerShaderIds.SetVectorArray(m, "_CloudSampleDitherMagic", CloudSampleDitherMagic, CloudSampleDitherMagic2, CloudSampleDitherMagic3, CloudSampleDitherMagic4);
                //WeatherMakerShaderIds.SetFloatArray(m, "_CloudSampleDitherIntensity", CloudSampleDitherIntensity.x, CloudSampleDitherIntensity.y, CloudSampleDitherIntensity.z, CloudSampleDitherIntensity.w);

                WeatherMakerShaderIds.SetColorArray(m, "_CloudColor",
                    CloudLayer1.CloudColor * WeatherMakerScript.EvaluateSunGradient(CloudLayer1.CloudGradientColor),
                    CloudLayer2.CloudColor * WeatherMakerScript.EvaluateSunGradient(CloudLayer2.CloudGradientColor),
                    CloudLayer3.CloudColor * WeatherMakerScript.EvaluateSunGradient(CloudLayer3.CloudGradientColor),
                    CloudLayer4.CloudColor * WeatherMakerScript.EvaluateSunGradient(CloudLayer4.CloudGradientColor));
                WeatherMakerShaderIds.SetColorArray(m, "_CloudEmissionColor",
                    CloudLayer1.CloudEmissionColor,
                    CloudLayer2.CloudEmissionColor,
                    CloudLayer3.CloudEmissionColor,
                    CloudLayer4.CloudEmissionColor);
                WeatherMakerShaderIds.SetFloatArray(m, "_CloudAmbientMultiplier",
                    CloudLayer1.CloudAmbientMultiplier,
                    CloudLayer2.CloudAmbientMultiplier,
                    CloudLayer3.CloudAmbientMultiplier,
                    CloudLayer4.CloudAmbientMultiplier);
                WeatherMakerShaderIds.SetVectorArray(m, "_CloudNoiseScale",
                    CloudLayer1.CloudNoiseScale * scaleReducer,
                    CloudLayer2.CloudNoiseScale * scaleReducer,
                    CloudLayer3.CloudNoiseScale * scaleReducer,
                    CloudLayer4.CloudNoiseScale * scaleReducer);
                WeatherMakerShaderIds.SetVectorArray(m, "_CloudNoiseMultiplier",
                    CloudLayer1.CloudNoiseMultiplier,
                    CloudLayer2.CloudNoiseMultiplier,
                    CloudLayer3.CloudNoiseMultiplier,
                    CloudLayer4.CloudNoiseMultiplier);
                float velMult = Time.deltaTime * 0.005f;
                WeatherMakerShaderIds.SetVectorArray(m, "_CloudNoiseVelocity",
                    (cloudNoiseVelocityAccum1 += (CloudLayer1.CloudNoiseVelocity * velMult)),
                    (cloudNoiseVelocityAccum2 += (CloudLayer2.CloudNoiseVelocity * velMult)),
                    (cloudNoiseVelocityAccum3 += (CloudLayer3.CloudNoiseVelocity * velMult)),
                    (cloudNoiseVelocityAccum4 += (CloudLayer4.CloudNoiseVelocity * velMult)));
                WeatherMakerShaderIds.SetFloatArrayRotation(m, "_CloudNoiseRotation",
                    CloudLayer1.CloudNoiseRotation.LastValue,
                    CloudLayer2.CloudNoiseRotation.LastValue,
                    CloudLayer3.CloudNoiseRotation.LastValue,
                    CloudLayer4.CloudNoiseRotation.LastValue);
                if (CloudLayer1.CloudNoiseMask == null && CloudLayer2.CloudNoiseMask == null && CloudLayer3.CloudNoiseMask == null && CloudLayer4.CloudNoiseMask == null)
                {
                    m.EnableKeyword("ENABLE_CLOUDS");
                }
                else
                {
                    m.EnableKeyword("ENABLE_CLOUDS_MASK");
                    m.SetTexture("_CloudNoiseMask1", CloudLayer1.CloudNoiseMask ?? Texture2D.whiteTexture);
                    m.SetTexture("_CloudNoiseMask2", CloudLayer2.CloudNoiseMask ?? Texture2D.whiteTexture);
                    m.SetTexture("_CloudNoiseMask3", CloudLayer3.CloudNoiseMask ?? Texture2D.whiteTexture);
                    m.SetTexture("_CloudNoiseMask4", CloudLayer4.CloudNoiseMask ?? Texture2D.whiteTexture);
                    WeatherMakerShaderIds.SetVectorArray(m, "_CloudNoiseMaskOffset",
                        CloudLayer1.CloudNoiseMaskOffset,
                        CloudLayer2.CloudNoiseMaskOffset,
                        CloudLayer3.CloudNoiseMaskOffset,
                        CloudLayer4.CloudNoiseMaskOffset);
                    WeatherMakerShaderIds.SetVectorArray(m, "_CloudNoiseMaskVelocity",
                        (cloudNoiseMaskVelocityAccum1 += (CloudLayer1.CloudNoiseMaskVelocity * velMult)),
                        (cloudNoiseMaskVelocityAccum2 += (CloudLayer2.CloudNoiseMaskVelocity * velMult)),
                        (cloudNoiseMaskVelocityAccum3 += (CloudLayer3.CloudNoiseMaskVelocity * velMult)),
                        (cloudNoiseMaskVelocityAccum4 += (CloudLayer4.CloudNoiseMaskVelocity * velMult)));
                    WeatherMakerShaderIds.SetFloatArray(m, "_CloudNoiseMaskScale",
                        CloudLayer1.CloudNoiseMaskScale * scaleReducer,
                        CloudLayer2.CloudNoiseMaskScale * scaleReducer,
                        CloudLayer3.CloudNoiseMaskScale * scaleReducer,
                        CloudLayer4.CloudNoiseMaskScale * scaleReducer);
                    WeatherMakerShaderIds.SetFloatArrayRotation(m, "_CloudNoiseMaskRotation",
                        CloudLayer1.CloudNoiseMaskRotation.LastValue,
                        CloudLayer2.CloudNoiseMaskRotation.LastValue,
                        CloudLayer3.CloudNoiseMaskRotation.LastValue,
                        CloudLayer4.CloudNoiseMaskRotation.LastValue);
                }
                WeatherMakerShaderIds.SetFloatArray(m, "_CloudHeight",
                    CloudLayer1.CloudHeight,
                    CloudLayer2.CloudHeight,
                    CloudLayer3.CloudHeight,
                    CloudLayer4.CloudHeight);
                WeatherMakerShaderIds.SetFloatArray(m, "_CloudCover",
                    CloudLayer1.CloudCover,
                    CloudLayer2.CloudCover,
                    CloudLayer3.CloudCover,
                    CloudLayer4.CloudCover);
                WeatherMakerShaderIds.SetFloatArray(m, "_CloudDensity",
                    CloudLayer1.CloudDensity,
                    CloudLayer2.CloudDensity,
                    CloudLayer3.CloudDensity,
                    CloudLayer4.CloudDensity);
                WeatherMakerShaderIds.SetFloatArray(m, "_CloudLightAbsorption",
                    CloudLayer1.CloudLightAbsorption,
                    CloudLayer2.CloudLightAbsorption,
                    CloudLayer3.CloudLightAbsorption,
                    CloudLayer4.CloudLightAbsorption);
                WeatherMakerShaderIds.SetFloatArray(m, "_CloudSharpness",
                    CloudLayer1.CloudSharpness,
                    CloudLayer2.CloudSharpness,
                    CloudLayer3.CloudSharpness,
                    CloudLayer4.CloudSharpness);
                WeatherMakerShaderIds.SetFloatArray(m, "_CloudShadowThreshold",
                    CloudLayer1.CloudShadowThreshold,
                    CloudLayer2.CloudShadowThreshold,
                    CloudLayer3.CloudShadowThreshold,
                    CloudLayer4.CloudShadowThreshold);
                float shadowDotPower = Mathf.Clamp(Mathf.Pow(3.0f * Vector3.Dot(Vector3.down, WeatherMakerScript.Instance.Sun.Transform.forward), 0.5f), 0.0f, 1.0f);
                float shadowPower1 = Mathf.Lerp(2.0f, CloudLayer1.CloudShadowPower, shadowDotPower);
                float shadowPower2 = Mathf.Lerp(2.0f, CloudLayer2.CloudShadowPower, shadowDotPower);
                float shadowPower3 = Mathf.Lerp(2.0f, CloudLayer3.CloudShadowPower, shadowDotPower);
                float shadowPower4 = Mathf.Lerp(2.0f, CloudLayer4.CloudShadowPower, shadowDotPower);
                WeatherMakerShaderIds.SetFloatArray(m, "_CloudShadowPower", shadowPower1, shadowPower2, shadowPower3, shadowPower4);
                WeatherMakerShaderIds.SetFloatArray(m, "_CloudRayOffset",
                    CloudLayer1.CloudRayOffset,
                    CloudLayer2.CloudRayOffset,
                    CloudLayer3.CloudRayOffset,
                    CloudLayer4.CloudRayOffset);

#if UNITY_EDITOR

                if (Application.isPlaying)
                {

#endif

                    float cover = CloudCoverTotal * (1.5f - CloudLightAbsorptionTotal);
                    float sunIntensityMultiplier = Mathf.Clamp(1.0f - (CloudDensityTotal * 0.85f), 0.0f, 1.0f);
                    WeatherMakerScript.Instance.DayNightScript.DirectionalLightIntensityMultipliers["WeatherMakerFullScreenCloudsScript"] = sunIntensityMultiplier * Mathf.Lerp(1.0f, DirectionalLightIntensityMultiplier, cover);
                    float sunShadowMultiplier = Mathf.Lerp(1.0f, 0.0f, Mathf.Clamp(((CloudDensityTotal + cover) * 0.85f), 0.0f, 1.0f));
                    WeatherMakerScript.Instance.DayNightScript.DirectionalLightShadowIntensityMultipliers["WeatherMakerFullScreenCloudsScript"] = sunShadowMultiplier * Mathf.Lerp(1.0f, DirectionalLightShadowStrengthMultiplier, cover);

#if UNITY_EDITOR

                }

#endif

            }
            else
            {

#if UNITY_EDITOR

                if (Application.isPlaying)
                {

#endif

                    WeatherMakerScript.Instance.DayNightScript.DirectionalLightIntensityMultipliers["WeatherMakerFullScreenCloudsScript"] = 1.0f;
                    WeatherMakerScript.Instance.DayNightScript.DirectionalLightShadowIntensityMultipliers["WeatherMakerFullScreenCloudsScript"] = 1.0f;

#if UNITY_EDITOR

                }

#endif

            }
        }

        private void LoadDefaultLayerIfNeeded(ref WeatherMakerCloudLayerProfileScript script)
        {
            if (script == null)
            {
                script = Resources.Load<WeatherMakerCloudLayerProfileScript>("WeatherMakerCloudLayerProfile_None");
            }
        }

        public void EnsureNonNullLayers()
        {
            LoadDefaultLayerIfNeeded(ref CloudLayer1);
            LoadDefaultLayerIfNeeded(ref CloudLayer2);
            LoadDefaultLayerIfNeeded(ref CloudLayer3);
            LoadDefaultLayerIfNeeded(ref CloudLayer4);
        }

        public WeatherMakerCloudProfileScript Clone()
        {
            WeatherMakerCloudProfileScript clone = ScriptableObject.Instantiate(this);
            clone.EnsureNonNullLayers();
            clone.CloudLayer1 = ScriptableObject.Instantiate(clone.CloudLayer1);
            clone.CloudLayer2 = ScriptableObject.Instantiate(clone.CloudLayer2);
            clone.CloudLayer3 = ScriptableObject.Instantiate(clone.CloudLayer3);
            clone.CloudLayer4 = ScriptableObject.Instantiate(clone.CloudLayer4);
            CopyStateTo(clone);
            return clone;
        }

        public void Update()
        {
            EnsureNonNullLayers();
            CloudsEnabled =
            (
                (CloudLayer1.CloudNoise != null && CloudLayer1.CloudColor.a > 0.0f && CloudLayer1.CloudCover > 0.0f) ||
                (CloudLayer2.CloudNoise != null && CloudLayer2.CloudColor.a > 0.0f && CloudLayer2.CloudCover > 0.0f) ||
                (CloudLayer3.CloudNoise != null && CloudLayer3.CloudColor.a > 0.0f && CloudLayer3.CloudCover > 0.0f) ||
                (CloudLayer4.CloudNoise != null && CloudLayer4.CloudColor.a > 0.0f && CloudLayer4.CloudCover > 0.0f)
            );
            CloudCoverTotal = Mathf.Min(1.0f, (CloudLayer1.CloudCover + CloudLayer2.CloudCover + CloudLayer3.CloudCover + CloudLayer4.CloudCover));
            CloudDensityTotal = Mathf.Min(1.0f,
                (CloudLayer1.CloudCover * CloudLayer1.CloudDensity) +
                (CloudLayer2.CloudCover * CloudLayer2.CloudDensity) +
                (CloudLayer3.CloudCover * CloudLayer1.CloudDensity) +
                (CloudLayer4.CloudCover * CloudLayer4.CloudDensity));
            CloudLightAbsorptionTotal = Mathf.Min(1.0f,
                (CloudLayer1.CloudCover * CloudLayer1.CloudLightAbsorption) +
                (CloudLayer2.CloudCover * CloudLayer2.CloudLightAbsorption) +
                (CloudLayer3.CloudCover * CloudLayer3.CloudLightAbsorption) +
                (CloudLayer4.CloudCover * CloudLayer4.CloudLightAbsorption));
            CloudDirectionalLightDirectBlock = Mathf.Min(1.0f, (CloudCoverTotal + CloudDensityTotal) * 1.2f);
            CloudHasShadows = CloudsEnabled &&
            (
                (CloudLayer1.CloudShadowThreshold < 0.999f && CloudLayer1.CloudShadowPower < 2.0f) ||
                (CloudLayer2.CloudShadowThreshold < 0.999f && CloudLayer2.CloudShadowPower < 2.0f) ||
                (CloudLayer3.CloudShadowThreshold < 0.999f && CloudLayer3.CloudShadowPower < 2.0f) ||
                (CloudLayer4.CloudShadowThreshold < 0.999f && CloudLayer4.CloudShadowPower < 2.0f)
            );
        }

        public void CopyStateTo(WeatherMakerCloudProfileScript other)
        {
            other.cloudNoiseVelocityAccum1 = this.cloudNoiseVelocityAccum1;
            other.cloudNoiseVelocityAccum2 = this.cloudNoiseVelocityAccum2;
            other.cloudNoiseVelocityAccum3 = this.cloudNoiseVelocityAccum3;
            other.cloudNoiseVelocityAccum4 = this.cloudNoiseVelocityAccum4;
            other.cloudNoiseMaskVelocityAccum1 = this.cloudNoiseMaskVelocityAccum1;
            other.cloudNoiseMaskVelocityAccum2 = this.cloudNoiseMaskVelocityAccum2;
            other.cloudNoiseMaskVelocityAccum3 = this.cloudNoiseMaskVelocityAccum3;
            other.cloudNoiseMaskVelocityAccum4 = this.cloudNoiseMaskVelocityAccum4;
            other.CloudCoverTotal = this.CloudCoverTotal;
            other.CloudDensityTotal = this.CloudDensityTotal;
            other.CloudLightAbsorptionTotal = this.CloudLightAbsorptionTotal;
            other.CloudDirectionalLightDirectBlock = this.CloudDirectionalLightDirectBlock;
            other.CloudsEnabled = this.CloudsEnabled;
            other.CloudHasShadows = this.CloudHasShadows;
        }
    }
}
