using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

	public float Speed = 25.0f;
	public float JumpHeight = 5.0f;

	void Start() {

	}

	void Update() {

		// MOVEMENT
		var x = Input.GetAxis("Horizontal") * Time.deltaTime * Speed;
		var z = Input.GetAxis("Vertical") * Time.deltaTime * Speed;

		// JUMPING
		var y = Input.GetAxis("Jump") * Time.deltaTime * JumpHeight;

		// MOVEMENT/JUMPING
		transform.Translate(x, 0, 0);
		transform.Translate(0, y, 0);
		transform.Translate(0, 0, z);

	}

}
