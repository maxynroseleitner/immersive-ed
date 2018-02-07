using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

	public Vector3 HealthBarCoordinates = new Vector3(0, 0, 0);
	public Color healthBarColor;
	public Vector2 healthBarSize;

	// Use this for initialization
	void Start () {
		SetHealthBarSize (160f);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void SetHealthBarCoordinates(float xValue, float yValue){
		HealthBarCoordinates.x = xValue/100 - 5;
		HealthBarCoordinates.y = yValue/200;
	}

	public void SetHealthBarColor(Color newHealthBarColor){
		healthBarColor = newHealthBarColor;
	}

	public void SetHealthBarSize(float newSize){
		healthBarSize.x = 0.0002f*newSize;
		healthBarSize.y = 0.0003f*newSize;
	}

}
