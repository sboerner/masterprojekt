using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class Server : MonoBehaviour {

	private const int MAX_CONNECTION = 100;

	private int port = 5701;

	private int hostId;
	private int webHostId;

    //reliable - got sent and received in the same order
    //unreliable might get lost e.g. movement
    private int reliableChannel;
	private int unreliableChannel;

	private bool isStarted = false;
	private byte error;


	private void Start()
    {
        //Starts Network # needs UnityEngine.Networking
		NetworkTransport.Init();
        ConnectionConfig cc = new ConnectionConfig();

        //add channels to cc
        reliableChannel = cc.AddChannel(QosType.Reliable);
        unreliableChannel = cc.AddChannel(QosType.Unreliable);

        //create network with cc
        HostTopology topo = new HostTopology(cc, MAX_CONNECTION);

        //null accepts connection from everybody
        hostId = NetworkTransport.AddHost(topo, port, null);
        webHostId = NetworkTransport.AddWebsocketHost(topo, port, null);

        isStarted = true;
    }

    private void Update()
    {
        if (!isStarted)
        {
            return;
        }


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
                Debug.Log("Player " + connectionId + " has connected");
                break;
            case NetworkEventType.DataEvent:       //3
                string msg = Encoding.Unicode.GetString(recBuffer, 0, dataSize);
                Debug.Log("Player " + connectionId + " has sent");
                break;
            case NetworkEventType.DisconnectEvent: //4
                Debug.Log("Player " + connectionId + " has disconnected");
                break;
        }

    }
}
