using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DigitalRuby.WeatherMaker;
using TrueClouds;

public class WeatherManager : MonoBehaviour {

	public Camera mainCamera;

	public GameObject weatherMakerObject;
	private WeatherMakerScript weatherMakerScript;

	public GameObject cloudRoot;
	private CloudCamera3D cloudScript;
	
	public float cloudScale = 1.0f;
	public float weatherTransitionTime = 1.0f;

	public Dictionary<string, WeatherMakerPrecipitationType> precipitationDict = new Dictionary<string, WeatherMakerPrecipitationType>{
																					{"anger", WeatherMakerPrecipitationType.Hail},
																					{"sadness", WeatherMakerPrecipitationType.Sleet},
																					{"fear",WeatherMakerPrecipitationType.Snow}, 
																					{"joy",WeatherMakerPrecipitationType.Custom}, 
																					{"neutral",WeatherMakerPrecipitationType.None}};

	// Emotion activation thresholds (when modifying, ensure that they are in ascending order)
	private float[] joyThreshold = {25.0f, 50.0f, 75.0f, 100.0f};
	private float[] angerThreshold = {25.0f, 50.0f, 75.0f, 100.0f};
	private float[] sadnessThreshold = {25.0f, 50.0f, 75.0f, 100.0f};
	private float[] fearThreshold = {25.0f, 50.0f, 75.0f, 100.0f};
	public Dictionary<string,Vector3> posOffsets = new Dictionary<string,Vector3>{
		{"anger", new Vector3(0f, 50f, 120f)  },
		{"sadness", new Vector3(0f, 80f, 150f) },
		{"fear", new Vector3(0f, 60f, 200f) },
		{"joy", new Vector3(0f, 8f, 10f) },
		{"neutral", new Vector3(0f,0f,0f) }
	};

	// Use this for initialization
	void Start () 
	{
		// Get the scripts needed to run the weather effects
		cloudScript = (CloudCamera3D) mainCamera.GetComponent<CloudCamera3D>();
		weatherMakerScript = (WeatherMakerScript) weatherMakerObject.GetComponent<WeatherMakerScript>();

		// Set some default values
		cloudRoot.transform.localScale = new Vector3(cloudScale, cloudScale, cloudScale);
	}
	
	// Update is called once per frame
	void Update () 
	{
		weatherMakerScript.HailScript.ParticleSystem.transform.position = new Vector3 (cloudRoot.transform.position.x + posOffsets ["anger"].x, cloudRoot.transform.position.y + posOffsets ["anger"].y, cloudRoot.transform.position.z + posOffsets ["anger"].z);
		weatherMakerScript.SleetScript.ParticleSystem.transform.position = new Vector3 (cloudRoot.transform.position.x + posOffsets ["sadness"].x, cloudRoot.transform.position.y + posOffsets ["sadness"].y, cloudRoot.transform.position.z + posOffsets ["sadness"].z);
		weatherMakerScript.SnowScript.ParticleSystem.transform.position = new Vector3 (cloudRoot.transform.position.x + posOffsets ["fear"].x, cloudRoot.transform.position.y + posOffsets ["fear"].y, cloudRoot.transform.position.z + posOffsets ["fear"].z);
		weatherMakerScript.CustomPrecipitationScript.ParticleSystem.transform.position = new Vector3 (cloudRoot.transform.position.x + posOffsets ["joy"].x, cloudRoot.transform.position.y + posOffsets ["joy"].y, cloudRoot.transform.position.z + posOffsets ["joy"].z);
		weatherMakerScript.HailScript.ParticleSystemSecondary.transform.position = new Vector3 (cloudRoot.transform.position.x + posOffsets ["anger"].x, cloudRoot.transform.position.y + posOffsets ["anger"].y, cloudRoot.transform.position.z + posOffsets ["anger"].z);
		weatherMakerScript.SleetScript.ParticleSystemSecondary.transform.position = new Vector3 (cloudRoot.transform.position.x + posOffsets ["sadness"].x, cloudRoot.transform.position.y + posOffsets ["sadness"].y, cloudRoot.transform.position.z + posOffsets ["sadness"].z);
		weatherMakerScript.SnowScript.ParticleSystemSecondary.transform.position = new Vector3 (cloudRoot.transform.position.x + posOffsets ["fear"].x, cloudRoot.transform.position.y + posOffsets ["fear"].y, cloudRoot.transform.position.z + posOffsets ["fear"].z);
		weatherMakerScript.CustomPrecipitationScript.ParticleSystemSecondary.transform.position = new Vector3 (cloudRoot.transform.position.x + posOffsets ["joy"].x, cloudRoot.transform.position.y + posOffsets ["joy"].y, cloudRoot.transform.position.z + posOffsets ["joy"].z);
	}

