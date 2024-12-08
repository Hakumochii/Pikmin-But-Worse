using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public int pikminFollowing;
    public int pikminOverall = 3;
    public TextMeshProUGUI pikminCountText;
    public int treasureCount;
    private int treasuresInTotal = 3;
    public TextMeshProUGUI treasureCountText;
    public TextMeshProUGUI pikminAllText;
    public GameObject redPicture;
    public GameObject bluePicture;
    public GameObject yellowPicture;
    public GameObject player;


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

    
    void Start()
    {
        TurnOffAllPictures();
        PikminBehavior.OnPikminFollowStateChanged += UpdatePikminFollowing;
        pikminAllText.text = pikminOverall.ToString();
    }

    void Update()
    {
        FindClosestPikmin(); 
        CheckIfPikminAlive();
        CheckIfTreasuresCollected();
    }

    private void CheckIfTreasuresCollected()
    {
        if (treasureCount == treasuresInTotal)
        {
            SceneManager.LoadScene("TreasuresCollectedScene");
        }
    }

    private void CheckIfPikminAlive()
    {
        GameObject[] allPikmin = GameObject.FindGameObjectsWithTag("Pikmin");
        int yellowAlive = 0;
        int redAlive = 0;
        int blueAlive = 0;

        foreach (GameObject pikmin in allPikmin)
        {
            PikminBehavior pikminBehavior = pikmin.GetComponent<PikminBehavior>();
            // Access the PikminType from the PikminBehavior component
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

        if (blueAlive < 1 || yellowAlive < 1)
        {
            SceneManager.LoadScene("PikminDiedScene");
        }

    }

    private void OnDestroy()
    {
        PikminBehavior.OnPikminFollowStateChanged -= UpdatePikminFollowing; // Unsubscribe from event
    }

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
    
    private void TurnOffAllPictures()
    {
        redPicture.SetActive(false);
        bluePicture.SetActive(false);
        yellowPicture.SetActive(false);
    }

    private void UpdatePikminFollowing(int change)
    {
        pikminFollowing += change;
        pikminCountText.text = pikminFollowing.ToString();
    }

    public void DecreasePikminCount()
    {
        pikminOverall--;
        pikminAllText.text = pikminOverall.ToString();
    }

    public void UpdateTreasureCount()
    {
        treasureCount++;
        treasureCountText.text = treasureCount.ToString();
    }
}
