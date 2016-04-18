using UnityEngine;
using System.Collections;
using Prime31;

public class WolfEnemy : MonoBehaviour {


	public float health = 50;

	public enum behaviorState
	{
		PATROLLING,
		AGGRO,
		ATTACKING

	}

	public behaviorState currBehavior = behaviorState.PATROLLING;
	public AudioClip atkSound;
	public Transform dropOnDie;

	public float attackTime = 3;
	bool attacking = false;
	float attackTimer = 0f;


	public Transform[] patrolPoints;
	public Transform target;
	public float aggroRange = 15f;
	public float aggroFalloff = 5f;
	private float aggroTimer = 0;
	public float moveSpeed = 6f;
	public float gravity = -25f;
	public bool knockedBack = false;
	public bool canTakeDamage = true;
	float lastHitTime;
	Vector3 knockDir = Vector3.zero;

	public float knockBackTime = 0.5f;
	public float knockBackMagnitude;
	public Animator anim;

	public Transform dieFX;


	public Transform currPoint;
	private Vector3 moveVector;

	private Vector3 _velocity;
	private CharacterController2D _controller;
	bool jumped = false;
	float lastHitDamage;

	public ParticleSystem ps;
	new public AudioSource audio;
	        
	// Use this for initialization
	void Awake () {
		anim = GetComponent<Animator> ();
		audio = GetComponent<AudioSource> ();
		_controller = GetComponent<CharacterController2D>();
		currPoint = patrolPoints [Random.Range (0, patrolPoints.Length)];
		_controller.onTriggerEnterEvent += onTriggerEnter;
	}

	void onTriggerEnter (Collider2D col)
	{

		if (col.tag == "PlayerSword") {
			if(canTakeDamage)
				TakeDamage (col.transform.parent.GetComponent<PlayerController> ().currDamage, col.transform);
		}
	}
	
	void Update () {

		if (health < 0) {
			if(dropOnDie != null)
				Instantiate (dropOnDie,transform.position,Quaternion.identity);
			Instantiate (dieFX,transform.position,Quaternion.identity);
			Destroy (this.gameObject);
		}

		Behavior ();
		HandleKnockBack ();

		_velocity.y += gravity * Time.deltaTime;
		_controller.move (_velocity * Time.deltaTime);
	}


	void Behavior(){

		if (_controller.isGrounded)
			_velocity.y = 0;

		if (Vector2.Distance (transform.position, target.position) < aggroRange) {

			currBehavior = behaviorState.AGGRO;
		} 


		if (Vector2.Distance (transform.position, target.position) < aggroRange/2) {
			if (attackTimer < attackTime && !attacking) {
				attackTimer += Time.deltaTime;
			} else if (attackTimer > attackTime && !attacking) {
				attackTimer = 0;
				currBehavior = behaviorState.ATTACKING;
			}
		}

		else if(Vector2.Distance (transform.position, target.position) < aggroRange + 5 && currBehavior == behaviorState.AGGRO && !attacking) {// Must leave further than the edge of the aggro range to get away from aggro.
			if (aggroTimer >= aggroFalloff) {
				currBehavior = behaviorState.PATROLLING;
			} else {
				aggroTimer += Time.deltaTime;
			}
		}

		if ((_controller.collisionState.left || _controller.collisionState.right) && _controller.isGrounded && !jumped) {
			if (moveVector.y >= -0.1f) {
				jumped = true;
				_velocity.y = Mathf.Sqrt (3f - gravity);
			}
		}

		if (jumped = true && _controller.isGrounded) {
			jumped = false;
		}


		if (!knockedBack) {
			switch (currBehavior){
				case behaviorState.PATROLLING:
					Patrolling ();
					break;
				case behaviorState.AGGRO:
					Aggro ();
					break;
				case behaviorState.ATTACKING:
					Attack ();
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

	void Patrolling(){

		moveVector =  currPoint.position - transform.position ;
		anim.Play (Animator.StringToHash ("WOLF_RUN"));
		if (Vector2.Distance (transform.position, currPoint.position) > 0.5f) {
			_velocity.x = Mathf.Lerp (_velocity.x, moveVector.normalized.x * moveSpeed/2, Time.deltaTime * 20f);
		} else {
			currPoint = patrolPoints [Random.Range (0, patrolPoints.Length)];

		}
	}

	void Aggro(){
		
		moveVector = target.position -transform.position;
		if (Vector2.Distance (transform.position, target.position) > 1f) {
			anim.Play (Animator.StringToHash ("WOLF_RUN"));
			_velocity.x = Mathf.Lerp (_velocity.x, moveVector.normalized.x * moveSpeed, Time.deltaTime * 20f);
		} else {
			anim.Play (Animator.StringToHash ("WOLF_IDLE"));
			_velocity.x = 0;
		}
			

	}

	void Attack(){
		if (attacking == false) {
			anim.Play (Animator.StringToHash ("WOLF_ATTACK"));
			GetComponent<AudioSource> ().PlayOneShot (atkSound);

			jumped = true;
			_velocity.x += (moveVector.x * moveSpeed * Mathf.Sqrt (2f * 20f));
			_velocity.y = Mathf.Sqrt (2f * 1f * -gravity);
			StartCoroutine (attackDelay ());
		}
	}


	void TakeDamage(float damage, Transform damagedBy){
			
		health -= damage;
		StartCoroutine (damageTimer());
		StartCoroutine( KnockBack(damagedBy, damage));
	}

	IEnumerator attackDelay(){
		attacking = true;
		attackTimer = 0;
		currBehavior = behaviorState.ATTACKING;
		yield return new WaitForSeconds (0.5f);
		attacking = false;
		currBehavior = behaviorState.PATROLLING;
	}


	IEnumerator damageTimer(){
		canTakeDamage = false;
		yield return new WaitForSeconds (0.5f);
		canTakeDamage = true;
	}


	void HandleKnockBack(){
		if( knockedBack == true)
		{
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

	void FlashSprite(){
		GetComponent<SpriteRenderer> ().enabled = !GetComponent<SpriteRenderer> ().enabled;
	}

	public IEnumerator KnockBack (Transform knockedBy, float magnitude)
	{
		InvokeRepeating ("FlashSprite",0,0.1f);

		lastHitTime = Time.time;
		lastHitDamage = magnitude;
		knockDir = transform.position - knockedBy.position;
		knockedBack = true;
		Debug.Log (knockDir.normalized);
		yield return null;
	}

	public IEnumerator triggerParticles (float delay)
	{
		yield return new WaitForSeconds (delay);
		ps.Emit (10);
	}



}
