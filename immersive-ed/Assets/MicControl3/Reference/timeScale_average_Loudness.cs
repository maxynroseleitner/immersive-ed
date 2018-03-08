using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class timeScale_average_Loudness : MonoBehaviour {

	//store the micController in here.
	public GameObject controller;

	//temporary values to store information between frames
	float oldvalue=0.0f;
	float newvalue=0.0f;

	//value to store the average of above values.
	float averagedLoudness=0.0f;

	public float minTimeScale=0.25f;



	//create an extra value to extra amplify our input. This way we can controll amplification per object.
	public float amp=1.0f;


	
	void Update () {
		//set the loudness in our newvalue
		newvalue = controller.GetComponent<MicControlC> ().loudness;

		if (newvalue >= minTimeScale) {
			//calculate the average. formula= (a+b)/c. (C equals the amount of values used, in this case 2)
			averagedLoudness = ((newvalue + oldvalue) / 2) * amp;
			averagedLoudness = Mathf.Clamp (averagedLoudness,0.0f,100);
			Time.timeScale = averagedLoudness;
		} else{

		Time.timeScale = minTimeScale;
	}

		//save the newvalue for the use in next frame.
		oldvalue = newvalue;


			
	

	}



	//this value is only menat for the UI slider, you can ignore it.
	public void sensitivity(float sense){
		amp = sense;

	}



}
