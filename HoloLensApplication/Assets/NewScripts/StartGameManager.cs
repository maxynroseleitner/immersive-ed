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

	private Coroutine tutorial;
	
	
	private Coroutine currentCoroutine1;
	private Coroutine currentCoroutine2;
	private Coroutine lightningCoroutine;


	private Dictionary<string, WeatherMakerPrecipitationType> precipitationDict = new Dictionary<string, WeatherMakerPrecipitationType>{
																					{"anger", WeatherMakerPrecipitationType.Hail},
																					{"sadness", WeatherMakerPrecipitationType.Sleet},
																					{"fear",WeatherMakerPrecipitationType.Snow}, 
																					{"joy",WeatherMakerPrecipitationType.Custom}, 
																					{"neutral",WeatherMakerPrecipitationType.None}};
	public Dictionary<string,Vector3> posOffsets = new Dictionary<string,Vector3>{
		{"anger", new Vector3(0f, 50f, 120f)  },
		{"sadness", new Vector3(0f, 80f, 150f) },
		{"fear", new Vector3(0f, 60f, 200f) },
		{"joy", new Vector3(0f, 8f, 10f) },
		{"neutral", new Vector3(0f,0f,0f) }
	};


	void Start () 
	{
		cloudScript = (CloudCamera3D) mainCamera.GetComponent<CloudCamera3D>();
		weatherScript = (WeatherMakerScript) weatherMaker.GetComponent<WeatherMakerScript>();
		displayText.text = "";

		// Default to no clouds
		cloudScale = 0.0f;
		cloudRoot.transform.localScale = new Vector3(cloudScale, cloudScale, cloudScale);

		// Begin the weather tutorial
		tutorial = StartCoroutine(LoopThroughWeather());
	}

	void Update ()
	{
		weatherScript.HailScript.ParticleSystem.transform.position = new Vector3 (cloudRoot.transform.position.x + posOffsets ["anger"].x, cloudRoot.transform.position.y + posOffsets ["anger"].y, cloudRoot.transform.position.z + posOffsets ["anger"].z);
		weatherScript.SleetScript.ParticleSystem.transform.position = new Vector3 (cloudRoot.transform.position.x + posOffsets ["sadness"].x, cloudRoot.transform.position.y + posOffsets ["sadness"].y, cloudRoot.transform.position.z + posOffsets ["sadness"].z);
		weatherScript.SnowScript.ParticleSystem.transform.position = new Vector3 (cloudRoot.transform.position.x + posOffsets ["fear"].x, cloudRoot.transform.position.y + posOffsets ["fear"].y, cloudRoot.transform.position.z + posOffsets ["fear"].z);
		weatherScript.CustomPrecipitationScript.ParticleSystem.transform.position = new Vector3 (cloudRoot.transform.position.x + posOffsets ["joy"].x, cloudRoot.transform.position.y + posOffsets ["joy"].y, cloudRoot.transform.position.z + posOffsets ["joy"].z);
//		weatherScript.HailScript.ParticleSystemSecondary.transform.position = new Vector3 (cloudRoot.transform.position.x + posOffsets ["anger"].x, cloudRoot.transform.position.y + posOffsets ["anger"].y, cloudRoot.transform.position.z + posOffsets ["anger"].z);
//		weatherScript.SleetScript.ParticleSystemSecondary.transform.position = new Vector3 (cloudRoot.transform.position.x + posOffsets ["sadness"].x, cloudRoot.transform.position.y + posOffsets ["sadness"].y, cloudRoot.transform.position.z + posOffsets ["sadness"].z);
		weatherScript.SnowScript.ParticleSystemSecondary.transform.position = new Vector3 (cloudRoot.transform.position.x + posOffsets ["fear"].x, cloudRoot.transform.position.y + posOffsets ["fear"].y, cloudRoot.transform.position.z + posOffsets ["fear"].z);
		weatherScript.CustomPrecipitationScript.ParticleSystemSecondary.transform.position = new Vector3 (cloudRoot.transform.position.x + posOffsets ["joy"].x, cloudRoot.transform.position.y + posOffsets ["joy"].y, cloudRoot.transform.position.z + posOffsets ["joy"].z);
	}

	// The main tutorial loop that cycles through all the emotion to weather mappings indefinitely
	IEnumerator LoopThroughWeather ()
	{
		while (true) { 
			/******************************* Anger *******************************/
			currentCoroutine1 = StartCoroutine(FadeTextToNext(textTransitionTime, displayText, "Anger"));
			currentCoroutine2 = StartCoroutine(FadeCloudsToNext(weatherTransitionTime, "anger"));
			lightningCoroutine = StartCoroutine(SummonIntenseLightning(weatherDisplayTime));
			WeatherMakerScript.Instance.Precipitation = precipitationDict["anger"];
			yield return new WaitForSeconds (weatherDisplayTime);
			
			/******************************* Sadness *******************************/
			currentCoroutine1 = StartCoroutine(FadeTextToNext(textTransitionTime, displayText, "Sadness"));
			currentCoroutine2 = StartCoroutine(FadeCloudsToNext(weatherTransitionTime, "sadness"));
			WeatherMakerScript.Instance.Precipitation = precipitationDict["sadness"];
			yield return new WaitForSeconds (weatherDisplayTime);

			/******************************* Fear *******************************/
			currentCoroutine1 = StartCoroutine(FadeTextToNext(textTransitionTime, displayText, "Fear"));
			currentCoroutine2 = StartCoroutine(FadeCloudsToNext(weatherTransitionTime, "fear"));
			WeatherMakerScript.Instance.Precipitation = precipitationDict["fear"];
			yield return new WaitForSeconds (weatherDisplayTime);

			/******************************* Joy *******************************/
			currentCoroutine1 = StartCoroutine(FadeTextToNext(textTransitionTime, displayText, "Joy"));
			currentCoroutine2 = StartCoroutine(FadeCloudsToNext(weatherTransitionTime, "joy"));
			WeatherMakerScript.Instance.Precipitation = precipitationDict["joy"];
			yield return new WaitForSeconds (weatherDisplayTime);

			/******************************* Neutral *******************************/
			currentCoroutine1 = StartCoroutine(FadeTextToNext(textTransitionTime, displayText, "Neutral"));
			currentCoroutine2 = StartCoroutine(FadeCloudsToNext(weatherTransitionTime, "neutral"));
			WeatherMakerScript.Instance.Precipitation = precipitationDict["neutral"];
			yield return new WaitForSeconds (weatherDisplayTime);
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

	// Load sad emotion
	public void sadEmo() {
		stopAllCoroutine();
		StopCoroutine(tutorial);

		// Start displaying the associated weather effects
		currentCoroutine1 = StartCoroutine(FadeTextToNext(textTransitionTime, displayText, "Sadness"));
		currentCoroutine2 = StartCoroutine(FadeCloudsToNext(weatherTransitionTime, "sadness"));
		WeatherMakerScript.Instance.Precipitation = precipitationDict["sadness"];	
	}

	// Load joy emotion
	public void joyEmo() {
		// Ensure no other coroutines are running
		stopAllCoroutine();
		StopCoroutine(tutorial);
		
		// Start displaying the associated weather effects
		currentCoroutine1 = StartCoroutine(FadeTextToNext(textTransitionTime, displayText, "Joy"));
		currentCoroutine2 = StartCoroutine(FadeCloudsToNext(weatherTransitionTime, "joy"));
		WeatherMakerScript.Instance.Precipitation = precipitationDict["joy"];
	}

	// Load angry emotion
	public void angryEmo() {
		// Ensure no other coroutines are running
		stopAllCoroutine(); 
		StopCoroutine(tutorial);

		// Start displaying the associated weather effects
		currentCoroutine1 = StartCoroutine(FadeTextToNext(textTransitionTime, displayText, "Anger"));
		currentCoroutine2 = StartCoroutine(FadeCloudsToNext(weatherTransitionTime, "anger"));
		lightningCoroutine = StartCoroutine(SummonIntenseLightning(weatherDisplayTime));
		WeatherMakerScript.Instance.Precipitation = precipitationDict["anger"];
	}

	//Load neutral emotion
	public void neutralEmo() {
		// Ensure no other coroutines are running
		stopAllCoroutine();
		StopCoroutine(tutorial);

		// Start displaying the associated weather effects
		currentCoroutine1 = StartCoroutine(FadeTextToNext(textTransitionTime, displayText, "Neutral"));
		currentCoroutine2 = StartCoroutine(FadeCloudsToNext(weatherTransitionTime, "neutral"));
		WeatherMakerScript.Instance.Precipitation = precipitationDict["neutral"];
	}

	// Load fear emotion
	public void fearEmo() {
		// Ensure no other coroutines are running
		stopAllCoroutine(); 
		StopCoroutine(tutorial);

		// Start displaying the associated weather effects
		currentCoroutine1 = StartCoroutine(FadeTextToNext(textTransitionTime, displayText, "Fear"));
		currentCoroutine2 = StartCoroutine(FadeCloudsToNext(weatherTransitionTime, "fear"));
		WeatherMakerScript.Instance.Precipitation = precipitationDict["fear"];
	}


	// Stop all emotion coroutines
	private void stopAllCoroutine() {
		if(currentCoroutine1 != null) StopCoroutine(currentCoroutine1);
		if(currentCoroutine2 != null) StopCoroutine(currentCoroutine2);
		if(lightningCoroutine != null) StopCoroutine(lightningCoroutine);
	}

	// Switch scene
	public void switchScene() {
		StopCoroutine(tutorial);
		StartCoroutine(endScene());
	} 

	// End the current scene and transition to the DefaultScene
	public IEnumerator endScene() {
		anim.SetBool("Fade", true);
		yield return new WaitUntil( () =>black.color.a == 1);
		transitionToNextScene("DefaultScene");
	}

}

