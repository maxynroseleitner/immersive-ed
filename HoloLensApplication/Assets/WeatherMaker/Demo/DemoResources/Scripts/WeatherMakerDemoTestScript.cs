using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DigitalRuby.WeatherMaker
{
    public class WeatherMakerDemoTestScript : MonoBehaviour
    {
        public WeatherMakerProfileScript Profile;

        private void Update()
        {
            if (Profile != null)
            {
                WeatherMakerScript.Instance.WeatherProfile = Profile;
                Profile = null;
            }
        }
    }
}
