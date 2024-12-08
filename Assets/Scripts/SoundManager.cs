using UnityEngine;

public class SoundManager : MonoBehaviour
{
    private static SoundManager instance;
    public static SoundManager Instance
    {
        // Ensure there is always an instance of the sound manager
        get
        {
            // Check if the instance is null or has been destroyed
            if (instance == null || instance.gameObject == null)
            {
                // Find an existing instance in the scene
                instance = FindObjectOfType<SoundManager>();

                // If no instance exists, create a new one
                if (instance == null)
                {
                    GameObject obj = new GameObject(nameof(SoundManager));
                    instance = obj.AddComponent<SoundManager>();
                }
            }
            return instance;
        }
    }

    [SerializeField] private AudioSource backgroundMusic;

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

    // Methods for playing audioclips 
    public void PlayBackgroundMusic()
    {
        if (backgroundMusic != null)
        {
            backgroundMusic.Play();
        }
    }

}

