using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Treasure : MonoBehaviour
{
    public List<GameObject> spotList = new List<GameObject>();
    public Dictionary<GameObject, bool> spot = new Dictionary<GameObject, bool>();
    public float threshold;
    
    //references for moving the treasure
    private UnityEngine.AI.NavMeshAgent agent;
    public GameObject playerBase;
    public GameObject treasureBody;
    private Vector3 basePosition;
    private bool treasureCollected = false;


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

        if (Vector3.Distance(transform.position, basePosition) < threshold) 
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
        if (!treasureCollected)
        {
            treasureCollected = true;
            StartCoroutine(Collect());
        }
       
    }

    private IEnumerator Collect()
    {
        GameObject[] allPikmin = GameObject.FindGameObjectsWithTag("Pikmin");
        
        foreach (GameObject pikmin in allPikmin)
        {
            PikminBehavior pikminBehavior = pikmin.GetComponent<PikminBehavior>();
            if (pikminBehavior != null && pikminBehavior.task == PikminBehavior.Task.CarryingTreasure && pikminBehavior.currentTreasure == this.gameObject)
            {
                pikminBehavior.StopLiftingTreasure();
                pikminBehavior.task = PikminBehavior.Task.Idle;
            }
        }
        yield return new WaitForSeconds(2f);
        GameManager.Instance.UpdateTreasureCount();
        Destroy(gameObject);
    }
}
