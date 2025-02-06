using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    [SerializeField] private List<GameObject> spawnPoints;                  // List of all spawn points in the environment
    [HideInInspector] public List<int> usedSpawnPointIndexes;               // List of indices of all used spawn points

    // Flags
    public bool spawnPointsSet;                                             // Flag that checks if the spawn point list has been initialized

    private void Start()
    {
        // Initialize variables
        spawnPoints = new List<GameObject>();
        for (int i = 0; i < transform.childCount; i++) spawnPoints.Add(transform.GetChild(i).gameObject);
        
        usedSpawnPointIndexes = new List<int>();
        spawnPointsSet = false;
    }

    // Refreshes list of available spawn points
    public void RefreshSpawnPoints()
    {        
        usedSpawnPointIndexes.Clear();                                      // Update list to show all spawn points are available
        spawnPointsSet = true;                                              // Update flag to show that spawn points have been initialized
    }

    // Moves a game object to a random, unused spawn point
    public void MoveToSpawnPoint(GameObject objectToMove, Vector3 positionOffset, Quaternion rotation)
    {
        int spawnPointIndex = FindIndex();                                  // Retrieve an index for an unused spawn point
        usedSpawnPointIndexes.Add(spawnPointIndex);                         // Update list to show that spawn point has been used
    
        // Move the game object to the spawn point
        objectToMove.transform.localPosition = spawnPoints[spawnPointIndex].transform.localPosition + positionOffset;
        objectToMove.transform.rotation = rotation;
    }

    // Returns a random index for an unused spawn point
    private int FindIndex()
    {
        int index;                                                          // Initialize return variable
        do
        {
            index = Random.Range(0, spawnPoints.Count);                     // Generate a random index for a spawn point
        } while (usedSpawnPointIndexes.Contains(index));                    // Loop while the index generated has already been used
    
        return index;
    }
}