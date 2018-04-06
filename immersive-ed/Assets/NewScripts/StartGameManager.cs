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

	public float transistionTime = 5.0f;

	private Dictionary<string, WeatherMakerPrecipitationType> precipitationDict = new Dictionary<string, WeatherMakerPrecipitationType>{
																					{"anger", WeatherMakerPrecipitationType.Hail},
																					{"sadness", WeatherMakerPrecipitationType.Snow},
																					{"fear",WeatherMakerPrecipitationType.Rain}, 
																					{"joy",WeatherMakerPrecipitationType.Sleet}, 
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
		StartCoroutine(LoopThroughWeather());
	}

	void Update ()
	{
		
	}




	IEnumerator LoopThroughWeather ()
	{
		while (true) { 
			// Anger
			WeatherMakerScript.Instance.Precipitation = precipitationDict["anger"];
			// WeatherMakerScript.Instance.Clouds = WeatherMakerCloudType.Heavy;
			// Heavy clouds
			cloudScript.materialUsed.SetVector("_BaseColor", new Vector4(0.2f, 0.2f, 0.2f, 1.0f));
			cloudScript.materialUsed.SetFloat("_Density", 0.5f);
			displayText.text = "anger";
			yield return new WaitForSeconds (5);
			// Sadness
			WeatherMakerScript.Instance.Precipitation = precipitationDict["sadness"];
			// WeatherMakerScript.Instance.Clouds = cloudDict ["negative"];
			// Heavy clouds
			cloudScript.materialUsed.SetVector("_BaseColor", new Vector4(0.2f, 0.2f, 0.2f, 1.0f));
			cloudScript.materialUsed.SetFloat("_Density", 0.5f);
			displayText.text = "sadness";
			yield return new WaitForSeconds (transistionTime);
			// Fear
			WeatherMakerScript.Instance.Precipitation = precipitationDict["fear"];
			// WeatherMakerScript.Instance.Clouds = cloudDict ["negative"];
			// Heavy clouds
			cloudScript.materialUsed.SetVector("_BaseColor", new Vector4(0.2f, 0.2f, 0.2f, 1.0f));
			cloudScript.materialUsed.SetFloat("_Density", 0.5f);
			displayText.text = "fear";
			yield return new WaitForSeconds (transistionTime);
			// Joy
			WeatherMakerScript.Instance.Precipitation = precipitationDict["joy"];
			// WeatherMakerScript.Instance.Clouds = cloudDict ["positive"];
			// No clouds
			cloudScript.materialUsed.SetVector("_BaseColor", new Vector4(1.0f, 1.0f, 1.0f, 1.0f));
			cloudScript.materialUsed.SetFloat("_Density", -0.5f);
			displayText.text = "joy";
			yield return new WaitForSeconds (transistionTime);
			// Neutral
			WeatherMakerScript.Instance.Precipitation = precipitationDict["neutral"];
			// WeatherMakerScript.Instance.Clouds = cloudDict ["positive"];
			// Medium clouds
			cloudScript.materialUsed.SetVector("_BaseColor", new Vector4(0.5f, 0.5f, 0.5f, 1.0f));
			cloudScript.materialUsed.SetFloat("_Density", 0.0f);
			displayText.text = "neutral";
			yield return new WaitForSeconds (transistionTime);

			// Load the next scene
			transitionToNextScene("DefaultScene");
			// WeatherMakerScript.Instance.Precipitation = precipitationDict["disgust"];
			// WeatherMakerScript.Instance.Clouds = cloudDict ["negative"];
			// displayText.text = "disgust";
			// yield return new WaitForSeconds (transistionTime);
			// WeatherMakerScript.Instance.Precipitation = precipitationDict["surprise"];
			// WeatherMakerScript.Instance.Clouds = cloudDict ["negative"];
			// displayText.text = "surprise";
			// yield return new WaitForSeconds (transistionTime);

		}
    }

	// fades to black while transitioning to the next scene
	public void transitionToNextScene(string sceneName)
	{
		SceneManager.LoadSceneAsync(sceneName);
	}

}

