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
using System.Collections;

namespace DigitalRuby.WeatherMaker
{
    /// <summary>
    /// Sky modes
    /// </summary>
    public enum WeatherMakeSkyMode
    {
        /// <summary>
        /// Textured - day, dawn/dusk and night are all done via textures
        /// </summary>
        Textured = 0,

        /// <summary>
        /// Procedural sky - day and dawn/dusk textures are overlaid on top of procedural sky, night texture is used as is
        /// </summary>
        ProceduralTexturedUnityStyle,

        /// <summary>
        /// Procedural sky - day, dawn/dusk textures are ignored, night texture is used as is
        /// </summary>
        ProceduralUnityStyle,

        /// <summary>
        /// Procedural sky - day and dawn/dusk textures are overlaid on top of procedural sky, night texture is used as is - preetham style
        /// </summary>
        ProceduralTexturedPreethamStyle,

        /// <summary>
        /// Procedural sky - day, dawn/dusk textures are ignored, night texture is used as is - preetham style
        /// </summary>
        ProceduralPreethamStyle
    }

    [CreateAssetMenu(fileName = "WeatherMakerSkyProfile", menuName = "WeatherMaker/Sky Profile", order = 2)]
    public class WeatherMakerSkyProfileScript : ScriptableObject
    {
        [Header("Sky Rendering")]
        [Tooltip("The sky mode. 'Textured' uses a texture for day, dawn/dusk and night. " +
            "'Procedural textured' combines a procedural sky with the day and dawn/dusk textures using alpha, and uses the night texture as is. " +
            "'Procedural' uses the night texture as is and does everything else procedurally.")]
        public WeatherMakeSkyMode SkyMode = WeatherMakeSkyMode.ProceduralUnityStyle;

        [Range(0.0f, 1.0f)]
        [Tooltip("Dither level")]
        public float DitherLevel = 0.005f;

        [Header("Positioning")]
        [Range(-0.5f, 0.5f)]
        [Tooltip("Offset the sky this amount from the camera y. This value is multiplied by the height of the sky sphere.")]
        public float YOffsetMultiplier = 0.0f;

        [Range(0.1f, 1.0f)]
        [Tooltip("Place the sky sphere at this amount of the far clip plane")]
        public float FarClipPlaneMultiplier = 0.8f;

        [Tooltip("Sky gradient. Useful for extra tint of sky and clouds as sun nears the horizon. Center of gradient is sun right at horizon.")]
        public Gradient SkyGradient;

        [Header("Textures")]
        [Tooltip("The daytime texture (ignored for procedural sky mode).")]
        public Texture2D DayTexture;

        [Tooltip("The dawn / dusk texture (ignored for procedural sky mode).")]
        public Texture2D DawnDuskTexture;

        [Tooltip("The night time texture")]
        public Texture2D NightTexture;

        [Header("Night Sky")]
        [Range(0.0f, 1.0f)]
        [Tooltip("Night pixels must have an R, G or B value greater than or equal to this to be visible. Raise this value if you want to hide dimmer elements " +
            "of your night texture or there is a lot of light pollution, i.e. a city.")]
        public float NightVisibilityThreshold = 0.0f;

        [Range(0.0f, 32.0f)]
        [Tooltip("Intensity of night sky. Pixels that don't meet the NightVisibilityThreshold will still be invisible.")]
        public float NightIntensity = 1.0f;

        [Range(0.0f, 100.0f)]
        [Tooltip("How fast the twinkle pulsates")]
        public float NightTwinkleSpeed = 6.6f;

        [Tooltip("The variance of the night twinkle. The higher the value, the more variance.")]
        [Range(0.0f, 10.0f)]
        public float NightTwinkleVariance = 1.11f;

        [Tooltip("The minimum of the max rgb component for the night pixel to twinkle")]
        [Range(0.0f, 1.0f)]
        public float NightTwinkleMinimum = 0.02f;

        [Tooltip("The amount of randomness in the night sky twinkle")]
        [Range(0.0f, 5.0f)]
        public float NightTwinkleRandomness = 0.87f;

        [Header("Sky Parameters (Advanced)")]
        [Tooltip("Allows eclipses - beware Unity bug that causes raycast to be very expensive. If you see CPU spike, disable.")]
        public bool CheckForEclipse;

