//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//
//public class Call_SpectrumData : MonoBehaviour {
//
//	//create a slot to place our controller in which we want to call from. Instead of GameObject, one could also use MicControlC as a type.
//	public GameObject controller;
//	//next we need a float array for easy acces to our spectrumData values. (Alternativly, you can also call the loudness value directly).
//	float[] getSpectrumData;
//
//
//	void Update () {
//
//	//before we do anything at all, we first need to check if the controller has spectrumData enabled or not.
//if (controller.GetComponent<MicControlC> ().enableSpectrumData) {
//
//		//update our float array every frame with the new input value. Use this value in your code.
//		getSpectrumData = controller.GetComponent<MicControlC> ().spectrumData;
//
//
//
//	}
//
//}
//
//}
