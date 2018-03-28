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
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
//#if UNITY_EDITOR
//using System.IO;
//#endif

[RequireComponent (typeof (AudioSource))]

public class MicControlC : MonoBehaviour {


	//should this controller be in loudness or spectrum mode (simple or advanced). This is also used by the editor script to show the correct visuals.
	public bool enableSpectrumData=false;


	//if false the below will override and set the mic selected in the editor
	public bool useDefaultMic=false;

	public bool ShowDeviceName=true;
	public bool SetDeviceSlot=false;

	public int InputDevice=0;
	string selectedDevice;


	public bool advanced=false;
	public bool spectrumDropdown=false;

	public AudioSource audioSource;
	//The maximum amount of sample data that gets loaded in, best is to leave it on 256, unless you know what you are doing. A higher number gives more accuracy but 
	//lowers performance allot, it is best to leave it at 256.
	public int amountSamples=256;


	//the variables that do the users magic
	public float loudness;
	public float rawInput;
	public float[] spectrumData;
	public string[] audBySec = new string[10];
	//public AudioClip audData;
	public string tokenUrl = "https://token.beyondverbal.com/token";
	private string apiKey = "412e16e9-e26c-4b9e-a018-3071ffbfad56"; //"322360d1-236c-4902-bb9c-1ce56fb84578";
	public string startUrl = "https://apiv4.beyondverbal.com/v4/recording/";
	public string wavFile;
	public string analysisUrl;
	private float timeIdx = 1.0f;
	public string requestData;
	public string token;
	public string startResponseString;
	public string recordingId;
	public JSONNode currentAnalysis;
	public AudioClip audBuffer;
	private ToneAnalysis vocalToneResults = new ToneAnalysis ();

	//settings
	public float sensitivity=500.0f;
	public Vector2 minMaxSensitivity= new Vector2(0.0f,500.0f);

	public int bufferTime=11;

	public enum freqList {_44100HzCD,_48000HzDVD}
	public freqList freq;
	public int setFrequency=8000;


	public bool Mute=true;
	public bool debug=false;

	bool recording=true; 


