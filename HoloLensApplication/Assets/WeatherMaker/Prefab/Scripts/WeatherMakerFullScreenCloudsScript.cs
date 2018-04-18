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

// uncomment to test cloud animations in editor mode
// #define ANIMATE_EDITOR_CLOUD_PROFILE_CHANGES

using UnityEngine;
using UnityEngine.Rendering;

using System;
using System.Collections.Generic;

namespace DigitalRuby.WeatherMaker
{
    public class WeatherMakerFullScreenCloudsScript : MonoBehaviour
    {
        [Header("Full Screen Clouds - Rendering")]
        [Tooltip("Cloud profile")]
        [SerializeField]
        private WeatherMakerCloudProfileScript _CloudProfile;
        private WeatherMakerCloudProfileScript currentRenderCloudProfile;
        private bool currentRenderCloudProfileIsTemporary;

        [Tooltip("Cloud shadow projector script")]
        public WeatherMakerCloudShadowProjectorScript CloudShadowProjectorScript;

        [Tooltip("Cloud rendering material.")]
        public Material Material;

        [Tooltip("Material to blit the full screen clouds.")]
        public Material FullScreenMaterial;

        [Tooltip("Down sample scale.")]
        [Range(0.01f, 1.0f)]
        public float DownSampleScale = 1.0f;

        [Tooltip("Blur Material.")]
        public Material BlurMaterial;

        [Tooltip("Blur Shader Type.")]
        public BlurShaderType BlurShader;

        [Tooltip("Render Queue")]
        public CameraEvent RenderQueue = CameraEvent.BeforeImageEffectsOpaque;

        private WeatherMakerCloudProfileScript lastProfile;
        public WeatherMakerCloudProfileScript CloudProfile
        {
            get { return _CloudProfile; }
            set { ShowCloudsAnimated(5.0f, value); }
        }

        [Tooltip("Used to block lens flare if clouds are over the sun. Just needs to be a sphere collider.")]
        public GameObject CloudLensFlareBlocker;

        private WeatherMakerFullScreenEffect effect;
        private System.Action<WeatherMakerCommandBuffer> updateShaderPropertiesAction;
        private static WeatherMakerCloudProfileScript emptyProfile;

        private void UpdateShaderProperties(WeatherMakerCommandBuffer b)
        {
            SetShaderCloudParameters(b.Material);
        }

        private void UpdateLensFlare(Camera c)
        {
            if (WeatherMakerScript.Instance.Sun == null || CloudProfile == null)
            {
                return;
            }
            LensFlare flare = WeatherMakerScript.Instance.Sun.Transform.GetComponent<LensFlare>();
            if (flare == null)
            {
                return;
            }
            if (CloudProfile.CloudCoverTotal < 0.5f)
            {
                CloudLensFlareBlocker.SetActive(false);
            }
            else
            {
                CloudLensFlareBlocker.SetActive(true);
                Vector3 toSun = (c.transform.position - WeatherMakerScript.Instance.Sun.Transform.position).normalized;
                CloudLensFlareBlocker.transform.position = c.transform.position + (toSun * 16.0f);
            }
        }

        private void DeleteAndTransitionRenderProfile(WeatherMakerCloudProfileScript newProfile)
        {
            if (currentRenderCloudProfile != null)
            {
                if (newProfile != null)
                {
                    currentRenderCloudProfile.CopyStateTo(newProfile);
                }
                if (currentRenderCloudProfileIsTemporary)
                {
                    GameObject.Destroy(currentRenderCloudProfile.CloudLayer1);
                    GameObject.Destroy(currentRenderCloudProfile.CloudLayer2);
                    GameObject.Destroy(currentRenderCloudProfile.CloudLayer3);
                    GameObject.Destroy(currentRenderCloudProfile.CloudLayer4);
                    GameObject.Destroy(currentRenderCloudProfile);
                    currentRenderCloudProfileIsTemporary = false;
                }
            }
            currentRenderCloudProfile = _CloudProfile = lastProfile = newProfile;
        }

        private void EnsureProfile()
        {
            if (emptyProfile == null)
            {
                emptyProfile = Resources.Load<WeatherMakerCloudProfileScript>("WeatherMakerCloudProfile_None");
                emptyProfile = emptyProfile.Clone();
            }
            if (_CloudProfile == null)
            {
                _CloudProfile = emptyProfile;
            }
            if (effect == null)
            {
                lastProfile = currentRenderCloudProfile = _CloudProfile;
                currentRenderCloudProfileIsTemporary = false;
                updateShaderPropertiesAction = UpdateShaderProperties;
                effect = new WeatherMakerFullScreenEffect
                {
                    CommandBufferName = "WeatherMakerFullScreenCloudsScript",
                    DownsampleRenderBufferTextureName = "_MainTex2",
                    RenderQueue = RenderQueue,
                    ZTest = CompareFunction.LessEqual
                };
            }
        }

