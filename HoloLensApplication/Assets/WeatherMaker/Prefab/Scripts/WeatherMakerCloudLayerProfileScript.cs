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
    [CreateAssetMenu(fileName = "WeatherMakerCloudLayerProfile", menuName = "WeatherMaker/Cloud Layer Profile", order = 4)]
    public class WeatherMakerCloudLayerProfileScript : ScriptableObject
    {
        [Header("Clouds - noise")]
        [Tooltip("Texture for cloud noise - use single channel texture only.")]
        public Texture2D CloudNoise;

        /*
        [Tooltip("Cloud sample count, layer 1")]
        [Range(1, 100)]
        public int CloudSampleCount = 1;

        [SingleLine("Cloud sample step multiplier, up to 4 octaves.")]
        public Vector4 CloudSampleStepMultiplier = new Vector4(50.0f, 50.0f, 50.0f, 50.0f);

        [SingleLine("Cloud sample dither magic, controls appearance of clouds through ray march")]
        public Vector4 CloudSampleDitherMagic;

        [SingleLine("Cloud sample dither intensit")]
        public Vector4 CloudSampleDitherIntensity;
        */

        [SingleLine("Cloud noise scale, up to 4 octaves, set to 0 to not process that octave.")]
        public Vector4 CloudNoiseScale = new Vector4(0.0002f, 0.0f, 0.0f, 0.0f);

        [SingleLine("Cloud noise multiplier, up to 4 octaves. Should add up to about 1.")]
        public Vector4 CloudNoiseMultiplier = new Vector4(1.0f, 0.0f, 0.0f, 0.0f);

        [Tooltip("Cloud noise velocity.")]
        public Vector3 CloudNoiseVelocity;

        [SingleLine("Cloud noise rotation in degrees.")]
        public RangeOfFloats CloudNoiseRotation;

        [Tooltip("Texture for masking cloud noise, makes clouds visible in only certain parts of the sky.")]
        public Texture2D CloudNoiseMask;

        [Tooltip("Cloud noise mask scale.")]
        [Range(0.000001f, 1.0f)]
        public float CloudNoiseMaskScale = 0.0001f;

        [Tooltip("Offset for cloud noise mask.")]
        public Vector2 CloudNoiseMaskOffset;

        [Tooltip("Cloud noise mask velocity.")]
        public Vector3 CloudNoiseMaskVelocity;

        [SingleLine("Cloud noise mask rotation in degrees.")]
        public RangeOfFloats CloudNoiseMaskRotation;

        [Header("Clouds - appearance")]
        [Tooltip("Cloud color, for lighting.")]
        public Color CloudColor = Color.white;

        [Tooltip("Cloud emission color, always emits this color regardless of lighting.")]
        public Color CloudEmissionColor = Color.clear;

        [Tooltip("Cloud gradient color, where center of gradient is sun at horizon, right is up.")]
        public Gradient CloudGradientColor;

        [Tooltip("Cloud ambient light multiplier.")]
        [Range(0.0f, 10.0f)]
        public float CloudAmbientMultiplier = 1.0f;

        [Tooltip("Cloud height - affects how fast clouds move as player moves and affects scale.")]
        [Range(1.0f, 10000.0f)]
        public float CloudHeight = 5000;

        [Tooltip("Cloud cover, controls how many clouds / how thick the clouds are.")]
        [Range(0.0f, 1.0f)]
        public float CloudCover = 0.0f;

        [Tooltip("Cloud density, controls how opaque the clouds are and how much the cloud block directional light.")]
        [Range(0.0f, 1.0f)]
        public float CloudDensity = 0.0f;

        [Tooltip("Cloud light absorption. As this approaches 0, more light is absorbed.")]
        [Range(0.01f, 1.0f)]
        public float CloudLightAbsorption = 0.13f;

        [Tooltip("Cloud sharpness, controls how distinct the clouds are.")]
        [Range(0.0f, 1.0f)]
        public float CloudSharpness = 0.015f;

        [Tooltip("Cloud pixels with alpha greater than this will cast a shadow. Set to 1 to disable cloud shadows.")]
        [Range(0.0f, 1.0f)]
        public float CloudShadowThreshold = 0.1f;

        [Tooltip("Cloud shadow power. 0 is full power, higher values reduce shadow power.")]
        [Range(0.0001f, 2.0f)]
        public float CloudShadowPower = 0.15f;

        [Tooltip("Bring clouds down at the horizon at the cost of stretching over the top.")]
        [Range(0.0f, 0.5f)]
        public float CloudRayOffset = 0.2f;
    }
}
