// Copyright (c) 2017 Vulcan, Inc. All rights reserved.  
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

using VacuumShaders.TextureExtensions;

#if !UNITY_EDITOR && (UNITY_WSA || NETFX_CORE)
using Windows.Foundation;
using Windows.System.Threading;
using System.Threading.Tasks;
#else
using System.Threading;
using System.Threading.Tasks;
#endif

public class VideoPanel : MonoBehaviour
{
	public GameManager gameManagerScript;
	[Space(10)]
	public GameObject camDisplayQuad;
	public DisplayInfo displayInfo;
	public DisplayFPS displayFPS;
	public float displayHeightMultiplier;
	public bool displayLogData = false;
	public bool viewFeedOnDevice;
	public bool flipHorizontal = false;
	public bool flipVertical = true;
	[Space (10)]

	public bool startSending = false;
	public bool useQuality;
	public int quality;
	public int bufferSize;
	[Space (10)]

	public int resizeToWidth;
	public int resizeToHeight;
	public int requestedFrameRate;
	public int reduceFrameRateTo;

	int finalWidth;
	int finalHeight;
	int finalFrameRate;

	string status;
	int countOfFramesApp = 0;
	int countOfFramesActual = 0;
	int countOfFramesProcessed = 0;
	byte[] stackedImage;

	[HideInInspector]
	public byte[] compressedImage;
	[HideInInspector]
	public int compressedImageSize = 0;
	[HideInInspector]
	public Texture2D resizedFrameTexture;
	Texture2D stackedFrameTexture;

	Queue<byte[]> queueOfFrames = new Queue<byte[]>();

	//////////////////////////////////////////////// SET CAMERA FEED  START /////////////////////////////////////////////////////////////
	// Configure Webcam output object
	private Renderer camDisplayQuadRenderer;
	private bool camReady;

	public void SetResolution(int width, int height, int framerate)
    {
		finalWidth = width;
		finalHeight = height;
		finalFrameRate = framerate;

		//Adjust resized Height as per aspect ratio of original feed
		float aspectRatio = (float)finalWidth / (float)finalHeight;
		resizeToHeight = Mathf.CeilToInt((float)resizeToWidth/aspectRatio);

		if (viewFeedOnDevice) {
			// Maintain orientation of display
			float flipDisplayX = flipHorizontal ? 1f : -1f;
			float flipDisplayY = flipVertical ? 1f : -1f;

			// Set the webcam-Render-Quad to have the same aspect ratio as the video feed
			camDisplayQuad.transform.localScale = new Vector3 (-10 * flipDisplayX * aspectRatio * displayHeightMultiplier, -10 * flipDisplayY * displayHeightMultiplier, 1.0f);
			Debug.Log ("Feed Width: " + finalWidth + " Feed Height: " + finalHeight + " Aspect Ratio: " + aspectRatio);

			//For setting up Cam Quad Display resolution
			Texture2D targetTexture = new Texture2D(finalWidth, finalHeight, TextureFormat.RGB24, false);
			camDisplayQuadRenderer.material.mainTexture = targetTexture;
			Mesh targetMesh = camDisplayQuadRenderer.GetComponentInParent<MeshFilter> ().mesh;
			var newUV = targetMesh.uv;

			for (int i=0; i<newUV.Length; i++) {
				if (Mathf.Approximately (newUV [i].x, 1))
					newUV[i].x = 0;
				else
					newUV[i].x = 1;
			}

			targetMesh.uv = newUV;	
		}
    }
		
	///////////////////////////////////////////////// SET CAMERA FEED  END //////////////////////////////////////////////////////////////
	int preProcessCounter = 0;
	public void SetBytes(byte[] image)
    {			
		++countOfFramesActual;
		++preProcessCounter;
		if (preProcessCounter > finalFrameRate) {
			preProcessCounter = 0;
		}

		if (queueOfFrames.Count < bufferSize) {
//			if (preProcessCounter < reduceFrameRateTo) {
//				queueOfFrames.Enqueue (image);
//			}

			if (preProcessCounter%frameReductionFactor  == 0) {
				queueOfFrames.Enqueue (image);
			}

		}
    }

