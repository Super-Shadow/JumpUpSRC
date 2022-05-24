using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/* Script for anything movement or control related
 * Will handle movement left, right, the jumping code
 * Collision with enemies and objects ect...
 * 
 * For now most things can be done here, if the script needs
 * to be broken up, it can be down the line.
*/

public class PlayerController : MonoBehaviour
{
	[CanBeNull] [HideInInspector] public MenuHandler menu;
	[SerializeField] private float jumpMult;
	[SerializeField] private float moveSpeed;
	[SerializeField] private float airSpeed;
	[SerializeField] private Rigidbody2D rigid;
	[SerializeField] private BoxCollider2D boxCollider;

	[SerializeField] private PhysicsMaterial2D normalPhysMat;
	[SerializeField] private PhysicsMaterial2D bouncePhysMat;
	[SerializeField] private PhysicsMaterial2D slidePhysMat;

	// How long the button is held for, the time period where
	// a jump is a set height (makes smaller jumps more consistent)
	// and the maximum time where a jump will then be executed.
	private float heldTime;
	private readonly float adjustedJumpTime = 0.25f;
	private readonly float maximumJumpTime = 1.25f;
	private readonly float screenShakeVelocityThreshold = 2f;

	// Collision detection via hitbox overlap.
	// If someone wants to do raycasting instead feel free
	// to implement it instead.
	[SerializeField] private LayerMask groundLayer;
	[SerializeField] private LayerMask enemyLayer;
	//[SerializeField] private float checkRadius;
	//[SerializeField] private Transform checkingPoint;
	private bool grounded;
	private bool airborn;
	private float moveInput;
	private bool dying;

	private bool IsGrounded => grounded;

	private float checkRayLength = 0.5f; // We use this just to check if ground is bouncy or not
	private float minRayLength = 0.3f; // This is used to check whether we have basically hit the ground
	private float rayPosOffset = 0; // This is used to offset the ray position inwards from the corners of the collider (or outwards by -)

	[HideInInspector] public KeyCode jumpKeyCode = KeyCode.Space;
	[HideInInspector] public KeyCode leftKeyCode = KeyCode.LeftArrow;
	[HideInInspector] public KeyCode rightKeyCode = KeyCode.RightArrow;

	[HideInInspector] public bool inMenu;

	[SerializeField] private SpriteRenderer sprite;

	private int score;
	[SerializeField] private Text scoreText;
	[SerializeField] private Text finishText;

	private bool finishing = false;

	public void IncreaseScore(int i)
	{
		score += i;
		scoreText.text = "Score: " + score;
	}

	private void Start()
	{
		rigid = GetComponent<Rigidbody2D>();
		boxCollider = GetComponent<BoxCollider2D>();
		boxCollider.sharedMaterial = normalPhysMat;
		jumpKeyCode = KeyCode.Space;
		leftKeyCode = KeyCode.LeftArrow;
		rightKeyCode = KeyCode.RightArrow;
	}


	struct Collision
	{
		public Collider2D GetCollider => collider;
		public bool IsSlideable { get; }
		public float GetLength { get; }
		public bool IsNull => collider == null;

		private Collider2D collider;

		public Collision(bool x, Collider2D y, float distance)
		{
			IsSlideable = x;
			collider = y;
			GetLength = distance;
		}
	};

