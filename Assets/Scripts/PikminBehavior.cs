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
    private GameObject currentTreasure;

    //variables used in calculations
    private float PikminDistanceAbovePlayer = 1.5f;
    [SerializeField] private Vector3 positionAroundTreasure;

    //enumerators for diffrent taskes of a pikmin
    public enum Task { Idle, FollowingTask, Lifted, Thrown, Fighting, MovingToTreasure, CarryingTreasure, Dying }
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
            case Task.MovingToTreasure:
                CheckTreasureArrival(positionAroundTreasure);
                break;
            case Task.CarryingTreasure:
                LiftingTreasure();
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

    private void GoToTreasure()
    {
        agent.enabled = true;
        rb.isKinematic = true;
        positionAroundTreasure = AddPikminPositionAroundTreasure();
        agent.SetDestination(positionAroundTreasure);
        task = Task.MovingToTreasure; 
    }

    //make positions for pikmin based on pikmin recuired and treasurecollider and add to dictionary
    private Vector3 AddPikminPositionAroundTreasure()
    {
        Treasure treasureScript = currentTreasure.GetComponent<Treasure>();
        GameObject treasureBody = GameObject.FindWithTag("TreasureBody");
        SphereCollider treasureCollider = treasureBody.GetComponent<SphereCollider>();

        float radius = treasureCollider.radius * transform.localScale.x;
        // Calculate angle in radians for each Pikmin position
        float angle = 2 * Mathf.PI * (treasureScript.PikminAcquired % treasureScript.PikminRequired) / treasureScript.PikminRequired;
        // Use cosine and sine to place positions in a circular pattern
        float x = treasureCollider.bounds.center.x + radius * Mathf.Cos(angle);
        float z = treasureCollider.bounds.center.z + radius * Mathf.Sin(angle);
        float y = treasureCollider.bounds.center.y;

        return new Vector3(x, y, z);
    }

    private void CheckTreasureArrival(Vector3 positionAroundTreasure)
    {
        if (transform.position == positionAroundTreasure)
        {
            task = Task.CarryingTreasure; 
        }
    }

    private void LiftingTreasure()
    {
        if (currentTreasure == null) 
        {
            Debug.LogError("currentTreasure is null!");
            return; // Return early to avoid the NullReferenceException
        } 
        Treasure treasure = currentTreasure.GetComponent<Treasure>();
        treasure.PikminAcquired += 1;
        agent.enabled = false;
        rb.isKinematic = false;
        transform.parent = currentTreasure.transform;
    }

    /*private void StopLiftingTreasure()
    {
        Treasure treasure = currentTreasure.GetComponent<Treasure>();
        treasure.PikminAcquired -= 1;
        agent.enabled = true;
        rb.isKinematic = true;
        transform.parent = null;
    }*/

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

        if (collision.gameObject.CompareTag("Treasure"))
        {
            currentTreasure = collision.gameObject;
            GoToTreasure();
        }

    }

   
    private void OnTriggerExit(Collider collision)
    {
         //if the pikmin is told to follow the player but the player gets too far away the pikmin will start going towards the player
        if (collision.gameObject.CompareTag("PlayerRange") && task == Task.FollowingTask)
        {
            followingTask = FollowingTask.GoingTowardsPlayer;
        }

        /*if (collision.gameObject.CompareTag("Treasure"))
        {
            currentTreasure = null;

            if (task == Task.CarryingTreasure)
            {
                StopLiftingTreasure();
            }
        }*/
    } 

}
