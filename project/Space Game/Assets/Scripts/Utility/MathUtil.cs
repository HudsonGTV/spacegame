using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MathUtil {

	public static Vector3 Lerp(Vector3 start, Vector3 end, float time) {

		float x = Mathf.Lerp(start.x, end.x, time);
		float y = Mathf.Lerp(start.y, end.y, time);
		float z = Mathf.Lerp(start.z, end.z, time);

		return new Vector3(x, y, z);

	}

}
