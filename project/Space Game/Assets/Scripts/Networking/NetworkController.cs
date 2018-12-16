using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Threading;
using System.Text;
using System;

public class NetworkController : MonoBehaviour {

	public static List<NetworkIdentity> ids = new List<NetworkIdentity>();

	public static void AddID(NetworkIdentity id) {
		id.id = id.gameObject.GetInstanceID();
		ids.Add(id);
	}

	public int m_port = 7777;

	private bool ServerConnected = false;
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
	private bool Host = false;

	//server variables
	Server server;

	public string message;

	public bool getHost() {
		return Host;
	}

	private void Start() {

		Net.networkController = this;

		server = new Server();

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

		if (server.Started()) {
			server.Update();
		}

		if (ClientConnected) {
			Client();
		}

	}

	private void processMsg(byte[] buffer) {
		Debug.Log("Client Recieved: " + buffer);

		switch (buffer[0]) {
			case (byte)NetType.Translation:
			case (byte)NetType.Rotation:
			case (byte)NetType.Scale:
				for (int i = 0; i < ids.Count; ++i) {
					if (ids[i].id == BitConverter.ToInt32(buffer, 1)) {
						ids[i].queue.Add(buffer);
						break;
					}
				}
				break;
		}

	}

	public void StartHost() {

		Debug.Log("Server Starting on Port: " + m_port);
		server = new Server(m_port, max_connections, this);
		Debug.Log("Server Started");
		server.Update();
		StartClient("127.0.0.1", m_port);

		Host = true;

	}

	public void StopHost() {
		server.Stop();
		Host = false;
	}

	public bool StartClient(string ip, int port) {
		if (!ClientConnected) {
			connectionID = NetworkTransport.Connect(hostID, ip, port, 0, out error);
			Debug.Log("Connected to server: " + hostID + " " + ip + " " + port + " with id of: " + connectionID);
			Debug.Log(error);
			ClientConnected = true;
			return true;
		}
		return false;
	}

	void ConnectedToServer() {
		//send welcome message
		sendString("chat:player says hello");
	}

	public void StopClient() {
		ClientConnected = false;
		NetworkTransport.Disconnect(hostID, connectionID, out error);
		if (server.Started()) {
			StopHost();
		}
	}

	private void OnApplicationQuit() {
		StopClient();
	}

	public void sendString(string str) {

		byte[] strbytes = Encoding.Unicode.GetBytes(str);
		byte[] buffer = new byte[strbytes.Length];
		Array.Copy(strbytes, 0, buffer, 1, strbytes.Length);

		buffer[0] = (byte)NetType.Chat;

		if (Host) {
			server.HostRecieve(buffer);
		} else {
			NetworkTransport.Send(hostID, connectionID, unrelChan, buffer, buffer.Length * sizeof(byte), out error);
		}
	}

	public void sendBytes(byte[] buffer) {
		if (Host) {
			server.HostRecieve(buffer);
		} else {
			NetworkTransport.Send(hostID, connectionID, unrelChan, buffer, buffer.Length * sizeof(byte), out error);
		}
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
				if (!Host && connectionID == this.connectionID) {
					string msg = Encoding.Unicode.GetString(recBuffer, 0, dataSize);
					message = msg;
					processMsg(recBuffer);
				}
				break;
			case NetworkEventType.ConnectEvent:
				if (connectionID == this.connectionID) {
					ConnectedToServer();
				}
				Debug.Log(connectionID);
				Debug.Log(this.connectionID);
				break;
		}
	}

	public void HostRecieveEvent(byte[] buffer) {
		string msg = Encoding.Unicode.GetString(buffer);
		processMsg(buffer);
	}

}
