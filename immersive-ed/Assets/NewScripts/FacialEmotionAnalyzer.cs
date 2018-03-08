using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Affdex;

public class FacialEmotionAnalyzer : ImageResultsListener {

	public EmotionStruct currentEmotions;
    public FaceStruct currentFace;
    public Affdex.FeaturePoint[] featurePointsList;

    // "Coordinates" for health bar tracking
    public float a_x = 0.011845f;
    public float b_x = -6.53954f;
    public float a_y = -0.01195f;
    public float b_y = 5.65949f;

	// Use this for initialization
	void Start () {
		currentEmotions = new EmotionStruct();
        currentFace = new FaceStruct();
        a_x = -0.01195f;
        b_y = 5.64959f;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public EmotionStruct getCurrentEmotions()
	{
		Debug.Log("Got current emotions.");
		return currentEmotions;
	}

    public FaceStruct getCurrentFace()
    {
        return currentFace;
    }

	public override void onFaceFound(float timestamp, int faceId)
    {
        Debug.Log("Found the face");
    }

    public override void onFaceLost(float timestamp, int faceId)
    {
        Debug.Log("Lost the face");
    }

    public override void onImageResults(Dictionary<int, Face> faces)
    {
        Debug.Log("Got face results");

        foreach (KeyValuePair<int, Face> pair in faces)
        {
            int FaceId = pair.Key;  // The Face Unique Id.
            Face face = pair.Value;    // Instance of the face class containing emotions, and facial expression values.

            //Retrieve the Emotions Scores
            // face.Emotions.TryGetValue(Emotions.Contempt, out currentContempt);
            // face.Emotions.TryGetValue(Emotions.Valence, out currentValence);
			face.Emotions.TryGetValue(Emotions.Joy, out currentEmotions.joy);
            face.Emotions.TryGetValue(Emotions.Fear, out currentEmotions.fear);
            face.Emotions.TryGetValue(Emotions.Disgust, out currentEmotions.disgust);
            face.Emotions.TryGetValue(Emotions.Sadness, out currentEmotions.sadness);
            face.Emotions.TryGetValue(Emotions.Anger, out currentEmotions.anger);
            face.Emotions.TryGetValue(Emotions.Surprise, out currentEmotions.surprise);
            

            //Retrieve the Smile Score
            // face.Expressions.TryGetValue(Expressions.Smile, out currentSmile);


            //Retrieve the Interocular distance, the distance between two outer eye corners.
            // currentInterocularDistance = face.Measurements.interOcularDistance;


            //Retrieve the coordinates of the facial landmarks (face feature points)
            featurePointsList = face.FeaturePoints;
            float rightEyeX = featurePointsList[16].x;
            float rightEyeY = featurePointsList[16].y;
            currentFace.rightEye[0] = a_x * rightEyeX + b_x;
            currentFace.rightEye[1] = a_y * rightEyeY + b_y;
        }
    }
}
