using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System;

public class PikminBehavior : MonoBehaviour
{
    //variables needed for moving pikmin around
    public NavMeshAgent agent;
    public Rigidbody rb;
    private GameObject player;
    public GameObject currentTreasure;
    [SerializeField] private Vector3 positionAroundTreasure;

    //variables used in calculations
    private float pikminDistanceAbovePlayer = 1.5f;
    private float spotThreshold = 1;

    //enumerators for diffrent taskes of a pikmin
    public enum Task { Idle, FollowingTask, Lifted, Thrown, MovingToTreasure, CarryingTreasure, Dying }
    public Task task = Task.Idle;

    public enum FollowingTask { GoingTowardsPlayer, Waiting }
    public FollowingTask followingTask;
    private GameObject currentTreasureSpot;
   
    public enum PikminType { Blue, Red, Yellow};
    public PikminType pikminType;

    //some different states for small fixes like avoiding repeating code and an event for keeping gamemanager updated on pikmin following
    public static event Action<int> OnPikminFollowStateChanged; 
    private bool _isFollowingPlayer = false; 
    private bool isDying = false;


    private void Start()
    {
        // Initialize the NavMeshAgent and Rigidbody components and get player 
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
        player = GameObject.FindGameObjectWithTag("Player");

        //initailly disable physics
        rb.isKinematic = true;
    }

    private void Update()
    {
        //checks if task == Task.FollowingTask is true or not if its true a pikmin wil be added to the following variable in game manager
        bool wasFollowing = _isFollowingPlayer;
        _isFollowingPlayer = (task == Task.FollowingTask);

        if (_isFollowingPlayer != wasFollowing)
        {
            // Notify state change (+1 if following, -1 if other)
            OnPikminFollowStateChanged?.Invoke(_isFollowingPlayer ? 1 : -1);
        }

        //switching tasks based on differnet factors
        switch (task)
        {
            case Task.Idle:
                //no task
                break;
            case Task.FollowingTask:
                FollowPlayer();
                break;
            case Task.Lifted:
                PickedUp();
                break;
            case Task.Thrown:
                InAir();
                break;
            case Task.MovingToTreasure:
                CheckTreasureArrival(positionAroundTreasure);
                break;
            case Task.CarryingTreasure:
                LiftingTreasure();
                break;
            case Task.Dying:
                //no task while waiting to die
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
    public void Wait()
    {
        agent.enabled = true;
        rb.isKinematic = true;
        agent.isStopped = true;
        agent.ResetPath();
    }

    //stops the agent and moves the pikmin to ontop of the player
    private void PickedUp()
    {
        agent.enabled = false;
        transform.position = player.transform.position + Vector3.up * pikminDistanceAbovePlayer;
        transform.parent = player.transform; 
    }

    //turns on the physics and removes the pikmin form the player so that it can be thown
    private void InAir()
    {
        rb.isKinematic = false;
        transform.parent = null;
    }

    //sets the pikmin to move to a spot around the current treasue if there is one
    private void GoToTreasure()
    {
        agent.enabled = true;
        rb.isKinematic = true;
        positionAroundTreasure = GetPositionAroundTreasure();
        if (positionAroundTreasure != Vector3.zero)  // Check for the sentinel value
        {
            agent.SetDestination(positionAroundTreasure);
            task = Task.MovingToTreasure;
        }
        else
        {
            Debug.Log("No valid treasure position found.");
            task = Task.Idle;  // Stop if no valid position found
        }
    }

    //get a dictionary of positions and values of taken or not taken and check id there is one that is not taken 
    //if there isnt retun zero which indicates nothing shoild happen
    private Vector3 GetPositionAroundTreasure()
    {
        Treasure treasure = currentTreasure.GetComponent<Treasure>();
        foreach (KeyValuePair<GameObject, bool> entry in treasure.spot)
        {
            currentTreasureSpot = entry.Key;
            bool isTaken = entry.Value;   

            if (!isTaken)
            {
                isTaken = true;
                return currentTreasureSpot.transform.position; 
            }
        }
        return Vector3.zero; 
        
    }

    //check is pikmin has reached the sprt around the treasure and if the positions was zero do nothing
    private void CheckTreasureArrival(Vector3 positionAroundTreasure)
    {
        if (positionAroundTreasure == Vector3.zero)
        {
            Debug.Log("No valid treasure position found.");
            return;
        }

        if (Vector3.Distance(transform.position, positionAroundTreasure) < spotThreshold)
        {
            Debug.Log("Pikmin has reached the treasure position now carying treasure.");
            task = Task.CarryingTreasure; 
        }
    }

    //while lifing the treasure the spot is now taken, pikmin is child to treasure 
    //and the agnet is turned of so the pikmin is technically carried by the treasure
    private void LiftingTreasure()
    {
        Treasure treasure = currentTreasure.GetComponent<Treasure>();
        treasure.spot[currentTreasureSpot] = true;
        agent.enabled = false;
        transform.parent = currentTreasure.transform;
    }

    //if the pikmin is set to stop lifting the spot wil no longer be taken, and everythingabout the treasure that the pikmin kept will be null
    public void StopLiftingTreasure()
    {
        Treasure treasure = currentTreasure.GetComponent<Treasure>();
        treasure.spot[currentTreasureSpot] = false;
        agent.enabled = true;
        transform.parent = null;
        currentTreasureSpot = null;
        currentTreasure = null;
    }

    //if pikmin is indanger start a corutine that will kill it and it its carrying a tresure it will stop
    private void InDanger()
    {
        if (!isDying)
        {
            if (task == Task.CarryingTreasure)
            {
                StopLiftingTreasure();
            }
            isDying = true;
            StartCoroutine(DeathTimer());
        }
        
    }

    //a coroutine that will kill the pikmin after 0.5 seconds if it is in danger 
    private IEnumerator DeathTimer()
    {
        yield return new WaitForSeconds(0.5f);
        task = Task.Dying;
        agent.isStopped = true;
        yield return new WaitForSeconds(3f);
        GameManager.Instance.DecreasePikminCount();
        Destroy(gameObject);
    }

    //hnadels different interactions with other objects fx for followinf player og carrying treasure or touching water
    private void OnTriggerEnter(Collider collision)
    {
        //if in contact with player and it is not being lifted the pikmin is set to follow the player 
        //but its state is set to wait because the pikmin would be too close to the player
        if (collision.gameObject.CompareTag("Player") && task != Task.Lifted && task != Task.CarryingTreasure && task != Task.Dying)
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
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground") && task == Task.Thrown)
        {
            task = Task.Idle;
        }

        if (collision.gameObject.CompareTag("Treasure") && task != Task.FollowingTask)
        {
            currentTreasure = collision.gameObject;
            GoToTreasure();
        }

        if (collision.gameObject.layer == LayerMask.NameToLayer("Water") && pikminType != PikminType.Blue)
        {
            InDanger();
        }
    }

    //part of the logic that keeps the pikmin forn bumping into the player but still follow them
    private void OnTriggerExit(Collider collision)
    {
         //if the pikmin is told to follow the player but the player gets too far away the pikmin will start going towards the player
        if (collision.gameObject.CompareTag("PlayerRange") && task == Task.FollowingTask)
        {
            followingTask = FollowingTask.GoingTowardsPlayer;
        }
    } 

}
