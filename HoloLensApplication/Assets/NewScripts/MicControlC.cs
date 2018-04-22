using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using SimpleJSON;
using UnityEngine.Networking;

public class MicControlC : MonoBehaviour {



	//the variables that do the users magic

	public string[] audBySec = new string[10];
	public string tokenUrl = "https://token.beyondverbal.com/token";
	private string[] apiKeyBucket = {"8f6d2151-d5b5-4928-bae7-a4febea3a4ea","412e16e9-e26c-4b9e-a018-3071ffbfad56","74b28335-c258-4cc9-98a3-2f02570a8827","22147938-29cc-4a2c-9720-2c4ddcb493e8","636ca4e3-d830-4f4d-9c30-30514817b0f0"};
	private string apiKey = "8f6d2151-d5b5-4928-bae7-a4febea3a4ea";
	public int bucketIdx = 0;
	private string startUrl = "https://apiv5.beyondverbal.com/v5/recording/";
	public string wavFile;
	public string analysisUrl;
	public string requestData;
	public string token;
	public string startResponseString;
	public string recordingId;
	public JSONNode currentAnalysis;
	public AudioClip audClip;
	public AudioClip audBuffer;
	private ToneAnalysis vocalToneResults = new ToneAnalysis ();
	private float timeIdx = 1.0f;

	public bool doNotDestroyOnLoad=false;

	public ToneAnalysis getVocalToneResults(){
		return vocalToneResults;
	}

	void Start () {
		Debug.Log (apiKey);
		requestData = "apiKey=" + apiKey + "&grant_type=client_credentials";

		token = authRequest(tokenUrl, Encoding.UTF8.GetBytes(requestData));
		Debug.LogWarning ("TOKEN: " + token);
		StartCoroutine(CreateWebRequest(startUrl + "start", Encoding.UTF8.GetBytes("{ dataFormat: { type: \"WAV\" } }"), token));
		StartCoroutine (yieldedStart());
	}

	private IEnumerator yieldedStart(){


		//wait till level is loaded
		yield return new WaitUntil(() => audClip != null);
		InvokeRepeating("RecordChunk", 1.0f, 1.0f);

		yield return 0;
		//make this controller persistent
		if(doNotDestroyOnLoad){
			DontDestroyOnLoad (transform.gameObject);
		}

	}

	void RecordChunk(){
		if (!audBuffer) {
			audBuffer = AudioClip.Create ("audioBuffer", audClip.samples*10, audClip.channels, audClip.frequency, false);
		}
		SaveWavFile (audClip);
		float[] samples = new float[audClip.samples * audClip.channels];
		audClip.GetData(samples, 0);
		audBuffer.SetData (samples, Mathf.RoundToInt((timeIdx-1.0f) * audClip.frequency));
		if (timeIdx < 10.0f) {
			timeIdx += 1.0f;
		} else {
			Analyze();
		}
	}

	void Analyze(){
		wavFile = SaveWavFile (audBuffer);
		analysisUrl = startUrl + recordingId;
		StartCoroutine (Analysis(analysisUrl, token));

		bucketIdx=(bucketIdx+1) % apiKeyBucket.Length;
		apiKey = apiKeyBucket[bucketIdx];
		requestData = "apiKey=" + apiKey + "&grant_type=client_credentials";

		//token = authRequestTest(tokenUrl, Encoding.UTF8.GetBytes(requestData));
		Debug.LogWarning(token);
		if (token == "") {
			return;
		} else {
			float[] samples = new float[audBuffer.samples * audBuffer.channels];
			audBuffer.GetData(samples, Mathf.RoundToInt((1.0f) * audClip.frequency));
			audBuffer.SetData (samples, 0);
		}
	}

	void Update () {

	}

	private static string authRequest(string url, byte[] data)
	{
		JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings() { Formatting = Formatting.Indented };

		UnityWebRequest request = UnityWebRequest.Put (url, data);
		request.method = UnityWebRequest.kHttpVerbPOST;
		request.SendWebRequest ();
		while (!request.isDone) {

		}
		var res = request.downloadHandler.text;
		var responseContent = JsonConvert.DeserializeObject<Dictionary<string, string>>(res, jsonSerializerSettings);
		return responseContent["access_token"];
	}

	private IEnumerator CreateWebRequest(string url, byte[] data, string token = null)
	{
		Debug.LogWarning ("START");
		UnityWebRequest request = UnityWebRequest.Put (url, data);
		request.method = UnityWebRequest.kHttpVerbPOST;
		request.SetRequestHeader("Authorization","Bearer "+token);
		yield return request.SendWebRequest ();
		Debug.LogWarning ("Got recordingid");
		var res = request.downloadHandler.text;
		Debug.LogWarning (res);
		var startResponseObj = JsonConvert.DeserializeObject<Dictionary<string, string>>(res);
		recordingId = startResponseObj["recordingId"];
	}

	private IEnumerator Analysis(string url, string token = null)
	{
		var data = File.ReadAllBytes(wavFile);
		UnityWebRequest request = UnityWebRequest.Put (url, data);
		request.method = UnityWebRequest.kHttpVerbPOST;
		request.SetRequestHeader("Authorization","Bearer "+token);
		yield return request.SendWebRequest();
		var res = request.downloadHandler.text;
		Debug.LogWarning (res);
		if (res == null) {
			yield return null;
		} else {
			if (res == "" || res == "Api key is missing") {
				yield return null;
			} else {
				currentAnalysis = JSON.Parse(res);
				vocalToneResults.TemperVal = Single.Parse(currentAnalysis["result"]["analysisSegments"][0]["analysis"]["Temper"]["Value"]);
				vocalToneResults.ArousalVal = Single.Parse(currentAnalysis["result"]["analysisSegments"][0]["analysis"]["Arousal"]["Value"]);
				vocalToneResults.ValenceVal = Single.Parse(currentAnalysis["result"]["analysisSegments"][0]["analysis"]["Valence"]["Value"]);
				vocalToneResults.TemperGroup = currentAnalysis["result"]["analysisSegments"][0]["analysis"]["Temper"]["Group"];
				vocalToneResults.ArousalGroup = currentAnalysis["result"]["analysisSegments"][0]["analysis"]["Arousal"]["Group"];
				vocalToneResults.ValenceGroup = currentAnalysis["result"]["analysisSegments"][0]["analysis"]["Valence"]["Group"];
			}
		}
		StartCoroutine(CreateWebRequest(startUrl + "start", Encoding.UTF8.GetBytes("{ dataFormat: { type: \"WAV\" } }"), token));

	}

	public string SaveWavFile (AudioClip aud)
	{
		string filepath;
		WavUtility.FromAudioClip(aud, out filepath, true);
		return filepath;
	}

	public void SetAudClip(AudioClip externClip){
		audClip = externClip;
	}



}