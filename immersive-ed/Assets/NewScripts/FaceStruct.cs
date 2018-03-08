using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceStruct  {
	public float[] leftEye;
	public float[] rightEye;
	public float interocularDistance; 

	public FaceStruct()
	{
		leftEye = new float[2];
		rightEye = new float[2];
		interocularDistance = 0;
	}
}
