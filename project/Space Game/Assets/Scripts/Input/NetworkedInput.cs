using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum InputType : byte {
	forward,
	backward,
	left,
	right,
	jump
}

public class NetworkedInput : MonoBehaviour {

	[Range(1, 50)]
	public float ups = 5;

	private float counter = 0;

	public List<byte[]> queue = new List<byte[]>();

	Dictionary<InputType, bool> KeyVals = new Dictionary<InputType, bool>() {
		{ InputType.forward, false },
		{ InputType.backward, false },
		{ InputType.left, false },
		{ InputType.right, false },
		{ InputType.jump, false }
	};
	Dictionary<InputType, KeyCode> KeyBindings = new Dictionary<InputType, KeyCode>() {
		{ InputType.forward, KeyCode.W },
		{ InputType.backward, KeyCode.S },
		{ InputType.left, KeyCode.A },
		{ InputType.right, KeyCode.D },
		{ InputType.jump, KeyCode.Space }
	};

	Dictionary<int, Dictionary<InputType, bool>> OtherPlayers;

	void Start() {
		Net.networkInput = this;
		OtherPlayers = new Dictionary<int, Dictionary<InputType, bool>>();
	}

	public void AddPlayer(int id) {
		if (!OtherPlayers.ContainsKey(id)) {
			OtherPlayers.Add(id, new Dictionary<InputType, bool>() {
				{ InputType.forward, false },
				{ InputType.backward, false },
				{ InputType.left, false },
				{ InputType.right, false },
				{ InputType.jump, false }
			});
		}
	}


	void Update() {
		counter += Time.deltaTime;

		//update key values
		foreach (KeyValuePair<InputType, KeyCode> entry in KeyBindings) {
			KeyVals[entry.Key] = Input.GetKey(entry.Value);
		}

		if (counter >= 1 / ups) {
			counter = 0;

			foreach (KeyValuePair<InputType, bool> entry in KeyVals) {

				byte[] buffer = new byte[sizeof(int) + 3];
				buffer[0] = (byte)NetType.Control;
				buffer[1+sizeof(int)] = (byte)entry.Key;
				if(entry.Value)
					buffer[sizeof(int) + 2] = 1;
				else
					buffer[sizeof(int) + 2] = 0;

				buffer[1] = 0;
				buffer[2] = 0;
				buffer[3] = 0;
				buffer[4] = 0;

				Net.networkController.sendBytes(buffer);

			}

		}

		if (Net.networkController.getHost()) {
			foreach (byte[] buffer in queue) {
				int id = BitConverter.ToInt32(buffer, 1);
				AddPlayer(id);
				OtherPlayers[id][(InputType)buffer[1 + sizeof(int)]] = buffer[2 + sizeof(int)] != 0;
			}
			queue.Clear();
		}

	}

	public bool NetworkedKeyDown(InputType type, bool MyPlayer, int id = 0) {

		if (MyPlayer) {
			return KeyVals[type];
		}

		if (Net.networkController.getHost()) {
			return OtherPlayers[id][type];
		}

		return false;
	}

}
