using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Threading;
using System.Text;
using System;

[RequireComponent(typeof(NetworkedInput))]
public class NetworkController : MonoBehaviour {

	public static List<NetworkIdentity> ids = new List<NetworkIdentity>();

	public GameObject PlayerPrefab;

	public static void AddID(NetworkIdentity id) {
		ids.Add(id);
		Net.RequestNewID();
	}

	public int m_port = 7777;

	private bool ServerConnected = false;
	private bool ClientConnected = false;

	private bool ConnectionConfirmed = false;

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

		//StartHost();
		StartClient("127.0.0.1", m_port);

	}

	private void Update() {

		if (server.Started()) {
			server.Update();
		}

		if (ClientConnected) {
			Client();
		}

	}

	private void assignID(int id) {
		Debug.Log("id: " + id + " recieved, looking for id-less object");
		Debug.Log(ids.Count);
		for (int i = 0; i < ids.Count; ++i) {
			if (ids[i].id == -1) {
				Debug.Log("id-less object found!");
				ids[i].id = id;
				return;
			}
		}
	}

	private void processMsg(byte[] buffer) {

		if (!ConnectionConfirmed) {
			ConnectionConfirmed = true;
			ConnectedToServer();
		}

		switch (buffer[0]) {
			case (byte)NetType.Translation:
			case (byte)NetType.Rotation:
			case (byte)NetType.Scale:
				if (BitConverter.ToInt32(buffer, 1) < 1) {
					return;
				}
				for (int i = 0; i < ids.Count; ++i) {
					if (ids[i].id == BitConverter.ToInt32(buffer, 1)) {
						ids[i].queue.Add(buffer);
						return;
					}
				}
				//object not found, instantiate

				byte[] strbuf = new byte[buffer.Length - (sizeof(int) + 1 + (sizeof(float) * 3))];

				Array.Copy(buffer, sizeof(int) + 1 + (sizeof(float) * 3), strbuf, 0, strbuf.Length);

				GameObject netObj = (GameObject)Instantiate(Resources.Load(Encoding.Unicode.GetString(strbuf)));
				netObj.GetComponent<NetworkIdentity>().id = BitConverter.ToInt32(buffer, 1);
				netObj.GetComponent<NetworkIdentity>().queue.Add(buffer);
				return;
			case (byte)NetType.Sync:
				if (BitConverter.ToInt32(buffer, 1 + sizeof(int)) < 1) {
					return;
				}
				for (int i = 0; i < ids.Count; ++i) {
					if (ids[i].id == BitConverter.ToInt32(buffer, 1 + sizeof(int))) {
						ids[i].player = true;
						ids[i].plyerID = BitConverter.ToInt32(buffer, 1);
						return;
					}
				}
				//object not found, instantiate

				Debug.Log("new player detected, adding");

				GameObject nPlayer = Instantiate(PlayerPrefab);

				float x = BitConverter.ToSingle(buffer, 1 + (sizeof(int) * 2) + (sizeof(float) * 0));
				float y = BitConverter.ToSingle(buffer, 1 + (sizeof(int) * 2) + (sizeof(float) * 1));
				float z = BitConverter.ToSingle(buffer, 1 + (sizeof(int) * 2) + (sizeof(float) * 2));

				nPlayer.transform.position = new Vector3(x, y, z);

				NetworkIdentity nPlayerNetID = nPlayer.GetComponent<NetworkIdentity>();

				nPlayerNetID.id = BitConverter.ToInt32(buffer, 1 + sizeof(int));
				nPlayerNetID.player = true;
				nPlayerNetID.plyerID = BitConverter.ToInt32(buffer, 1);
				return;
			case (byte)NetType.Control:
				Net.networkInput.queue.Add(buffer);
				return;
			case (byte)NetType.RequestID:
				assignID(BitConverter.ToInt32(buffer, 1));
				return;
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
			Debug.Log("Connecting to server: " + hostID + " " + ip + " " + port);
			connectionID = NetworkTransport.Connect(hostID, ip, port, 0, out error);
			ClientConnected = true;
			return true;
		}
		return false;
	}

	void ConnectedToServer() {

		Debug.Log("Connected to server: " + hostID + " with id of: " + connectionID);

		//create player object
		GameObject nPlayer = Instantiate(PlayerPrefab);

		NetworkIdentity nPlayerNetID = nPlayer.GetComponent<NetworkIdentity>();

		nPlayerNetID.player = true;
		nPlayerNetID.MyPlayer = true;

		AddID(nPlayerNetID);

		nPlayer.transform.position = new Vector3(0,0,0);

	}

	private static bool idsContains(int id) {
		for (int i = 0; i < ids.Count; ++i) {
			if (ids[i].id == id) {
				return true;
			}
		}
		return false;
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

	public void sendBytesRel(byte[] buffer) {
		if (Host) {
			server.HostRecieve(buffer);
		} else {
			NetworkTransport.Send(hostID, connectionID, relChan, buffer, buffer.Length * sizeof(byte), out error);
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
				//if (connectionID == this.connectionID) {
					//ConnectedToServer();
				//}
				break;
		}
	}

	public void HostRecieveEvent(byte[] buffer) {
		string msg = Encoding.Unicode.GetString(buffer);
		processMsg(buffer);
	}

}
