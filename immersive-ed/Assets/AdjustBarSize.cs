using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AdjustBarSize : MonoBehaviour {

	Image barContent;

	// Use this for initialization
	void Start () {
		//barContent = gameObject.GetComponent<Image> ();
		barContent = gameObject.GetComponentInChildren<Image>();

		Vector2 currentBarSize = gameObject.GetComponent<RectTransform> ().sizeDelta;

		barContent.rectTransform.sizeDelta = new Vector2 (currentBarSize.x, currentBarSize.y);
	}
	
	// Update is called once per frame
	void Update () {
		//Debug.Log ("Facial bar size (X): " + barContent.rectTransform.sizeDelta.x);
		//Debug.Log ("Facial bar size (Y): " + barContent.rectTransform.sizeDelta.y);
	}
}
