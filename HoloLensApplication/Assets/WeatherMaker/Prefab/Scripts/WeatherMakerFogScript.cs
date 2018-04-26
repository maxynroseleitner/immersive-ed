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
using System.Collections;

namespace DigitalRuby.WeatherMaker
{
    public abstract class WeatherMakerFogScript<T> : MonoBehaviour where T : WeatherMakerFogProfileScript
    {
        [Header("Fog profile and material")]
        [Tooltip("Fog profile")]
        public T FogProfile;

        [Tooltip("Fog material")]
        public Material FogMaterial;

        private bool initialized;

        /// <summary>
        /// Shortcut to fog profile density
        /// </summary>
        public float FogDensity
        {
            get { return FogProfile == null ? 0.0f : FogProfile.FogDensity; }
            set { if (FogProfile != null) { FogProfile.FogDensity = value; } }
        }

        /// <summary>
        /// Set a new fog density over a period of time - if set to 0, game object will be disabled at end of transition
        /// </summary>
        /// <param name="fromDensity">Start of new fog density</param>
        /// <param name="toDensity">End of new fog density</param>
        /// <param name="transitionDuration">How long to transition to the new fog density in seconds</param>
        public void TransitionFogDensity(float fromDensity, float toDensity, float transitionDuration)
        {
            if (!isActiveAndEnabled)
            {
                Debug.LogError("Fog script must be enabled to show fog");
                return;
            }

            FogProfile.FogDensity = fromDensity;
            TweenFactory.Tween("WeatherMakerFog_" + gameObject.name, fromDensity, toDensity, transitionDuration, TweenScaleFunctions.Linear, (v) =>
            {
                FogProfile.FogDensity = v.CurrentValue;
            }, null);
        }

        protected virtual void Awake()
        {

        }

        protected virtual void Start()
        {
            
        }

        protected virtual void UpdateFogMaterialFromProfile()
        {
            FogProfile.UpdateMaterialProperties(FogMaterial, null);
        }

        protected virtual void LateUpdate()
        {
            if (!initialized)
            {
                Initialize();
                OnInitialize();
                initialized = true;
            }
            UpdateFogMaterialFromProfile();
        }

        protected virtual void OnDestroy()
        {
        }

        protected virtual void OnEnable()
        {
        }

        protected virtual void OnDisable()
        {
        }

        protected virtual void OnWillRenderObject()
        {
        }

        protected virtual void OnBecameVisible()
        {
        }

        protected virtual void OnBecameInvisible()
        {
        }

        protected virtual void OnInitialize()
        {

        }

        private void Initialize()
        {
            if (FogProfile == null)
            {
                FogProfile = Resources.Load<T>("WeatherMakerFogProfile_Default");
            }

#if UNITY_EDITOR

            if (Application.isPlaying)
            {

#endif

                // clone fog material and profile
                if (FogMaterial != null)
                {
                    FogMaterial = new Material(FogMaterial);
                }
                FogProfile = ScriptableObject.Instantiate(FogProfile);

#if UNITY_EDITOR

            }

#endif

        }
    }
}
