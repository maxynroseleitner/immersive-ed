/**
* Copyright 2015 IBM Corp. All Rights Reserved.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
*      http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*
*/

using UnityEngine;
using System.Collections;
using IBM.Watson.DeveloperCloud.Logging;
using IBM.Watson.DeveloperCloud.Services.SpeechToText.v1;
using IBM.Watson.DeveloperCloud.Utilities;
using IBM.Watson.DeveloperCloud.DataTypes;
using System.Collections.Generic;
using UnityEngine.UI;
using IBM.Watson.DeveloperCloud.Services.NaturalLanguageUnderstanding.v1;
using IBM.Watson.DeveloperCloud.Connection;
using System;
using SimpleJSON;
public class SentimentAnalyzer : MonoBehaviour
{
    // Original Watson speech to text credentials
    // private string _username = "bd7c3afe-72db-467d-b029-399ea4e2d0d4";
    // private string _password = "oaQ8pVOIG2oq";

    // Second Watson set of credentials
    // private string _username = "55095a1d-71db-4b7b-9007-2de45bbfc8ef";
    // private string _password = "4AydC5ntEOUs";

    private string _username = "91962287-1aa6-4dbf-a193-6f4e9e95d34e";
    private string _password = "6YMPWyz5SUBd";
    private string _url = "https://stream.watsonplatform.net/speech-to-text/api";

    public Text ResultsField;
    private int counter = 0;
    private int _recordingRoutine = 0;
    private string _microphoneID = null;
    private AudioClip _recording = null;
    private int _recordingBufferSize = 1;
    private int _recordingHZ = 16000;
    public Text ResponseField;
    private NaturalLanguageUnderstanding _nlu;
    //private string _analysisModel = "en-es";
    private bool _getModelsTested = false;
    private bool _analyzeTested = false;
    private SpeechToText _speechToText;
    ArrayList al = new ArrayList();
    ArrayList bufferfor10 = new ArrayList();
    string recordData=""; 
    private IEnumerator coroutine;

    public EmotionStruct currentEmotions;

	public EmotionStruct getCurrentEmotions()
	{
		Debug.Log("Got current emotions from sentiment analyzer.");
        Debug.Log("Joy: " + currentEmotions.joy);
        Debug.Log("anger: " + currentEmotions.anger);
        Debug.Log("fear: " + currentEmotions.fear);
        Debug.Log("disgust: " + currentEmotions.disgust);
        Debug.Log("sadness: " + currentEmotions.sadness);
		return currentEmotions;
	}

    void Start()
    {
        // Initialize the current emotions
        currentEmotions = new EmotionStruct();

        LogSystem.InstallDefaultReactors();

        //  Create credential and instantiate service
        Credentials credentials = new Credentials(_username, _password, _url);

        _speechToText = new SpeechToText(credentials);
        Credentials naturalLanguageUnderstandingCredentials = new Credentials()
        {
            // Original Watson NLP credentials
            // Username = "b2e711a5-5d95-4988-a355-8653923d7769",
            // Password = "1VRamFU1cXr1",

            Username = "b2e711a5-5d95-4988-a355-8653923d7769",
            Password = "1VRamFU1cXr1",
            Url = "https://gateway.watsonplatform.net/natural-language-understanding/api"
        };
        _nlu = new NaturalLanguageUnderstanding(naturalLanguageUnderstandingCredentials);
        Active = true;
        StartCoroutine(coroutineA());
        StartRecording();
    }

    public bool Active
    {
        get { return _speechToText.IsListening; }
        set
        {
            if (value && !_speechToText.IsListening)
            {
                _speechToText.DetectSilence = true;
                _speechToText.EnableWordConfidence = true;
                _speechToText.EnableTimestamps = true;
                _speechToText.SilenceThreshold = 0.01f;
                _speechToText.MaxAlternatives = 0;
                _speechToText.EnableInterimResults = true;
                _speechToText.OnError = OnError;
                _speechToText.InactivityTimeout = -1;
                _speechToText.ProfanityFilter = false;
                _speechToText.SmartFormatting = true;
                _speechToText.SpeakerLabels = false;
                _speechToText.WordAlternativesThreshold = null;
                _speechToText.StartListening(OnRecognize, OnRecognizeSpeaker);
            }
            else if (!value && _speechToText.IsListening)
            {
                _speechToText.StopListening();
            }
        }
    }