        [Tooltip("(2D only) Sky y offset. Causes sky ray to increase, moving the sunset, sunrise and sky horizon up.")]
        [Range(0.0f, 0.5f)]
        public float SkyYOffset = 0.25f;

        [Range(0.0f, 1.0f)]
        [Tooltip("Sky camera height (km)")]
        public float SkyCameraHeight = 0.0001f;

        [Tooltip("Sky sphere exposure, decrease/increase brightness")]
        [Range(0.01f, 8.0f)]
        public float SkyExposure = 1.0f;

        [Tooltip("Sky tint color")]
        public Color SkyTintColor = new Color(0.5f, 0.5f, 0.5f);

        [Range(0.0f, 1.0f)]
        [Tooltip("Sky atmosphere mie g, controls glow around the sun, etc.")]
        public float SkyAtmosphereMie = 0.99f;

        [Range(0.0f, 5.0f)]
        [Tooltip("Sky atmosphere thickness")]
        public float SkyAtmosphereThickness = 1.0f;

        [Range(0.0f, 10.0f)]
        [Tooltip("Sky atmosphere turbidity")]
        public float SkyAtmosphereTurbidity = 1.0f;

        [Range(1.0f, 2.0f)]
        [Tooltip("Sky outer radius")]
        public float SkyOuterRadius = 1.025f;

        [Range(1.0f, 2.0f)]
        [Tooltip("Sky inner radius")]
        public float SkyInnerRadius = 1.0f;

        [Range(0.0f, 50.0f)]
        [Tooltip("Sky mie multiplier")]
        public float SkyMieMultiplier = 1.0f;

        [Range(0.0f, 50.0f)]
        [Tooltip("Sky rayleigh multiplier")]
        public float SkyRayleighMultiplier = 1.0f;

        [SingleLine("Light wave lengths (RGB, Tint Range)")]
        public Vector4 LightWaveLengths = new Vector4(0.650f, 0.570f, 0.475f, 0.15f);

        private static RaycastHit[] eclipseHits = new RaycastHit[16];

        private static Vector4 GetTotalRayleigh(Vector3 waveLength, float rayleighMultiplier)
        {
            // https://github.com/ngokevin/kframe/blob/master/components/sun-sky/shaders/fragment.glsl
            const float n = 1.0003f; // refractive index of air
            const float N = 2.545E25f; // number of molecules per unit volume for air at 288.15K and 1013mb (sea level -45 celsius)
            const float pn = 0.035f;  // depolatization factor for standard air

            float sunPosY = -WeatherMakerScript.Instance.Sun.Transform.forward.y;
            float sunfade = 1.0f - Mathf.Clamp(1.0f - Mathf.Exp((sunPosY / 450000.0f)), 0.0f, 1.0f);
            float rayleighCoefficient = rayleighMultiplier - (1.0f * (1.0f - sunfade));
            Vector4 r = new Vector4
            (
                rayleighCoefficient * (((8.0f * Mathf.Pow(Mathf.PI, 3.0f) * (Mathf.Pow(Mathf.Pow(n, 2.0f) - 1.0f, 2.0f))) * (6.0f + 3.0f * pn)) / ((3.0f * N * Mathf.Pow(waveLength.x, 4.0f)) * (6.0f - 7.0f * pn))),
                rayleighCoefficient * (((8.0f * Mathf.Pow(Mathf.PI, 3.0f) * (Mathf.Pow(Mathf.Pow(n, 2.0f) - 1.0f, 2.0f))) * (6.0f + 3.0f * pn)) / ((3.0f * N * Mathf.Pow(waveLength.y, 4.0f)) * (6.0f - 7.0f * pn))),
                rayleighCoefficient * (((8.0f * Mathf.Pow(Mathf.PI, 3.0f) * (Mathf.Pow(Mathf.Pow(n, 2.0f) - 1.0f, 2.0f))) * (6.0f + 3.0f * pn)) / ((3.0f * N * Mathf.Pow(waveLength.z, 4.0f)) * (6.0f - 7.0f * pn))),
                sunfade
            );
            return r;
        }

