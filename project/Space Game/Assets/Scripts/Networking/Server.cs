using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;

public class Server {

	private bool ServerStarted = false;

	//server variables
	private int m_port;
	private int m_maxConnections;
	private int m_hostID;
	private int m_unrelChan;
	private int m_relChan;
	private byte m_error;
	private byte[] hostBuffer = new byte[1024];
	private int hostConnectionID = 1;
	NetworkController m_controller;

	private List<int> ClientIDs;

	public byte[] getHostBuffer() {
		return hostBuffer;
	}

	public bool Started() {
		return ServerStarted;
	}

	public Server() {
		ServerStarted = false;
	}

	public Server(int port, int maxConnections, NetworkController controller) {

		ClientIDs = new List<int>();
		m_controller = controller;
		m_port = port;
		m_maxConnections = maxConnections;

		//server initialization
		NetworkTransport.Init();
		ConnectionConfig cc = new ConnectionConfig();
		m_unrelChan = cc.AddChannel(QosType.Unreliable);
		m_relChan = cc.AddChannel(QosType.Reliable);
		HostTopology hostT = new HostTopology(cc, m_maxConnections);
		m_hostID = NetworkTransport.AddHost(hostT, m_port, null);

		ServerStarted = true;
	}

	private void ConnectClient(int id) {
		if (!ClientIDs.Contains(id)) {
			ClientIDs.Add(id);
		}
	}

	private void DisconnectClient(int id) {
		if (ClientIDs.Contains(id)) {
			ClientIDs.Remove(id);
		}
	}

	private void Broadcast(string str) {
		foreach (int id in ClientIDs) {
			SendToClient(id, str);
		}
	}

	public void Update() {

		if (ServerStarted) {

			//all the server code
			int recHostID;
			int connectionID;
			int channelID;
			byte[] recBuffer = new byte[1024];
			int bufferSize = 1024;
			int dataSize;
			byte error;

			NetworkEventType recData = NetworkTransport.Receive(out recHostID, out connectionID, out channelID, recBuffer, bufferSize, out dataSize, out error);

			string msg = "";

			switch (recData) {
				case NetworkEventType.Nothing:
					break;

				case NetworkEventType.ConnectEvent:
					msg = Encoding.Unicode.GetString(recBuffer, 0, dataSize);
					Debug.Log("player" + connectionID + " connected " + msg);
					ConnectClient(connectionID);
					SendToClient(connectionID, "chat:welcome player" + connectionID);
					Broadcast("chat:Player" + connectionID + " has joined!");
					break;

				case NetworkEventType.DataEvent:
					msg = Encoding.Unicode.GetString(recBuffer, 0, dataSize);
					proccessMsg(msg, connectionID);
					break;

				case NetworkEventType.DisconnectEvent:
					msg = Encoding.Unicode.GetString(recBuffer, 0, dataSize);
					Debug.Log("player" + connectionID + " disconnected " + msg);
					DisconnectClient(connectionID);
					break;
			}
		}
	}

	private void SendToClient(int connectionID, string str) {

		byte[] buffer = Encoding.Unicode.GetBytes(str);

		if (connectionID == hostConnectionID) {
			hostBuffer = buffer;
			m_controller.HostRecieveEvent(hostBuffer);
		} else {
			NetworkTransport.Send(m_hostID, connectionID, m_unrelChan, buffer, str.Length * sizeof(char), out m_error);
		}

	}

	public void HostRecieve(byte[] buffer) {
		proccessMsg(Encoding.Unicode.GetString(buffer), 1);
	}

	private void proccessMsg(string str, int id) {
		Debug.Log("Server Recieved: " + str + " from player" + id);
	}

	public void Stop() {
		ServerStarted = false;
		NetworkTransport.RemoveHost(m_hostID);
	}

}
