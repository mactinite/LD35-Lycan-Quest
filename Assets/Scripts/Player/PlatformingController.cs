using UnityEngine;
using System.Collections;
using Prime31;


public class PlatformingController : MonoBehaviour
{
	public bool canMove = true;
	private bool jumped = false;
	// movement config
	public float gravity = -25f;
	public float runSpeed = 8f;
	public float groundDamping = 20f;
	// how fast do we change direction? higher means faster
	public float inAirDamping = 5f;
	public float jumpHeight = 3f;

	private bool attacking = false;
	private bool shapeShifting = false;
	[HideInInspector]
	public bool knockedBack = false;
	public float knockBackTime = 0.5f;
	public float knockBackMagnitude = 15.0f;

	public ParticleSystem particleSystem;

	[HideInInspector]
	private float normalizedHorizontalSpeed = 0;

	private CharacterController2D _controller;
	private RaycastHit2D _lastControllerColliderHit;
	private Vector3 _velocity;
	private Animator anim;
	private PlayerController playerController;

	private Vector3 knockDir;
	private float lastHitTime;

	void Awake ()
	{
		_controller = GetComponent<CharacterController2D> ();
		playerController = GetComponent<PlayerController> ();
		anim = GetComponent<Animator> ();
		// listen to some events for illustration purposes
		_controller.onControllerCollidedEvent += onControllerCollider;
		_controller.onTriggerEnterEvent += onTriggerEnterEvent;
		_controller.onTriggerExitEvent += onTriggerExitEvent;
	}


	#region Event Listeners

	void onControllerCollider (RaycastHit2D hit)
	{

		if (hit.collider.tag == "Enemy") {
			playerController.TakeDamage (hit.collider.GetComponent<DamageOnHit> ().damage, hit.collider.transform);
		}

		// bail out on plain old ground hits cause they arent very interesting
		if (hit.normal.y == 1f)
			return;
	}


	void onTriggerEnterEvent (Collider2D col)
	{

	}


	void onTriggerExitEvent (Collider2D col)
	{
		Debug.Log ("onTriggerExitEvent: " + col.gameObject.name);
	}

	#endregion

	// the Update loop contains a very simple example of moving the character around and controlling the animation
	void Update ()
	{
		if (canMove) {
			Controls ();
		}
	}


	void Controls ()
	{
		if (_controller.isGrounded)
			_velocity.y = 0;

		if (!knockedBack) {
			Movement ();
		}else {
			normalizedHorizontalSpeed = 0;

			if (_controller.isGrounded && !attacking && !shapeShifting) {
				anim.Play (Animator.StringToHash (playerController.currState.ToString () + "_IDLE"));

			}
			//IDLE ANIM
		} 

		HandleKnockBack ();


		// we can only jump whilst grounded
		if (_controller.isGrounded && Input.GetKeyDown (KeyCode.UpArrow) && !shapeShifting && !Input.GetKey (KeyCode.DownArrow)) {
			_velocity.y = Mathf.Sqrt (2f * jumpHeight * -gravity);
			anim.Play (Animator.StringToHash (playerController.currState.ToString () + "_JUMP"));
			jumped = true;
			//JUMP ANIM
		}
		if (Input.GetKeyDown (KeyCode.Z) && !attacking && !shapeShifting) {
			if (playerController.currState == PlayerController.shiftState.WOLF && jumped == false)
				StartCoroutine (Attack ());
			else if (playerController.currState == PlayerController.shiftState.HUMAN)
				StartCoroutine (Attack ());
		}



		// apply horizontal speed smoothing it. dont really do this with Lerp. Use SmoothDamp or something that provides more control
		var smoothedMovementFactor = _controller.isGrounded ? groundDamping : inAirDamping; // how fast do we change direction?
		_velocity.x = Mathf.Lerp (_velocity.x, normalizedHorizontalSpeed * runSpeed, Time.deltaTime * smoothedMovementFactor);

		// apply gravity before moving
		_velocity.y += gravity * Time.deltaTime;

		// if holding down bump up our movement amount and turn off one way platform detection for a frame.
		// this lets uf jump down through one way platforms
		if (_controller.isGrounded && Input.GetKey (KeyCode.DownArrow)) {

			_velocity.y *= 3f;
			_controller.ignoreOneWayPlatformsThisFrame = true;
		}

		_controller.move (_velocity * Time.deltaTime);

		// grab our current _velocity to use as a base for all calculations
		_velocity = _controller.velocity;

		if (!_controller.isGrounded && !Input.GetKeyDown (KeyCode.UpArrow) && !shapeShifting && !attacking) {
			anim.Play (Animator.StringToHash (playerController.currState.ToString () + "_FALL"));
			if (Physics2D.Raycast (transform.position, Vector2.down, 0.1f, _controller.platformMask) && !jumped && !attacking) {
				StartCoroutine (triggerParticles (0.1f));
			}

		}



		if (_controller.isGrounded && jumped == true) {
			jumped = false;
			StartCoroutine (triggerParticles (0));
		}
	}

