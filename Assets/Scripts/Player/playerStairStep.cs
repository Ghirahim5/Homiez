using UnityEngine;

public class PlayerStairStepSystem
{
    private playerController _pc;

    public PlayerStairStepSystem(playerController controller)
    {
        _pc = controller;
    }

    public void StairStep()
    {
        _pc.takeoffSpeed = 5f;

        Vector2 movementInput = _pc.playerInputHandler.MovementInput;

        float rayDistance = 1f;
        Vector3 origin = _pc.feetRayPos;
        RaycastHit hit;

        // Determine primary direction based on input
        Vector3 inputDir = new Vector3(movementInput.x, 0f, movementInput.y).normalized;
        Vector3 mainDir = _pc.playerSkin.TransformDirection(inputDir);
        
        // Check if player is grounded via downward Raycast
        float groundCheckDistance = 2f;
        if (Physics.Raycast(_pc.transform.position, Vector3.down, out hit, groundCheckDistance) && hit.collider.CompareTag("Stairs"))
        {
            _pc.Animator.animator.SetBool("isFalling", false);
        }
        
        // Cast ray only in the main movement direction
        if (Physics.Raycast(origin, mainDir, out hit, rayDistance) && hit.collider.CompareTag("Stairs"))
        {
            if (_pc.playerInputHandler.JumpTriggered)
            {
                _pc.Animator.animator.SetTrigger("Jump");
                _pc.Animator.animator.SetBool("isRunning", false);
                _pc.Animator.animator.SetBool("isWalking", false);
                _pc.rb.linearVelocity = new Vector3(
                    _pc.rb.linearVelocity.x,
                    _pc.jumpForce,
                    _pc.rb.linearVelocity.z
                );
                return;
            }
            
            if(_pc.playerInputHandler.SprintTriggered && !_pc.playerInputHandler.CrouchTriggered)
            {
                _pc.Animator.animator.SetBool("isRunning", true);
            }
            else
            {
                _pc.Animator.animator.SetBool("isWalking", true);
            }
            
            float boost = _pc.playerInputHandler.SprintTriggered ? _pc.stepBoost * 2f : _pc.stepBoost;
            _pc.rb.linearVelocity = new Vector3(
                _pc.rb.linearVelocity.x,
                boost,
                _pc.rb.linearVelocity.z
            );
        }

        if (Mathf.Abs(movementInput.x) > 0 && Mathf.Abs(movementInput.y) > 0)
        {
            Vector3 diagonalDir = _pc.playerSkin.TransformDirection(new Vector3(movementInput.x, 0f, movementInput.y).normalized);
            if (Physics.Raycast(origin, diagonalDir, out hit, rayDistance) && hit.collider.CompareTag("Stairs"))
            {
                if (_pc.playerInputHandler.JumpTriggered)
                {
                    _pc.Animator.animator.SetTrigger("Jump");
                    _pc.Animator.animator.SetBool("isRunning", false);
                    _pc.Animator.animator.SetBool("isWalking", false);
                    _pc.rb.linearVelocity = new Vector3(
                        _pc.rb.linearVelocity.x,
                        _pc.jumpForce,
                        _pc.rb.linearVelocity.z
                    );
                    return;
                }

                if(_pc.playerInputHandler.SprintTriggered && !_pc.playerInputHandler.CrouchTriggered)
                {
                    _pc.Animator.animator.SetBool("isRunning", true);
                }
                else
                {
                    _pc.Animator.animator.SetBool("isWalking", true);
                }

                float boost = _pc.playerInputHandler.SprintTriggered ? _pc.stepBoost * 2f : _pc.stepBoost;
                _pc.rb.linearVelocity = new Vector3(
                    _pc.rb.linearVelocity.x,
                    boost,
                    _pc.rb.linearVelocity.z
                );
            }
        }
    }
}