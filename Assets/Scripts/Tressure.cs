using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tressure : MonoBehaviour
{
    //Set up reference to pikmin
    [SerializeField] private int pikminRecuired;
    private int pikminAcuired;
    public int PikminAcuired 
    {
        get { return pikminAcuired; } 
        set { pikminAcuired = value; }
    }

    //references for moving the tressure
    private UnityEngine.AI.NavMeshAgent agent;
    private Rigidbody rb;
    private Vector3 BasePosition = new Vector3(8,6,5);

    //variable for collecting tressure completion
    private bool tressureCollected = false;
    public bool TressureCollected
    {
        get { return tressureCollected; }
        set { tressureCollected = value; }
    }

    private void Start()
    {
        //get variables for movemnet of tressure
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
        //set variables
        agent.enabled = false;
        rb.isKinematic = true;
    }


    private void Update()
    {
        //check for different sattes to deterine the behavior of the tressure
        if (pikminAcuired == pikminRecuired)
        {
            MoveToBase();
        }
        else
        {
            LayOnGround();
        }
        if (transform.position == BasePosition)
        {
            TressureCollectedComplete();
        }
    }

    //move tressure towards base if recuired pikmin is reached
    private void MoveToBase()
    {
        agent.enabled = true;
        rb.isKinematic = false;
        agent.SetDestination(BasePosition);
    }

    //lay tressure on the ground if not all pikmin is reached
    private void LayOnGround()
    {
        agent.enabled = false; 
        rb.isKinematic = true;
    }

    //method for tressure collection when it reaches the base position
    private void TressureCollectedComplete()
    {
        LayOnGround();
        tressureCollected = true;
    }
}