	// Updates the weather effects in the scene based on the given aggregate emotion detected.
	// t specifies the time over which to complete all transitions
	public void updateWeather(EmotionStruct aggregateEmotions, float t)
	{
		// Currently just takes the largest emotion value and uses that
		// TODO: Enable the weather to transition to multiple weather states at once. This will require modifying the TransitionToEmotion() IEnumerators
		string strongestEmotion = aggregateEmotions.getSingleHighestEmotionString();
		Debug.Log("Strongest Emotion: " + strongestEmotion);
		if (strongestEmotion == "joy")
		{
			StartCoroutine(TransitionToJoy(t, aggregateEmotions.joy));
		}
		else if (strongestEmotion == "anger")
		{
			StartCoroutine(TransitionToAnger(t, aggregateEmotions.anger));
		}
		else if (strongestEmotion == "sadness")
		{
			StartCoroutine(TransitionToSadness(t, aggregateEmotions.sadness));
		}
		else if (strongestEmotion == "fear")
		{
			StartCoroutine(TransitionToFear(t, aggregateEmotions.fear));
		}
		else if (strongestEmotion == "neutral")
		{
			StartCoroutine(TransitionToNeutral(t));
		}
	}

	private IEnumerator TransitionToJoy(float t, float joyVal)
	{
		if (joyVal > joyThreshold[0])
		{
			StartCoroutine(FadeCloudsToNext(weatherTransitionTime, "joy"));
			WeatherMakerScript.Instance.Precipitation = precipitationDict["neutral"];
		}
		if (joyVal > joyThreshold[1])
		{
			WeatherMakerScript.Instance.Precipitation = precipitationDict["joy"];
			WeatherMakerScript.Instance.PrecipitationIntensity = 0.33f;
		}
		if (joyVal > joyThreshold[2])
		{
			WeatherMakerScript.Instance.PrecipitationIntensity = 0.66f;
		}
		if (joyVal > joyThreshold[3])
		{
			WeatherMakerScript.Instance.PrecipitationIntensity = 1.0f;
		}

		yield return null;
	}

	private IEnumerator TransitionToAnger(float t, float angerVal)
	{
		if (angerVal > angerThreshold[0])
		{
			StartCoroutine(FadeCloudsToNext(weatherTransitionTime, "anger"));
			WeatherMakerScript.Instance.Precipitation = precipitationDict["neutral"];
		}
		if (angerVal > angerThreshold[1])
		{
			WeatherMakerScript.Instance.Precipitation = precipitationDict["anger"];
			WeatherMakerScript.Instance.PrecipitationIntensity = 0.33f;
		}
		if (angerVal > angerThreshold[2])
		{
			WeatherMakerScript.Instance.PrecipitationIntensity = 0.66f;
		}
		if (angerVal > angerThreshold[3])
		{
			WeatherMakerScript.Instance.PrecipitationIntensity = 1.0f;
			StartCoroutine(SummonIntenseLightning(1.0f)); // Run for 1 second
		}

		yield return null;
	}

	private IEnumerator TransitionToSadness(float t, float sadnessVal)
	{
		if (sadnessVal > sadnessThreshold[0])
		{
			StartCoroutine(FadeCloudsToNext(weatherTransitionTime, "sadness"));
			WeatherMakerScript.Instance.Precipitation = precipitationDict["neutral"];
		}
		if (sadnessVal > sadnessThreshold[1])
		{
			WeatherMakerScript.Instance.Precipitation = precipitationDict["sadness"];
			WeatherMakerScript.Instance.PrecipitationIntensity = 0.33f;
		}
		if (sadnessVal > sadnessThreshold[2])
		{
			WeatherMakerScript.Instance.PrecipitationIntensity = 0.66f;
		}
		if (sadnessVal > sadnessThreshold[3])
		{
			WeatherMakerScript.Instance.PrecipitationIntensity = 1.0f;
		}

		yield return null;
	}