	public bool focused=true;
	public bool Initialized=false;


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
		InvokeRepeating("RecordChunk", 1.0f, 1.0f);
		InvokeRepeating("Analyze", 10.0f, 1.0f);

	}

	private IEnumerator yieldedStart(){


		//wait till level is loaded
		if(LoadingLevel()){

			yield return 0;

		}

		//make this controller persistent
		if(doNotDestroyOnLoad){
			DontDestroyOnLoad (transform.gameObject);
		}

		yield return new WaitForSeconds(1);


		//return and throw error if no device is connected
		if(Microphone.devices.Length==0){
			Debug.LogError("No connected device detected! Connect at least one device.");
			Debug.LogError("No usable device detected! Try setting your device as the system's default.");
			//return;
		}

		Initialized=false;
		if(!Initialized){
			InitMic();
			//Initialized=true;
		}


	}



	void RecordChunk(){
		if (!audBuffer) {
			audBuffer = AudioClip.Create ("audioBuffer", audioSource.clip.samples*10, audioSource.clip.channels, audioSource.clip.frequency, false);
		}
		SaveWavFile (audioSource.clip);
		float[] samples = new float[audioSource.clip.samples * audioSource.clip.channels];
		audioSource.clip.GetData(samples, 0);
		audBuffer.SetData (samples, Mathf.RoundToInt((timeIdx-1.0f) * 8000));
		if (timeIdx < 10.0f){
			timeIdx += 1.0f;
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
				var analysisResponseString = CreateWebRequest(analysisUrl, bytes, token);
				//							Debug.Log(analysisResponseString);
				currentAnalysis = JSON.Parse(analysisResponseString);
				Debug.Log ("Post 10");
				startResponseString = CreateWebRequest(startUrl + "start", Encoding.UTF8.GetBytes("{ dataFormat: { type: \"WAV\" } }"), token);
				Debug.Log ("Listen 10");
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
				recordingId = startResponseObj["recordingId"];
								Debug.Log(vocalToneResults.TemperVal);
								Debug.Log(vocalToneResults.TemperGroup);
								Debug.Log(vocalToneResults.ArousalVal);
								Debug.Log(vocalToneResults.ArousalGroup);
								Debug.Log(vocalToneResults.ValenceVal);
								Debug.Log(vocalToneResults.ValenceGroup);

			}).Start();
		float[] samples = new float[audBuffer.samples * audBuffer.channels];
		audBuffer.GetData(samples, Mathf.RoundToInt((1.0f) * 8000));
		audBuffer.SetData (samples, 0);
	}




	//apply the mic input data stream to a float and or array;
	void Update () {

		//pause everything when not focused on the app and then re-initialize.


		if(focused && !LoadingLevel() ){
			if(!Initialized){
				InitMic();
				if(debug){
					Debug.Log("mic started: "+ selectedDevice);
				}
				//	Initialized=true;

			}
		}


		if (!focused && Initialized){
			StopMicrophone ();
			if(debug){
				Debug.Log("mic stopped");
			}
			return;
		}


		if (!Application.isPlaying && Initialized) {
			//stop the microphone if you are clicking inside the editor and the player is stopped
			StopMicrophone();
			if(debug){
				Debug.Log("mic stopped");
			}
			return;
		}

		if (LoadingLevel() && Initialized) {
			//stop the microphone if you are clicking inside the editor and the player is stopped
			StopMicrophone();
			if(debug){
				Debug.Log("mic stopped");
			}
			return;
		}


		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		if(focused && Initialized){
			//			Debug.Log ((Microphone.IsRecording (selectedDevice) && recording));
			//----------//----------//----------//----------//----------//----------//----------//----------//----------//----------
			if(Microphone.IsRecording(selectedDevice) && recording){

				//the simple strength to float method, used by most users. Outputs the mic's loudness into a single float
				loudness = GetDataStream()*sensitivity;
				rawInput = GetDataStream();

				//----------//----------//----------//----------//----------//----------//----------//----------//----------//----------//----------
				//the more advanced spectrum data analyses, for the advanced users. Outputs array of frequencies received from the microphone.

				//				if(enableSpectrumData){
				//					spectrumData=  GetSpectrumAnalysis();
				//				}

			}

			////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



			//Make sure the AudioSource volume is always 1
			audioSource.volume = 1;


		}

	}

	public static bool MyRemoteCertificateValidationCallback(System.Object sender,
		X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
	{
		bool isOk = true;
		// If there are errors in the certificate chain,
		// look at each error to determine the cause.
		if (sslPolicyErrors != SslPolicyErrors.None) {
			for (int i=0; i<chain.ChainStatus.Length; i++) {
				if (chain.ChainStatus[i].Status == X509ChainStatusFlags.RevocationStatusUnknown) {
					continue;
				}
				chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
				chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
				chain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan (0, 1, 0);
				chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllFlags;
				bool chainIsValid = chain.Build ((X509Certificate2)certificate);
				if (!chainIsValid) {
					isOk = false;
					break;
				}
			}
		}
		return isOk;
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

		ServicePointManager.ServerCertificateValidationCallback = MyRemoteCertificateValidationCallback;
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
		ServicePointManager.ServerCertificateValidationCallback = MyRemoteCertificateValidationCallback;
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



	public float GetDataStream(){

		if (Microphone.IsRecording (selectedDevice)) {

			float[] dataStream = new float[amountSamples];
			float audioValue = 0f;
			audioSource.GetOutputData (dataStream, 0);

			//add up all the outputdata
			for (int a = 0; a <= dataStream.Length - 1; a++) {
				audioValue = Mathf.Abs(audioValue+dataStream [a]);
			}

			//return the combined output data deviced by the sample amount to get the average loudness.
			return audioValue / amountSamples;
		}
		return 0;

	}




	public float[] GetSpectrumAnalysis(){

		float[] dataSpectrum = new float[amountSamples];
		audioSource.GetSpectrumData(dataSpectrum,0, FFTWindow.Rectangular);

		for(int i = 0; i<=dataSpectrum.Length-1;i++){

			dataSpectrum[i]= Mathf.Abs(dataSpectrum[i]*sensitivity);

		}

		return dataSpectrum;

	}
	public string SaveWavFile (AudioClip aud)
	{
		string filepath;
		WavUtility.FromAudioClip(aud, out filepath, true);
		return filepath;
	}


	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//Initialize microphone
	public void InitMic(){


		//select audio source
		if(!audioSource){
			audioSource = transform.GetComponent<AudioSource>();
		} 
		//	if(!audBuffer){
		//		audBuffer = transform.GetComponent<AudioSource>();
		//	}
		//only Initialize microphone if a device is detected
		if(Microphone.devices.Length>=0){

			int i=0;
			//count amount of devices connected
			foreach(string device in Microphone.devices){
				i++;
			}

			//set selected device from isnpector as device number. (to find the device).
			if(i>=1 && !useDefaultMic){
				selectedDevice= Microphone.devices[InputDevice];
			}

			//set the default device if enabled
			if(useDefaultMic){
				selectedDevice= Microphone.devices[0];
			}




			//Now that we know which device to listen to, lets set the frequency we want to record at

			if(freq== freqList._44100HzCD){
				setFrequency=8000;

			}

			if(freq == freqList._48000HzDVD){
				setFrequency=48000;

			}



			//detect the selected microphone one time to geth the first buffer.
			audioSource.clip =  Microphone.Start(selectedDevice, true, 1, setFrequency);


			//loop the playing of the recording so it will be realtime
			audioSource.loop = true;
			//if you only need the data stream values  check Mute, if you want to hear yourself ingame don't check Mute. 
			audioSource.mute = Mute;	



			AudioMixer mixer = Resources.Load("MicControl3Mixer") as AudioMixer;
			if(Mute){
				mixer.SetFloat("MicControl3Volume",-80);
			}
			else{
				mixer.SetFloat("MicControl3Volume",0);
			}





			//don't do anything until the microphone started up
			while (!(Microphone.GetPosition(selectedDevice) > 0)){
				if(debug){
					Debug.Log("Awaiting connection");
				}
			}
			if(debug){
				Debug.Log("Connected");
			}

			//Now that the basic initialisation is done, we are ready to start the microphone and gather data.
			StartCoroutine(StartMicrophone ());
			recording=true; 

		}

		Initialized=true;

	}
	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// 



	//for the above control the mic start or stop


	public IEnumerator StartMicrophone () {

		GetMicCaps ();

		audioSource.clip = Microphone.Start(selectedDevice, true, 1, setFrequency);//Starts recording

		while (!(Microphone.GetPosition(selectedDevice) > 0)){// Wait if a device is detected and  only then start the recording
			if(debug){
				Debug.Log("Waiting for connection:"+Time.deltaTime);
			}
			yield return 0;
		}
		if(true){		       
			Debug.Log("started"+ ", Freq (Hz): "+setFrequency+", samples: "+ amountSamples+", sensitivity: "+sensitivity); 
		}

		//	audioSource.Play(); // Play the audio source! 

		if(debug){		       
			Debug.Log("Receiving data"); 
		}
	}





	public void StopMicrophone () {
		audioSource.Stop();//Stops the audio
		Microphone.End(selectedDevice);//Stops the recording of the device  
		Initialized=false;
		recording=false; 

	}



	void GetMicCaps () {
		int minFreq;
		int maxFreq;

		Microphone.GetDeviceCaps(selectedDevice, out minFreq, out maxFreq);//Gets the frequency of the device
		//if the selected device has no frequency or is not sending data, the script wills top and throw out an error.
		if ((0 + maxFreq) == 0)
			Debug.LogError("No frequency detected on device: " +selectedDevice+ "... frequency= "+maxFreq);
		return;

	}



	#if!  UNITY_WEBGL
	#if!  UNITY_IOS
	#if !UNITY_ANDROID 
	//start or stop the script from running when the state is paused or not.
	void OnApplicationFocus(bool focus) {
		focused = focus;
	}

	void OnApplicationPause(bool focus) {
		focused = !focus;
	}

	void OnApplicationExit(bool focus) {
		focused = focus;
	}

	#endif
	#endif
	#endif


	public bool LoadingLevel(){

		if (SceneManager.GetActiveScene ().isLoaded) {
			return false;
		}
		return true;
	}











	#if UNITY_EDITOR

	//draw the gizmo
	void OnDrawGizmos () {

		Gizmos.DrawIcon (transform.position, "MicControlGizmo.tif", true);



		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//if gizmo folder does not exist create it
		if (!Directory.Exists(Application.dataPath+"/Gizmos")){
			Directory.CreateDirectory(Application.dataPath+"/Gizmos");
		}

		if(!File.Exists(Application.dataPath+"/Gizmos/MicControlGizmo.tif")){
			File.Copy(Application.dataPath+"/MicControl3/Resources/MicControlGizmo.tif",Application.dataPath+"/Gizmos/MicControlGizmo.tif");
		}
		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////	

	}


	#endif















}