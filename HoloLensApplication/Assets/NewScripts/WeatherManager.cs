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
	private float cloudScale = 1.0f;
	public float weatherTransitionTime = 1.0f;

	public GameObject sunObject;
	private float sunScale = 0.0f;

	public GameObject moonObject;
	private float moonScale = 0.0f;

	public GameObject LightningRod;

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

		// Set some default values for the size of the clouds, the moon, and the sun
		cloudRoot.transform.localScale = new Vector3(cloudScale, cloudScale, cloudScale);
		sunObject.transform.localScale = new Vector3(sunScale, sunScale, sunScale);
		moonObject.transform.localScale = new Vector3(moonScale, moonScale, moonScale);
		WeatherMakerScript.Instance.LightningScript.LightningBoltScript.Source = cloudRoot;
		WeatherMakerScript.Instance.LightningScript.LightningBoltScript.Destination = LightningRod;
	}
	
	// Update is called once per frame
	void Update () 
	{
		weatherMakerScript.SleetScript.ParticleSystem.transform.position = new Vector3 (cloudRoot.transform.position.x-0.2f, 
																						cloudRoot.transform.position.y-1f,
																						cloudRoot.transform.position.z-1f);
	}

	// Updates the weather effects in the scene based on the given aggregate emotion detected.
	// t specifies the time over which to complete all transitions
	public void updateWeather(EmotionStruct aggregateEmotions, float t)
	{
		// Currently just takes the largest emotion value and uses that
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

	// Transitions to a sun with no clouds or precipitation effects
	private IEnumerator TransitionToJoy(float t, float joyVal)
	{
		StartCoroutine(FadeCloudsToNext(weatherTransitionTime, "joy"));
		WeatherMakerScript.Instance.Precipitation = WeatherMakerPrecipitationType.None;
		sunObject.GetComponent<Light> ().enabled = true;
		moonObject.GetComponent<Light> ().enabled = false;

		yield return null;
	}

	// Transitions to dark clouds and lightning
	private IEnumerator TransitionToAnger(float t, float angerVal)
	{
		StartCoroutine(FadeCloudsToNext(weatherTransitionTime, "anger"));
		WeatherMakerScript.Instance.Precipitation = WeatherMakerPrecipitationType.None;
		StartCoroutine(SummonIntenseLightning(1.0f)); // Run for 1 second
		sunObject.GetComponent<Light> ().enabled = false;
		moonObject.GetComponent<Light> ().enabled = false;

		yield return null;
	}

	// Transitions to rain and gray clouds
	private IEnumerator TransitionToSadness(float t, float sadnessVal)
	{
		StartCoroutine(FadeCloudsToNext(weatherTransitionTime, "sadness"));
		WeatherMakerScript.Instance.Precipitation = WeatherMakerPrecipitationType.Sleet;
		WeatherMakerScript.Instance.PrecipitationIntensity = 1.0f;
		sunObject.GetComponent<Light> ().enabled = false;
		moonObject.GetComponent<Light> ().enabled = false;
		yield return null;
	}

	// Transitions to a moon with eerie clouds
	private IEnumerator TransitionToFear(float t, float fearVal)
	{
		StartCoroutine(FadeCloudsToNext(weatherTransitionTime, "fear"));
		WeatherMakerScript.Instance.Precipitation = WeatherMakerPrecipitationType.None;
		sunObject.GetComponent<Light> ().enabled = false;
		moonObject.GetComponent<Light> ().enabled = true;

		yield return null;
	}

	// Transition to white puffy clouds
	private IEnumerator TransitionToNeutral(float t)
	{
		StartCoroutine(FadeCloudsToNext(weatherTransitionTime, "neutral"));
		WeatherMakerScript.Instance.Precipitation = WeatherMakerPrecipitationType.None;
		sunObject.GetComponent<Light> ().enabled = false;
		moonObject.GetComponent<Light> ().enabled = false;

		yield return null;
	}

	// Fades from the current clouds to the given cloudType over t seconds
	private IEnumerator FadeCloudsToNext(float t, string cloudType)
	{
		Color startLightColor = cloudScript.LightColor;
		Color startShadowColor = cloudScript.ShadowColor;
		float startCloudScale = cloudScale;
		float startSunScale = sunScale;
		float startMoonScale = moonScale;

		Color finalLightColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
		Color finalShadowColor = new Color(0.9f, 0.9f, 0.9f, 1.0f);
		float finalCloudScale = 0.0f;
		float finalSunScale = 0.0f;
		float finalMoonScale = 0.0f;

		if (cloudType == "joy")
		{
			// Set the final cloud color and density
			finalLightColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
			finalShadowColor = new Color(0.9f, 0.9f, 0.9f, 1.0f);
			finalCloudScale = 0.0f;
			finalSunScale = 1.0f;
			finalMoonScale = 0.0f;
		}
		else if (cloudType == "fear")
		{
			// Set the final cloud color and density
			finalLightColor = new Color32(169, 184, 255, 255);
			finalShadowColor = new Color32(40, 45, 58, 255);
			finalCloudScale = 1.0f;
			finalSunScale = 0.0f;
			finalMoonScale = 1.0f;
		}
		else if (cloudType == "sadness")
		{
			// Set the final cloud color and density
			finalLightColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
			finalShadowColor = new Color(0.9f, 0.9f, 0.9f, 1.0f);
			finalCloudScale = 1.0f;
			finalSunScale = 0.0f;
			finalMoonScale = 0.0f;
		}
		else if (cloudType == "anger")
		{
			// Set the final cloud color and density
			finalLightColor = new Color(0.6f, 0.6f, 0.6f, 1.0f);
			finalShadowColor = new Color(0.2f, 0.2f, 0.2f, 1.0f);
			finalCloudScale = 1.0f;
			finalSunScale = 0.0f;
			finalMoonScale = 0.0f;
		}
		else // neutral
		{
			// Set the final cloud color and density
			finalLightColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
			finalShadowColor = new Color(0.8f, 0.8f, 0.8f, 1.0f);
			finalCloudScale = 1.0f;
			finalSunScale = 0.0f;
			finalMoonScale = 0.0f;
		}

		// Interpolate cloud properties over time
   		float elapsedTime = 0.0f;
     	while (elapsedTime < t)
     	{
			// Perform interpolation between the start and final cloud values
			Color interpLightColor = Color.Lerp(startLightColor, finalLightColor, (elapsedTime / t));
			Color interpShadowColor = Color.Lerp(startShadowColor, finalShadowColor, (elapsedTime / t));
			float interpCloudScale = Mathf.Lerp(startCloudScale, finalCloudScale, (elapsedTime / t));
			float interpSunScale = Mathf.Lerp(startSunScale, finalSunScale, (elapsedTime / t));
			float interpMoonScale = Mathf.Lerp(startMoonScale, finalMoonScale, (elapsedTime / t));
			elapsedTime += Time.deltaTime;
			
			// Set the color and density in the material's shader
			cloudScript.LightColor = interpLightColor;
			cloudScript.ShadowColor = interpShadowColor;
			cloudScale = interpCloudScale;
			sunScale = interpSunScale;
			moonScale = interpMoonScale;

			// Set the scale of all the clouds, sun, and moon in the scene to reflect the change in their scales
			cloudRoot.transform.localScale = new Vector3(cloudScale, cloudScale, cloudScale);
			sunObject.transform.localScale = new Vector3(sunScale, sunScale, sunScale);
			moonObject.transform.localScale = new Vector3(moonScale, moonScale, moonScale);
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