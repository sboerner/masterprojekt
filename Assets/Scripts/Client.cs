using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Player
{
    public string playerName;
    public GameObject avatar;
    public int connectionId;
}

public class Client : MonoBehaviour {
	
	private const int MAX_CONNECTION = 100;

	private int port = 5701;

	private int hostId;
	private int webHostId;

	private int reliableChannel;
	private int unreliableChannel;

    private int connectionId;
    private int ourClientId;

    private float connectionTime;
	private bool isConnected = false;
    private bool isStarted = false;
	private byte error;

    private string playerName;
    public GameObject playerPrefab;
    public Dictionary<int,Player> players = new Dictionary<int,Player>();

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
        string ip = GameObject.Find("IPInput").GetComponent<InputField>().text;
        connectionId = NetworkTransport.Connect(hostId, ip, port, 0, out error);
        //connectionId = NetworkTransport.Connect(hostId, "141.57.58.32", port, 0, out error);

        connectionTime = Time.time;
        GameObject.Find("ConnectionHint").GetComponent<Text>().text = ("Could connect to server.");
        isConnected = true;
    }

    void OnFailedToConnect(NetworkConnectionError error)
    {
        Debug.Log("Could not connect to server: " + error);
        GameObject.Find("ConnectionHint").GetComponent<Text>().text = ("Could not connect to server: " + error);
        //GameObject.Find("ConnectionHint").SetActive(true);
        isConnected = false;
    }

    private void Update()
    {
        if (!isConnected)
        {
            //GameObject.Find("Status").GetComponent<Text>().text = "false";
            return;
        }

        //GameObject.Find("Status").GetComponent<Text>().text = "true";

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
            //case NetworkEventType.Nothing:         //1
            //    break;
            //case NetworkEventType.ConnectEvent:    //2
            //    break;
            case NetworkEventType.DataEvent:       //3
                string msg = Encoding.Unicode.GetString(recBuffer, 0, dataSize);
                Debug.Log("Receiving: " + msg);

                string[] splitData = msg.Split('|');

                //Switch für PIPE
                switch (splitData[0])
                {
                    case "ASKNAME":
                        OnAskName(splitData);
                        break;
                    case "CNN":
                        SpawnPlayer(int.Parse(splitData[2]), splitData[1]);
                        break;
                    case "DC":
                        PlayerDisconnected(int.Parse(splitData[1]));
                        break;
                    case "ASKPOSITION":
                        OnAskPosition(splitData);
                        break;
                    default:
                        Debug.Log("Invalid message: " + msg);
                        break;
                }

                break;
            //case NetworkEventType.DisconnectEvent: //4
            //    break;
        }
    }

    private void OnAskName(string[] splitData)
    {
        //Wenn in Pipe ASKNAME steht
        ourClientId = int.Parse(splitData[1]);

        //Send our name to the server
        Send("NAMEIS|" + playerName, reliableChannel);

        //Create all the other players
        for (int i = 2; i < splitData.Length - 1; i++){
            string[] d = splitData[i].Split('%');
            SpawnPlayer(int.Parse(d[1]), d[0]);
        }
    }

    private void OnAskPosition(string[] data)
    {
        //Ask Position erst nachdem der Spieler da ist
        if (!isStarted)
        {
            return;
        }


        //Update every other player except himself
        for (int i = 1; i < data.Length;i++)
        {
            string[] d = data[i].Split('%');

            //Prevent the server from updating us
            if (ourClientId != int.Parse(d[0]))
            {
                Vector3 position = Vector3.zero;
                position.x = float.Parse(d[1]);
                position.y = float.Parse(d[2]);
                players[int.Parse(d[0])].avatar.transform.position = position;
            }

        }

        //Send our own position
        Vector3 myPosition = players[ourClientId].avatar.transform.position;
        string msgMyPosition = "MYPOSITION|"+myPosition.x.ToString()+'|'+myPosition.y.ToString();
        Send(msgMyPosition, unreliableChannel);
    }

    private void SpawnPlayer(int cnnId, string playerName)
    {
        GameObject go = Instantiate(playerPrefab) as GameObject;
        //is this ours?
        if (cnnId == ourClientId)
        {
            //Add mobility through PlayerMotor script
            //go.AddComponent<PlayerMotor>();

            //deactivate connection screen
            GameObject.Find("Canvas").SetActive(false);
            //
            isStarted = true;
        }

        //eigene Klasse
        Player p = new Player();
        p.avatar = go;
        p.playerName = playerName;
        p.connectionId = cnnId;
        p.avatar.GetComponentInChildren<TextMesh>().text = playerName;
        players.Add(cnnId,p);
    }

    private void PlayerDisconnected(int cnnId)
    {
        Destroy(players[cnnId].avatar);
        players.Remove(cnnId);
    }

    private void Send(string message, int channelId)
    {
        Debug.Log("Sending: " + message);
        byte[] msg = Encoding.Unicode.GetBytes(message);
        NetworkTransport.Send(hostId, connectionId, channelId, msg, message.Length * sizeof(char), out error);
    }
}
