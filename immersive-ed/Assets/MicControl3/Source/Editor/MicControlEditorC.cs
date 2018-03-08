using UnityEditor;
using UnityEngine;
using System.Collections;




[CustomEditor (typeof(MicControlC))] 
public class MicControlEditorC : Editor{


	Texture logoInspector;
	void LoadTex()	{
		if (logoInspector == null) {
			logoInspector = Resources.Load ("MicControlGizmo", typeof(Texture)) as Texture;

		}
	}


	/////////////////////////////////////////////////////////////////////////////////////////////////		
	public override void  OnInspectorGUI ()	{
		//draw logo
		if (logoInspector == null) {
			LoadTex ();
		}
		EditorGUILayout.BeginHorizontal ();
		GUILayout.Label ("");
		GUILayout.Box (logoInspector, GUILayout.Width(400), GUILayout.Height(100));
		GUILayout.Label ("");
		EditorGUILayout.EndHorizontal ();


		//draw inspector.


		GameObject  ListenToMic = Selection.activeGameObject;
		float  micInputValue = ListenToMic.GetComponent<MicControlC> ().loudness;

		ProgressBar (micInputValue, "Loudness: " + ListenToMic.GetComponent<MicControlC> ().loudness, 18, 18);

		//this button copy's the basic code to call the value, for quick acces.
		//use horizontal mapping incase we add more menu buttons later.
		EditorGUILayout.BeginHorizontal ();


		//help button redirects to website
		if (GUILayout.Button (new GUIContent ("Help", "Need help? Check the FAQ or fill in a contact form."))) {
			Application.OpenURL ("http://markduisters.com/asset-portfolio/");       
		}
		//contact button redirects to the contact form on the website
		if (GUILayout.Button (new GUIContent ("Contact", "Have a question or found a bug? Let me know!"))) {
			Application.OpenURL ("http://markduisters.com/contact/");       
		}

		EditorGUILayout.EndHorizontal ();

		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//show the selected device and create a device selection drop down.


		//this visually shows if the micrpohone is active or not
		EditorGUILayout.BeginHorizontal ();

		try {
			//if the application is focused (ingame) show a green circ
			if (ListenToMic.GetComponent<MicControlC> ().focused) {
				GUI.color = Color.green;
				GUILayout.Box ("        ");
				GUI.color = Color.white;        	
			} else {
				GUI.color = Color.red;
				GUILayout.Box ("        ");
				GUI.color = Color.white;
			}

			//show selected device
			GUILayout.Label (Microphone.devices [ListenToMic.GetComponent<MicControlC> ().InputDevice]);
		} catch (System.Exception e) {
			Debug.LogError (e);
		}

		EditorGUILayout.EndHorizontal ();

		//Redirect ShowDeviceName ingame
		//count devices
		int count = 0;
		foreach (string device in Microphone.devices) {
			count++;
		}

		//toggle if a default mic should be used
		ListenToMic.GetComponent<MicControlC> ().useDefaultMic = GUILayout.Toggle ( ListenToMic.GetComponent<MicControlC>().useDefaultMic , new  GUIContent ("Use default device microphone", "When enabled the controller will always grab the default microphone of the device where the application is run from.(mobile mic, pc mic that is currently set as default,..."), GUILayout.Width (200));

		if (!ListenToMic.GetComponent<MicControlC> ().useDefaultMic) {

			// instead of using the buttons or default setting to select. Users can manually input a slot number (for android, etc..)
			ListenToMic.GetComponent<MicControlC> ().SetDeviceSlot = GUILayout.Toggle ( ListenToMic.GetComponent<MicControlC> ().SetDeviceSlot, new GUIContent ("Set device slot", "Handy for building to mobile platyforms that use another microphone other than the internal one. If you know the device location/number position, you can type it here. Keep in mind that although you can set not connected slots now.Your microphone input will not work in the editor. A good rule of thumb to follow is, to always develop in the editor with your workstation default microphone and only at build set the slot to what Android would be using. If your android device only uses the internal microphone, simply select use default device and you are good to go."), GUILayout.Width (200));
			if (!ListenToMic.GetComponent<MicControlC> ().SetDeviceSlot) {

				ListenToMic.GetComponent<MicControlC> ().ShowDeviceName = EditorGUILayout.Foldout (  ListenToMic.GetComponent<MicControlC> ().ShowDeviceName, new GUIContent ("Detected devices: " + count, "Show a list of all detected devices (1 is showed as default device in the drop down menu)"));
				if (ListenToMic.GetComponent<MicControlC> ().ShowDeviceName) {

					if (Microphone.devices.Length >= 0) {

						int i = 0;
						//count amount of devices connected
						foreach (string device in Microphone.devices) {
							if (device == null) {
								Debug.LogError ("No usable device detected! Try setting your device as the system's default. Or set a slot manually");
								return;
							}
							i++;

							GUILayout.BeginVertical ();

							//if selected slot is not equal to number count, make button grey.
							if (ListenToMic.GetComponent<MicControlC> ().InputDevice != i - 1) {
								GUI.color = Color.grey;
							}

							//create a selection button
							if (GUILayout.Button (device)) {
								ListenToMic.GetComponent<MicControlC> ().InputDevice = i - 1;
							}

							GUI.color = Color.white;

							GUILayout.EndVertical ();
						}

					}

					//throw error when no device is found.
					else {
						Debug.LogError ("No connected device detected! Connect at least one device.");	
						return;
					}
				}
			} else {
				ListenToMic.GetComponent<MicControlC> ().InputDevice = EditorGUILayout.IntField ("Slot number =",  ListenToMic.GetComponent<MicControlC> ().InputDevice);

			}

		}


		if (!ListenToMic.GetComponent<MicControlC> ().focused) {
			GUILayout.Label ("");
			GUILayout.Label ("The microphone will only send data when the game window is active!");
			GUILayout.Label ("");
		}

		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		//show advanced variables
		ListenToMic.GetComponent<MicControlC> ().advanced = EditorGUILayout.Foldout (ListenToMic.GetComponent<MicControlC> ().advanced, new GUIContent ("Advanced settings", "Reveal all tweakable variables"));

		if (ListenToMic.GetComponent<MicControlC> ().advanced) {

			GUILayout.Label ("");

			EditorGUILayout.BeginHorizontal ();
			//	GUILayout.Label("");

			//Redirect debug ingame
			ListenToMic.GetComponent<MicControlC> ().debug = GUILayout.Toggle (ListenToMic.GetComponent<MicControlC> ().debug, new GUIContent ("Debug", "This will show the connection progress in the console."), GUILayout.Width (60));

			//keep this controller persistend between scenes
			ListenToMic.GetComponent<MicControlC> ().doNotDestroyOnLoad = GUILayout.Toggle (ListenToMic.GetComponent<MicControlC> ().doNotDestroyOnLoad, new GUIContent ("Don't destroy on load", "If selected, this controller will be persistend when switching scenes during runtime"), GUILayout.Width (150));

			//Redirect Mute ingame
			ListenToMic.GetComponent<MicControlC> ().Mute = GUILayout.Toggle (ListenToMic.GetComponent<MicControlC> ().Mute, new GUIContent ("Mute", "Leave enabled when you only need the input value of your device. When dissabled you can listen to the playback of the device"), GUILayout.Width (60));


			EditorGUILayout.EndHorizontal ();

			//enable or disable spectrum data analisys
			ListenToMic.GetComponent<MicControlC> ().enableSpectrumData = GUILayout.Toggle (ListenToMic.GetComponent<MicControlC> ().enableSpectrumData, new GUIContent ("enable spectrum data analysis", "When enabled users will have acces to the full frequency spectrum output by MicControl2. This is the big brother of the 'loudness' variable, instead of having a single float to represent the microphone's loudness, a full float array is filled with the frequency spectrum data. "), GUILayout.Width (500));

			GUILayout.Label ("");

			//ListenToMic.GetComponent<MicControlC>().maxFreq = EditorGUILayout.FloatField(new GUIContent ("Frequency (Hz)","Set the quality of the received data: It is recommended but not required, to match this to your selected microphone's frequency."), ListenToMic.GetComponent<MicControlC>().maxFreq);
			ListenToMic.GetComponent<MicControlC> ().freq = (MicControlC.freqList) EditorGUILayout.EnumPopup (new GUIContent ("Frequency (Hz)", "Select the quality of the received data: It is recommended but not required, to match this to your selected microphone's frequency."),  ListenToMic.GetComponent<MicControlC> ().freq);


			ListenToMic.GetComponent<MicControlC> ().bufferTime = EditorGUILayout.IntField (new GUIContent ("RAM buffer time", "How many seconds of audio should be loaded into RAM. This will then be filled up with the Sample amount."), ListenToMic.GetComponent<MicControlC> ().bufferTime);
			//always have at least 1 second of audio to fill the ram.
			if (ListenToMic.GetComponent<MicControlC> ().bufferTime < 1) {
				ListenToMic.GetComponent<MicControlC> ().bufferTime = 1;
			}


			ListenToMic.GetComponent<MicControlC> ().amountSamples = EditorGUILayout.IntSlider (new GUIContent ("Sample amount", "This is basically how much the buffer gets filled (per frame) with samples and determines the precission of the loudness variable, if you are not sure, leave this between 256 and 1024 as it gives more than enough precision for basic tasks/scripts. Higher samples = more precision/quality of the loudness value and smoother results. However for spectrumData this is different. As spectrumData gives you direct acces to this sample buffer. This means that the bigger the sample buffer, the more data you have acces to. "), ListenToMic.GetComponent<MicControlC> ().amountSamples, 256, 8192);
			//lock to increments
			int tempSamples = ListenToMic.GetComponent<MicControlC> ().amountSamples;
			if (tempSamples >= 256 && tempSamples <= 512) {
				ListenToMic.GetComponent<MicControlC> ().amountSamples = 256;
			}

			if (tempSamples >= 512 && tempSamples <= 1024) {
				ListenToMic.GetComponent<MicControlC> ().amountSamples = 512;
			}

			if (tempSamples >= 1024 && tempSamples <= 2048) {
				ListenToMic.GetComponent<MicControlC> ().amountSamples = 1024;
			}

			if (tempSamples >= 2048 && tempSamples <= 4096) {
				ListenToMic.GetComponent<MicControlC> ().amountSamples = 2048;
			}

			if (tempSamples >= 4096 && tempSamples <= 8192) {
				ListenToMic.GetComponent<MicControlC> ().amountSamples = 4096;
			}

			if (tempSamples >= 8192) {
				ListenToMic.GetComponent<MicControlC> ().amountSamples = 8192;
			}

			ListenToMic.GetComponent<MicControlC> ().sensitivity = EditorGUILayout.Slider (new GUIContent ("Sensitivity", "Set the sensitivity of your input: The higher the number, the more sensitive (higher) the -loudness- value will be"), ListenToMic.GetComponent<MicControlC> ().sensitivity, ListenToMic.GetComponent<MicControlC> ().minMaxSensitivity.x, ListenToMic.GetComponent<MicControlC> ().minMaxSensitivity.y);
			EditorGUILayout.MinMaxSlider (new GUIContent ("Sensitivity range", "Helps you tweak the sensitivity"),ref ListenToMic.GetComponent<MicControlC> ().minMaxSensitivity.x, ref ListenToMic.GetComponent<MicControlC> ().minMaxSensitivity.y, 0.0f, 1000.0f);

			EditorGUILayout.BeginHorizontal ();
			GUILayout.Label ("");
			GUILayout.Label ("min: " + ListenToMic.GetComponent<MicControlC> ().minMaxSensitivity.x, GUILayout.Width (100));
			GUILayout.Label ("max: " + ListenToMic.GetComponent<MicControlC> ().minMaxSensitivity.y, GUILayout.Width (100));
			EditorGUILayout.EndHorizontal ();


			//show loudness progress bars
			GUILayout.Label ("Raw");
			ProgressBar (ListenToMic.GetComponent<MicControlC> ().rawInput, "rawInput", 18, 7);
			GUILayout.Label ("Loudness");
			ProgressBar (micInputValue, "loudness", 18, 7);



			//show spectrum data progress bars
			if (ListenToMic.GetComponent<MicControlC> ().enableSpectrumData) {

				float[] micSpectrum = ListenToMic.GetComponent<MicControlC> ().spectrumData;
				ListenToMic.GetComponent<MicControlC> ().spectrumDropdown = EditorGUILayout.Foldout ( ListenToMic.GetComponent<MicControlC> ().spectrumDropdown, new GUIContent ("Spectrum data", "Reveal the complete frequency spectrum"));

				if (ListenToMic.GetComponent<MicControlC> ().spectrumDropdown) {
					for (int s = 0; s <= micSpectrum.Length - 1; s++) {

						EditorGUILayout.BeginHorizontal ();
						GUILayout.Label ("spectrumData[" + s + "]");
						ProgressBar (micSpectrum [s], "spectrumData: " + s, 18, 7);
						EditorGUILayout.EndHorizontal ();

					}
				}
			}



		}


		//EditorUtility.SetDirty(target);
		//this.Repaint ();

		// Show default inspector property editor
		//	DrawDefaultInspector ();
	}



	// Custom GUILayout progress bar.
	void ProgressBar (float value, string laber, int scaleX, int scaleY){

		// Get a rect for the progress bar using the same margins as a textfield:
		Rect rect = GUILayoutUtility.GetRect (scaleX, scaleY, "TextField");
		EditorGUI.ProgressBar (rect, value, "");


		EditorGUILayout.Space ();
	}







}
