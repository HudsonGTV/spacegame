using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkIdentity : MonoBehaviour {

	public int id;

	public List<byte[]> queue = new List<byte[]>();

	void Start() {
		NetworkController.ids.Add(this);
	}
	
	void Update() {
		
	}
}
