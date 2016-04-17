using UnityEngine;
using System.Collections;
using Prime31;

public class Patrol : MonoBehaviour {


	public float health = 50;

	public Transform[] patrolPoints;
	public Transform target;
	public float aggroRange = 15f;
	public float moveSpeed = 6f;
	public float gravity = -25f;
	public bool knockedBack = false;
	public bool canTakeDamage = true;
	float lastHitTime;
	Vector3 knockDir = Vector3.zero;

	public float knockBackTime = 0.5f;
	public float knockBackMagnitude;

	public Transform currPoint;
	private Vector3 moveVector;

	private Vector3 _velocity;
	private CharacterController2D _controller;

	// Use this for initialization
	void Awake () {
		_controller = GetComponent<CharacterController2D>();
		currPoint = patrolPoints [Random.Range (0, patrolPoints.Length)];
		_controller.onControllerCollidedEvent += onControllerCollider;
	}

	void onControllerCollider (RaycastHit2D hit)
	{		
		if (hit.collider.tag == "PlayerSword") {
			if(canTakeDamage)
				TakeDamage (hit.collider.transform.parent.GetComponent<PlayerController> ().currDamage, hit.collider.transform);
		}
	}
	
	void Update () {

		if (health < 0)
			Destroy (this.gameObject);

		moveVector =  currPoint.position - transform.position ;

		if (moveVector.normalized.x < 0) {
			GetComponent<SpriteRenderer> ().flipX = true;
		} else {
			GetComponent<SpriteRenderer> ().flipX = false;
		}

		if (!knockedBack) {
			
			if (Vector2.Distance (transform.position, currPoint.position) > 0.25f) {
				_velocity.x = Mathf.Lerp (_velocity.x, moveVector.normalized.x * moveSpeed, Time.deltaTime);
			} else {
				currPoint = patrolPoints [Random.Range (0, patrolPoints.Length)];

			}
		}
		HandleKnockBack ();

		_velocity.y += gravity * Time.deltaTime;
		_controller.move (_velocity * Time.deltaTime);
	}


	void TakeDamage(float damage, Transform damagedBy){
			
			health -= damage;
		StartCoroutine (damageTimer());
			StartCoroutine( KnockBack (damagedBy));
	}


	IEnumerator damageTimer(){
		canTakeDamage = false;
		yield return new WaitForSeconds (0.5f);
		canTakeDamage = true;
	}


	void HandleKnockBack(){
		if( knockedBack == true)
		{

			_velocity += knockDir.normalized * knockBackMagnitude;
			if (Time.time > lastHitTime + knockBackTime) {
				knockedBack = false;
				CancelInvoke ("FlashSprite");
				GetComponent<SpriteRenderer> ().enabled = true;

			}
			knockDir = Vector3.zero;
		}

	}

	void FlashSprite(){
		GetComponent<SpriteRenderer> ().enabled = !GetComponent<SpriteRenderer> ().enabled;
	}

	public IEnumerator KnockBack (Transform knockedBy)
	{
		InvokeRepeating ("FlashSprite",0,0.1f);

		lastHitTime = Time.time;
		knockDir = transform.position - knockedBy.position;
		knockedBack = true;
		Debug.Log (knockDir.normalized);
		yield return null;
	}




}
