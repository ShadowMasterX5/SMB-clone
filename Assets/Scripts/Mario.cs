using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/* Mario physics reference: http://s276.photobucket.com/user/jdaster64/media/smb_playerphysics.png.html */


public class Mario : MonoBehaviour 
{
	private LevelManager t_LevelManager;
	private Transform m_GroundCheck1, m_GroundCheck2;
	private GameObject m_StompBox;
	private Animator m_Animator;
	private Rigidbody2D m_Rigidbody2D;
	private CircleCollider2D m_CircleCollider2D;

	public LayerMask GroundLayers;
	public GameObject Fireball;
	public Transform FirePos;
	private float waitBetweenFire = .2f;
	private float fireTime1, fireTime2;

	private float faceDirectionX;
	private float moveDirectionX;
	private float normalGravity;

	private float currentSpeedX;
	private float speedXBeforeJump;

	private float minWalkSpeedX = 4.48f;
	private float walkAccelerationX = 2.24f;
	private float runAccelerationX = 3.36f;
	private float releaseDecelerationX = 4.0f; // original: .19f;
	private float skidDecelerationX = 8.0f; // .38f;
	private float skidTurnaroundSpeedX = 28.0f; // 2.11;
	private float maxWalkSpeedX = 93.76f;
	private float maxRunSpeedX = 153.76f;

	private float jumpSpeedY;
	private float jumpUpGravity;
	private float jumpDownGravity;
	private float midairAccelerationX;
	private float midairDecelerationX;

	private float automaticWalkSpeedX;
	private float automaticGravity;

	public float castleWalkSpeedX = 93.76f;
	public float levelEntryWalkSpeedX = 48.8f;

	private bool isGrounded;
	public bool isDashing;
	private bool isFalling;
	private bool isJumping;
	private bool isChangingDirection;
	private bool wasDashingBeforeJump;
	private bool isShooting;
	public bool isCrouching;

	private bool jumpButtonHeld;
	private bool jumpButtonReleased;

	public bool inputFreezed;


	// Use this for initialization
	void Start () 
	{
		t_LevelManager = FindObjectOfType<LevelManager>();
		m_GroundCheck1 = transform.Find ("Ground Check 1");
		m_GroundCheck2 = transform.Find ("Ground Check 2");
		m_StompBox = transform.Find ("Stomp Box").gameObject;
		m_Animator = GetComponent<Animator> ();
		m_Rigidbody2D = GetComponent<Rigidbody2D> ();
		m_CircleCollider2D = GetComponent<CircleCollider2D> ();
		normalGravity = m_Rigidbody2D.gravityScale;

		// Drop Mario at spawn position
		transform.position = FindObjectOfType<LevelManager>().FindSpawnPosition();

		// Set correct size
		UpdateSize ();

		jumpButtonReleased = true;
		fireTime1 = 0;
		fireTime2 = 0;
	}

	/****************** Movement control */
	void SetJumpParams() 
	{
		if (currentSpeedX < 60.0f) 
		{
			jumpSpeedY = 240.0f;
			jumpUpGravity = 7.52f;
			jumpDownGravity = 26.24f;
		} 
		else if (currentSpeedX < 138.72f) 
		{
			jumpSpeedY = 240f;
			jumpUpGravity = 7.04f;
			jumpDownGravity = 22.56f;
		} 
		else 
		{
			jumpSpeedY = 300.0f;
			jumpUpGravity = 9.44f;
			jumpDownGravity = 33.76f;
		}
	}

	void SetMidairParams() 
	{
		if (currentSpeedX < 93.76f) 
		{
			midairAccelerationX = 2.24f;
			if (speedXBeforeJump < 108.8f) 
			{
				midairDecelerationX = 2.24f;
			} 
			else 
			{
				midairDecelerationX = 3.04f;
			}
		} 
		else 
		{
			midairAccelerationX = 3.36f;
			midairDecelerationX = 3.36f;
		}
	}

