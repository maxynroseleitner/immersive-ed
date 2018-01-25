using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Affdex;

public class DisplayWebCamOutput : MonoBehaviour {

	private CameraInput camInputScript;
	public Camera mainCamera;
	public GameObject cameraDisplayPlane;

	private Renderer planeRenderer;

	// Use this for initialization
	void Start () {
		camInputScript = mainCamera.GetComponent<CameraInput>();
		planeRenderer = cameraDisplayPlane.GetComponent<Renderer>();
		
		// Set the cameraDisplayPlane to have the same aspect ratio as the video feed
		float aspectRatio = camInputScript.targetWidth / (float) camInputScript.targetHeight;
		cameraDisplayPlane.transform.localScale = new Vector3(aspectRatio, 1.0f, 1.0f);
	}
	
	// Update is called once per frame
	void Update () {
		planeRenderer.material.mainTexture = camInputScript.Texture;
	}
}
