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
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace DigitalRuby.WeatherMaker
{
    public class WeatherMakerFallingParticleScript : MonoBehaviour
    {
        [Tooltip("Light particle looping audio source")]
        public AudioSource LoopSourceLight;

        [Tooltip("Medium particle looping audio source")]
        public AudioSource LoopSourceMedium;

        [Tooltip("Heavy particle looping audio source")]
        public AudioSource LoopSourceHeavy;

        [Tooltip("Intensity threshold for medium looping sound")]
        public float SoundMediumIntensityThreshold = 0.33f;

        [Tooltip("Intensity threshold for heavy loop sound")]
        public float SoundHeavyIntensityThreshold = 0.67f;

        [Tooltip("Overall intensity of the system (0-1)")]
        [Range(0.0f, 1.0f)]
        public float Intensity;

        [Tooltip("Intensity multiplier for fewer or extra particles")]
        [Range(0.1f, 10.0f)]
        public float IntensityMultiplier = 1.0f;

        [Tooltip("Intensity multiplier for fewer or extra secondary particles")]
        [Range(0.1f, 10.0f)]
        public float SecondaryIntensityMultiplier = 1.0f;

        [Tooltip("Intensity multiplier for fewer or extra mist particles")]
        [Range(0.1f, 10.0f)]
        public float MistIntensityMultiplier = 1.0f;

        [Tooltip("Base number of particles to emit per second. This is multiplied by intensity and intensity multiplier.")]
        [Range(100, 10000)]
        public int BaseEmissionRate = 1000;

        [Tooltip("Base number of secondary particles to emit per second. This is multiplied by intensity and intensity multiplier.")]
        [Range(100, 10000)]
        public int BaseEmissionRateSecondary = 100;

        [Tooltip("Base number of mist paticles to emit per second. This is multiplied by intensity and intensity multiplier.")]
        [Range(5, 500)]
        public int BaseMistEmissionRate = 50;

        [Tooltip("Particle system")]
        public ParticleSystem ParticleSystem;

        [Tooltip("Particle system that is secondary and optional")]
        public ParticleSystem ParticleSystemSecondary;

        [Tooltip("Particle system to use for mist")]
        public ParticleSystem MistParticleSystem;

        [Tooltip("Particles system for when particles hit something")]
        public ParticleSystem ExplosionParticleSystem;

        [Tooltip("The threshold that Intensity must pass for secondary particles to appear (0 - 1). Set to 1 for no secondary particles. Set this before changing Intensity.")]
        [Range(0.0f, 1.0f)]
        public float SecondaryThreshold = 0.75f;

        [Tooltip("The threshold that Intensity must pass for mist particles to appear (0 - 1). Set to 1 for no mist. Set this before changing Intensity.")]
        [Range(0.0f, 1.0f)]
        public float MistThreshold = 0.5f;

        [Tooltip("Particle dithering factor")]
        [Range(0.0f, 1.0f)]
        public float DitherLevel = 0.002f;

        public WeatherMakerLoopingAudioSource AudioSourceLight { get; private set; }
        public WeatherMakerLoopingAudioSource AudioSourceMedium { get; private set; }
        public WeatherMakerLoopingAudioSource AudioSourceHeavy { get; private set; }
        public WeatherMakerLoopingAudioSource AudioSourceCurrent { get; private set; }

        public Material Material { get; private set; }
        public Material MaterialSecondary { get; private set; }
        public Material ExplosionMaterial { get; private set; }
        public Material MistMaterial { get; private set; }

        [NonSerialized]
        private float lastIntensityValue = -1.0f;

        [NonSerialized]
        private float lastIntensityMultiplierValue = -1.0f;

        [NonSerialized]
        private float lastSecondaryIntensityMultiplierValue = -1.0f;

        [NonSerialized]
        private float lastMistIntensityMultiplierValue = -1.0f;

        private void PlayParticleSystem(ParticleSystem p, int baseEmissionRate, float intensityMultiplier)
        {
            var e = p.emission;
            ParticleSystem.MinMaxCurve rate = p.emission.rateOverTime;
            rate.mode = ParticleSystemCurveMode.Constant;
            rate.constantMin = rate.constantMax = baseEmissionRate * Intensity * intensityMultiplier;
            e.rateOverTime = rate;
            var m = p.main;
            m.maxParticles = (int)Mathf.Max(m.maxParticles, rate.constantMax * m.startLifetime.constantMax);
            if (!p.isEmitting)
            {
                p.Play();
            }
        }

        private void CheckForIntensityChange()
        {
            if (lastIntensityValue == Intensity && lastIntensityMultiplierValue == IntensityMultiplier &&
                lastSecondaryIntensityMultiplierValue == SecondaryIntensityMultiplier && lastMistIntensityMultiplierValue == MistIntensityMultiplier)
            {
                return;
            }

            lastIntensityValue = Intensity;
            lastIntensityMultiplierValue = IntensityMultiplier;
            lastSecondaryIntensityMultiplierValue = SecondaryIntensityMultiplier;
            lastMistIntensityMultiplierValue = MistIntensityMultiplier;

            if (Intensity < 0.01f)
            {
                if (AudioSourceCurrent != null)
                {
                    AudioSourceCurrent.Stop();
                    AudioSourceCurrent = null;
                }
                if (ParticleSystem != null && ParticleSystem.isEmitting)
                {
                    ParticleSystem.Stop();
                }
                if (ParticleSystemSecondary != null && ParticleSystemSecondary.isEmitting)
                {
                    ParticleSystemSecondary.Stop();
                }
                if (MistParticleSystem != null && MistParticleSystem.isEmitting)
                {
                    MistParticleSystem.Stop();
                }
            }
            else
            {
                WeatherMakerLoopingAudioSource newSource;
                if (Intensity >= SoundHeavyIntensityThreshold)
                {
                    newSource = AudioSourceHeavy;
                }
                else if (Intensity >= SoundMediumIntensityThreshold)
                {
                    newSource = AudioSourceMedium;
                }
                else
                {
                    newSource = AudioSourceLight;
                }
                if (AudioSourceCurrent != newSource)
                {
                    if (AudioSourceCurrent != null)
                    {
                        AudioSourceCurrent.Stop();
                    }
                    AudioSourceCurrent = newSource;
                    AudioSourceCurrent.Play(1.0f);
                }
                AudioSourceCurrent.SecondaryVolumeModifier = Mathf.Pow(Intensity, 0.3f);
                if (ParticleSystem != null)
                {
                    PlayParticleSystem(ParticleSystem, BaseEmissionRate, IntensityMultiplier);
                }
                if (ParticleSystemSecondary != null)
                {
                    if (SecondaryThreshold >= Intensity)
                    {
                        ParticleSystemSecondary.Stop();
                    }
                    else
                    {
                        PlayParticleSystem(ParticleSystemSecondary, BaseEmissionRateSecondary, SecondaryIntensityMultiplier);
                    }
                }
                if (MistParticleSystem != null)
                {
                    if (MistThreshold >= Intensity)
                    {
                        MistParticleSystem.Stop();
                    }
                    else
                    {
                        PlayParticleSystem(MistParticleSystem, BaseMistEmissionRate, MistIntensityMultiplier);
                    }
                }
            }
        }

        private void CheckForParticleSystem()
        {

#if DEBUG

            if (ParticleSystem == null)
            {
                Debug.LogError("Particle system is null");
                return;
            }

#endif

        }

        private void UpdateInitialParticleSystemValues(ParticleSystem p, System.Action<Vector2> startSpeed, System.Action<KeyValuePair<Vector3, Vector3>> startSize)
        {
            if (p != null)
            {
                var m = p.main;
                startSpeed(new Vector2(m.startSpeed.constantMin, m.startSpeed.constantMax));
                var curveX = m.startSizeX;
                var curveY = m.startSizeY;
                var curveZ = m.startSizeZ;
                startSize(new KeyValuePair<Vector3, Vector3>(new Vector3(curveX.constantMin, curveY.constantMin, curveZ.constantMin),
                    new Vector3(curveX.constantMax, curveY.constantMax, curveZ.constantMax)));
            }
        }

        protected virtual void OnCollisionEnabledChanged() { }

        protected virtual void Awake()
        {
            CheckForParticleSystem();
            AudioSourceLight = new WeatherMakerLoopingAudioSource(LoopSourceLight);
            AudioSourceMedium = new WeatherMakerLoopingAudioSource(LoopSourceMedium);
            AudioSourceHeavy = new WeatherMakerLoopingAudioSource(LoopSourceHeavy);
            UpdateInitialParticleSystemValues(ParticleSystem, (f) => InitialStartSpeed = f, (f) => InitialStartSize = f);
            UpdateInitialParticleSystemValues(ParticleSystemSecondary, (f) => InitialStartSpeedSecondary = f, (f) => InitialStartSizeSecondary = f);
            UpdateInitialParticleSystemValues(MistParticleSystem, (f) => InitialStartSpeedMist = f, (f) => InitialStartSizeMist = f);
            Material = new Material(ParticleSystem.GetComponent<Renderer>().sharedMaterial);
            MaterialSecondary = (ParticleSystemSecondary == null ? null : new Material(ParticleSystemSecondary.GetComponent<Renderer>().sharedMaterial));
            MistMaterial = (MistParticleSystem == null ? null : new Material(MistParticleSystem.GetComponent<Renderer>().sharedMaterial));
            ExplosionMaterial = (ExplosionParticleSystem == null ? null : new Material(ExplosionParticleSystem.GetComponent<Renderer>().sharedMaterial));
        }

        protected virtual void Start()
        {

        }

        protected virtual void Update()
        {
            CheckForIntensityChange();
            AudioSourceLight.Update();
            AudioSourceMedium.Update();
            AudioSourceHeavy.Update();
            if (MistMaterial != null)
            {
                MistMaterial.SetFloat("_ParticleDitherLevel", DitherLevel);
            }
        }

        protected virtual void FixedUpdate()
        {
        }

        protected Vector2 InitialStartSpeed { get; private set; }
        protected KeyValuePair<Vector3, Vector3> InitialStartSize { get; private set; }
        protected Vector2 InitialStartSpeedSecondary { get; private set; }
        protected KeyValuePair<Vector3, Vector3> InitialStartSizeSecondary { get; private set; }
        protected Vector2 InitialStartSpeedMist { get; private set; }
        protected KeyValuePair<Vector3, Vector3> InitialStartSizeMist { get; private set; }

        public void SetVolumeModifier(float modifier)
        {
            if (AudioSourceLight == null)
            {
                return;
            }
            AudioSourceLight.VolumeModifier = AudioSourceMedium.VolumeModifier = AudioSourceHeavy.VolumeModifier = modifier;
        }

        private bool collisionEnabled;
        public bool CollisionEnabled
        {
            get { return collisionEnabled; }
            set
            {
                if (value != collisionEnabled)
                {
                    collisionEnabled = value;
                    OnCollisionEnabledChanged();
                }
            }
        }

        public virtual void PreCullCamera(Camera c) { }
    }
}