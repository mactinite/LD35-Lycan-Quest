using UnityEngine;
using System.Collections;

public class SmoothFollow : MonoBehaviour {

	public float dampening = 25.0f;
	public Transform target;

	private Vector3 targetPos;
	// Use this for initialization
	void Start () {
		targetPos = transform.position;
	}
	
	// Update is called once per frame
	void Update () {
	
		targetPos.x = Mathf.Lerp (targetPos.x, target.position.x, Time.deltaTime * dampening);
		targetPos.y = Mathf.Lerp (targetPos.y, target.position.y, Time.deltaTime * dampening);

		transform.position = targetPos;

	}
}
