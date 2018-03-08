using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//scale an object based on the average loudness gathered beteen two frames.
public class Frame_average_Loudness : MonoBehaviour {
	//store the micController in here.
	public GameObject controller;

	//temporary values to store information between frames
	float oldvalue=0.0f;
	float newvalue=0.0f;

	//value to store the average of above values.
	float averagedLoudness=0.0f;

	//create an extra value to extra amplify our input. This way we can controll amplification per object.
	public float amp=1.0f;

		
	// Update is called once per frame
	void Update () {
		//set the loudness in our newvalue
		newvalue = controller.GetComponent<MicControlC> ().loudness;

		//calculate the average. formula= (a+b)/c. (C equals the amount of values used, in this case 2)
		averagedLoudness = (newvalue + oldvalue) / 2;

		transform.localScale = new Vector3 (0.5f+averagedLoudness*amp,0.5f+averagedLoudness*amp,0.5f+averagedLoudness*amp);

		//save the newvalue for the use in next frame.
		oldvalue = newvalue;

		
	}



	//this value is only menat for the UI slider, you can ignore it.
	public void sensitivity(float sense){
		amp = sense;

	}





}
