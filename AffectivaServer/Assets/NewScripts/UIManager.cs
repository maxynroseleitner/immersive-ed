using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using System.IO;
using Affdex;

public class UIManager : MonoBehaviour {

	public bool NetworkingOn;
	[Space(10)]

	// Import Game Manager object
	public GameObject gameManagerObject;
	private GameManager gameManagerScript;
	[Space(10)]

	// Import Webcam input object
	public Camera mainCamera;
	public GameObject inputDeviceCamera;
	//public GameObject webcamRenderPlane;
	public GameObject webcamRenderRawImage;
	private CameraInput camInputScript;
	private Receiver2 receiverScript;

	RawImage tempRawImage;

	// Use this for initialization
	void Start () {
		gameManagerScript = gameManagerObject.GetComponent<GameManager>();
		camInputScript = inputDeviceCamera.GetComponent<CameraInput>();
		//planeRenderer = webcamRenderPlane.GetComponent<Renderer> ();
		tempRawImage = webcamRenderRawImage.GetComponent<RawImage>();
		receiverScript = inputDeviceCamera.GetComponent<Receiver2>();

		// Camera feed parameters
		if (camInputScript.Texture == null) {
			Debug.Log ("Camera not started");
			feedWidth = camInputScript.targetWidth;
			feedHeight = camInputScript.targetHeight;
			camReady = false;
		}
			
		SetFeed();

		// Initalize the colors
		previousEmotionColor = new Color();
		currentEmotionColor = new Color();
		indexOfEmotion = 0;

		// Start the background emotion updater
		StartCoroutine(RequestEmotionUpdate());

		if (NetworkingOn) {
			InitializeDataSend();
		}
	}
	
	// Update is called once per frame
	void Update ()
	{

		if (!camReady) {
			if (camInputScript.Texture == null) {
				Debug.Log ("Camera not started");
				feedWidth = camInputScript.targetWidth;
				feedHeight = camInputScript.targetHeight;
				camReady = false;
			} else {
				Debug.Log ("Camera is Working");
				feedWidth = camInputScript.Texture.width;
				feedHeight = camInputScript.Texture.height;

				Debug.Log("Cam ready:\nWidth = " + feedWidth + " Height = " + feedHeight);
				if (feedWidth > 100 && feedHeight > 100) {
					camReady = true;
					SetFeed();
				}
			}
		}
			
	}
		
////////////////////////////////////////////////// EMOTION UPDATE START ///////////////////////////////////////////////////////////////
	private IEnumerator RequestEmotionUpdate() {

		// Debug.Log("Entered REQUEST EMOTION UPDATE COROUTINE.");
		while (true) {

			yield return new WaitForSeconds(colorUpdateTime);

			previousEmotionColor = currentEmotionColor;
			currentEmotionColor = gameManagerScript.getCurrentCumulativeEmotionColor();
			indexOfEmotion = gameManagerScript.indexOfEmotion;
		}
	}
////////////////////////////////////////////////// EMOTION UPDATE END ///////////////////////////////////////////////////////////////

/////////////////////////////////////////// SET MOOD TRACKER ATTRIBUTES  START //////////////////////////////////////////////////////
	[HideInInspector] public Vector3 normalizedMoodTrackerCoordinates;
	[HideInInspector] public Vector3 moodTrackerSize;
	[HideInInspector] public Color moodTrackerColor;

	// Define moodtracker scaling and offset by hit-and-trial
	[Space(10)]
	public float offsetX = 0f;
	public float offsetY = 8f;
	public float scaleXpercent  = 0.1f;
	public float scaleYpercent  = 0.1f;

	// Update the colors on-screen every X seconds
	public float colorUpdateTime = 0.5f;
	public float lerpTime = 0.25f;
	[Space(10)]

	private Color currentEmotionColor;
	private Color previousEmotionColor;
	private int indexOfEmotion;

	public void SetMoodTrackerGeometry (Vector3 moodTrackerCoordinates){

		float flipTrackerX = flipHorizontal ? -1f : 1f;
		float flipTrackerY = flipVertical ? 1f : -1f;

		float xValue = moodTrackerCoordinates.x;
		float yValue = moodTrackerCoordinates.y;
		float interOcularDistance = moodTrackerCoordinates.z;

		// Debug.Log ("xValue: " + xValue + " yValue: " + yValue + " IOD: " + interOcularDistance);
		// Mapping - Camera feed to Mixed Reality Worldspace
		// Offset X/Y to make cube appear above face and 
		// incline towards a side(20% or 40% screen width or height)
		// Account for Horizontal flip and Vertical flip
		// Works good for width = 1280 and height = 720

		// Mapping detected facial coordinates to Worldspace
		float originX = feedWidth / 2f;
		float originY = feedHeight / 2f;

		float recenterX = flipTrackerX * (xValue - originX);
		float recenterY = flipTrackerY * (yValue - originY);

		// Normalizing final Coordinates
		float scaleX = scaleXpercent * feedWidth;
		float scaleY = scaleYpercent * feedHeight;

		float offsetXpercent = offsetX * interOcularDistance / originX;
		float offsetYpercent = offsetY * interOcularDistance / originY;

		float normalizeX = (recenterX / scaleX) + offsetXpercent;
		float normalizeY = (recenterY / scaleY) + offsetYpercent;

		// Assigning values
		normalizedMoodTrackerCoordinates.x = normalizeX;
		normalizedMoodTrackerCoordinates.y = normalizeY;
		normalizedMoodTrackerCoordinates.z = 10f;

		////////////////////////////////////////////////////////////
//		if (NetworkingOn) {
//			PrepareDataToSend();
//		}
	}

