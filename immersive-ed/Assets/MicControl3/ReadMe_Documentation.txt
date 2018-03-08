---------------------------------------------------------
---MicControl 3 (C#), created by Mark Duisters.----------
---questions: http://markduisters.com/contact/-----------
---Help: http://markduisters.com/asset-portfolio/-------
---------------------------------------------------------
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//Short legal notice:
You are free to modify this system, add or remove code and tweak the system to your will. As long as you hold a license and follow Unity's EULA.
https://unity3d.com/legal/as_terms
In short: The asset can only be sold by the user as an integrated part of the project build.
It is not allowed to sell the asset on its own or show partial/complete parts of the source code on public platforms (github, etc...) where it can be accessed by non licensed users.
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


'Quick start guide:

'- To place a controller in the scene go to: GameObjects->Audio->MicControl 3. This will place an independant controller in your scene.

'- You can also drag the prefab from the MicControl3 root folder in your scene. Keep in mind that if you drag multiple prefabs in one scene, the settings will be shared.





Chapter 1: 	'Important User variables. This chapter contains all values users can utilize in their code.

Chapter 2:	Background variables. This chapter contains all the other values used by the system. Although users can access and modify these variables, it is mostly handled by the system itself.
			The only reason to access these, would be if you want to custom set settings outside of the controller inspector.

Chapter 3: 	'Function and method listing. This chapter contains detailed explanation of MicControl 3 its functions and methods. Although the user does not need to know these in order
		  	'to work with the system, the information can be handy if one wants to modify the system.






_____________________________________

CHAPTER 1: Important User variables.
_____________________________________


//the variables that do the users magic
public float loudness; 	'Present since MicControl v1.0, the loudness value is the most used variable and one of the key features of MicControl. 
						'It is a float representation of the loudness detected of the selected device.

public float rawInput;	'This is the unmodified version of the loudness value(basically the loudness value before sensitivity is applied).

public float[] spectrumData; 	'The bigger brother of the loudness value, spectrumData. Instead of getting a simple single float, a array is filled completely with each frequency detected.
								'The array sized is determined by the controller its sample amount. E.g.: sample amount = 1024, a total of 1024 different frequencies will be store in the array.
								'The user can call to each frequency separately by filling the position number between the [x] brackets. The inspector shows full position information for each 
								'frequency value. (Important note is that the sample size can have a performance impact. See chapter 2).


public bool focused;		'This value is normally controlled by the System automatically. However for mobile builds it needs to be set manually through script. 
							'The focus bool tells the system whether your game window is active or not. If false, the system will stop reading data from the microphone.
							'This is also active inside the editor. You must select your play window in order to receive data!

public bool Initialized;		'The initialized bool is handled automatically by the system and is used by users to check if the controller is 
								'ready doing its basic setup and has started reading data. This value is very important when using spectrumData as you only want your code to read the array when
								'it is actually filled with data. Normally one should treat this value as READ ONLY, however, if you have stopped the microphone manually with StopMicrophone (), this value will be
								'automatically set to false.  (see chapter 3).









_____________________________________

CHAPTER 2: Background variables.
_____________________________________



Most of these variables are just setting pointers for the system's editor script, which then uses these settings to properly setup your controller.
They are of little use to the user. However should one want to write an ingame menu for
adjusting the complete controller. Then these are the variables needed for initialization, listing and interpreting.

public bool enableSpectrumData;//should this controller be in loudness or spectrum mode (simple or advanced). This is also used by the editor script to show the correct visuals.



public bool useDefaultMic;//if false the below will override and set the mic selected in the editor


public bool ShowDeviceName;//used by the editor to draw the devices in the inspector.
public bool SetDeviceSlot;//used by the editor to know when a slot should be set.

public int InputDevice;//This int value determines what device we are listening to. It directly correlates to the device position on our machine (0 = default device).
string selectedDevice;//When we know the above device position, its full name is stored in this value. We use this value to reference the device for processing.


public bool advanced;//Another pointer for the editor.
public bool spectrumDropdown;//Another pointer for the editor.Controls whether or not we see the list of the frequency spectrumdata array.

public AudioSource audioSource;//Container for the AudioSource. The AudioSource is created automatically by the system. It is used to store our audio data for processing.

public int amountSamples;	//The maximum amount of sample data that gets loaded in, best is to leave it on 256, unless you know what you are doing. A higher number gives more accuracy but 
						 	//lowers performance allot, it is best to leave it at 1024. This value influences the spectrumData array size (see chapter 1).



//settings
public float sensitivity=1.0f;//This value influences the rawInput to become the loudness variable. It can be set through script. However the system must first be stopped and re-initialized.
public Vector2 minMaxSensitivity= new Vector2(0.0f,500.0f);//editor pointer. Only used by the editor for slider control.

public int bufferTime=1;//In seconds, how much audio should we load into an audio clip and thus into the system's ram. 1 second should always be more than enough for the
						//average real-time input stream usage.

public enum freqList {_44100HzCD,_48000HzDVD}//For editor use only. Creates two items we can use to preset our frequency.
public freqList freq;//Container of above set items.
public int setFrequency=44100;//The editor controls this value based on above items. However should one want to have this option in their own interface. The above can be ignored and the frequency
						 //can be manually set in script (don't forget to stop the device first and re-initialize it).

public bool Mute;//Controlled by the editor, but can also be controlled through script. Sets playback on or off (controlled through provided AudioMixer).
				 //Always mute through the provided AudioMixer, never mute the AudioSource directly. As of Unity 5.2 muting the AudioSource directly stops all audio data.

public bool debug;//Show debug information or not.

bool recording;//Becomes true when the device is done Initializing and starts recording. Can be used as an extra check in script. However calling Initialized to check if a device should be sufficient.


public bool doNotDestroyOnLoad;	//Enable this if your project has more than one scene that uses the exact same controller. Keep in mind that you must account a search for this controller in your scripts
								//that are outside of the first start scene.



_____________________________________

CHAPTER 3: Functions and methods.
_____________________________________


public void InitMic():	'Used by the system to initialize the selected device with the set controller settings.
						'Calling this function from an external script will re-initialize the device with the new set settings. Do not forget to first stop the device before calling this method.

public IEnumerator StartMicrophone():	'Called at the end of InitMic() to fully start the device and receive data. This can be called manually, however this is not a good idea as your device still has to be initialized.
										'When changing settings through script, it is best to always call InitMic() instead, as this will automatically end up calling StartMicrophone() anyway.

public void StopMicrophone (): 'Stops the data stream from your device and stops reading from the AudioSource. It also resets Initialized and recording to false. Meaning you will have to call InitMic() to restart your device.


//Below is the "meat" of MicControl
public float GetDataStream(): 'Collects and processes device data to convert it to the loudness float value.
public float[] GetSpectrumAnalysis(): 'Collects and processes device frequencies to convert them to the spectrumData float array.


BELOW FUNCTIONS ARE NOT USED ON ANDROID AND IOS PLATFORMS. In case of android platforms you must set the focused bool manually

	void OnApplicationFocus(bool focus)://Start or stop the system from running when the application is no longer the active window. This is to prevent input delay.

	void OnApplicationPause(bool focus)://Start or stop the system from running when the application is paused. This is to prevent input delay.

	void OnApplicationExit(bool focus)://Start or stop the system from running when the application is shutdown. This is to prevent null pointer behavior.


	public bool LoadingLevel(): 'Detects if a scene is loading or not. Returns true if scene is loading, false if scene is loaded. Used by the system to let the device wait properly and prevent lag.
								'Can also be called by users to see if a scene is done.


				
