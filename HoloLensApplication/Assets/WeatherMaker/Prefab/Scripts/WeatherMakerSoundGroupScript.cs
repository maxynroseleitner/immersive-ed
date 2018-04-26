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
    [CreateAssetMenu(fileName = "WeatherMakerSoundGroup", menuName = "WeatherMaker/Sound Group", order = 9)]
    [System.Serializable]
    public class WeatherMakerSoundGroupScript : WeatherMakerBaseScriptableObjectScript
    {
        [Tooltip("All sounds in the group")]
        public List<WeatherMakerSoundScript> Sounds;

        public bool CanPlay
        {
            get
            {
                return Sounds == null || Sounds.Count == 0 || Sounds[0] == null ? false : Sounds[0].CanPlay;
            }
            set
            {
                if (Sounds != null)
                {
                    foreach (WeatherMakerSoundScript script in Sounds)
                    {
                        if (script != null)
                        {
                            script.CanPlay = value;
                        }
                    }
                }
            }
        }

        public override void Initialize()
        {
            base.Initialize();
            if (Sounds != null)
            {
                foreach (WeatherMakerSoundScript sound in Sounds)
                {
                    if (sound != null)
                    {
                        sound.Initialize();
                    }
                }
            }
        }

        public override void Update()
        {
            base.Update();
            if (Sounds != null)
            {
                foreach (WeatherMakerSoundScript sound in Sounds)
                {
                    if (sound != null)
                    {
                        sound.Update();
                    }
                }
            }
        }
    }
}