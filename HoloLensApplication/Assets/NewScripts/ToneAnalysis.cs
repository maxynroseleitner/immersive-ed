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
