//Script was created referencing https://assetstore.unity.com/packages/essentials/starter-assets-thirdperson-updates-in-new-charactercontroller-pa-196526
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    // general movement
    private Vector2 move;               
    [SerializeField] private float MoveSpeed; 
    private PlayerInput _playerInput;
    private CharacterController _controller;

    // camera
    [SerializeField] private GameObject CinemachineCameraTarget; 
    private Vector3 _initialCameraPosition;
    private Quaternion _initialCameraRotation;
    private Vector3 cameraOffset; 

    // direction and turning    
    private float _targetRotation = 0.0f;
    private float _rotationVelocity;
    private float _verticalVelocity;
    private float RotationSmoothTime = 0.12f; 

    private void Start()
    {
        _controller = GetComponent<CharacterController>();
        _playerInput = GetComponent<PlayerInput>();

        // Capture the initial position and rotation of the CinemachineCameraTarget
        _initialCameraPosition = CinemachineCameraTarget.transform.position;
        _initialCameraRotation = CinemachineCameraTarget.transform.rotation;

        cameraOffset = _initialCameraPosition - transform.position;

        // Set the camera to the initial position and rotation once at the start
        CinemachineCameraTarget.transform.position = _initialCameraPosition;
        CinemachineCameraTarget.transform.rotation = _initialCameraRotation;
    }

    public void OnMove(InputValue value)
    {
        MoveInput(value.Get<Vector2>());
    }

    private void MoveInput(Vector2 newMoveDirection)
    {
        move = newMoveDirection;
    }

    private void Update()
    {
        Move();
        CameraFollow(); // Ensure camera follows the player
    }

    private void Move()
    {
        // Normalize input direction for consistent movement speed
        Vector3 inputDirection = new Vector3(move.x, 0.0f, move.y).normalized;

        // Apply rotation only if there's input
        if (move != Vector2.zero)
        {
            _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg;
            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                RotationSmoothTime);

            transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
        }

        // Apply gravity
        if (_controller.isGrounded)
        {
            _verticalVelocity = 0f; // Reset vertical velocity when grounded
        }
        else
        {
            _verticalVelocity += Physics.gravity.y * Time.deltaTime; // Apply gravity over time
        }

        // Calculate target direction and move only if there is input
        Vector3 targetDirection = Vector3.zero;
        if (move != Vector2.zero)  // Check if there is any movement input
        {
            targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;
        }

        // Move the player only if there is input
        _controller.Move((targetDirection * MoveSpeed * Time.deltaTime) +
                        new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
    }

    private void CameraFollow()
    {
        // Keep the camera target's position following the player without modifying its rotation
        CinemachineCameraTarget.transform.position = transform.position + cameraOffset;

        // Reset the rotation of the camera target to the initial fixed rotation
        CinemachineCameraTarget.transform.rotation = Quaternion.Euler( // Locking rotation
            _initialCameraRotation.eulerAngles.x, // Maintain initial pitch
            _initialCameraRotation.eulerAngles.y, // Maintain initial yaw
            _initialCameraRotation.eulerAngles.z  // Maintain initial roll (if any)
        );
    }
}

