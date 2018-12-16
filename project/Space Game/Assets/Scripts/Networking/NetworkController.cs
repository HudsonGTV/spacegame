using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Threading;
using System.Text;

public class NetworkController : MonoBehaviour {

	public int m_port = 7777;

	private bool ServerStarted = false;
	private bool ClientConnected = false;

	private Thread ServerThread;

	//client variables
	private const int max_connections = 20;
	private int hostID;
	private int connectionID;
	private int unrelChan;
	private int relChan;
	private byte error;
	private string m_ip;

	//server variables
	private const int s_maxConnections = 20;
	private int s_hostID;
	private int s_unrelChan;
	private int s_relChan;
	private byte s_error;

	public string message;

	private void Start() {
		NetworkTransport.Init();
		ConnectionConfig cc = new ConnectionConfig();
		unrelChan = cc.AddChannel(QosType.Unreliable);
		relChan = cc.AddChannel(QosType.Reliable);
		HostTopology hostT = new HostTopology(cc, max_connections);
		hostID = NetworkTransport.AddHost(hostT, 0);

		StartHost();
		//StartClient("127.0.0.1", m_port);

	}

	private void Update() {

		if (ClientConnected) {

			Client();

		}

		if (ServerStarted) {

			Server();

		}

	}

	private void processMsg(string msg) {
		Debug.Log("Client Recieved: " + msg);
	}

	public void StartHost() {

		Debug.Log("Server Starting on Port: " + m_port);
		ServerStart();
		Debug.Log("Server Started");
		Server();
		StartClient("127.0.0.1", m_port);
	}

	public void StopHost() {
		ServerStarted = false;
	}

	public bool StartClient(string ip, int port) {
		if (!ClientConnected) {
			connectionID = NetworkTransport.Connect(hostID, ip, port, 0, out error);
			ClientConnected = true;
			return true;
		}
		return false;
	}

	void OnConnectedToServer() {
		//send welcome message
		sendString("chat:player" + connectionID + " says hello");
	}

	public void StopClient() {
		ClientConnected = false;
		NetworkTransport.Disconnect(hostID, connectionID, out error);
	}

	private void OnApplicationQuit() {
		StopClient();
		StopHost();
	}

	private void ServerStart() {

		//server initialization
		NetworkTransport.Init();
		ConnectionConfig cc = new ConnectionConfig();
		s_unrelChan = cc.AddChannel(QosType.Unreliable);
		s_relChan = cc.AddChannel(QosType.Reliable);
		HostTopology hostT = new HostTopology(cc, max_connections);
		s_hostID = NetworkTransport.AddHost(hostT, m_port, null);

		ServerStarted = true;
	}

	private void sendString(string str) {
		byte[] buffer = Encoding.Unicode.GetBytes(str);
		NetworkTransport.Send(hostID, connectionID, unrelChan, buffer, str.Length * sizeof(char), out error);
	}

	private void Client() {
		//all the client code

		int recHostID;
		int connectionID;
		int channelID;
		byte[] recBuffer = new byte[1024];
		int bufferSize = 1024;
		int dataSize;
		byte error;

		NetworkEventType recData = NetworkTransport.Receive(out recHostID, out connectionID, out channelID, recBuffer, bufferSize, out dataSize, out error);

		switch (recData) {
			case NetworkEventType.DataEvent:
				string msg = Encoding.Unicode.GetString(recBuffer, 0, dataSize);
				message = msg;
				processMsg(msg);
				break;
			case NetworkEventType.ConnectEvent:
				OnConnectedToServer();
				break;
		}
	}

	private void Server() {
		//all the server code
		int s_recHostID;
		int s_connectionID;
		int s_channelID;
		byte[] s_recBuffer = new byte[1024];
		int s_bufferSize = 1024;
		int s_dataSize;
		byte s_error;

		NetworkEventType recData = NetworkTransport.Receive(out s_recHostID, out s_connectionID, out s_channelID, s_recBuffer, s_bufferSize, out s_dataSize, out s_error);

		string msg = "";

		switch (recData) {
			case NetworkEventType.Nothing:
				break;

			case NetworkEventType.ConnectEvent:
				msg = Encoding.Unicode.GetString(s_recBuffer, 0, s_dataSize);
				Debug.Log("user " + s_connectionID + " connected " + msg);
				break;

			case NetworkEventType.DataEvent:
				msg = Encoding.Unicode.GetString(s_recBuffer, 0, s_dataSize);
				Debug.Log("Server Recieved: " + msg);
				byte[] buffer = Encoding.Unicode.GetBytes("hello");
				NetworkTransport.Send(s_hostID, s_connectionID, s_unrelChan, buffer, 5 * sizeof(char), out s_error);
				break;

			case NetworkEventType.DisconnectEvent:
				msg = Encoding.Unicode.GetString(s_recBuffer, 0, s_dataSize);
				Debug.Log("user " + s_connectionID + " disconnected " + msg);
				break;
		}
	}

}
