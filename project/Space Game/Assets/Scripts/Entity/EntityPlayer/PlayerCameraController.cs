using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(NetworkIdentity))]
public class PlayerCameraController : MonoBehaviour {

	public float Sensitivity = 100.0f;

	public float Pitch = 0.0f;
	public float Yaw = 0.0f;

	NetworkIdentity ident;

	private bool IsCursorLocked = true;

	void Start() {
		ident = GetComponent<NetworkIdentity>();
	}

	void Update() {

		if (!ident.MyPlayer) {
			transform.GetChild(0).GetComponent<Camera>().enabled = false;
			return;
		}

		// LOCK CURSOR
		if(Input.GetKeyDown(KeyCode.Tab)) {
			IsCursorLocked = !IsCursorLocked;
		}

		if(IsCursorLocked)
			Cursor.lockState = CursorLockMode.Locked;
		else
			Cursor.lockState = CursorLockMode.None;

		// CAMERA MOVEMENT
		float mouseX = Input.GetAxis("Mouse X");
		float mouseY = Input.GetAxis("Mouse Y");

		Yaw += mouseX * Time.deltaTime * Sensitivity;
		Pitch -= mouseY * Time.deltaTime * Sensitivity;

		transform.eulerAngles = new Vector3(0.0f, Yaw, 0.0f);
		transform.GetChild(0).eulerAngles = new Vector3(Pitch, Yaw, 0.0f);

	}

}
