using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Affdex;

public class UIManager : MonoBehaviour {

	public GameObject gameManagerObject;
	private GameManager gameManagerScript;

	public Camera mainCamera;
	public GameObject inputDeviceCamera;
	public GameObject webcamRenderPlane;

	private CameraInput camInputScript;
	private Renderer planeRenderer;

	// Emotion modality colors
	private Color currentFacialEmotionColor;
	private Color previousFacialEmotionColor;
	private Color currentWordSentimentEmotionColor;
	private Color previousWordSentimentEmotionColor;

	public float colorUpdateTime = 0.5f;	// Update the colors on-screen every X seconds
	private float lerpTime = 0.25f;

	public Image facialEmotionBar;
	private float currentFacialEmotionBarWidth;
	private float previousFacialEmotionBarWidth;

	public Image wordSentimentEmotionBar;
	private float currentWordSentimentEmotionBarWidth;
	private float previousWordSentimentEmotionBarWidth;

	// Use this for initialization
	void Start () {
		gameManagerScript = (GameManager) gameManagerObject.GetComponent(typeof(GameManager));
		camInputScript = (CameraInput) inputDeviceCamera.GetComponent<CameraInput>();
		planeRenderer = (Renderer) webcamRenderPlane.GetComponent<Renderer>();
		
		// Set the webcamRenderPlane to have the same aspect ratio as the video feed
		float aspectRatio = camInputScript.targetWidth / (float) camInputScript.targetHeight;
		webcamRenderPlane.transform.localScale = new Vector3(aspectRatio, 1.0f, 1.0f);

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
		planeRenderer.material.mainTexture = camInputScript.Texture;
		ToneAnalysis vocalToneResults = gameManagerScript.getCurrentVocalEmotion ();
		Debug.Log(vocalToneResults.TemperVal);
		Debug.Log(vocalToneResults.TemperGroup);
		Debug.Log(vocalToneResults.ArousalVal);
		Debug.Log(vocalToneResults.ArousalGroup);
		Debug.Log(vocalToneResults.ValenceVal);
		Debug.Log(vocalToneResults.ValenceGroup);
	}

	// Coroutine enumerator for updating the current emotion color using linear interpolation over a predefined amount of time
	private IEnumerator UpdateBackgroundColor()
	{		
		// Debug.Log("Entered UPDATE BACKGROUND COLOR COROUTINE.");
		float t = 0;
		while (t < 1)
		{
			// Now the loop will execute on every end of frame until the condition is true
			// Update the facial emotion bar
			facialEmotionBar.rectTransform.sizeDelta = new Vector2(Mathf.Lerp(previousFacialEmotionBarWidth, currentFacialEmotionBarWidth, t),
																   facialEmotionBar.rectTransform.sizeDelta.y);
			facialEmotionBar.color = Color.Lerp(previousFacialEmotionColor, currentFacialEmotionColor, t);

			// Update the word sentiment emotion bar
			wordSentimentEmotionBar.rectTransform.sizeDelta = new Vector2(Mathf.Lerp(previousWordSentimentEmotionBarWidth, currentWordSentimentEmotionBarWidth, t),
																   wordSentimentEmotionBar.rectTransform.sizeDelta.y);
			wordSentimentEmotionBar.color = Color.Lerp(previousWordSentimentEmotionColor, currentWordSentimentEmotionColor, t);	

			// mainCamera.backgroundColor = Color.Lerp(previousFacialEmotionColor, currentFacialEmotionColor, t);

			t += Time.deltaTime / lerpTime;

			yield return new WaitForEndOfFrame();
		}
	}

	private IEnumerator RequestEmotionUpdate()
	{
		// Debug.Log("Entered REQUEST EMOTION UPDATE COROUTINE.");
		while (true) 
		{
			yield return new WaitForSeconds(colorUpdateTime);

			// Update facial emotion colors
			previousFacialEmotionColor = currentFacialEmotionColor;
			currentFacialEmotionColor = gameManagerScript.calculateEmotionColor(gameManagerScript.getCurrentFacialEmotion());

			// Update word sentiment emotion colors
			previousWordSentimentEmotionColor = currentWordSentimentEmotionColor;
			currentWordSentimentEmotionColor = gameManagerScript.calculateEmotionColor(gameManagerScript.getCurrentWordSentimentEmotion());

			// Update the emotion bars
			previousFacialEmotionBarWidth = currentFacialEmotionBarWidth;
			currentFacialEmotionBarWidth = gameManagerScript.getValueOfStrongestEmotion(gameManagerScript.getCurrentFacialEmotion()) * 2;

			EmotionStruct currentEmotions = gameManagerScript.getCurrentWordSentimentEmotion();
			Debug.Log("Joy: " + currentEmotions.joy);
			Debug.Log("anger: " + currentEmotions.anger);
			Debug.Log("fear: " + currentEmotions.fear);
       		Debug.Log("disgust: " + currentEmotions.disgust);
        	Debug.Log("sadness: " + currentEmotions.sadness);

			previousWordSentimentEmotionBarWidth = currentWordSentimentEmotionBarWidth;
			currentWordSentimentEmotionBarWidth = gameManagerScript.getValueOfStrongestEmotion(gameManagerScript.getCurrentWordSentimentEmotion()) * 2;
			
			StartCoroutine(UpdateBackgroundColor());
		}
	}
}
