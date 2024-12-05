using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Treasure : MonoBehaviour
{
    //Set up reference to pikmin and tressure collider
    public Dictionary<GameObject, bool> pikmin = new Dictionary<GameObject, bool>

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
        if (AreAllPikminHelping())
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
    private bool AreAllPikminHelping()
    {
        foreach (bool isHelping in pikmin.Values)
        {
            if (!isHelping)
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