	public Color GetMoodTrackerColor(){
		return currentEmotionColor;
	}

	byte[] xMoodTrackerByte;
	byte[] yMoodTrackerByte;
	byte[] zMoodTrackerByte;
	byte[] indexOfMoodByte;

	byte[] joyByte;
	byte[] fearByte;
	byte[] disgustByte;
	byte[] sadnessByte;
	byte[] angerByte;
	byte[] surpriseByte;

	const int PacketDataSize = 4;
	byte[] emotionUpdateData;

	void InitializeDataSend(){
		xMoodTrackerByte = new byte[PacketDataSize];
		yMoodTrackerByte = new byte[PacketDataSize];
		zMoodTrackerByte = new byte[PacketDataSize];
		indexOfMoodByte  = new byte[PacketDataSize];

		joyByte = new byte[PacketDataSize];
		fearByte = new byte[PacketDataSize];
		disgustByte = new byte[PacketDataSize];
		sadnessByte = new byte[PacketDataSize];
		angerByte = new byte[PacketDataSize];
		surpriseByte = new byte[PacketDataSize];
	}

	int serial = 0;
	float[] emotionArray;
	public void PrepareDataToSend() {

		if (!NetworkingOn) {
			return;
		}

		int xPosition = 0;
		int yPosition = 0;
		int zPosition = 0;

		if (!(float.IsNaN (normalizedMoodTrackerCoordinates.x) || float.IsNaN (normalizedMoodTrackerCoordinates.y) || float.IsNaN (normalizedMoodTrackerCoordinates.z))) {
			xPosition = (int)Mathf.Ceil(100 * normalizedMoodTrackerCoordinates.x);
			yPosition = (int)Mathf.Ceil(100 * normalizedMoodTrackerCoordinates.y);
			zPosition = (int)Mathf.Ceil(100 * normalizedMoodTrackerCoordinates.z);	
		}

		serial++;
		ConvertIntegerToByteArray(xPosition, xMoodTrackerByte);
		ConvertIntegerToByteArray(yPosition, yMoodTrackerByte);
		ConvertIntegerToByteArray(zPosition, zMoodTrackerByte);
		ConvertIntegerToByteArray(indexOfEmotion, indexOfMoodByte);
		//ConvertIntegerToByteArray(serial, indexOfMoodByte);

		emotionArray = gameManagerScript.emotionArray;

		ConvertFloatToByteArray(emotionArray[0], joyByte);
		ConvertFloatToByteArray(emotionArray[1], fearByte);
		ConvertFloatToByteArray(emotionArray[2], disgustByte);
		ConvertFloatToByteArray(emotionArray[3], sadnessByte);
		ConvertFloatToByteArray(emotionArray[4], angerByte);
		ConvertFloatToByteArray(emotionArray[5], surpriseByte);

		emotionUpdateData = CombineBytes(xMoodTrackerByte, yMoodTrackerByte, zMoodTrackerByte, indexOfMoodByte, joyByte, fearByte, disgustByte, sadnessByte, angerByte, surpriseByte);
		receiverScript.UpdateEmotionData(emotionUpdateData);
	}

	//Converts integer to byte array
	void ConvertIntegerToByteArray(int valueToBeConvertedToByte, byte[] resultByteArray)
	{
		//Clear old data
		Array.Clear(resultByteArray, 0, PacketDataSize);
		//Convert int to bytes
		byte[] byteArrayToSend = BitConverter.GetBytes(valueToBeConvertedToByte);
		//Copy result to fullBytes
		byteArrayToSend.CopyTo(resultByteArray, 0);
	}

	//Converts float to byte array
	void ConvertFloatToByteArray(float valueToBeConvertedToByte, byte[] resultByteArray)
	{
		//Clear old data
		Array.Clear(resultByteArray, 0, PacketDataSize);
		//Convert int to bytes
		byte[] byteArrayToSend = BitConverter.GetBytes(valueToBeConvertedToByte);
		//Copy result to fullBytes
		byteArrayToSend.CopyTo(resultByteArray, 0);
	}

