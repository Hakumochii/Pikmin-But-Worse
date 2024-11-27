using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PikminBehavior : MonoBehaviour
{
    private NavMeshAgent agent;
    private Rigidbody rb;
    public GameObject player;

    // Define enum for task and create a variable to store the current task
    public enum Task { Idle, Following, Stop, PickedUp, Working, Dying }
    public Task task = Task.Idle;

    // Target GameObject to follow or interact with
    public GameObject target;
    public bool followingPlayer = false;

    private void Start()
    {
        // Initialize the NavMeshAgent and Rigidbody components
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();

        // Increase avoidance priority and adjust the avoidance type
        agent.avoidancePriority = 50; // Adjust priority, higher values move out of the way more
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
        agent.radius = 0.3f; // Adjust based on your Pikmin's size

        // Initially disable Rigidbody physics
        rb.isKinematic = true; // Disable physics until picked up
    }

    private void Update()
    {
        // Continuously check the task and update behavior
        UpdateTask();
    }

    // Update the task behavior based on the current task state
    private void UpdateTask()
    {
        switch (task)
        {
            case Task.Idle:
                // Idle behavior, such as wandering or resting
                break;
            case Task.Following:
                Follow(target);
                break;
            case Task.Stop:
                StandStill();
                break;
            case Task.PickedUp:
                break;
            case Task.Working:
                // Working behavior, such as gathering resources or repairing structures
                break;
            case Task.Dying:
                // Dying behavior, such as fading out or disappearing
                break;
        }
    }

    // Set the agent's destination to the target's position with offset
    private void Follow(GameObject target)
    {
        agent.enabled = true;
        rb.isKinematic = true;
        agent.isStopped = false;
        agent.SetDestination(target.transform.position);
    }

    private void StandStill()
    {
        agent.isStopped = true;
    }

    // On collision with the player, switch to the "Follow" task
    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("collided with player");
            if (task != Task.PickedUp)
            {
                target = player;
                followingPlayer = true;
                Debug.Log("now following player");
            }
            
        }

        if (collision.gameObject.CompareTag("PlayerRange"))
        {
            Debug. Log("collided with range");
            Debug. Log(task + " followingPlayer: " + followingPlayer );
            if (task != Task.PickedUp && followingPlayer)
            {
                Debug.Log("Now stopping because to close");
                task = Task.Stop;
            }  
        }

        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            if (!followingPlayer)
            {
                task = Task.Idle;
            }
        }

    }

    private void OnTriggerExit(Collider collision)
    {
        if (collision.gameObject.CompareTag("PlayerRange"))
        {
            if (followingPlayer)
            {
                task = Task.Following;
            }
        }
    }

    public void PickedUp()
    {
        // Change task to indicate it's picked up
        task = Task.PickedUp;
        agent.enabled = false;

        // Optionally disable any other behaviors while being carried
        followingPlayer = false;
    }

}