        // https://github.com/ngokevin/kframe/blob/master/components/sun-sky/shaders/fragment.glsl
        private static readonly Vector3 K = new Vector3(0.686f, 0.678f, 0.666f); // coefficient for primaries
        private static Vector4 GetTotalMie(Vector3 waveLength, float turbidity, float multiplier)
        {
            const float v = 2.0f; // 4 - 2
            float mieCoefficient = Mathf.Min(0.99f, 0.005f * multiplier * Mathf.Min(50.0f, 1.0f / Mathf.Pow(Mathf.Max(0.01f, -WeatherMakerScript.Instance.Sun.Transform.forward.y), 0.5f)));
            float c = (0.2f * turbidity) * 10E-18f;
            float d = 0.434f * c * Mathf.PI;
            Vector4 m = new Vector4
            (
               mieCoefficient * (d * Mathf.Pow((2.0f * Mathf.PI) / waveLength.x, v) * K.x),
               mieCoefficient * (d * Mathf.Pow((2.0f * Mathf.PI) / waveLength.y, v) * K.y),
               mieCoefficient * (d * Mathf.Pow((2.0f * Mathf.PI) / waveLength.z, v) * K.z),
               GetTotalSunIntensity()
            );
            return m;
        }

        private static float GetTotalSunIntensity()
        {
            // earth shadow hack
            const float cutoffAngle = 1.67510731556870734f;// Mathf.PI / 1.95f;
            const float steepness = 1.5f;
            const float EE = 1000.0f;

            Vector3 sunDir = -WeatherMakerScript.Instance.Sun.Transform.forward;
            float zenithAngleCos = Vector3.Dot(sunDir, Vector3.up);
            return Mathf.Max(0.5f, EE * Mathf.Max(0.0f, 1.0f - Mathf.Exp(-((cutoffAngle - Mathf.Acos(zenithAngleCos)) / steepness))));
        }

