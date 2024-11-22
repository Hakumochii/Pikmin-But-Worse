using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PikminBehavior : MonoBehaviour
{
    public NavMeshAgent agent;
    public GameObject player;

    // Define enum for task and create a variable to store the current task
    public enum Task { Idle, Follow, Working, Dying }
    public Task task = Task.Idle;

    // Target GameObject to follow or interact with
    private GameObject target;
    public Vector3 followOffset = new Vector3(-1, 0, -1); // Example offset to keep some distance

    private void Start()
    {
        // Initialize the NavMeshAgent component
        agent = GetComponent<NavMeshAgent>();

        // Increase avoidance priority and adjust the avoidance type
        agent.avoidancePriority = 50; // Adjust priority, higher values move out of the way more
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
        agent.radius = 0.3f; // Adjust based on your Pikmin's size
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
            case Task.Follow:
                if (target != null)
                {
                    UpdateTarget(target);
                }
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
    private void UpdateTarget(GameObject target)
    {
        if (agent != null && target != null)
        {
            // Apply the offset to the target's position
            agent.SetDestination(target.transform.position + followOffset);
        }
    }

    // On collision with the player, switch to the "Follow" task
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            task = Task.Follow;
            target = player;
        }
    }


}
