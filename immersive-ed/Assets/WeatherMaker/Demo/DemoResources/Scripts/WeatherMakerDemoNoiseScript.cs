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
    public class WeatherMakerDemoNoiseScript : MonoBehaviour
    {
        public Camera RenderCamera;
        public Transform RenderQuad;

        private RenderTexture renderTexture;

        private void Start()
        {
            renderTexture = new RenderTexture(4096, 4096, 16, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
            renderTexture.autoGenerateMips = false;
            renderTexture.name = "WeatherMakerDemoNoiseScript";
        }

        public void ExportClicked()
        {
            Texture2D t2d = new Texture2D(4096, 4096, TextureFormat.ARGB32, false, false);
            t2d.filterMode = FilterMode.Point;
            t2d.wrapMode = TextureWrapMode.Clamp;
            Rect rect = RenderCamera.rect;
            RenderCamera.rect = new Rect(0.0f, 0.0f, 1.0f, 1.0f);
            RenderCamera.targetTexture = renderTexture;
            RenderCamera.Render();
            RenderCamera.rect = rect;
            RenderTexture.active = renderTexture;
            t2d.ReadPixels(new Rect(0.0f, 0.0f, 4096.0f, 4096.0f), 0, 0);
            t2d.Apply();
            RenderTexture.active = null;
            RenderCamera.targetTexture = null;
            byte[] imageData = t2d.EncodeToPNG();
            Destroy(t2d);
            string docsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
            System.IO.File.WriteAllBytes(System.IO.Path.Combine(docsPath, "WeatherMakerNoiseTexture.png"), imageData);
        }
    }
}
