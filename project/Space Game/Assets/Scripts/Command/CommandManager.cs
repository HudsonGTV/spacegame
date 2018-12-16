using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CommandManager : MonoBehaviour {

	public GameObject EntityPlayer;

	private InputField inputField;

	void Start() {
		inputField = GetComponent<InputField>();
	}

	void Update() {

		if(Input.GetKey(KeyCode.Return)) {
			inputField.ActivateInputField();
		}

		if(inputField.text.ToLower() == "spawn") {

			EntityPlayer.transform.position = new Vector3(0.0f, 0.0f, 0.0f);

			inputField.text = "";
			inputField.DeactivateInputField();

		}

	}



}
