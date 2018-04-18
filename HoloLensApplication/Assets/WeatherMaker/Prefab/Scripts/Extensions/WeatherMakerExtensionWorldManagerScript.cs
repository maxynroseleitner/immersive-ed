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

#if WORLDAPI_PRESENT

using WAPI;

#endif

namespace DigitalRuby.WeatherMaker
{
    public class WeatherMakerExtensionWorldManagerScript : WeatherMakerExtensionRainSnowSeasonScript

#if WORLDAPI_PRESENT

        <WorldManager>, IWorldApiChangeHandler

#else

        <UnityEngine.MonoBehaviour>

#endif

    {

#if WORLDAPI_PRESENT
        
        [Tooltip("The minimum rain power.")]
        [Range(0.0f, 1.0f)]
        public float MinRainPower = 0.0f;

        [Tooltip("The minimum snow power.")]
        [Range(0.0f, 1.0f)]
        public float MinSnowPower = 0.0f;

        [Tooltip("How much cloud cover reduces specular highlights from directional light coming off the water. 0 for none, higher for more reduction.")]
        [Range(0.0f, 4.0f)]
        public float CloudCoverWaterSpecularPower = 2.0f;

        [Tooltip("How much cloud cover reduces reflections coming off the water. 0 for none, higher for more reduction.")]
        [Range(0.0f, 4.0f)]
        public float CloudCoverWaterReflectionPower = 2.0f;

        private void Start()
        {
            ConnectToWorldAPI();
        }

        private void OnDestroy()
        {
            DisconnectFromWorldAPI();
        }

        /// <summary>
        /// Start listening to world api messaged
        /// </summary>
        private void ConnectToWorldAPI()
        {
            WorldManager.Instance.AddListener(this);
        }

        /// <summary>
        /// Stop listening to world api messages
        /// </summary>
        private void DisconnectFromWorldAPI()
        {
            WorldManager.Instance.RemoveListener(this);
        }

        /// <summary>
        /// This method is called when the class has been added as a listener, and something has changed 
        /// one of the WAPI settings.
        /// 
        /// Use the HasChanged method to work out what was changed and respond accordingly. 
        /// 
        /// NOTE : As the majority of the World API values are properties, setting something 
        /// is as easy as reading its value, and setting a property will cause another
        /// OnWorldChanged event to be raised.
        /// 
        /// </summary>
        /// <param name="changeArgs"></param>
        public void OnWorldChanged(WorldChangeArgs changeArgs)
        {
            //throw new System.NotImplementedException();
        }

        protected override void OnUpdateRain(float rain)
        {
            WorldManager.Instance.RainPowerTerrain = rain;
        }

        protected override void OnUpdateSnow(float snow)
        {
            WorldManager.Instance.SnowPowerTerrain = snow;
        }

        protected override void OnUpdateSeason(float season)
        {
            WorldManager.Instance.Season = season;
        }

#endif

    }
}

