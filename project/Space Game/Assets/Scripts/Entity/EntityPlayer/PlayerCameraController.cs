using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCameraController : MonoBehaviour {

	public float Sensitivity = 100.0f;

	public float Pitch = 0.0f;
	public float Yaw = 0.0f;

	void Start() {

	}

	void Update() {

		// LOCK CURSOR
		Cursor.lockState = CursorLockMode.Locked;

		// CAMERA MOVEMENT
		float mouseX = Input.GetAxis("Mouse X");
		float mouseY = Input.GetAxis("Mouse Y");

		Yaw += mouseX * Time.deltaTime * Sensitivity;
		Pitch -= mouseY * Time.deltaTime * Sensitivity;

		//transform.eulerAngles = new Vector3(0.0f, Yaw, 0.0f);
		transform.GetChild(0).eulerAngles = new Vector3(Pitch, Yaw, 0.0f);

	}

}
