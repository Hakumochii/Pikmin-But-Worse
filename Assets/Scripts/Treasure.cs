using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Treasure : MonoBehaviour
{
    public List<GameObject> spotList = new List<GameObject>();
    public Dictionary<GameObject, bool> spot = new Dictionary<GameObject, bool>();
    
    //references for moving the treasure
    private UnityEngine.AI.NavMeshAgent agent;
    public GameObject playerBase;
    public GameObject treasureBody;
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

        // Populate the Dictionary from the List
        foreach (var spotObject in spotList)
        {
            spot[spotObject] = false;
        }
    }

    private void Update()
    {
        if (AreAllSpotsTaken())
        {
            MoveToBase();
            Debug.Log("Moving to base");
        }
        else
        {
            LayOnGround();
        }

        if (Vector3.Distance(transform.position, basePosition) < 0.1f) 
        {
            TreasureCollectedComplete();
        }
    }

    // Method to check if all Pikmin are helping
    private bool AreAllSpotsTaken()
    {
        foreach (bool isTaken in spot.Values)
        {
            if (!isTaken)
            {
                return false;
            }
        }
        return true; 
    }

    //move treasure towards base if recuired pikmin is reached
    private void MoveToBase()
    {
        agent.isStopped = false;
        agent.updateRotation = false;
        agent.baseOffset = 1f;
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
