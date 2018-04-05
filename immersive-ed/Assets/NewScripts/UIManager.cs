using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DigitalRuby.WeatherMaker;

using Affdex;

public class UIManager : MonoBehaviour {

	public GameObject gameManagerObject;
	private GameManager gameManagerScript;

	public Camera mainCamera;
	public GameObject inputDeviceCamera;
	//public GameObject webcamRenderPlane;
	public GameObject webcamRenderQuad;

	private CameraInput camInputScript;
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
	public GameObject weatherMaker;
	public WeatherMakerScript weatherScript;

	private Dictionary<string, WeatherMakerPrecipitationType> precipitationDict = new Dictionary<string, WeatherMakerPrecipitationType>{{"anger", WeatherMakerPrecipitationType.Hail},{"sadness", WeatherMakerPrecipitationType.Rain},{"fear",WeatherMakerPrecipitationType.None}, {"joy",WeatherMakerPrecipitationType.None}, {"neutral",WeatherMakerPrecipitationType.None}, {"disgust",WeatherMakerPrecipitationType.None}, {"surprise",WeatherMakerPrecipitationType.None}};
	private Dictionary<string, float> fogDict = new Dictionary<string, float>{{"anger",0.0f},{"sadness",0.0f},{"fear",1.0f}, {"joy",0.0f}, {"neutral",0.0f}, {"disgust",0.0f}, {"surprise",0.0f}};
	private Dictionary<string, WeatherMakerCloudType> cloudDict = new Dictionary<string, WeatherMakerCloudType>{{"positive",WeatherMakerCloudType.Light},{"negative",WeatherMakerCloudType.Heavy}};
	private Dictionary<string, float> dayDict = new Dictionary<string, float>{{"low",86400f},{"neutral",68400f},{"high",43200f}};

	// Volumetric cloud material
	public RaymarchedClouds cloudScript;

	// Use this for initialization
	void Start () {
		gameManagerScript = (GameManager) gameManagerObject.GetComponent(typeof(GameManager));
		camInputScript = (CameraInput) inputDeviceCamera.GetComponent<CameraInput>();
		//planeRenderer = (Renderer) webcamRenderPlane.GetComponent<Renderer>();
		quadRenderer = webcamRenderQuad.GetComponent<Renderer> ();
		weatherScript = (WeatherMakerScript) weatherMaker.GetComponent<WeatherMakerScript>();
		cloudScript = (RaymarchedClouds) mainCamera.GetComponent<RaymarchedClouds>();
		
		// Camera feed parameters
		if (camInputScript.Texture == null) {
			Debug.Log ("Camera not started");
			feedWidth = camInputScript.targetWidth;
			feedHeight = camInputScript.targetHeight;
			camReady = false;
		}

		SetFeed ();

		//Apply webcam texture to quad gameobject
		quadRenderer.material.mainTexture = camInputScript.Texture;

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

		// Initialize the weather
		WeatherMakerScript.Instance.Precipitation = WeatherMakerPrecipitationType.None;
		WeatherMakerScript.Instance.PrecipitationIntensity = 1.0f;
		WeatherMakerScript.Instance.Clouds = WeatherMakerCloudType.Light;

		// Start the background emotion updater
		StartCoroutine(RequestEmotionUpdate());

		// QUICK TEST: Sending new values to the cloud material shader to use in the raymarching/volume rendering
		// cloudScript.materialUsed.SetVector("_BaseColor", new Vector4(1.0f, 0.0f, 0.0f, 1.0f));
		// cloudScript.materialUsed.SetFloat("_Density", 1.0f);
		// END QUICK TEST
	}
	
	// Update is called once per frame
	void Update () {
		// Display the webcam input
		//planeRenderer.material.mainTexture = camInputScript.Texture;
		quadRenderer.material.mainTexture = camInputScript.Texture;

		if (gameManagerScript.useVocalToneEmotion)
		{
			ToneAnalysis vocalToneResults = gameManagerScript.getCurrentVocalEmotion ();
			// Debug.Log(vocalToneResults.TemperVal);
			// Debug.Log(vocalToneResults.TemperGroup);
//			Debug.Log(vocalToneResults.ArousalVal);
//			Debug.Log(vocalToneResults.ArousalGroup);
//			Debug.Log(vocalToneResults.ValenceVal);
//			Debug.Log(vocalToneResults.ValenceGroup);
		}

		SetCurrentWeather ();
		updateClouds();
	}

