using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class ServerClient
{
    public int connectionId;
    public string playerName;
    public Vector3 position;
}

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

    private List<ServerClient> clients = new List<ServerClient>();

    private float lastMovementUpdate;
    //Refreshrate
    private float movementUpdateRate = 0.05f;

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
            //case NetworkEventType.Nothing:         //1
            //    break;
            case NetworkEventType.ConnectEvent:    //2
                Debug.Log("Player " + connectionId + " has connected");
                OnConnection(connectionId);
                break;
            case NetworkEventType.DataEvent:       //3
                string msg = Encoding.Unicode.GetString(recBuffer, 0, dataSize);
                Debug.Log("Receiving " + connectionId + " has sent Message: " + msg);

                string[] splitData = msg.Split('|');

                //Switch für PIPE
                switch (splitData[0])
                {
                    case "NAMEIS":
                        OnNameIs(connectionId, splitData[1]);
                        break;
                    case "MYPOSITION":
                        //Fehler bin ich ja der Meinung
                        OnMyPosition(connectionId, float.Parse(splitData[1]), float.Parse(splitData[2]));
                        break;
                    default:
                        Debug.Log("Invalid message: " + msg);
                        break;
                }

                break;

            case NetworkEventType.DisconnectEvent: //4
                Debug.Log("Player " + connectionId + " has disconnected");
                OnDisconnection(connectionId);
                break;
        }

        //ASK Player for their position
        if ((Time.time - lastMovementUpdate) > movementUpdateRate)
        {
            lastMovementUpdate = Time.time;
            //ASK everybody
            string msgPosition = "ASKPOSITION|";
            //Send every position to player
            foreach (ServerClient sc in clients)
            {
                msgPosition += sc.connectionId.ToString() + '%' +sc.position.x.ToString()+ '%' +sc.position.y.ToString() + '|';
            }
            msgPosition = msgPosition.Trim('|');
            Send(msgPosition, unreliableChannel, clients);
        }
    }

    private void OnConnection(int cnnId)
    {
        //Add Player to a list
        ServerClient c = new ServerClient();
        c.connectionId = cnnId;
        c.playerName = "TEMP";
        clients.Add(c);

        //When Player joins the server, tell him is ID
        //Request his name and send the name of all the other players
        string msg = "ASKNAME|" + cnnId + "|";

        foreach (ServerClient sc in clients)
        {
            msg += sc.playerName + "%" + sc.connectionId + "|";
        }

        msg = msg.Trim('|');

        //echter C Pipe Einsatz wtf
        //ASKNAME|3|DAVE%1|MICHAEL%2|TEMP%3
        Send(msg, reliableChannel, cnnId);
    }

    public void ChangeScene(string sceneName)
    {
        NetworkManager.singleton.ServerChangeScene(sceneName);
        Debug.Log("Level sollte gewechselt sein!");
    }

    private void OnDisconnection(int cnnId)
    {
        //Remove player from client list
        clients.Remove(clients.Find(x=>x.connectionId==cnnId));


        //Tell everyone else that somebody has disconnected
        Send("DC|" + cnnId, reliableChannel,clients);
    }

    private void OnNameIs(int cnnId, string playerName)
    {
        // Link the name to the connection Id
        clients.Find(x => x.connectionId == cnnId).playerName = playerName;

        //Tell everybody that a new player has connected
        Send("CNN|" + playerName + "|" + cnnId, reliableChannel, clients);
    }

    private void OnMyPosition(int cnnId, float x, float y)
    {
        clients.Find(c => c.connectionId == cnnId).position = new Vector3(x, y, 0);
    }

    private void Send(string message, int channelId, int cnnId)
    {
        List<ServerClient> c = new List<ServerClient>();
        c.Add(clients.Find(x => x.connectionId == cnnId));
        Send(message, channelId, c);
    }

    private void Send(string message, int channelId, List<ServerClient> c)
    {
        Debug.Log("Sending: " + message);
        byte[] msg = Encoding.Unicode.GetBytes(message);
        foreach (ServerClient sc in c)
        {
            NetworkTransport.Send(hostId, sc.connectionId, channelId, msg, message.Length * sizeof(char), out error);
        }
    }
}
