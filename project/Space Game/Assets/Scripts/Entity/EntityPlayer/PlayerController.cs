using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour {

	public float Speed = 25.0f;
	public float JumpHeight = 5.0f;

	private bool isMoving = true;

	private Rigidbody rb;
	private PlayerCameraController pcc;

	void Start() {

		rb = GetComponent<Rigidbody>();
		pcc = GetComponent<PlayerCameraController>();

	}

	void Update() {

		Vector3 velocity = new Vector3(0.0f, 0.0f, 0.0f);

		float angle = Mathf.Deg2Rad * (-pcc.Yaw + 90.0f);

		// INPUT
		if(Input.GetKey(KeyCode.W))
			velocity += AngleToVec3(angle);
		else if(Input.GetKey(KeyCode.S))
			velocity -= AngleToVec3(angle);
		if(Input.GetKey(KeyCode.A))
			velocity += AngleToVec3(angle + 1.5708f);
		else if(Input.GetKey(KeyCode.D))
			velocity -= AngleToVec3(angle + 1.5708f);

		if(isMoving) {

			// UPDATE VELOCITY
			rb.velocity = new Vector3(
				velocity.x * Speed, 
				rb.velocity.y, 
				velocity.z * Speed
			);

		}

	}

	static Vector3 AngleToVec3(float angle) {
		return new Vector3(Mathf.Cos(angle), 0.0f, Mathf.Sin(angle));
	}

}
