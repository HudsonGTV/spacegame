using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NetworkIdentity))]
[RequireComponent(typeof(PlayerCameraController))]
public class PlayerController : MonoBehaviour {

	public float Speed = 25.0f;
	public float JumpHeight = 5.0f;

	private bool isMoving = true;

	private Rigidbody rb;
	private PlayerCameraController pcc;
	private NetworkIdentity netIdent;


	void Start() {

		rb = GetComponent<Rigidbody>();
		pcc = GetComponent<PlayerCameraController>();
		netIdent = GetComponent<NetworkIdentity>();

	}

	void Update() {

		Vector3 velocity = new Vector3(0.0f, 0.0f, 0.0f);

		float angle = Mathf.Deg2Rad * (-pcc.Yaw + 90.0f);

		// INPUT
		if(Net.networkInput.NetworkedKeyDown(InputType.forward, netIdent.MyPlayer))
			velocity += AngleToVec3(angle);
		if(Net.networkInput.NetworkedKeyDown(InputType.backward, netIdent.MyPlayer))
			velocity -= AngleToVec3(angle);
		if(Net.networkInput.NetworkedKeyDown(InputType.left, netIdent.MyPlayer))
			velocity += AngleToVec3(angle + 1.5708f);
		if(Net.networkInput.NetworkedKeyDown(InputType.right, netIdent.MyPlayer))
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
