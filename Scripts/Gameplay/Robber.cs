using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Robber : MonoBehaviour
{
    public enum TYPE {Agent, Player}                                        // Types of possible robber game objects                                           

    // Component References
    public SpawnPoint spawnPoints;

    // Game Object References
    public GameObject diamondToCollect;                                     // Diamond that the robber is trying to collect
    public GameObject diamondToDeposit;                                     // Diamond that the robber is trying to deposit
    public GameObject depositZone;                                          // Deposit zone reference

    // Target Lists
    public List<GameObject> diamonds;                                       // List of all uncollected diamonds in the environment
    public List<GameObject> collectedDiamonds;                              // List of all diamonds collected by this robber

    // Robber Variables
    public TYPE robberType;                                                 // Defines the type of robber (Agent/Player)
    public Coroutine powerUp;                                               // Coroutine reference to robber power-up

    public float timeUntilPowerUp;                                          // Time until power-up can be activated next
    public float movementSpeed;                                             // Speed at which the robber moves

    // Flags
    public bool canCollectDiamond;                                          // Flag that checks if the robber can collect a diamond
    public bool canDepositDiamond;                                          // Flag that checks if the robber can deposit a diamond
    public bool diamondIsCollected;                                         // Flag that checks if the robber current has a diamond
    public bool isHiding;                                                   // Flag that checks if the robber is current hiding
    public bool canWinGame;                                                 // Flag that checks if the robber has successfully completed its objective
    public bool isPositioned;                                               // Flag that checks if the robber has been moved to a spawn point

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Diamond") && !diamondIsCollected)             // Check if the robber collided with a diamond and is not currently holding one
        {
            diamondToCollect = other.gameObject;                            // Update diamond to be collected
            canCollectDiamond = true;                                       // Update flag to show the robber can collect a diamond
        }
        else if (other.CompareTag("Deposit Zone") && diamondIsCollected)    // Check if the robber has collided with the deposit zone and is currently holdign a diamond
        {
            diamondToDeposit = transform.GetChild(2).gameObject;            // Update diamond to be deposited
            canDepositDiamond = true;                                       // Update flag to show the robber can deposit a diamond
        } 
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Diamond"))                                    // Check if the robber is leaving a diamond
        {
            canCollectDiamond = false;                                      // Update flag to show the robber cannot collect a diamond
            diamondToCollect = null;                                        // Update diamond to be collected
        }
        else if (other.CompareTag("Deposit Zone"))                          // Check if the robber is leaving the deposit zone
        {
            canDepositDiamond = false;                                      // Update flag to show the robber cannot deposit a diamond
            diamondToDeposit = null;                                        // Update diamond to be deposited 
        }
    }

    // Initializes variables and sets the list of targets
    public void InitializeRobber(TYPE type)
    {
        // Initialize variables
        spawnPoints = transform.parent.GetComponentInChildren<SpawnPoint>();
        robberType = type;
        powerUp = null;

        timeUntilPowerUp = 0f;
        movementSpeed = 10f;

        // Initialize flags
        canCollectDiamond = false;
        canDepositDiamond = false;
        diamondIsCollected = false;
        isHiding = false;
        canWinGame = false;
        isPositioned = false;

        diamonds = new List<GameObject>();                                  // Initialize a new list for diamond targets
        collectedDiamonds = new List<GameObject>();                          // Initialize a new list for collected diamonds
        for (int i = 0; i < transform.parent.childCount; i++)               // Loop through all game objects in the environment
        {
            GameObject child = transform.parent.GetChild(i).gameObject;
            if (child.CompareTag("Diamond")) diamonds.Add(child);           // Add all diamonds in the environment to the target list
            else if (child.CompareTag("Deposit Zone")) depositZone = child; // Initialize reference to the deposit zone
        }
    }

    // Moves the diamonds and deposit zone into position
    public void MoveObjectsIntoPosition()
    {
        if (!spawnPoints.spawnPointsSet) spawnPoints.RefreshSpawnPoints();  // Refresh available spawn points if they have not been refreshed
        foreach (GameObject diamond in diamonds)                            // Loop through all diamonds in the environment
        {
            Diamond diamondScript = diamond.GetComponent<Diamond>();
            if (!diamondScript.isPositioned)                                // Check if the diamond has been positioned and move it to a random spawn point
            {
                spawnPoints.MoveToSpawnPoint(diamond, new Vector3(0f, 5f, 0f), Quaternion.Euler(-90f, 0f, 0f));
                diamondScript.spawnPointPosition = diamond.transform.position;

                diamondScript.isPositioned = true;                          // Update flag to show that the diamond is positioned
                diamondScript.StartAnimation();                             // Start animating the diamond
            }
        }

        // Check if the deposit zone has been positioned and move it to a random spawn point
        if (depositZone != null && !depositZone.GetComponent<DepositZone>().isPositioned)
        {
            spawnPoints.MoveToSpawnPoint(depositZone, Vector3.zero, Quaternion.Euler(Vector3.zero));
            
            depositZone.GetComponent<DepositZone>().isPositioned = true;    // Update flag to show that the deposit zone is positioned
        }
    }

    // Moves a diamond accordingly, dependent on the given command
    public void MoveDiamond(GameObject diamond, string command)
    {
        if (command == "Collect")                                           // Check if the robber is collecting a diamond
        {
            // Play the diamond's audio clip
            AudioManager.am.audioSource.PlayOneShot(AudioManager.am.diamond);

            canCollectDiamond = false;                                      // Update flag to show a diamond cannot be collected
            diamondIsCollected = true;                                      // Update flag to show a diamond is currently collected
            diamondToCollect = null;                                        // Update diamond to be collected
        
            for (int i = 0; i < transform.parent.childCount; i++)           // Remove collected diamond from all robbers' target lists
            {
                GameObject child = transform.parent.GetChild(i).gameObject;
                if (child.CompareTag("Robber")) child.GetComponent<Robber>().diamonds.Remove(diamond);
            }

            diamond.GetComponent<Diamond>().playAnimation = false;          // Update flag to stop animating the diamond
            diamond.transform.parent = transform;                           // Make the diamond a child of the robber
            diamond.transform.localPosition = new Vector3(0f, 2f, 0f);      // Reposition the diamond above the robber
        }
        else if (command == "Deposit")                                      // Check if the robber is depositing a diamond
        {
            // Play the diamond's audio clip
            AudioManager.am.audioSource.PlayOneShot(AudioManager.am.diamond);
            
            canDepositDiamond = false;                                      // Update flag to show a diamond cannot be deposited
            diamondIsCollected = false;                                     // Update flag to show a diamond is not currently collected
            diamondToDeposit = null;                                        // Update diamond to be deposited

            diamond.transform.parent = transform.parent;                    // Make the diamond a child of the environment
            diamond.transform.localPosition = new Vector3(0f, 50f, 0f);     // Reposition the diamond above the environment
            
            AudioManager.am.audioSource.PlayOneShot(AudioManager.am.diamond);
            
            diamond.SetActive(false);                                       // Deactivate the collected diamond
            GameData.numDiamondsCollected++;                                // Increment the number of diamonds collected
        
            bool diamondsLeft = false;                                      // Initialize flag to check if any robbers are currently carrying a diamond
            for (int i = 0; i < transform.parent.childCount; i++)           // Check if any robbers in the environment currently have a diamond
            {
                GameObject child = transform.parent.GetChild(i).gameObject;
                if (child.CompareTag("Robber") && child.GetComponent<Robber>().diamondIsCollected)
                {
                    diamondsLeft = true;                                    // Update flag to show that a diamond is still in the environment
                    break;
                }
            }
            if (!diamondsLeft && diamonds.Count == 0) canWinGame = true;    // Check if all diamonds are collected and update flag to show the robber has completed its objective
        }
    }

    // Activates the robber's power-up is enough time has passed
    public bool ActivatePowerUp()
    {
        if (powerUp == null && Time.time > timeUntilPowerUp)                // Check if the power-up is not active and enough time has passed to activate it
        {
            GameData.timesHidden++;                                         // Increment the number of times the power-up has been activated
            powerUp = StartCoroutine(Hide());                               // Start the coroutine
            return true;
        }
        return false;
    }

    // Temporarily prevents the robber from being tagged by cops
    private IEnumerator Hide()
    {
        isHiding = true;                                                    // Update flag to show the robber is hiding

        // Stop NavMeshAgent if robber is an agent
        if (robberType == TYPE.Agent) GetComponent<NavMeshAgent>().isStopped = true;

        transform.GetChild(1).gameObject.SetActive(true);                   // Activate the robber's barrier
        GetComponent<CapsuleCollider>().enabled = false;                    // Disable the robber's collider
        yield return new WaitForSeconds(10f);

        isHiding = false;                                                   // Update flag to show the robber is no longer hiding

        // Restart NavMeshAgent if robber is an agent
        if (robberType == TYPE.Agent) GetComponent<NavMeshAgent>().isStopped = false;

        transform.GetChild(1).gameObject.SetActive(false);                  // Dectivate the robber's barrier
        GetComponent<CapsuleCollider>().enabled = true;                     // Enable the robber's collider
    
        timeUntilPowerUp = Time.time + 10f;                                 // Update timer for power-up
        powerUp = null;                                                     // Reset coroutine reference
        yield break;
    }
}