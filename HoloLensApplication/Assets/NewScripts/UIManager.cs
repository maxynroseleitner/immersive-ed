using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DigitalRuby.WeatherMaker;

//using Affdex;

public class UIManager : MonoBehaviour {

	public GameObject gameManagerObject;
	private GameManager gameManagerScript;

	public Camera mainCamera;
	public GameObject inputDeviceCamera;
	//public GameObject webcamRenderPlane;
	public GameObject webcamRenderQuad;

//	private CameraInput camInputScript;
	//private Renderer planeRenderer;

	// Emotion modality colors
	private Color currentFacialEmotionColor;
	private Color previousFacialEmotionColor;
	private Color currentWordSentimentEmotionColor;
	private Color previousWordSentimentEmotionColor;

	public Image facialEmotionBar;
	private float currentFacialEmotionBarWidth;
	private float previousFacialEmotionBarWidth;

	public Image wordSentimentEmotionBar;
	private float currentWordSentimentEmotionBarWidth;
	private float previousWordSentimentEmotionBarWidth;

	// Weather Objects and Variables
	public GameObject weatherManagerObject;
	private WeatherManager weatherManagerScript;

	// Emotion updating variables
	public float requestEmotionUpdateTime = 1.0f; // the number of seconds between each request for an update from the emotion modality analyzers

	// Use this for initialization
	void Start () {
		gameManagerScript = (GameManager) gameManagerObject.GetComponent(typeof(GameManager));
//		camInputScript = (CameraInput) inputDeviceCamera.GetComponent<CameraInput>();
		weatherManagerScript = (WeatherManager) weatherManagerObject.GetComponent<WeatherManager>();
		quadRenderer = webcamRenderQuad.GetComponent<Renderer> ();
		
		// Camera feed parameters
//		if (camInputScript.Texture == null) {
//			Debug.Log ("Camera not started");
//			feedWidth = camInputScript.targetWidth;
//			feedHeight = camInputScript.targetHeight;
//			camReady = false;
//		}

//		SetFeed ();

		//Apply webcam texture to quad gameobject
//		quadRenderer.material.mainTexture = camInputScript.Texture;

		// Initalize the colors
		previousFacialEmotionColor = new Color();
		currentFacialEmotionColor = new Color();

		previousWordSentimentEmotionColor = new Color();
		currentWordSentimentEmotionColor = new Color();

		// Initialize the bar widths
		currentFacialEmotionBarWidth = 0.0f;
		previousFacialEmotionBarWidth = 0.0f;

		currentWordSentimentEmotionBarWidth = 0.0f;
		previousWordSentimentEmotionBarWidth = 0.0f;

		// Start the background emotion updater
		StartCoroutine(RequestEmotionUpdate());
	}
	
	// Update is called once per frame
	void Update () {
		// Display the webcam input	
//		quadRenderer.material.mainTexture = camInputScript.Texture;
	}

	// Pulls the most recent emotion values from the GameManager
	private IEnumerator RequestEmotionUpdate()
	{
		while (true)
		{
			EmotionStruct aggregateEmotion = calculateAverageEmotionValues();
			Debug.Log("Agg Joy: " + aggregateEmotion.joy);
			Debug.Log("Agg Fear: " + aggregateEmotion.fear);
			Debug.Log("Agg Anger: " + aggregateEmotion.anger);
			Debug.Log("Agg Sadness: " + aggregateEmotion.sadness);
			weatherManagerScript.updateWeather(aggregateEmotion, requestEmotionUpdateTime);
			yield return new WaitForSeconds(requestEmotionUpdateTime);
		}

		// while (true) 
		// {
		// 	yield return new WaitForSeconds(colorUpdateTime);

		// 	if (gameManagerScript.useFacialEmotion)
		// 	{
		// 		EmotionStruct currentEmotions = gameManagerScript.getCurrentFacialEmotion();

		// 		// Update facial emotion colors
		// 		previousFacialEmotionColor = currentFacialEmotionColor;
		// 		currentFacialEmotionColor = gameManagerScript.calculateEmotionColor(gameManagerScript.getCurrentFacialEmotion());

		// 		// Update the emotion bars
		// 		previousFacialEmotionBarWidth = currentFacialEmotionBarWidth;
		// 		currentFacialEmotionBarWidth = gameManagerScript.getValueOfStrongestEmotion(gameManagerScript.getCurrentFacialEmotion()) * 2;
		// 	}

		// 	if (gameManagerScript.useWordSentimentEmotion)
		// 	{
		// 		// Update word sentiment emotion colors
		// 		previousWordSentimentEmotionColor = currentWordSentimentEmotionColor;
		// 		currentWordSentimentEmotionColor = gameManagerScript.calculateEmotionColor(gameManagerScript.getCurrentWordSentimentEmotion());

		// 		EmotionStruct currentEmotions = gameManagerScript.getCurrentWordSentimentEmotion();

		// 		previousWordSentimentEmotionBarWidth = currentWordSentimentEmotionBarWidth;
		// 		currentWordSentimentEmotionBarWidth = gameManagerScript.getValueOfStrongestEmotion(gameManagerScript.getCurrentWordSentimentEmotion()) * 2;
		// 	}

		// 	if (gameManagerScript.useVocalToneEmotion)
		// 	{
		// 		// Update vocal tone emotion
		// 	}

		// 	StartCoroutine(UpdateBackgroundColor());
		// }
	}

