// Unity derives Camera Input Component UI from this file
using UnityEngine;
using System.Collections;
#if UNITY_XBOXONE
using Kinect;
#endif

namespace Affdex
{
    /// <summary>
    /// Provides WebCam access to the detector.  Sample rate set per second.  Use
    /// </summary>
    [RequireComponent(typeof(Detector))]
    public class CameraInput : MonoBehaviour, IDetectorInput
    {
        /// <summary>
        /// Number of frames per second to sample.  Use 0 and call ProcessFrame() manually to run manually.
        /// Enable/Disable to start/stop the sampling
        /// </summary>
        public float sampleRate = 20;

        /// <summary>
        /// Should the selected camera be front facing?
        /// </summary>
        public bool isFrontFacing = true;

        /// <summary>
        /// Desired width for capture
        /// </summary>
        public int targetWidth = 640;

        /// <summary>
        /// Desired height for capture
        /// </summary>
        public int targetHeight = 480;

        /// <summary>
        /// Import the Receiver2 script here
        /// </summary>
        Receiver2 textureReceiverScript;
        public bool NetworkingOn;
		public GameObject inputDeviceCamera;

#if UNITY_XBOXONE
        /// <summary>
        /// Kinect texture
        /// </summary>
        [HideInInspector]
        private Texture2D cameraTexture;

        /// <summary>
        /// The rotation of the camera
        /// </summary>
        public float videoRotationAngle = 0;
#elif UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX || UNITY_IOS || UNITY_ANDROID
        /// <summary>
        /// List of WebCams accessible to Unity
        /// </summary>
        [HideInInspector]
        protected WebCamDevice[] devices;

        /// <summary>
        /// WebCam chosen to gather metrics from
        /// </summary>
        [HideInInspector]
        protected WebCamDevice device;

        /// <summary>
        /// Web Cam texture
        /// </summary>
        [HideInInspector]
        private WebCamTexture cameraTexture;
		private Texture2D receivedCamTexture;

        public float videoRotationAngle
        {
            get
            {
                return cameraTexture.videoRotationAngle;
            }
        }
#endif
        /// <summary>
        /// The detector that is on this game object
        /// </summary>
        public Detector detector
        {
            get; private set;
        }

        /// <summary>
        /// The texture that is being modified for processing
        /// </summary>
        public Texture Texture
        {
            get
            {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX || UNITY_XBOXONE || UNITY_IOS || UNITY_ANDROID

				if(!NetworkingOn){
					return cameraTexture;
				} else {
					return (Texture)receivedCamTexture;
				}

				            
#else
                return new Texture();
#endif
            }
        }

        void Start ()
		{

			textureReceiverScript = FindObjectOfType<Receiver2>();
			//textureReceiverScript = inputDeviceCamera.GetComponent<Receiver2> ();

			if (textureReceiverScript != null) {
				Debug.Log ("Found receiver");
			}

            if (!AffdexUnityUtils.ValidPlatform())
                return;
            detector = GetComponent<Detector>();
#if !UNITY_XBOXONE && UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX || UNITY_IOS || UNITY_ANDROID
            devices = WebCamTexture.devices;

			if(!NetworkingOn)
			{
				if (devices.Length > 0)
	            {
	                SelectCamera(isFrontFacing);

					if (device.name != "Null")
	                {
	                    cameraTexture = new WebCamTexture(device.name, targetWidth, targetHeight, (int)sampleRate);
	                    cameraTexture.Play();
	                }
	            }
	        }

            else
            {
				receivedCamTexture = textureReceiverScript.receivedTexture;
				//cameraTexture = textureReceiverScript.receivedTexture;
			}
#endif
        }

        /// <summary>
        /// Set the target device (by name or orientation)
        /// </summary>
        /// <param name="isFrontFacing">Should the device be forward facing?</param>
        /// <param name="name">The name of the webcam to select.</param>
        public void SelectCamera(bool isFrontFacing, string name = "")
        {
#if !UNITY_XBOXONE && UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX || UNITY_IOS || UNITY_ANDROID
            foreach (WebCamDevice d in devices)
            {
				if (d.name.Length > 1 && d.name == name && !NetworkingOn)
                {
                        cameraTexture.Stop();
                        device = d;

                        cameraTexture = new WebCamTexture(device.name, targetWidth, targetHeight, (int)sampleRate);
                        cameraTexture.Play();
                }
                else if (d.isFrontFacing == isFrontFacing)
                {
                    device = d;
                }
            }
#endif
        }

        void OnEnable()
        {
            if (!AffdexUnityUtils.ValidPlatform())
                return;

            //get the selected camera!

            if (sampleRate > 0)
                StartCoroutine(SampleRoutine());
        }

        /// <summary>
        /// Coroutine to sample frames from the camera
        /// </summary>
        /// <returns></returns>
        private IEnumerator SampleRoutine()
        {
            while (enabled)
            {
                yield return new WaitForSeconds(1 / sampleRate);
                ProcessFrame();
            }
        }


        /// <summary>
        /// Sample an individual frame from the webcam and send to detector for processing.
        /// </summary>
        public void ProcessFrame ()
		{

			//Debug.Log ("I am processing frames");

			if (receivedCamTexture == null) {
				//Debug.Log ("I am not receiving");
				receivedCamTexture = textureReceiverScript.receivedTexture;
			} else {
				//Debug.Log("I am receiving");
			}

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX || UNITY_XBOXONE || UNITY_IOS || UNITY_ANDROID
            if (cameraTexture != null)
            {
                if (detector.IsRunning)
                {
					if (cameraTexture.isPlaying && !NetworkingOn)
					{
	                    Frame.Orientation orientation = Frame.Orientation.Upright;
	                    Frame frame = new Frame(cameraTexture.GetPixels32(), cameraTexture.width, cameraTexture.height, orientation, Time.realtimeSinceStartup);
	                    detector.ProcessFrame(frame);
                    }
                }
            }

			else if (receivedCamTexture != null)
            {

            	//Debug.Log("I am receiving");
				if (detector.IsRunning)
                {
					if (NetworkingOn)
					{
						if(!(receivedCamTexture.width == 0 || receivedCamTexture.height == 0))
						{
							Frame.Orientation orientation = Frame.Orientation.Upright;
							Frame frame = new Frame(receivedCamTexture.GetPixels32(), receivedCamTexture.width, receivedCamTexture.height, orientation, Time.realtimeSinceStartup);
	                        detector.ProcessFrame(frame);
						}
                    }
                }
            }

			


#endif
                }

        void OnDestroy()
        {
#if !UNITY_XBOXONE && UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX || UNITY_IOS || UNITY_ANDROID
			if (cameraTexture != null && !NetworkingOn)
            {
                cameraTexture.Stop();
            }
#endif
        }
    }
}
