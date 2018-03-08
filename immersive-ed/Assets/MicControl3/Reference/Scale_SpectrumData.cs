using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//scale object array with spectrumData
public class Scale_SpectrumData : MonoBehaviour {

	//create a slot to place our controller in which we want to call from. Instead of GameObject, one could also use MicControlC as a type.
	public GameObject controller;
	//next we need a float array for easy acces to our spectrumData values. (Alternativly, you can also call the loudness value directly).
	float[] getSpectrumData;
	//we set a value to give extra amplification
	public float amp=1.0f;

	//we need a container to place all our objects in.
	public Transform[] objectList = null;

	//set default size
	public float defScale=1.0f;




	void Update () {
		//before we do anything at all, we first need to check if the controller has spectrumData enabled or not and that the controller has finished initializing.
		//This is to prevent reading the spectrumData array when it's still empty.
		if (controller.GetComponent<MicControlC> ().enableSpectrumData && controller.GetComponent<MicControlC> ().Initialized) {

			//update our float array every frame with the new input value. Use this value in your code.
			getSpectrumData = controller.GetComponent<MicControlC> ().spectrumData;

			//go through each transform in objectList and get a spectrumData value with the same position in the array. (maximum transform array size should not be larger than the spectrumData array).
			for (int i = 0; i < objectList.Length-1; i++) {
		
				objectList [i].localScale = new Vector3 (defScale, getSpectrumData [i+1] * amp, defScale);
	
			}


		}




	}



	//this value is only menat for the UI slider, you can ignore it.
	public void sensitivity(float sense){
		amp = sense;

	}





}
