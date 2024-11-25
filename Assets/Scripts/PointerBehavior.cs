using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PointerBehavior : MonoBehaviour
{
    private PlayerInput playerInput;

    // Declare actions for input events
    private InputAction positionAction;
    private InputAction leftClickAction;
    private InputAction rightClickAction;
    private InputAction scrollAction;

    // Reference to the cursor circle (the sprite used as the pointer on the ground)
    public GameObject cursorCirclePrefab;  // Assign your sprite here in the Inspector
    private GameObject currentCursorCircle; // The instance of the cursor circle in the scene

    // Layer mask for the raycast (only hits the "Ground" layer)
    public LayerMask groundLayer;  // Make sure you assign this in the Inspector to the correct layer (e.g., Ground layer)
    public GameObject player;
    public GameObject currentPikmin;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        var actionMap = playerInput.actions.FindActionMap("Pointer");
        positionAction = actionMap.FindAction("Position");
        leftClickAction = actionMap.FindAction("LeftClick");
        rightClickAction = actionMap.FindAction("RightClick");
        scrollAction = actionMap.FindAction("Scroll");
    }


    private void OnEnable()
    {
        // Enable input actions
        positionAction.Enable();
        leftClickAction.Enable();
        rightClickAction.Enable();
        scrollAction.Enable();

        // Subscribe to input events
        positionAction.performed += OnMouseMove;
        leftClickAction.performed += OnLeftClick;
        rightClickAction.performed += OnRightClick;
        scrollAction.performed += OnScroll;
    }

    private void OnDisable()
    {
        // Unsubscribe from events and disable input actions
        positionAction.performed -= OnMouseMove;
        leftClickAction.performed -= OnLeftClick; // Ensure this matches the method name
        rightClickAction.performed -= OnRightClick;
        scrollAction.performed -= OnScroll;

        positionAction.Disable();
        leftClickAction.Disable();
        rightClickAction.Disable();
        scrollAction.Disable();
    }


    private void Update()
    {
        // We no longer need to raycast here, it's handled in OnMouseMove
    }

    private void OnMouseMove(InputAction.CallbackContext context)
    {
        // Get mouse position from input system
        Vector2 mousePosition = context.ReadValue<Vector2>();

        // Call RaycastFromMouse with the mouse position
        RaycastFromMouse(mousePosition);
    }

    private void OnLeftClick(InputAction.CallbackContext context)
    {
        Debug.Log("Left Click detected!");
        RaycastFromMouse(Mouse.current.position.ReadValue());  // Use the current mouse position
        FindPikmin();
    }

    private void OnRightClick(InputAction.CallbackContext context)
    {
        Debug.Log("Right Click detected!");
        // Handle right-click events here
    }

    private void OnScroll(InputAction.CallbackContext context)
    {
        Vector2 scrollDelta = context.ReadValue<Vector2>();
        Debug.Log($"Mouse Scroll: {scrollDelta.y}");

        if (scrollDelta.y > 0)
        {
            // Handle scroll up
            Debug.Log("Scrolling Up");
        }
        else if (scrollDelta.y < 0)
        {
            // Handle scroll down
            Debug.Log("Scrolling Down");
        }
    }

    private void RaycastFromMouse(Vector2 mousePosition)
    {
        // Perform raycast from the mouse position on the screen to the 3D world
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayer))  // Only hit objects on the Ground layer
        {
            // If we hit the ground, update the cursor circle position
            UpdatePointerPosition(hit);
        }
    }

    private void UpdatePointerPosition(RaycastHit hit)
    {
        // If we hit the ground, update the cursor circle position
        if (currentCursorCircle == null)
        {
            // Instantiate the circle on the first hit (if it doesn't already exist)
            currentCursorCircle = Instantiate(cursorCirclePrefab, hit.point, Quaternion.identity);
            currentCursorCircle.transform.rotation = Quaternion.Euler(90f, 0f, 0f); // Keep it flat on the ground (facing upwards)
        }
        else
        {
            // Update the position of the existing circle to the new hit point
            currentCursorCircle.transform.position = hit.point;
        }

        // To prevent clipping into the ground, offset the position slightly above the ground
        currentCursorCircle.transform.position = new Vector3(
            currentCursorCircle.transform.position.x,
            hit.point.y + 0.1f,  // Slightly above the ground
            currentCursorCircle.transform.position.z
        );

        // Optionally: Lock rotation on X and Z axes only (keep it flat on the ground)
        currentCursorCircle.transform.rotation = Quaternion.Euler(90f, 0f, 0f);  // Keep it facing up
    }

    private void FindPikmin()
    {
        // Find all Pikmin in the scene (assuming they have the "Pikmin" tag)
        GameObject[] allPikmin = GameObject.FindGameObjectsWithTag("Pikmin");

        if (allPikmin.Length == 0)
        {
            Debug.LogWarning("No Pikmin found in the scene.");
            currentPikmin = null;
            return;
        }

        // Initialize variables to keep track of the closest Pikmin
        GameObject closestPikmin = null;
        float closestDistance = Mathf.Infinity;

        // Loop through all Pikmin and find the closest one to the player
        foreach (GameObject pikmin in allPikmin)
        {
            // Calculate the distance between the player and the current Pikmin
            float distance = Vector3.Distance(player.transform.position, pikmin.transform.position);

            // If this Pikmin is closer than the current closest, update the variables
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPikmin = pikmin;
            }
        }

        // Store the closest Pikmin as the current Pikmin
        currentPikmin = closestPikmin;

        if (currentPikmin != null)
        {
            Debug.Log($"Closest Pikmin found: {currentPikmin.name} at distance {closestDistance}");
            PickUpPikmin(currentPikmin); // Call method to pick up the Pikmin
        }
    }

    private void PickUpPikmin(GameObject pikmin)
    {
        // Move Pikmin to player's position
        pikmin.transform.position = player.transform.position + Vector3.up * 1.5f; // Position above player
        pikmin.transform.parent = player.transform; // Parent Pikmin to the player

        // Disable Pikmin's movement
        PikminBehavior pikminBehavior = pikmin.GetComponent<PikminBehavior>();
        if (pikminBehavior != null)
        {
            pikminBehavior.PickedUp();
        }

        Debug.Log($"{pikmin.name} has been picked up by the player.");
    }


}