	IEnumerator WorkOnTexture(){
		countOfFramesProcessed++;
		Debug.LogWarning("Number of frames process" + countOfFramesProcessed);
		byte[] stackedImage = queueOfFrames.Dequeue();
		stackedFrameTexture.LoadRawTextureData(stackedImage);
		stackedFrameTexture.Apply();
		yield return null;
	}

	private bool LoadTexture(){
		//Debug.LogWarning("Number of frames process" + countOfFramesProcessed);
		byte[] stackedImage = queueOfFrames.Dequeue();
		stackedFrameTexture.LoadRawTextureData(stackedImage);
		stackedFrameTexture.Apply();
		return true;
	}

	public IEnumerator PreProcessFrameRoutine(){
		countOfFramesProcessed++;
		status = "Starting pre-processing";
		if (queueOfFrames.Count > 0) {
			bool textureLoaded;
			yield return textureLoaded = LoadTexture();
			status = "Texture Applied";

			TextureResizePro.ResizePro(stackedFrameTexture, resizeToWidth, resizeToHeight, out resizedFrameTexture, false);
			status = "Texture Resized";

			//Encode to JPG for smallest size, Encode to PNG for better quality
			if (useQuality) {
				yield return compressedImage = resizedFrameTexture.EncodeToJPG (quality);	
			} else {
				yield return compressedImage = resizedFrameTexture.EncodeToJPG();
			}

			gameManagerScript.PrepareToSend(compressedImage);
			compressedImageSize = compressedImage.Length;
			status = "Compression done" + compressedImage.Length;

			if (!startSending)
				startSending = true;

			Resources.UnloadUnusedAssets();
			//Debug.Log(status);
		}

		yield return null;
	}

	public void PreProcessFrame(){

		status = "Starting pre-processing";
		if (queueOfFrames.Count < 1) {
			return;
		}

		countOfFramesProcessed++;
		byte[] stackedImage = queueOfFrames.Dequeue();

		stackedFrameTexture.LoadRawTextureData(stackedImage);
		stackedFrameTexture.Apply();
		status = "Texture Applied";

		TextureResizePro.ResizePro(stackedFrameTexture, resizeToWidth, resizeToHeight, out resizedFrameTexture, false);
		status = "Texture Resized";

		//Encode to JPG for smallest size, Encode to PNG for better quality
		if (useQuality) {
			compressedImage = resizedFrameTexture.EncodeToJPG (quality);	
		} else {
			compressedImage = resizedFrameTexture.EncodeToJPG();
		}

		gameManagerScript.PrepareToSend(compressedImage);
		compressedImageSize = compressedImage.Length;
		status = "Compression done" + compressedImage.Length;

		if (!startSending)
			startSending = true;

		Resources.UnloadUnusedAssets();
		//Debug.Log(status);
	}

	void DisplayFrame(){
		if (viewFeedOnDevice) {
			camDisplayQuadRenderer.material.mainTexture = stackedFrameTexture;
			status = "Texture Loaded";
		}
	}

	void AllocateMemoryToTextures(){
		startSending = false;
		resizedFrameTexture = new Texture2D (resizeToWidth, resizeToHeight, TextureFormat.RGB24 , false);

		#if !UNITY_EDITOR && (UNITY_WSA || NETFX_CORE)
		stackedFrameTexture = new Texture2D (896, 504, TextureFormat.BGRA32, false);
		#else
		stackedFrameTexture = new Texture2D (640, 480, TextureFormat.RGBA32, false);
		#endif
	}

