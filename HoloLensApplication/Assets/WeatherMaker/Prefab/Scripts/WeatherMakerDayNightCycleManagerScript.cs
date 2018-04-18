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
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace DigitalRuby.WeatherMaker
{
    [ExecuteInEditMode]
    public class WeatherMakerDayNightCycleManagerScript : MonoBehaviour
    {
        [Tooltip("Day night cycle profile and color scheme")]
        public WeatherMakerDayNightCycleProfileScript DayNightProfile;

#if UNITY_EDITOR

#pragma warning disable 0414

        [ReadOnlyLabel]
        [SerializeField]
        private string TimeOfDayLabel = string.Empty;

#pragma warning restore 0414

#endif

        /// <summary>
        /// Day speed
        /// </summary>
        public float Speed { get { return DayNightProfile.Speed; } set { DayNightProfile.Speed = value; } }

        /// <summary>
        /// Night speed
        /// </summary>
        public float NightSpeed { get { return DayNightProfile.NightSpeed; } set { DayNightProfile.NightSpeed = value; } }

        /// <summary>
        /// Get / set the time of day in seconds, 0 to 86400
        /// </summary>
        public float TimeOfDay { get { return DayNightProfile.TimeOfDay; } set { DayNightProfile.TimeOfDay = value; } }

        /// <summary>
        /// Time of day category
        /// </summary>
        public WeatherMakerTimeOfDayCategory TimeOfDayCategory { get { return DayNightProfile.TimeOfDayCategory; } }

        /// <summary>
        /// Time of day as a TimeSpan object
        /// </summary>
        public TimeSpan TimeOfDayTimespan { get { return DayNightProfile.TimeOfDayTimespan; } }

        /// <summary>
        /// Time zone offset in seconds
        /// </summary>
        public int TimeZoneOffsetSeconds { get { return DayNightProfile.TimeZoneOffsetSeconds; } set { DayNightProfile.TimeZoneOffsetSeconds = value; } }

        /// <summary>
        /// Year
        /// </summary>
        public int Year {  get { return DayNightProfile.Year; } set { DayNightProfile.Year = value; } }

        /// <summary>
        /// Month
        /// </summary>
        public int Month { get { return DayNightProfile.Month; } set { DayNightProfile.Month = value; } }

        /// <summary>
        /// Day
        /// </summary>
        public int Day { get { return DayNightProfile.Day; } set { DayNightProfile.Day = value; } }

        /// <summary>
        /// Latitude in degrees
        /// </summary>
        public double Latitude { get { return DayNightProfile.Latitude; } set { DayNightProfile.Latitude = value; } }

        /// <summary>
        /// Longitude in degrees
        /// </summary>
        public double Longitude { get { return DayNightProfile.Longitude; } set { DayNightProfile.Longitude = value; } }

        /// <summary>
        /// 1 if it is fully day
        /// </summary>
        public float DayMultiplier { get { return DayNightProfile.DayMultiplier; } }

        /// <summary>
        /// 1 if it is fully dawn or dusk
        /// </summary>
        public float DawnDuskMultiplier { get { return DayNightProfile.DawnDuskMultiplier; } }

        /// <summary>
        /// 1 if it is fully night
        /// </summary>
        public float NightMultiplier { get { return DayNightProfile.NightMultiplier; } }

        /// <summary>
        /// Sun data
        /// </summary>
        public WeatherMakerDayNightCycleProfileScript.SunInfo SunData { get { return DayNightProfile.SunData; } }

        /// <summary>
        /// Directional light intensity multipliers - all are applied to the final directional light intensities
        /// </summary>
        [NonSerialized]
        public readonly Dictionary<string, float> DirectionalLightIntensityMultipliers = new Dictionary<string, float>();

        /// <summary>
        /// Directional light shadow intensity multipliers - all are applied to the final directional light shadow intensities
        /// </summary>
        [NonSerialized]
        public readonly Dictionary<string, float> DirectionalLightShadowIntensityMultipliers = new Dictionary<string, float>();

        private void EnsureProfile()
        {
            if (DayNightProfile == null)
            {
                DayNightProfile = Resources.Load<WeatherMakerDayNightCycleProfileScript>("WeatherMakerDayNightCycleProfile_Default");
            }

#if UNITY_EDITOR

            if (Application.isPlaying)
            {

#endif

                DayNightProfile = ScriptableObject.Instantiate(DayNightProfile);

#if UNITY_EDITOR

            }

#endif

        }

        private void Start()
        {
            EnsureProfile();
            DayNightProfile.UpdateFromProfile(WeatherMakerScript.Instance.Camera, WeatherMakerScript.Instance.Sun, WeatherMakerScript.Instance.Moons,
                DirectionalLightIntensityMultipliers, DirectionalLightShadowIntensityMultipliers,
                WeatherMakerScript.Instance.NetworkScript.IsServer);
        }

        private void Update()
        {
            DayNightProfile.UpdateFromProfile(WeatherMakerScript.Instance.Camera, WeatherMakerScript.Instance.Sun, WeatherMakerScript.Instance.Moons,
                DirectionalLightIntensityMultipliers, DirectionalLightShadowIntensityMultipliers,
                WeatherMakerScript.Instance.NetworkScript.IsServer);

#if UNITY_EDITOR

            TimeOfDayLabel = DayNightProfile.TimeOfDayLabel;

#endif

        }
    }
}

// resources:
// https://en.wikipedia.org/wiki/Position_of_the_Sun
// http://stackoverflow.com/questions/8708048/position-of-the-sun-given-time-of-day-latitude-and-longitude
// http://www.grasshopper3d.com/forum/topics/solar-calculation-plugin
// http://guideving.blogspot.nl/2010/08/sun-position-in-c.html
// https://github.com/mourner/suncalc
// http://stackoverflow.com/questions/1058342/rough-estimate-of-the-time-offset-from-gmt-from-latitude-longitude
// http://www.stjarnhimlen.se/comp/tutorial.html
// http://www.suncalc.net/#/40.7608,-111.891,12/2000.09.21/12:46
// http://www.suncalc.net/scripts/suncalc.js

// total eclipse:
// 43.7678
// -111.8323
// Maximum eclipse : 	2017/08/21	17:34:18.6	49.5°	133.1°