using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    //player input
    private PlayerInput playerInput;
    private InputAction positionAction;
    private InputAction leftClickAction;
    private InputAction rightClickAction;
    private InputAction QAction;
    private InputAction EAction;

    //reference to cursors
    [SerializeField] private GameObject cursorCircleSmallPrefab;
    [SerializeField] private GameObject cursorCircleBigPrefab;
    private GameObject cursorCircleSmall;
    private GameObject cursorCircleBig;

    //reference to player and ground, treasure and pikmin layers
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask treasureLayer;
    [SerializeField] private LayerMask pikminLayer;
    public GameObject player;
    [HideInInspector] GameObject currentPikmin;

    //vaiables used for states or calculations
    private bool callingPikmin = false; 
    private Quaternion cursorRotation = Quaternion.Euler(90f, 0f, 0f);
    private float cursorOffsetFromGround = 0.5f;

    //corutines
    private Coroutine callPikminCoroutine;

    //singleton pattern
    private static PlayerInteraction instance;
    public static PlayerInteraction Instance
    {
        // Ensure there is always an instance of the sound manager
        get
        {
            // Check if the instance is null or has been destroyed
            if (instance == null || instance.gameObject == null)
            {
                // Find an existing instance in the scene
                instance = FindObjectOfType<PlayerInteraction>();

                // If no instance exists, create a new one
                if (instance == null)
                {
                    GameObject obj = new GameObject(nameof(PlayerInteraction));
                    instance = obj.AddComponent<PlayerInteraction>();
                }
            }
            return instance;
        }
    }

    private void Awake()
    {
        // Ensure the instance isn't destroyed when loading new scenes
        if (instance == null || instance.gameObject == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            // If another instance exists, destroy this one
            Destroy(gameObject);
            return;
        }

        //finds all the actions in the action map
        playerInput = GetComponent<PlayerInput>();
        var actionMap = playerInput.actions.FindActionMap("Buttons");
        positionAction = actionMap.FindAction("Position");
        leftClickAction = actionMap.FindAction("LeftClick");
        rightClickAction = actionMap.FindAction("RightClick");
        QAction = actionMap.FindAction("Q");
        EAction = actionMap.FindAction("E");
    }

    //subscribe to events so that the game is always listeninf for player input
    private void OnEnable()
    {
        positionAction.Enable();
        leftClickAction.Enable();
        rightClickAction.Enable();
        QAction.Enable();
        EAction.Enable();

        positionAction.performed += OnMouseMove;
        leftClickAction.performed += OnLeftClick;
        leftClickAction.canceled += OnLeftClickRelease;
        rightClickAction.performed += OnTestClick;
        rightClickAction.canceled += OnRightClickRelease;
        QAction.performed += OnQ;
        EAction.performed += OnE;
    }

    //unsubscribe for events if the gameobject were to get destrypted of inactive 
    //so it doent lisent to something it cant do anything about
    private void OnDisable()
    {
        positionAction.performed -= OnMouseMove;
        leftClickAction.performed -= OnLeftClick;
        leftClickAction.canceled -= OnLeftClickRelease;
        rightClickAction.performed -= OnTestClick;
        rightClickAction.canceled -= OnRightClickRelease;
        QAction.performed -= OnQ;
        EAction.performed -= OnE;

        positionAction.Disable();
        leftClickAction.Disable();
        rightClickAction.Disable();
        QAction.Disable();
        EAction.Disable();
    }

    //gets the input form the mouse and uses if for a raycasy that handles a cursor
    private void OnMouseMove(InputAction.CallbackContext context)
    {
        Vector2 mousePosition = context.ReadValue<Vector2>();
        HitGroundWithMouse(mousePosition);
    }

    //calls teh method to find pikmin so that the player can thow it
    private void OnLeftClick(InputAction.CallbackContext context)
    {
        FindPikmin();
    }

    //throws the pikmin if they forund one
    private void OnLeftClickRelease(InputAction.CallbackContext context)
    {
        if (currentPikmin != null)
        {
            ThrowPikmin();
        }
    }

    //Right click makes calling pikmin is set to true and triggers courutine 
    //thet triggers a ray that sends a call for the pikmin hit to get them to follow the player 
    private void OnTestClick(InputAction.CallbackContext context)
    {
        if (callPikminCoroutine == null)
        {
            callingPikmin = true;
            callPikminCoroutine = StartCoroutine(CallPikminContinuously());
        }
    }

    //when no longer clicking right button calling for pikmin is stopped
    private void OnRightClickRelease(InputAction.CallbackContext context)
    {
        callingPikmin = false;
        if (callPikminCoroutine != null)
        {
            StopCoroutine(callPikminCoroutine);
            callPikminCoroutine = null;
        }
    }

    //stop pikmin form following the player
    private void OnQ(InputAction.CallbackContext context)
    {
        DismissPikmin();
    }

    //changes the pikmin while holing then to throw the one you want
    private void OnE(InputAction.CallbackContext context)
    {
        ChangePikmin();
    }

    //Shoots a constant ray form the camera and mouse position and calls ProjectCursor when the ground is hit
    private void HitGroundWithMouse(Vector2 mousePosition)
    {
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);

        int combinedLayerMask = groundLayer | treasureLayer;

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, combinedLayerMask))  // Raycast against both layers
        {
            ProjectCursor(hit);
        }
    }

    //projects a cursor cirkle on the ground and handels werther or not i should be big or small
    private GameObject currentCursorCircleBig; private void ProjectCursor(RaycastHit hit)
    {
        GameObject circle;

        if (callingPikmin)
        {
            // If switching to the big cursor, destroy the small one if it exists
            if (cursorCircleSmall != null)
            {
                Destroy(cursorCircleSmall);
                cursorCircleSmall = null;
            }

            if (cursorCircleBig == null)
            {
                cursorCircleBig = Instantiate(cursorCircleBigPrefab, hit.point, Quaternion.identity);
                cursorCircleBig.transform.rotation = cursorRotation;
            }
            circle = cursorCircleBig;
        }
        else
        {
            // If switching to the small cursor, destroy the big one if it exists
            if (cursorCircleBig != null)
            {
                Destroy(cursorCircleBig);
                cursorCircleBig = null;
            }

            if (cursorCircleSmall == null)
            {
                cursorCircleSmall = Instantiate(cursorCircleSmallPrefab, hit.point, Quaternion.identity);
                cursorCircleSmall.transform.rotation = cursorRotation;
            }
            circle = cursorCircleSmall;
        }
        // Update position
        circle.transform.position = new Vector3(hit.point.x, hit.point.y + cursorOffsetFromGround, hit.point.z);
    }

    //makes sure the call for pikmin keeps going while calling pikmin 
    //and also get the mouse input for where the player is looking for pikmin
    private IEnumerator CallPikminContinuously()
    {
        while (true)
        {
            if (!callingPikmin) break; 
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            CallPikmin(mousePosition);
            yield return null; 
        }
    }

    //A big ray is cast (maches the cursor) to call for pikmin 
    //if pikmin are found thet are tould to follow the player
    private void CallPikmin(Vector2 mousePosition)
    {
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        RaycastHit hit;
        float rayRadius = 3f;

        if (Physics.SphereCast(ray, rayRadius, out hit, Mathf.Infinity, pikminLayer))
        {
            if (hit.collider != null && hit.collider.CompareTag("Pikmin"))
            {
                GameObject pikmin = hit.collider.gameObject;
                PikminBehavior pikminBehavior = pikmin.GetComponent<PikminBehavior>();
                if (pikminBehavior != null)
                {
                    if (pikminBehavior.task != PikminBehavior.Task.FollowingTask && pikminBehavior.task != PikminBehavior.Task.Thrown)
                    {
                        if (pikminBehavior.task == PikminBehavior.Task.CarryingTreasure)
                        {
                            pikminBehavior.StopLiftingTreasure();
                        }
                        pikminBehavior.task = PikminBehavior.Task.FollowingTask;
                        pikminBehavior.followingTask = PikminBehavior.FollowingTask.GoingTowardsPlayer; 
                    }
                    
                }
            }
        }
    }

    //it finds all the pikmin in the scene and checks if they are following the player
    //if they are their distance is calculated and the one that is the closest is set to be picked up
    private void FindPikmin()
    {
        GameObject[] allPikmin = GameObject.FindGameObjectsWithTag("Pikmin");
        GameObject closestPikmin = null;
        float closestDistance = Mathf.Infinity;
        
        foreach (GameObject pikmin in allPikmin)
        {
            PikminBehavior pikminBehavior = pikmin.GetComponent<PikminBehavior>();
            if (pikminBehavior != null && pikminBehavior.task == PikminBehavior.Task.FollowingTask)
            {
                float distance = Vector3.Distance(player.transform.position, pikmin.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPikmin = pikmin;
                }
            }
        }

        currentPikmin = closestPikmin;
        PickUpPikmin(currentPikmin);

    }

    //sets the task of the closest pikmin to lifted and swhich positions with the pikmin on top of the player
    private void PickUpPikmin(GameObject pikmin)
    {
        PikminBehavior pikminBehavior = pikmin.GetComponent<PikminBehavior>();
        if (pikminBehavior != null)
        {
            pikminBehavior.task = PikminBehavior.Task.Lifted;
        }
    }

    private void ThrowPikmin()
    {
        // Get the Pikmin's behavior script to access its Throw method
        PikminBehavior pikminBehavior = currentPikmin.GetComponent<PikminBehavior>();
        if (pikminBehavior != null)
        {
            // Set the Pikmin's task to thrown (turn on physics)
            pikminBehavior.task = PikminBehavior.Task.Thrown;

            // Get the target position (cursor circle position)
            Vector3 targetPosition = cursorCircleSmall.transform.position;

            // Calculate the direction and distance to the target position
            Vector3 direction = targetPosition - currentPikmin.transform.position;
            direction.y = 0; // Ignore vertical distance for the horizontal direction calculation

            float horizontalDistance = direction.magnitude; // Calculate the horizontal distance

            // Gravity constant (assuming Earth gravity)
            float gravity = Mathf.Abs(Physics.gravity.y); // Get absolute value for consistency

            // Choose an optimal launch angle (45 degrees is a good default for range)
            float angle = 45f;

            // Declare throwHeightFactor outside of the if-else block
            float throwHeightFactor = 1f; // Default value for Pikmin types other than Yellow

            // Modify the throw height based on Pikmin type
            if (pikminBehavior.pikminType == PikminBehavior.PikminType.Yellow)
            {
                throwHeightFactor = 1.5f; // Adjust the factor for Yellow Pikmin
            }

            // Calculate the initial velocity needed to reach the target
            float launchVelocity = Mathf.Sqrt(horizontalDistance * gravity / Mathf.Sin(2 * Mathf.Deg2Rad * angle));

            // Decompose the launch velocity into horizontal and vertical components
            float velocityX = launchVelocity * Mathf.Cos(Mathf.Deg2Rad * angle);
            float velocityY = (launchVelocity * Mathf.Sin(Mathf.Deg2Rad * angle)) * throwHeightFactor; // Apply the height factor here

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
                rb.isKinematic = false;
                rb.velocity = velocity;  // Apply the calculated velocity
            }

            // Detach the Pikmin from the player and reset the reference
            currentPikmin = null;
        }
    }

    //sets the following pikmin to idle
    private void DismissPikmin()
    {
        GameObject[] allPikmin = GameObject.FindGameObjectsWithTag("Pikmin");
        
        foreach (GameObject pikmin in allPikmin)
        {
            PikminBehavior pikminBehavior = pikmin.GetComponent<PikminBehavior>();
            if (pikminBehavior != null && pikminBehavior.task == PikminBehavior.Task.FollowingTask)
            {
                pikminBehavior.task = PikminBehavior.Task.Idle;
            }
        }
    }

    //gets the pikmin following and puts their type in a list so that it can circle through the list and find the next type of pikmin so that you get a differt type
    private void ChangePikmin()
    {
        // Get all Pikmin in the scene
        GameObject[] allPikmin = GameObject.FindGameObjectsWithTag("Pikmin");
        List<GameObject> followingPikmin = new List<GameObject>();

        PikminBehavior currentPikminBehavior = currentPikmin.GetComponent<PikminBehavior>();
        
        // Add all following Pikmin to the list
        foreach (GameObject pikmin in allPikmin)
        {
            PikminBehavior pikminBehavior = pikmin.GetComponent<PikminBehavior>();
            if (pikminBehavior != null && pikminBehavior.task == PikminBehavior.Task.FollowingTask)
            {
                followingPikmin.Add(pikmin);
            }
        }


        // Create a list of available Pikmin types that are following the player
        List<PikminBehavior.PikminType> availableTypes = new List<PikminBehavior.PikminType>();
        
        foreach (GameObject pikmin in followingPikmin)
        {
            PikminBehavior pikminBehavior = pikmin.GetComponent<PikminBehavior>();
            if (!availableTypes.Contains(pikminBehavior.pikminType))
            {
                availableTypes.Add(pikminBehavior.pikminType);
            }
        }

        // If no following Pikmin are available, do nothing
        if (availableTypes.Count == 0)
        {
            return;
        }

        if (availableTypes.Count > 1)
        {
            // List of all Pikmin types
            PikminBehavior.PikminType[] pikminTypes = (PikminBehavior.PikminType[])System.Enum.GetValues(typeof(PikminBehavior.PikminType));
                
            // Find the next type in the list
            PikminBehavior.PikminType currentType = currentPikminBehavior.pikminType;
            int nextTypeIndex = (System.Array.IndexOf(pikminTypes, currentType) + 1) % pikminTypes.Length;
            PikminBehavior.PikminType nextType = pikminTypes[nextTypeIndex];

            // Search for the next Pikmin of the next type
            foreach (GameObject pikmin in followingPikmin)
            {
                PikminBehavior pikminBehavior = pikmin.GetComponent<PikminBehavior>();
                
                // If the Pikmin's type matches the next type, select it
                if (pikminBehavior != null && pikminBehavior.pikminType == nextType)
                {
                    // Detach the current Pikmin from the player
                    currentPikminBehavior.transform.parent = null;

                    // Move the current Pikmin to the new Pikmin's position
                    currentPikmin.transform.position = pikmin.transform.position;

                    // Set the task to FollowingTask
                    currentPikminBehavior.task = PikminBehavior.Task.FollowingTask;

                    // Pick up the new Pikmin
                    currentPikmin = pikmin;
                    PickUpPikmin(currentPikmin);

                    break; // Exit the loop once we find the next Pikmin
                }
            }
        }
        else
        {
            // Find the next Pikmin type in the list
            PikminBehavior.PikminType currentType = currentPikminBehavior.pikminType;
            int nextTypeIndex = (availableTypes.IndexOf(currentType) + 1) % availableTypes.Count;
            PikminBehavior.PikminType nextType = availableTypes[nextTypeIndex];

            // Search for the next Pikmin of the next type
            foreach (GameObject pikmin in followingPikmin)
            {
                PikminBehavior pikminBehavior = pikmin.GetComponent<PikminBehavior>();

                // If the Pikmin's type matches the next type, select it
                if (pikminBehavior != null && pikminBehavior.pikminType == nextType)
                {
                    // Detach the current Pikmin from the player
                    currentPikminBehavior.transform.parent = null;

                    // Move the current Pikmin to the new Pikmin's position
                    currentPikmin.transform.position = pikmin.transform.position;

                    // Set the task to FollowingTask
                    currentPikminBehavior.task = PikminBehavior.Task.FollowingTask;

                    // Pick up the new Pikmin
                    currentPikmin = pikmin;
                    PickUpPikmin(currentPikmin);

                    break; // Exit the loop once we find the next Pikmin
                }
            }
        }
    }


    
}