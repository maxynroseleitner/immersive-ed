using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DigitalRuby.WeatherMaker
{
    public class WeatherMakerDemoScriptCloudMask : MonoBehaviour
    {
        public WeatherMakerCloudProfileScript MaskProfile;

        private void Start()
        {
            WeatherMakerScript.Instance.CloudScript.CloudProfile = MaskProfile;
        }
    }
}
