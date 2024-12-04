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
    public CameraController cameraController;

    private int idleHash, walkingHash, jumpingHash, dieHash, dashHash, wallclingHash, windupHash, groundpoundHash, stunHash;

    // Start is called before the first frame update
    void Start()
    {
        idleHash = Animator.StringToHash("Idle");
        walkingHash = Animator.StringToHash("Walking");
        jumpingHash = Animator.StringToHash("Jumping");
        dieHash = Animator.StringToHash("Death");
        dashHash = Animator.StringToHash("Dash");
        wallclingHash = Animator.StringToHash("Wall Cling");
        windupHash = Animator.StringToHash("Wind Up");
        groundpoundHash = Animator.StringToHash("Ground Pound");
        stunHash = Animator.StringToHash("Stun");
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
                    if(playerController.previousCharacterState == CharacterState.jump) cameraController.Shake(5f, 0.35f);

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
                case CharacterState.dash:
                    animator.CrossFade(dashHash, 0f);
                    break;
                case CharacterState.wallcling:
                    animator.CrossFade(wallclingHash, 0f);
                    break;
                case CharacterState.windup:
                    animator.CrossFade(windupHash, 0f);
                    break;
                case CharacterState.groundpound:
                    animator.CrossFade(groundpoundHash, 0f);
                    break;
                case CharacterState.stun:
                    animator.CrossFade(stunHash, 0f);
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