	private EmotionStruct calculateAverageEmotionValues()
	{
		int numModalities = 0;
		EmotionStruct aggregateEmotion = new EmotionStruct();
		// Calculate the sum of current emotions detected
		if (gameManagerScript.useFacialEmotion)
		{
			EmotionStruct face = gameManagerScript.getCurrentFacialEmotion();
			aggregateEmotion.joy += face.joy;
			aggregateEmotion.anger += face.anger;
			aggregateEmotion.sadness += face.sadness;
			aggregateEmotion.fear += face.fear;
			numModalities++;
		}
		if (gameManagerScript.useWordSentimentEmotion)
		{
			EmotionStruct word = gameManagerScript.getCurrentWordSentimentEmotion();
			aggregateEmotion.joy += word.joy;
			aggregateEmotion.anger += word.anger;
			aggregateEmotion.sadness += word.sadness;
			aggregateEmotion.fear += word.fear;
			numModalities++;
		}
		if (gameManagerScript.useWordSentimentEmotion)
		{
			EmotionStruct tone = gameManagerScript.getCurrentVocalEmotion();
			aggregateEmotion.joy += tone.joy;
			aggregateEmotion.anger += tone.anger;
			aggregateEmotion.sadness += tone.sadness;
			aggregateEmotion.fear += tone.fear;
			numModalities++;
		}

		// Calculate the average of the current emotions detected
		if (numModalities > 0)
		{
			aggregateEmotion.joy /= numModalities;
			aggregateEmotion.anger /= numModalities;
			aggregateEmotion.sadness /= numModalities;
			aggregateEmotion.fear /= numModalities;
		}
		return aggregateEmotion;
	}

	// Coroutine enumerator for updating the current emotion color using linear interpolation over a predefined amount of time
	// private IEnumerator UpdateBackgroundColor()
	// {		
	// 	// Debug.Log("Entered UPDATE BACKGROUND COLOR COROUTINE.");
	// 	float t = 0;
	// 	while (t < 1)
	// 	{
	// 		// Now the loop will execute on every end of frame until the condition is true
	// 		if (gameManagerScript.useFacialEmotion)
	// 		{
	// 			// Update the facial emotion bar
	// 			facialEmotionBar.rectTransform.sizeDelta = new Vector2(Mathf.Lerp(previousFacialEmotionBarWidth, currentFacialEmotionBarWidth, t),
	// 				facialEmotionBar.rectTransform.sizeDelta.y);
	// 			facialEmotionBar.color = Color.Lerp(previousFacialEmotionColor, currentFacialEmotionColor, t);
	// 		}

	// 		if (gameManagerScript.useWordSentimentEmotion)
	// 		{
	// 			// Update the word sentiment emotion bar
	// 			wordSentimentEmotionBar.rectTransform.sizeDelta = new Vector2(Mathf.Lerp(previousWordSentimentEmotionBarWidth, currentWordSentimentEmotionBarWidth, t),
	// 				wordSentimentEmotionBar.rectTransform.sizeDelta.y);
	// 			wordSentimentEmotionBar.color = Color.Lerp(previousWordSentimentEmotionColor, currentWordSentimentEmotionColor, t);	
	// 		}
	// 		// mainCamera.backgroundColor = Color.Lerp(previousFacialEmotionColor, currentFacialEmotionColor, t);

	// 		t += Time.deltaTime / lerpTime;

	// 		yield return new WaitForEndOfFrame();
	// 	}
	// }

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


	}

	public Color GetMoodTrackerColor(){
		return currentEmotionColor;
	}
	/////////////////////////////////////////// SET MOOD TRACKER ATTRIBUTES  END ////////////////////////////////////////////////////////

//////////////////////////////////////////////// SET CAMERA FEED  START /////////////////////////////////////////////////////////////
	// Configure Webcam output object
	[Space(10)]
	public float displayHeight = 0.54f;
	public bool flipHorizontal = false;
	public bool flipVertical = true;

	private Renderer quadRenderer;
	private float feedWidth;
	private float feedHeight;
	private bool camReady;

	public void SetFeed (){

		float flipDisplayX = flipHorizontal ? 1f : -1f;
		float flipDisplayY = flipVertical ? 1f : -1f;

		// Set the webcam-Render-Quad to have the same aspect ratio as the video feed
		float aspectRatio = feedWidth / feedHeight;
		webcamRenderQuad.transform.localScale = new Vector3 (-10*flipDisplayX*aspectRatio*displayHeight, -10*flipDisplayY*displayHeight, 1.0f);


		// Debug.Log (" Feed Width: " + feedWidth + " Feed Height: " + feedHeight + " Aspect Ratio: " + aspectRatio);

		//New code
		//For setting up Cam Quad Display
		Texture2D targetTexture = new Texture2D((int)feedWidth, (int)feedHeight, TextureFormat.BGRA32, false);
		quadRenderer.material.mainTexture = targetTexture;

	}
///////////////////////////////////////////////// SET CAMERA FEED  END //////////////////////////////////////////////////////////////
}
