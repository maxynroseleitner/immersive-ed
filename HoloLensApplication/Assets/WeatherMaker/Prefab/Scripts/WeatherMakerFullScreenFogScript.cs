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

using UnityEngine;
using UnityEngine.Rendering;

using System;
using System.Collections.Generic;

namespace DigitalRuby.WeatherMaker
{
    public class WeatherMakerFullScreenFogScript : WeatherMakerFogScript<WeatherMakerFullScreenFogProfileScript>
    {
        [Header("Full screen fog rendering")]
        [Tooltip("Material to render the fog full screen after it has been calculated")]
        public Material FogFullScreenMaterial;

        [Tooltip("Down sample scale.")]
        [Range(0.01f, 1.0f)]
        public float DownSampleScale = 1.0f;

        [Tooltip("Fog Blur Material.")]
        public Material FogBlurMaterial;

        [Tooltip("Fog Blur Shader Type.")]
        public BlurShaderType BlurShader;

        [Tooltip("Render fog in this render queue for the command buffer.")]
        public CameraEvent FogRenderQueue = CameraEvent.BeforeForwardAlpha;

        private WeatherMakerFullScreenEffect effect;
        private System.Action<WeatherMakerCommandBuffer> updateShaderPropertiesAction;

        private const string commandBufferName = "WeatherMakerFullScreenFogScript";

        private void UpdateFogProperties()
        {
            if (FogProfile == null)
            {
                return;
            }

            float multiplier;
            if (FogProfile.FogMode == WeatherMakerFogMode.Constant || FogProfile.FogMode == WeatherMakerFogMode.Linear)
            {
                float h = (FogProfile.FogHeight < Mathf.Epsilon ? 1000.0f : FogProfile.FogHeight) * 0.01f;
                multiplier = 1.0f - (FogProfile.FogDensity * 4.0f * h);
            }
            else if (FogProfile.FogMode == WeatherMakerFogMode.Exponential)
            {
                float h = (FogProfile.FogHeight < Mathf.Epsilon ? 1000.0f : FogProfile.FogHeight) * 0.02f;
                multiplier = 1.0f - Mathf.Min(1.0f, Mathf.Pow(FogProfile.FogDensity * 32.0f * h, 0.5f));
            }
            else
            {
                float h = (FogProfile.FogHeight < Mathf.Epsilon ? 1000.0f : FogProfile.FogHeight) * 0.04f;
                multiplier = 1.0f - Mathf.Min(1.0f, Mathf.Pow(FogProfile.FogDensity * 64.0f * h, 0.5f));
            }
            WeatherMakerScript.Instance.DayNightScript.DirectionalLightShadowIntensityMultipliers["WeatherMakerFullScreenFogScript"] = Mathf.Clamp(multiplier, 0.0f, 1.0f);
            effect.SetupEffect(FogMaterial, FogFullScreenMaterial, FogBlurMaterial, BlurShader, DownSampleScale,
                (FogProfile.SunShaftSampleCount <= 0 ? 0.0f : FogProfile.SunShaftDownSampleScale), updateShaderPropertiesAction,
                (FogProfile.FogDensity > Mathf.Epsilon && FogProfile.FogMode != WeatherMakerFogMode.None));
        }

        private void UpdateShaderProperties(WeatherMakerCommandBuffer b)
        {
            FogProfile.UpdateMaterialProperties(b.Material, b.Camera);
        }

        protected override void Start()
        {
            // create fog profile now, base.Start will also create it but uses a non-full screen profile default
            if (FogProfile == null)
            {
                FogProfile = Resources.Load<WeatherMakerFullScreenFogProfileScript>("WeatherMakerFullScreenFogProfile_Default");
            }
            updateShaderPropertiesAction = UpdateShaderProperties;

            base.Start();

            effect = new WeatherMakerFullScreenEffect
            {
                CommandBufferName = commandBufferName,
                DownsampleRenderBufferTextureName = "_MainTex2",
                RenderQueue = FogRenderQueue
            };

#if UNITY_EDITOR

            if (Application.isPlaying)
            {

#endif

                FogFullScreenMaterial = new Material(FogFullScreenMaterial);
                FogBlurMaterial = new Material(FogBlurMaterial);

#if UNITY_EDITOR

            }

#endif

        }

        protected override void LateUpdate()
        {
            base.LateUpdate();
            UpdateFogProperties();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (effect != null)
            {
                effect.Dispose();
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (effect != null)
            {
                effect.Dispose();
            }
        }

        protected override void UpdateFogMaterialFromProfile()
        {
            // no need to call base class as we set material properties elsewhere
        }

        public void PreCullCamera(Camera camera)
        {

        }

        public void PreRenderCamera(Camera camera)
        {
            if (effect != null)
            {
                effect.SetupCamera(camera);
            }
        }
    }
}