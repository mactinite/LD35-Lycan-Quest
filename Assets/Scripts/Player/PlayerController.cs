using UnityEngine;
using System.Collections;
using Prime31;
using UnityStandardAssets.ImageEffects;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour {


	public enum shiftState{HUMAN,WOLF};
	public shiftState currState = shiftState.HUMAN;
	private BoxCollider2D col;
	private CharacterController2D cc;
	private PlatformingController pCtrl;
	public AudioClip shiftSound;
	public AudioClip hitSound;



	[Header("Player Stats")]
	public float health = 100;
	public Image healthImage;
	private float hImageInitWidth;
	private Vector2 hVector = Vector2.zero;


	public float stamina = 100;
	public Image staminaImage;
	private float sImageInitWidth;
	private Vector2 sVector= Vector2.zero;
	new public AudioSource audio;

	public float currDamage;


	[Header("Progress Related Variables")]
	public bool hasWolfForm = false;
	public Image w_Image;
	public bool hasHammer = false;
	public Image hmr_Image;
	public bool hasKey = false;
	public Image key_Image;
	public AudioClip pickupSound;


	[Header("Human specific variables")]
	public float h_formSpeed = 25.0f;
	public float h_damage = 5.0f;
	public float h_jumpHeight = 3.5f;
	public AudioClip h_atkSound;


	[Header("Wolf specific variables")]
	public AudioClip w_atkSound;
	public float w_damage = 10.0f;
	public float w_formSpeed = 35.0f;
	public float w_jumpHeight = 2.5f;
	public float w_chargeAttackCost = 30.0f;
	public float w_chargeAttackSpeed = 30.0f;
	public float w_chargeAttackHeight = 1.0f;


	[Header("Screen Shake")]
	public float magnitude = 30f;
	public float damper = 5f;
	public float duration = 0.5f;



	void onTriggerEnterEvent(Collider2D col){
		if (col.tag == "WolfPickup") {
			hasWolfForm = true;
			audio.PlayOneShot (pickupSound);
			Destroy (col.gameObject);
		} else if (col.tag == "HammerPickup") {
			hasHammer = true;
			SceneManager.LoadScene (2);
			audio.PlayOneShot (pickupSound);
			Destroy (col.gameObject);
		} else if (col.tag == "KeyPickup") {
			hasKey = true;
			audio.PlayOneShot (pickupSound);
			Destroy (col.gameObject);
		}else if (col.tag == "HealthPickup") {
			health += 25;
			audio.PlayOneShot (pickupSound);
			Destroy (col.gameObject);
		}


		if(col.tag == "Door" && hasKey){
			col.BroadcastMessage ("OpenDoor", SendMessageOptions.DontRequireReceiver);
			hasKey = false;
		}


	}

	// Use this for initialization
	void Start () {
		col = GetComponent<BoxCollider2D> ();
		cc = GetComponent<CharacterController2D> ();
		pCtrl = GetComponent<PlatformingController> ();
		audio = GetComponent<AudioSource> ();
		sImageInitWidth = staminaImage.rectTransform.sizeDelta.x;
		hImageInitWidth = healthImage.rectTransform.sizeDelta.x;
		cc.onTriggerEnterEvent += onTriggerEnterEvent;
	}
	
	// Update is called once per frame
	void Update () {
		hmr_Image.gameObject.SetActive(hasHammer);
		w_Image.gameObject.SetActive(hasWolfForm);
		key_Image.gameObject.SetActive(hasKey);

		hVector.x = ((health / 100) * hImageInitWidth) -1;
		sVector.x = ((stamina / 100) * sImageInitWidth)-1;
		staminaImage.rectTransform.sizeDelta = sVector;
		healthImage.rectTransform.sizeDelta = hVector;


		if (hasWolfForm) {
			HandleShift ();
		}

		if (currState == shiftState.HUMAN) {
			HumanControls ();
		} else {
		
			WolfControls ();
		}

		if (Camera.main.GetComponent<VignetteAndChromaticAberration> ().chromaticAberration > 6f) {
			Camera.main.GetComponent<VignetteAndChromaticAberration> ().chromaticAberration -= Time.deltaTime * 100;

		}

		stamina = Mathf.Clamp (stamina, 0, 100);
	}

	void HandleShift(){
		if (currState == shiftState.WOLF) {
			if (!Physics2D.Raycast (transform.position + Vector3.up, Vector2.up, 0.5f, cc.platformMask)) {
				if (Input.GetKeyDown (KeyCode.C)) {
					ChangeState ();
				}
			}
		} else if (currState == shiftState.HUMAN) {
			if (Input.GetKeyDown (KeyCode.C)) {
				ChangeState ();
			}
		}
	}

	void HumanControls(){
		if (hasHammer) {

		}

		if (hasKey) {

		}

	}

	void WolfControls(){
		
	}

	public IEnumerator Shake() {

		float elapsed = 0.0f;

		while (elapsed < duration) {
			Vector3 currCamPos = Camera.main.transform.position;
			elapsed += Time.deltaTime;          

			float percentComplete = elapsed / duration;         
			float damper = 1.0f - Mathf.Clamp(4.0f * percentComplete - 3.0f, 0.0f, 1.0f);
			// map value to [-1, 1]
			float x = Random.value * 2.0f - 1.0f;
			float y = Random.value * 2.0f - 1.0f;
			x *= magnitude * damper;
			y *= magnitude * damper;

			Camera.main.transform.position = new Vector3(currCamPos.x + x, currCamPos.y + y, currCamPos.z);

			yield return null;
		}
	}



	void ChangeState(){

		audio.pitch = Random.Range (1.0f, 2.0f);

		if (currState == shiftState.HUMAN) {
			Camera.main.GetComponent<VignetteAndChromaticAberration> ().chromaticAberration = 100;
			currState = shiftState.WOLF;
			currDamage = w_damage;
			StartCoroutine (pCtrl.ShapeShift ());
			StartCoroutine (Shake ());
			audio.PlayOneShot (shiftSound);
			transform.position = new Vector2 (transform.position.x, transform.position.y);
			col.size = new Vector2 (0.5f, 0.7f);
			col.offset = new Vector2 (0,0.4f);
			cc.recalculateDistanceBetweenRays ();

		} else {
			currState = shiftState.HUMAN;
			currDamage = h_damage;
			StartCoroutine (pCtrl.ShapeShift ());
			Camera.main.GetComponent<VignetteAndChromaticAberration> ().chromaticAberration = 100;
			StartCoroutine (Shake ());
			audio.PlayOneShot (shiftSound);
			transform.position = new Vector2 (transform.position.x, transform.position.y);
			col.size = new Vector2 (0.5f, 1.2f);
			col.offset = new Vector2 (0,0.6f);
			cc.recalculateDistanceBetweenRays ();

		}

		audio.pitch = 1;
	}


	public void TakeDamage(int damage, Transform damagedBy){

		if (health - damage <= 0) {
			health -= damage;
			Camera.main.GetComponent<VignetteAndChromaticAberration> ().chromaticAberration = 100;
			StartCoroutine (pCtrl.KnockBack (damagedBy));
			StartCoroutine (Die ());
		}
		if (!pCtrl.knockedBack) {
			StartCoroutine (Shake());
			audio.PlayOneShot (hitSound);
			Camera.main.GetComponent<VignetteAndChromaticAberration> ().chromaticAberration = 100;
			health -= damage;
			StartCoroutine (pCtrl.KnockBack (damagedBy));
		}
	}


	public IEnumerator Die(){
		audio.PlayOneShot (shiftSound);
		pCtrl.canMove = false;
		audio.pitch = 1;
		Time.timeScale = 0.1f;
		yield return new WaitForSeconds (0.1f);
		Time.timeScale = 1;
		SceneManager.LoadScene (SceneManager.GetActiveScene().name);

	}
		
}
