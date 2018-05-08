using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

	//new
	// Import Webcam input object
	public DisplayInfo displayExtraInfo;
	public TCPNetworking tcpNetworkingScript;
	public VideoPanel videoPanelScript;

	// For checking if camera has started
	[Space(10)]
	private bool camReady;

	//old
	public GameObject gameManager;
	public GameObject uiManager;
	private UIManager uiManagerScript;

	public GameObject facialEmotionAnalyzerObject;
	public GameObject wordSentimentEmotionAnalyzerObject;
	public GameObject vocalEmotionAnalyzerObject;

//	private FacialEmotionAnalyzer facialAnalyzer;
	private SentimentAnalyzer wordAnalyzer;
	// private VocalEmotionAnalyzer vocalAnalyzer;
	private MicControlC vocalAnalyzer;

	// Flags for enabling and disabling certain emotion analysis features
	public bool useFacialEmotion = false;
	public bool useWordSentimentEmotion = false;
	public bool useVocalToneEmotion = false;

	public EmotionStruct currentFacialEmotion = new EmotionStruct();
	public EmotionStruct currentWordSentimentEmotion = new EmotionStruct();
	// public ToneAnalysis currentVocalEmotion = new ToneAnalysis();
	public EmotionStruct currentVocalEmotion = new EmotionStruct();
	private float emotionThreshold = 10.0f;

	private ArrayList facialEmotionWindow;
	private int facialEmotionWindowSize = 5;

	void Awake()
	{
		
	}

	IEnumerator passAudio()
	{
		while (true)
		{
			Debug.Log (wordAnalyzer);
			vocalAnalyzer.SetAudClip(wordAnalyzer.GetAudClip());
			yield return new WaitForSeconds(1.0f);
		}
	}

	// Use this for initialization
	void Start () {
		// Initialize the facial emotion window
		facialEmotionWindow = new ArrayList ();
		facialEmotionWindow.Capacity = facialEmotionWindowSize;

		uiManagerScript = uiManager.GetComponent<UIManager> ();

//		if (!useFacialEmotion)
//		{
//			facialEmotionAnalyzerObject.SetActive(false);
//		}
//		else
//		{
//			facialAnalyzer = (FacialEmotionAnalyzer) facialEmotionAnalyzerObject.GetComponent(typeof(FacialEmotionAnalyzer));
//		}

		if (!useWordSentimentEmotion)
		{
			wordSentimentEmotionAnalyzerObject.SetActive(false);
		}
		else
		{
			wordAnalyzer = (SentimentAnalyzer) wordSentimentEmotionAnalyzerObject.GetComponent(typeof(SentimentAnalyzer));
		}

		if (!useVocalToneEmotion | !useWordSentimentEmotion)
		{
			vocalEmotionAnalyzerObject.SetActive(false);
		}
		else
		{
			vocalAnalyzer = (MicControlC) vocalEmotionAnalyzerObject.GetComponent(typeof(MicControlC));
			StartCoroutine (passAudio ());
		}

		if (tcpNetworkingScript.GetNetworkIsNotActive()) {
			if(videoPanelScript.startSending){
				tcpNetworkingScript.SetNetworkIsNotActive();
				tcpNetworkingScript.InitializeCommunicationOverTCP();
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
		// Pull in the most recent emotional state for each of the modalities
//		{
//			currentFacialEmotion = facialAnalyzer.getCurrentEmotions();
//			// Debug.Log("got facial emotion struct");
//		}
//			
		if (useWordSentimentEmotion)
		{
			currentWordSentimentEmotion = wordAnalyzer.getCurrentEmotions();
		}
			
		if (useVocalToneEmotion)
		{
			currentVocalEmotion = convertToneAnalysisToEmotionStruct(vocalAnalyzer.getVocalToneResults());
		}

		//new
		if (!camReady) {
			if (videoPanelScript.GetCameraStatus()) {
				Debug.Log ("Camera is Working");
				camReady = true;
			} else {
				Debug.Log ("Camera not started");
				camReady = false;
			}
		}

		if (tcpNetworkingScript.GetNetworkIsNotActive()) {
			if(videoPanelScript.startSending){
				tcpNetworkingScript.SetNetworkIsNotActive();
				tcpNetworkingScript.InitializeCommunicationOverTCP();
			}
		}

		//ProcessReceivedTextData();
		ProcessReceivedMoodTrackerData();
	}

	public EmotionStruct getCurrentFacialEmotion()
	{
		return currentFacialEmotion;
	}

	public EmotionStruct getCurrentWordSentimentEmotion()
	{
		return currentWordSentimentEmotion;
	}

	public EmotionStruct getCurrentVocalEmotion()
	{
		return currentVocalEmotion;
	}

	public EmotionStruct convertToneAnalysisToEmotionStruct(ToneAnalysis toneValues)
	{
		// TODO: Implement the mapping properly
		EmotionStruct vocalEmotions = new EmotionStruct();
		if (toneValues.TemperGroup == "high") {
			vocalEmotions.anger += 33.33f;
		}
		else if (toneValues.TemperGroup == "med") {
			vocalEmotions.joy += 33.33f;
		}
		else if (toneValues.TemperGroup == "low") {
			vocalEmotions.sadness += 33.33f;
			vocalEmotions.fear += 33.33f;
		}
		if (toneValues.ValenceGroup == "positive") {
			vocalEmotions.joy += 33.33f;
		}
//		else if (toneValues.ValenceGroup == "neutral") {
//			
//		}
		else if (toneValues.ValenceGroup == "negative") {
			vocalEmotions.anger += 33.33f;
			vocalEmotions.sadness += 33.33f;
			vocalEmotions.fear += 33.33f;
		}
		if (toneValues.ArousalGroup == "high") {
			vocalEmotions.joy += 33.33f;
			vocalEmotions.anger += 33.33f;
		}
		else if (toneValues.ArousalGroup == "med") {
			vocalEmotions.fear += 33.33f;
		}
		else if (toneValues.ArousalGroup == "low") {
			vocalEmotions.sadness += 33.33f;
		}
		Debug.Log (vocalEmotions.anger);
		Debug.Log (vocalEmotions.joy);
		Debug.Log (vocalEmotions.sadness);
		Debug.Log (vocalEmotions.fear);

		return vocalEmotions;
	}

	// Returns the strongest emotion present in the current cumulative emotion
	public float getValueOfStrongestEmotion(EmotionStruct emotions)
	{
		if (emotions.joy > emotions.fear &&
			emotions.joy > emotions.disgust &&
			emotions.joy > emotions.sadness && 
			emotions.joy > emotions.anger && 
			emotions.joy > emotions.surprise &&
			emotions.joy > emotionThreshold)
		{
			return emotions.joy;
		}
		else if (emotions.fear > emotions.disgust &&
				 emotions.fear > emotions.sadness &&
				 emotions.fear > emotions.anger &&
				 emotions.fear > emotions.surprise &&
				 emotions.fear > emotionThreshold)
		{
			return emotions.fear;
		}
		else if (emotions.disgust > emotions.sadness &&
				 emotions.disgust > emotions.anger &&
				 emotions.disgust > emotions.surprise &&
				 emotions.disgust > emotionThreshold)
		{
			return emotions.disgust;
		}
		else if (emotions.sadness > emotions.anger &&
				 emotions.sadness > emotions.surprise &&
				 emotions.sadness > emotionThreshold)
		{
			return emotions.sadness;
		}
		else if (emotions.anger > emotions.surprise &&
				 emotions.anger > emotionThreshold)
		{
			return emotions.anger;
		}
		else if(emotions.surprise > emotionThreshold)
		{
			return emotions.surprise;
		}
		else
		{
			return 0.0f;
		}
	}

	// Returns a color to be used by the user interface based on the current synthesized emotion
	public Color calculateEmotionColor(EmotionStruct emotions)
	{
		if (emotions.joy > emotions.fear &&
			emotions.joy > emotions.disgust &&
			emotions.joy > emotions.sadness && 
			emotions.joy > emotions.anger && 
			emotions.joy > emotions.surprise &&
			emotions.joy > emotionThreshold)
		{
			return new Color(0.0f, 1.0f, 0.0f, 1.0f);		// green
		}
		else if (emotions.fear > emotions.disgust &&
				 emotions.fear > emotions.sadness &&
				 emotions.fear > emotions.anger &&
				 emotions.fear > emotions.surprise &&
				 emotions.fear > emotionThreshold)
		{
			return new Color(1.0f, 0.0f, 1.0f, 1.0f);		// magenta
		}
		else if (emotions.disgust > emotions.sadness &&
				 emotions.disgust > emotions.anger &&
				 emotions.disgust > emotions.surprise &&
				 emotions.disgust > emotionThreshold)
		{
			return new Color(1.0f, 1.0f, 0.0f, 1.0f);		// yellow
		}
		else if (emotions.sadness > emotions.anger &&
				 emotions.sadness > emotions.surprise &&
				 emotions.sadness > emotionThreshold)
		{
			return new Color(0.0f, 0.0f, 1.0f, 1.0f);		// blue
		}
		else if (emotions.anger > emotions.surprise &&
				 emotions.anger > emotionThreshold)
		{
			return new Color(1.0f, 0.0f, 0.0f, 1.0f);		// red
		}
		else if(emotions.surprise > emotionThreshold)
		{
			return new Color(1.0f, 1.0f, 1.0f, 1.0f);		// white
		}
		else
		{
			return new Color(0.0f, 0.0f, 0.0f, 1.0f);		// black
		}
	}

////////////////////////////////////// SET/CALCULATE MOOD TRACKER COORDINATES START /////////////////////////////////////////////////
//new
	public void PrepareToSend(byte[] newData){
		tcpNetworkingScript.UpdateDataToSend(newData);
	}

	void ProcessReceivedTextData()
	{
		byte[] dataToBeProcessed;
		if (!(tcpNetworkingScript.queueOfReceivedDataPackets.Count > 0)) {
			return;
		}

		dataToBeProcessed = tcpNetworkingScript.queueOfReceivedDataPackets.Dequeue();
		string dataText = System.Text.Encoding.UTF8.GetString(dataToBeProcessed);
		Debug.Log(dataText);
	}

	//Each individual packet is 4 bytes long
	//Do not change this
	int PacketDataSize = 4;
	private Vector3 moodTrackerCoordinates;
	void ProcessReceivedMoodTrackerData()
	{
		byte[] dataToBeProcessed = tcpNetworkingScript.latestByte;

		if (dataToBeProcessed.Length == 0)
			return;

		byte[] xPositionByte = new byte[PacketDataSize];
		byte[] yPositionByte = new byte[PacketDataSize];
		byte[] zPositionByte = new byte[PacketDataSize];

		byte[] emotionIndexByte = new byte[PacketDataSize];
		byte[] joyByte = new byte[PacketDataSize];
		byte[] fearByte = new byte[PacketDataSize];
		byte[] disgustByte = new byte[PacketDataSize];
		byte[] sadnessByte = new byte[PacketDataSize];
		byte[] angerByte = new byte[PacketDataSize];
		byte[] surpriseByte = new byte[PacketDataSize];

		//Array.Reverse(dataToBeProcessed);

		Buffer.BlockCopy(dataToBeProcessed, 0, xPositionByte, 0, PacketDataSize);
		Buffer.BlockCopy(dataToBeProcessed, PacketDataSize, yPositionByte, 0, PacketDataSize);
		Buffer.BlockCopy(dataToBeProcessed, 2*PacketDataSize, zPositionByte, 0, PacketDataSize);

		Buffer.BlockCopy(dataToBeProcessed, 3*PacketDataSize, emotionIndexByte, 0, PacketDataSize);
		Buffer.BlockCopy(dataToBeProcessed, 4*PacketDataSize, joyByte, 0, PacketDataSize);
		Buffer.BlockCopy(dataToBeProcessed, 5*PacketDataSize, fearByte, 0, PacketDataSize);
		Buffer.BlockCopy(dataToBeProcessed, 6*PacketDataSize, disgustByte, 0, PacketDataSize);
		Buffer.BlockCopy(dataToBeProcessed, 7*PacketDataSize, sadnessByte, 0, PacketDataSize);
		Buffer.BlockCopy(dataToBeProcessed, 8*PacketDataSize, angerByte, 0, PacketDataSize);
		Buffer.BlockCopy(dataToBeProcessed, 9*PacketDataSize, surpriseByte, 0, PacketDataSize);

		int xPosition = BitConverter.ToInt32(xPositionByte, 0);
		int yPosition = BitConverter.ToInt32(yPositionByte, 0);
		int zPosition = BitConverter.ToInt32(zPositionByte, 0);
		int emotionIndex = BitConverter.ToInt32(emotionIndexByte, 0);

		float[] emotionData = new float[6];
		emotionData [0] = BitConverter.ToSingle(joyByte, 0);
		emotionData [1] = BitConverter.ToSingle(fearByte, 0);
		emotionData [2] = BitConverter.ToSingle(disgustByte, 0);
		emotionData [3] = BitConverter.ToSingle(sadnessByte, 0);
		emotionData [4] = BitConverter.ToSingle(angerByte, 0);
		emotionData [5] = BitConverter.ToSingle(surpriseByte, 0);
		SetEmotionStructFromRecievedData (emotionData);


		moodTrackerCoordinates.x = xPosition / 100.0f;
		moodTrackerCoordinates.y = yPosition / 100.0f;
		moodTrackerCoordinates.z = zPosition / 100.0f;

		//SetMoodTrackerGeometry(moodTrackerCoordinates);
		SetMoodTrackerColor(emotionIndex);

//		string receivedMessage = "x = " + xPosition + " y = " + yPosition + " z = " + zPosition + " emotion = " + emotionIndex;
//		Debug.Log(receivedMessage);

//		receivedMessage = "joy = " + emotionData[0] + " fear = " + emotionData[1] + " disgust = " + emotionData[2];
//		Debug.Log(receivedMessage);

//		receivedMessage = "sadness = " + emotionData[3] + " anger = " + emotionData[4] + " surprise = " + emotionData[5];
//		Debug.Log(receivedMessage);
	}
		
	private void SetEmotionStructFromRecievedData(float[] emotionData){
		// Create a new facial emotion struct based on the data received from the Affectiva Server app
		EmotionStruct nextFacialEmotion = new EmotionStruct ();
		nextFacialEmotion.joy = emotionData[0];
		nextFacialEmotion.fear = emotionData[1];
		nextFacialEmotion.disgust = emotionData[2];
		nextFacialEmotion.sadness = emotionData[3];
		nextFacialEmotion.anger = emotionData[4];
		nextFacialEmotion.surprise = emotionData[5];
		Debug.Log ("Current Joy: " + nextFacialEmotion.joy + " | anger: " + nextFacialEmotion.anger + " | sadness: " + nextFacialEmotion.sadness + " | fear: " + nextFacialEmotion.fear);

		// Add in the next emotion captured by Affectiva
		if (facialEmotionWindow.Count == facialEmotionWindowSize - 1)
		{
			// add the tenth analysis to complete the window
			facialEmotionWindow.Add(nextFacialEmotion);

			// calculate the currentEmotions for this window and assign the parameter
			calculateCurrentFacialEmotion();

			// shift the window back to prepare for the next emotion data
			facialEmotionWindow.RemoveAt(0);
		}
		else
		{
			// add the next element
			facialEmotionWindow.Add (nextFacialEmotion);
		}
	}

	[HideInInspector] public Vector3 normalizedMoodTrackerCoordinates;
	[HideInInspector] public Vector3 moodTrackerSize;
	private Color moodTrackerColor;
	private int indexOfEmotion;
	public void SetMoodTrackerGeometry(Vector3 moodTrackerCoordinates){
		normalizedMoodTrackerCoordinates.x = moodTrackerCoordinates.x;
		normalizedMoodTrackerCoordinates.y = moodTrackerCoordinates.y;
		normalizedMoodTrackerCoordinates.z = moodTrackerCoordinates.z;

		int xPosition = (int) normalizedMoodTrackerCoordinates.x;
		int yPosition = (int) normalizedMoodTrackerCoordinates.y;
		int zPosition = (int) normalizedMoodTrackerCoordinates.z;

		string receivedMessage = "x = " + xPosition + " y = " + yPosition + "\nz = " + zPosition;
		displayExtraInfo.ClearAndSetDisplayText(receivedMessage);
	}

	public Color SetMoodTrackerColor(int emotionIndex){

		if (emotionIndex == 1)
			moodTrackerColor = Color.green; //joy
		else if (emotionIndex == 2)
			moodTrackerColor = Color.magenta; //fear
		else if (emotionIndex == 3)
			moodTrackerColor = Color.yellow; //disgust //excluded
		else if (emotionIndex == 4)
			moodTrackerColor = Color.blue; //sadness
		else if (emotionIndex == 5)
			moodTrackerColor = Color.red; //anger
		else if (emotionIndex == 6)
			moodTrackerColor = Color.white; //surprise //excluded
		else
			moodTrackerColor = Color.black; //neutral

		return moodTrackerColor;
	}

	public Color GetMoodTrackerColor(){
		return moodTrackerColor;
	}




/////////////////////////////////////// SET/CALCULATE MOOD TRACKER COORDINATES END //////////////////////////////////////////////////

	// Updates the currentFacialEmotion struct with an average of the values in the facialEmotionWindow list
	private void calculateCurrentFacialEmotion()
	{
		if (facialEmotionWindow.Count > 0)
		{
			EmotionStruct emotionSum = new EmotionStruct();
			foreach (EmotionStruct e in facialEmotionWindow)
			{
				emotionSum.anger += e.anger;
				emotionSum.joy += e.joy;
				emotionSum.fear += e.fear;
				emotionSum.sadness += e.sadness;
				emotionSum.disgust += e.disgust;
				emotionSum.surprise += e.surprise;
			}

			currentFacialEmotion.anger = emotionSum.anger / (float) facialEmotionWindow.Count;
			currentFacialEmotion.joy = emotionSum.joy / (float) facialEmotionWindow.Count;
			currentFacialEmotion.fear = emotionSum.fear / (float) facialEmotionWindow.Count;
			currentFacialEmotion.sadness = emotionSum.sadness / (float) facialEmotionWindow.Count;
			currentFacialEmotion.disgust = emotionSum.disgust / (float) facialEmotionWindow.Count;
			currentFacialEmotion.surprise = emotionSum.surprise / (float) facialEmotionWindow.Count;
		}
		else
		{
			currentFacialEmotion = new EmotionStruct();
		}
	}

}
