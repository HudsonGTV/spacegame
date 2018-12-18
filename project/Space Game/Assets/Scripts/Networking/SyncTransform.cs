using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[RequireComponent(typeof(NetworkIdentity))]
public class SyncTransform : MonoBehaviour {

	NetworkIdentity ident;

	public bool position = true;
	public bool rotation = false;
	public bool scale = false;

	[Range(1, 50)]
	public float ups = 10;

	[Range(1, 10)]
	public float Importance = 5;

	private float counter = 0;

	void Start() {
		ident = GetComponent<NetworkIdentity>();
	}
	
	
	void FixedUpdate() {

		if (ident.active()) {

			counter += Time.deltaTime;

			if (counter >= 1 / ups) {
				counter = 0;
				//send
				if (position) {
					Net.SendTransform(NetType.Translation, ident.id, transform.position, gameObject.name);
				}

				if (rotation) {
					Net.SendTransform(NetType.Rotation, ident.id, transform.eulerAngles, gameObject.name);
				}

				if (scale) {
					Net.SendTransform(NetType.Scale, ident.id, transform.localScale, gameObject.name);
				}
			}

			//recieve
			for (int i = 0; i < Importance; ++i) {
				process();
			}
		}

	}

	private void process() {
		List<byte[]> queue = new List<byte[]>();

		for (int i = 0, c = 0; i < ident.queue.Count || c > Importance; ++i) {
			if ((ident.queue[i][0] == (byte)NetType.Translation && position) || (ident.queue[i][0] == (byte)NetType.Rotation && rotation) || (ident.queue[i][0] == (byte)NetType.Scale && scale)) {
				++c;
				queue.Add(ident.queue[i]);
				ident.queue.RemoveAt(i);
			}
		}

		for (int i = 0; i < queue.Count; ++i) {
			switch (queue[i][0]) {
				case (byte)NetType.Translation:
					RecievePos(queue[i]);
					break;
				case (byte)NetType.Rotation:
					RecieveRot(queue[i]);
					break;
				case (byte)NetType.Scale:
					RecieveScl(queue[i]);
					break;
			}
		}

	}

	private void RecievePos(byte[] buf) {

		float x = BitConverter.ToSingle(buf, 1 + sizeof(int));
		float y = BitConverter.ToSingle(buf, 1 + sizeof(int) + sizeof(float));
		float z = BitConverter.ToSingle(buf, 1 + sizeof(int) + (sizeof(float)*2));

		transform.position = new Vector3(x, y, z);

	}

	private void RecieveRot(byte[] buf) {

		float x = BitConverter.ToSingle(buf, 1 + sizeof(int));
		float y = BitConverter.ToSingle(buf, 1 + sizeof(int) + sizeof(float));
		float z = BitConverter.ToSingle(buf, 1 + sizeof(int) + (sizeof(float) * 2));

		transform.eulerAngles = new Vector3(x, y, z);

	}

	private void RecieveScl(byte[] buf) {

		float x = BitConverter.ToSingle(buf, 1 + sizeof(int));
		float y = BitConverter.ToSingle(buf, 1 + sizeof(int) + sizeof(float));
		float z = BitConverter.ToSingle(buf, 1 + sizeof(int) + (sizeof(float) * 2));

		transform.localScale = new Vector3(x, y, z);

	}

}
