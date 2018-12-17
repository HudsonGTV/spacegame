using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;

public enum NetType : byte {
	Chat,
	Translation,
	Rotation,
	Scale,
	Sync,
	Control
}

public class Net {

	public static NetworkController networkController;
	public static NetworkedInput networkInput;


	public static void SendChatMSG(string msg) {
		networkController.sendString(msg);
	}

	public static bool SendTransform(NetType type, int id, float x, float y, float z, string name) {
		byte[] buffer = new byte[1 + (sizeof(float) * 3) + sizeof(int) + (name.Length * sizeof(char))];
		byte[] bufid = BitConverter.GetBytes(id);
		byte[] bufx = BitConverter.GetBytes(x);
		byte[] bufy = BitConverter.GetBytes(y);
		byte[] bufz = BitConverter.GetBytes(z);
		byte[] bname = Encoding.Unicode.GetBytes(name);

		Array.Copy(bufid, 0, buffer, 1,                                     bufid.Length);
		Array.Copy(bufx , 0, buffer, sizeof(int) + 1,                       bufx.Length );
		Array.Copy(bufy , 0, buffer, sizeof(float) + 1 + sizeof(int),       bufx.Length );
		Array.Copy(bufz , 0, buffer, (sizeof(float)*2) + 1 + sizeof(int),   bufx.Length );
		Array.Copy(bname, 0, buffer, (sizeof(float) * 3) + 1 + sizeof(int), bufx.Length );

		buffer[0] = (byte)type;

		networkController.sendBytes(buffer);

		return true;
	}

	public static bool SendTransform(NetType type, int id, Vector3 vec, string name) {
		return SendTransform(type, id, vec.x, vec.y, vec.z, name);
	}

}
