using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Client : MonoBehaviour {
	
	private const int MAX_CONNECTION = 100;

	private int port = 5701;

	private int hostId;
	private int webHostId;

	private int reliableChannel;
	private int unreliableChannel;

    private int connectionId;

    private float connectionTime;
	private bool isConnected = false;
    private bool isStarted = false;
	private byte error;

    private string playerName;

    public void Connect()
    {

        //Check PlayerName
        string pName = GameObject.Find("NameInput").GetComponent<InputField>().text;
        if (pName =="") {
            Debug.Log("You must enter a name!");
            return;
        }

        playerName = pName;

        //Starts Network # needs UnityEngine.Networking
        NetworkTransport.Init();
        ConnectionConfig cc = new ConnectionConfig();

        //add channels to cc
        reliableChannel = cc.AddChannel(QosType.Reliable);
        unreliableChannel = cc.AddChannel(QosType.Unreliable);

        //create network with cc
        HostTopology topo = new HostTopology(cc, MAX_CONNECTION);

        //null accepts connection from everybody
        hostId = NetworkTransport.AddHost(topo,0);
        connectionId = NetworkTransport.Connect(hostId, "127.0.0.1", port, 0, out error);
        //connectionId = NetworkTransport.Connect(hostId, "141.57.58.32", port, 0, out error);

        connectionTime = Time.time;
        isConnected = true;
    }

    private void Update()
    {
        if (!isConnected)
        {
            GameObject.Find("Status").GetComponent<Text>().text = "false";
            return;
        }

        GameObject.Find("Status").GetComponent<Text>().text = "true";

        //https://docs.unity3d.com/Manual/UNetUsingTransport.html
        int recHostId;
        int connectionId;
        int channelId;
        byte[] recBuffer = new byte[1024];
        int bufferSize = 1024;
        int dataSize;
        byte error;
        NetworkEventType recData = NetworkTransport.Receive(out recHostId, out connectionId, out channelId, recBuffer, bufferSize, out dataSize, out error);
        switch (recData)
        {
            case NetworkEventType.Nothing:         //1
                break;
            case NetworkEventType.ConnectEvent:    //2
                break;
            case NetworkEventType.DataEvent:       //3
                break;
            case NetworkEventType.DisconnectEvent: //4
                break;
        }
    }

}
