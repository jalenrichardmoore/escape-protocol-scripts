using UnityEngine;
using UnityEngine.AI;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class RobberController : Agent
{
    // Agent Variables
    [SerializeField] private NavMeshAgent robberNavMesh;
    [SerializeField] private Robber robber;
    private bool validDistance;

    // Target Variables
    [SerializeField] private GameObject targetObject;
    private bool targetSet;

    // Environment Variables
    [SerializeField] private GameObject environment;
    private Material evaluationMaterial;

    public override void Initialize()
    {
        // Initialize variables
        if (environment != null) evaluationMaterial = environment.transform.GetChild(1).GetComponent<Renderer>().material;
        else environment = GameObject.FindGameObjectWithTag("Downtown");
        robber.InitializeRobber(Robber.TYPE.Agent);
    }

    public override void OnEpisodeBegin()
    {
        // Initialize variables for current episode
        robber.InitializeRobber(Robber.TYPE.Agent);
        robberNavMesh.speed = robber.movementSpeed;

        // Initialize flags
        robberNavMesh.isStopped = false;
        validDistance = true;

        // Check if the robber has a diamond and make it a child of the environment
        if (transform.childCount == 3) transform.GetChild(2).parent = transform.parent;

        Debug.Log("Moving diamonds and deposit zone");
        robber.MoveObjectsIntoPosition();                                   // Move the diamonds and deposit zone into position

        // Initialize target variables
        targetSet = false;
        targetObject = null;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(targetSet);                                   // Collect info on if the agent has set a target
        sensor.AddObservation(robber.powerUp == null);                      // Collect info on if the agent's power-up is active
        sensor.AddObservation(Time.time > robber.timeUntilPowerUp);         // Collect info on if the agent can activate its power-up again
        sensor.AddObservation(validDistance);                               // Collect info on if the agent is close enough to a cop to activate its power-up
        sensor.AddObservation(robber.isHiding);                             // Collect info on if the agent is hiding
        sensor.AddObservation(robber.canCollectDiamond);                    // Collect info on if the agent can collect a diamond
        sensor.AddObservation(robber.canDepositDiamond);                    // Collect info on if the agent can deposit a diamond
        sensor.AddObservation(robber.canWinGame);                           // Collect info on if the agent has completed its objective
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;    // Collect the discrete actions

        // Receive input for movement
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) discreteActions[0] = 1;
        else discreteActions[0] = 0;

        // Receive input for activating the power-up
        if (Input.GetKey(KeyCode.E)) discreteActions[1] = 1;
        else discreteActions[1] = 0;

        // Receive input for tagging robber
        if (Input.GetKey(KeyCode.Space)) discreteActions[2] = 1;
        else discreteActions[2] = 0;

        // Receive input for ending the session
        if (Input.GetKey(KeyCode.Return)) discreteActions[3] = 1;
        else discreteActions[3] = 0;
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        bool moveToTarget = actions.DiscreteActions[0] > 0;                 // Receive input action for movement
        if (moveToTarget && !robber.isHiding)                               // Check if the agent is attempting to move towards a target and is not hiding
        {
            if (!targetSet)                                                 // Check if the agent currently does not have a target
            {
                // Check if the robber has a diamond and if there are any diamonds left to target, and set appropriate target
                if (!robber.diamondIsCollected && robber.diamonds.Count > 0) targetObject = robber.diamonds[Random.Range(0, robber.diamonds.Count)];
                else targetObject = robber.depositZone;

                // Move agent towards target
                robberNavMesh.SetDestination(targetObject.transform.position);
                targetSet = true;                                           // Update flag to show the agent has a target

                AddReward(3f);                                              // Reward agent for succesfully targetting
                if (evaluationMaterial != null) evaluationMaterial.color = Color.blue;
            }
            else                                                            // Check if the agent currently does have a target
            {
                AddReward(-1f);                                             // Punish agent for attempting to make a second target
                if (evaluationMaterial != null) evaluationMaterial.color = Color.yellow;
            }
        }

        bool activatePowerUp = actions.DiscreteActions[1] > 0;              // Receive input action for activating power-up
        if (activatePowerUp)                                                // Check if the agent is attempting to activate a power-up
        {
            if (validDistance)                                              // Check if the agent is close enough to a cop to activate the power-up
            {
                if (robber.ActivatePowerUp())                               // Check if the power-up was activated successfully
                {
                    AddReward(2f);                                          // Reward agent for successfully activating its power-up
                    if (evaluationMaterial != null) evaluationMaterial.color = Color.blue;
                }
                else                                                        // Check if the power-up failed to activate
                {
                    AddReward(-1f);                                         // Punish agent for failing to activate its power-up
                    if (evaluationMaterial != null) evaluationMaterial.color = Color.yellow;
                }
            }
        }

        bool moveDiamond = actions.DiscreteActions[2] > 0;                  // Receive input action for collecting/depositing a diamond
        if (moveDiamond)                                                    // Check if the agent is attempting to move a diamond
        {
            if (robber.canCollectDiamond)                                   // Check if the agent can collect a diamond
            {
                robber.MoveDiamond(robber.diamondToCollect, "Collect");     // Collect the diamond
                targetSet = false;                                          // Update flag to show that the agent does not have a target

                AddReward(5f);                                              // Reward agent for successfully collecting a diamond
                if (evaluationMaterial != null) evaluationMaterial.color = Color.blue;
            }
            else if (robber.canDepositDiamond)                              // Check if the agent can deposit a diamond
            {
                robber.MoveDiamond(robber.diamondToDeposit, "Deposit");     // Deposit the diamond
                targetSet = false;                                          // Update flag to show that the agent does not have a target

                AddReward(5f);                                              // Reward agent for successfully depositing a diamond
                if (evaluationMaterial != null) evaluationMaterial.color = Color.blue;
            }
            else                                                            // Check if the agent can neither collect nor deposit a diamond
            {
                AddReward(-2f);                                             // Punish agent for attempting to move a diamond when it couldn't
                if (evaluationMaterial != null) evaluationMaterial.color = Color.yellow;
            }
        }

        bool completeObjective = actions.DiscreteActions[3] > 0;            // Receive input action for completing agent's objective
        if (completeObjective)                                              // Check if the agent is attempting to complete its objective
        {
            if (robber.canWinGame)                                          // Check if the agent has completed its objective
            {
                for (int i = 0; i < transform.parent.childCount; i++)       // End the current session for all agents
                {
                    GameObject child = transform.parent.GetChild(i).gameObject;
                    if (child.TryGetComponent<RobberController>(out RobberController robberAgent)) robberAgent.EndSession(true);
                    else if (child.TryGetComponent<CopController>(out CopController copAgent)) copAgent.EndSession(false);
                }
            }
        }
        else                                                                // Check if the agent has not completed its objective
        {
            AddReward(-3f);                                                 // Punish agent for attempting to end episode early
            if (evaluationMaterial != null) evaluationMaterial.color = Color.yellow;
        }
    }

    private void Update()
    {
        CheckDistance();                                                    // Check if the agent is close enough to a cop to activate power-up

        if (targetObject == null) return;                                   // Return if the agent does not have a target
        
        // Checks if the agent's current target has been collected 
        if (targetObject.CompareTag("Diamond") && !robber.diamonds.Contains(targetObject))
        {
            targetSet = false;                                              // Update flag to show the agent does not have a target
            targetObject = null;                                            // Update target to show the agent does not have a target
        }
        else robberNavMesh.SetDestination(targetObject.transform.position); // Move agent towards its target's current position
    }

    // Checks if the agent is a valid distance to any cop in the environment to use its power-up
    private void CheckDistance()
    {
        for (int i = 0; i < transform.parent.childCount; i++)               // Loop through all cops in the environment
        {
            GameObject child = transform.parent.GetChild(i).gameObject;
            if (child.CompareTag("Cop"))
            {
                // Check the distance between the agent and each cop
                if (child.GetComponent<Cop>().CheckDistance(child.transform.position, transform.position))
                {
                    validDistance = true;                                   // Update flag to show the agent is close enough to a cop
                    return;
                }
            }
        }
        validDistance = false;                                              // Update flag to show the agent is to far from every cop
    }

    // Completes an episode and resets the environment
    public void EndSession(bool victoryStatus)
    {
        if (victoryStatus)                                                  // Check if the agent succeeded this episode
        {
            AddReward(10f);                                                 // Reward agent for successfully completing episode
            if (evaluationMaterial != null) evaluationMaterial.color = Color.blue;
        }
        else                                                                // Check if the agent failed this episode
        {
            AddReward(-10f);                                                // Punish agent for failing this episode
            if (evaluationMaterial != null) evaluationMaterial.color = Color.yellow;
        }

        for (int i = 0; i < transform.parent.childCount; i++)               // Activate all diamonds in the environment
        {
            GameObject child = transform.parent.GetChild(i).gameObject;
            if (child.CompareTag("Diamond")) child.SetActive(true);
        }

        robber.spawnPoints.spawnPointsSet = false;                          // Reset available spawn points

        // End the episode appropriately, dependent on training or gameplay
        if (DowntownManager.dm != null) DowntownManager.dm.EndSession(false);
        else EndEpisode();
    }
}