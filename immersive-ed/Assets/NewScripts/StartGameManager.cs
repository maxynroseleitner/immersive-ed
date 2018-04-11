using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DigitalRuby.WeatherMaker;
using UnityEngine.SceneManagement;
using TrueClouds;

public class StartGameManager : MonoBehaviour {

	public Camera mainCamera;
	public Text displayText;
	public GameObject weatherMaker;
	private WeatherMakerScript weatherScript;

	public float weatherDisplayTime = 5.0f;
	public float weatherTransitionTime = 2.0f;
	public float textTransitionTime = 1.0f;

	private CloudCamera3D cloudScript;
	public GameObject cloudRoot;
	public float cloudScale = 1.0f;

	public Animator anim;
	public Image black;

	private Dictionary<string, WeatherMakerPrecipitationType> precipitationDict = new Dictionary<string, WeatherMakerPrecipitationType>{
																					{"anger", WeatherMakerPrecipitationType.Hail},
																					{"sadness", WeatherMakerPrecipitationType.Rain},
																					{"fear",WeatherMakerPrecipitationType.Snow}, 
																					{"joy",WeatherMakerPrecipitationType.None}, 
																					{"neutral",WeatherMakerPrecipitationType.None}};


	void Start () 
	{
		cloudScript = (CloudCamera3D) mainCamera.GetComponent<CloudCamera3D>();
		weatherScript = (WeatherMakerScript) weatherMaker.GetComponent<WeatherMakerScript>();
		displayText.text = "";

		// Default to no clouds
		cloudScale = 0.0f;
		cloudRoot.transform.localScale = new Vector3(cloudScale, cloudScale, cloudScale);

		// Begin the weather tutorial
		StartCoroutine(LoopThroughWeather());
	}

	void Update ()
	{

	}

	// The main tutorial loop that cycles through all the emotion to weather mappings
	IEnumerator LoopThroughWeather ()
	{
		while (true) { 
			/******************************* Anger *******************************/
			StartCoroutine(FadeTextToNext(textTransitionTime, displayText, "Anger"));
			StartCoroutine(FadeCloudsToNext(weatherTransitionTime, "heavy"));
			StartCoroutine(SummonIntenseLightning(weatherDisplayTime));
			WeatherMakerScript.Instance.Precipitation = precipitationDict["anger"];
			yield return new WaitForSeconds (weatherDisplayTime);
			
			/******************************* Sadness *******************************/
			StartCoroutine(FadeTextToNext(textTransitionTime, displayText, "Sadness"));
			StartCoroutine(FadeCloudsToNext(weatherTransitionTime, "medium"));
			WeatherMakerScript.Instance.Precipitation = precipitationDict["sadness"];
			yield return new WaitForSeconds (weatherDisplayTime);

			/******************************* Fear *******************************/
			StartCoroutine(FadeTextToNext(textTransitionTime, displayText, "Fear"));
			StartCoroutine(FadeCloudsToNext(weatherTransitionTime, "light"));
			WeatherMakerScript.Instance.Precipitation = precipitationDict["fear"];
			yield return new WaitForSeconds (weatherDisplayTime);

			/******************************* Joy *******************************/
			StartCoroutine(FadeTextToNext(textTransitionTime, displayText, "Joy"));
			StartCoroutine(FadeCloudsToNext(weatherTransitionTime, "none"));
			WeatherMakerScript.Instance.Precipitation = precipitationDict["joy"];
			yield return new WaitForSeconds (weatherDisplayTime);

			/******************************* Neutral *******************************/
			StartCoroutine(FadeTextToNext(textTransitionTime, displayText, "Neutral"));
			StartCoroutine(FadeCloudsToNext(weatherTransitionTime, "neutral"));
			WeatherMakerScript.Instance.Precipitation = precipitationDict["neutral"];
			yield return new WaitForSeconds (weatherDisplayTime);

			/********************** Load the next scene *************************/
			anim.SetBool("Fade", true);
			yield return new WaitUntil( () =>black.color.a == 1);
			transitionToNextScene("NewDefaultScene");
		}
    }

	// fades to black while transitioning to the next scene
	public void transitionToNextScene(string sceneName)
	{
		SceneManager.LoadSceneAsync(sceneName);
	}

	// Fades the given Text i to display the given newText over 2*t seconds
    public IEnumerator FadeTextToNext(float t, Text i, string newText)
    {
		// Fade to zero alpha
		i.color = new Color(i.color.r, i.color.g, i.color.b, 1);
        while (i.color.a > 0.0f)
        {
            i.color = new Color(i.color.r, i.color.g, i.color.b, i.color.a - (Time.deltaTime / t));
            yield return null;
        }
		// Change the text
		i.text = newText;

		// Fade back to full alpha
        i.color = new Color(i.color.r, i.color.g, i.color.b, 0);
        while (i.color.a < 1.0f)
        {
            i.color = new Color(i.color.r, i.color.g, i.color.b, i.color.a + (Time.deltaTime / t));
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

	// Fades from the current clouds to the given cloudType over t seconds
	public IEnumerator FadeCloudsToNext(float t, string cloudType)
	{
		Color startLightColor = cloudScript.LightColor;
		Color startShadowColor = cloudScript.ShadowColor;
		float startScale = cloudScale;

		Color finalLightColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
		Color finalShadowColor = new Color(0.9f, 0.9f, 0.9f, 1.0f);
		float finalScale = 0.0f;

		if (cloudType == "none")
		{
			// Set the final cloud color and density
			finalLightColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
			finalShadowColor = new Color(0.9f, 0.9f, 0.9f, 1.0f);
			finalScale = 0.0f;
		}
		else if (cloudType == "light")
		{
			// Set the final cloud color and density
			finalLightColor = new Color(0.75f, 0.75f, 0.75f, 1.0f);
			finalShadowColor = new Color(0.65f, 0.65f, 0.65f, 1.0f);
			finalScale = 1.0f;
		}
		else if (cloudType == "medium")
		{
			// Set the final cloud color and density
			finalLightColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
			finalShadowColor = new Color(0.4f, 0.4f, 0.4f, 1.0f);
			finalScale = 1.0f;
		}
		else if (cloudType == "heavy")
		{
			// Set the final cloud color and density
			finalLightColor = new Color(0.2f, 0.2f, 0.2f, 1.0f);
			finalShadowColor = new Color(0.1f, 0.1f, 0.1f, 1.0f);
			finalScale = 1.0f;
		}
		else // neutral
		{
			// Set the final cloud color and density
			finalLightColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
			finalShadowColor = new Color(0.9f, 0.9f, 0.9f, 1.0f);
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
}

