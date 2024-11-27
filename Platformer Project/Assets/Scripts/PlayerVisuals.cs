using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PlayerController;

/// <summary>
/// This script manages updating the visuals of the character based on the values that are passed to it from the PlayerController.
/// NOTE: You shouldn't make changes to this script when attempting to implement the functionality for the W10 journal.
/// </summary>
public class PlayerVisuals : MonoBehaviour
{
    public Animator animator;
    public SpriteRenderer bodyRenderer;
    public PlayerController playerController;

    private int idleHash, walkingHash, jumpingHash, dieHash;

    // Start is called before the first frame update
    void Start()
    {
        idleHash = Animator.StringToHash("Idle");
        walkingHash = Animator.StringToHash("Walking");
        jumpingHash = Animator.StringToHash("Jumping");
        dieHash = Animator.StringToHash("Death");

    }

    // Update is called once per frame
    void Update()
    {
        VisualsUpdate();
    }

    //It is not recommended to make changes to the functionality of this code for the W10 journal.
    private void VisualsUpdate()
    {
        // Update animation based on state
        if(playerController.previousCharacterState != playerController.currentCharacterState)
        {
            switch(playerController.currentCharacterState)
            {
                case CharacterState.idle:
                    animator.CrossFade(idleHash, 0f);
                    break;

                case CharacterState.walk:
                    animator.CrossFade(walkingHash, 0f);
                    break;

                case CharacterState.jump:
                    animator.CrossFade(jumpingHash, 0f);
                    break;

                case CharacterState.die:
                    animator.CrossFade(dieHash, 0f);
                    break;
            }
        }

        switch (playerController.GetFacingDirection())
        {
            case FacingDirection.left:
                bodyRenderer.flipX = true;
                break;
            case FacingDirection.right:
            default:
                bodyRenderer.flipX = false;
                break;
        }
    }
}
