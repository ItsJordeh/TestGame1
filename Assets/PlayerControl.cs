using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    
    private float movementInputDirection;

    private int jumpsLeft;

    private bool isFacingRight = true;
    private bool isWalking;
    private bool isGrounded;
    private bool isWallJumping;

    private bool isWallSliding;
    private bool canJump;
    private bool isTouchingWall;

    private Rigidbody2D rb;
    private Animator animator;

    public int numJumps = 1;

    public float moveSpeed = 10f;
    public float jumpForce = 5f;

    public float wallCheckDistance;

    public float airMoveForce;

    public float wallSlideSpeed;

    public float groundCheckRadius;

    public float airDragMultiplier = 0.95f;
    public float varJumpHeightMultiplier = 0.5f;

    public float wallJumpLaunchSpeed = 6f;

    public LayerMask whatIsGround;

    public Transform groundCheck;
    public Transform frontWallCheck;
    public Transform backWallCheck;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        jumpsLeft = numJumps;
    }

    // Update is called once per frame
    void Update()
    {
        CheckInput();
        CheckMovementDirection();
        UpdateAnimations();
        CheckIfCanJump();
        CheckIfWallSliding();
        CheckIfWallJumping();
    }

    private void FixedUpdate()
    {
        ApplyMovement();
        CheckSurroundings();
    }

    private void CheckSurroundings()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, whatIsGround);

        isTouchingWall = Physics2D.Raycast(frontWallCheck.position, transform.right, wallCheckDistance, whatIsGround);
        if(!isTouchingWall)
        {
            isTouchingWall = Physics2D.Raycast(backWallCheck.position, -transform.right, wallCheckDistance, whatIsGround);
        }

    }
    private void CheckIfCanJump()
    {
        if(isGrounded && rb.velocity.y <= 0)
        {
            jumpsLeft = numJumps;
        }
        
        if(jumpsLeft <= 0)
        {
            canJump = false;
        }
        else if(isWallSliding)
        {
            canJump = true;
        }
        else
        {
            canJump = true;
        }

        
    }
    private void CheckIfWallJumping()
    {
        if(isGrounded || isWallSliding)
        {
            isWallJumping = false;
        }
        
        

        
    }

    private void CheckInput()
    {
        movementInputDirection = Input.GetAxisRaw("Horizontal");


        if(Input.GetButtonDown("Jump"))
        {
            Jump();
        }


        if(Input.GetButtonUp("Jump"))
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * varJumpHeightMultiplier);
        }

    }

    private void Jump()
    {
        if(canJump)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            jumpsLeft--;
        }
        else if(isWallSliding)
        {
            if(isFacingRight)
            {
                rb.velocity = new Vector2(-wallJumpLaunchSpeed/2, wallJumpLaunchSpeed);
                
            }
            else if(!isFacingRight)
            {
                rb.velocity = new Vector2(wallJumpLaunchSpeed/2, wallJumpLaunchSpeed);
                
            }
            isFacingRight = !isFacingRight;
            transform.Rotate(0.0f, 180.0f, 0.0f);
            isWallSliding = false;
            isWallJumping = true;
        }
        
    }

    private void ApplyMovement()
    {
        if(isGrounded)
        {
            rb.velocity = new Vector2(moveSpeed * movementInputDirection, rb.velocity.y);
        }
        else if(isWallJumping)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y);
        }
        else if(!isGrounded && !isWallSliding && isWallJumping)
        {
            rb.velocity = new Vector2(rb.velocity.x * airDragMultiplier, rb.velocity.y);
        }
        else if(!isGrounded && !isWallSliding && !isWallJumping && movementInputDirection != 0)
        {
            Vector2 addForce = new Vector2(airMoveForce * movementInputDirection, 0);
            rb.AddForce(addForce);

            if(Mathf.Abs(rb.velocity.x) > moveSpeed)
            {
                rb.velocity = new Vector2(moveSpeed * movementInputDirection, rb.velocity.y);
            }
        }
        else if(!isGrounded && !isWallSliding && movementInputDirection == 0)
        {
            rb.velocity = new Vector2(rb.velocity.x * airDragMultiplier, rb.velocity.y);
        }


        if(isWallSliding)
        {
            if((movementInputDirection <= 0 && isFacingRight) || (movementInputDirection >= 0 && !isFacingRight) )
            {
                rb.velocity = new Vector2(moveSpeed * movementInputDirection, rb.velocity.y);
            }

            if(rb.velocity.y < -wallSlideSpeed)
            {
                rb.velocity = new Vector2(rb.velocity.x, -wallSlideSpeed);
            }
        }
        
    }

    private void CheckIfWallSliding()
    {
        if(isTouchingWall && !isGrounded && rb.velocity.y < 0)
        {
            isWallSliding = true;
            jumpsLeft = 0;
        }
        else
        {
            isWallSliding = false;
        }
    }

    private void CheckMovementDirection()
    {
        if(isFacingRight && movementInputDirection < 0)
        {
            Flip();
        }
        else if(!isFacingRight && movementInputDirection > 0)
        {
            Flip();
        }

        if(Mathf.Abs(rb.velocity.x) >= 0.25 && isGrounded)
        {
            isWalking = true;
        }
        else
        {
            isWalking = false;
        }

    }

    private void UpdateAnimations()
    {
        animator.SetBool("isWalking", isWalking);
        animator.SetBool("isGrounded", isGrounded);
        animator.SetFloat("yVelocity", rb.velocity.y);
        animator.SetBool("isWallSliding", isWallSliding);
    }

    private void Flip()
    {
        if(!isWallSliding)
        {
            isFacingRight = !isFacingRight;
            transform.Rotate(0.0f, 180.0f, 0.0f);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);

        Gizmos.DrawLine(frontWallCheck.position, new Vector3(frontWallCheck.position.x + wallCheckDistance, frontWallCheck.position.y, frontWallCheck.position.z));
    }

}