	IEnumerator DisplayStatus(){
		while (true) {
			yield return new WaitForEndOfFrame();	
			countOfFramesApp++;
			string message1 = "Frames: " + countOfFramesApp + "\nWidth: " + finalWidth + "\nHeight: " + finalHeight;
			string message2 = "Status: " + status + "\nCompressed: " + compressedImageSize;
			string message3 = "Camera FPS: " + finalFrameRate + "\nCamera frames: " + countOfFramesActual + "\nFrames sent: " + countOfFramesProcessed + "\nBuffered Frames: " + queueOfFrames.Count;
			displayInfo.ClearAndSetDisplayText (message1 + "\n" + message2 + "\n" + message3);
		}
	}

	void Start()
	{
		camDisplayQuad.SetActive(viewFeedOnDevice);
		if (viewFeedOnDevice) {
			camDisplayQuadRenderer = camDisplayQuad.GetComponent<Renderer>();
		}

		AllocateMemoryToTextures();
		displayInfo.SetDisplayMode(displayLogData);
		displayFPS.SetDisplayMode(displayLogData);

		if (displayLogData) {
			StartCoroutine (DisplayStatus());
		}

	}

	int frameReductionFactor = 1;
	void Update(){

		frameReductionFactor = finalFrameRate / reduceFrameRateTo;
		if (frameReductionFactor == 0) {
			frameReductionFactor = 1;	
		}

//		if (preProcessCounter < reduceFrameRateTo) {
//			//PreProcessFrame();
//			//StartCoroutine(PreProcessFrameRoutine());
//			PreProcessFrameAsync();
//		}

		if ((preProcessCounter+1) % frameReductionFactor == 0) {
			PreProcessFrameAsync();
		}

//		if (preProcessCounter == (finalFrameRate - reduceFrameRateTo)) {
//			//PreProcessFrame();
//			StartCoroutine(PreProcessFrameRoutine());
//		}

		//PreProcessFrame();
		//StartCoroutine(PreProcessFrameRoutine());

		DisplayFrame();
	}
		

	// Get camera status
	public bool GetCameraStatus(){
		if (resizedFrameTexture == null) {
			return false;
		}
		return true;
	}

	////////////////////////////// ASYNC FUNCTIONS ///////////////////////////////////////
	public async void PreProcessFrameAsync(){
		countOfFramesProcessed++;
		status = "Starting pre-processing";
		//Debug.Log(status + " " + countOfFramesProcessed);
		if (queueOfFrames.Count > 0) {
			await LoadTextureAsync();
			status = "Texture Applied";
			LOGDATA(status + " " + countOfFramesProcessed);


			TextureResizePro.ResizePro(stackedFrameTexture, resizeToWidth, resizeToHeight, out resizedFrameTexture, false);
			status = "Texture Resized";
			Debug.Log(status + " " + countOfFramesProcessed);

			//Encode to JPG for smallest size, Encode to PNG for better quality
			await EncodeToJPEG();

			gameManagerScript.PrepareToSend(compressedImage);
			compressedImageSize = compressedImage.Length;
			status = "Compression done " + compressedImage.Length;
			Debug.Log(status + " " + countOfFramesProcessed);

			if (!startSending)
				startSending = true;

			await UnloadResourcesFromGraphicsCard();
			//Debug.Log(status);
		}
	}

	private async Task<int> LoadTextureAsync(){
		//Debug.LogWarning("Number of frames process" + countOfFramesProcessed);
		byte[] stackedImage = queueOfFrames.Dequeue();
		stackedFrameTexture.LoadRawTextureData(stackedImage);
		stackedFrameTexture.Apply ();
		return 1;
	}

	private async Task<int> EncodeToJPEG(){
		if (useQuality) {
			compressedImage = resizedFrameTexture.EncodeToJPG (quality);
		} else {
			compressedImage = resizedFrameTexture.EncodeToJPG();
		}
		return 1;
	}
	private async Task<int> UnloadResourcesFromGraphicsCard(){
		Resources.UnloadUnusedAssets();
		return 1;
	}

	public bool enableLogData;
	void LOGDATA(string messsage)
	{
		if (enableLogData)
			Debug.Log(messsage);
	}
}