	private IEnumerator TransitionToFear(float t, float fearVal)
	{
		if (fearVal > fearThreshold[0])
		{
			StartCoroutine(FadeCloudsToNext(weatherTransitionTime, "fear"));
			WeatherMakerScript.Instance.Precipitation = precipitationDict["neutral"];
		}
		if (fearVal > fearThreshold[1])
		{
			WeatherMakerScript.Instance.Precipitation = precipitationDict["fear"];
			WeatherMakerScript.Instance.PrecipitationIntensity = 0.33f;
		}
		if (fearVal > fearThreshold[2])
		{
			WeatherMakerScript.Instance.PrecipitationIntensity = 0.66f;
		}
		if (fearVal > fearThreshold[3])
		{
			WeatherMakerScript.Instance.PrecipitationIntensity = 1.0f;
		}

		yield return null;
	}

	private IEnumerator TransitionToNeutral(float t)
	{
		StartCoroutine(FadeCloudsToNext(weatherTransitionTime, "neutral"));
		WeatherMakerScript.Instance.Precipitation = precipitationDict["neutral"];
		yield return null;
	}

	// Fades from the current clouds to the given cloudType over t seconds
	private IEnumerator FadeCloudsToNext(float t, string cloudType)
	{
		Color startLightColor = cloudScript.LightColor;
		Color startShadowColor = cloudScript.ShadowColor;
		float startScale = cloudScale;

		Color finalLightColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
		Color finalShadowColor = new Color(0.9f, 0.9f, 0.9f, 1.0f);
		float finalScale = 0.0f;

		if (cloudType == "joy")
		{
			// Set the final cloud color and density
			finalLightColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
			finalShadowColor = new Color(0.9f, 0.9f, 0.9f, 1.0f);
			finalScale = 0.0f;
		}
		else if (cloudType == "fear")
		{
			// Set the final cloud color and density
			finalLightColor = new Color32(249, 208, 249, 255);
			finalShadowColor = new Color32(115, 1, 113, 255);
			finalScale = 1.0f;
		}
		else if (cloudType == "sadness")
		{
			// Set the final cloud color and density
			finalLightColor = new Color32(98, 112, 255, 255);
			finalShadowColor = new Color32(23, 34, 142, 255);
			finalScale = 1.0f;
		}
		else if (cloudType == "anger")
		{
			// Set the final cloud color and density
			finalLightColor = new Color32(255, 187, 197, 255);
			finalShadowColor = new Color32(142, 0, 21, 255);
			finalScale = 1.0f;
		}
		else // neutral
		{
			// Set the final cloud color and density
			finalLightColor = new Color32(255, 255, 255, 255);
			finalShadowColor = new Color32(157, 157, 157, 255);
			finalScale = 1.0f;
		}

		// Interpolate cloud properties over time
   		float elapsedTime = 0.0f;
     	while (elapsedTime < t)
     	{
			// Perform interpolation between the start and final cloud values
			Color interpLightColor = Color.Lerp(startLightColor, finalLightColor, (elapsedTime / t));
			Color interpShadowColor = Color.Lerp(startShadowColor, finalShadowColor, (elapsedTime / t));
			float interpScale = Mathf.Lerp(startScale, finalScale, (elapsedTime / t));
			elapsedTime += Time.deltaTime;
			
			// Set the color and density in the material's shader
			cloudScript.LightColor = interpLightColor;
			cloudScript.ShadowColor = interpShadowColor;
			cloudScale = interpScale;

			// Set the scale of all the clouds in the scene to reflect the change in cloudScale
			cloudRoot.transform.localScale = new Vector3(cloudScale, cloudScale, cloudScale);
			yield return null;
     	}
	}
		
	// Use WeatherMaker to generate intense lightning strikes
	public IEnumerator SummonIntenseLightning(float duration)
	{
		float elapsedTime = 0.0f;
		int index = 0;
		while (elapsedTime < duration)
		{
			elapsedTime += Time.deltaTime;
			if (index % 20 == 0)
				WeatherMakerScript.Instance.LightningScript.CallIntenseLightning();
			index++;
			yield return null;
		}
	}
}