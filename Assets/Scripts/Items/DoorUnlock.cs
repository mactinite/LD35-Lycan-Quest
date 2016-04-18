using UnityEngine;
using System.Collections;

public class DoorUnlock : MonoBehaviour {

	public Transform doorObj;

	public void OpenDoor(){
		Destroy (doorObj.gameObject);
	}


}
