using System;
using UnityEngine;

	public class ToneAnalysis
	{
		public float TemperVal;
		public float ArousalVal;
		public float ValenceVal;
		public string TemperGroup;
		public string ArousalGroup;
		public string ValenceGroup;
		public ToneAnalysis(){
			TemperVal = 0.0f;
			ArousalVal = 0.0f;
			ValenceVal = 0.0f;
			TemperGroup = "";
			ArousalGroup = "";
			ValenceGroup = "";
		}
	}

//using System;
//using UnityEngine;
//
//public class ToneAnalysis : MonoBehaviour
//{
//	public float TemperVal;
//	public float ArousalVal;
//	public float ValenceVal;
//	public string TemperGroup;
//	public string ArousalGroup;
//	public string ValenceGroup;
//	public ToneAnalysis(){
//		this.TemperVal = 0.0f;
//		this.ArousalVal = 0.0f;
//		this.ValenceVal = 0.0f;
//		this.TemperGroup = "";
//		this.ArousalGroup = "";
//		this.ValenceGroup = "";
//	}
//}
