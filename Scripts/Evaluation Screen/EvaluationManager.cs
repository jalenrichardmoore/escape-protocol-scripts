using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

using Python.Runtime;

public class EvaluationManager : MonoBehaviour
{
    // Static Reference
    static EvaluationManager em;

    // UI References
    [SerializeField] private GameObject selectCopButton;
    [SerializeField] private GameObject selectRobberButton;
    [SerializeField] private GameObject playAgainButton;
    [SerializeField] private GameObject quitGameButton;
    [SerializeField] private TMP_Text playerStats;
    [SerializeField] private TMP_Text evaluation;
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private Slider loadingSlider;
    [SerializeField] private TMP_Text loadingProgressText;

    // Evaluation Variables
    public Dictionary<string, float> sessionData;                           // Stores data of player performance for current session
    private string evaluationResult;                                        // Stores the result of the algorithm's evaluation

    // Flags
    public bool sessionDataCreated;                                         // Flag that checks if the session data dictionary has been created

    private void Start()
    {
        Cursor.visible = true;                                              // Enable the cursor after gameplay

        // Initialize variables
        em = GetComponent<EvaluationManager>();
        sessionData = new Dictionary<string, float>();
        evaluationResult = "";
        
        CreateSessionData();                                                // Populate the dictionary with player statistics for current session
        CreateStatisticsMenu();                                             // Update the menu detailing the player's performance statistics
        
        // Send the session data to be evaluated by the XGBoostClassifier model
        // Initialize Python connection
        Runtime.PythonDLL = Application.streamingAssetsPath + "/python39.dll";
        PythonEngine.Initialize();
        using (Py.GIL())
        {
            dynamic sys = Py.Import("sys");
            sys.path.append(Application.streamingAssetsPath);
            
            var pythonScript = Py.Import("difficultyEvaluation");
            var result = pythonScript.InvokeMethod("evaluate_entry");
        }
    }

    // Stores summary of player's perforamnce in current game session
    private void CreateSessionData()
    {
        // Add data stored in 'GameData' to the session data summary
        sessionData.Add("Success State", GameData.successState);
        sessionData.Add("Session Time", GameData.sessionTime);
        sessionData.Add("Player Type", (float)GameData.playerRole);
        sessionData.Add("Percentage Robbers Tagged", GameData.percentageRobbersTagged);
        sessionData.Add("Times Sped Up", GameData.timesSpedUp);
        sessionData.Add("Percentage Diamonds Collected", GameData.percentageDiamondsCollected);
        sessionData.Add("Times Hidden", GameData.timesHidden);

        sessionDataCreated = true;                                          // Update flag to show that the session data summary has been created
    }

    // Updates the display of the player's statistics menu
    private void CreateStatisticsMenu()
    {
        playAgainButton.SetActive(true);                                    // Enable the 'Play Again' button
        quitGameButton.SetActive(true);                                     // Enable the 'Quit Game' button
        selectCopButton.SetActive(false);                                   // Disable the 'Select Cop' button
        selectRobberButton.SetActive(false);                                // Disalbe the 'Select Robber' button

        // Initialize text variables
        string successState = (sessionData["Success State"] == 1) ? "Win" : "Loss";
        string playerRole = (sessionData["Player Type"] == 0) ? "Cop" : "Robber";

        // Update the 'Player Statistics' menu to display the current session summary
        playerStats.text = "Player Statistics:\n\n";
        playerStats.text += "Success State: " + successState + "\n";
        playerStats.text += "Session Time: " + sessionData["Session Time"].ToString("F3") + "s\n";
        playerStats.text += "Player Type: " + playerRole + "\n";
        playerStats.text += "Percentage Robbers Tagged: " + (sessionData["Percentage Robbers Tagged"] * 100).ToString("F1") + "%\n";
        playerStats.text += "Times Sped Up: " + sessionData["Times Sped Up"] + "\n";
        playerStats.text += "Percentage Diamonds Collected: " + (sessionData["Percentage Diamonds Collected"] * 100).ToString("F1") + "%\n";
        playerStats.text += "Times Hidden: " + sessionData["Times Hidden"]; 
    }

    // Adjusts the game difficulty dependent on evaluation result
    public void AdjustDifficulty(string result)
    {
        evaluationResult = result;                                           // Save the evaluation's result
        evaluation.text += result;                                          // Update the evaluation text with the result

        // Adjust the AI difficulty according to the evaluation result
        if (result == "Increase") GameData.currentModelIndex = Mathf.Min(GameData.currentModelIndex + 1, 9);
        else if (result == "Decrease") GameData.currentModelIndex = Mathf.Max(GameData.currentModelIndex - 1, 0);
    }

    // Activates the 'Select Cop' & 'Select Robber' Buttons so the player can play the game again
    public void PlayAgain()
    {
        // Move the 'Quit Game' button over to the right
        quitGameButton.GetComponent<RectTransform>().anchoredPosition += new Vector2(350f, 0f);

        playAgainButton.SetActive(false);                                   // Disable the 'Play Again' button
        selectCopButton.SetActive(true);                                    // Enable the 'Select Cop' button
        selectRobberButton.SetActive(true);                                 // Enable the 'Select Robber' button
    }

    // Quits the game
    public void QuitGame()
    {
        PythonEngine.Shutdown();
        Application.Quit();
    }

    // Selects the cop as the player character and reloads the game level
    public void SelectCop(int sceneIndex)
    {
        int randomChance = Random.Range(0, 100);                            // Generate a random number

        // Randomly adjust cop objective, dependent on evaluation result
        if (randomChance > 80)
        {
            if (evaluationResult == "Increase") GameData.numRobberAgents = Mathf.Min(GameData.numRobberAgents + 1, 4);
            else if (evaluationResult == "Decrease") GameData.numRobberAgents = Mathf.Max(GameData.numRobberAgents - 1, 1);
        }

        GameData.playerRole = 0;                                            // Initialize the player's role to be 'Cop'
        loadingScreen.SetActive(true);                                      // Activate the loading screen
        StartCoroutine(LoadAsynchronously(sceneIndex));                     // Load the game level
    }

    // Selects the robber as the player character and loads the game level
    public void SelectRobber(int sceneIndex)
    {
        int randomChance = Random.Range(0, 100);                            // Generate a random number

        // Randomly adjust cop objective, dependent on evaluation result
        if (randomChance > 80)
        {
            if (evaluationResult == "Increase") GameData.numDiamonds = Mathf.Min(GameData.numDiamonds + 1, 4);
            else if (evaluationResult == "Decrease") GameData.numDiamonds = Mathf.Max(GameData.numDiamonds - 1, 1);
        }

        GameData.playerRole = 1;                                            // Initialize the player's role to be 'Robber'
        loadingScreen.SetActive(true);                                      // Activate the loading screen
        StartCoroutine(LoadAsynchronously(sceneIndex));                     // Load the game level
    }

    // Loads level asynchronously while updating progress bar
    private IEnumerator LoadAsynchronously(int sceneIndex)
    {
        PythonEngine.Shutdown();
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