using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents.Policies;
using Unity.Barracuda;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class DowntownManager : MonoBehaviour
{
    // Static Reference
    static public DowntownManager dm;

    // UI References
    [SerializeField] private TMP_Text objectiveText;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private Slider loadingSlider;
    [SerializeField] private TMP_Text loadingProgressText;

    // Prefab References
    [SerializeField] private GameObject copAgent;
    [SerializeField] private GameObject robberAgent;
    [SerializeField] private GameObject copPlayer;
    [SerializeField] private GameObject robberPlayer;
    [SerializeField] private GameObject diamond;
    [SerializeField] private GameObject depositZone;

    // Game Object References
    public Transform downtownEnvironment;
    public GameObject player;

    // Agent Model References
    [SerializeField] private List<NNModel> copModels;
    [SerializeField] private List<NNModel> robberModels;

    private int objectiveTotal;                                             // Number of targets the player must collect

    public float currentSessionTime;                                        // Stores the length of the current session

    // Flags
    private bool sessionEnded;                                              // Flag that checks if the session has ended

    private void Start()
    {
        Cursor.visible = false;                                             // Disable the cursor during gameplay

        // Initialize variables
        dm = GetComponent<DowntownManager>();
        currentSessionTime = 0f;
        sessionEnded = false;


        if (GameData.playerRole == 0) objectiveTotal = GameData.numRobberAgents;
        else if (GameData.playerRole == 1) objectiveTotal = GameData.numDiamonds;

        // Instantiate the diamonds and deposit zone and make them children of the environment
        for (int i = 0; i < GameData.numDiamonds; i++) Instantiate(diamond, downtownEnvironment);
        Instantiate(depositZone, downtownEnvironment);
    
        if (GameData.playerRole == 0)                                       // Check if the player is a cop
        {
            for (int i = 0; i < GameData.numRobberAgents; i++)              // Create all robber agents and make them children of the environment
            {
                GameObject robber = Instantiate(robberAgent, downtownEnvironment);

                // Initialize agent with the current difficulty model
                robber.GetComponent<BehaviorParameters>().Model = robberModels[GameData.currentModelIndex];
                robber.GetComponent<BehaviorParameters>().BehaviorType = BehaviorType.InferenceOnly;
            }

            player = Instantiate(copPlayer, downtownEnvironment);           // Create the player and make them a child of the environment
        }
        else if (GameData.playerRole == 1)                                  // Check if the player is a robber
        {
            player = Instantiate(robberPlayer, downtownEnvironment);        // Create the player and make them a child of the environment

            for (int i = 0; i < GameData.numCopAgents; i++)                 // Create all cop agents and make them children of the environment
            {
                GameObject cop = Instantiate(copAgent, downtownEnvironment);

                // Initialize agent with the current difficulty model
                cop.GetComponent<BehaviorParameters>().Model = copModels[GameData.currentModelIndex];
                cop.GetComponent<BehaviorParameters>().BehaviorType = BehaviorType.InferenceOnly;
            }
        }

        // Position the camera to be directly above the player
        Camera.main.transform.position = new Vector3(player.transform.position.x, Camera.main.transform.position.y, player.transform.position.z);

        GameData.ResetSessionData();                                        // Reset session data for current session
    }

    private void Update()
    {    
        DisplayTime(currentSessionTime);                                    // Display the length of current session
        currentSessionTime += Time.deltaTime;                               // Increment current session time

        // Update the objective text as the player completes their objective
        if (player.CompareTag("Cop")) objectiveText.text = "Robbers Tagged:\n" + GameData.numRobbersTagged + " / " + objectiveTotal;
        else if (player.CompareTag("Robber")) objectiveText.text = "Diamonds Collected:\n" + GameData.numDiamondsCollected + " / " + objectiveTotal;

        // Update the objective text when the player is ready to win the game
        if (player.CompareTag("Cop") && player.GetComponent<Cop>().canWinGame || player.CompareTag("Robber") && player.GetComponent<Robber>().canWinGame) objectiveText.text = "Press \'Enter\' to win!";
    }

    private void LateUpdate()
    {
        // Check if there is a player, and updates the main camera position to track player movement
        if (player != null) Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, new Vector3(player.transform.position.x, Camera.main.transform.position.y, player.transform.position.z), 0.125f);
    }

    // Displays the total session time in a seconds-milliseconds format
    private void DisplayTime(float timeToDisplay)
    {
        float seconds = Mathf.FloorToInt(timeToDisplay);                    // Calculate total seconds that have passed
        float milliseconds = (timeToDisplay - seconds) * 100;               // Calculate total milliseconds that have passed

        // Display the time in seconds:milliseconds format
        timerText.text = string.Format("{0:00}:{1:00}", seconds, Mathf.FloorToInt(milliseconds));
    }

    // End the current session and load the player evaluation screen
    public void EndSession(bool victoryStatus)
    {
        if (!sessionEnded)                                                  // Check if the session is still going on
        {
            sessionEnded = true;                                            // Update flag to show that the session is now ending
        
            // Calculate session data for evaluation
            GameData.successState = victoryStatus ? 1f : 0f;
            GameData.sessionTime = Time.time;
            GameData.percentageDiamondsCollected = (float) GameData.numDiamondsCollected / GameData.numDiamonds;
            GameData.percentageRobbersTagged = GameData.playerRole == 0 ? (float) GameData.numRobbersTagged / GameData.numRobberAgents : (float) GameData.numRobbersTagged / 1;
        
            loadingScreen.SetActive(true);                                  // Enable the loading screen
            StartCoroutine(LoadAsynchronously(2));                          // Load the 'Evaluation Screen' scene
        }
    }

    // Loads level asynchronously while updating progress bar
    private IEnumerator LoadAsynchronously(int sceneIndex)
    {
        // Load the appropriate level
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneIndex);
        
        while (!operation.isDone)                                           // Loops while the level is still loading
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);      // Calculates progress of loading
            loadingSlider.value = progress;                                 // Updates the progress bar
            loadingProgressText.text = (int)(progress * 100) + "%";         // Displays the percentage
            yield return null;
        }
    }
}