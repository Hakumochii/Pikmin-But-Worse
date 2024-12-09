using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Treasure : MonoBehaviour
{
    //list for spots around treasure
    public List<GameObject> spotList = new List<GameObject>();
    public Dictionary<GameObject, bool> spot = new Dictionary<GameObject, bool>();
    //for how close the treasure should be to the base
    public float threshold;
    
    //references for moving the treasure
    private UnityEngine.AI.NavMeshAgent agent;
    public GameObject playerBase;
    public GameObject treasureBody;
    private Vector3 basePosition;
    private bool treasureCollected = false;

    //get the agent, base and fill the spot dictionary (list was to be able to use the inspector)
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

    //checks if the treasure has the requirenets to move else its juat lays static
    //abd checks if the treasure has reached the base
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

    //move treasure towards base if all spots are taken
    private void MoveToBase()
    {
        agent.isStopped = false;
        agent.updateRotation = false;
        agent.SetDestination(basePosition);
    }

    //lay treasure on the ground if not all spots are taken
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

    //findas all the pikmina and set then to no longer carry the reasure and than set them to idle 
    //then the treasure count will go up and the treasure wil destroy itself
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
