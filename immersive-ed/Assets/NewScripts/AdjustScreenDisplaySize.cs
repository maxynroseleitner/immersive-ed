using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdjustScreenDisplaySize : MonoBehaviour {

	int currentScreenWidth;
	int currentScreenHeight;

	// Use this for initialization
	void Start () {

		Debug.Log("Screen Width @Start  : " + Screen.width);
		Debug.Log("Screen Height @Start : " + Screen.height);

	}
	
	// Update is called once per frame
	void Update () {

		Debug.Log("Screen Width  : " + Screen.width);
		Debug.Log("Screen Height : " + Screen.height);

	}
}