    private void StartRecording()
    {
        if (_recordingRoutine == 0)
        {
            UnityObjectUtil.StartDestroyQueue();
           _recordingRoutine = Runnable.Run(RecordingHandler());
          
            //Runnable.Run(GetResult());
        }
    }
    /*private IEnumerator WaitAndPrint(float waitTime)
    {
        while (true)
        {
            yield return new WaitForSeconds(waitTime);
            Log.Debug("WaitAndPrint " + Time.time,"" ,"dfsdf");
        }
    }*/

    IEnumerator coroutineA()
    {
        // // wait for 1 second
        // while (true)
        // {
        //    // Debug.Log("coroutineA created");
        //     string analyseText = "";
        //     foreach (string item in al)
        //     {
        //         analyseText += item;
        //     }
        //     al.Clear();
        //     analyse(analyseText);
            
        //     yield return new WaitForSeconds(10.0f);
            
        //     //Debug.Log("coroutineA running again");
        // }
        // wait for 1 second
        int timeInt =10;
        
        while (true)
        {
            if (timeInt==0)
            {
                // Get the next second of data
                bufferfor10.Add(recordData);
                recordData="";

                // Perform analysis of the current recorded 10 seconds
                string analyseText="";
                int index=0;
                while(index<10)
                {
                    analyseText+=bufferfor10[index]+" ";
                    index++;
                }
                analyse(analyseText);

                // Remove the first element
                bufferfor10.RemoveAt(0);
            }
            else
            {
                // Record the first 10 seconds before performing any analysis
                bufferfor10.Add(recordData);
                recordData="";
                timeInt=timeInt-1;
            }

            yield return new WaitForSeconds(1.0f);
        }
    }

    private void StopRecording()
    {
        if (_recordingRoutine != 0)
        {
            Microphone.End(_microphoneID);
            Runnable.Stop(_recordingRoutine);
            StopAllCoroutines();
            _recordingRoutine = 0;
        }
    }
    private IEnumerator GetResult()
    {
        if (counter >0)
            counter -= 1;
        if (counter==0)
        Log.Debug("counter", "counter", "counter");
        return null;

    }
    private void OnError(string error)
    {
        Active = false;

        Log.Debug("ExampleStreaming.OnError()", "Error! {0}", error);
    }

    private IEnumerator RecordingHandler()
    {
        Log.Debug("ExampleStreaming.RecordingHandler()", "devices: {0}", Microphone.devices);
        _recording = Microphone.Start(_microphoneID, true, _recordingBufferSize, _recordingHZ);
        yield return null;      // let _recordingRoutine get set..

        if (_recording == null)
        {
            StopRecording();
            yield break;
        }

        bool bFirstBlock = true;
        int midPoint = _recording.samples / 2;
        float[] samples = null;

        while (_recordingRoutine != 0 && _recording != null)
        {
            int writePos = Microphone.GetPosition(_microphoneID);
            if (writePos > _recording.samples || !Microphone.IsRecording(_microphoneID))
            {
                Log.Error("ExampleStreaming.RecordingHandler()", "Microphone disconnected.");

                StopRecording();
                yield break;
            }

            if ((bFirstBlock && writePos >= midPoint)
              || (!bFirstBlock && writePos < midPoint))
            {
                // front block is recorded, make a RecordClip and pass it onto our callback.
                samples = new float[midPoint];
                _recording.GetData(samples, bFirstBlock ? 0 : midPoint);

                AudioData record = new AudioData();
                record.MaxLevel = Mathf.Max(Mathf.Abs(Mathf.Min(samples)), Mathf.Max(samples));
                record.Clip = AudioClip.Create("Recording", midPoint, _recording.channels, _recordingHZ, false);
                record.Clip.SetData(samples, 0);

                _speechToText.OnListen(record);

                bFirstBlock = !bFirstBlock;
            }
            else
            {
                // calculate the number of samples remaining until we ready for a block of audio, 
                // and wait that amount of time it will take to record.
                int remaining = bFirstBlock ? (midPoint - writePos) : (_recording.samples - writePos);
                float timeRemaining = (float)remaining / (float)_recordingHZ;

                yield return new WaitForSeconds(timeRemaining);
            }

        }

        yield break;
    }
   
