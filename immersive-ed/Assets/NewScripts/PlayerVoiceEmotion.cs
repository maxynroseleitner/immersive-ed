using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerVoiceEmotion : MonoBehaviour {
	public GameObject audioInputObject;
	public Text currentVolume;
	MicrophoneInput micIn;
	// Use this for initialization
	void Start () {
		audioInputObject = GameObject.Find("MicMonitor");
		audioInputObject.SetActive(true);
		micIn = (MicrophoneInput) audioInputObject.GetComponent("MicrophoneInput");
		micIn.enabled = true;
	}
	
	// Update is called once per frame
	void Update () {
		currentVolume.text = micIn.loudness.ToString("G");
	}
}
