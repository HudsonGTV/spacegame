using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityPlayer : MonoBehaviour {

	void Start() {

	}

	void Update() {

		// END GAME KEY
		if(Input.GetKey("escape")) {
			#if UNITY_EDITOR
				UnityEditor.EditorApplication.isPlaying = false;
			#else
				Application.Quit();
			#endif
		}

	}

}
