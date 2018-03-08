using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

	public GameObject gameManager;
	public GameObject uiManager;

	public GameObject facialEmotionAnalyzerObject;
	public GameObject wordSentimentEmotionAnalyzerObject;
	public GameObject vocalEmotionAnalyzerObject;

	private FacialEmotionAnalyzer facialAnalyzer;
	private SentimentAnalyzer wordAnalyzer;
	// private VocalEmotionAnalyzer vocalAnalyzer;

	// Flags for enabling and disabling certain emotion analysis features
	public bool useFacialEmotion = false;
	public bool useWordSentimentEmotion = false;
	public bool useVocalToneEmotion = false;

	public EmotionStruct currentFacialEmotion;
	public EmotionStruct currentWordSentimentEmotion;
	public EmotionStruct currentVocalEmotion;
	private float emotionThreshold = 10.0f;

	// Emotion bar data
	public FaceStruct currentFace;

	// Use this for initialization
	void Start () {
		// Initialize the current emotion
		currentFacialEmotion = new EmotionStruct();
		currentWordSentimentEmotion = new EmotionStruct();
		currentVocalEmotion = new EmotionStruct();

		// Initialize the face struct
		currentFace = new FaceStruct();

		// Find the script for facial emotion analysis
		try
		{
			facialAnalyzer = (FacialEmotionAnalyzer) facialEmotionAnalyzerObject.GetComponent(typeof(FacialEmotionAnalyzer)); // this seems to fail silently...
		}
		catch (System.Exception)
		{
			Debug.Log("Unable to find facial emotion analyzer. This functionality will be disabled.");
			useFacialEmotion = false;
		}

		// Find the script for sentiment analysis of speech to text results
		try
		{
			wordAnalyzer = (SentimentAnalyzer) wordSentimentEmotionAnalyzerObject.GetComponent(typeof(SentimentAnalyzer));
		}
		catch (System.Exception)
		{
			Debug.Log("Unable to find the sentiment analyzer. This functionality will be disabled.");
			useWordSentimentEmotion = false;
		}
	}
	
	// Update is called once per frame
	void Update () {
		// Pull in the most recent emotional state for each of the modalities
		currentFacialEmotion = facialAnalyzer.getCurrentEmotions();
		currentWordSentimentEmotion = wordAnalyzer.getCurrentEmotions();
		currentFace = facialAnalyzer.getCurrentFace();
	}

	public EmotionStruct getCurrentFacialEmotion()
	{
		return currentFacialEmotion;
	}

	public EmotionStruct getCurrentWordSentimentEmotion()
	{
		return currentWordSentimentEmotion;
	}

	// Returns the strongest emotion present in the current cumulative emotion
	public float getValueOfStrongestEmotion(EmotionStruct emotions)
	{
		if (emotions.joy > emotions.fear &&
			emotions.joy > emotions.disgust &&
			emotions.joy > emotions.sadness && 
			emotions.joy > emotions.anger && 
			emotions.joy > emotions.surprise &&
			emotions.joy > emotionThreshold)
		{
			return emotions.joy;
		}
		else if (emotions.fear > emotions.disgust &&
				 emotions.fear > emotions.sadness &&
				 emotions.fear > emotions.anger &&
				 emotions.fear > emotions.surprise &&
				 emotions.fear > emotionThreshold)
		{
			return emotions.fear;
		}
		else if (emotions.disgust > emotions.sadness &&
				 emotions.disgust > emotions.anger &&
				 emotions.disgust > emotions.surprise &&
				 emotions.disgust > emotionThreshold)
		{
			return emotions.disgust;
		}
		else if (emotions.sadness > emotions.anger &&
				 emotions.sadness > emotions.surprise &&
				 emotions.sadness > emotionThreshold)
		{
			return emotions.sadness;
		}
		else if (emotions.anger > emotions.surprise &&
				 emotions.anger > emotionThreshold)
		{
			return emotions.anger;
		}
		else if(emotions.surprise > emotionThreshold)
		{
			return emotions.surprise;
		}
		else
		{
			return 0.0f;
		}
	}

	// Returns a color to be used by the user interface based on the current synthesized emotion
	public Color calculateEmotionColor(EmotionStruct emotions)
	{
		if (emotions.joy > emotions.fear &&
			emotions.joy > emotions.disgust &&
			emotions.joy > emotions.sadness && 
			emotions.joy > emotions.anger && 
			emotions.joy > emotions.surprise &&
			emotions.joy > emotionThreshold)
		{
			return new Color(0.0f, 1.0f, 0.0f, 1.0f);		// green
		}
		else if (emotions.fear > emotions.disgust &&
				 emotions.fear > emotions.sadness &&
				 emotions.fear > emotions.anger &&
				 emotions.fear > emotions.surprise &&
				 emotions.fear > emotionThreshold)
		{
			return new Color(1.0f, 0.0f, 1.0f, 1.0f);		// magenta
		}
		else if (emotions.disgust > emotions.sadness &&
				 emotions.disgust > emotions.anger &&
				 emotions.disgust > emotions.surprise &&
				 emotions.disgust > emotionThreshold)
		{
			return new Color(1.0f, 1.0f, 0.0f, 1.0f);		// yellow
		}
		else if (emotions.sadness > emotions.anger &&
				 emotions.sadness > emotions.surprise &&
				 emotions.sadness > emotionThreshold)
		{
			return new Color(0.0f, 0.0f, 1.0f, 1.0f);		// blue
		}
		else if (emotions.anger > emotions.surprise &&
				 emotions.anger > emotionThreshold)
		{
			return new Color(1.0f, 0.0f, 0.0f, 1.0f);		// red
		}
		else if(emotions.surprise > emotionThreshold)
		{
			return new Color(1.0f, 1.0f, 1.0f, 1.0f);		// white
		}
		else
		{
			return new Color(0.0f, 0.0f, 0.0f, 1.0f);		// black
		}
	}
}
