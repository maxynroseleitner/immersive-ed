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
    public class WeatherMakerSoundZoneScript : MonoBehaviour
    {
        [Tooltip("Sounds to play if in the sound zone")]
        public List<WeatherMakerSoundGroupScript> Sounds;

        private void OnTriggerEnter(Collider other)
        {
            if (other.tag == null || !other.tag.Contains("Player"))
            {
                return;
            }

            // entered the trigger, can play
            foreach (WeatherMakerSoundGroupScript script in Sounds)
            {
                script.CanPlay = true;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.tag == null || !other.tag.Contains("Player"))
            {
                return;
            }

            foreach (WeatherMakerSoundGroupScript script in Sounds)
            {
                // left the trigger, can't play
                script.CanPlay = false;
            }
        }

        private void Start()
        {
            foreach (WeatherMakerSoundGroupScript script in Sounds)
            {
                script.Initialize();

                // can't play until we enter the trigger
                script.CanPlay = false;
            }
        }

        private void LateUpdate()
        {
            // update all sounds
            foreach (WeatherMakerSoundGroupScript script in Sounds)
            {
                script.Update();
            }
        }
    }
}