	void FixedUpdate () 
	{
		/******** Horizontal movement on ground */
		if (isGrounded) 
		{
			// If holding directional button, accelerate until reach max walk speed
			// If holding Dash, accelerate until reach max run speed
			if (faceDirectionX != 0) 
			{
				if (currentSpeedX == 0) 
				{
					currentSpeedX = minWalkSpeedX;
				} 
				else if (currentSpeedX < maxWalkSpeedX) 
				{
					currentSpeedX = IncreaseWithinBound (currentSpeedX, walkAccelerationX, maxWalkSpeedX);
				} 
				else if (isDashing && currentSpeedX < maxRunSpeedX) 
				{
					currentSpeedX = IncreaseWithinBound (currentSpeedX, runAccelerationX, maxRunSpeedX);
				}
			} 

			// Decelerate upon release of directional button
			else if (currentSpeedX > 0) 
			{
				currentSpeedX = DecreaseWithinBound (currentSpeedX, releaseDecelerationX, 0);
			}

			// If change direction, skid until lose all momentum then turn around
			if (isChangingDirection) 
			{
				if (currentSpeedX > skidTurnaroundSpeedX) 
				{
					moveDirectionX = -faceDirectionX;
					m_Animator.SetBool ("isSkidding", true);
					currentSpeedX = DecreaseWithinBound (currentSpeedX, skidDecelerationX, 0);
				} 
				else 
				{
					moveDirectionX = faceDirectionX;
					m_Animator.SetBool ("isSkidding", false);
				}
			} 
			else 
			{
				m_Animator.SetBool ("isSkidding", false);
			}

			// Freeze horizontal movement while crouching
			if (isCrouching) 
			{
				currentSpeedX = 0;
			}


		/******** Horizontal movement on air */
		} 
		else 
		{
			SetMidairParams ();

			// Holding Dash while in midair has no effect
			if (faceDirectionX != 0) 
			{
				if (currentSpeedX == 0) 
				{
					currentSpeedX = minWalkSpeedX;
				} 
				else if (currentSpeedX < maxWalkSpeedX) 
				{
					currentSpeedX = IncreaseWithinBound (currentSpeedX, midairAccelerationX, maxWalkSpeedX);
				} 
				else if (wasDashingBeforeJump && currentSpeedX < maxRunSpeedX) 
				{
					currentSpeedX = IncreaseWithinBound (currentSpeedX, midairAccelerationX, maxRunSpeedX);
				}
			} 
			else if (currentSpeedX > 0) 
			{
				currentSpeedX = DecreaseWithinBound (currentSpeedX, releaseDecelerationX, 0);
			}

			// If change direction, decelerate but keep facing move direction
			if (isChangingDirection) 
			{
				faceDirectionX = moveDirectionX;
				currentSpeedX = DecreaseWithinBound (currentSpeedX, midairDecelerationX, 0);
			}
		}


		/******** Vertical movement */
		if (isGrounded) 
		{
			isJumping = false;
			m_Rigidbody2D.gravityScale = normalGravity;
		}

		if (!isJumping) 
		{
			if (isGrounded && jumpButtonHeld && jumpButtonReleased) 
			{
				SetJumpParams ();
				m_Rigidbody2D.velocity = new Vector2 (m_Rigidbody2D.velocity.x, jumpSpeedY);
				isJumping = true;
				jumpButtonReleased = false;
				speedXBeforeJump = currentSpeedX;
				wasDashingBeforeJump = isDashing;
				if (t_LevelManager.marioSize == 0) 
				{
					t_LevelManager.soundSource.PlayOneShot (t_LevelManager.jumpSmallSound);
				} 
				else 
				{
					t_LevelManager.soundSource.PlayOneShot (t_LevelManager.jumpSuperSound);
				}
			}
		} 
		else 
		{  // lower gravity if Jump button held; increased gravity if released
			if (m_Rigidbody2D.velocity.y > 0 && jumpButtonHeld) 
			{
				m_Rigidbody2D.gravityScale = normalGravity * jumpUpGravity;
			} 
			else 
			{
				m_Rigidbody2D.gravityScale = normalGravity * jumpDownGravity;
			}
		}


		// Disable Stomp Box if not falling down
		// Disable Circle Collider if falling down (to prevent multi collisions registered)
		if (!isFalling) 
		{
			m_StompBox.SetActive (false);
			m_CircleCollider2D.enabled = true;
		} 
		else 
		{
			m_StompBox.SetActive (true);
			m_CircleCollider2D.enabled = false;
		}

		/******** Horizontal orientation */
		if (faceDirectionX > 0) 
		{
			transform.localScale = new Vector2 (1, 1); // facing right
		} 
		else if (faceDirectionX < 0) 
		{
			transform.localScale = new Vector2 (-1, 1);
		}

		/******** Reset params for automatic movement */
		if (inputFreezed) 
		{
			currentSpeedX = automaticWalkSpeedX;
			m_Rigidbody2D.gravityScale = automaticGravity;
		}

		/******** Shooting */
		if (isShooting && t_LevelManager.marioSize == 2) 
		{
			fireTime2 = Time.time;

			if (fireTime2 - fireTime1 >= waitBetweenFire) 
			{
				m_Animator.SetTrigger ("isFiring");
				GameObject fireball = Instantiate (Fireball, FirePos.position, Quaternion.identity);
				fireball.GetComponent<MarioFireball> ().directionX = transform.localScale.x;
				t_LevelManager.soundSource.PlayOneShot (t_LevelManager.fireballSound);
				fireTime1 = Time.time;
			}
		}

		/******** Set params */
		m_Rigidbody2D.velocity = new Vector2 (moveDirectionX*currentSpeedX, m_Rigidbody2D.velocity.y);

		m_Animator.SetBool ("isJumping", isJumping);
		m_Animator.SetBool ("isFallingNotFromJump", isFalling && !isJumping);
		m_Animator.SetBool ("isCrouching", isCrouching);
		m_Animator.SetFloat ("absSpeed", Mathf.Abs (currentSpeedX));

		if (faceDirectionX != 0 && !isChangingDirection) 
		{
			moveDirectionX = faceDirectionX;
		}
			
	}

