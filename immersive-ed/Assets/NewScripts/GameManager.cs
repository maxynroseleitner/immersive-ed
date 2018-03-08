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

	// public EmotionStruct currentCumulativeEmotion;
	public EmotionStruct currentFacialEmotion;
	public EmotionStruct currentWordSentimentEmotion;
	public EmotionStruct currentVocalEmotion;
	private float emotionThreshold = 10.0f;

	// Use this for initialization
	void Start () {
		// Initialize the current emotion
		// currentCumulativeEmotion = new EmotionStruct();
		currentFacialEmotion = new EmotionStruct();
		currentWordSentimentEmotion = new EmotionStruct();
		currentVocalEmotion = new EmotionStruct();

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
		// Calculate a synthesized emotional state for the user
		// calculateCumulativeEmotion();

		// Pull in the most recent emotional state for each of the modalities
		currentFacialEmotion = facialAnalyzer.getCurrentEmotions();
		currentWordSentimentEmotion = wordAnalyzer.getCurrentEmotions();
	}

	public EmotionStruct getCurrentFacialEmotion()
	{
		return currentFacialEmotion;
	}

	public EmotionStruct getCurrentWordSentimentEmotion()
	{
		return currentWordSentimentEmotion;
	}

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

	// Returns a final emotion based on the emotion input types that are specified using a simple weighted average.
	// public void calculateCumulativeEmotion() {

	// 	EmotionStruct emotionSum = new EmotionStruct();
	// 	int numEmotionModes = 0;

	// 	if (useFacialEmotion)
	// 	{
	// 		EmotionStruct facialEmotions = facialAnalyzer.getCurrentEmotions();
	// 		emotionSum.joy += facialEmotions.joy;
	// 		emotionSum.sadness += facialEmotions.sadness;
	// 		emotionSum.anger += facialEmotions.anger;
	// 		emotionSum.fear += facialEmotions.fear;
	// 		emotionSum.disgust += facialEmotions.disgust;
	// 		emotionSum.surprise += facialEmotions.surprise;

	// 		numEmotionModes++;
	// 	}
	// 	if (useWordSentimentEmotion)
	// 	{
	// 		Debug.Log("Need to implement emotion from word sentiment.");
	// 	}
	// 	if (useVocalToneEmotion)
	// 	{
	// 		Debug.Log("Need to implement emotion from vocal tone.");
	// 	}

	// 	if (numEmotionModes > 0) 
	// 	{
	// 		currentCumulativeEmotion.joy = emotionSum.joy / (float) numEmotionModes;
	// 		currentCumulativeEmotion.sadness = emotionSum.sadness / (float) numEmotionModes;
	// 		currentCumulativeEmotion.anger = emotionSum.anger / (float) numEmotionModes;
	// 		currentCumulativeEmotion.fear = emotionSum.fear / (float) numEmotionModes;
	// 		currentCumulativeEmotion.disgust = emotionSum.disgust / (float) numEmotionModes;
	// 		currentCumulativeEmotion.surprise = emotionSum.surprise / (float) numEmotionModes;
	// 	}
			
	// }

	// // Returns a color to be used by the user interface based on the current synthesized emotion
	// public Color getCurrentCumulativeEmotionColor()
	// {
	// 	if (currentCumulativeEmotion.joy > currentCumulativeEmotion.fear &&
	// 		currentCumulativeEmotion.joy > currentCumulativeEmotion.disgust &&
	// 		currentCumulativeEmotion.joy > currentCumulativeEmotion.sadness && 
	// 		currentCumulativeEmotion.joy > currentCumulativeEmotion.anger && 
	// 		currentCumulativeEmotion.joy > currentCumulativeEmotion.surprise &&
	// 		currentCumulativeEmotion.joy > emotionThreshold)
	// 	{
	// 		return new Color(0.0f, 1.0f, 0.0f, 1.0f);		// green
	// 	}
	// 	else if (currentCumulativeEmotion.fear > currentCumulativeEmotion.disgust &&
	// 			 currentCumulativeEmotion.fear > currentCumulativeEmotion.sadness &&
	// 			 currentCumulativeEmotion.fear > currentCumulativeEmotion.anger &&
	// 			 currentCumulativeEmotion.fear > currentCumulativeEmotion.surprise &&
	// 			 currentCumulativeEmotion.fear > emotionThreshold)
	// 	{
	// 		return new Color(1.0f, 0.0f, 1.0f, 1.0f);		// magenta
	// 	}
	// 	else if (currentCumulativeEmotion.disgust > currentCumulativeEmotion.sadness &&
	// 			 currentCumulativeEmotion.disgust > currentCumulativeEmotion.anger &&
	// 			 currentCumulativeEmotion.disgust > currentCumulativeEmotion.surprise &&
	// 			 currentCumulativeEmotion.disgust > emotionThreshold)
	// 	{
	// 		return new Color(1.0f, 1.0f, 0.0f, 1.0f);		// yellow
	// 	}
	// 	else if (currentCumulativeEmotion.sadness > currentCumulativeEmotion.anger &&
	// 			 currentCumulativeEmotion.sadness > currentCumulativeEmotion.surprise &&
	// 			 currentCumulativeEmotion.sadness > emotionThreshold)
	// 	{
	// 		return new Color(0.0f, 0.0f, 1.0f, 1.0f);		// blue
	// 	}
	// 	else if (currentCumulativeEmotion.anger > currentCumulativeEmotion.surprise &&
	// 			 currentCumulativeEmotion.anger > emotionThreshold)
	// 	{
	// 		return new Color(1.0f, 0.0f, 0.0f, 1.0f);		// red
	// 	}
	// 	else if(currentCumulativeEmotion.surprise > emotionThreshold)
	// 	{
	// 		return new Color(1.0f, 1.0f, 1.0f, 1.0f);		// white
	// 	}
	// 	else
	// 	{
	// 		return new Color(0.0f, 0.0f, 0.0f, 1.0f);		// black
	// 	}
	// }

}
