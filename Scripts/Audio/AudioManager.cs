using UnityEngine;

public class AudioManager : MonoBehaviour
{
    // Static Reference
    public static AudioManager am;

    // Audio References
    public AudioSource audioSource;
    public AudioClip diamond;
    public AudioClip robber;

    private void Awake()
    {
        // Initialize variables
        am = GetComponent<AudioManager>();
        
        // Let the Audio Manager persist between scenes
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        // If the 'ESC' key is pressed, quit the game
        if (Input.GetKey(KeyCode.Escape)) Application.Quit();
    }
}
