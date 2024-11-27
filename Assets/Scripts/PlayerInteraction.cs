using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    private PlayerInput playerInput;
    private InputAction positionAction;
    private InputAction leftClickAction;
    private InputAction rightClickAction;
    private InputAction scrollAction;
    public GameObject cursorCirclePrefab;
    public GameObject cursorCircleBigPrefab;
    private GameObject currentCursorCircle;
    private GameObject currentCursorCircleBig;
    private bool callingPikmin = false; 

    public LayerMask groundLayer;
    public LayerMask pikminLayer;
    public GameObject player;
    public GameObject currentPikmin;

    private Coroutine callPikminCoroutine;

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
        positionAction.Enable();
        leftClickAction.Enable();
        rightClickAction.Enable();
        scrollAction.Enable();

        positionAction.performed += OnMouseMove;
        leftClickAction.performed += OnLeftClick;
        leftClickAction.canceled += OnLeftClickRelease;
        rightClickAction.performed += OnTestClick;
        rightClickAction.canceled += OnRightClickRelease;
        scrollAction.performed += OnScroll;
    }

    private void OnDisable()
    {
        positionAction.performed -= OnMouseMove;
        leftClickAction.performed -= OnLeftClick;
        leftClickAction.canceled -= OnLeftClickRelease;
        rightClickAction.performed -= OnTestClick;
        rightClickAction.canceled -= OnRightClickRelease;
        scrollAction.performed -= OnScroll;

        positionAction.Disable();
        leftClickAction.Disable();
        rightClickAction.Disable();
        scrollAction.Disable();
    }

    private void OnMouseMove(InputAction.CallbackContext context)
    {
        Vector2 mousePosition = context.ReadValue<Vector2>();
        RaycastFromMouse(mousePosition);
    }

    private void OnLeftClick(InputAction.CallbackContext context)
    {
        FindPikmin();
    }

    private void OnLeftClickRelease(InputAction.CallbackContext context)
    {
        ThrowPikmin();
    }

    public void OnTestClick(InputAction.CallbackContext context)
    {
        if (callPikminCoroutine == null) // Start only if not already running
        {
            callPikminCoroutine = StartCoroutine(CallPikminContinuously());
        }
    }

    private void OnRightClickRelease(InputAction.CallbackContext context)
    {
        StopCallPikmin();
        if (callPikminCoroutine != null)
        {
            StopCoroutine(callPikminCoroutine);
            callPikminCoroutine = null;
        }
    }

    private void OnScroll(InputAction.CallbackContext context)
    {
        Vector2 scrollDelta = context.ReadValue<Vector2>();
        //Debug.Log($"Mouse Scroll: {scrollDelta.y}");

        if (scrollDelta.y > 0)
        {
            Debug.Log("Scrolling Up");
        }
        else if (scrollDelta.y < 0)
        {
            Debug.Log("Scrolling Down");
        }
    }

    private void RaycastFromMouse(Vector2 mousePosition)
    {
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayer))  // Only hit objects on the Ground layer
        {
            UpdatePointerPosition(hit);
        }
    }

    private void UpdatePointerPosition(RaycastHit hit)
    {
        GameObject circle;

        if (callingPikmin)
        {
            // If switching to the big cursor, destroy the small one if it exists
            if (currentCursorCircle != null)
            {
                Destroy(currentCursorCircle);
                currentCursorCircle = null;
            }

            if (currentCursorCircleBig == null)
            {
                currentCursorCircleBig = Instantiate(cursorCircleBigPrefab, hit.point, Quaternion.identity);
                currentCursorCircleBig.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            }
            circle = currentCursorCircleBig;
        }
        else
        {
            // If switching to the small cursor, destroy the big one if it exists
            if (currentCursorCircleBig != null)
            {
                Destroy(currentCursorCircleBig);
                currentCursorCircleBig = null;
            }

            if (currentCursorCircle == null)
            {
                currentCursorCircle = Instantiate(cursorCirclePrefab, hit.point, Quaternion.identity);
                currentCursorCircle.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            }
            circle = currentCursorCircle;
        }

        // Update position
        circle.transform.position = new Vector3(hit.point.x, hit.point.y + 0.5f, hit.point.z);
        circle.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
    }

    private IEnumerator CallPikminContinuously()
    {
        while (true)
        {
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            CallPikmin(mousePosition);
            yield return null; // Wait for the next frame to continue the loop
        }
    }

    private void CallPikmin(Vector2 mousePosition)
    {
        callingPikmin = true;
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        RaycastHit hit;
        float rayRadius = 2.5f; // Define the radius of the sphere (larger means bigger area)

        // Cast a sphere along the ray's path
        if (Physics.SphereCast(ray, rayRadius, out hit, Mathf.Infinity, pikminLayer))
        {
            // Check if the ray hits a GameObject with the "Pikmin" tag
            if (hit.collider != null && hit.collider.CompareTag("Pikmin"))
            {
                GameObject pikmin = hit.collider.gameObject;
                PikminBehavior pikminBehavior = pikmin.GetComponent<PikminBehavior>();
                if (pikminBehavior != null)
                {
                    if (!pikminBehavior.followingPlayer)
                    {
                        pikminBehavior.target = player;
                        pikminBehavior.task = PikminBehavior.Task.Following;
                        pikminBehavior.followingPlayer = true;
                    }
                    
                }
            }
        }
    }

    private void StopCallPikmin()
    {
        callingPikmin = false;
    }

    private void FindPikmin()
    {
        GameObject[] allPikmin = GameObject.FindGameObjectsWithTag("Pikmin");
        GameObject closestPikmin = null;
        float closestDistance = Mathf.Infinity;

        foreach (GameObject pikmin in allPikmin)
        {
            // Make sure the Pikmin has a PikminBehavior component before accessing it
            PikminBehavior pikminBehavior = pikmin.GetComponent<PikminBehavior>();
            if (pikminBehavior != null && pikminBehavior.followingPlayer)
            {
                float distance = Vector3.Distance(player.transform.position, pikmin.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPikmin = pikmin;
                }
            }
        }

        // After finding the closest Pikmin, pick it up
        if (closestPikmin != null)
        {
            currentPikmin = closestPikmin;
            PickUpPikmin(currentPikmin);
        }
    }

    private void PickUpPikmin(GameObject pikmin)
    {
        // Call PickedUp on the Pikmin behavior to disable its agent and activate its Rigidbody physics
        PikminBehavior pikminBehavior = pikmin.GetComponent<PikminBehavior>();
        if (pikminBehavior != null)
        {
            pikminBehavior.PickedUp();  // Disable NavMeshAgent and enable Rigidbody
        }

        // Position the Pikmin in front of the player
        pikmin.transform.position = player.transform.position + Vector3.up * 1.5f; // Adjust height
        pikmin.transform.parent = player.transform; // Attach to player
    }

    private void ThrowPikmin()
    {
        if (currentPikmin != null && currentCursorCircle != null)
        {
            // Get the Pikmin's behavior script to access its Throw method
            PikminBehavior pikminBehavior = currentPikmin.GetComponent<PikminBehavior>();
            if (pikminBehavior != null)
            {
                // Get the target position (cursor circle position)
                Vector3 targetPosition = currentCursorCircle.transform.position;

                // Calculate the direction and distance to the target position
                Vector3 direction = targetPosition - currentPikmin.transform.position;
                direction.y = 0; // Ignore vertical distance for the horizontal direction calculation

                float horizontalDistance = direction.magnitude; // Calculate the horizontal distance

                // Gravity constant (assuming Earth gravity)
                float gravity = Mathf.Abs(Physics.gravity.y); // Get absolute value for consistency

                // Choose an optimal launch angle (45 degrees is a good default for range)
                float angle = 45f;

                // Calculate the initial velocity needed to reach the target
                float launchVelocity = Mathf.Sqrt(horizontalDistance * gravity / Mathf.Sin(2 * Mathf.Deg2Rad * angle));

                // Decompose the launch velocity into horizontal and vertical components
                float velocityX = launchVelocity * Mathf.Cos(Mathf.Deg2Rad * angle);
                float velocityY = launchVelocity * Mathf.Sin(Mathf.Deg2Rad * angle);

                // Normalize the direction vector
                direction.Normalize();

                // Calculate the final velocity to apply to the Pikmin
                Vector3 velocity = direction * velocityX + Vector3.up * velocityY;

                // Now, let's check if the velocity overshoots. We will slightly reduce the velocity.
                // Reduce the velocity in case of overshoot
                float overshootCorrection = 0.9f; // Reduce the force slightly to prevent overshooting
                velocity *= overshootCorrection;

                // Apply the calculated velocity to the Pikmin's Rigidbody
                Rigidbody rb = currentPikmin.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = false;  // Enable physics
                    rb.velocity = velocity;  // Apply the calculated velocity
                }

                // Detach the Pikmin from the player and reset the reference
                currentPikmin.transform.parent = null;
                currentPikmin = null;
            }
        }
    }
}
