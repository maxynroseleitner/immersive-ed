using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

public class UI_devices_dropdown_list : MonoBehaviour {

	//Place this script on a dropdown menu in the Canvas
	//The script will place every detected device as an item in the list.
	//Then we will set the selected device as an input in MicControl 3.
	//These variables are used by MicControl 3 " public int InputDevice, String selectedDevice;"
	//InputDevice is the position the devices has on your system's device array and MicControl3 will use that position to read out the name in selectedDevice.
	//selectedDevice is then used by MicControl to setup unity's microphone class to call the proper device.

	//In this script we will only be setting the InputDevice variable.

	//One more thing to know: the device on Position 0 is always the system's default device. So the first dropdown item we will always see, will be the default device.

	//in here we place the controller that we want to call and control.
	public MicControlC getController;

	//here we store the detected devices
	List <string> getDeviceList= new List <string>();



	void Start () {

		//clear the previous list
		transform.GetComponent<Dropdown> ().ClearOptions();


		//loop through all detected devices on the system and add them in the list
		foreach(string device in Microphone.devices){
				getDeviceList.Add (device);

		}


		//place the filled list as clickable options.
		transform.GetComponent<Dropdown> ().AddOptions ( getDeviceList);
			
	}



	//now that we have a drop down list, we can use Dropdown.value to set the MicController's InputDeivce variable.

	void Update(){

		//to prevent setting it every frame, we only update the value when the selection has changed in the dropdown menu.

		if (transform.GetComponent<Dropdown> ().value != getController.InputDevice) {

			//before we do anything to the device, we must first stop the current connection
			getController.StopMicrophone();

			//change the values you want to change here. In this case we update the input device.
			getController.InputDevice = transform.GetComponent<Dropdown> ().value;

			//next we re-initialise and start the new connected device.
			getController.InitMic();

			//dubbel check in your controller's inspecter during runtime wether or not the new connection was a succes.
		}

	}


}
