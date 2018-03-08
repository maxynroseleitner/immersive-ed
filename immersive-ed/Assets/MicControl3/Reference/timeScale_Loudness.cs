using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class timeScale_Loudness : MonoBehaviour {

	//create a slot to place our controller in which we want to call from. Instead of GameObject, one could also use MicControlC as a type.
	public GameObject controller;
	//next we need a float for easy acces to our loudness value. (Alternativly, you can also call the loudness value directly).
	float getLoudness=0.0f;
	public float amp=1.0f;
	public float minTimeScale=0.25f;

	
	void Update () {
		//update our float every frame with the new input value. Use this value in your code.
		getLoudness = controller.GetComponent<MicControlC> ().loudness;

		if (getLoudness >= minTimeScale) {
			Time.timeScale = getLoudness * amp;
		} else {
		
			Time.timeScale = minTimeScale;
		}

	}



	//this value is only menat for the UI slider, you can ignore it.
	public void sensitivity(float sense){
		amp = sense;

	}



}
