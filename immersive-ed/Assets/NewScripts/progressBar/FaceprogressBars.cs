using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class FaceprogressBars : MonoBehaviour {

	public Transform AngryProgressBar;
	public Transform JoyProgressBar;
	public Transform SadnessProgressBar;
	public Transform SurpriseProgressBar;
	public Transform FearProgressBar;
	public Transform DisgustProgressBar;
	public float threshold = 0.0005f;
	public int updateInterval = 600;
	public int countFrame = 0;
	// Use this for initialization
	void Start () {
		threshold = 0.05f;
		countFrame = 0;
		updateInterval = 30;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void UpdateUI (float amountAngry, float amountJoy, float amountSad, float amountSurprise, float amountFear, float amountDisgust)
	{
		if (countFrame == updateInterval) { //update every updateInterval
			
			if (amountAngry < threshold) {
				//Debug.Log(amountAngry + "angry " + "threshold" + threshold);
				AngryProgressBar.GetComponent<Image> ().fillAmount = 0.0f;
				AngryProgressBar.GetComponentInChildren<Text> ().text = "N/A";
			} else {
				AngryProgressBar.GetComponent<Image> ().fillAmount = amountAngry;
				AngryProgressBar.GetComponentInChildren<Text> ().text = "Angry";
			}

			if (amountJoy < threshold) {
				JoyProgressBar.GetComponentInChildren<Text> ().text = "NA";
				JoyProgressBar.GetComponent<Image> ().fillAmount = 0.0f;
			} else {
				JoyProgressBar.GetComponent<Image> ().fillAmount = amountJoy;
				JoyProgressBar.GetComponentInChildren<Text> ().text = "Joy";
			}

			if (amountSad < threshold) {
				SadnessProgressBar.GetComponentInChildren<Text> ().text = "NA";
				SadnessProgressBar.GetComponent<Image> ().fillAmount = 0.0f;
			} else {
				SadnessProgressBar.GetComponent<Image> ().fillAmount = amountSad;
				SadnessProgressBar.GetComponentInChildren<Text> ().text = "Sad";
			}

			if (amountSurprise < threshold) {
				SurpriseProgressBar.GetComponentInChildren<Text> ().text = "NA";
				SurpriseProgressBar.GetComponent<Image> ().fillAmount = 0.0f;
			} else {
				SurpriseProgressBar.GetComponent<Image> ().fillAmount = amountSurprise;
				SurpriseProgressBar.GetComponentInChildren<Text> ().text = "Surprise";
			}

			if (amountFear < threshold) {
				FearProgressBar.GetComponentInChildren<Text> ().text = "NA";
				FearProgressBar.GetComponent<Image> ().fillAmount = 0.0f;
			} else {
				FearProgressBar.GetComponent<Image> ().fillAmount = amountFear;
				FearProgressBar.GetComponentInChildren<Text> ().text = "Fear";
			}

			if (amountDisgust < threshold) {
				DisgustProgressBar.GetComponentInChildren<Text> ().text = "NA";
				DisgustProgressBar.GetComponent<Image> ().fillAmount = 0.0f;
			} else {
				DisgustProgressBar.GetComponent<Image> ().fillAmount = amountDisgust;
				DisgustProgressBar.GetComponentInChildren<Text> ().text = "Disgust";
			}
			countFrame = 0;
		} else {
			Debug.Log("hi i am the count" + countFrame);
			countFrame = countFrame + 1;
		}
	}
}
