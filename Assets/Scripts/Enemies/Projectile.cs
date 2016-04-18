using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour {

	Vector3 moveVector;
	public LayerMask platforms;
	// Update is called once per frame
	void Start(){
		if (GetComponent<SpriteRenderer> ().flipX == true) {
			moveVector = Vector3.right;
		} else {
			moveVector = -Vector3.right;
		}



	}
	void Update () {
		transform.Translate (moveVector * 5 * Time.deltaTime);

		RaycastHit2D hit = Physics2D.Raycast (transform.position, Vector2.down, Mathf.Infinity,platforms);
		transform.position = new Vector3(transform.position.x, hit.point.y,0);
		
	}

	public void DestroyThis(){

		Destroy (this.gameObject);
	}
}