	void Movement(){
		if (Input.GetKey (KeyCode.RightArrow) && !shapeShifting) {
			normalizedHorizontalSpeed = 1;
			if (transform.localScale.x < 0f)
				transform.localScale = new Vector3 (-transform.localScale.x, transform.localScale.y, transform.localScale.z);

			if (_controller.isGrounded && !attacking && !shapeShifting) {
				anim.Play (Animator.StringToHash (playerController.currState.ToString () + "_RUN"));
			}
			//RUN ANIM
		} else if (Input.GetKey (KeyCode.LeftArrow) && !shapeShifting) {
			normalizedHorizontalSpeed = -1;
			if (transform.localScale.x > 0f)
				transform.localScale = new Vector3 (-transform.localScale.x, transform.localScale.y, transform.localScale.z);

			if (_controller.isGrounded && !attacking && !shapeShifting) {
				anim.Play (Animator.StringToHash (playerController.currState.ToString () + "_RUN"));
			}
			//RUN ANIM
		} else {
			normalizedHorizontalSpeed = 0;

			if (_controller.isGrounded && !attacking && !shapeShifting) {
				anim.Play (Animator.StringToHash (playerController.currState.ToString () + "_IDLE"));

			}
			//IDLE ANIM
		}
	}


	public IEnumerator triggerParticles (float delay)
	{
		yield return new WaitForSeconds (delay);
		particleSystem.Emit (10);
	}

	IEnumerator Attack ()
	{
		GetComponent<AudioSource> ().pitch = Random.Range (1.0f, 2.0f);
		GetComponent<AudioSource> ().volume = 0.75f;
		attacking = true;
		anim.Play (Animator.StringToHash (playerController.currState.ToString () + "_ATTACK"));
		if (playerController.currState == PlayerController.shiftState.WOLF) {
			GetComponent<AudioSource> ().PlayOneShot (playerController.w_atkSound);
			jumped = true;
			_velocity.x += (transform.localScale.x * runSpeed * Mathf.Sqrt (2f * playerController.w_chargeAttackSpeed)) - _velocity.x;
			_velocity.y = Mathf.Sqrt (2f * playerController.w_chargeAttackHeight * -gravity);
		} else if (playerController.currState == PlayerController.shiftState.HUMAN) {
			GetComponent<AudioSource> ().PlayOneShot (playerController.h_atkSound);
		}
		yield return new WaitForSeconds (0.4f);
		attacking = false;
		GetComponent<AudioSource> ().pitch = 1;
		GetComponent<AudioSource> ().volume = 1f;
	}

	public IEnumerator ShapeShift ()
	{
		shapeShifting = true;
		anim.Play (Animator.StringToHash (playerController.currState.ToString () + "_SHAPESHIFT"));

		if (playerController.currState == PlayerController.shiftState.WOLF) {
			runSpeed = playerController.w_formSpeed;
			jumpHeight = playerController.w_jumpHeight;
		} else {
			runSpeed = playerController.h_formSpeed;
			jumpHeight = playerController.h_jumpHeight;
		}

		yield return new WaitForSeconds (0.75f);
		shapeShifting = false;
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
