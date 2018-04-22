using UnityEngine;
using System.Collections;

using System;
using System.IO;
using UnityEngine.UI;

using System.Linq;
using System.Text;
using System.Net;
using System.Threading;
using System.Collections.Generic;

#if NETFX_CORE
using Windows.Networking.Sockets;
#else
using System.Net.Sockets;
#endif

public class Receiver2 : MonoBehaviour {

	public UIManager uiManagerScript;

	public bool enableLog = false;
	public bool enableLogData = false;

	[HideInInspector]
	public Texture2D receivedTexture;

	// Use this for initialization
	void Start()
	{
		receivedTexture = new Texture2D(0, 0);

		StartCoroutine(ReceiveData());
	}

	int m_frameCounter = 0;
	float m_timeCounter = 0.0f;
	float m_lastFramerate = 0.0f;
	public float m_refreshTime = 5f;
	void Update(){

		//Calculate FPS
		if( m_timeCounter < m_refreshTime )
		{
			m_timeCounter += Time.deltaTime;
		}
		else
		{
			//This code will break if you set your m_refreshTime to 0, which makes no sense.
			m_lastFramerate = (float)m_frameCounter/m_timeCounter;
			m_frameCounter = 0;
			m_timeCounter = 0.0f;
		}

		Debug.Log ("Current FPS: " + m_lastFramerate.ToString());
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//For setting up receiving server
	private List<TcpClient> senderClients = new List<TcpClient>();
	private TcpListener networkListener;
	NetworkStream senderClientStream = null;
	public int receiverPort = 8010;
	private bool connectionReady = false;
	private bool stop = false;
	//This must be the-same with SEND_COUNT on the senderClient
	const int SEND_RECEIVE_COUNT = 15;

	IEnumerator ReceiveData()
	{
		networkListener = new TcpListener (IPAddress.Any, receiverPort);
		networkListener.Start ();

		//Debug.Log("I am here 0");

		bool isConnected = false;
		TcpClient senderClient = null;
		// Wait for senderClient to connect in another Thread 
		Loom.RunAsync(() =>
			{
				//while (!connectionReady)
				while (!stop)
				{
					LOGWARNING("Connecting to sender ... ");

					// Wait for senderClient connection
					senderClient = networkListener.AcceptTcpClient();

					//Debug.Log("I am here 1");

					// We are connected
					senderClients.Add(senderClient);

					//Debug.Log("I am here 2");

					isConnected = true;
					connectionReady = true;
					senderClientStream = senderClient.GetStream();

					//Debug.Log("I am here 3");
				}
			});

		//Wait until senderClient has connected
		while (!isConnected)
		{
			connectionReady = false;
			yield return null;
		}

		LOGWARNING("Connected with sender");

		ImageReceiver();
	}

	void ImageReceiver()
	{
		//Debug.Log ("I am here 4");
		//While loop in another Thread is fine so we don't block main Unity Thread
		Loom.RunAsync(() =>
			{
				//Debug.Log ("I am here 5 " + stop.ToString());
				while (!stop)
				{
					//Read Image Count
					int imageSize = readImageByteSize(SEND_RECEIVE_COUNT);
					LOGWARNING("Received Image byte Length: " + imageSize);

					//Read Image Bytes and Display it
					readFrameBytesAndDisplay(imageSize);

					//For calculating FPS
					m_frameCounter++;
				}
			});
	}

	//Read incoming header bytes to get size of incoming image
	private int readImageByteSize(int size)
	{
		//Debug.Log ("I am here 6");
		bool disconnected = false;

		//NetworkStream serverStream = client.GetStream();
		byte[] imageBytesCount = new byte[size];
		var total = 0;
		do
		{
			//Debug.Log("I am here 7");
			var read = senderClientStream.Read(imageBytesCount, total, size - total);
			//Debug.Log("Client recieved " + total.ToString() + " bytes");
			if (read == 0)
			{
				disconnected = true;
				break;
			}
			total += read;
		} while (total != size);

		//Debug.Log("I am here 8");

		int byteLength;
		if (disconnected)
		{
			byteLength = -1;
		}
		else
		{
			//byteLength = frameByteArrayToByteLength(imageBytesCount);
			byteLength = BitConverter.ToInt32(imageBytesCount, 0);
		}
		return byteLength;
	}

	//Read incoming bytes from stream to prepare image for display
	private void readFrameBytesAndDisplay(int size)
	{
		bool disconnected = false;

		//NetworkStream serverStream = client.GetStream();
		byte[] imageBytes = new byte[size];
		var total = 0;
		do
		{
			//var read = serverStream.Read(imageBytes, total, size - total);
			var read = senderClientStream.Read(imageBytes, total, size - total);
			//Debug.LogFormat("Client recieved {0} bytes", total);
			if (read == 0)
			{
				disconnected = true;
				break;
			}
			total += read;
		} while (total != size);

		bool readyToReadAgain = false;

		//Display Image
		if (!disconnected)
		{
			//Display Image on the main Thread
			Loom.QueueOnMainThread(() =>
				{
					displayReceivedImage(imageBytes);
					readyToReadAgain = true;
					AskUIManagerToPrepareData();
				});
		}

		//Wait until old Image is displayed
		while (!readyToReadAgain)
		{
			System.Threading.Thread.Sleep(1);
		}
	}

	void AskUIManagerToPrepareData(){
		uiManagerScript.PrepareDataToSend();
	}

	int countOfFramesReceived = 0;
	void displayReceivedImage(byte[] receivedImageBytes)
	{
		countOfFramesReceived++;
		//Debug.Log("Count of frames received: " + countOfFramesReceived);
		receivedTexture.LoadImage(receivedImageBytes);
		receivedTexture.Apply();
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// Sender - Only send data to receiver when this app is connected to receiver already

	/// //Set data to be sent here
	byte[] updatedEmotionData;
	public void UpdateEmotionData (byte[] newData) {
		updatedEmotionData = newData;
		SendData();
	}

	int loopcheck = 0;
	void SendData()
	{
		loopcheck++;
		//Debug.Log("Sent data-packets: " + loopcheck);

		byte[] dataPacketSize = new byte[SEND_RECEIVE_COUNT];

		byteLengthToDataByteArray (updatedEmotionData.Length, dataPacketSize);

		Loom.RunAsync(() =>
			{
				//Send total byte count first
				senderClientStream.Write(dataPacketSize, 0, dataPacketSize.Length);
				LOGDATA("Sent Image byte Length: " + dataPacketSize.Length);

				//Send the image bytes
				senderClientStream.Write(updatedEmotionData, 0, updatedEmotionData.Length);
				LOGDATA("Sending Image byte array data : " + updatedEmotionData.Length);
			});
	}

	//Converts integer to byte array
	void byteLengthToDataByteArray(int byteLength, byte[] fullBytes)
	{
		//Clear old data
		Array.Clear(fullBytes, 0, fullBytes.Length);
		//Convert int to bytes
		byte[] bytesToSendCount = BitConverter.GetBytes(byteLength);
		//Copy result to fullBytes
		bytesToSendCount.CopyTo(fullBytes, 0);
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	void LOG(string messsage)
	{
		if (enableLog)
			Debug.Log(messsage);
	}

	void LOGDATA(string messsage)
	{
		if (enableLogData)
			Debug.Log(messsage);
	}

	void LOGWARNING(string messsage)
	{
		if (enableLog)
			Debug.LogWarning(messsage);
	}

	void OnApplicationQuit()
	{
		if (networkListener != null)
		{
			networkListener.Stop();
		}

		foreach (TcpClient c in senderClients)
			c.Close();
	}

	WaitForEndOfFrame endOfFrame = new WaitForEndOfFrame();
}
