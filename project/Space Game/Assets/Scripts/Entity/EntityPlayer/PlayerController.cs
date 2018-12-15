using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PlayerController : MonoBehaviour {

	public float Speed = 25.0f;
	public float JumpHeight = 5.0f;

	void Start() {

	}

	void Update() {

		// Movement
		var x = Input.GetAxis("Horizontal") * Time.deltaTime * Speed;
		var z = Input.GetAxis("Vertical") * Time.deltaTime * Speed;

		// Jumping
		var y = Input.GetAxis("Jump") * Time.deltaTime * JumpHeight;

		// Movement/Jumping
		transform.Translate(x, 0, 0);
		transform.Translate(0, y, 0);
		transform.Translate(0, 0, z);

	}

}