        internal static void SetGlobalSkyParameters(Gradient skyGradient, float skyAtmosphereMie, float skyMieMultiplier, float skyRayleighMultiplier, float skyAtmosphereThickness,
            float skyAtmosphereTurbidity, float skyExposure, Color skyTintColor, float skyOuterRadius, float skyInnerRadius, float skyCameraHeight, Vector4 waveLength)
        {
            // global sky parameters
            float mieG = -skyAtmosphereMie;
            float mieG2 = skyAtmosphereMie * skyAtmosphereMie;
            float mieConstant = 0.001f * skyMieMultiplier;
            float rayleighConstant = 0.0025f * skyRayleighMultiplier;
            rayleighConstant = Mathf.LerpUnclamped(0.0f, rayleighConstant, Mathf.Pow(skyAtmosphereThickness, 2.5f));
            float redColor = waveLength.x;
            float greenColor = waveLength.y;
            float blueColor = waveLength.z;
            float lightRange = waveLength.w;
            float lightWaveLengthRedTint = Mathf.Lerp(redColor - lightRange, redColor + lightRange, 1.0f - skyTintColor.r);
            float lightWaveLengthGreenTint = Mathf.Lerp(greenColor - lightRange, greenColor + lightRange, 1.0f - skyTintColor.g);
            float lightWaveLengthBlueTint = Mathf.Lerp(blueColor - lightRange, blueColor + lightRange, 1.0f - skyTintColor.b);
            float lightWaveLengthRed4 = lightWaveLengthRedTint * lightWaveLengthRedTint * lightWaveLengthRedTint * lightWaveLengthRedTint;
            float lightWaveLengthGreen4 = lightWaveLengthGreenTint * lightWaveLengthGreenTint * lightWaveLengthGreenTint * lightWaveLengthGreenTint;
            float lightWaveLengthBlue4 = lightWaveLengthBlueTint * lightWaveLengthBlueTint * lightWaveLengthBlueTint * lightWaveLengthBlueTint;
            float lightInverseWaveLengthRed4 = 1.0f / lightWaveLengthRed4;
            float lightInverseWaveLengthGreen4 = 1.0f / lightWaveLengthGreen4;
            float lightInverseWaveLengthBlue4 = 1.0f / lightWaveLengthBlue4;
            const float sunBrightnessFactor = 40.0f;
            float sunRed = rayleighConstant * sunBrightnessFactor * lightInverseWaveLengthRed4;
            float sunGreen = rayleighConstant * sunBrightnessFactor * lightInverseWaveLengthGreen4;
            float sunBlue = rayleighConstant * sunBrightnessFactor * lightInverseWaveLengthBlue4;
            float sunIntensity = mieConstant * sunBrightnessFactor;
            float pi4Red = rayleighConstant * 4.0f * skyAtmosphereTurbidity * Mathf.PI * lightInverseWaveLengthRed4;
            float pi4Green = rayleighConstant * 4.0f * skyAtmosphereTurbidity * Mathf.PI * lightInverseWaveLengthGreen4;
            float pi4Blue = rayleighConstant * 4.0f * skyAtmosphereTurbidity * Mathf.PI * lightInverseWaveLengthBlue4;
            float pi4Intensity = mieConstant * 4.0f * skyAtmosphereTurbidity * Mathf.PI;
            float scaleFactor = 1.0f / (skyOuterRadius - 1.0f);
            const float scaleDepth = 0.25f;
            float scaleOverScaleDepth = scaleFactor / scaleDepth;
            Vector4 col = WeatherMakerScript.EvaluateSunGradient(skyGradient);
            col.w = skyExposure;
            Shader.SetGlobalVector("_WeatherMakerSkyGradientColor", col);
            Shader.SetGlobalFloat("_WeatherMakerSkySamples", 3.0f);
            float mieDot = Mathf.Pow(1.0f - Vector3.Dot(Vector3.up, -WeatherMakerScript.Instance.Sun.Transform.forward), 5.0f);
            Shader.SetGlobalVector("_WeatherMakerSkyMieG", new Vector4(mieG, mieG2, -mieG, mieDot));
            Shader.SetGlobalVector("_WeatherMakerSkyAtmosphereParams", new Vector4(skyAtmosphereThickness, skyAtmosphereTurbidity, skyExposure, 0.0f));
            Shader.SetGlobalVector("_WeatherMakerSkyRadius", new Vector4(skyOuterRadius, skyOuterRadius * skyOuterRadius, skyInnerRadius, skyInnerRadius * skyInnerRadius));
            Shader.SetGlobalVector("_WeatherMakerSkyMie", new Vector4(1.5f * ((1.0f - mieG2) / (2.0f + mieG2)), 1.0f + mieG2, 2.0f + mieG, 0.0f));
            Shader.SetGlobalVector("_WeatherMakerSkyLightScattering", new Vector4(sunRed, sunGreen, sunBlue, sunIntensity));
            Shader.SetGlobalVector("_WeatherMakerSkyLightPIScattering", new Vector4(pi4Red, pi4Green, pi4Blue, pi4Intensity));
            Shader.SetGlobalVector("_WeatherMakerSkyScale", new Vector4(scaleFactor, scaleDepth, scaleOverScaleDepth, skyCameraHeight));

            // reduce wave length for preetham sky formulas
            waveLength *= 1E-6f;
            Shader.SetGlobalVector("_WeatherMakerSkyTotalRayleigh", GetTotalRayleigh(waveLength, skyRayleighMultiplier * skyAtmosphereThickness));
            Shader.SetGlobalVector("_WeatherMakerSkyTotalMie", GetTotalMie(waveLength, skyAtmosphereTurbidity, skyMieMultiplier));
        }

        private void SetShaderSkyParameters(Camera camera, Material material)
        {
            material.DisableKeyword("ENABLE_TEXTURED_SKY");
            material.DisableKeyword("ENABLE_PROCEDURAL_TEXTURED_SKY");
            material.DisableKeyword("ENABLE_PROCEDURAL_TEXTURED_SKY_PREETHAM");
            material.DisableKeyword("ENABLE_PROCEDURAL_SKY");
            material.DisableKeyword("ENABLE_PROCEDURAL_SKY_PREETHAM");

            switch (SkyMode)
            {
                case WeatherMakeSkyMode.Textured:
                    material.EnableKeyword("ENABLE_TEXTURED_SKY");
                    break;

                case WeatherMakeSkyMode.ProceduralTexturedUnityStyle:
                    material.EnableKeyword("ENABLE_PROCEDURAL_TEXTURED_SKY");
                    break;

                case WeatherMakeSkyMode.ProceduralTexturedPreethamStyle:
                    material.EnableKeyword("ENABLE_PROCEDURAL_TEXTURED_SKY_PREETHAM");
                    break;

                case WeatherMakeSkyMode.ProceduralUnityStyle:
                    material.EnableKeyword("ENABLE_PROCEDURAL_SKY");
                    break;

                case WeatherMakeSkyMode.ProceduralPreethamStyle:
                    material.EnableKeyword("ENABLE_PROCEDURAL_SKY_PREETHAM");
                    break;

            }

            material.mainTexture = DayTexture;
            material.SetTexture("_DawnDuskTex", DawnDuskTexture);
            material.SetTexture("_NightTex", NightTexture);

            SetGlobalSkyParameters(SkyGradient, SkyAtmosphereMie, SkyMieMultiplier, SkyRayleighMultiplier, SkyAtmosphereThickness, SkyAtmosphereTurbidity,
                SkyExposure, SkyTintColor, SkyOuterRadius, SkyInnerRadius, SkyCameraHeight, LightWaveLengths);

            bool gamma = (QualitySettings.activeColorSpace == ColorSpace.Gamma);
            float radius = (camera.farClipPlane * FarClipPlaneMultiplier) * 0.95f;
            Shader.SetGlobalFloat("_WeatherMakerSkySphereRadius", radius);
            Shader.SetGlobalFloat("_WeatherMakerSkySphereRadiusSquared", radius * radius);
            Shader.SetGlobalFloat("_WeatherMakerSkyDitherLevel", (gamma ? DitherLevel : DitherLevel * 0.5f));
            Shader.SetGlobalFloat("_WeatherMakerSkyYOffset", SkyYOffset);
        }