        private void Start()
        {
            EnsureProfile();
        }

        private void LateUpdate()
        {
            if (CloudProfile != lastProfile)
            {
                CloudProfile.EnsureNonNullLayers();
                DeleteAndTransitionRenderProfile(CloudProfile);
            }
            currentRenderCloudProfile.Update();
            effect.SetupEffect(Material, FullScreenMaterial, BlurMaterial, BlurShader, DownSampleScale, 0.0f, updateShaderPropertiesAction, currentRenderCloudProfile.CloudsEnabled);
        }

        private void OnEnable()
        {
            EnsureProfile();
        }

        private void OnDestroy()
        {
            if (effect != null)
            {
                effect.Dispose();
            }
            DeleteAndTransitionRenderProfile(null);
        }

        private void OnDisable()
        {
            if (effect != null)
            {
                effect.Dispose();
            }
        }

        internal void SetShaderCloudParameters(Material m)
        {
            currentRenderCloudProfile.SetShaderCloudParameters(m);
        }

        public void PreCullCamera(Camera camera)
        {
            if (effect != null)
            {
                effect.SetupCamera(camera);
            }

#if UNITY_EDITOR

            if (Application.isPlaying)
            {

#endif

                UpdateLensFlare(camera);
                CloudShadowProjectorScript.RenderCloudShadows(camera);

#if UNITY_EDITOR

            }

#endif

        }

        /// <summary>
        /// Show clouds animated. Animates cover, density, sharpness, light absorption and color. To ensure smooth animations, all noise textures on all layers in both profiles should match.
        /// </summary>
        /// <param name="duration">Transition duration in seconds</param>
        /// <param name="profileName">Cloud profile name</param>
        public void ShowCloudsAnimated(float duration, string profileName)
        {
            ShowCloudsAnimated(duration, Resources.Load<WeatherMakerCloudProfileScript>(profileName));
        }

