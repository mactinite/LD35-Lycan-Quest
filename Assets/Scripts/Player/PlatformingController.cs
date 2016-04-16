using UnityEngine;
using System.Collections;
using Prime31;


public class PlatformingController : MonoBehaviour
{


	// movement config
	public float gravity = -25f;
	public float runSpeed = 8f;
	public float groundDamping = 20f; // how fast do we change direction? higher means faster
	public float inAirDamping = 5f;
	public float jumpHeight = 3f;

	private bool attacking = false;
	private bool shapeShifting = false;

	[HideInInspector]
	private float normalizedHorizontalSpeed = 0;

	private CharacterController2D _controller;
	private RaycastHit2D _lastControllerColliderHit;
	private Vector3 _velocity;
	private Animator anim;
	private PlayerController playerController;


	void Awake()
	{
		_controller = GetComponent<CharacterController2D>();
		playerController = GetComponent<PlayerController> ();
		anim = GetComponent<Animator> ();
		// listen to some events for illustration purposes
		_controller.onControllerCollidedEvent += onControllerCollider;
		_controller.onTriggerEnterEvent += onTriggerEnterEvent;
		_controller.onTriggerExitEvent += onTriggerExitEvent;
	}


	#region Event Listeners

	void onControllerCollider( RaycastHit2D hit )
	{
		// bail out on plain old ground hits cause they arent very interesting
		if( hit.normal.y == 1f )
			return;

		// logs any collider hits if uncommented. it gets noisy so it is commented out for the demo
		//Debug.Log( "flags: " + _controller.collisionState + ", hit.normal: " + hit.normal );
	}


	void onTriggerEnterEvent( Collider2D col )
	{
		Debug.Log( "onTriggerEnterEvent: " + col.gameObject.name );
	}


	void onTriggerExitEvent( Collider2D col )
	{
		Debug.Log( "onTriggerExitEvent: " + col.gameObject.name );
	}

	#endregion


	// the Update loop contains a very simple example of moving the character around and controlling the animation
	void Update()
	{
		if( _controller.isGrounded )
			_velocity.y = 0;

		if( Input.GetKey( KeyCode.RightArrow ) )
		{
			normalizedHorizontalSpeed = 1;
			if( transform.localScale.x < 0f )
				transform.localScale = new Vector3( -transform.localScale.x, transform.localScale.y, transform.localScale.z );

			if (_controller.isGrounded && !attacking && !shapeShifting) {
				anim.Play(Animator.StringToHash(playerController.currState.ToString() + "_RUN"));
			}
				//RUN ANIM
		}
		else if( Input.GetKey( KeyCode.LeftArrow ) )
		{
			normalizedHorizontalSpeed = -1;
			if( transform.localScale.x > 0f )
				transform.localScale = new Vector3( -transform.localScale.x, transform.localScale.y, transform.localScale.z );

			if (_controller.isGrounded && !attacking && !shapeShifting) {
				anim.Play(Animator.StringToHash(playerController.currState.ToString() + "_RUN"));
			}
				//RUN ANIM
		}
		else
		{
			normalizedHorizontalSpeed = 0;

			if (_controller.isGrounded && !attacking && !shapeShifting) {
				anim.Play(Animator.StringToHash(playerController.currState.ToString() + "_IDLE"));

			}
				//IDLE ANIM
		}

		if (Input.GetKeyDown (KeyCode.Z) && !attacking) {

			StartCoroutine (Attack ());
		}



		// we can only jump whilst grounded
		if( _controller.isGrounded && Input.GetKeyDown( KeyCode.UpArrow ) && !shapeShifting && !Input.GetKey(KeyCode.DownArrow))
		{
			_velocity.y = Mathf.Sqrt( 2f * jumpHeight * -gravity );
			anim.Play(Animator.StringToHash(playerController.currState.ToString() + "_JUMP"));
			//JUMP ANIM
		}


		// apply horizontal speed smoothing it. dont really do this with Lerp. Use SmoothDamp or something that provides more control
		var smoothedMovementFactor = _controller.isGrounded ? groundDamping : inAirDamping; // how fast do we change direction?
		_velocity.x = Mathf.Lerp( _velocity.x, normalizedHorizontalSpeed * runSpeed, Time.deltaTime * smoothedMovementFactor );

		// apply gravity before moving
		_velocity.y += gravity * Time.deltaTime;

		// if holding down bump up our movement amount and turn off one way platform detection for a frame.
		// this lets uf jump down through one way platforms
		if( _controller.isGrounded && Input.GetKey( KeyCode.DownArrow ) )
		{

			_velocity.y *= 3f;
			_controller.ignoreOneWayPlatformsThisFrame = true;
		}

		_controller.move( _velocity * Time.deltaTime );

		// grab our current _velocity to use as a base for all calculations
		_velocity = _controller.velocity;
	}


	IEnumerator Attack(){
		attacking = true;
		anim.Play(Animator.StringToHash("ATTACK"));
		yield return new WaitForSeconds(0.4f);
		attacking = false;
	}

	public IEnumerator ShapeShift(){
		shapeShifting = true;
		anim.Play(Animator.StringToHash(playerController.currState.ToString() + "_SHAPESHIFT"));
		yield return new WaitForSeconds(0.5f);
		shapeShifting = false;
	}

}
