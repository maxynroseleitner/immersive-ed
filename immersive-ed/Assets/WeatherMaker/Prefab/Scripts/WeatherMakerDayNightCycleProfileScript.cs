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
    [CreateAssetMenu(fileName = "WeatherMakerDayNightCycleProfile", menuName = "WeatherMaker/Day-Night Cycle Profile", order = 8)]
    public class WeatherMakerDayNightCycleProfileScript : ScriptableObject
    {
        #region Classes

        private class WebClientWithTimeout : System.Net.WebClient
        {
            protected override System.Net.WebRequest GetWebRequest(Uri uri)
            {
                System.Net.WebRequest w = base.GetWebRequest(uri);
                w.Timeout = Timeout;
                return w;
            }

            /// <summary>
            /// Milliseconds
            /// </summary>
            public int Timeout { get; set; }
        }

        public class SunInfo
        {
            /// <summary>
            /// Calculation parameter, the date/time on the observer planet
            /// </summary>
            public DateTime DateTime;

            /// <summary>
            /// Calculation parameter, latitude of observer planet in degrees
            /// </summary>
            public double Latitude;

            /// <summary>
            /// Calculation parameter, longitude of observer planet in degrees
            /// </summary>
            public double Longitude;

            /// <summary>
            /// Calculation parameter, axis tilt of observer planet in degrees
            /// </summary>
            public double AxisTilt;

            /// <summary>
            /// Position (unit vector) of the sun in the sky from origin
            /// </summary>
            public Vector3 UnitVectorUp;

            /// <summary>
            /// Normal (unit vector) of the sun in the sky pointing to origin (negation of Position)
            /// </summary>
            public Vector3 UnitVectorDown;

            /// <summary>
            /// Time of dawn
            /// </summary>
            public TimeSpan Dawn;

            /// <summary>
            /// Time of sunrise
            /// </summary>
            public TimeSpan SunRise;

            /// <summary>
            /// Time of sunset
            /// </summary>
            public TimeSpan SunSet;

            /// <summary>
            /// Time of dusk
            /// </summary>
            public TimeSpan Dusk;

            // the rest of these are stored in case needed and are best understood by a Google or Bing search
            public double JulianDays;
            public double Declination;
            public double RightAscension;
            public double Azimuth;
            public double Altitude;
            public double SolarMeanAnomaly;
            public double EclipticLongitude;
            public double SiderealTime;
        }

        public class MoonInfo
        {
            /// <summary>
            /// The sun data used to calculate the moon info
            /// </summary>
            public SunInfo SunData;

            /// <summary>
            /// Position (unit vector) of the moon in the sky from origin
            /// </summary>
            public Vector3 UnitVectorUp;

            /// <summary>
            /// Normal (unit vector) of the moon in the sky pointing to origin (negation of Position)
            /// </summary>
            public Vector3 UnitVectorDown;

            /// <summary>
            /// Distance in kilometers
            /// </summary>
            public double Distance;

            /// <summary>
            /// Moon illumination phase (0.5 is full, 0.0 to 1.0)
            /// </summary>
            public double Phase;

            /// <summary>
            /// Percent (0 to 1) that moon is full
            /// </summary>
            public double PercentFull;

            /// <summary>
            /// Moon illumination angle
            /// </summary>
            public double Angle;

            /// <summary>
            /// Moon illumination fraction
            /// </summary>
            public double Fraction;

            // the rest of these are stored in case needed and are best understood by a Google or Bing search
            public double Azimuth;
            public double Altitude;
            public double RightAscension;
            public double Declination;
            public double LunarMeanAnomaly;
            public double EclipticLongitude;
            public double SiderealTime;
            public double ParallacticAngle;
        }

        #endregion Classes

        [Header("Day/night cycle")]
        [Range(-100000, 100000.0f)]
        [Tooltip("The day speed of the cycle. Set to 0 to freeze the cycle and manually control it. At a speed of 1, the cycle is in real-time. " +
            "A speed of 100 is 100 times faster than normal. Negative numbers run the cycle backwards.")]
        public float Speed = 10.0f;

        [Range(-100000, 100000.0f)]
        [Tooltip("The night speed of the cycle. Set to 0 to freeze the cycle and manually control it. At a speed of 1, the cycle is in real-time. " +
            "A speed of 100 is 100 times faster than normal. Negative numbers run the cycle backwards.")]
        public float NightSpeed = 10.0f;

        [Tooltip("How often the update cycle updates. Use higher values if you have issues with shadow flickering, etc. Turning on temporal anti-aliasing " +
            "is also a good way to reduce flicker, though you may have to play with the settings.")]
        [Range(0.0f, 10.0f)]
        public float UpdateInterval = 0.0f;

        /// <summary>
        /// How much time has accumulated for the next update
        /// </summary>
        private float accumulatedTime = 0.0f;

        [Range(0.0f, SecondsPerDay)]
        [Tooltip("The current time of day in seconds (local time).")]
        public float TimeOfDay = SecondsPerDay * 0.5f; // high noon default time of day

#if UNITY_EDITOR

#pragma warning disable 0414

        [ReadOnlyLabel]
        [SerializeField]
        internal string TimeOfDayLabel = string.Empty;

#pragma warning restore 0414

#endif

        [Header("Date")]
        [Tooltip("The year for simulating the sun and moon position - this can change during runtime. " +
            "The calculation is only correct for dates in the range March 1 1900 to February 28 2100.")]
        [Range(1900, 2100)]
        public int Year = 2000;

        [Tooltip("The month for simulating the sun and moon position - this can change during runtime.")]
        [Range(1, 12)]
        public int Month = 9;

        [Tooltip("The day for simulating the sun and moon position - this can change during runtime.")]
        [Range(1, 31)]
        public int Day = 21;

        [Tooltip("Whether to adjust the date when the day ends. This is important to maintain accurate sun and moon positions as days begin and end, but if your time is static you can turn it off.")]
        public bool AdjustDateWhenDayEnds = true;

        [Tooltip("Offset for the time zone of the lat / lon in seconds. Set to -1111 to auto-calculate (just tab out of the text field after you type -1111). Note about -1111: during editor mode, a web service is used. During play mode, longitude is used for fast calculation.")]
        public int TimeZoneOffsetSeconds = -21600;

        [Header("Location")]
        [Range(-90.0f, 90.0f)]
        [Tooltip("The latitude in degrees on the planet that the camera is at - 90 (north pole) to -90 (south pole)")]
        public double Latitude = 40.7608; // salt lake city latitude

        [Range(-180.0f, 180.0f)]
        [Tooltip("The longitude in degrees on the planet that the camera is at. -180 to 180.")]
        public double Longitude = -111.8910; // salt lake city longitude

        [Range(0.0f, 360.0f)]
        [Tooltip("The amount of degrees your planet is tilted - Earth is about 23.439f")]
        public float AxisTilt = 23.439f;

        [Header("Ambient color modifiers")]
        [Tooltip("Whether to set the Unity built in ambient colors to the day, dawn/dusk and night ambient colors.")]
        public WeatherMakerDayNightAmbientColorMode AmbientColorMode = WeatherMakerDayNightAmbientColorMode.AmbientColor;

        [Tooltip("Determines when it is day, dawn/dusk or night, where center of gradient is sun at horizon. Green = day, Red = dawn/dusk, Blue = night. If it's dawn/dusk, the time of day before 12 p.m. will be dawn, after 12 p.m. will be dusk.")]
        public Gradient DayDawnDuskNightGradient;

        [Header("Ambient colors - day")]
        [Tooltip("Day ambient color, where far right is fully day - alpha is used for intensity")]
        public Gradient DayAmbientColor;

        [Tooltip("Additional day ambient color intensity")]
        [Range(0.0f, 10.0f)]
        public float DayAmbientColorIntensity = 0.05f;

        [Tooltip("Day ambient sky color, where far right is fully day - alpha is used for intensity")]
        public Gradient DayAmbientColorSky;

        [Tooltip("Additional day ambient sky color intensity")]
        [Range(0.0f, 10.0f)]
        public float DayAmbientColorSkyIntensity = 0.05f;

        [Tooltip("Day ambient ground color, where far right is fully day - alpha is used for intensity")]
        public Gradient DayAmbientColorGround;

        [Tooltip("Additional day ambient ground color intensity")]
        [Range(0.0f, 10.0f)]
        public float DayAmbientColorGroundIntensity = 0.05f;

        [Tooltip("Day ambient equator color, where far right is fully day - alpha is used for intensity")]
        public Gradient DayAmbientColorEquator;

        [Tooltip("Additional day ambient equator color intensity")]
        [Range(0.0f, 10.0f)]
        public float DayAmbientColorEquatorIntensity = 0.05f;

        [Header("Ambient colors - dawn/dusk")]
        [Tooltip("Dawn/dusk ambient color, where far right is fully dawn or dusk - alpha is used for intensity")]
        public Gradient DawnDuskAmbientColor;

        [Tooltip("Additional daw/dusk ambient color intensity")]
        [Range(0.0f, 10.0f)]
        public float DawnDuskAmbientColorIntensity = 0.05f;

        [Tooltip("Dawn/dusk ambient sky color, where far right is fully dawn or dusk - alpha is used for intensity")]
        public Gradient DawnDuskAmbientColorSky;

        [Tooltip("Additional daw/dusk ambient sky color intensity")]
        [Range(0.0f, 10.0f)]
        public float DawnDuskAmbientColorSkyIntensity = 0.05f;

        [Tooltip("Dawn/dusk ambient ground color, where far right is fully dawn or dusk - alpha is used for intensity")]
        public Gradient DawnDuskAmbientColorGround;

        [Tooltip("Additional daw/dusk ambient ground color intensity")]
        [Range(0.0f, 10.0f)]
        public float DawnDuskAmbientColorGroundIntensity = 0.05f;

        [Tooltip("Dawn/dusk ambient equator color, where far right is fully dawn or dusk - alpha is used for intensity")]
        public Gradient DawnDuskAmbientColorEquator;

        [Tooltip("Additional daw/dusk ambient equator color intensity")]
        [Range(0.0f, 10.0f)]
        public float DawnDuskAmbientColorEquatorIntensity = 0.05f;

        [Header("Ambient colors - night")]
        [Tooltip("Night ambient color, where far right is fully night - alpha is used for intensity")]
        public Gradient NightAmbientColor;

        [Tooltip("Additional night ambient color intensity")]
        [Range(0.0f, 10.0f)]
        public float NightAmbientColorIntensity = 0.05f;

        [Tooltip("Night ambient sky color, where far right is fully night - alpha is used for intensity")]
        public Gradient NightAmbientColorSky;

        [Tooltip("Additional night ambient sky color intensity")]
        [Range(0.0f, 10.0f)]
        public float NightAmbientColorSkyIntensity = 0.05f;

        [Tooltip("Night ambient ground color, where far right is fully night - alpha is used for intensity")]
        public Gradient NightAmbientColorGround;

        [Tooltip("Additional night ambient ground color intensity")]
        [Range(0.0f, 10.0f)]
        public float NightAmbientColorGroundIntensity = 0.05f;

        [Tooltip("Night ambient equator color, where far right is fully night - alpha is used for intensity")]
        public Gradient NightAmbientColorEquator;

        [Tooltip("Additional night ambient equator color intensity")]
        [Range(0.0f, 10.0f)]
        public float NightAmbientColorEquatorIntensity = 0.05f;

        [Header("Sun modifiers")]
        [Tooltip("Tint color of sun, where center is sun at horizon")]
        public Gradient SunTintColorGradient;

        [Tooltip("Control sun intensity where center of gradient is sun at horizon")]
        public Gradient SunIntensityGradient;

        [Tooltip("Control sun shadow strength where center of gradient is sun at horizon")]
        public Gradient SunShadowStrengthGradient;

        /// <summary>
        /// Time of day as a TimeSpan object
        /// </summary>
        public TimeSpan TimeOfDayTimespan { get; private set; }

        /// <summary>
        /// Time of day category
        /// </summary>
        public WeatherMakerTimeOfDayCategory TimeOfDayCategory { get; private set; }

        /// <summary>
        /// 1 if it is fully day
        /// </summary>
        public float DayMultiplier { get; private set; }

        /// <summary>
        /// 1 if it is fully dawn or dusk
        /// </summary>
        public float DawnDuskMultiplier { get; private set; }

        /// <summary>
        /// 1 if it is fully night
        /// </summary>
        public float NightMultiplier { get; private set; }

        /// <summary>
        /// Current sun info
        /// </summary>
        [NonSerialized]
        public readonly SunInfo SunData = new SunInfo();

        /// <summary>
        /// Current moon info
        [NonSerialized]
        public readonly List<MoonInfo> MoonDatas = new List<MoonInfo>();

        /// <summary>
        /// Number of seconds per day
        /// </summary>
        public const float SecondsPerDay = 86400.0f;

        /// <summary>
        /// Time of day at high noon
        /// </summary>
        public const float HighNoonTimeOfDay = SecondsPerDay * 0.5f;

        /// <summary>
        /// Number of seconds in one degree
        /// </summary>
        public const float SecondsForOneDegree = SecondsPerDay / 360.0f;

        /// <summary>
        /// How many seconds must elapse in time of day time to update dynamic GI should that option be selected
        /// </summary>
        public const float DynamicGIUpdateThresholdSeconds = 300.0f;

        private float lastTimeOfDayForDynamicGIUpdate = -999999.0f;

        public static DateTime JulianToDateTime(double julianDate)
        {
            double unixTime = (julianDate - 2440587.5) * 86400;
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTime);
            return dtDateTime;
        }

        public static void ConvertAzimuthAtltitudeToUnitVector(double azimuth, double altitude, ref Vector3 v)
        {
            v.y = (float)Math.Sin(altitude);
            float hyp = (float)Math.Cos(altitude);
            v.z = hyp * (float)Math.Cos(azimuth);
            v.x = hyp * (float)Math.Sin(azimuth);
        }

        /// <summary>
        /// Calculate the position of the sun
        /// </summary>
        /// <param name="sunInfo">Calculates and receives sun info, including position, etc. Parameters marked as calculation parameters need to be set first.</param>
        /// <param name="rotateYDegrees">Rotate around the Y axis</param>
        public static void CalculateSunPosition(SunInfo sunInfo, float rotateYDegrees)
        {
            // dateTime should already be UTC format
            double d = (sunInfo.DateTime.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds / dayMs) + jDiff;
            //double d = sunInfo.DateTime.ToOADate() + 2415018.5;
            double e = degreesToRadians * sunInfo.AxisTilt; // obliquity of the Earth
            double m = SolarMeanAnomaly(d);
            double l = EclipticLongitude(m);
            double dec = Declination(e, l, 0);
            double ra = RightAscension(e, l, 0);
            double lw = -degreesToRadians * sunInfo.Longitude;
            double phi = degreesToRadians * sunInfo.Latitude;
            double h = SiderealTime(d, lw) - ra;
            double azimuth = Azimuth(h, phi, dec);
            double altitude = Altitude(h, phi, dec);
            ConvertAzimuthAtltitudeToUnitVector(azimuth, altitude, ref sunInfo.UnitVectorUp);

            sunInfo.UnitVectorUp = Quaternion.AngleAxis(rotateYDegrees, Vector3.up) * sunInfo.UnitVectorUp;
            sunInfo.UnitVectorDown = -sunInfo.UnitVectorUp;
            sunInfo.JulianDays = d;
            sunInfo.Declination = dec;
            sunInfo.RightAscension = ra;
            sunInfo.Azimuth = azimuth;
            sunInfo.Altitude = altitude;
            sunInfo.SolarMeanAnomaly = m;
            sunInfo.EclipticLongitude = l;
            sunInfo.SiderealTime = h;

            double n = JulianCycle(d, lw);
            double ds = ApproxTransit(0, lw, n);
            double jnoon = SolarTransit(ds, m, l);
            double jSunSet = JulianDateForSunAltitude(-0.8 * (Math.PI / 180.0), lw, phi, dec, n, m, l);
            double jSunRise = jnoon - (jSunSet - jnoon);
            double jDusk = JulianDateForSunAltitude(-10.0 * (Math.PI / 180.0), lw, phi, dec, n, m, l);
            double jDawn = jnoon - (jDusk - jnoon);

            try
            {
                sunInfo.Dawn = JulianToDateTime(jDawn).TimeOfDay;
                sunInfo.Dusk = JulianToDateTime(jDusk).TimeOfDay;
                sunInfo.SunRise = JulianToDateTime(jSunRise).TimeOfDay;
                sunInfo.SunSet = JulianToDateTime(jSunSet).TimeOfDay;
            }
            catch
            {
                // don't crash if date time is out of bounds
            }
        }

        /// <summary>
        /// Calculate moon position
        /// </summary>
        /// <param name="sunInfo">Sun info, already calculated</param>
        /// <param name="moonInfo">Receives moon info</param>
        /// <param name="rotateYDegrees">Rotate the moon in the sky around the y axis by this degrees</param>
        public static void CalculateMoonPosition(SunInfo sunInfo, MoonInfo moonInfo, float rotateYDegrees)
        {
            double d = sunInfo.JulianDays;
            double e = degreesToRadians * sunInfo.AxisTilt; // obliquity of the Earth
            double L = degreesToRadians * (218.316 + 13.176396 * d); // ecliptic longitude
            double M = degreesToRadians * (134.963 + 13.064993 * d); // mean anomaly
            double F = degreesToRadians * (93.272 + 13.229350 * d); // mean distance
            double l = L + degreesToRadians * 6.289 * Math.Sin(M); // longitude
            double b = degreesToRadians * 5.128 * Math.Sin(F); // latitude
            double dist = 385001.0 - (20905.0 * Math.Cos(M)); // distance to the moon in km
            double ra = RightAscension(e, l, b);
            double dec = Declination(e, l, b);
            const double sunDistance = 149598000.0; // avg sun distance to Earth
            double phi = Math.Acos(Math.Sin(sunInfo.Declination) * Math.Sin(dec) + Math.Cos(sunInfo.Declination) * Math.Cos(dec) * Math.Cos(sunInfo.RightAscension - ra));
            double inc = Math.Atan2(sunDistance * Math.Sin(phi), dist - sunDistance * Math.Cos(phi));
            double angle = Math.Atan2(Math.Cos(sunInfo.Declination) * Math.Sin(sunInfo.RightAscension - ra), Math.Sin(sunInfo.Declination) * Math.Cos(dec) - Math.Cos(sunInfo.Declination) * Math.Sin(dec) * Math.Cos(sunInfo.RightAscension - ra));
            double fraction = (1.0 + Math.Cos(inc)) * 0.5;
            double phase = 0.5 + (0.5 * inc * Math.Sign(angle) * (1.0 / Math.PI));
            double lw = -degreesToRadians * sunInfo.Longitude;
            phi = degreesToRadians * sunInfo.Latitude;
            double H = SiderealTime(d, lw) - ra;
            double h = Altitude(H, phi, dec);

            // formula 14.1 of "Astronomical Algorithms" 2nd edition by Jean Meeus (Willmann-Bell, Richmond) 1998.
            double pa = Math.Atan2(Math.Sin(H), Math.Tan(phi) * Math.Cos(dec) - Math.Sin(dec) * Math.Cos(H));
            h = h + AstroRefraction(h); // altitude correction for refraction
            double azimuth = Azimuth(H, phi, dec);
            double altitude = h;
            ConvertAzimuthAtltitudeToUnitVector(azimuth, altitude, ref moonInfo.UnitVectorUp);

            // set moon position and look at the origin
            moonInfo.UnitVectorUp = Quaternion.AngleAxis(rotateYDegrees, Vector3.up) * moonInfo.UnitVectorUp;
            moonInfo.UnitVectorDown = -moonInfo.UnitVectorUp;
            moonInfo.Distance = dist;
            moonInfo.Phase = phase;
            moonInfo.PercentFull = 1.0 - Math.Abs((0.5 - phase) * 2.0);
            moonInfo.Angle = angle;
            moonInfo.Fraction = fraction;
            moonInfo.Azimuth = azimuth;
            moonInfo.Altitude = altitude;
            moonInfo.RightAscension = ra;
            moonInfo.Declination = dec;
            moonInfo.LunarMeanAnomaly = M;
            moonInfo.EclipticLongitude = L;
            moonInfo.SiderealTime = H;
            moonInfo.ParallacticAngle = pa;
        }

        private const double degreesToRadians = Math.PI / 180.0;
        private const double dayMs = 1000.0 * 60.0 * 60.0 * 24.0;
        private const double j0 = 0.0009;
        private const double j1970 = 2440587.5;
        private const double j2000 = 2451545.0;
        private const double jDiff = (j1970 - j2000);

        private static double RightAscension(double e, double l, double b)
        {
            return Math.Atan2(Math.Sin(l) * Math.Cos(e) - Math.Tan(b) * Math.Sin(e), Math.Cos(l));
        }

        private static double Declination(double e, double l, double b)
        {
            return Math.Asin(Math.Sin(b) * Math.Cos(e) + Math.Cos(b) * Math.Sin(e) * Math.Sin(l));
        }

        private static double Azimuth(double h, double phi, double dec)
        {
            return Math.Atan2(Math.Sin(h), Math.Cos(h) * Math.Sin(phi) - Math.Tan(dec) * Math.Cos(phi));
        }

        private static double Altitude(double h, double phi, double dec)
        {
            return Math.Asin(Math.Sin(phi) * Math.Sin(dec) + Math.Cos(phi) * Math.Cos(dec) * Math.Cos(h));
        }

        private static double SiderealTime(double d, double lw)
        {
            return degreesToRadians * (280.16 + 360.9856235 * d) - lw;
        }

        private static double SolarMeanAnomaly(double d)
        {
            return degreesToRadians * (357.5291 + 0.98560028 * d);
        }

        private static double EclipticLongitude(double m)
        {
            double c = degreesToRadians * (1.9148 * Math.Sin(m) + 0.02 * Math.Sin(2.0 * m) + 0.0003 * Math.Sin(3.0 * m)); // equation of center
            double p = degreesToRadians * 102.9372; // perihelion of the Earth
            return m + c + p + Math.PI;
        }

        private static double AstroRefraction(double h)
        {
            // the following formula works for positive altitudes only.
            // if h = -0.08901179 a div/0 would occur.
            h = (h < 0.0 ? 0.0 : h);

            // formula 16.4 of "Astronomical Algorithms" 2nd edition by Jean Meeus (Willmann-Bell, Richmond) 1998.
            // 1.02 / tan(h + 10.26 / (h + 5.10)) h in degrees, result in arc minutes -> converted to rad:
            return 0.0002967 / Math.Tan(h + 0.00312536 / (h + 0.08901179));
        }

        private static double JulianCycle(double d, double lw)
        {
            return Math.Round(d - j0 - lw / (2 * Math.PI));
        }

        private static double ApproxTransit(double Ht, double lw, double n)
        {
            return j0 + (Ht + lw) / (2 * Math.PI) + n;
        }

        private static double SolarTransit(double ds, double M, double L)
        {
            return j2000 + ds + 0.0053 * Math.Sin(M) - 0.0069 * Math.Sin(2 * L);
        }

        private static double HourAngle(double h, double phi, double d)
        {
            return Math.Acos((Math.Sin(h) - Math.Sin(phi) * Math.Sin(d)) / (Math.Cos(phi) * Math.Cos(d)));
        }

        private static double JulianDateForSunAltitude(double h, double lw, double phi, double dec, double n, double M, double L)
        {
            double w = HourAngle(h, phi, dec);
            double a = ApproxTransit(w, lw, n);
            return SolarTransit(a, M, L);
        }

        private static double CorrectAngle(double angleInRadians)
        {
            if (angleInRadians < 0)
            {
                return (2 * Math.PI) + angleInRadians;
            }
            else if (angleInRadians > 2 * Math.PI)
            {
                return angleInRadians - (2 * Math.PI);
            }
            else
            {
                return angleInRadians;
            }
        }