        /// <summary>
        /// Show clouds animated. Animates cover, density, sharpness, light absorption and color. To ensure smooth animations, all noise textures on all layers in both profiles should match.
        /// </summary>
        /// <param name="duration">Transition duration in seconds</param>
        /// <param name="newProfile">Cloud profile, or pass null to hide clouds</param>
        public void ShowCloudsAnimated(float duration, WeatherMakerCloudProfileScript newProfile)
        {
            if (!isActiveAndEnabled || currentRenderCloudProfile == null)
            {
                Debug.LogError("Full screen cloud script must be enabled to show clouds");
                return;
            }

            WeatherMakerCloudProfileScript oldProfile = currentRenderCloudProfile;

            // dynamic start and end properties
            float endCover1, endCover2, endCover3, endCover4;
            float endDensity1, endDensity2, endDensity3, endDensity4;
            float startSharpness1, startSharpness2, startSharpness3, startSharpness4;
            float endSharpness1, endSharpness2, endSharpness3, endSharpness4;
            float startLightAbsorption1, startLightAbsorption2, startLightAbsorption3, startLightAbsorption4;
            float endLightAbsorption1, endLightAbsorption2, endLightAbsorption3, endLightAbsorption4;
            Vector4 startScale1, startScale2, startScale3, startScale4;
            Vector4 endScale1, endScale2, endScale3, endScale4;
            Vector4 startMultiplier1, startMultiplier2, startMultiplier3, startMultiplier4;
            Vector4 endMultiplier1, endMultiplier2, endMultiplier3, endMultiplier4;
            float startMaskScale1, startMaskScale2, startMaskScale3, startMaskScale4;
            float endMaskScale1, endMaskScale2, endMaskScale3, endMaskScale4;
            float startHeight1, startHeight2, startHeight3, startHeight4;
            float endHeight1, endHeight2, endHeight3, endHeight4;

            // set to empty profile if null passed in
            if (newProfile == null)
            {
                newProfile = emptyProfile;
            }
            newProfile.EnsureNonNullLayers();

            // apply end cover and density
            endCover1 = newProfile.CloudLayer1.CloudCover;
            endCover2 = newProfile.CloudLayer2.CloudCover;
            endCover3 = newProfile.CloudLayer3.CloudCover;
            endCover4 = newProfile.CloudLayer4.CloudCover;
            endDensity1 = newProfile.CloudLayer1.CloudDensity;
            endDensity2 = newProfile.CloudLayer2.CloudDensity;
            endDensity3 = newProfile.CloudLayer3.CloudDensity;
            endDensity4 = newProfile.CloudLayer4.CloudDensity;

            float startCover1 = oldProfile.CloudLayer1.CloudCover;
            float startCover2 = oldProfile.CloudLayer2.CloudCover;
            float startCover3 = oldProfile.CloudLayer3.CloudCover;
            float startCover4 = oldProfile.CloudLayer4.CloudCover;
            float startDensity1 = oldProfile.CloudLayer1.CloudDensity;
            float startDensity2 = oldProfile.CloudLayer2.CloudDensity;
            float startDensity3 = oldProfile.CloudLayer3.CloudDensity;
            float startDensity4 = oldProfile.CloudLayer4.CloudDensity;
            Color startColor1 = oldProfile.CloudLayer1.CloudColor;
            Color startColor2 = oldProfile.CloudLayer2.CloudColor;
            Color startColor3 = oldProfile.CloudLayer3.CloudColor;
            Color startColor4 = oldProfile.CloudLayer4.CloudColor;
            Color startEmissionColor1 = oldProfile.CloudLayer1.CloudEmissionColor;
            Color startEmissionColor2 = oldProfile.CloudLayer2.CloudEmissionColor;
            Color startEmissionColor3 = oldProfile.CloudLayer3.CloudEmissionColor;
            Color startEmissionColor4 = oldProfile.CloudLayer4.CloudEmissionColor;
            float startAmbientMultiplier1 = oldProfile.CloudLayer1.CloudAmbientMultiplier;
            float startAmbientMultiplier2 = oldProfile.CloudLayer2.CloudAmbientMultiplier;
            float startAmbientMultiplier3 = oldProfile.CloudLayer3.CloudAmbientMultiplier;
            float startAmbientMultiplier4 = oldProfile.CloudLayer4.CloudAmbientMultiplier;
            Vector4 startVelocity1 = oldProfile.CloudLayer1.CloudNoiseVelocity;
            Vector4 startVelocity2 = oldProfile.CloudLayer2.CloudNoiseVelocity;
            Vector4 startVelocity3 = oldProfile.CloudLayer3.CloudNoiseVelocity;
            Vector4 startVelocity4 = oldProfile.CloudLayer4.CloudNoiseVelocity;
            Vector4 startMaskVelocity1 = oldProfile.CloudLayer1.CloudNoiseMaskVelocity;
            Vector4 startMaskVelocity2 = oldProfile.CloudLayer2.CloudNoiseMaskVelocity;
            Vector4 startMaskVelocity3 = oldProfile.CloudLayer3.CloudNoiseMaskVelocity;
            Vector4 startMaskVelocity4 = oldProfile.CloudLayer4.CloudNoiseMaskVelocity;
            Vector4 startRotation = new Vector4(oldProfile.CloudLayer1.CloudNoiseRotation.LastValue, oldProfile.CloudLayer2.CloudNoiseRotation.LastValue, oldProfile.CloudLayer3.CloudNoiseRotation.LastValue, oldProfile.CloudLayer4.CloudNoiseRotation.LastValue);
            Vector4 startMaskRotation = new Vector4(oldProfile.CloudLayer1.CloudNoiseMaskRotation.LastValue, oldProfile.CloudLayer2.CloudNoiseMaskRotation.LastValue, oldProfile.CloudLayer3.CloudNoiseMaskRotation.LastValue, oldProfile.CloudLayer4.CloudNoiseMaskRotation.LastValue);
            float startDirectionalLightIntensityMultiplier = oldProfile.DirectionalLightIntensityMultiplier;
            float startDirectionalLightShadowStrengthMultiplier = oldProfile.DirectionalLightShadowStrengthMultiplier;

            Color endColor1 = newProfile.CloudLayer1.CloudColor;
            Color endColor2 = newProfile.CloudLayer2.CloudColor;
            Color endColor3 = newProfile.CloudLayer3.CloudColor;
            Color endColor4 = newProfile.CloudLayer4.CloudColor;
            Color endEmissionColor1 = newProfile.CloudLayer1.CloudEmissionColor;
            Color endEmissionColor2 = newProfile.CloudLayer2.CloudEmissionColor;
            Color endEmissionColor3 = newProfile.CloudLayer3.CloudEmissionColor;
            Color endEmissionColor4 = newProfile.CloudLayer4.CloudEmissionColor;
            float endAmbientMultiplier1 = newProfile.CloudLayer1.CloudAmbientMultiplier;
            float endAmbientMultiplier2 = newProfile.CloudLayer2.CloudAmbientMultiplier;
            float endAmbientMultiplier3 = newProfile.CloudLayer3.CloudAmbientMultiplier;
            float endAmbientMultiplier4 = newProfile.CloudLayer4.CloudAmbientMultiplier;
            Vector3 endVelocity1 = newProfile.CloudLayer1.CloudNoiseVelocity;
            Vector3 endVelocity2 = newProfile.CloudLayer2.CloudNoiseVelocity;
            Vector3 endVelocity3 = newProfile.CloudLayer3.CloudNoiseVelocity;
            Vector3 endVelocity4 = newProfile.CloudLayer4.CloudNoiseVelocity;
            Vector3 endMaskVelocity1 = newProfile.CloudLayer1.CloudNoiseMaskVelocity;
            Vector3 endMaskVelocity2 = newProfile.CloudLayer2.CloudNoiseMaskVelocity;
            Vector3 endMaskVelocity3 = newProfile.CloudLayer3.CloudNoiseMaskVelocity;
            Vector3 endMaskVelocity4 = newProfile.CloudLayer4.CloudNoiseMaskVelocity;
            Vector4 endRotation = new Vector4(newProfile.CloudLayer1.CloudNoiseRotation.Random(), newProfile.CloudLayer2.CloudNoiseRotation.Random(), newProfile.CloudLayer3.CloudNoiseRotation.Random(), newProfile.CloudLayer4.CloudNoiseRotation.Random());
            Vector4 endMaskRotation = new Vector4(newProfile.CloudLayer1.CloudNoiseMaskRotation.Random(), newProfile.CloudLayer2.CloudNoiseMaskRotation.Random(), newProfile.CloudLayer3.CloudNoiseMaskRotation.Random(), newProfile.CloudLayer4.CloudNoiseMaskRotation.Random());
            float endDirectionalLightIntensityMultiplier = newProfile.DirectionalLightIntensityMultiplier;
            float endDirectionalLightShadowStrengthMultiplier = newProfile.DirectionalLightShadowStrengthMultiplier;

            if (startCover1 == 0.0f && startCover2 == 0.0f && startCover3 == 0.0f && startCover4 == 0.0f)
            {
                // transition from no clouds to clouds, start at new profile values
                startSharpness1 = endSharpness1 = newProfile.CloudLayer1.CloudSharpness;
                startSharpness2 = endSharpness2 = newProfile.CloudLayer2.CloudSharpness;
                startSharpness3 = endSharpness3 = newProfile.CloudLayer3.CloudSharpness;
                startSharpness4 = endSharpness4 = newProfile.CloudLayer4.CloudSharpness;
                startLightAbsorption1 = endLightAbsorption1 = newProfile.CloudLayer1.CloudLightAbsorption;
                startLightAbsorption2 = endLightAbsorption2 = newProfile.CloudLayer2.CloudLightAbsorption;
                startLightAbsorption3 = endLightAbsorption3 = newProfile.CloudLayer3.CloudLightAbsorption;
                startLightAbsorption4 = endLightAbsorption4 = newProfile.CloudLayer4.CloudLightAbsorption;
                startScale1 = endScale1 = newProfile.CloudLayer1.CloudNoiseScale;
                startScale2 = endScale2 = newProfile.CloudLayer2.CloudNoiseScale;
                startScale3 = endScale3 = newProfile.CloudLayer3.CloudNoiseScale;
                startScale4 = endScale4 = newProfile.CloudLayer4.CloudNoiseScale;
                startMultiplier1 = endMultiplier1 = newProfile.CloudLayer1.CloudNoiseMultiplier;
                startMultiplier2 = endMultiplier2 = newProfile.CloudLayer2.CloudNoiseMultiplier;
                startMultiplier3 = endMultiplier3 = newProfile.CloudLayer3.CloudNoiseMultiplier;
                startMultiplier4 = endMultiplier4 = newProfile.CloudLayer4.CloudNoiseMultiplier;
                startMaskScale1 = endMaskScale1 = newProfile.CloudLayer1.CloudNoiseMaskScale;
                startMaskScale2 = endMaskScale2 = newProfile.CloudLayer2.CloudNoiseMaskScale;
                startMaskScale3 = endMaskScale3 = newProfile.CloudLayer3.CloudNoiseMaskScale;
                startMaskScale4 = endMaskScale4 = newProfile.CloudLayer4.CloudNoiseMaskScale;
                startHeight1 = endHeight1 = newProfile.CloudLayer1.CloudHeight;
                startHeight2 = endHeight2 = newProfile.CloudLayer2.CloudHeight;
                startHeight3 = endHeight3 = newProfile.CloudLayer3.CloudHeight;
                startHeight4 = endHeight4 = newProfile.CloudLayer4.CloudHeight;
                _CloudProfile = newProfile;
            }
            else if (endCover1 == 0.0f && endCover2 == 0.0f && endCover3 == 0.0f && endCover4 == 0.0f)
            {
                // transition from clouds to no clouds, start at old profile values
                startSharpness1 = endSharpness1 = oldProfile.CloudLayer1.CloudSharpness;
                startSharpness2 = endSharpness2 = oldProfile.CloudLayer2.CloudSharpness;
                startSharpness3 = endSharpness3 = oldProfile.CloudLayer3.CloudSharpness;
                startSharpness4 = endSharpness4 = oldProfile.CloudLayer4.CloudSharpness;
                startLightAbsorption1 = endLightAbsorption1 = oldProfile.CloudLayer1.CloudLightAbsorption;
                startLightAbsorption2 = endLightAbsorption2 = oldProfile.CloudLayer2.CloudLightAbsorption;
                startLightAbsorption3 = endLightAbsorption3 = oldProfile.CloudLayer3.CloudLightAbsorption;
                startLightAbsorption4 = endLightAbsorption4 = oldProfile.CloudLayer4.CloudLightAbsorption;
                startScale1 = endScale1 = oldProfile.CloudLayer1.CloudNoiseScale;
                startScale2 = endScale2 = oldProfile.CloudLayer2.CloudNoiseScale;
                startScale3 = endScale3 = oldProfile.CloudLayer3.CloudNoiseScale;
                startScale4 = endScale4 = oldProfile.CloudLayer4.CloudNoiseScale;
                startMultiplier1 = endMultiplier1 = oldProfile.CloudLayer1.CloudNoiseMultiplier;
                startMultiplier2 = endMultiplier2 = oldProfile.CloudLayer2.CloudNoiseMultiplier;
                startMultiplier3 = endMultiplier3 = oldProfile.CloudLayer3.CloudNoiseMultiplier;
                startMultiplier4 = endMultiplier4 = oldProfile.CloudLayer4.CloudNoiseMultiplier;
                startMaskScale1 = endMaskScale1 = oldProfile.CloudLayer1.CloudNoiseMaskScale;
                startMaskScale2 = endMaskScale2 = oldProfile.CloudLayer2.CloudNoiseMaskScale;
                startMaskScale3 = endMaskScale3 = oldProfile.CloudLayer3.CloudNoiseMaskScale;
                startMaskScale4 = endMaskScale4 = oldProfile.CloudLayer4.CloudNoiseMaskScale;
                startHeight1 = endHeight1 = oldProfile.CloudLayer1.CloudHeight;
                startHeight2 = endHeight2 = oldProfile.CloudLayer2.CloudHeight;
                startHeight3 = endHeight3 = oldProfile.CloudLayer3.CloudHeight;
                startHeight4 = endHeight4 = oldProfile.CloudLayer4.CloudHeight;

                // transition with old profile, the new profile is empty and can't be used for transition
                _CloudProfile = oldProfile;
            }
            else
            {
                // regular transition from one clouds to another, transition normally
                startScale1 = oldProfile.CloudLayer1.CloudNoiseScale;
                startScale2 = oldProfile.CloudLayer2.CloudNoiseScale;
                startScale3 = oldProfile.CloudLayer3.CloudNoiseScale;
                startScale4 = oldProfile.CloudLayer4.CloudNoiseScale;
                endScale1 = newProfile.CloudLayer1.CloudNoiseScale;
                endScale2 = newProfile.CloudLayer2.CloudNoiseScale;
                endScale3 = newProfile.CloudLayer3.CloudNoiseScale;
                endScale4 = newProfile.CloudLayer4.CloudNoiseScale;
                startMultiplier1 = oldProfile.CloudLayer1.CloudNoiseMultiplier;
                startMultiplier2 = oldProfile.CloudLayer2.CloudNoiseMultiplier;
                startMultiplier3 = oldProfile.CloudLayer3.CloudNoiseMultiplier;
                startMultiplier4 = oldProfile.CloudLayer4.CloudNoiseMultiplier;
                endMultiplier1 = newProfile.CloudLayer1.CloudNoiseMultiplier;
                endMultiplier2 = newProfile.CloudLayer2.CloudNoiseMultiplier;
                endMultiplier3 = newProfile.CloudLayer3.CloudNoiseMultiplier;
                endMultiplier4 = newProfile.CloudLayer4.CloudNoiseMultiplier;
                startMaskScale1 = oldProfile.CloudLayer1.CloudNoiseMaskScale;
                startMaskScale2 = oldProfile.CloudLayer2.CloudNoiseMaskScale;
                startMaskScale3 = oldProfile.CloudLayer3.CloudNoiseMaskScale;
                startMaskScale4 = oldProfile.CloudLayer4.CloudNoiseMaskScale;
                endMaskScale1 = newProfile.CloudLayer1.CloudNoiseMaskScale;
                endMaskScale2 = newProfile.CloudLayer2.CloudNoiseMaskScale;
                endMaskScale3 = newProfile.CloudLayer3.CloudNoiseMaskScale;
                endMaskScale4 = newProfile.CloudLayer4.CloudNoiseMaskScale;
                startSharpness1 = oldProfile.CloudLayer1.CloudSharpness;
                startSharpness2 = oldProfile.CloudLayer2.CloudSharpness;
                startSharpness3 = oldProfile.CloudLayer3.CloudSharpness;
                startSharpness4 = oldProfile.CloudLayer4.CloudSharpness;
                endSharpness1 = newProfile.CloudLayer1.CloudSharpness;
                endSharpness2 = newProfile.CloudLayer2.CloudSharpness;
                endSharpness3 = newProfile.CloudLayer3.CloudSharpness;
                endSharpness4 = newProfile.CloudLayer4.CloudSharpness;
                startLightAbsorption1 = oldProfile.CloudLayer1.CloudLightAbsorption;
                startLightAbsorption2 = oldProfile.CloudLayer2.CloudLightAbsorption;
                startLightAbsorption3 = oldProfile.CloudLayer3.CloudLightAbsorption;
                startLightAbsorption4 = oldProfile.CloudLayer4.CloudLightAbsorption;
                endLightAbsorption1 = newProfile.CloudLayer1.CloudLightAbsorption;
                endLightAbsorption2 = newProfile.CloudLayer2.CloudLightAbsorption;
                endLightAbsorption3 = newProfile.CloudLayer3.CloudLightAbsorption;
                endLightAbsorption4 = newProfile.CloudLayer4.CloudLightAbsorption;
                startHeight1 = oldProfile.CloudLayer1.CloudHeight;
                startHeight2 = oldProfile.CloudLayer2.CloudHeight;
                startHeight3 = oldProfile.CloudLayer3.CloudHeight;
                startHeight4 = oldProfile.CloudLayer4.CloudHeight;
                endHeight1 = newProfile.CloudLayer1.CloudHeight;
                endHeight2 = newProfile.CloudLayer2.CloudHeight;
                endHeight3 = newProfile.CloudLayer3.CloudHeight;
                endHeight4 = newProfile.CloudLayer4.CloudHeight;

                // use new profile for transition
                _CloudProfile = newProfile;
            }

            DeleteAndTransitionRenderProfile(_CloudProfile);

            // create temp object for animation, we don't want to modify variables in the actual assets during animation
            _CloudProfile = lastProfile = currentRenderCloudProfile = _CloudProfile.Clone();
            currentRenderCloudProfileIsTemporary = true;

            // animate animatable properties
            TweenFactory.Tween("WeatherMakerClouds", 0.0f, 1.0f, duration, TweenScaleFunctions.Linear, (ITween<float> c) =>
            {
                float progress = c.CurrentValue;
                currentRenderCloudProfile.CloudLayer1.CloudNoiseScale = Vector4.Lerp(startScale1, endScale1, progress);
                currentRenderCloudProfile.CloudLayer2.CloudNoiseScale = Vector4.Lerp(startScale2, endScale2, progress);
                currentRenderCloudProfile.CloudLayer3.CloudNoiseScale = Vector4.Lerp(startScale3, endScale3, progress);
                currentRenderCloudProfile.CloudLayer4.CloudNoiseScale = Vector4.Lerp(startScale4, endScale4, progress);
                currentRenderCloudProfile.CloudLayer1.CloudNoiseMultiplier = Vector4.Lerp(startMultiplier1, endMultiplier1, progress);
                currentRenderCloudProfile.CloudLayer2.CloudNoiseMultiplier = Vector4.Lerp(startMultiplier2, endMultiplier2, progress);
                currentRenderCloudProfile.CloudLayer3.CloudNoiseMultiplier = Vector4.Lerp(startMultiplier3, endMultiplier3, progress);
                currentRenderCloudProfile.CloudLayer4.CloudNoiseMultiplier = Vector4.Lerp(startMultiplier4, endMultiplier4, progress);
                currentRenderCloudProfile.CloudLayer1.CloudNoiseRotation.LastValue = Mathf.Lerp(startRotation.x, endRotation.x, progress);
                currentRenderCloudProfile.CloudLayer2.CloudNoiseRotation.LastValue = Mathf.Lerp(startRotation.y, endRotation.y, progress);
                currentRenderCloudProfile.CloudLayer3.CloudNoiseRotation.LastValue = Mathf.Lerp(startRotation.z, endRotation.z, progress);
                currentRenderCloudProfile.CloudLayer4.CloudNoiseRotation.LastValue = Mathf.Lerp(startRotation.w, endRotation.w, progress);
                currentRenderCloudProfile.CloudLayer1.CloudNoiseVelocity = Vector3.Lerp(startVelocity1, endVelocity1, progress);
                currentRenderCloudProfile.CloudLayer2.CloudNoiseVelocity = Vector3.Lerp(startVelocity2, endVelocity2, progress);
                currentRenderCloudProfile.CloudLayer3.CloudNoiseVelocity = Vector3.Lerp(startVelocity3, endVelocity3, progress);
                currentRenderCloudProfile.CloudLayer4.CloudNoiseVelocity = Vector3.Lerp(startVelocity4, endVelocity4, progress);
                currentRenderCloudProfile.CloudLayer1.CloudNoiseMaskScale = Mathf.Lerp(startMaskScale1, endMaskScale1, progress);
                currentRenderCloudProfile.CloudLayer2.CloudNoiseMaskScale = Mathf.Lerp(startMaskScale2, endMaskScale2, progress);
                currentRenderCloudProfile.CloudLayer3.CloudNoiseMaskScale = Mathf.Lerp(startMaskScale3, endMaskScale3, progress);
                currentRenderCloudProfile.CloudLayer4.CloudNoiseMaskScale = Mathf.Lerp(startMaskScale4, endMaskScale4, progress);
                currentRenderCloudProfile.CloudLayer1.CloudNoiseMaskRotation.LastValue = Mathf.Lerp(startMaskRotation.x, endMaskRotation.x, progress);
                currentRenderCloudProfile.CloudLayer2.CloudNoiseMaskRotation.LastValue = Mathf.Lerp(startMaskRotation.y, endMaskRotation.y, progress);
                currentRenderCloudProfile.CloudLayer3.CloudNoiseMaskRotation.LastValue = Mathf.Lerp(startMaskRotation.z, endMaskRotation.z, progress);
                currentRenderCloudProfile.CloudLayer4.CloudNoiseMaskRotation.LastValue = Mathf.Lerp(startMaskRotation.w, endMaskRotation.w, progress);
                currentRenderCloudProfile.CloudLayer1.CloudNoiseMaskVelocity = Vector3.Lerp(startMaskVelocity1, endMaskVelocity1, progress);
                currentRenderCloudProfile.CloudLayer2.CloudNoiseMaskVelocity = Vector3.Lerp(startMaskVelocity2, endMaskVelocity2, progress);
                currentRenderCloudProfile.CloudLayer3.CloudNoiseMaskVelocity = Vector3.Lerp(startMaskVelocity3, endMaskVelocity3, progress);
                currentRenderCloudProfile.CloudLayer4.CloudNoiseMaskVelocity = Vector3.Lerp(startMaskVelocity4, endMaskVelocity4, progress);
                currentRenderCloudProfile.CloudLayer1.CloudCover = Mathf.Lerp(startCover1, endCover1, progress);
                currentRenderCloudProfile.CloudLayer2.CloudCover = Mathf.Lerp(startCover2, endCover2, progress);
                currentRenderCloudProfile.CloudLayer3.CloudCover = Mathf.Lerp(startCover3, endCover3, progress);
                currentRenderCloudProfile.CloudLayer4.CloudCover = Mathf.Lerp(startCover4, endCover4, progress);
                currentRenderCloudProfile.CloudLayer1.CloudDensity = Mathf.Lerp(startDensity1, endDensity1, progress);
                currentRenderCloudProfile.CloudLayer2.CloudDensity = Mathf.Lerp(startDensity2, endDensity2, progress);
                currentRenderCloudProfile.CloudLayer3.CloudDensity = Mathf.Lerp(startDensity3, endDensity3, progress);
                currentRenderCloudProfile.CloudLayer4.CloudDensity = Mathf.Lerp(startDensity4, endDensity4, progress);
                currentRenderCloudProfile.CloudLayer1.CloudSharpness = Mathf.Lerp(startSharpness1, endSharpness1, progress);
                currentRenderCloudProfile.CloudLayer2.CloudSharpness = Mathf.Lerp(startSharpness2, endSharpness2, progress);
                currentRenderCloudProfile.CloudLayer3.CloudSharpness = Mathf.Lerp(startSharpness3, endSharpness3, progress);
                currentRenderCloudProfile.CloudLayer4.CloudSharpness = Mathf.Lerp(startSharpness4, endSharpness4, progress);
                currentRenderCloudProfile.CloudLayer1.CloudLightAbsorption = Mathf.Lerp(startLightAbsorption1, endLightAbsorption1, progress);
                currentRenderCloudProfile.CloudLayer2.CloudLightAbsorption = Mathf.Lerp(startLightAbsorption2, endLightAbsorption2, progress);
                currentRenderCloudProfile.CloudLayer3.CloudLightAbsorption = Mathf.Lerp(startLightAbsorption3, endLightAbsorption3, progress);
                currentRenderCloudProfile.CloudLayer4.CloudLightAbsorption = Mathf.Lerp(startLightAbsorption4, endLightAbsorption4, progress);
                currentRenderCloudProfile.CloudLayer1.CloudColor = Color.Lerp(startColor1, endColor1, progress);
                currentRenderCloudProfile.CloudLayer2.CloudColor = Color.Lerp(startColor2, endColor2, progress);
                currentRenderCloudProfile.CloudLayer3.CloudColor = Color.Lerp(startColor3, endColor3, progress);
                currentRenderCloudProfile.CloudLayer4.CloudColor = Color.Lerp(startColor4, endColor4, progress);
                currentRenderCloudProfile.CloudLayer1.CloudEmissionColor = Color.Lerp(startEmissionColor1, endEmissionColor1, progress);
                currentRenderCloudProfile.CloudLayer2.CloudEmissionColor = Color.Lerp(startEmissionColor2, endEmissionColor2, progress);
                currentRenderCloudProfile.CloudLayer3.CloudEmissionColor = Color.Lerp(startEmissionColor3, endEmissionColor3, progress);
                currentRenderCloudProfile.CloudLayer4.CloudEmissionColor = Color.Lerp(startEmissionColor4, endEmissionColor4, progress);
                currentRenderCloudProfile.CloudLayer1.CloudAmbientMultiplier = Mathf.Lerp(startAmbientMultiplier1, endAmbientMultiplier1, progress);
                currentRenderCloudProfile.CloudLayer2.CloudAmbientMultiplier = Mathf.Lerp(startAmbientMultiplier2, endAmbientMultiplier2, progress);
                currentRenderCloudProfile.CloudLayer3.CloudAmbientMultiplier = Mathf.Lerp(startAmbientMultiplier3, endAmbientMultiplier3, progress);
                currentRenderCloudProfile.CloudLayer4.CloudAmbientMultiplier = Mathf.Lerp(startAmbientMultiplier4, endAmbientMultiplier4, progress);
                currentRenderCloudProfile.CloudLayer1.CloudHeight = Mathf.Lerp(startHeight1, endHeight1, progress);
                currentRenderCloudProfile.CloudLayer2.CloudHeight = Mathf.Lerp(startHeight2, endHeight2, progress);
                currentRenderCloudProfile.CloudLayer3.CloudHeight = Mathf.Lerp(startHeight3, endHeight3, progress);
                currentRenderCloudProfile.CloudLayer4.CloudHeight = Mathf.Lerp(startHeight4, endHeight4, progress);
                currentRenderCloudProfile.DirectionalLightIntensityMultiplier = Mathf.Lerp(startDirectionalLightIntensityMultiplier, endDirectionalLightIntensityMultiplier, progress);
                currentRenderCloudProfile.DirectionalLightShadowStrengthMultiplier = Mathf.Lerp(startDirectionalLightShadowStrengthMultiplier, endDirectionalLightShadowStrengthMultiplier, progress);
            }, (ITween<float> c) =>
            {
                DeleteAndTransitionRenderProfile(newProfile);
                currentRenderCloudProfileIsTemporary = false;
            });
        }

        /// <summary>
        /// Hide clouds animated, all layers
        /// </summary>
        /// <param name="duration">Transition duration in seconds</param>
        public void HideCloudsAnimated(float duration)
        {
            ShowCloudsAnimated(duration, (WeatherMakerCloudProfileScript)null);
        }
    }
}
