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

//#if UNITY_EDITOR
//using System.IO;
//#endif


public class MicControlC : MonoBehaviour {



	//the variables that do the users magic

	public string[] audBySec = new string[10];
	public string tokenUrl = "https://token.beyondverbal.com/token";
	public string[] apiKeyBucket = {"322360d1-236c-4902-bb9c-1ce56fb84578","22147938-29cc-4a2c-9720-2c4ddcb493e8","8f6d2151-d5b5-4928-bae7-a4febea3a4ea","412e16e9-e26c-4b9e-a018-3071ffbfad56","74b28335-c258-4cc9-98a3-2f02570a8827","22147938-29cc-4a2c-9720-2c4ddcb493e8","636ca4e3-d830-4f4d-9c30-30514817b0f0"};
	public string apiKey = "322360d1-236c-4902-bb9c-1ce56fb84578";
	public int bucketIdx = 0;
	public string startUrl = "https://apiv4.beyondverbal.com/v4/recording/";
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
		requestData = "apiKey=" + apiKey + "&grant_type=client_credentials";

		token = authRequest(tokenUrl, Encoding.UTF8.GetBytes(requestData));
		//start
		//		Debug.Log(token);
		startResponseString = CreateWebRequest(startUrl + "start", Encoding.UTF8.GetBytes("{ dataFormat: { type: \"WAV\" } }"), token);
		//		Debug.Log (startResponseString);
		var startResponseObj = JsonConvert.DeserializeObject<Dictionary<string, string>>(startResponseString);
		if (startResponseObj["status"] != "success")
		{
			Debug.Log("Response Status: " + startResponseObj["status"]);
			return;
		}
		recordingId = startResponseObj["recordingId"];
		StartCoroutine (yieldedStart ());
	}

	private IEnumerator yieldedStart(){


		//wait till level is loaded
		yield return new WaitUntil(() => audClip != null);
		Debug.Log ("STARTING BEYOND VERBAL!!!");
		InvokeRepeating("RecordChunk", 1.0f, 1.0f);
//		InvokeRepeating("Analyze", 10.0f, 1.0f);
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
			Analyze ();
		}
	}

	void Analyze(){
		wavFile = SaveWavFile (audBuffer);
		analysisUrl = startUrl + recordingId;
		new Thread(() => 
			{
				Thread.CurrentThread.IsBackground = true; 
				/* run your code here */ 
				var bytes = File.ReadAllBytes(wavFile);
				Debug.Log ("POST 10");
				var analysisResponseString = CreateWebRequest(analysisUrl, bytes, token);
				Debug.Log ("Listen 10");
				currentAnalysis = JSON.Parse(analysisResponseString);
				startResponseString = CreateWebRequest(startUrl + "start", Encoding.UTF8.GetBytes("{ dataFormat: { type: \"WAV\" } }"), token);
				var startResponseObj = JsonConvert.DeserializeObject<Dictionary<string, string>>(startResponseString);
				if (startResponseObj["status"] != "success")
				{
					Debug.Log("Response Status: " + startResponseObj["status"]);
					return;
				}
				vocalToneResults.TemperVal = Single.Parse(currentAnalysis["result"]["analysisSegments"][0]["analysis"]["Temper"]["Value"]);
				vocalToneResults.ArousalVal = Single.Parse(currentAnalysis["result"]["analysisSegments"][0]["analysis"]["Arousal"]["Value"]);
				vocalToneResults.ValenceVal = Single.Parse(currentAnalysis["result"]["analysisSegments"][0]["analysis"]["Valence"]["Value"]);
				vocalToneResults.TemperGroup = currentAnalysis["result"]["analysisSegments"][0]["analysis"]["Temper"]["Group"];
				vocalToneResults.ArousalGroup = currentAnalysis["result"]["analysisSegments"][0]["analysis"]["Arousal"]["Group"];
				vocalToneResults.ValenceGroup = currentAnalysis["result"]["analysisSegments"][0]["analysis"]["Valence"]["Group"];
				bucketIdx=(bucketIdx+1) % apiKeyBucket.Length;
				apiKey = apiKeyBucket[bucketIdx];
				requestData = "apiKey=" + apiKey + "&grant_type=client_credentials";

				token = authRequest(tokenUrl, Encoding.UTF8.GetBytes(requestData));
				//start
				//		Debug.Log(token);
				startResponseString = CreateWebRequest(startUrl + "start", Encoding.UTF8.GetBytes("{ dataFormat: { type: \"WAV\" } }"), token);
				//		Debug.Log (startResponseString);
				startResponseObj = JsonConvert.DeserializeObject<Dictionary<string, string>>(startResponseString);
				if (startResponseObj["status"] != "success")
				{
					Debug.Log("Response Status: " + startResponseObj["status"]);
					return;
				}
				recordingId = startResponseObj["recordingId"];
				Debug.Log(vocalToneResults.TemperVal);
				Debug.Log(vocalToneResults.TemperGroup);
				Debug.Log(vocalToneResults.ArousalVal);
				Debug.Log(vocalToneResults.ArousalGroup);
				Debug.Log(vocalToneResults.ValenceVal);
				Debug.Log(vocalToneResults.ValenceGroup);

			}).Start();
		float[] samples = new float[audBuffer.samples * audBuffer.channels];
		audBuffer.GetData(samples, Mathf.RoundToInt((1.0f) * audClip.frequency));
		audBuffer.SetData (samples, 0);
	}




	//apply the mic input data stream to a float and or array;
	void Update () {

	}


	private static string authRequest(string url, byte[] data)
	{
		JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings() { Formatting = Formatting.Indented };
		HttpWebRequest request = HttpWebRequest.Create(url) as HttpWebRequest;
		request.Method = "POST";
		request.ContentType = "application/x-www-form-urlencoded";
		//		request.ServicePoint.SetTcpKeepAlive(false, 0, 0);
		request.ServicePoint.UseNagleAlgorithm = false;
		request.ReadWriteTimeout = 1000000;
		request.Timeout = 10000000;
		request.SendChunked = false;
		request.AllowWriteStreamBuffering = true;
		//		request.AllowReadStreamBuffering = false;
		request.KeepAlive = true;

		using (var requestStream = request.GetRequestStream())
		{
			requestStream.Write(data, 0, data.Length);
		}

		using (var response = request.GetResponse() as HttpWebResponse)
		using (var responseStream = response.GetResponseStream())
		using (var streamReader = new StreamReader(responseStream, Encoding.UTF8))
		{
			var res = streamReader.ReadToEnd();
			var responceContent = JsonConvert.DeserializeObject<Dictionary<string, string>>(res, jsonSerializerSettings);
			return responceContent["access_token"];

		}
	}

	private static string CreateWebRequest(string url, byte[] data, string token = null)
	{
		//		JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings() { Formatting = Formatting.Indented };
		HttpWebRequest request = HttpWebRequest.Create(url) as HttpWebRequest;
		request.Method = "POST";
		request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

		request.KeepAlive = true;
		//		request.ServicePoint.SetTcpKeepAlive(true, 10000, 10000);

		request.Timeout = 10000000;
		request.SendChunked = true;
		request.AllowWriteStreamBuffering = true;
		//		request.AllowReadStreamBuffering = false;

		if (string.IsNullOrEmpty(token) == false)
			request.Headers.Add("Authorization", "Bearer " + token);

		using (var requestStream = request.GetRequestStream())
		{
			requestStream.Write(data, 0, data.Length);
		}

		using (var response = request.GetResponse() as HttpWebResponse)
		using (var responseStream = response.GetResponseStream())
		using (var streamReader = new StreamReader(responseStream, Encoding.UTF8))
		{
			return streamReader.ReadToEnd();
		}
	}
		
	public string SaveWavFile (AudioClip aud)
	{
		string filepath;
		WavUtility.FromAudioClip(aud, out filepath, true);
		return filepath;
	}


	public void SetAudClip(AudioClip externClip){
		Debug.Log("SET CLIP");
		audClip = externClip;
		Debug.Log (audClip);
	}
		


}