        private void SetShaderLightParameters(Material material)
        {
            material.SetFloat("_NightSkyMultiplier", Mathf.Max(1.0f - Mathf.Min(1.0f, SkyAtmosphereThickness), WeatherMakerScript.Instance.DayNightScript.NightMultiplier));
            material.SetFloat("_NightVisibilityThreshold", NightVisibilityThreshold);
            material.SetFloat("_NightIntensity", NightIntensity);
            if (material.IsKeywordEnabled("ENABLE_NIGHT_TWINKLE"))
            {
                material.DisableKeyword("ENABLE_NIGHT_TWINKLE");
            }
            if (NightTwinkleRandomness > 0.0f || (NightTwinkleVariance > 0.0f && NightTwinkleSpeed > 0.0f))
            {
                material.SetFloat("_NightTwinkleSpeed", NightTwinkleSpeed);
                material.SetFloat("_NightTwinkleVariance", NightTwinkleVariance);
                material.SetFloat("_NightTwinkleMinimum", NightTwinkleMinimum);
                material.SetFloat("_NightTwinkleRandomness", NightTwinkleRandomness);
                material.EnableKeyword("ENABLE_NIGHT_TWINKLE");
            }
        }

        private void RaycastForEclipse(Camera camera, Material material)
        {
            // disable allow eclipses everywhere by default
            float eclipsePower = 0.0f;
            foreach (WeatherMakerCelestialObject moon in WeatherMakerScript.Instance.Moons)
            {
                moon.Renderer.sharedMaterial.DisableKeyword("ENABLE_SUN_ECLIPSE");
                if (moon.Collider != null)
                {
                    moon.Collider.enabled = CheckForEclipse;
                }
            }

            if (CheckForEclipse)
            {
                float sunRadius = Mathf.Lerp(0.0f, 1000.0f, Mathf.Pow(WeatherMakerScript.Instance.Sun.Scale, 0.15f));
                Vector3 origin = camera.transform.position - (WeatherMakerScript.Instance.Sun.Transform.forward * camera.farClipPlane * 1.7f);
                int hitCount = Physics.SphereCastNonAlloc(origin, sunRadius, WeatherMakerScript.Instance.Sun.Transform.forward, eclipseHits, camera.farClipPlane);
                for (int i = 0; i < hitCount; i++)
                {
                    foreach (WeatherMakerCelestialObject moon in WeatherMakerScript.Instance.Moons)
                    {
                        if (moon.Transform.gameObject == eclipseHits[i].collider.gameObject)
                        {
                            float dot = Mathf.Abs(Vector3.Dot(eclipseHits[i].normal, WeatherMakerScript.Instance.Sun.Transform.forward));
                            eclipsePower += Mathf.Pow(dot, 256.0f);
                            if (!moon.Renderer.sharedMaterial.IsKeywordEnabled("ENABLE_SUN_ECLIPSE"))
                            {
                                moon.Renderer.sharedMaterial.EnableKeyword("ENABLE_SUN_ECLIPSE");
                            }
                            //Debug.LogFormat("Eclipse raycast normal: {0}, dot: {1}, power: {2}", eclipseHits[i].normal, dot, eclipsePower);
                            break;
                        }
                    }
                }
            }

            if (eclipsePower == 0.0f)
            {
                WeatherMakerScript.Instance.DayNightScript.DirectionalLightIntensityMultipliers["WeatherMakerSkySphereScriptEclipse"] = 1.0f;
            }
            else
            {
                float eclipseLightReducer = 1.0f - Mathf.Clamp(eclipsePower, 0.0f, 1.0f);
                WeatherMakerScript.Instance.DayNightScript.DirectionalLightIntensityMultipliers["WeatherMakerSkySphereScriptEclipse"] = eclipseLightReducer;
                material.SetFloat("_NightSkyMultiplier", Mathf.Max(1.0f - Mathf.Min(1.0f, SkyAtmosphereThickness), Mathf.Max(eclipsePower, WeatherMakerScript.Instance.DayNightScript.NightMultiplier)));
            }
        }