	/****************** Automatic movement sequences */
	void Update() 
	{
		if (!inputFreezed) 
		{
			faceDirectionX = Input.GetAxisRaw("D-Pad Horizontal"); // > 0 for right, < 0 for left
			isDashing = Input.GetButton("B Button");
			isCrouching = Input.GetAxis("D-Pad Vertical") < 0;
			isShooting = Input.GetButtonDown("B Button");
			jumpButtonHeld = Input.GetButton("A Button");
			if (Input.GetButtonUp("A Button")) 
			{
				jumpButtonReleased = true;
			}
		}

		isFalling = m_Rigidbody2D.velocity.y < 0 && !isGrounded;
		isGrounded = Physics2D.OverlapPoint (m_GroundCheck1.position, GroundLayers) || Physics2D.OverlapPoint (m_GroundCheck2.position, GroundLayers); 
		isChangingDirection = currentSpeedX > 0 && faceDirectionX * moveDirectionX < 0;


		if (inputFreezed && !t_LevelManager.gamePaused) 
		{
			if (isDying) 
			{
				deadUpTimer -= Time.unscaledDeltaTime;
				if (deadUpTimer > 0) { // TODO MovePosition not working
//					m_Rigidbody2D.MovePosition (m_Rigidbody2D.position + deadUpVelocity * Time.unscaledDeltaTime);
					gameObject.transform.position += Vector3.up * 3.52f;
				} 
				else 
				{
//					m_Rigidbody2D.MovePosition (m_Rigidbody2D.position + deadDownVelocity * Time.unscaledDeltaTime);
					gameObject.transform.position += Vector3.down * 3.2f;
				}
			} 
			else if (isClimbingFlagPole) 
			{
				m_Rigidbody2D.MovePosition (m_Rigidbody2D.position + climbFlagPoleVelocity * Time.deltaTime);
			}
		}
	}

	public bool isDying = false;
	float deadUpTimer = .25f;
//	Vector2 deadUpVelocity = new Vector2 (0, 10f);
//	Vector2 deadDownVelocity = new Vector2 (0, -15f);

	public void FreezeAndDie() 
	{
		FreezeUserInput ();
		isDying = true;
		m_Rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
		m_Animator.SetTrigger ("respawn");
		gameObject.layer = LayerMask.NameToLayer ("Falling to Kill Plane");
		gameObject.GetComponent<SpriteRenderer>().sortingLayerName = "Foreground Effect";
	}