	private Collision CheckForCollision(Vector2 direction)
	{
		var position = transform.position;
		var boxColliderSize = boxCollider.size;

		// 2 Rays for each corner of box. Used to detect what side a player hit.
		// Initialise as checking down
		var ray1 = Physics2D.Raycast(new Vector2(position.x - (boxColliderSize.x / 2) + rayPosOffset, position.y - (boxColliderSize.y / 2)), Vector2.down, checkRayLength, enemyLayer);
		var ray2 = Physics2D.Raycast(new Vector2(position.x + (boxColliderSize.x / 2) - rayPosOffset, position.y - (boxColliderSize.y / 2)), Vector2.down, checkRayLength, enemyLayer);

		if (ray1.collider == null && ray2.collider == null)
		{
			ray1 = Physics2D.Raycast(new Vector2(position.x - (boxColliderSize.x / 2) + rayPosOffset, position.y - (boxColliderSize.y / 2)), Vector2.down, checkRayLength, groundLayer);
			ray2 = Physics2D.Raycast(new Vector2(position.x + (boxColliderSize.x / 2) - rayPosOffset, position.y - (boxColliderSize.y / 2)), Vector2.down, checkRayLength, groundLayer);
		}

		if (direction == Vector2.left)
		{
			ray1 = Physics2D.Raycast(new Vector2(position.x - (boxColliderSize.x / 2) + rayPosOffset, position.y - (boxColliderSize.y / 2) + rayPosOffset), Vector2.left, checkRayLength, groundLayer);
			ray2 = Physics2D.Raycast(new Vector2(position.x - (boxColliderSize.x / 2) + rayPosOffset, position.y + (boxColliderSize.y / 2) - rayPosOffset), Vector2.left, checkRayLength, groundLayer);
		}
		else if (direction == Vector2.right)
		{
			ray1 = Physics2D.Raycast(new Vector2(position.x + (boxColliderSize.x / 2) - rayPosOffset, position.y - (boxColliderSize.y / 2) + rayPosOffset), Vector2.right, checkRayLength, groundLayer);
			ray2 = Physics2D.Raycast(new Vector2(position.x + (boxColliderSize.x / 2) - rayPosOffset, position.y + (boxColliderSize.y / 2) - rayPosOffset), Vector2.right, checkRayLength, groundLayer);

		}
		else if (direction == Vector2.up)
		{
			ray1 = Physics2D.Raycast(new Vector2(position.x - (boxColliderSize.x / 2) + rayPosOffset, position.y + (boxColliderSize.y / 2)), Vector2.up, checkRayLength, groundLayer);
			ray2 = Physics2D.Raycast(new Vector2(position.x + (boxColliderSize.x / 2) - rayPosOffset, position.y + (boxColliderSize.y / 2)), Vector2.up, checkRayLength, groundLayer);
		}

		// Did we hit something?

		var didRay1Hit = ray1.collider != null;
		var didRay2Hit = ray2.collider != null;

		if (!didRay1Hit && !didRay2Hit) 
			return new Collision(false, null, 0);

		if (didRay1Hit)
		{
			if (ray1.collider.CompareTag("Finish") && !finishing)
			{
				finishText.gameObject.SetActive(true);
				finishing = true;
				StartCoroutine(Finish());
			}
		}

		if (didRay2Hit)
		{
			if (ray2.collider.CompareTag("Finish") && !finishing)
			{
				finishText.gameObject.SetActive(true);
				finishing = true;
				StartCoroutine(Finish());
			}
		}


		var slideable = (didRay1Hit && ray1.collider.CompareTag("Slide")) || (didRay2Hit && ray2.collider.CompareTag("Slide"));
		if(didRay1Hit && didRay2Hit) // If both are not null then we need to check whether we are still on a ramp or hanging over a ramp
			slideable = ray1.point.y > ray2.point.y ? ray1.collider.CompareTag("Slide") : ray2.collider.CompareTag("Slide");
		
		// Is it slideable
			
		return new Collision(slideable, didRay1Hit ? ray1.collider : ray2.collider, didRay1Hit ? ray1.distance : ray2.distance);

	}

	private void HandleCollisions()
	{
		// Going downwards
		if (rigid.velocity.y < 0)
		{
			var collision = CheckForCollision(Vector2.down);
			grounded = false; // We either didn't hit anything!

			if (!collision.IsNull)
			{
				
				
				{
					// Is it slideable
					if (collision.IsSlideable)
					{
						boxCollider.sharedMaterial = slidePhysMat;
						grounded = false; // We hit something but it was slidable!
					}
					else
					{
						boxCollider.sharedMaterial = normalPhysMat;
					

						// Did we hit something going down?
						if (collision.GetLength < minRayLength)
						{
							
							if (collision.GetCollider.CompareTag("Enemy") && !dying)
							{
								StartCoroutine(collision.GetCollider.GetComponent<GroundEnemy>().Die());
							}
							else
							{
								grounded = true;
								if (rigid.velocity.y < -7) // Not slideable and we are falling fast
									StartCoroutine(GameManager.Inst.GameCamera.effects.Shake(0.25f, 0.045f));

							}
						}
					}
				}

			}
		}

		if (grounded) 
			return;
		/*
			if(rigid.velocity == Vector2.zero) // needed if we are stuck
			{
				groundRayLength = 0.05f;
				grounded = true;
				boxCollider.sharedMaterial = normalPhysMat;
			}*/

		// Going upwards
		if (rigid.velocity.y > 0)
		{
			var collision = CheckForCollision(Vector2.up);

			// Did we hit something going up?
			if (!collision.IsNull)
			{
				// Is it slideable
				if (collision.IsSlideable)
				{
					boxCollider.sharedMaterial = slidePhysMat;
				}
				else
				{
					// Bounce and apply shake
					boxCollider.sharedMaterial = bouncePhysMat;
					if (collision.GetLength < minRayLength && rigid.velocity.y > 5)
						StartCoroutine(GameManager.Inst.GameCamera.effects.Shake(0.25f, 0.045f));
				}
			}
		}

		// Going left
		if (rigid.velocity.x < 0)
		{
			var collision = CheckForCollision(Vector2.left);

			// Did we hit something going left?
			if (!collision.IsNull)
			{
				// Is it slideable
				if (collision.IsSlideable)
				{
					boxCollider.sharedMaterial = slidePhysMat;
				}
				else
				{
					// Bounce and apply shake
					boxCollider.sharedMaterial = bouncePhysMat;
					if (collision.GetLength < minRayLength && rigid.velocity.x < -1) // Not slideable and we are moving fast
						StartCoroutine(GameManager.Inst.GameCamera.effects.Shake(0.25f, 0.045f));
				}
			}
		}

		// Going right
		if (rigid.velocity.x > 0)
		{
			var collision = CheckForCollision(Vector2.right);

			// Did we hit something going right?
			if (!collision.IsNull)
			{
				// Is it slideable
				if (collision.IsSlideable)
				{
					boxCollider.sharedMaterial = slidePhysMat;
				}
				else
				{
					// Bounce and apply shake
					boxCollider.sharedMaterial = bouncePhysMat;
					if (collision.GetLength < minRayLength && rigid.velocity.x > 1) // Not slideable and we are moving fast
						StartCoroutine(GameManager.Inst.GameCamera.effects.Shake(0.25f, 0.045f));
				}
			}
		}
	}

