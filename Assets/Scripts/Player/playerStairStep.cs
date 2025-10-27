using UnityEngine;

public class PlayerStairStepSystem
{
    private playerController _pc;

    public PlayerStairStepSystem(playerController controller)
    {
        _pc = controller;
    }

    // Boosts the player up using force
    private void BoostStep()
    {
        // Get the player's forward direction relative to their skin
        Vector3 forwardDir = _pc.playerSkin.forward;

        // Determine if moving backward
        Vector2 movementInput = _pc.playerInputHandler.MovementInput;
        float forwardMultiplier = movementInput.y < 0 ? -1f : 1f;

        // Apply a small forward/backward boost along with the vertical boost
        float forwardBoost = 0.3f;
        _pc.rb.linearVelocity = new Vector3(
            _pc.rb.linearVelocity.x + forwardDir.x * forwardBoost * forwardMultiplier,
            _pc.stepBoost,
            _pc.rb.linearVelocity.z + forwardDir.z * forwardBoost * forwardMultiplier
        );
    }

    public void StairStep()
    {
        _pc.takeoffSpeed = 5f;

        Vector2 movementInput = _pc.playerInputHandler.MovementInput;

        float rayDistance = 1f;
        Vector3 origin = _pc.feetRayPos.transform.position;
        RaycastHit hit;

        // Determine primary direction based on input
        Vector3 inputDir = new Vector3(movementInput.x, 0f, movementInput.y).normalized;
        Vector3 mainDir = _pc.playerSkin.TransformDirection(inputDir);
        
        // Check if player is grounded via downward Raycast
        float groundCheckDistance = 1f;
        if (Physics.Raycast(_pc.transform.position, Vector3.down, out hit, groundCheckDistance) && hit.collider.CompareTag("Stairs"))
        {
            _pc.Animator.animator.SetBool("isFalling", false);
        }

        // Cast ray only in the main movement direction
        if (Physics.Raycast(origin, mainDir, out hit, rayDistance) && hit.collider.CompareTag("Stairs"))
        {
            _pc.Animator.animator.SetBool("isWalking", true);
            BoostStep();
        }

        if (Mathf.Abs(movementInput.x) > 0 && Mathf.Abs(movementInput.y) > 0)
        {
            Vector3 diagonalDir = _pc.playerSkin.TransformDirection(new Vector3(movementInput.x, 0f, movementInput.y).normalized);
            if (Physics.Raycast(origin, diagonalDir, out hit, rayDistance) && hit.collider.CompareTag("Stairs"))
            {
                _pc.Animator.animator.SetBool("isWalking", true);
                BoostStep();
            }
        }
    }
}