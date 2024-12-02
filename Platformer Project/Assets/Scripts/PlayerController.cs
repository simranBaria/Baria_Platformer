using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Character states
    public enum CharacterState { idle, walk, jump, die, dash, wallcling, windup, groundpound, stun }
    public CharacterState currentCharacterState = CharacterState.idle;
    public CharacterState previousCharacterState = CharacterState.idle;

    // Facing direction
    public enum FacingDirection { left, right }
    public FacingDirection direction = FacingDirection.right;

    // Horizontal movement
    public float walkSpeed;
    public float maxWalkSpeed;
    public bool walking = false;
    public bool decelerate = false;
    public float acceleration;
    public float accelerationTime;
    float xMovement;

    // Dashing
    public bool dashing = false;
    public bool canDash = true;
    public float dashVelocity;
    public float dashTime;
    public float dashCooldown;
    public float dashDirection;

    // Vertical movement
    public float apexHeight;
    public float apexTime;
    public float initialJumpVelocity;
    public bool jumped = false;
    public float baseGravity;
    public float currentGravity;
    public float terminalSpeed;
    public float coyoteTime;
    public float coyoteTimeTimer;

    // Wall jump
    public float wallJumpDirection;
    public float wallJumpDistance;
    public bool wallJumping = false;
    public float wallClingFriction;

    // Ground pound
    public bool groundPounding = false;
    public float groundPoundWindUpHeight;
    public float groundPoundGravity;
    public bool windUpComplete = false;
    public bool stunComplete = false;
    public float groundPoundVelocityMultiplier;

    // Grounded
    public float groundCheck;
    public Transform groundPosition;

    // Walled
    public enum WalledState { left, right, none }
    public float wallCheck;
    public Transform leftWallPosition, rightWallPosition;

    // Health
    public int health = 10;

    Rigidbody2D rb;

    // Start is called before the first frame update
    void Start()
    {
        // Get the rigidbody
        rb = GetComponent<Rigidbody2D>();

        // Gravity
        baseGravity = -2 * apexHeight / Mathf.Pow(apexTime, 2);

        // Jump velocity
        initialJumpVelocity = 2 * apexHeight / apexTime;

        // Walk acceleration
        acceleration = maxWalkSpeed / accelerationTime;

        // Set the coyote time timer
        coyoteTimeTimer = coyoteTime;

        // Set the gravity
        currentGravity = baseGravity;
    }

    // Update is called once per frame
    void Update()
    {
        // Update state
        previousCharacterState = currentCharacterState;

        // HORIZONTAL MOVEMENT

        // Input is not allowed for the following checks
        if(!dashing && !wallJumping && !groundPounding)
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

            // Dash
            if (Input.GetKey(KeyCode.J) && canDash)
            {
                // Get the direction of the dash
                if (direction == FacingDirection.left) dashDirection = -1;
                else dashDirection = 1;

                walking = false;
                decelerate = false;
                StartCoroutine(Dash());
            }
        }

        // VERTICAL MOVEMENT

        // Coyote time
        if (!IsGrounded())
        {
            // Countdown
            if (coyoteTimeTimer > 0) coyoteTimeTimer -= Time.deltaTime;
            else coyoteTimeTimer = 0;
        }
        else coyoteTimeTimer = coyoteTime;

        // Up input
        if (Input.GetKey(KeyCode.W) && (IsGrounded() || coyoteTimeTimer > 0 || IsWalled() != WalledState.none) && !dashing && !groundPounding)
        {
            jumped = true;
            coyoteTimeTimer = 0;
        }

        // Ground pound
        if (Input.GetKey(KeyCode.S) && !groundPounding)
        {
            groundPounding = true;
            windUpComplete = false;
            stunComplete = false;
            groundPoundGravity = 0;

            // Disable literally everything
            walking = false;
            decelerate = false;
            jumped = false;
            dashing = false;
            wallJumping = false;
            walkSpeed = 0;
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

                // Dashing
                if (IsDashing()) currentCharacterState = CharacterState.dash;

                // Wall cling
                if(IsWalled() != WalledState.none) currentCharacterState = CharacterState.wallcling;

                // Ground pounding
                if (groundPounding) currentCharacterState = CharacterState.windup;

                break;

            // Walking
            case CharacterState.walk:
                // Stopped walking
                if (!IsWalking()) currentCharacterState = CharacterState.idle;

                // Jumping
                if (!IsGrounded()) currentCharacterState = CharacterState.jump;

                // Dashing
                if (IsDashing()) currentCharacterState = CharacterState.dash;

                // Ground pounding
                if (groundPounding) currentCharacterState = CharacterState.windup;

                break;

            // Idle
            case CharacterState.idle:
                // Walking
                if (IsWalking()) currentCharacterState = CharacterState.walk;

                // Jumping
                if (!IsGrounded()) currentCharacterState = CharacterState.jump;

                // Dashing
                if(IsDashing()) currentCharacterState = CharacterState.dash;

                // Ground pounding
                if (groundPounding) currentCharacterState = CharacterState.windup;

                break;

            // Dashing
            case CharacterState.dash:
                // Do not interupt the dash movement
                if (!IsDashing())
                {
                    // Not on the ground
                    if (!IsGrounded())
                    {
                        if (IsWalled() == WalledState.none) currentCharacterState = CharacterState.jump;
                        else currentCharacterState = CharacterState.wallcling;
                    }

                    // On the ground
                    if (IsWalking()) currentCharacterState = CharacterState.walk;
                    else currentCharacterState = CharacterState.idle;
                }

                // Ground pounding
                if (groundPounding) currentCharacterState = CharacterState.windup;

                break;

            // Wall cling
            case CharacterState.wallcling:
                // Either slid down to the floor or jumped off
                if(IsGrounded()) currentCharacterState = CharacterState.idle;
                else if (IsWalled() == WalledState.none) currentCharacterState = CharacterState.jump;

                // Ground pounding
                if (groundPounding) currentCharacterState = CharacterState.windup;

                break;

            // Ground pound wind up
            case CharacterState.windup:
                if (windUpComplete) currentCharacterState = CharacterState.groundpound;
                break;

            // Ground pound
            case CharacterState.groundpound:
                if (IsGrounded()) currentCharacterState = CharacterState.stun;
                break;

            // Stun
            case CharacterState.stun:
                if (stunComplete) currentCharacterState = CharacterState.idle;
                break;
        }

        // Player died
        if (IsDead()) currentCharacterState = CharacterState.die;
    }

    // Physics movement
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

        // Walk
        if (!dashing)
        {
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
        }

        xChange = xMovement * walkSpeed;

        // Apply gravity
        if (!IsGrounded())
        {
            // Wall clinging
            if (IsWalled() != WalledState.none) currentGravity = baseGravity / wallClingFriction;
            else if (groundPounding) currentGravity = groundPoundGravity;
            else currentGravity = baseGravity;

            // Cap the change of y to the fall speed
            if (yChange > terminalSpeed) yChange += currentGravity * Time.deltaTime;
            else yChange = terminalSpeed;
        }

        // Jump
        if (jumped)
        {
            yChange = initialJumpVelocity;

            // Wall jump
            if (IsWalled() != WalledState.none && !IsGrounded())
            {
                // Get wall jump direction
                if (IsWalled() == WalledState.left) wallJumpDirection = 1;
                else wallJumpDirection = -1;

                wallJumping = true;
                StartCoroutine(WallJump());
            }

            jumped = false;
        }

        // Ground pound wind up
        if (groundPounding && !windUpComplete) yChange = groundPoundWindUpHeight;

        // Stop the player's walk speed if they're against a wall
        if (IsWalled() != WalledState.none && !wallJumping)
        {
            walkSpeed = 0;
            decelerate = false;
            walking = false;
        }

        // Makes it so the player isn't zooming through the air
        if (!IsGrounded() && !dashing) xChange /= 2;

        // Update the movement
        rb.velocity = new Vector2(xChange, yChange);

        // Change visual direction
        if (xMovement > 0) direction = FacingDirection.right;
        else if (xMovement < 0) direction = FacingDirection.left;
    }

    // Coroutine to dash
    IEnumerator Dash()
    {
        // Dash
        dashing = true;
        canDash = false;
        xMovement = dashDirection;
        walkSpeed = dashVelocity;

        // Stop dashing
        yield return new WaitForSeconds(dashTime);
        dashing = false;
        walkSpeed = 0;

        // Cooldown
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    // Coroutine to wall jump
    IEnumerator WallJump()
    {
        // Wall jump
        xMovement = wallJumpDirection;
        walkSpeed = wallJumpDistance;

        // Stop wall jumping
        yield return new WaitForSeconds(apexTime);
        wallJumping = false;
    }

    // Checks if the player is walking
    public bool IsWalking()
    {
        // Player is walking if their velocity isn't 0
        if (rb.velocity.x != 0) return true;
        else return false;
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

    // Checks if the player is dashing
    public bool IsDashing()
    {
        return dashing;
    }

    // Checks the player's walled state
    public WalledState IsWalled()
    {
        // Player is facing the left wall and holding down the movement key
        if (Physics2D.Raycast(leftWallPosition.position, Vector2.left, wallCheck) && Input.GetKey(KeyCode.A)) return WalledState.left;

        // Player is facing the right wall and holding down the movement key
        else if (Physics2D.Raycast(rightWallPosition.position, Vector2.right, wallCheck) && Input.GetKey(KeyCode.D)) return WalledState.right;

        // Player is doing neither
        else return WalledState.none;
    }

    // Wind up animation complete
    public void OnWindUpAnimationComplete()
    {
        // Increase the gravity
        groundPoundGravity = baseGravity * groundPoundVelocityMultiplier;
        windUpComplete = true;
    }

    // Stun animation complete
    public void OnStunAnimationComplete()
    {
        // Stop the ground pounding state
        groundPounding = false;
        stunComplete = true;
    }
}
