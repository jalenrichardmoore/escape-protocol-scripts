using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class TitleManager : MonoBehaviour
{
    // Static Reference
    static public TitleManager tm;

    // UI References
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject characterSelectMenu;
    [SerializeField] private GameObject loadingBar;
    [SerializeField] private Slider loadingSlider;
    [SerializeField] private TMP_Text loadingProgressText;

    private void Start()
    {
        // Initialize variables
        tm = GetComponent<TitleManager>();
        GameData.currentModelIndex = 5;

        loadingBar.SetActive(false);                                        // Deactivate the level loading bar
        MainMenu();                                                         // Display the main menu
    }

    // Switches the current active screen to the main menu
    public void MainMenu()
    {
        mainMenu.SetActive(true);
        characterSelectMenu.SetActive(false);
    }

    // Switches the current active screen to the character select screen
    public void CharacterSelectMenu()
    {
        characterSelectMenu.SetActive(true);
        mainMenu.SetActive(false);
    }

    // Selects the cop as the player character and loads the game level
    public void SelectCop(int sceneIndex)
    {
        GameData.playerRole = 0;                                            // Initialize the player's role to be 'Cop'
        loadingBar.SetActive(true);                                         // Activate the level loading bar
        StartCoroutine(LoadAsynchronously(sceneIndex));                     // Load the game level
    }

    // Selects the robber as the player character and loads the game level
    public void SelectRobber(int sceneIndex)
    {
        GameData.playerRole = 1;                                            // Initialize the player's role to be 'Robber'
        loadingBar.SetActive(true);                                         // Activate the level loading bar
        StartCoroutine(LoadAsynchronously(sceneIndex));                     // Load the game level
    }

    // Quits the game
    public void QuitGame()
    {
        Application.Quit();
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