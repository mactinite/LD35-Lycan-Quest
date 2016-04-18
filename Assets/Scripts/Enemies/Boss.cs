using UnityEngine;
using System.Collections;
using Prime31;

public class Boss : MonoBehaviour
{


	public float health = 200;

	public enum behaviorState
	{
		PATROLLING,
		AGGRO,
		ATTACKING,
		STOP

	}

	public behaviorState currBehavior = behaviorState.PATROLLING;

	public Transform projectile;
	public Transform ProjectilePos;
	public float attackTime = 3;
	bool attacking = false;
	float attackTimer = 0f;

	public Sprite dangerSprite;
	public Sprite safeSprite;

	public AudioClip atkSound;
	public Transform dieFX;
	public Transform dropOnDie;
	public BoxCollider2D damageBox;

	public Transform[] patrolPoints;
	public Transform target;
	public float moveSpeed = 3f;
	public float gravity = -25f;
	public bool knockedBack = false;
	public bool canTakeDamage = true;
	float lastHitTime;
	public float pauseDelaymin = 3;
	public float pauseDelaymax = 6;
	float pauseDelay;
	float pauseDelayTimer;
	public float pauseTimemin = 3;
	public float pauseTimemax = 6;
	float pauseTime;
	float pauseTimeTimer;

	Vector3 knockDir = Vector3.zero;

	public float knockBackTime = 0.5f;
	public float knockBackMagnitude = 0.25f;

	public Transform currPoint;
	private Vector3 moveVector;

	private Vector3 _velocity;
	private CharacterController2D _controller;
	bool jumped = false;
	float lastHitDamage;

	// Use this for initialization
	void Awake ()
	{
		_controller = GetComponent<CharacterController2D> ();
		currPoint = patrolPoints [Random.Range (0, patrolPoints.Length)];
		_controller.onTriggerEnterEvent += onTriggerEnter;
	}

	void onTriggerEnter (Collider2D col)
	{

		if (col.tag == "PlayerSword") {
			if (canTakeDamage)
				TakeDamage (col.transform.parent.GetComponent<PlayerController> ().currDamage, col.transform);
		}
	}

	void Update ()
	{
		if (health < 0) {
			Destroy (this.gameObject);
			Instantiate (dieFX,transform.position,Quaternion.identity);
			if(dropOnDie != null)
				Instantiate (dropOnDie,transform.position,Quaternion.identity);
		}
		Behavior ();
		HandleKnockBack ();

		_velocity.y += gravity * Time.deltaTime;
		_controller.move (_velocity * Time.deltaTime);
	}


	void Behavior ()
	{

		if (_controller.isGrounded)
			_velocity.y = 0;
		
		if (attackTimer < attackTime && !attacking && currBehavior != behaviorState.STOP) {
			attackTimer += Time.deltaTime;
		} else if (attackTimer > attackTime && !attacking && currBehavior != behaviorState.STOP) {
			attackTimer = 0;
			currBehavior = behaviorState.ATTACKING;
		}

		if (pauseDelayTimer < pauseDelay && currBehavior != behaviorState.STOP ) {
			GetComponent<SpriteRenderer> ().color = Color.Lerp (Color.white, Color.gray, (pauseDelayTimer / pauseDelay) * 1.0f);
			pauseDelayTimer += Time.deltaTime;
		} else {
			currBehavior = behaviorState.STOP;
			pauseDelayTimer = 0;
			pauseDelay = Random.Range (pauseDelaymin, pauseDelaymax);
		}



		if (jumped = true && _controller.isGrounded) {
			jumped = false;
		}


		if (!knockedBack) {
			switch (currBehavior) {
			case behaviorState.PATROLLING:
				Patrolling ();
				break;
			case behaviorState.ATTACKING:
				Attack ();
				break;
			case behaviorState.STOP:
				Stopped ();
				break;
			default:
				Patrolling ();
				break;
			}

		}

		//Handle direction facing based onmovement vector
		if (moveVector.normalized.x < 0) {
			GetComponent<SpriteRenderer> ().flipX = true;
		} else {
			GetComponent<SpriteRenderer> ().flipX = false;
		}


	}