	byte[] CombineBytes(byte[] first, byte[] second, byte[] third, byte[] fourth, byte[] fifth, byte[] sixth, byte[] seventh, byte[] eighth, byte[] ninth, byte[] tenth)
	{
		//Just to see if the conversion is proper
		int x = BitConverter.ToInt32(first, 0);
		int y = BitConverter.ToInt32(second, 0);
		int z = BitConverter.ToInt32(third, 0);
		int e = BitConverter.ToInt32(fourth, 0);

		float joy = BitConverter.ToSingle(fifth, 0);
		float fear = BitConverter.ToSingle(sixth, 0);
		float disgust = BitConverter.ToSingle(seventh, 0);
		float sadness = BitConverter.ToSingle(eighth, 0);
		float anger = BitConverter.ToSingle(ninth, 0);
		float surprise = BitConverter.ToSingle(tenth, 0);

		string message = "x = " + x + " y = " + y + " z = " + z + " e = " + e;
		Debug.Log(message);

		message = "joy = " + joy + " fear = " + fear + " disgust = " + disgust;
		Debug.Log(message);

		message = "sadness = " + sadness + " anger = " + anger + " surprise = " + surprise;
		Debug.Log(message);

		byte[] combinedByte = new byte[10*PacketDataSize];

		//Copy geometric data first
		Buffer.BlockCopy(first, 0, combinedByte, 0, PacketDataSize);
		Buffer.BlockCopy(second, 0, combinedByte, PacketDataSize, PacketDataSize);
		Buffer.BlockCopy(third, 0, combinedByte, 2*PacketDataSize, PacketDataSize);

		//Copy emotion-analysis data
		Buffer.BlockCopy(fourth, 0, combinedByte, 3*PacketDataSize, PacketDataSize);
		Buffer.BlockCopy(fifth, 0, combinedByte, 4*PacketDataSize, PacketDataSize);
		Buffer.BlockCopy(sixth, 0, combinedByte, 5*PacketDataSize, PacketDataSize);
		Buffer.BlockCopy(seventh, 0, combinedByte, 6*PacketDataSize, PacketDataSize);
		Buffer.BlockCopy(eighth, 0, combinedByte, 7*PacketDataSize, PacketDataSize);
		Buffer.BlockCopy(ninth, 0, combinedByte, 8*PacketDataSize, PacketDataSize);
		Buffer.BlockCopy(tenth, 0, combinedByte, 9*PacketDataSize, PacketDataSize);
		return combinedByte;
	}

//	byte[] CombineBytes(byte[] first, byte[] second, byte[] third, byte[] fourth)
//	{
//
//		int x = BitConverter.ToInt32(first, 0);
//		int y = BitConverter.ToInt32(second, 0);
//		int z = BitConverter.ToInt32(third, 0);
//		int e = BitConverter.ToInt32(fourth, 0);
//
//		string message = "x = " + x + " y = " + y + " z = " + z + " e = " + e;
//		Debug.Log(message);
//
//		byte[] combinedByte = new byte[4*PacketDataSize];
//		Buffer.BlockCopy(first, 0, combinedByte, 0, PacketDataSize);
//		Buffer.BlockCopy(second, 0, combinedByte, PacketDataSize, PacketDataSize);
//		Buffer.BlockCopy(third, 0, combinedByte, 2*PacketDataSize, PacketDataSize);
//		Buffer.BlockCopy(fourth, 0, combinedByte, 3*PacketDataSize, PacketDataSize);
//		return combinedByte;
//	}

/////////////////////////////////////////// SET MOOD TRACKER ATTRIBUTES  END ////////////////////////////////////////////////////////

//////////////////////////////////////////////// SET CAMERA FEED  START /////////////////////////////////////////////////////////////
	// Configure Webcam output object
	[Space(10)]
	public float displayHeight = 0.54f;
	public bool flipHorizontal = false;
	public bool flipVertical = true;

	//private Renderer planeRenderer;
	private Texture rawImageTexture;
	private float feedWidth;
	private float feedHeight;
	private bool camReady;

	public void SetFeed()
	{

		float flipDisplayX = flipHorizontal ? 1f : -1f;
		float flipDisplayY = flipVertical ? 1f : -1f;

		// Set the webcam-Render-Plane to have the same aspect ratio as the video feed
		float aspectRatio = 1;
		if (feedHeight != 0 && feedWidth != 0) {
			aspectRatio = feedWidth / feedHeight;
		}

		//webcamRenderPlane.transform.localScale = new Vector3 (flipDisplayX*aspectRatio*displayHeight, 1.0f, flipDisplayY*displayHeight);
		tempRawImage.transform.localScale = new Vector3 (-flipDisplayX*aspectRatio*displayHeight, 1.0f, flipDisplayY*displayHeight);
		Debug.Log (" Feed Width: " + feedWidth + " Feed Height: " + feedHeight + " Aspect Ratio: " + aspectRatio);

		// Display the webcam input
		//planeRenderer.material.mainTexture = camInputScript.Texture;
		//rawImageTexture = camInputScript.Texture;
		tempRawImage.texture = camInputScript.Texture;
	}

///////////////////////////////////////////////// SET CAMERA FEED  END //////////////////////////////////////////////////////////////
}
