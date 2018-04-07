using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DigitalRuby.WeatherMaker;
using UnityEngine.SceneManagement;

public class StartGameManager : MonoBehaviour {

	public Camera mainCamera;
	public Text displayText;
	public GameObject weatherMaker;
	public WeatherMakerScript weatherScript;
	private RaymarchedClouds cloudScript;

	public float weatherDisplayTime = 5.0f;
	public float weatherTransitionTime = 2.0f;
	public float textTransitionTime = 1.0f;

	public Animator anim;
	public Image black;

	private Dictionary<string, WeatherMakerPrecipitationType> precipitationDict = new Dictionary<string, WeatherMakerPrecipitationType>{
																					{"anger", WeatherMakerPrecipitationType.Hail},
																					{"sadness", WeatherMakerPrecipitationType.Rain},
																					{"fear",WeatherMakerPrecipitationType.Sleet}, 
																					{"joy",WeatherMakerPrecipitationType.None}, 
																					{"neutral",WeatherMakerPrecipitationType.None}, 
																					{"disgust",WeatherMakerPrecipitationType.None}, 
																					{"surprise",WeatherMakerPrecipitationType.None}};


	private Dictionary<string, float> fogDict = new Dictionary<string, float>{{"anger",0.0f},{"sadness",0.0f},{"fear",1.0f}, {"joy",0.0f}, {"neutral",0.0f}, {"disgust",0.0f}, {"surprise",0.0f}};
	private Dictionary<string, WeatherMakerCloudType> cloudDict = new Dictionary<string, WeatherMakerCloudType>{{"positive",WeatherMakerCloudType.Light},{"negative",WeatherMakerCloudType.Heavy}};
	private Dictionary<string, float> dayDict = new Dictionary<string, float>{{"low",86400f},{"neutral",68400f},{"high",43200f}};

	void Start () 
	{
		cloudScript = (RaymarchedClouds) mainCamera.GetComponent<RaymarchedClouds>();
		weatherScript = (WeatherMakerScript) weatherMaker.GetComponent<WeatherMakerScript>();
		displayText.text = "";
		// Default to no clouds
		cloudScript.materialUsed.SetVector("_BaseColor", new Vector4(1.0f, 1.0f, 1.0f, 1.0f));
		cloudScript.materialUsed.SetFloat("_Density", -0.5f);
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
			StartCoroutine(FadeCloudsToNext(weatherTransitionTime, "heavy"));
			WeatherMakerScript.Instance.Precipitation = precipitationDict["sadness"];
			yield return new WaitForSeconds (weatherDisplayTime);

			/******************************* Fear *******************************/
			StartCoroutine(FadeTextToNext(textTransitionTime, displayText, "Fear"));
			StartCoroutine(FadeCloudsToNext(weatherTransitionTime, "heavy"));
			WeatherMakerScript.Instance.Precipitation = precipitationDict["fear"];
			yield return new WaitForSeconds (weatherDisplayTime);

			/******************************* Joy *******************************/
			StartCoroutine(FadeTextToNext(textTransitionTime, displayText, "Joy"));
			StartCoroutine(FadeCloudsToNext(weatherTransitionTime, "none"));
			WeatherMakerScript.Instance.Precipitation = precipitationDict["joy"];
			yield return new WaitForSeconds (weatherDisplayTime);

			/******************************* Neutral *******************************/
			StartCoroutine(FadeTextToNext(textTransitionTime, displayText, "Neutral"));
			StartCoroutine(FadeCloudsToNext(weatherTransitionTime, "medium"));
			WeatherMakerScript.Instance.Precipitation = precipitationDict["neutral"];
			yield return new WaitForSeconds (weatherDisplayTime);

			/********************** Load the next scene *************************/
			anim.SetBool("Fade", true);
			yield return new WaitUntil( () =>black.color.a == 1);
			transitionToNextScene("DefaultScene");
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
			if (index % 10 == 0)
				WeatherMakerScript.Instance.LightningScript.CallIntenseLightning();
			index++;
			yield return null;
		}
	}

	// Fades from the current clouds to the given cloudType over t seconds
	public IEnumerator FadeCloudsToNext(float t, string cloudType)
	{
		Vector4 startColor = cloudScript.materialUsed.GetVector("_BaseColor");
		float startDensity = cloudScript.materialUsed.GetFloat("_Density");

		Vector4 finalColor = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
		float finalDensity = 0.0f;

		if (cloudType == "none")
		{
			// Set the final cloud color and density
			finalColor = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
			finalDensity = -0.5f;
		}
		else if (cloudType == "light")
		{
			// Set the final cloud color and density
			finalColor = new Vector4(0.9f, 0.9f, 0.9f, 1.0f);
			finalDensity = -0.2f;
		}
		else if (cloudType == "medium")
		{
			// Set the final cloud color and density
			finalColor = new Vector4(0.5f, 0.5f, 0.5f, 1.0f);
			finalDensity = 0.0f;
		}
		else if (cloudType == "heavy")
		{
			// Set the final cloud color and density
			finalColor = new Vector4(0.2f, 0.2f, 0.2f, 1.0f);
			finalDensity = 0.5f;
		}
		else 
		{
			// The given cloud type is not valid, so do nothing
			yield return null;
		}

		// Interpolate cloud properties over time
   		float elapsedTime = 0.0f;
     	while (elapsedTime < t)
     	{
			// Perform interpolation between the start and final cloud values
			Vector4 interpColor = Vector4.Lerp(startColor, finalColor, (elapsedTime / t));
			float interpDensity = Mathf.Lerp(startDensity, finalDensity, (elapsedTime / t));
			elapsedTime += Time.deltaTime;
			
			// Set the color and density in the material's shader
			cloudScript.materialUsed.SetVector("_BaseColor", interpColor);
			cloudScript.materialUsed.SetFloat("_Density", interpDensity);
			
			yield return null;
     	}
	}
}

