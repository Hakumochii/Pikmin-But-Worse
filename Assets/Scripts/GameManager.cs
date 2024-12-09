using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    //variables for UI and completion conditions
    public int pikminFollowing;
    public int pikminOverall = 3;
    public int treasureCount;
    private int treasuresInTotal = 3;

    //UI references
    public TextMeshProUGUI pikminCountText;
    public TextMeshProUGUI treasureCountText;
    public TextMeshProUGUI pikminAllText;
    public GameObject redPicture;
    public GameObject bluePicture;
    public GameObject yellowPicture;
    public GameObject player;


    // Singleton pattern because there should only be one and many scripts acess it
    private static GameManager instance;
    public static GameManager Instance
    {
        // Ensure there is always an instance of the sound manager
        get
        {
            // Check if the instance is null or has been destroyed
            if (instance == null || instance.gameObject == null)
            {
                // Find an existing instance in the scene
                instance = FindObjectOfType<GameManager>();

                // If no instance exists, create a new one
                if (instance == null)
                {
                    GameObject obj = new GameObject(nameof(GameManager));
                    instance = obj.AddComponent<GameManager>();
                }
            }
            return instance;
        }
    }

    
    private void Awake()
    {
        // Ensure the instance isn't destroyed when loading new scenes
        if (instance == null || instance.gameObject == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            // If another instance exists, destroy this one
            Destroy(gameObject);
            return;
        }
    }

    //turn off unneeded UI, set text and subscribe to pikmin following event
    void Start()
    {
        TurnOffAllPictures();
        PikminBehavior.OnPikminFollowStateChanged += UpdatePikminFollowing;
        pikminAllText.text = pikminOverall.ToString();
    }

    //Cnatunuesly look for pikmin, check if they are alive and if all treasures are collected
    void Update()
    {
        FindClosestPikmin(); 
        CheckIfPikminAlive();
        CheckIfTreasuresCollected();
    }

    //cheacks if all treasure is collected and loads win sceen if true
    private void CheckIfTreasuresCollected()
    {
        if (treasureCount == treasuresInTotal)
        {
            SceneManager.LoadScene("TreasuresCollectedScene");
        }
    }

    //checks for what kinds of pikmin are alive and if the pikmin needed to complete teh game are dead loos scene wil load
    private void CheckIfPikminAlive()
    {
        GameObject[] allPikmin = GameObject.FindGameObjectsWithTag("Pikmin");
        int yellowAlive = 0;
        int redAlive = 0;
        int blueAlive = 0;

        foreach (GameObject pikmin in allPikmin)
        {
            PikminBehavior pikminBehavior = pikmin.GetComponent<PikminBehavior>();
            if (pikminBehavior.pikminType == PikminBehavior.PikminType.Blue)
            {
                blueAlive++;
            }
            else if (pikminBehavior.pikminType == PikminBehavior.PikminType.Red)
            {
                redAlive++;
            }
            else if (pikminBehavior.pikminType == PikminBehavior.PikminType.Yellow)
            {
                yellowAlive++;
            }    
        }

        //you cant kill blue right now but just in case
        if (blueAlive < 8 || yellowAlive < 6)
        {
            SceneManager.LoadScene("PikminDiedScene");
        }

    }

    //unsubscibe form ´´ Following event if this object is destroyed
    private void OnDestroy()
    {
        PikminBehavior.OnPikminFollowStateChanged -= UpdatePikminFollowing; // Unsubscribe from event
    }

    //finds the cloosest pikmin that is following the player and sets the UI to mach
    private void FindClosestPikmin()
    {
        GameObject[] allPikmin = GameObject.FindGameObjectsWithTag("Pikmin");
        float closestDistance = Mathf.Infinity;

        foreach (GameObject pikmin in allPikmin)
        {
            PikminBehavior pikminBehavior = pikmin.GetComponent<PikminBehavior>();
            if (pikminBehavior != null && pikminBehavior.task == PikminBehavior.Task.FollowingTask)
            {   
                float distance = Vector3.Distance(player.transform.position, pikmin.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;

                    // Access the PikminType from the PikminBehavior component
                    if (pikminBehavior.pikminType == PikminBehavior.PikminType.Blue)
                    {
                        TurnOffAllPictures();
                        bluePicture.SetActive(true);
                    }
                    else if (pikminBehavior.pikminType == PikminBehavior.PikminType.Red)
                    {
                        TurnOffAllPictures();
                        redPicture.SetActive(true);
                    }
                    else if (pikminBehavior.pikminType == PikminBehavior.PikminType.Yellow)
                    {
                        TurnOffAllPictures();
                        yellowPicture.SetActive(true);
                    }
                }
            }
        }
    }
    
    //turns off all pictures for pikmin in the UI
    private void TurnOffAllPictures()
    {
        redPicture.SetActive(false);
        bluePicture.SetActive(false);
        yellowPicture.SetActive(false);
    }

    //changes the pikminfollwing variable is dependant on the pikminBehavior script
    private void UpdatePikminFollowing(int change)
    {
        pikminFollowing += change;
        pikminCountText.text = pikminFollowing.ToString();
    }

    //decreases the overall pikmin count and updates the UI text
    public void DecreasePikminCount()
    {
        pikminOverall--;
        pikminAllText.text = pikminOverall.ToString();
    }

    //increases the treasure count and updates the UI text
    public void UpdateTreasureCount()
    {
        treasureCount++;
        treasureCountText.text = treasureCount.ToString();
    }
}
