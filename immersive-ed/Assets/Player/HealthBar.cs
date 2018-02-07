using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour {

	GameManager gameManager;
	Image healthbar;

	// Use this for initialization
	void Start () {
		gameManager = FindObjectOfType<GameManager> ();
		healthbar = gameObject.GetComponentInChildren<Image> ();
		//healthbarBG.color = new Color(1.0f, 1.0f, 0.0f, 1.0f);
	}
	
	// Update is called once per frame
	void Update () {
		Vector3 healthBarPosition = new Vector3 (gameManager.HealthBarCoordinates.x, gameManager.HealthBarCoordinates.y, gameManager.HealthBarCoordinates.z);
		//gameObject.transform.parent.position = healthBarPosition;
		gameObject.transform.position = healthBarPosition;
		healthbar.color = gameManager.healthBarColor;

		//RectTransform healthBarTransform = gameObject.transform as RectTransform;

		//Vector2 healthBarSize = gameObject.GetComponent<RectTransform> ().rect.size;


		//Debug.Log (healthBarTransform.sc);
		//healthBarTransform.sizeDelta = new Vector2 (0.03f, 10f);

		Vector2 healthBarNewSize = new Vector2 (gameManager.healthBarSize.x, gameManager.healthBarSize.y);
		gameObject.transform.localScale = healthBarNewSize;
		//healthBarSize = healthBarNewSize;

		//healthBarTransform.rect.size = healthBarSize;
		//healthBarTransform.anchoredPosition.Scale = gameManager.healthBarSize;
		//healthBarTransform.SetSizeWithCurrentAnchors (healthBarTransform.anchoredPosition.);
	}
}