	bool isClimbingFlagPole = false;
	Vector2 climbFlagPoleVelocity = new Vector2 (0, -80f);
	public void ClimbFlagPole() 
	{
		FreezeUserInput ();
		isClimbingFlagPole = true;
		m_Animator.SetBool ("climbFlagPole", true);
		m_Rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
		Debug.Log (this.name + ": Mario starts climbing flag pole");
	}

	void JumpOffPole() 
	{ // get off pole and start walking right

		transform.position = new Vector2 (transform.position.x + 8.0f, transform.position.y);
		m_Animator.SetBool ("climbFlagPole", false);
		AutomaticWalk(castleWalkSpeedX);
		m_Rigidbody2D.bodyType = RigidbodyType2D.Dynamic;
		Debug.Log (this.name + ": Mario jumps off pole and walks to castle");
	}

	/****************** Automatic movement (e.g. walk to castle sequence) */
	public void UnfreezeUserInput() 
	{
		inputFreezed = false;
		Debug.Log (this.name + " UnfreezeUserInput called");
	}

	public void FreezeUserInput() 
	{
		inputFreezed = true;
		jumpButtonHeld = false;
		jumpButtonReleased = true;

		faceDirectionX = 0;
		moveDirectionX = 0;

		currentSpeedX = 0;
		speedXBeforeJump = 0;
		automaticWalkSpeedX = 0;
		automaticGravity = normalGravity;

		isDashing = false;
		wasDashingBeforeJump = false;
		isCrouching = false;
		isChangingDirection = false;
		isShooting = false;

		gameObject.GetComponent<Rigidbody2D>().velocity = Vector3.zero; // stop all momentum
		Debug.Log (this.name + " FreezeUserInput called");
	}

	public void AutomaticWalk(float walkVelocityX) 
	{
		FreezeUserInput ();
		if (walkVelocityX != 0) 
		{
			faceDirectionX = walkVelocityX / Mathf.Abs (walkVelocityX);
		}
		automaticWalkSpeedX = Mathf.Abs(walkVelocityX);
		Debug.Log (this.name + " AutomaticWalk: speed=" + automaticWalkSpeedX.ToString());
	}

	public void AutomaticCrouch() 
	{
		FreezeUserInput ();
		isCrouching = true;
	}

	/****************** Misc */
	public void UpdateSize() 
	{
		GetComponent<Animator>().SetInteger("marioSize", FindObjectOfType<LevelManager>().marioSize);
	}

	float IncreaseWithinBound(float val, float delta, float maxVal = Mathf.Infinity) 
	{
		val += delta;

		if (val > maxVal) 
		{
			val = maxVal;
		}
		return val;
	}

	float DecreaseWithinBound(float val, float delta, float minVal = 0) 
	{
		val -= delta;

		if (val < minVal) 
		{
			val = minVal;
		}
		return val;
	}

	void OnCollisionEnter2D(Collision2D other) 
	{
		Vector2 normal = other.contacts[0].normal;
		Vector2 bottomSide = new Vector2 (0f, 16.0f);
		bool bottomHit = normal == bottomSide;

		if (other.gameObject.tag.Contains ("Enemy")) 
		{ // TODO: koopa shell static does no damage
			Enemy enemy = other.gameObject.GetComponent<Enemy> ();

			if (!t_LevelManager.isInvincible ()) 
			{
				if (!other.gameObject.GetComponent<KoopaShell> () || 
					other.gameObject.GetComponent<KoopaShell> ().isRolling ||  // non-rolling shell should do no damage
					!bottomHit || (bottomHit && !enemy.isBeingStomped)) 
				{
					Debug.Log (this.name + " OnCollisionEnter2D: Damaged by " + other.gameObject.name
						+ " from " + normal.ToString () + "; isFalling=" + isFalling); // TODO sometimes fire before stompbox reacts
					t_LevelManager.MarioPowerDown ();
				}

			} 
			else if (t_LevelManager.isInvincibleStarman) 
			{
				t_LevelManager.MarioStarmanTouchEnemy (enemy);
			}
		
		} 
		else if (other.gameObject.tag == "Goal" && isClimbingFlagPole && bottomHit) 
		{
			Debug.Log (this.name + ": Mario hits bottom of flag pole");
			isClimbingFlagPole = false;
			JumpOffPole ();
		}
	}

}
