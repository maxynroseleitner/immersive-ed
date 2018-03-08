using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInfoScript : MonoBehaviour {

	GameManager gameManager;
	//DialogueManager dialogueManager;
	GameObject PlayerInfo;
	GameObject Bar;
	public Transform HealthBar;
	bool start = true;
	// Use this for initialization
	void Start () {
		PlayerInfo = GameObject.Find("PlayerInfo");
		//Bar = FindObjectOfType<GameManager> ();
		//Bar = PlayerInfo.GetComponentsInChildren<Image>;
		//dialogueManager = FindObjectOfType<DialogueManager> ();
		//healthbarBG.color = new Color(1.0f, 1.0f, 0.0f, 1.0f);
		//dialogueManager.StartDialogue();
	}
	
	// Update is called once per frame
	void Update () {

	}

	public void updateUI(float x, float y, float z){
		Vector3 PlayerInfoPosition = new Vector3 (x, y, z);
		gameObject.transform.position = PlayerInfoPosition;
	}

	public void updateHealthBar (EmotionStruct emotion)
	{

		Debug.Log("hey time to change the healBar size");
		float fillLength = HealthBar.GetComponent<Image> ().fillAmount;
		if (emotion.anger > emotion.joy || emotion.disgust > emotion.joy) {
				HealthBar.GetComponent<Image> ().fillAmount -= 0.002f;
		} else {
				HealthBar.GetComponent<Image> ().fillAmount += 0.004f;
		}
	}
}
