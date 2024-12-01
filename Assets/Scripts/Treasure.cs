using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Treasure : MonoBehaviour
{
    //Set up reference to pikmin and tressure collider
    [SerializeField] private int pikminRequired;
    public int PikminRequired
    {
        get { return pikminRequired; } 
        set { pikminRequired = value; }
    }
    private int pikminAcquired;
    public int PikminAcquired
    {
        get { return pikminAcquired; } 
        set { pikminAcquired = value; }
    }

    //references for moving the treasure
    private UnityEngine.AI.NavMeshAgent agent;
    public GameObject playerBase;
    private Vector3 basePosition;

    //variable for collecting treasure completion
    private bool treasureCollected = false;
    public bool TreasureCollected
    {
        get { return treasureCollected; }
        set { treasureCollected = value; }
    }

    private void Start()
    {
        //get variables for movemnet of treasure
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        basePosition = playerBase.transform.position;
    }

    private void Update()
    {
        //check for different sattes to deterine the behavior of the treasure
        if (pikminAcquired == pikminRequired)
        {
            MoveToBase();
        }
        else
        {
            LayOnGround();
        }

        if (transform.position == basePosition)
        {
            TreasureCollectedComplete();
        }
    }

    //move treasure towards base if recuired pikmin is reached
    private void MoveToBase()
    {
        agent.isStopped = false;
        agent.baseOffset = 3f;
        agent.SetDestination(basePosition);
    }

    //lay treasure on the ground if not all pikmin is reached
    private void LayOnGround()
    {
        agent.isStopped = true;
    }

    //method for treasure collection when it reaches the base position
    private void TreasureCollectedComplete()
    {
        LayOnGround();
        treasureCollected = true;
    }
}