	private IEnumerator RequestEmotionUpdate()
	{
		// Debug.Log("Entered REQUEST EMOTION UPDATE COROUTINE.");
		while (true) 
		{
			yield return new WaitForSeconds(colorUpdateTime);

			if (gameManagerScript.useFacialEmotion)
			{
				EmotionStruct currentEmotions = gameManagerScript.getCurrentFacialEmotion();
				// Debug.Log("Joy: " + currentEmotions.joy);
				// Debug.Log("anger: " + currentEmotions.anger);
				// Debug.Log("fear: " + currentEmotions.fear);
				// Debug.Log("disgust: " + currentEmotions.disgust);
				// Debug.Log("sadness: " + currentEmotions.sadness);

				// Update facial emotion colors
				previousFacialEmotionColor = currentFacialEmotionColor;
				currentFacialEmotionColor = gameManagerScript.calculateEmotionColor(gameManagerScript.getCurrentFacialEmotion());

				// Update the emotion bars
				previousFacialEmotionBarWidth = currentFacialEmotionBarWidth;
				currentFacialEmotionBarWidth = gameManagerScript.getValueOfStrongestEmotion(gameManagerScript.getCurrentFacialEmotion()) * 2;
			}

			if (gameManagerScript.useWordSentimentEmotion)
			{
				// Update word sentiment emotion colors
				previousWordSentimentEmotionColor = currentWordSentimentEmotionColor;
				currentWordSentimentEmotionColor = gameManagerScript.calculateEmotionColor(gameManagerScript.getCurrentWordSentimentEmotion());

				EmotionStruct currentEmotions = gameManagerScript.getCurrentWordSentimentEmotion();
				// Debug.Log("Joy: " + currentEmotions.joy);
				// Debug.Log("anger: " + currentEmotions.anger);
				// Debug.Log("fear: " + currentEmotions.fear);
				// Debug.Log("disgust: " + currentEmotions.disgust);
				// Debug.Log("sadness: " + currentEmotions.sadness);

				previousWordSentimentEmotionBarWidth = currentWordSentimentEmotionBarWidth;
				currentWordSentimentEmotionBarWidth = gameManagerScript.getValueOfStrongestEmotion(gameManagerScript.getCurrentWordSentimentEmotion()) * 2;
			}

			StartCoroutine(UpdateBackgroundColor());
		}
	}

	// Coroutine enumerator for updating the current emotion color using linear interpolation over a predefined amount of time
	private IEnumerator UpdateBackgroundColor()
	{		
		// Debug.Log("Entered UPDATE BACKGROUND COLOR COROUTINE.");
		float t = 0;
		while (t < 1)
		{
			// Now the loop will execute on every end of frame until the condition is true
			if (gameManagerScript.useFacialEmotion)
			{
				// Update the facial emotion bar
				facialEmotionBar.rectTransform.sizeDelta = new Vector2(Mathf.Lerp(previousFacialEmotionBarWidth, currentFacialEmotionBarWidth, t),
					facialEmotionBar.rectTransform.sizeDelta.y);
				facialEmotionBar.color = Color.Lerp(previousFacialEmotionColor, currentFacialEmotionColor, t);
			}

			if (gameManagerScript.useWordSentimentEmotion)
			{
				// Update the word sentiment emotion bar
				wordSentimentEmotionBar.rectTransform.sizeDelta = new Vector2(Mathf.Lerp(previousWordSentimentEmotionBarWidth, currentWordSentimentEmotionBarWidth, t),
					wordSentimentEmotionBar.rectTransform.sizeDelta.y);
				wordSentimentEmotionBar.color = Color.Lerp(previousWordSentimentEmotionColor, currentWordSentimentEmotionColor, t);	
			}
			// mainCamera.backgroundColor = Color.Lerp(previousFacialEmotionColor, currentFacialEmotionColor, t);

			t += Time.deltaTime / lerpTime;

			yield return new WaitForEndOfFrame();
		}
	}

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

	public float CalculateLightningMin (float x) {

		return (1.5f / 40f) * ((100.0f - x));
	}

	public float CalculateLightningMax (float x) {

		return (2.0f / 40f) * ((100.0f - x)+ 0.5f);
	}

	public float CalculateTimeOfDay(float x) {
		return (86400 - 432.0f * x);
	}

