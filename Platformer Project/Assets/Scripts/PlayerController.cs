using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Character states
    public enum CharacterState { idle, walk, jump, die }
    public CharacterState currentCharacterState = CharacterState.idle;
    public CharacterState previousCharacterState = CharacterState.idle;

    // Facing direction
    public enum FacingDirection { left, right }
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
    public float gravity;
    public float terminalSpeed;
    public float coyoteTime;
    public float coyoteTimeTimer;

    // Grounded
    public float groundCheck;
    public Transform groundPosition;

    // Health
    public int health = 10;

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

        // Set the coyote time timer
        coyoteTimeTimer = coyoteTime;
    }

    // Update is called once per frame
    void Update()
    {
        // Update state
        previousCharacterState = currentCharacterState;

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

        // Coyote time
        if (!IsGrounded())
        {
            if (coyoteTimeTimer > 0) coyoteTimeTimer -= Time.deltaTime;
            else coyoteTimeTimer = 0;
        }
        else coyoteTimeTimer = coyoteTime;

        // Vertical input
        if (Input.GetKey(KeyCode.W) && (IsGrounded() || coyoteTimeTimer > 0))
        {
            jumped = true;
            coyoteTimeTimer = 0;
        }

        // Change player state
        switch (currentCharacterState)
        {
            // Death
            case CharacterState.die:
                // No transition out of death
                break;

            // Jumping
            case CharacterState.jump:
                // Landed
                if (IsGrounded())
                {
                    // Either walking or idle
                    if (IsWalking()) currentCharacterState = CharacterState.walk;
                    else currentCharacterState = CharacterState.idle;
                }

                break;

            // Walking
            case CharacterState.walk:
                // Stopped walking
                if (!IsWalking()) currentCharacterState = CharacterState.idle;

                // Jumping
                if (!IsGrounded()) currentCharacterState = CharacterState.jump;

                break;

            // Walking
            case CharacterState.idle:
                // Walking
                if (IsWalking()) currentCharacterState = CharacterState.walk;

                // Jumping
                if (!IsGrounded()) currentCharacterState = CharacterState.jump;

                break;
        }

        // Player died
        if (IsDead()) currentCharacterState = CharacterState.die;
    }

    private void FixedUpdate()
    {
        // Move the player
        MovementUpdate(xMovement);
    }

    // Method to move the player
    private void MovementUpdate(float xMovement)
    {
        float xChange;
        float yChange = rb.velocity.y;

        // Apply gravity
        if (!IsGrounded())
        {
            // Cap the change of y to the fall speed
            if (yChange > terminalSpeed) yChange += gravity * Time.deltaTime;
            else yChange = terminalSpeed;
        }

        // Jump
        if (jumped)
        {
            yChange = initialJumpVelocity;
            jumped = false;
        }

        // Walk
        if (walking)
        {
            // Accelerate until max speed is reached
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

        // Makes it so the player isn't zooming through the air
        if (!IsGrounded()) xChange /= 2;

        // Update the movement
        rb.velocity = new Vector2(xChange, yChange);

        // Change visual direction
        if (xMovement > 0) direction = FacingDirection.right;
        else if (xMovement < 0) direction = FacingDirection.left;
    }

    // Checks if the player is walking
    public bool IsWalking()
    {
        return walking;
    }

    // Checks if the player is on the ground
    public bool IsGrounded()
    {
        // Checks if the raycast hits an object underneath the player at 0.1 distance
        return Physics2D.Raycast(groundPosition.position, Vector2.down, groundCheck);
    }

    // Gets the direction the player is facing
    public FacingDirection GetFacingDirection()
    {
        // Return the facing direction
        return direction;
    }

    // Set the player to dead
    public bool IsDead()
    {
        if (health <= 0) return true;
        else return false;
    }

    // Deactivate the dead player
    public void OnDeathAnimationComplete()
    {
        gameObject.SetActive(false);
    }
}
