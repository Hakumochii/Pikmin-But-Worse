using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public int pikminFollowing;
    private int pikminOverall = 3;
    public TextMeshProUGUI pikminCountText;
    public TextMeshProUGUI treasureCountText;
    public TextMeshProUGUI pikminAllText;
    public GameObject redPicture;
    public GameObject bluePicture;
    public GameObject yellowPicture;
    public GameObject player;

    
    void Start()
    {
        TurnOffAllPictures();
        PikminBehavior.OnPikminFollowStateChanged += UpdatePikminCount;
        pikminAllText.text = pikminOverall.ToString();
    }

    void Update()
    {
        FindClosestPikmin(); 
    }

    private void OnDestroy()
    {
        PikminBehavior.OnPikminFollowStateChanged -= UpdatePikminCount; // Unsubscribe from event
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

    private void UpdatePikminCount(int change)
    {
        pikminFollowing += change;
        pikminCountText.text = pikminFollowing.ToString();
    }

    public void DecreasePikminCount()
    {
        pikminOverall--;
        pikminAllText.text = pikminOverall.ToString();
    }
}
