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

	void Start() {
		Net.networkInput = this;
	}

	void Update() {

		//update key values
		foreach (KeyValuePair<InputType, KeyCode> entry in KeyBindings) {
			KeyVals[entry.Key] = Input.GetKey(entry.Value);
		}

	}

	public bool NetworkedKeyDown(InputType type, bool MyPlayer) {

		if (MyPlayer) {
			return KeyVals[type];
		}

		return false;
	}

}
