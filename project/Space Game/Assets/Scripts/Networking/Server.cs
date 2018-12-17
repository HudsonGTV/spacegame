using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System;

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

	private void Broadcast(byte[] buffer) {
		foreach (int id in ClientIDs) {
			SendToClient(id, buffer);
		}
	}

	private void BroadcastFrom(byte[] buffer, int sender) {
		foreach (int id in ClientIDs) {
			if(id != sender)
				SendToClient(id, buffer);
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
					break;

				case NetworkEventType.DataEvent:
					msg = Encoding.Unicode.GetString(recBuffer, 0, dataSize);
					proccessMsg(recBuffer, connectionID);
					break;

				case NetworkEventType.DisconnectEvent:
					msg = Encoding.Unicode.GetString(recBuffer, 0, dataSize);
					Debug.Log("player" + connectionID + " disconnected " + msg);
					DisconnectClient(connectionID);
					break;
			}
		}
	}

	private void SendToClient(int connectionID, byte[] buffer) {

		if (connectionID == hostConnectionID) {
			hostBuffer = buffer;
			m_controller.HostRecieveEvent(hostBuffer);
		} else {
			NetworkTransport.Send(m_hostID, connectionID, m_unrelChan, buffer, buffer.Length, out m_error);
		}

	}

	public void HostRecieve(byte[] buffer) {
		proccessMsg(buffer, 1);
	}

	private void proccessMsg(byte[] buffer, int id) {

		switch (buffer[0]) {
			case (byte)NetType.Translation:
			case (byte)NetType.Rotation:
			case (byte)NetType.Scale:
				if (id == 1) {
					BroadcastFrom(buffer, id);
				}
				break;
			case (byte)NetType.Control:
				byte[] idbuf = BitConverter.GetBytes(id);
				Array.Copy(idbuf, 0, buffer, 1, idbuf.Length);
				SendToClient(1, buffer);
				break;
			case (byte)NetType.Sync:
				idbuf = BitConverter.GetBytes(id);
				Array.Copy(idbuf, 0, buffer, 1, idbuf.Length);
				BroadcastFrom(buffer, id);
				break;
		}

	}

	public void Stop() {
		ServerStarted = false;
		NetworkTransport.RemoveHost(m_hostID);
	}

}
