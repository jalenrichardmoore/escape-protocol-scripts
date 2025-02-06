using UnityEngine;
using UnityEngine.AI;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class CopController : Agent
{
    // Agent Variables
    [SerializeField] private NavMeshAgent copNavMesh;
    [SerializeField] private Cop cop;
    private Vector3 spawnPointPosition;

    // Target Variables
    [SerializeField] private GameObject targetObject;
    private Vector3 targetDestination;
    private bool targetSet;

    // Environment Variables
    [SerializeField] private GameObject environment;
    private Material evaluationMaterial;

    public override void Initialize()
    {
        // Initialize variables
        if (environment != null) evaluationMaterial = environment.transform.GetChild(0).GetComponent<Renderer>().material;
        else environment = GameObject.FindGameObjectWithTag("Downtown");
        cop.InitializeCop(Cop.TYPE.Agent);
    }

    public override void OnEpisodeBegin()
    {
        // Initialize variables for current episode
        cop.InitializeCop(Cop.TYPE.Agent);
        copNavMesh.speed = cop.movementSpeed;

        cop.MoveObjectsIntoPosition();                                      // Move the cop and robbers into position
        spawnPointPosition = transform.position;                            // Initialize the cop's spawn point position for current episode

        // Initialize target variables
        targetObject = gameObject;
        targetDestination = transform.position;
        targetSet = false;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(cop.canWinGame);                              // Collect info on if the agent has completed its objective
        sensor.AddObservation(targetSet);                                   // Collect info on if the agent has set a target
        sensor.AddObservation(cop.powerUp == null);                         // Collect info on if the agent's power-up is active
        sensor.AddObservation(Time.time > cop.timeUntilPowerUp);            // Collect info on if the agent can activate its power-up again

        // Collect info on if the agent is close enough to its target
        sensor.AddObservation(cop.CheckDistance(transform.position, targetDestination));
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
        if (moveToTarget)                                                   // Check if the agent is moving toward a robber
        {
            if (!targetSet)                                                 // Check if the agent currently does not have a target
            {
                if (cop.robbers.Count > 0)                                  // Check if there are any robbers left to target, and target a random robber
                {
                    targetObject = cop.robbers[Random.Range(0, cop.robbers.Count)];
                    targetDestination = targetObject.transform.position;
                }

                copNavMesh.SetDestination(targetDestination);               // Move the agent towards its target
                targetSet = true;                                           // Update flag to show the agent has a target

                AddReward(3f);                                              // Reward the agent for setting a target
                if (evaluationMaterial != null) evaluationMaterial.color = Color.green;
            }
            else                                                            // Check if the agent already has a target
            {
                AddReward(-1f);                                             // Punish the robber for trying to set a second target
                if (evaluationMaterial != null) evaluationMaterial.color = Color.red;
            }
        }

        bool activatePowerUp = actions.DiscreteActions[1] > 0;              // Receive input action for activating power-up
        if (activatePowerUp)                                                // Check if the agent is attempting to activate a power-up
        {
            if (cop.ActivatePowerUp())                                      // Check if the agent successfully activated its power-up
            {
                AddReward(2f);                                              // Reward agent for successfully activating its power-up
                if (evaluationMaterial != null) evaluationMaterial.color = Color.green;
            }
            else                                                            // Check if the agent failed to activate its power-up
            {
                AddReward(-1f);                                             // Punish the agent for failing to activate its power-up
                if (evaluationMaterial != null) evaluationMaterial.color = Color.red;
            }
        }

        bool tagRobber = actions.DiscreteActions[2] > 0;                    // Receive input action for tagging a robber
        if (tagRobber)                                                      // Check if the agent is attempting to tag a robber
        {
            if (cop.TagRobber())                                            // Check if the agent successfully tagged a robber
            {
                targetObject = gameObject;                                  // Update target to be the agent
                targetSet = false;                                          // Update flag to show the agent is not targetting a robber

                AddReward(5f);                                              // Reward agent for successfully tagging a robber
                if (evaluationMaterial != null) evaluationMaterial.color = Color.green;
            }
            else                                                            // Check if the agent failed to tag a robber
            {
                AddReward(-2f);                                             // Punish the agent for failing to tag a robber
                if (evaluationMaterial != null) evaluationMaterial.color = Color.red;
            }
        }

        bool completeObjective = actions.DiscreteActions[3] > 0;            // Receive input action for completing objective
        if (completeObjective)                                              // Check if the agent is attempting to complete its objective
        {
            if (cop.canWinGame)                                             // Check if the agent has completed its objective
            {
                for (int i = 0; i < transform.parent.childCount; i++)       // End the current session for all agents in the environment
                {
                    GameObject child = transform.parent.GetChild(i).gameObject;
                    if (child.TryGetComponent<RobberController>(out RobberController robberAgent)) robberAgent.EndSession(false);
                    else if (child.TryGetComponent<CopController>(out CopController copAgent)) copAgent.EndSession(true);
                }
            }
            else                                                            // Check if the agent has not completed its objective
            {
                AddReward(-3f);                                             // Punish the agent for trying to end the session early
                if (evaluationMaterial != null) evaluationMaterial.color = Color.red;
            }
        }
    }

    private void Update()
    {
        if (targetObject == null) return;                                   // Check if the agent does not have a target and return

        // Check if the agent has a target that is hiding and set destination to its spawn point
        if (targetObject.CompareTag("Robber") && targetObject.GetComponent<Robber>().isHiding) 
        {
            copNavMesh.SetDestination(spawnPointPosition);
        }
        else if (!cop.robbers.Contains(targetObject))                       // Check if the agent's target has been tagged
        {
            targetObject = null;                                            // Update target to show the agent has no target
            targetDestination = transform.position;                         // Update target destination to be the agent
            targetSet = false;                                              // Update flag to show the agent does not have a target
            copNavMesh.SetDestination(spawnPointPosition);                  // Set destination to agent's spawn point
        }
        else
        {
            targetDestination = targetObject.transform.position;            // Update target destination to target's current position
            copNavMesh.SetDestination(targetDestination);                   // Set target destination to the agent's target
        }
    }

    // Completes an episode and resets the environment
    public void EndSession(bool victoryStatus)
    {
        if (victoryStatus)                                                  // Check if the agent succeeded this episode
        {
            AddReward(10f);                                                 // Reward agent for successfully completing episode
            if (evaluationMaterial != null) evaluationMaterial.color = Color.green;
        }
        else                                                                // Check if the agent failed this episode
        {
            AddReward(-10f);                                                // Punish agent for failing this episode
            if (evaluationMaterial != null) evaluationMaterial.color = Color.red;
        }

        for (int i = 0; i < transform.parent.childCount; i++)               // Reactivate all robbers in the environment
        {
            GameObject child = transform.parent.GetChild(i).gameObject;
            if (child.CompareTag("Robber"))
            {
                child.SetActive(true);                                      // Activate the robber

                // Turn on the NavMeshAgent for all robber agents
                if (child.GetComponent<Robber>().robberType == Robber.TYPE.Agent) child.GetComponent<NavMeshAgent>().isStopped = false;
            
                // Check if the robber has a diamond and make it a child of the environment
                if (child.transform.childCount == 3) child.transform.GetChild(2).parent = transform.parent;
            
                // Activate all diamonds
                foreach (GameObject diamond in child.GetComponent<Robber>().diamonds) diamond.SetActive(true);
            }
        }

        cop.spawnPoints.spawnPointsSet = false;                             // Reset available spawn points

        // End the episode appropriately, dependent on training or gameplay
        if (DowntownManager.dm != null) DowntownManager.dm.EndSession(false);
        else EndEpisode();
    }
}