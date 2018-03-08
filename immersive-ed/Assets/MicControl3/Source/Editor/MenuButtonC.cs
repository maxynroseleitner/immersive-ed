using UnityEngine;
using UnityEditor;
using System.Collections;
using UnityEngine.Audio;

public class MenuButtonC : MonoBehaviour {


	[MenuItem ("GameObject/Audio/MicControl 3",false,-16)]



	private static void  SpawnMicControllerCC (){

		GameObject MicControllerC= new GameObject("MicControllerC");
	

		MicControllerC.AddComponent<MicControlC>();

		if(MicControllerC.GetComponent<AudioSource>()==null){
			MicControllerC.AddComponent<AudioSource>();
		}

		AudioMixer mixer = Resources.Load("MicControl3Mixer") as AudioMixer;
		MicControllerC.GetComponent<AudioSource>().outputAudioMixerGroup = mixer.FindMatchingGroups("MicControlOutputVolume")[0] ;

		if(Camera.current){
			MicControllerC.transform.position=Camera.current.transform.position;
		}
		else{
			MicControllerC.transform.position = new Vector3(0,0,0);
		}


			Undo.RegisterCreatedObjectUndo (MicControllerC, "Create MicController");


	}


}