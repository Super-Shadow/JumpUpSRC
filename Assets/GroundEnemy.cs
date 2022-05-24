using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class GroundEnemy : MonoBehaviour
{
	[SerializeField] private Camera cameraController;
	[SerializeField] private SpriteRenderer sprite;

	public float speed = 100;

	private BoxCollider2D boxCollider;
	[SerializeField] private LayerMask groundLayer;
	[SerializeField] private LayerMask playerLayer;
	private Rigidbody2D rigid;
	private bool grounded;
	private bool dying;

	private float checkRayLength = 0.3f; // We use this just to check if ground is bouncy or not
	private float rayPosOffset = 0; // This is used to offset the ray position inwards from the corners of the collider (or outwards by -)

	private Vector2 rigidDirection = Vector2.right;

	struct Collision
	{
		public Collider2D GetCollider => collider;
		public float GetLength { get; }
		public bool IsNull => collider == null;

		private Collider2D collider;

		public Collision(Collider2D y, float distance)
		{
			collider = y;
			GetLength = distance;
		}
	};

	private Collision CheckForCollision(Vector2 direction)
	{
		var position = transform.position;
		var boxColliderSize = boxCollider.size;

		// 2 Rays for each corner of box. Used to detect what side hit.
		// Initialise as checking down
		var ray1 = Physics2D.Raycast(new Vector2(position.x - (boxColliderSize.x / 2) + rayPosOffset, position.y - (boxColliderSize.y / 2)), Vector2.down, checkRayLength, groundLayer);
		var ray2 = Physics2D.Raycast(new Vector2(position.x + (boxColliderSize.x / 2) - rayPosOffset, position.y - (boxColliderSize.y / 2)), Vector2.down, checkRayLength, groundLayer);

		if (direction == Vector2.left)
		{
			ray1 = Physics2D.Raycast(new Vector2(position.x - (boxColliderSize.x / 2) + rayPosOffset, position.y - (boxColliderSize.y / 2) + rayPosOffset), Vector2.left, checkRayLength, playerLayer);
			ray2 = Physics2D.Raycast(new Vector2(position.x - (boxColliderSize.x / 2) + rayPosOffset, position.y + (boxColliderSize.y / 2) - rayPosOffset), Vector2.left, checkRayLength, playerLayer);
		}
		else if (direction == Vector2.right)
		{
			ray1 = Physics2D.Raycast(new Vector2(position.x + (boxColliderSize.x / 2) - rayPosOffset, position.y - (boxColliderSize.y / 2) + rayPosOffset), Vector2.right, checkRayLength, playerLayer);
			ray2 = Physics2D.Raycast(new Vector2(position.x + (boxColliderSize.x / 2) - rayPosOffset, position.y + (boxColliderSize.y / 2) - rayPosOffset), Vector2.right, checkRayLength, playerLayer);

		}
		else if (direction == Vector2.up)
		{
			ray1 = Physics2D.Raycast(new Vector2(position.x - (boxColliderSize.x / 2) + rayPosOffset, position.y + (boxColliderSize.y / 2)), Vector2.up, checkRayLength, playerLayer);
			ray2 = Physics2D.Raycast(new Vector2(position.x + (boxColliderSize.x / 2) - rayPosOffset, position.y + (boxColliderSize.y / 2)), Vector2.up, checkRayLength, playerLayer);
		}

		// Did we hit something?

		var didRay1Hit = ray1.collider != null;
		var didRay2Hit = ray2.collider != null;

		rigidDirection = didRay1Hit switch
		{
			// Hanging over cliff change direction
			true when !didRay2Hit => Vector2.left,
			false when didRay2Hit => Vector2.right,
			_ => rigidDirection
		};

		if (!didRay1Hit && !didRay2Hit) 
			return new Collision(null, 0);

		// Is it slideable
			
		return new Collision(didRay1Hit ? ray1.collider : ray2.collider, didRay1Hit ? ray1.distance : ray2.distance);

	}

	private void HandleCollisions()
	{
		// Going downwards
		CheckForCollision(Vector2.down);

		// Going upwards
		var collision = CheckForCollision(Vector2.up);
		// Did we hit something going up?
		if (!collision.IsNull)
		{
			if (collision.GetCollider.CompareTag("Player"))
			{
				collision.GetCollider.GetComponent<PlayerController>().IncreaseScore(10);

				StartCoroutine(Die());
			}
		}
		else
		{
			if (rigidDirection == Vector2.left)
			{
				// Going left
				collision = CheckForCollision(Vector2.left);
				// Did we hit something going left?
				if (!collision.IsNull)
				{
					if (collision.GetCollider.CompareTag("Player"))
					{
						StartCoroutine(collision.GetCollider.GetComponent<PlayerController>().Die());
					}
				}
			}

			if (rigidDirection == Vector2.right)
			{


				// Going right
				collision = CheckForCollision(Vector2.right);
				// Did we hit something going right?
				if (!collision.IsNull)
				{
					if (collision.GetCollider.CompareTag("Player"))
					{
						StartCoroutine(collision.GetCollider.GetComponent<PlayerController>().Die());
					}
				}
			}
		}

	}

	// Start is called before the first frame update
	void Start()
    {
	    rigid = GetComponent<Rigidbody2D>();
	    boxCollider = GetComponent<BoxCollider2D>();
    }

    // Update is called once per frame
    void Update()
    {
	    // Only update if we are in camera view
        if (transform.position.y < cameraController.transform.position.y -
            cameraController.orthographicSize || transform.position.y > cameraController.transform.position.y +
            cameraController.orthographicSize || dying)
			return;

        HandleCollisions();

        if (rigidDirection == Vector2.right) 
	        transform.localScale = new Vector3(1f, 1f, 1f); 
        else if (rigidDirection == Vector2.left)
	        transform.localScale = new Vector3(-1f, 1f, 1f);


    }

    private void FixedUpdate()
    {
	    // Only update if we are in camera view
	    if (transform.position.y < cameraController.transform.position.y -
	        cameraController.orthographicSize || transform.position.y > cameraController.transform.position.y +
	        cameraController.orthographicSize || dying)
		    return;
	    rigid.velocity = rigidDirection * (speed * Time.fixedDeltaTime);
    }

    public IEnumerator Die()
    {
	    if (dying) 
		    yield break;
	    dying = true;
	    rigid.velocity = Vector2.zero;
	    var amt = PlayerPrefs.GetFloat("FlashEffects");
	    yield return FadeOut(0.1f / amt, Color.red);
	    yield return FadeOut(0.1f / amt, Color.white);
	    yield return FadeOut(0.05f / amt, Color.clear);
	    gameObject.SetActive(false);
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
}