#if UNITY_EDITOR

        private DateTime lastTimeZoneCheck = DateTime.MinValue;

#endif

        private void SetCelestialObjectPosition(WeatherMakerCelestialObject obj, Vector3 transformForward)
        {
            switch (obj.OrbitType)
            {
                case WeatherMakerOrbitType.Custom:
                    IWeatherMakerCelestialObjectCustomOrbit orbitInterface = obj.OrbitTypeCustomScript as IWeatherMakerCelestialObjectCustomOrbit;
                    if (orbitInterface == null)
                    {
                        Debug.LogErrorFormat("Unable to use custom orbit for celestial object {0}, field OrbitTypeCustomScript" +
                            " must be set to a MonoBehavior script implementing IWeatherMakerCelestialObjectCustomOrbit", obj.Transform.gameObject.name);
                    }
                    else
                    {
                        transformForward = orbitInterface.CalculatePosition(obj);
                    }
                    break;

                default:
                    // drop sun faster as it dips below horizon, in summer the sky horizon is visible even at 3am!
                    if (transformForward.y > 0.1f)
                    {
                        transformForward.y = Mathf.Pow(transformForward.y, 0.8f);
                    }
                    break;
            }

            obj.Transform.forward = transformForward;
        }

        private void UpdateSun(Camera camera, WeatherMakerCelestialObject sun, float l)
        {
            if (camera != null && camera.orthographic)
            {
                sun.Transform.rotation = Quaternion.AngleAxis(180.0f + ((TimeOfDay / SecondsPerDay) * 360.0f), Vector3.right);
                float dot = Mathf.Clamp(Vector3.Dot(sun.Transform.forward, Vector3.forward) + 0.5f, 0.0f, 1.0f);
                sun.Light.intensity = sun.LightBaseIntensity * dot;
            }
            else
            {
                // convert local time of day to UTC time of day - quick and dirty calculation
                double offsetSeconds = TimeZoneOffsetSeconds;
                TimeSpan t = TimeSpan.FromSeconds(TimeOfDay - offsetSeconds);
                SunData.DateTime = new DateTime(Year, Month, Day, 0, 0, 0, DateTimeKind.Utc) + t;
                SunData.Latitude = Latitude;
                SunData.Longitude = Longitude;
                SunData.AxisTilt = AxisTilt;

                // calculate and set sun position in sky
                CalculateSunPosition(SunData, sun.RotateYDegrees);
                SetCelestialObjectPosition(sun, SunData.UnitVectorDown);

                // calculate sun intensity and shadow strengths
                float sunIntensityLookup = SunIntensityGradient.Evaluate(l).grayscale;
                sunIntensityLookup *= sunIntensityLookup;
                float sunShadowLookup = SunShadowStrengthGradient.Evaluate(l).grayscale;
                sun.Light.color *= SunTintColorGradient.Evaluate(l);
                sun.Light.intensity = sun.LightBaseIntensity * sunIntensityLookup;
                sun.Light.shadowStrength = sun.LightBaseShadowStrength * sunShadowLookup;
            }
        }

        private void UpdateMoons(Camera camera, WeatherMakerCelestialObject sun, IList<WeatherMakerCelestialObject> moons)
        {
            float dot, yPower;
            while (MoonDatas.Count > moons.Count)
            {
                MoonDatas.RemoveAt(MoonDatas.Count - 1);
            }
            while (MoonDatas.Count < moons.Count)
            {
                MoonDatas.Add(new MoonInfo());
            }

            for (int i = 0; i < moons.Count; i++)
            {
                WeatherMakerCelestialObject moon = moons[i];
                CalculateMoonPosition(SunData, MoonDatas[i], moon.RotateYDegrees);
                SetCelestialObjectPosition(moon, MoonDatas[i].UnitVectorDown);

                // intensity raises squared compare to moon fullness - this means less full is squared amount of less light
                // moon light intensity reduces as sun light intensity approaches 1
                // reduce moon light as it drops below horizon
                dot = Mathf.Clamp(Vector3.Dot(MoonDatas[i].UnitVectorDown, Vector3.down) + 0.2f, 0.0f, 1.0f);
                dot = Mathf.Pow(dot, 0.25f);
                yPower = Mathf.Clamp((MoonDatas[i].UnitVectorUp.y + 0.1f) * 3.0f, 0.0f, 1.0f);
                moon.Light.intensity = moon.LightBaseIntensity * yPower * (1.0f - Mathf.Min(1.0f, sun.Light.intensity)) * (float)MoonDatas[i].PercentFull * (float)MoonDatas[i].PercentFull * dot * moon.LightMultiplier;
            }
        }

        private void ProcessCelestialObject(WeatherMakerCelestialObject obj,
            Dictionary<string, float> directionalLightIntensityModifiers,
            Dictionary<string, float> directionalLightShadowModifier)
        {
            if (obj.Light != null)
            {
                foreach (KeyValuePair<string, float> multiplier in directionalLightIntensityModifiers)
                {
                    obj.Light.intensity *= multiplier.Value;
                }
                obj.Light.color = obj.LightColor;
                if (obj.Light.shadows != LightShadows.None)
                {
                    foreach (KeyValuePair<string, float> multiplier in directionalLightShadowModifier)
                    {
                        obj.Light.shadowStrength *= multiplier.Value;
                    }
                }
            }
        }

        private void UpdateLightIntensitiesAndShadows(WeatherMakerCelestialObject sun, ICollection<WeatherMakerCelestialObject> moons, 
            Dictionary<string, float> directionalLightIntensityModifiers,
            Dictionary<string, float> directionalLightShadowModifier)
        {
            ProcessCelestialObject(sun, directionalLightIntensityModifiers, directionalLightShadowModifier);
            foreach (WeatherMakerCelestialObject obj in moons)
            {
                ProcessCelestialObject(obj, directionalLightIntensityModifiers, directionalLightShadowModifier);
            }
        }

        private void UpdateDayMultipliers(Camera camera, WeatherMakerCelestialObject sun, float l)
        {
            Color lookup = DayDawnDuskNightGradient.Evaluate(l);
            DayMultiplier = lookup.g;
            DawnDuskMultiplier = lookup.r;
            NightMultiplier = lookup.b;
            TimeOfDayCategory = (WeatherMakerTimeOfDayCategory)0;
            if (DayMultiplier > 0.0f)
            {
                TimeOfDayCategory |= WeatherMakerTimeOfDayCategory.Day;
            }
            if (DawnDuskMultiplier > 0.0f)
            {
                if (TimeOfDay > (SecondsPerDay * 0.5))
                {
                    TimeOfDayCategory |= WeatherMakerTimeOfDayCategory.Dusk;
                }
                else
                {
                    TimeOfDayCategory |= WeatherMakerTimeOfDayCategory.Dawn;
                }
            }
            if (NightMultiplier > 0.0f)
            {
                TimeOfDayCategory |= WeatherMakerTimeOfDayCategory.Night;
            }

            if (camera != null && camera.orthographic)
            {
                float z = sun.Transform.forward.z;
                if (z >= -0.1f && z <= 0.1f)
                {
                    if (TimeOfDay > (SecondsPerDay * 0.5f))
                    {
                        TimeOfDayCategory |= WeatherMakerTimeOfDayCategory.Sunset;
                    }
                    else
                    {
                        TimeOfDayCategory |= WeatherMakerTimeOfDayCategory.Sunrise;
                    }
                }
            }
            else
            {
                float y = sun.Transform.forward.y;
                if (y >= -0.05f && y <= 0.2f)
                {
                    if (TimeOfDay > (SecondsPerDay * 0.5f))
                    {
                        TimeOfDayCategory |= WeatherMakerTimeOfDayCategory.Sunset;
                    }
                    else
                    {
                        TimeOfDayCategory |= WeatherMakerTimeOfDayCategory.Sunrise;
                    }
                }
            }
        }

        private void UpdateTimeZone()
        {
            if (TimeZoneOffsetSeconds == -1111)
            {
                TimeZoneOffsetSeconds = (int)(Longitude * 24 / 360) * 3600;

#if UNITY_EDITOR

                if ((DateTime.UtcNow - lastTimeZoneCheck).TotalSeconds > 10.0)
                {
                    lastTimeZoneCheck = DateTime.UtcNow;
                    WebClientWithTimeout c = new WebClientWithTimeout();
                    c.Timeout = 3000;
                    TimeSpan unixTimeSpan = new DateTime(Year, Month, Day, 1, 1, 1, DateTimeKind.Utc) - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                    string url = "http://api.timezonedb.com/v2/get-time-zone?by=position&lat=" + Latitude + "&lng=" + Longitude + "&time=" + (long)unixTimeSpan.TotalSeconds + "&key=1H9B390ZKKPX";
                    try
                    {
                        c.DownloadStringCompleted += (o, e) =>
                        {
                            string xml = e.Result;
                            System.Text.RegularExpressions.Match m = System.Text.RegularExpressions.Regex.Match(xml, @"\<gmtOffset\>(?<gmtOffset>.*?)\</gmtOffset\>");
                            if (m.Success)
                            {
                                TimeZoneOffsetSeconds = int.Parse(m.Groups["gmtOffset"].Value);
                                WeatherMakerScript.Instance.QueueOnMainThread(() =>
                                {
                                    SerializationHelper.SetDirty(this);
                                });
                            }
                        };
                        c.DownloadStringAsync(new System.Uri(url));
                    }
                    catch
                    {
                        // eat exceptions
                    }
                }

#endif

            }
        }

        private void UpdateTimeOfDay(bool updateTimeOfDay)
        {
            UpdateTimeZone();

            if (!updateTimeOfDay)
            {
                return;
            }

#if UNITY_EDITOR

            TimeOfDayLabel = DateTime.Today.Add(TimeSpan.FromSeconds(TimeOfDay)).ToString("hh:mm tt");

            if (!Application.isPlaying)
            {
                return;
            }
            else

#endif

            if (NightMultiplier != 1.0f && Speed != 0.0f)
            {
                TimeOfDay += (Speed * accumulatedTime);
            }
            else if (NightMultiplier == 1.0f && NightSpeed != 0.0f)
            {
                TimeOfDay += (NightSpeed * accumulatedTime);
            }
            if (AdjustDateWhenDayEnds)
            {
                // handle wrapping of time of day
                if (TimeOfDay < 0.0f)
                {
                    TimeOfDay += SecondsPerDay;
                    DateTime dt = new DateTime(Year, Month, Day) - TimeSpan.FromDays(1.0) + TimeSpan.FromSeconds(TimeOfDay);
                    Year = dt.Year;
                    Month = dt.Month;
                    Day = dt.Day;
                }
                else if (TimeOfDay >= SecondsPerDay)
                {
                    TimeOfDay -= SecondsPerDay;
                    DateTime dt = new DateTime(Year, Month, Day) + TimeSpan.FromDays(1.0) + TimeSpan.FromSeconds(TimeOfDay);
                    Year = dt.Year;
                    Month = dt.Month;
                    Day = dt.Day;
                }
            }
            else if (TimeOfDay < 0.0f)
            {
                TimeOfDay += SecondsPerDay;
            }
            else if (TimeOfDay >= SecondsPerDay)
            {
                TimeOfDay -= SecondsPerDay;
            }
            TimeOfDayTimespan = TimeSpan.FromSeconds(TimeOfDay);
        }

        private void UpdateAmbientColors(Camera camera, Transform sun, float l)
        {

#if UNITY_EDITOR

            if (DayAmbientColor == null)
            {
                return;
            }

#endif

            Color dayAmbient = DayAmbientColor.Evaluate(l);
            Color dayAmbientSky = DayAmbientColorSky.Evaluate(l);
            Color dayAmbientGround = DayAmbientColorGround.Evaluate(l);
            Color dayAmbientEquator = DayAmbientColorEquator.Evaluate(l);
            Color dawnDuskAmbient = DawnDuskAmbientColor.Evaluate(l);
            Color dawnDuskAmbientSky = DawnDuskAmbientColorSky.Evaluate(l);
            Color dawnDuskAmbientGround = DawnDuskAmbientColorGround.Evaluate(l);
            Color dawnDuskAmbientEquator = DawnDuskAmbientColorEquator.Evaluate(l);
            Color nightAmbient = NightAmbientColor.Evaluate(l);
            Color nightAmbientSky = NightAmbientColorSky.Evaluate(l);
            Color nightAmbientGround = NightAmbientColorGround.Evaluate(l);
            Color nightAmbientEquator = NightAmbientColorEquator.Evaluate(l);
            Color ambientLight = (dayAmbient * DayMultiplier * dayAmbient.a * DayAmbientColorIntensity) +
                (dawnDuskAmbient * DawnDuskMultiplier * dawnDuskAmbient.a * DawnDuskAmbientColorIntensity) +
                (nightAmbient * NightMultiplier * nightAmbient.a * NightAmbientColorIntensity);
            Color ambientLightSky = (dayAmbientSky * DayMultiplier * dayAmbientSky.a * DayAmbientColorSkyIntensity) +
                (dawnDuskAmbientSky * DawnDuskMultiplier * dawnDuskAmbientSky.a * DawnDuskAmbientColorSkyIntensity) +
                (nightAmbientSky * NightMultiplier * nightAmbientSky.a * NightAmbientColorSkyIntensity);
            Color ambientLightGround = (dayAmbientGround * DayMultiplier * dayAmbientGround.a * DayAmbientColorGroundIntensity) +
                (dawnDuskAmbientGround * DawnDuskMultiplier * dawnDuskAmbientGround.a * DawnDuskAmbientColorGroundIntensity) +
                (nightAmbientGround * NightMultiplier * nightAmbientGround.a * NightAmbientColorGroundIntensity);
            Color ambientLightEquator = (dayAmbientEquator * DayMultiplier * dayAmbientEquator.a * DayAmbientColorEquatorIntensity) +
                (dawnDuskAmbientEquator * DawnDuskMultiplier * dawnDuskAmbientEquator.a * DawnDuskAmbientColorEquatorIntensity) +
                (nightAmbientEquator * NightMultiplier * nightAmbientEquator.a * NightAmbientColorEquatorIntensity);

            float modifier = (AmbientColorMode == WeatherMakerDayNightAmbientColorMode.None ? 0.0f : 1.0f);

            Shader.SetGlobalColor("_WeatherMakerAmbientLightColor", ambientLight * modifier);
            Shader.SetGlobalColor("_WeatherMakerAmbientLightColorSky", ambientLightSky * modifier);
            Shader.SetGlobalColor("_WeatherMakerAmbientLightColorGround", ambientLightGround * modifier);
            Shader.SetGlobalColor("_WeatherMakerAmbientLightColorEquator", ambientLightEquator * modifier);
            Shader.SetGlobalFloat("_WeatherMakerDayMultiplier", DayMultiplier);
            Shader.SetGlobalFloat("_WeatherMakerDawnDuskMultiplier", DawnDuskMultiplier);
            Shader.SetGlobalFloat("_WeatherMakerNightMultiplier", NightMultiplier);

            switch (AmbientColorMode)
            {
                case WeatherMakerDayNightAmbientColorMode.AmbientColor:
                    RenderSettings.ambientLight = ambientLight;
                    break;

                case WeatherMakerDayNightAmbientColorMode.SkyOnly:
                    RenderSettings.ambientSkyColor = ambientLightSky;
                    break;

                case WeatherMakerDayNightAmbientColorMode.SkyEquatorGround:
                    RenderSettings.ambientSkyColor = ambientLightSky;
                    RenderSettings.ambientEquatorColor = ambientLightEquator;
                    RenderSettings.ambientGroundColor = ambientLightGround;
                    break;

                case WeatherMakerDayNightAmbientColorMode.All:
                    RenderSettings.ambientLight = ambientLight;
                    goto case WeatherMakerDayNightAmbientColorMode.SkyEquatorGround;

                case WeatherMakerDayNightAmbientColorMode.DynamicGIUpdateOnly:
                    if (Mathf.Abs(TimeOfDay - lastTimeOfDayForDynamicGIUpdate) > DynamicGIUpdateThresholdSeconds)
                    {
                        // update dynamic GI every 5 minutes
                        lastTimeOfDayForDynamicGIUpdate = TimeOfDay;
                        DynamicGI.UpdateEnvironment();
                    }
                    break;
            }
        }

        /// <summary>
        /// Update
        /// </summary>
        /// <param name="camera">Camera</param>
        /// <param name="sun">Sun</param>
        /// <param name="moons">Moons</param>
        /// <param name="directionalLightIntensityModifiers">Directional light intensity modifiers</param>
        /// <param name="directionalLightShadowModifier">Directional light shadow modifiers</param>
        /// <param name="updateTimeOfDay">Whether to update the time of day</param>
        public void UpdateFromProfile
        (
            Camera camera,
            WeatherMakerCelestialObject sun,
            IList<WeatherMakerCelestialObject> moons,
            Dictionary<string, float> directionalLightIntensityModifiers,
            Dictionary<string, float> directionalLightShadowModifier,
            bool updateTimeOfDay)
        {
            float l = WeatherMakerScript.GetSunGradientLookup(camera, sun.Transform);
            UpdateAmbientColors(camera, sun.Transform, l);
            accumulatedTime += Time.deltaTime;
            if (accumulatedTime > UpdateInterval)
            {
                UpdateTimeOfDay(updateTimeOfDay);
                UpdateSun(camera, sun, l);
                UpdateDayMultipliers(camera, sun, l);
                UpdateMoons(camera, sun, moons);
                accumulatedTime = 0.0f;
            }
            UpdateLightIntensitiesAndShadows(sun, moons, directionalLightIntensityModifiers, directionalLightShadowModifier);
        }
    }

    /// <summary>
    /// Day / night cycle ambient color mode
    /// </summary>
    public enum WeatherMakerDayNightAmbientColorMode
    {
        /// <summary>
        /// Do not modify ambient colors
        /// </summary>
        None = 0,

        /// <summary>
        /// Modify the ambient color only
        /// </summary>
        AmbientColor = 1,

        /// <summary>
        /// Modify the ambient sky color only
        /// </summary>
        SkyOnly = 2,

        /// <summary>
        /// Modify ambient sky, equator and ground
        /// </summary>
        SkyEquatorGround = 4,

        /// <summary>
        /// Modify ambient color plus ambient sky, equator and ground
        /// </summary>
        All = 8,

        /// <summary>
        /// Only call DynamicGI.UpdateEnvironment periodically, using DynamicGIUpdateThresholdSeconds constant
        /// </summary>
        DynamicGIUpdateOnly = 16
    }

    /// <summary>
    /// Time of day category
    /// </summary>
    [Flags]
    public enum WeatherMakerTimeOfDayCategory
    {
        /// <summary>
        /// Dawn
        /// </summary>
        Dawn = 1,

        /// <summary>
        /// Day
        /// </summary>
        Day = 2,

        /// <summary>
        /// Dusk
        /// </summary>
        Dusk = 4,

        /// <summary>
        /// Night
        /// </summary>
        Night = 8,

        /// <summary>
        /// Sunrise
        /// </summary>
        Sunrise = 16,

        /// <summary>
        /// Sunset
        /// </summary>
        Sunset = 32
    }
}