	void Stopped(){
		damageBox.enabled = false;
		canTakeDamage = true;
		GetComponent<SpriteRenderer> ().sprite = safeSprite;
		if (pauseTimeTimer < pauseTime) {
			pauseTimeTimer += Time.deltaTime;
			GetComponent<SpriteRenderer> ().color = Color.Lerp (Color.gray, Color.white, (pauseTimeTimer / pauseTime) * 1.0f);
		} else {
			
			currBehavior = behaviorState.PATROLLING;
			pauseTime = Random.Range (pauseTimemin, pauseTimemax);
			pauseTimeTimer = 0;
			GetComponent<SpriteRenderer> ().sprite = dangerSprite;
			GetComponent<SpriteRenderer> ().color = Color.white;
			damageBox.enabled = true;
		}
		moveVector.x = 0;
		_velocity.x = 0;
	}

	void Patrolling ()
	{
		canTakeDamage = false;
		moveVector = currPoint.position - transform.position;

		if (Vector2.Distance (transform.position, currPoint.position) > 0.5f) {
			_velocity.x = Mathf.Lerp (_velocity.x, moveVector.normalized.x * moveSpeed / 2, Time.deltaTime * 20f);
		} else {
			currPoint = patrolPoints [Random.Range (0, patrolPoints.Length)];
		}
	}
		

	void Attack ()
	{
		if (attacking == false) {
			StartCoroutine (attackDelay ());
			GetComponent<AudioSource> ().PlayOneShot (atkSound);
			Transform obj = Instantiate (projectile, ProjectilePos.position, Quaternion.identity) as Transform;
			Transform obj2 = Instantiate (projectile, ProjectilePos.position, Quaternion.identity) as Transform;
			obj.GetComponent<SpriteRenderer> ().flipX = true;
			obj.GetComponent<Projectile> ().platforms = _controller.platformMask;

			obj2.GetComponent<SpriteRenderer> ().flipX = false;
			obj2.GetComponent<Projectile> ().platforms = _controller.platformMask;
			 
		}
	}


	void TakeDamage (float damage, Transform damagedBy)
	{

		health -= damage;
		StartCoroutine (damageTimer ());
		StartCoroutine (KnockBack (damagedBy, damage));
	}

	IEnumerator attackDelay ()
	{
		attacking = true;
		attackTimer = 0;
		currBehavior = behaviorState.ATTACKING;
		yield return new WaitForSeconds (0.5f);
		attacking = false;
		currBehavior = behaviorState.PATROLLING;
	}


	IEnumerator damageTimer ()
	{
		canTakeDamage = false;
		yield return new WaitForSeconds (0.5f);
		canTakeDamage = true;
	}


	void HandleKnockBack ()
	{
		if (knockedBack == true) {
			Vector3 kb = knockDir.normalized * knockBackMagnitude * lastHitDamage;
			kb.z = 0;
			_velocity += kb;
			if (Time.time > lastHitTime + knockBackTime) {
				knockedBack = false;
				CancelInvoke ("FlashSprite");
				GetComponent<SpriteRenderer> ().enabled = true;
				lastHitDamage = 0;
			}
			knockDir = Vector3.zero;
		}

	}



	void FlashSprite ()
	{
		GetComponent<SpriteRenderer> ().enabled = !GetComponent<SpriteRenderer> ().enabled;
	}

	public IEnumerator KnockBack (Transform knockedBy, float magnitude)
	{

		lastHitTime = Time.time;
		lastHitDamage = magnitude;
		knockDir = transform.position - knockedBy.position;
		knockedBack = true;
		Debug.Log (knockDir.normalized);
		yield return null;
	}




}
