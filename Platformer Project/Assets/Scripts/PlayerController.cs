using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Facing direction
    public enum FacingDirection
    {
        left, right
    }
    FacingDirection direction = FacingDirection.right;

    // Horizontal movement
    public float walkSpeed;
    public float maxWalkSpeed;
    public bool walking = false;
    public bool decelerate = false;
    public float acceleration;
    public float accelerationTime;
    float xMovement;

    // Vertical movement
    public float apexHeight;
    public float apexTime;
    public float initialJumpVelocity;
    public bool jumped = false;
    public float maxFallSpeed;
    public float gravity;

    // Grounded
    public float groundCheck;
    public Transform groundPosition;

    // Roofed
    public float roofCheck;
    public Transform roofPosition;

    Rigidbody2D rb;

    // Start is called before the first frame update
    void Start()
    {
        // Get the rigidbody
        rb = GetComponent<Rigidbody2D>();

        // Gravity
        gravity = -2 * apexHeight / Mathf.Pow(apexTime, 2);

        // Jump velocity
        initialJumpVelocity = 2 * apexHeight / apexTime;

        // Walk acceleration
        acceleration = maxWalkSpeed / accelerationTime;
    }

    // Update is called once per frame
    void Update()
    {
        // Horizontal input stopped
        if (Input.GetKeyUp(KeyCode.D) || Input.GetKeyUp(KeyCode.A))
        {
            decelerate = true;
            walking = false;
        }

        // Right input
        if (Input.GetKey(KeyCode.D))
        {
            xMovement = 1;
            direction = FacingDirection.right;
            walking = true;
        }
        // Left input
        else if (Input.GetKey(KeyCode.A))
        {
            xMovement = -1;
            direction = FacingDirection.left;
            walking = true;
        }

        // Vertical input
        if (Input.GetKey(KeyCode.W) && IsGrounded()) jumped = true;

        MovementUpdate(xMovement);
    }

    private void MovementUpdate(float xMovement)
    {
        float xChange;
        float yChange = rb.velocity.y;

        // Apply gravity
        if (!IsGrounded()) yChange += gravity * Time.deltaTime;

        // Jump
        if (jumped)
        {
            yChange = initialJumpVelocity;
            jumped = false;
        }

        // Limit fall velocity
        //if (rb.velocity.y < -maxFallSpeed) rb.velocity = new Vector2(rb.velocity.x, maxFallSpeed);

        // Walk

        if (walking)
        {
            if (walkSpeed < maxWalkSpeed) walkSpeed += acceleration * Time.deltaTime;
            else walkSpeed = maxWalkSpeed;
        }
        // Decelerate
        else if (decelerate)
        {
            // Decelerate until stopped
            if (walkSpeed > 0) walkSpeed -= acceleration * Time.deltaTime;
            else
            {
                walkSpeed = 0;
                decelerate = false;
                xMovement = 0;
            }
        }
        xChange = xMovement * walkSpeed;

        rb.velocity = new Vector2(xChange, yChange);

        // Change visual direction
        if (xMovement > 0) direction = FacingDirection.right;
        else if (xMovement < 0) direction = FacingDirection.left;

        //// Don't move if no input
        //if (playerInput != Vector2.zero || decelerate)
        //{
        //    Debug.Log("test if in here");
        //    // Decelerate speed
        //    if (decelerate)
        //    {
        //        // Decelerate until stopped
        //        if (playerSpeed > 0) playerSpeed -= acceleration * Time.deltaTime;
        //        else
        //        {
        //            playerSpeed = 0;
        //            decelerate = false;
        //        }
        //    }
        //    // Accelerate speed
        //    else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.A))
        //    {
        //        // Accelerate until reached max speed
        //        if (playerSpeed < maxSpeed) playerSpeed += acceleration * Time.deltaTime;
        //        else playerSpeed = maxSpeed;
        //    }

        //    // Move the player
        //    currentVelocity = new Vector2(playerSpeed * Time.deltaTime * playerInput.x, currentVelocity.y);
        //}

        //// Apply gravity
        //if (verticalVelocity > gravity) verticalVelocity += gravity * Time.deltaTime;
        //else verticalVelocity = gravity;
        //currentVelocity = new Vector2(currentVelocity.x, verticalVelocity * Time.deltaTime);


        //rb.MovePosition(rb.position + currentVelocity);
    }

    public bool IsWalking()
    {
        return walking;
    }

    public bool IsGrounded()
    {
        // Checks if the raycast hits an object underneath the player at 0.1 distance
        return Physics2D.Raycast(groundPosition.position, Vector2.down, groundCheck);
    }

    public FacingDirection GetFacingDirection()
    {
        // Return the facing direction
        return direction;
    }

    public bool IsRoofed()
    {
        // Checks if the raycast hits an object above the player at 0.1 distance
        return Physics2D.Raycast(roofPosition.position, Vector2.up, roofCheck);
    }

    public void FallFast()
    {
        // Stops the player's jump fast once they hit a roof
        rb.velocity = new Vector2(rb.velocity.x, 0);
    }
}