	public void SetCurrentWeather (){
		// Get the strongest facial emotion type and value
		string currentStrongestEmotionString = gameManagerScript.getValueOfStrongestEmotionString(gameManagerScript.currentFacialEmotion);
		float currentStrongestEmotionValue = gameManagerScript.getValueOfStrongestEmotion(gameManagerScript.currentFacialEmotion);

		// PRECIPITATION
		WeatherMakerScript.Instance.Precipitation = precipitationDict[currentStrongestEmotionString];
		WeatherMakerScript.Instance.PrecipitationIntensity = currentStrongestEmotionValue * 0.01f;

		// FOG: May still need to be tuned?
		WeatherMakerScript.Instance.FogScript.TransitionFogDensity(fogDict[currentStrongestEmotionString] * currentStrongestEmotionValue * 0.01f, fogDict[currentStrongestEmotionString] * currentStrongestEmotionValue * 0.01f, 1.0f);

		// CLOUDS
		if (currentStrongestEmotionString == "neutral") 
		{
			WeatherMakerScript.Instance.Clouds = WeatherMakerCloudType.Medium;
		}
		if ( cloudDict.ContainsKey(gameManagerScript.getCurrentVocalEmotion().ValenceGroup ) )
		{
			WeatherMakerScript.Instance.Clouds = cloudDict[gameManagerScript.getCurrentVocalEmotion().ValenceGroup];
		}
		if (currentStrongestEmotionString == "joy") 
		{
			WeatherMakerScript.Instance.Clouds = WeatherMakerCloudType.None;
		}

		// TIME OF DAY
		if (dayDict.ContainsKey(gameManagerScript.getCurrentVocalEmotion().ArousalGroup))
		{
			WeatherMakerScript.Instance.DayNightScript.TimeOfDay = CalculateTimeOfDay(gameManagerScript.getCurrentVocalEmotion ().ArousalVal);
		}

		// LIGHTNING
		WeatherMakerThunderAndLightningScript thunderAndLightningScript = FindObjectOfType<WeatherMakerThunderAndLightningScript> ();
		if (gameManagerScript.getCurrentVocalEmotion().TemperGroup == "high") 
		{
			thunderAndLightningScript.EnableLightning = true;
		} 
		else 
		{
			thunderAndLightningScript.EnableLightning = false;
		}

		thunderAndLightningScript.LightningIntervalTimeRange = new RangeOfFloats {
			Minimum = CalculateLightningMin (gameManagerScript.getCurrentVocalEmotion().TemperVal),
			Maximum = CalculateLightningMin (gameManagerScript.getCurrentVocalEmotion().TemperVal)		// PROBLEM: This was previously CalculateLightningMin. Is that right?
		};

		thunderAndLightningScript.LightningIntenseProbability = gameManagerScript.getCurrentVocalEmotion().TemperVal * 0.01f;
	}

	private void updateClouds()
	{		
		if (Input.GetKeyDown(KeyCode.Alpha1))
		{
			// No clouds
			cloudScript.materialUsed.SetVector("_BaseColor", new Vector4(1.0f, 1.0f, 1.0f, 1.0f));
			cloudScript.materialUsed.SetFloat("_Density", -0.5f);
		}
		if (Input.GetKeyDown(KeyCode.Alpha2))
		{
			// Light clouds
			cloudScript.materialUsed.SetVector("_BaseColor", new Vector4(0.9f, 0.9f, 0.9f, 1.0f));
			cloudScript.materialUsed.SetFloat("_Density", -0.2f);
		}
		if (Input.GetKeyDown(KeyCode.Alpha3))
		{
			// Medium clouds
			cloudScript.materialUsed.SetVector("_BaseColor", new Vector4(0.5f, 0.5f, 0.5f, 1.0f));
			cloudScript.materialUsed.SetFloat("_Density", 0.0f);
		}
		if (Input.GetKeyDown(KeyCode.Alpha4))
		{
			// Heavy clouds
			cloudScript.materialUsed.SetVector("_BaseColor", new Vector4(0.2f, 0.2f, 0.2f, 1.0f));
			cloudScript.materialUsed.SetFloat("_Density", 0.5f);
		}
		if (Input.GetKeyDown(KeyCode.Space))
		{
			// Default cloud values for the material
			cloudScript.materialUsed.SetVector("_BaseColor", new Vector4(1.0f, 1.0f, 1.0f, 1.0f));
			cloudScript.materialUsed.SetFloat("_Density", 0.0f);
		}
	}

}
