using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[RequireComponent(typeof(NetworkIdentity))]
public class SyncTransform : MonoBehaviour {

	NetworkIdentity ident;

	public bool LocalClientAuthority = false;
	public bool Lerp = true;
	public bool position = true;
	public bool rotation = false;
	public bool scale = false;

	[Range(1, 50)]
	public float ups = 10;

	[Range(1, 10)]
	public float Importance = 5;

	[Range(1, 10)]
	public float LerpSmoothness = 5;

	private float counter = 0;

	private Vector3 postarget;
	private Vector3 rottarget;
	private Vector3 scltarget;

	void Start() {
		ident = GetComponent<NetworkIdentity>();

		postarget = transform.position;
		rottarget = transform.eulerAngles;
		scltarget = transform.localScale;

	}
	
	
	void FixedUpdate() {

		if (ident.active()) {

			counter += Time.deltaTime;

			if (counter >= 1 / ups) {
				counter = 0;
				if (!LocalClientAuthority) {
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
				} else if (ident.MyPlayer) {
					//send
					if (position) {
						Net.SendTransform(NetType.TranslationLoc, ident.id, transform.position, gameObject.name);
					}

					if (rotation) {
						Net.SendTransform(NetType.RotationLoc, ident.id, transform.eulerAngles, gameObject.name);
					}

					if (scale) {
						Net.SendTransform(NetType.ScaleLoc, ident.id, transform.localScale, gameObject.name);
					}
				}
			}

			

			//recieve
			process();

			if (!ident.MyPlayer) {
				if (Lerp) {

					transform.position = MathUtil.Lerp(transform.position, postarget, Time.fixedDeltaTime * (11-LerpSmoothness));
					transform.eulerAngles = MathUtil.Lerp(transform.eulerAngles, rottarget, Time.fixedDeltaTime * (11 - LerpSmoothness));
					transform.localScale = MathUtil.Lerp(transform.localScale, scltarget, Time.fixedDeltaTime * (11 - LerpSmoothness));

				} else {
					transform.position = postarget;
					transform.eulerAngles = rottarget;
					transform.localScale = scltarget;
				}
			}


		}

	}

	private void process() {
		List<byte[]> queue = new List<byte[]>();

		for (int i = 0, c = 0; i < ident.queue.Count || c > Importance; ++i) {
			if ((ident.queue[i][0] == (byte)NetType.Translation && position) || 
				(ident.queue[i][0] == (byte)NetType.Rotation && rotation) || 
				(ident.queue[i][0] == (byte)NetType.Scale && scale) ||
				(ident.queue[i][0] == (byte)NetType.TranslationLoc && position) || 
				(ident.queue[i][0] == (byte)NetType.RotationLoc && rotation) || 
				(ident.queue[i][0] == (byte)NetType.ScaleLoc && scale)) {
				++c;
				queue.Add(ident.queue[i]);
				ident.queue.RemoveAt(i);
			}
		}

		for (int i = 0; i < queue.Count; ++i) {
			if (LocalClientAuthority) {
				switch (queue[i][0]) {
					case (byte)NetType.TranslationLoc:
						RecievePos(queue[i]);
						break;
					case (byte)NetType.RotationLoc:
						RecieveRot(queue[i]);
						break;
					case (byte)NetType.ScaleLoc:
						RecieveScl(queue[i]);
						break;
				}
			} else {
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

	}

	private void RecievePos(byte[] buf) {

		float x = BitConverter.ToSingle(buf, 1 + sizeof(int));
		float y = BitConverter.ToSingle(buf, 1 + sizeof(int) + sizeof(float));
		float z = BitConverter.ToSingle(buf, 1 + sizeof(int) + (sizeof(float)*2));

		if (!ident.MyPlayer)
			postarget = new Vector3(x, y, z);
		else
			transform.position = new Vector3(x, y, z);

	}

	private void RecieveRot(byte[] buf) {

		float x = BitConverter.ToSingle(buf, 1 + sizeof(int));
		float y = BitConverter.ToSingle(buf, 1 + sizeof(int) + sizeof(float));
		float z = BitConverter.ToSingle(buf, 1 + sizeof(int) + (sizeof(float) * 2));

		if (!ident.MyPlayer)
			rottarget = new Vector3(x, y, z);
		else
			transform.eulerAngles = new Vector3(x, y, z);

	}

	private void RecieveScl(byte[] buf) {

		float x = BitConverter.ToSingle(buf, 1 + sizeof(int));
		float y = BitConverter.ToSingle(buf, 1 + sizeof(int) + sizeof(float));
		float z = BitConverter.ToSingle(buf, 1 + sizeof(int) + (sizeof(float) * 2));

		if (!ident.MyPlayer)
			scltarget = new Vector3(x, y, z);
		else
			transform.localScale = new Vector3(x, y, z);

	}

}
