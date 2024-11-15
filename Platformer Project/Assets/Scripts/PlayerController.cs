using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public enum FacingDirection
    {
        left, right
    }

    FacingDirection direction;
    Rigidbody2D rb;
    public float playerSpeed, maxSpeed, acceleration, accelerationTime;
    public bool decelerate;
    public float distanceToGround;
    public float jumpHeight;

    // Start is called before the first frame update
    void Start()
    {
        // Get the rigidbody
        rb = GetComponent<Rigidbody2D>();

        // Calculate acceleration
        acceleration = maxSpeed / accelerationTime;

        // Default value
        decelerate = false;
        direction = FacingDirection.right;

        // Get the distance to the ground
        distanceToGround = GetDistanceToGround();
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 playerInput;

        // Player stopped moving
        if (Input.GetKeyUp(KeyCode.D) || Input.GetKeyUp(KeyCode.A)) decelerate = true;

        // Moving right
        if (Input.GetKey(KeyCode.D))
        {
            direction = FacingDirection.right;
            playerInput = Vector2.right;
            decelerate = false;
        }
        // Moving left
        else if (Input.GetKey(KeyCode.A))
        {
            direction = FacingDirection.left;
            playerInput = Vector2.left;
            decelerate = false;
        }
        else playerInput = Vector2.zero;

        // Move the player
        MovementUpdate(playerInput);
    }

    private void MovementUpdate(Vector2 playerInput)
    {
        // Allow the player to jump if they're grounded
        if (Input.GetKeyDown(KeyCode.W) && IsGrounded()) rb.AddForce(new Vector2(rb.velocity.x, jumpHeight), ForceMode2D.Impulse);

        // Don't move if no input
        if (playerInput != Vector2.zero || decelerate)
        {
            // Decelerate speed
            if (decelerate)
            {
                // Decelerate until stopped
                if (playerSpeed > 0) playerSpeed -= acceleration * Time.deltaTime;
                else
                {
                    playerSpeed = 0;
                    decelerate = false;
                }
            }
            // Accelerate speed
            else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.A))
            {
                // Accelerate until reached max speed
                if (playerSpeed < maxSpeed) playerSpeed += acceleration * Time.deltaTime;
                else playerSpeed = maxSpeed;
            }

            // Move the player
            rb.AddForce(playerInput * playerSpeed);
            //rb.MovePosition(rb.position + playerInput * playerSpeed * Time.deltaTime);
        }
    }

    public bool IsWalking()
    {
        // The player is walking if their speed is greater than 0
        if (playerSpeed > 0) return true;
        else return false;
    }

    public bool IsGrounded()
    {
        // Checks if the raycast hits an object underneath the player at 0.1 distance
        return Physics2D.Raycast(transform.position, Vector2.down, distanceToGround + 0.1f);
    }

    public FacingDirection GetFacingDirection()
    {
        // Return the facing direction
        return direction;
    }

    public float GetDistanceToGround()
    {
        // Get the distance to the ground from the middle of the rigidbody
        return Physics2D.Raycast(rb.position, Vector2.down).distance;
    }
}
