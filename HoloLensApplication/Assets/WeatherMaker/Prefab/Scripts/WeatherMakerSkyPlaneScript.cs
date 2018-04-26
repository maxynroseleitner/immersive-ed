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
using UnityEngine.Rendering;

namespace DigitalRuby.WeatherMaker
{
    [ExecuteInEditMode]
    public class WeatherMakerSkyPlaneScript : WeatherMakerPlaneCreatorScript
    {

#if UNITY_EDITOR

        private void OnWillRenderObject()
        {
            if (!Application.isPlaying && Camera.current != null)
            {
                SkyPlaneProfile.UpdateSkyPlane(Camera.current, MeshRenderer.sharedMaterial, gameObject);
            }
        }

        protected override void Update()
        {
            base.Update();
            if (SkyPlaneProfile == null)
            {
                SkyPlaneProfile = Resources.Load<WeatherMakerSkyProfileScript>("WeatherMakerSkyProfile_Procedural");
            }
            if (!Application.isPlaying && Camera.main != null)
            {
                SkyPlaneProfile.UpdateSkyPlane(Camera.main, MeshRenderer.sharedMaterial, gameObject);
            }
        }

#endif

        [Header("Sky plane profile")]
        public WeatherMakerSkyProfileScript SkyPlaneProfile;

        public void PreCullCamera(Camera camera)
        {
            if (camera != null)
            {
                SkyPlaneProfile.UpdateSkyPlane(camera, MeshRenderer.sharedMaterial, gameObject);
            }
        }
    }
}