        private void SetSkySphereScalesAndPositions(Camera camera, GameObject skySphere)
        {
            // adjust sky sphere position and scale
            float farPlane = camera.farClipPlane * FarClipPlaneMultiplier * 0.9f;
            Vector3 anchor = camera.transform.position;
            float yOffset = farPlane * YOffsetMultiplier;
            skySphere.transform.position = anchor + new Vector3(0.0f, yOffset, 0.0f);
            float scale = farPlane * ((camera.farClipPlane - Mathf.Abs(yOffset)) / camera.farClipPlane);
            float finalScale;
            skySphere.transform.localScale = new Vector3(scale, scale, scale);

            // move sun back near the far plane and scale appropriately
            scale *= 0.5f;
            Vector3 sunOffset = (WeatherMakerScript.Instance.Sun.Transform.forward * ((farPlane * 0.9f) - scale));
            WeatherMakerScript.Instance.Sun.Transform.position = anchor - sunOffset;
            WeatherMakerScript.Instance.Sun.Transform.localScale = new Vector3(scale, scale, scale);

            // move moons back near the far plane and scale appropriately
            foreach (WeatherMakerCelestialObject moon in WeatherMakerScript.Instance.Moons)
            {
                scale = farPlane * moon.Scale;
                finalScale = Mathf.Clamp(Mathf.Abs(moon.Transform.forward.y) * 3.0f, 0.8f, 1.0f);
                finalScale = scale / finalScale;
                Vector3 moonOffset = (moon.Transform.forward * (farPlane - finalScale));
                moon.Transform.position = anchor - moonOffset;
                moon.Transform.localScale = new Vector3(finalScale, finalScale, finalScale);
            }
        }

        private void SetSkyPlaneScalesAndPositions(Camera camera, GameObject skyPlane)
        {
            float orthoHeight = camera.orthographicSize * 2.0f;
            skyPlane.transform.position = camera.transform.position + (Vector3.forward * camera.farClipPlane * 0.99f);
            skyPlane.transform.localScale = new Vector3(orthoHeight * camera.aspect, orthoHeight, 1.0f);
        }

        /// <summary>
        /// Perform updates to the sky sphere based on profile settings
        /// </summary>
        /// <param name="camera">Current camera</param>
        /// <param name="material">Sky sphere material</param>
        /// <param name="skySphere">Sky sphere game object</param>
        public void UpdateSkySphere(Camera camera, Material material, GameObject skySphere)
        {
            if (material == null)
            {
                return;
            }
            SetSkySphereScalesAndPositions(camera, skySphere);
            SetShaderLightParameters(material);
            RaycastForEclipse(camera, material);
            SetShaderSkyParameters(camera, material);
        }

        /// <summary>
        /// Perform updates to the sky plane based on profile settings
        /// </summary>
        /// <param name="camera">Current camera</param>
        /// <param name="material">Sky sphere material</param>
        /// <param name="skyPlane">Sky plane game object</param>
        public void UpdateSkyPlane(Camera camera, Material material, GameObject skyPlane)
        {
            if (material == null)
            {
                return;
            }
            SetSkyPlaneScalesAndPositions(camera, skyPlane);
            SetShaderLightParameters(material);
            RaycastForEclipse(camera, material);
            SetShaderSkyParameters(camera, material);
        }
    }
}
