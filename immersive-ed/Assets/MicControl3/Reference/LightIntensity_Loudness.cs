using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//change light intensity based on loudness
public class LightIntensity_Loudness : MonoBehaviour {

	//create a slot to place our controller in which we want to call from. Instead of GameObject, one could also use MicControlC as a type.
	public GameObject controller;
	//next we need a float for easy acces to our loudness value. (Alternativly, you can also call the loudness value directly).
	float getLoudness=0.0f;

	//create an extra value to extra amplify our input. This way we can controll amplification per object.
	public float amp=1.0f;

	
	void Update () {
		//update our float every frame with the new input value. Use this value in your code.
		getLoudness = controller.GetComponent<MicControlC> ().loudness;

		gameObject.GetComponent<Light> ().intensity = getLoudness*amp;

	}


	//this value is only menat for the UI slider, you can ignore it.
	public void sensitivity(float sense){
		amp = sense;

	}




}