	private IEnumerator Finish()
	{
		dying = true;
		yield return new WaitForSeconds(5);
		foreach (var dontDestroy in FindObjectsOfType<DontDestroy>())
		{
			SceneManager.MoveGameObjectToScene(dontDestroy.gameObject, SceneManager.GetActiveScene());
		}
		SceneManager.LoadScene(0);
	}

	public IEnumerator Die()
	{
		if (dying) 
			yield break;

		dying = true;
		var amt = PlayerPrefs.GetFloat("FlashEffects");
		AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(0);
		asyncLoad.allowSceneActivation = false;

		yield return FadeOut(0.1f / amt, Color.red);
		yield return FadeOut(0.1f / amt, Color.white);
		yield return FadeOut(0.05f / amt, Color.clear);
		yield return new WaitForSeconds(1);
		foreach (var dontDestroy in FindObjectsOfType<DontDestroy>())
		{
			SceneManager.MoveGameObjectToScene(dontDestroy.gameObject, SceneManager.GetActiveScene());
		}
		asyncLoad.allowSceneActivation = true;

	}

	IEnumerator FadeOut(float duration, Color fadeColor)
	{ 
		float time = 0;
		var startValue = sprite.color;
		var endValue = fadeColor;
		while (time < duration)
		{
			var t = time / duration;

			t = t * t * t * (t * (6f * t - 15f) + 10f); // Ease in and out smoothly

			sprite.color = Color.Lerp(startValue, endValue, t);
			time += Time.deltaTime;

			yield return null;
		}
		sprite.color = endValue;
	}

	private void Update()
	{
		if(dying)
			return;
		HandleCollisions();

		if (Input.GetKeyDown(KeyCode.Escape) && menu)
		{
			if (!inMenu)
			{
				menu.ShowMenu();
			}
			else
			{
				menu.HideMenu();
			}
		}

		// When the space key is released, execute a jump
		if (Input.GetKeyUp(jumpKeyCode))
		{
			if (IsGrounded)
			{
				rigid.velocity = Vector2.zero;
				Jump(heldTime, moveInput);
			}
		}

		// When the space key is held, count the time its held for
		if (Input.GetKey(jumpKeyCode))
			heldTime += Time.deltaTime;
		else
			heldTime = 0;

		if (moveInput > 0) 
			transform.localScale = new Vector3(1f, 1f, 1f); 
		else if (moveInput < 0)
			transform.localScale = new Vector3(-1f, 1f, 1f);
		

		if (!grounded)
		{
			if (!airborn) airborn = !airborn;
		}
		else
		{
			if (airborn && rigid.velocity.y == 0)
			{
				//StartCoroutine(GameManager.Inst.GameCamera.effects.Shake(0.25f, 0.045f));
				airborn = false;
			}
		}
	}

	private void FixedUpdate()
	{
		if(dying)
			return;
		// Old grounded check via overlapping hitbox
		//grounded = Physics2D.OverlapCircle(checkingPoint.transform.position, checkRadius, groundLayer);
		//moveInput = Input.GetAxis("Horizontal");

		if (Input.GetKey(leftKeyCode))
			moveInput = Mathf.MoveTowards(moveInput, -1, 0.05f);
		else if (Input.GetKey(rightKeyCode))
			moveInput = Mathf.MoveTowards(moveInput, 1, 0.05f);
		else
			moveInput = Mathf.MoveTowards(moveInput, 0, 0.1f);;
	}

	// Calculates the force for a jump,
	// then executes it.
	private void Jump(float heldtime, float direction)
	{
		//transform.localScale = direction < 0 ? new Vector3(-1f, 1f, 1f) : new Vector3(1f, 1f, 1f);
		var jumpForce = jumpMult * 100;
		var moveForce = airSpeed * 100;
		var jumpVector = heldtime < adjustedJumpTime ? new Vector2(direction * moveForce, adjustedJumpTime * jumpForce) : new Vector2(direction * moveForce, Mathf.Clamp(heldtime, 0, maximumJumpTime) * jumpForce);

		rigid.AddRelativeForce(jumpVector);
		// We have added force therefore, we must not be on the ground
		grounded = false;
	}
}
