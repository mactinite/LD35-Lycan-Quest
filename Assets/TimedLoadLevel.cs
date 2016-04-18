using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
public class TimedLoadLevel : MonoBehaviour {
	
	// Use this for initialization
	void Start () {
		
	}

	public void loadMainMenu(){
		SceneManager.LoadScene (0);
	}

}