    private void OnRecognize(SpeechRecognitionEvent result)
    {
        if (result != null && result.results.Length > 0)
        {
            foreach (var res in result.results)
            {
                foreach (var alt in res.alternatives)
                {
                    string text = string.Format("{0} ({1}, {2:0.00})\n", alt.transcript, res.final ? "Final" : "Interim", alt.confidence);
                    Log.Debug("ExampleStreaming.OnRecognize()", text);
//                    ResultsField.text = text;

                    int finalPos= text.IndexOf("(Final");
                    int interimPos=text.IndexOf("(Interim");
                    if (finalPos>0){
                        string replaceText=text.Substring(finalPos);
                        text=text.Replace(replaceText, "");                                                                                                                                                                                                                
                    }
                    
                    else if(interimPos>0){
                        string replaceText=text.Substring(interimPos);
                        text=text.Replace(replaceText, "");
                    }

                    //al.Add(text);
                    if (recordData=="")
                        recordData=text;
                    else recordData=recordData+" "+text;
                }

                if (res.keywords_result != null && res.keywords_result.keyword != null)
                {
                    foreach (var keyword in res.keywords_result.keyword)
                    {
                        Log.Debug("ExampleStreaming.OnRecognize()", "keyword: {0}, confidence: {1}, start time: {2}, end time: {3}", keyword.normalized_text, keyword.confidence, keyword.start_time, keyword.end_time);
                    }
                }

                if (res.word_alternatives != null)
                {
                    foreach (var wordAlternative in res.word_alternatives)
                    {
                        Log.Debug("ExampleStreaming.OnRecognize()", "Word alternatives found. Start time: {0} | EndTime: {1}", wordAlternative.start_time, wordAlternative.end_time);
                        foreach (var alternative in wordAlternative.alternatives)
                            Log.Debug("ExampleStreaming.OnRecognize()", "\t word: {0} | confidence: {1}", alternative.word, alternative.confidence);
                    }
                }
            }
        }
    }
    public void analyse(string inputText)
    {
        if (inputText=="")
            return;
        Parameters parameters = new Parameters()
        {
            //text = "In the rugged Colorado Desert of California, there lies buried a treasure ship sailed there hundreds of years ago by either Viking or Spanish explorers. Some say this is legend; others insist it is fact. A few have even claimed to have seen the ship, its wooden remains poking through the sand like the skeleton of a prehistoric beast. Among those who say they�ve come close to the ship is small-town librarian Myrtle Botts. In 1933, she was hiking with her husband in the Anza-Borrego Desert, not far from the border with Mexico. It was early March, so the desert would have been in bloom, its washed-out yellows and grays beaten back by the riotous invasion of wildflowers. Those wildflowers were what brought the Bottses to the desert, and they ended up near a tiny settlement called Agua Caliente. Surrounding place names reflected the strangeness and severity of the land: Moonlight Canyon, Hellhole Canyon, Indian Gorge. Try Newsweek for only $1.25 per week To enter the desert is to succumb to the unknowable. One morning, a prospector appeared in the couple�s camp with news far more astonishing than a new species of desert flora: He�d found a ship lodged in the rocky face of Canebrake Canyon. The vessel was made of wood, and there was a serpentine figure carved into its prow. There were also impressions on its flanks where shields had been attached�all the hallmarks of a Viking craft. Recounting the episode later, Botts said she and her husband saw the ship but couldn�t reach it, so they vowed to return the following day, better prepared for a rugged hike. That wasn�t to be, because, several hours later, there was a 6.4 magnitude earthquake in the waters off Huntington Beach, in Southern California. Botts claimed it dislodged rocks that buried her Viking ship, which she never saw again.There are reasons to doubt her story, yet it is only one of many about sightings of the desert ship. By the time Myrtle and her husband had set out to explore, amid the blooming poppies and evening primrose, the story of the lost desert ship was already about 60 years old. By the time I heard it, while working on a story about desert conservation, it had been nearly a century and a half since explorer Albert S. Evans had published the first account. Traveling to San Bernardino, Evans came into a valley that was �the grim and silent ghost of a dead sea,� presumably Lake Cahuilla. �The moon threw a track of shimmering light,� he wrote, directly upon �the wreck of a gallant ship, which may have gone down there centuries ago.� The route Evans took came nowhere near Canebrake Canyon, and the ship Evans claimed to see was Spanish, not Norse. Others have also seen this vessel, but much farther south, in Baja California, Mexico. Like all great legends, the desert ship is immune to its contradictions: It is fake news for the romantic soul, offering passage into some ancient American dreamtime when blood and gold were the main currencies of civic life.The legend does seem, prima facie, bonkers: a craft loaded with untold riches, sailed by early-European explorers into a vast lake that once stretched over much of inland Southern California, then run aground, abandoned by its crew and covered over by centuries of sand and rock and creosote bush as that lake dried out�and now it lies a few feet below the surface, in sight of the chicken-wire fence at the back of the Desert Dunes motel, $58 a night and HBO in most rooms.Totally insane, right? Let us slink back to our cubicles and never speak of the desert ship again. Let us only believe that which is shared with us on Facebook. Let us banish forever all traces of wonder from our lives. Yet there are believers who insist that, using recent advances in archaeology, the ship can be found. They point, for example, to a wooden sloop from the 1770s unearthed during excavations at the World Trade Center site in lower Manhattan, or the more than 40 ships, dating back perhaps 800 years, discovered in the Black Sea earlier this year.",
            //text = "Analyze various features of text content at scale. Provide text, raw HTML, or a public URL, and IBM Watson Natural Language Understanding will give you results for the features you request. The service cleans HTML content before analysis by default, so the results can ignore most advertisements and other unwanted content.",
            // text = "I am Happy, I am going to a beach party. ",
            text = inputText,
            
            return_analyzed_text = true,
            language = "en",
            features = new Features()
            {
                // entities = new EntitiesOptions()
                // {
                //     limit = 100,
                //     sentiment = true,
                //     emotion = true,
                // },
                keywords = new KeywordsOptions()
                {
                    limit = 1,
                    //relevance = true,
                    sentiment = true,
                    emotion = true
                }
            }
        };

        _nlu.Analyze(OnAnalyseResponse, OnFail, parameters);
    }
    private void OnAnalyseResponse(AnalysisResults response, Dictionary<string, object> customData)
    {
        Log.Debug("NaturalLanguageUnderstanding.OnAnalyze()", "AnalysisResults: {0}", customData["json"].ToString());
        _analyzeTested = true;

        var N = JSON.Parse(customData["json"].ToString());

        // multiply all emotions by 100 to put them in the same range as the other emotion modalities
		try{
        	currentEmotions.joy = Single.Parse(N["keywords"][0]["emotion"]["joy"]) * 100.0f;
        	currentEmotions.anger = Single.Parse(N["keywords"][0]["emotion"]["anger"]) * 100.0f;
        	currentEmotions.fear = Single.Parse(N["keywords"][0]["emotion"]["fear"]) * 100.0f;
        	currentEmotions.disgust = Single.Parse(N["keywords"][0]["emotion"]["disgust"]) * 100.0f;
        	currentEmotions.sadness = Single.Parse(N["keywords"][0]["emotion"]["sadness"]) * 100.0f;
		}
		catch(SystemException e){
			currentEmotions.joy = 0f;
			currentEmotions.anger = 0f;
			currentEmotions.fear = 0f;
			currentEmotions.disgust = 0f;
			currentEmotions.sadness = 0f;
		}
        // Debug.Log("Joy: " + currentEmotions.joy);
        // Debug.Log("anger: " + currentEmotions.anger);
        // Debug.Log("fear: " + currentEmotions.fear);
        // Debug.Log("disgust: " + currentEmotions.disgust);
        // Debug.Log("sadness: " + currentEmotions.sadness);
    }

    private void OnFail(RESTConnector.Error error, Dictionary<string, object> customData)
    {
        Log.Debug("SentimentAnalysisDemo.OnFail()", "Error (0)", error.ToString());
    }

    private void OnRecognizeSpeaker(SpeakerRecognitionEvent result)
    {
        if (result != null)
        {
            foreach (SpeakerLabelsResult labelResult in result.speaker_labels)
            {
                Log.Debug("ExampleStreaming.OnRecognize()", string.Format("speaker result: {0} | confidence: {3} | from: {1} | to: {2}", labelResult.speaker, labelResult.from, labelResult.to, labelResult.confidence));
            }
        }
    }
	public AudioClip GetAudClip(){
		return _recording;
	}
}