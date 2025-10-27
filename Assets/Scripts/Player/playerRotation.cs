using UnityEngine;

public class PlayerRotation
{
    private playerController _pc;

    public PlayerRotation(playerController controller)
    {
        _pc = controller;
    }

    // Reads input and applies the appropriate rotations to player and camera
    public void HandleRotation()
    {
        float mouseXRotation = _pc.playerInputHandler.RotationInput.x * _pc.mouseSensitivity;
        float mouseYRotation = _pc.playerInputHandler.RotationInput.y * _pc.mouseSensitivity;

        ApplyHorizontalRotation(mouseXRotation);
        ApplyVerticalRotation(mouseYRotation);
    }

    // Rotates the player horizontally based on input
    private void ApplyHorizontalRotation(float rotationAmount)
    {
        _pc.rb.MoveRotation(_pc.rb.rotation * Quaternion.Euler(0f, rotationAmount, 0f));
        if (_pc.mainCamera != null)
        {
            _pc.mainCamera.transform.Rotate(Vector3.up, rotationAmount, Space.World);
        }
    }

    // Rotates the camera vertically so the player can look up and down
    private void ApplyVerticalRotation(float rotationAmount)
    {
        _pc.verticalRotation = Mathf.Clamp(_pc.verticalRotation - rotationAmount, -_pc.upDownLookRange, _pc.upDownLookRange);
        _pc.mainCamera.transform.localRotation = Quaternion.Euler(_pc.verticalRotation, 0, 0);
    }
}