using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Cop : MonoBehaviour
{
    public enum TYPE {Agent, Player}                                        // Types of possible cop game objects                                           

    // Component References
    public SpawnPoint spawnPoints;

    // Target Lists
    public List<GameObject> robbers;                                        // List of all untagged robbers in the environment
    public List<GameObject> taggedRobbers;                                  // List of all robbers tagged by this cop

    // Cop Variables
    public TYPE copType;                                                    // Defines the type of cop (Agent/Player)
    public Coroutine powerUp;                                               // Coroutine reference to cop power-up

    public float timeUntilPowerUp;                                          // Time until power-up can be activated next
    public float movementSpeed;                                             // Speed at which the cop moves

    // Flags
    public bool canWinGame;                                                 // Flag that checks if the cop has successfully completed its objective
    public bool isPositioned;                                               // Flag taht checks if the cop has been moved to a spawn point

    // Initializes variables and sets the list of targets
    public void InitializeCop(TYPE type)
    {
        // Initialize variables
        spawnPoints = transform.parent.GetComponentInChildren<SpawnPoint>();
        copType = type;
        powerUp = null;

        timeUntilPowerUp = 0f;
        movementSpeed = 6f;

        // Initialize flags
        canWinGame = false;
        isPositioned = false;

        robbers = new List<GameObject>();                                   // Initialize a new list for robber targets
        taggedRobbers = new List<GameObject>();                             // Initialize a new list for tagged robbers
        for (int i = 0; i < transform.parent.childCount; i++)               // Loop through all game objects in the environment
        {
            GameObject child = transform.parent.GetChild(i).gameObject;
            if (child.CompareTag("Robber")) robbers.Add(child);             // Add all robbers in the environment to target list
        }
    }

    // Moves the cop and all robbers in the scene into position
    public void MoveObjectsIntoPosition()
    {
        if (!spawnPoints.spawnPointsSet) spawnPoints.RefreshSpawnPoints();  // Refresh available spawn points if they have not been refreshed
        if (!isPositioned)                                                  // Check if the cop has been positioned and move it to a random spawn point
        {
            spawnPoints.MoveToSpawnPoint(gameObject, new Vector3(0f, 3.2f, 0f), Quaternion.Euler(-25f, 0f, 0f));  
        
            isPositioned = true;                                            // Update flag to show cop has been moved
        }

        foreach (GameObject robber in robbers)                              // Loop through all robbers in environment
        {
            if (!robber.GetComponent<Robber>().isPositioned)                // Check if the robber has been positioned and move it to a random spawn point
            {
                spawnPoints.MoveToSpawnPoint(robber, new Vector3(0f, 3.2f, 0f), Quaternion.Euler(-25f, 0f, 0f));

                robber.GetComponent<Robber>().isPositioned = true;          // Update flag to show the robber has been moved
            }
        }
    }

    // Checks if the cop is a valid distance away from a robber
    public bool CheckDistance(Vector3 cop, Vector3 robber)
    {
        bool isValid = false;                                               // Initialize return variable
        float distance = Vector3.Distance(cop, robber);                     // Calculate distance between cop and robber
    
        if (distance < 6f) isValid = true;                                  // Update return variable if distance is within range
        return isValid;
    }

    // Tags a robber if the cop is close enough to one
    public bool TagRobber()
    {
        foreach (GameObject robber in robbers)                              // Loop through each active robber in the environment
        {
            // Check if the cop is close enough to a robber that is not hiding
            if (CheckDistance(transform.position, robber.transform.position) && !robber.GetComponent<Robber>().isHiding)
            {
                for (int i = 0; i < transform.parent.childCount; i++)       // Remove tagged robber from all active cops in the environment
                {
                    GameObject child = transform.parent.GetChild(i).gameObject;
                    if (child.CompareTag("Cop")) child.GetComponent<Cop>().robbers.Remove(robber);
                }

                if (robber.transform.childCount == 3)                       // Check if the robber was carrying a diamond
                {
                    GameObject diamondChild = robber.transform.GetChild(2).gameObject;
                    Diamond diamondScript = diamondChild.GetComponent<Diamond>();

                    // Move the diamond back to its spawn position and animate the diamond
                    diamondChild.transform.parent = transform.parent;
                    diamondChild.transform.position = diamondScript.spawnPointPosition;
                    diamondScript.StartAnimation();

                    // Add te diamond to all active robbers' target list
                    foreach (GameObject activeRobber in robbers) activeRobber.GetComponent<Robber>().diamonds.Add(diamondChild);
                }

                // Play the robber's audio clip
                AudioManager.am.audioSource.PlayOneShot(AudioManager.am.robber);
                
                taggedRobbers.Add(robber);                                  // Add the robbers to the list of tagged robbers
                robber.SetActive(false);                                    // Deactivate the tagged robber
                GameData.numRobbersTagged++;                                // Increment the number of robbers tagged

                if (robbers.Count == 0) canWinGame = true;                  // Check if all robbers are tagged and update flag to show the cop has completed its objective
                return true;
            }
        }
        return false;
    }

    // Activates the cop's power-up if enough time has passed
    public bool ActivatePowerUp()
    {
        if (powerUp == null && Time.time > timeUntilPowerUp)                // Check if the power-up is not active and enough time has passed to activate it
        {
            GameData.timesSpedUp++;                                         // Increment the number of times the power-up has been activated
            powerUp = StartCoroutine(IncreaseSpeed());                      // Start the coroutine to activate the power-up
            return true;
        }
        return false;
    }

    // Temporarily doubles the cop's movement speed
    private IEnumerator IncreaseSpeed()
    {
        movementSpeed = 12f;                                                // Increases movement speed

        // Update NavMeshAgent if the cop is an agent
        if (copType == TYPE.Agent) GetComponent<NavMeshAgent>().speed = movementSpeed;
        yield return new WaitForSeconds(7f);

        movementSpeed = 6f;                                                 // Return movement speed to normal
    
        // Update NavMeshAgent if the cop is an agent
        if (copType == TYPE.Agent) GetComponent<NavMeshAgent>().speed = movementSpeed;

        timeUntilPowerUp = Time.time + 10f;                                 // Update time until power-up can be activated again
        powerUp = null;                                                     // Reset coroutine reference
        yield break;
    }
}