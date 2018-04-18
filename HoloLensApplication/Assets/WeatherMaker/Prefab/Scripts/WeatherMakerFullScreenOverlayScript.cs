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
    public class WeatherMakerFullScreenOverlayScript : MonoBehaviour
    {
        [Header("Overlay - rendering")]
        [Tooltip("Overlay profile")]
        public WeatherMakerOverlayScript OverlayProfile;

        [Tooltip("Material that renders the overlay effect")]
        public Material OverlayMaterial;

        [Tooltip("Material to render the overlay full screen in a second pass if needed")]
        public Material OverlayFullScreenMaterial;

        [Tooltip("Down sample scale.")]
        [Range(0.01f, 1.0f)]
        public float DownSampleScale = 1.0f;

        [Tooltip("Overlay blur Material.")]
        public Material OverlayBlurMaterial;

        [Tooltip("Overlay blur Shader Type.")]
        public BlurShaderType BlurShader;

        [Tooltip("Render overlay in this render queue for the command buffer.")]
        public CameraEvent OverlayRenderQueue = CameraEvent.BeforeImageEffectsOpaque;

        [Tooltip("External intensity function")]
        public WeatherMakerOutputParameterEventFloat ExternalIntensityFunction;

        private WeatherMakerFullScreenEffect effect;
        private readonly WeatherMakerOutputParameterFloat param = new WeatherMakerOutputParameterFloat();
        private System.Action<WeatherMakerCommandBuffer> updateShaderPropertiesAction;

        private const string commandBufferName = "WeatherMakerFullScreenOverlayScript";

        private void UpdateEffectProperties()
        {
            bool enabled = (OverlayProfile != null && !OverlayProfile.Disabled && (OverlayProfile.OverlayIntensity > 0.0001f || OverlayProfile.AutoIntensityMultiplier != 0.0f || OverlayProfile.OverlayMinimumIntensity > 0.0001f) && OverlayProfile.OverlayColor.a > 0.0f);
            effect.SetupEffect(OverlayMaterial, OverlayFullScreenMaterial, OverlayBlurMaterial, BlurShader, DownSampleScale, 0.0f, updateShaderPropertiesAction, enabled);
        }

        private float ExternalIntensityFunctionImpl()
        {
            ExternalIntensityFunction.Invoke(param);
            return param.Value;
        }

        private void UpdateShaderProperties(WeatherMakerCommandBuffer b)
        {
            if (OverlayProfile != null && !OverlayProfile.Disabled)
            {
                if (ExternalIntensityFunction == null)
                {
                    OverlayProfile.ExternalIntensityFunction = null;
                }
                else
                {
                    OverlayProfile.ExternalIntensityFunction = ExternalIntensityFunctionImpl;
                }
                OverlayProfile.UpdateMaterial(b.Material);
            }
        }

        private void Start()
        {
            effect = new WeatherMakerFullScreenEffect
            {
                CommandBufferName = commandBufferName,
                DownsampleRenderBufferTextureName = "_MainTex2",
                RenderQueue = OverlayRenderQueue
            };
            updateShaderPropertiesAction = UpdateShaderProperties;

#if UNITY_EDITOR

            if (Application.isPlaying)
            {

#endif

                // clone during play mode so as to not overwrite profile properties inadvertantly
                if (OverlayProfile != null)
                {
                    OverlayProfile = ScriptableObject.Instantiate(OverlayProfile);
                }
                if (OverlayBlurMaterial != null)
                {
                    OverlayBlurMaterial = new Material(OverlayBlurMaterial);
                }

#if UNITY_EDITOR

            }

#endif

        }

        private void LateUpdate()
        {
            UpdateEffectProperties();
        }

        private void OnDestroy()
        {
            if (effect != null)
            {
                effect.Dispose();
            }
        }

        private void OnDisable()
        {
            if (effect != null)
            {
                effect.Dispose();
            }
        }

        public void PreCullCamera(Camera camera)
        {
            if (effect != null)
            {
                effect.SetupCamera(camera);
                if (effect.Enabled && camera.actualRenderingPath == RenderingPath.Forward && camera.depthTextureMode != DepthTextureMode.DepthNormals)
                {
                    camera.depthTextureMode = DepthTextureMode.DepthNormals;
                }
            }
        }
    }
}
