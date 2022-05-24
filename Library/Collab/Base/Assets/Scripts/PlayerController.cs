using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Script for anything movement or control related
 * Will handle movement left, right, the jumping code
 * Collision with enemies and objects ect...
 * 
 * For now most things can be done here, if the script needs
 * to be broken up, it can be down the line.
*/

public class PlayerController : MonoBehaviour
{

    [SerializeField] private float jumpMult;     
    [SerializeField] private float moveSpeed;
    [SerializeField] private float airSpeed;
    [SerializeField] private Rigidbody2D rigid;
    [SerializeField] private BoxCollider2D boxCollider;
    [SerializeField] private PhysicsMaterial2D normalPhysMat;
    [SerializeField] private PhysicsMaterial2D bouncePhysMat;
    
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
    //[SerializeField] private float checkRadius;
    //[SerializeField] private Transform checkingPoint;
    protected bool grounded;
    protected bool airborn;
    private float moveInput;

    // 2 Rays for each corner of box. Used to detect what side a player hit.
    private RaycastHit2D leftDownRay;
    private RaycastHit2D rightDownRay;
    private RaycastHit2D leftUpRay;
    private RaycastHit2D rightUpRay;
    private RaycastHit2D downLeftRay;
    private RaycastHit2D downRightRay;
    private RaycastHit2D upLeftRay;
    private RaycastHit2D upRightRay;

    public bool IsGrounded { get => grounded; }

    private float groundRayLength = 0.05f; // This is used to increase ray length for ground check since if we have bounce enabled, we bounce before we detect floor so use this to increase

    private void Start()
    {
        rigid = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        boxCollider.sharedMaterial = normalPhysMat;
    }

    private void Update()
    {
             
        downLeftRay = Physics2D.Raycast(new Vector2(transform.position.x - (boxCollider.size.x/2), transform.position.y - (boxCollider.size.y/2)), Vector2.down, groundRayLength, groundLayer);
        downRightRay = Physics2D.Raycast(new Vector2(transform.position.x + (boxCollider.size.x/2), transform.position.y - (boxCollider.size.y/2)), Vector2.down, groundRayLength, groundLayer);

        upLeftRay = Physics2D.Raycast(new Vector2(transform.position.x - (boxCollider.size.x/2), transform.position.y + (boxCollider.size.y/2)), Vector2.up, 0.5f, groundLayer);
        upRightRay = Physics2D.Raycast(new Vector2(transform.position.x + (boxCollider.size.x/2), transform.position.y + (boxCollider.size.y/2)), Vector2.up, 0.5f, groundLayer);

        leftDownRay = Physics2D.Raycast(new Vector2(transform.position.x - (boxCollider.size.x/2), transform.position.y - (boxCollider.size.y/2)), Vector2.left, 0.5f, groundLayer);
        leftUpRay = Physics2D.Raycast(new Vector2(transform.position.x - (boxCollider.size.x/2), transform.position.y + (boxCollider.size.y/2)), Vector2.left, 0.5f, groundLayer);

        rightDownRay = Physics2D.Raycast(new Vector2(transform.position.x + (boxCollider.size.x/2), transform.position.y - (boxCollider.size.y/2)), Vector2.right, 0.5f, groundLayer);
        rightUpRay = Physics2D.Raycast(new Vector2(transform.position.x + (boxCollider.size.x/2), transform.position.y + (boxCollider.size.y/2)), Vector2.right, 0.5f, groundLayer);

        if(downLeftRay.collider != null || downRightRay.collider != null)
		{
            groundRayLength = 0.05f;
            grounded = true;
            boxCollider.sharedMaterial = normalPhysMat;
		}
        else
            grounded = false; // Must be seperate otherwise it doesn't always catch for some reason!
        
        if(!grounded && (upLeftRay.collider != null || upRightRay.collider != null || 
                leftDownRay.collider != null || leftUpRay.collider != null || 
                rightDownRay.collider != null || rightUpRay.collider != null))
		{
            boxCollider.sharedMaterial = bouncePhysMat;
            groundRayLength = 0.5f;
            StartCoroutine(GameManager.Inst.GameCamera.effects.Shake(0.25f, 0.045f)); // camera shake on bounce!
		}

        // When the space key is released, execute a jump
        if (Input.GetKeyUp(KeyCode.Space))
        {
            if (IsGrounded)
            {
                rigid.velocity = Vector2.zero;
                Jump(heldTime, moveInput);
            }
        }

        // When the space key is held, count the time its held for
        if (Input.GetKey(KeyCode.Space))
        {
            heldTime += Time.deltaTime;     
        } else { 
            heldTime = 0; 
        }

        if (!grounded)
        {
            if (!airborn) airborn = !airborn;
        } else
        {
            if (airborn)
            {
                StartCoroutine(GameManager.Inst.GameCamera.effects.Shake(0.25f, 0.045f));
                airborn = false;
            }
        }
    }

    private void FixedUpdate()
    {
        // Old grounded check via overlapping hitbox
        //grounded = Physics2D.OverlapCircle(checkingPoint.transform.position, checkRadius, groundLayer);

        moveInput = Input.GetAxis("Horizontal");  
    }

    // Calculates the force for a jump,
    // then executes it.
    protected void Jump(float heldTime, float direction)
    {
        if (direction < 0) { transform.localScale = new Vector3(-1f, 1f, 1f); }
        else { transform.localScale = new Vector3(1f, 1f, 1f); }
        float jumpForce = jumpMult * 100;
        float moveForce = airSpeed * 100;
        Vector2 jumpVector;
        if(heldTime < adjustedJumpTime)
        {
            jumpVector = new Vector2(direction * moveForce, adjustedJumpTime * jumpForce);
        } else
        {
            jumpVector = new Vector2(direction * moveForce, Mathf.Clamp(heldTime, 0, maximumJumpTime) * jumpForce);
        }

        Debug.Log(jumpVector);
        rigid.AddRelativeForce(jumpVector);
    }
}
