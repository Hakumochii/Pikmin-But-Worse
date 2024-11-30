using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PikminBehavior : MonoBehaviour
{
    //variables needed for moving pikmin around
    private NavMeshAgent agent;
    private Rigidbody rb;
    private GameObject player;
    private GameObject currentTressure;

    //variables used in calculations
    private float PikminDistanceAbovePlayer = 1.5f;
    [SerializeField] private float ThresholdFromTressure = 0.5f;

    //enumerators for diffrent taskes of a pikmin
    public enum Task { Idle, FollowingTask, Lifted, Thrown, Fighting, CarryingTressure, Dying }
    public Task task = Task.Idle;

    public enum FollowingTask { GoingTowardsPlayer, Waiting }
    public FollowingTask followingTask;


    private void Start()
    {
        // Initialize the NavMeshAgent and Rigidbody components
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
        player = GameObject.FindGameObjectWithTag("Player");

        //initailly disable physics
        rb.isKinematic = true;
    }

    private void Update()
    {
        //switching tasks based on differnet factors
        switch (task)
        {
            case Task.Idle:
                // Idle behavior, such as wandering or resting
                break;
            case Task.FollowingTask:
                //swithes to another switchstatement for more multible tasks  
                FollowPlayer();
                break;
            case Task.Lifted:
                PickedUp();
                break;
            case Task.Thrown:
                InAir();
                break;
            case Task.Fighting:
                break;
            case Task.CarryingTressure:
                LiftingTressure();
                break;
            case Task.Dying:
                break;
        }
    }

    private void FollowPlayer()
    {
        //switch stamement for controlling the behavior of a pikmin while following the player
        switch (followingTask)
        {
            case FollowingTask.Waiting:
                Wait();
                break;
            case FollowingTask.GoingTowardsPlayer:
                GoTowardsPlayer();
                break;
        }
    }

    //sets the agent to follow teh player
    private void GoTowardsPlayer()
    {
        agent.enabled = true;
        rb.isKinematic = true;
        agent.isStopped = false;
        agent.SetDestination(player.transform.position);
    }

    //pikmin stands still if they get too close to the player while following them
    private void Wait()
    {
        agent.enabled = true;
        rb.isKinematic = true;
        agent.isStopped = true;
        agent.ResetPath();
    }

    //stops the agent and moves teh pikmin to ontop of the player
    private void PickedUp()
    {
        agent.enabled = false;
        transform.position = player.transform.position + Vector3.up * PikminDistanceAbovePlayer;
        transform.parent = player.transform; 
    }

    //turns on the physics and removes the pikmin form the player so that it can be thown
    private void InAir()
    {
        rb.isKinematic = false;
        transform.parent = null;
    }

    private void GoToTressure()
    {
        // Get the collider bounds of the treasure
        Collider treasureCollider = currentTressure.GetComponent<Collider>();
        Bounds treasureBounds = treasureCollider.bounds;

        // Define a random position around the treasure's collider
        Vector3 randomOffset = new Vector3(
            Random.Range(treasureBounds.min.x, treasureBounds.max.x), // Random X position within bounds
            treasureBounds.center.y, // Keep the Y position at the center (or adjust for height)
            Random.Range(treasureBounds.min.z, treasureBounds.max.z)  // Random Z position within bounds
        );

        // Move the Pikmin towards the random position near the treasure
        Vector3 direction = (randomOffset - transform.position).normalized; // Find the direction vector
        float moveSpeed = 3f; // Adjust this to your preferred speed

        // Apply the movement (using Rigidbody to move the Pikmin)
        rb.isKinematic = false;
        rb.velocity = direction * moveSpeed; // Move Pikmin toward the random position

        // Look at the treasure while moving (optional)
        transform.LookAt(currentTressure.transform.position);

        // When the Pikmin reaches the target (within a threshold distance)
        if (Vector3.Distance(transform.position, randomOffset) < ThresholdFromTressure)
        {
            task = Task.CarryingTressure;
            rb.velocity = Vector3.zero; // Stop moving when the Pikmin reaches the destination
        }
    }


    private void LiftingTressure()
    {
        Tressure tressure = currentTressure.GetComponent<Tressure>();
        tressure.PikminAcuired += 1;
        agent.enabled = false;
        rb.isKinematic = false;
        transform.parent = currentTressure.transform;
    }

    private void StopLiftingTressure()
    {
        Tressure tressure = currentTressure.GetComponent<Tressure>();
        tressure.PikminAcuired -= 1;
        agent.enabled = true;
        rb.isKinematic = true;
        transform.parent = null;
    }

    private void OnTriggerEnter(Collider collision)
    {
        //if in contact with player and it is not being lifted the pikmin is set to follow the player 
        //but its state is set to wait because the pikmin would be too close to the player
        if (collision.gameObject.CompareTag("Player") && task != Task.Lifted)
        {
            task = Task.FollowingTask;  
            followingTask = FollowingTask.Waiting;        
        }

        //if the pikmin is too close to the player it is told to wait
        if (collision.gameObject.CompareTag("PlayerRange") && task == Task.FollowingTask)
        {
            followingTask = FollowingTask.Waiting;
        }  

        //if the pikmin i touching the ground and it is not followinf the player it will just be in idle 
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground") && task != Task.FollowingTask)
        {
            task = Task.Idle;
        }

        if (collision.gameObject.CompareTag("Tressure") && task != Task.FollowingTask)
        {
            currentTressure = collision.gameObject;
            GoToTressure();
        }

    }

   
    private void OnTriggerExit(Collider collision)
    {
         //if the pikmin is told to follow the player but the player gets too far away the pikmin will start going towards the player
        if (collision.gameObject.CompareTag("PlayerRange") && task == Task.FollowingTask)
        {
            followingTask = FollowingTask.GoingTowardsPlayer;
        }

        if (collision.gameObject.CompareTag("Tressure"))
        {
            currentTressure = null;

            if (task == Task.CarryingTressure)
            {
                StopLiftingTressure();
            }
        }
    } 

}
