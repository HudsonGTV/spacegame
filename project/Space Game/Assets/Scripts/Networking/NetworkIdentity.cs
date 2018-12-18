using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class NetworkIdentity : MonoBehaviour {

	public bool player = false;
	public int plyerID = 0;
	public bool MyPlayer = false;
	public int id = -1;

	public bool active() {
		return id != -1;
	}

	public List<byte[]> queue = new List<byte[]>();

	[Range(0.2f, 5)]
	public float syncPS = 2;

	private float counter = 0;

	void Start() {
		NetworkController.ids.Add(this);
	}
	
	void Update() {
		if (MyPlayer && active()) {
			counter += Time.deltaTime;

			if (counter >= 1 / syncPS) {
				counter = 0;

				byte[] buffer = new byte[1 + (sizeof(int)*2) + (sizeof(float) * 3)];
				buffer[0] = (byte)NetType.Sync;

				byte[] idbuf = BitConverter.GetBytes(id);

				byte[] xbuf = BitConverter.GetBytes(transform.position.x);
				byte[] ybuf = BitConverter.GetBytes(transform.position.y);
				byte[] zbuf = BitConverter.GetBytes(transform.position.z);

				Array.Copy(idbuf, 0, buffer, 1 + sizeof(int), idbuf.Length);

				Array.Copy(xbuf, 0, buffer, 1 + (sizeof(int)*2) + (sizeof(float) * 0), xbuf.Length);
				Array.Copy(ybuf, 0, buffer, 1 + (sizeof(int)*2) + (sizeof(float) * 1), ybuf.Length);
				Array.Copy(zbuf, 0, buffer, 1 + (sizeof(int)*2) + (sizeof(float) * 2), zbuf.Length);

				Net.networkController.sendBytes(buffer);

			}
		}
	}
}
