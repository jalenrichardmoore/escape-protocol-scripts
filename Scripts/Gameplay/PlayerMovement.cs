using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // Component References
    [SerializeField] private Cop copPlayer;
    [SerializeField] private Robber robberPlayer;

    // Player Variables
    private Vector3 moveDirection;                                          // Direction that the player is moving
    private Quaternion targetRotation;                                      // Rotation that the player is performing
    
    private float rotationSpeed;                                            // Speed at which the player rotates
    
    // Flags
    private bool isRotating;                                                // Flag to check if the player is rotating

    private void Start()
    {
        // Initialize variables
        rotationSpeed = 5f;
        isRotating = false;

        if (this.CompareTag("Cop"))                                         // Check if the player is a cop
        {
            copPlayer.InitializeCop(Cop.TYPE.Player);                       // Initialize cop variables
            copPlayer.MoveObjectsIntoPosition();                            // Move cop and robbers into position
        }
        else if (this.CompareTag("Robber"))                                 // Check if the player is a robber
        {
            robberPlayer.InitializeRobber(Robber.TYPE.Player);              // Initialize robber variables
            robberPlayer.MoveObjectsIntoPosition();                         // Move diamonds and deposit zone into position
        }
    }

    private void Update()
    {
        // Check if the player is inputting a movement key, and set the corresponding 'targetRotation'
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) SetTargetRotation(new Vector3(-25, 0, 0), Vector3.forward);
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) SetTargetRotation(new Vector3(-25, 180, 0), Vector3.back);
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) SetTargetRotation(new Vector3(-25, 270, 0), Vector3.left);
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) SetTargetRotation(new Vector3(-25, 90, 0), Vector3.right);

        if (isRotating) RotateTowardsTarget();                              // Check if the player is currently rotating, and if so, rotate the target towards 'targetRotation'

        // Check if the player is inputting a movement key and is not rotating, and move the player in the corresponding direction
        if (!isRotating && (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) 
                          || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow) 
                          || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow) 
                          || Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))) MovePlayer();

        if (Input.GetKeyDown(KeyCode.E))                                    // Check if the player is attempting to activate a power-up
        {
            // Activate the appropriate power-up, dependent on player role
            if (this.CompareTag("Cop")) copPlayer.ActivatePowerUp();
            else if (this.CompareTag("Robber")) robberPlayer.ActivatePowerUp();
        }

        if (Input.GetKey(KeyCode.Space))                                    // Check if the player is inputting the 'Space' key
        {
            if (this.CompareTag("Cop")) copPlayer.TagRobber();              // Activate the appropriate action
            else if (this.CompareTag("Robber"))
            {
                // Check if the player can collect/deposit a diamond, and perform the appropriate action
                if (robberPlayer.canCollectDiamond) robberPlayer.MoveDiamond(robberPlayer.diamondToCollect, "Collect");
                else if (robberPlayer.canDepositDiamond) robberPlayer.MoveDiamond(robberPlayer.diamondToDeposit, "Deposit");
            }
        }

        if (Input.GetKey(KeyCode.Return))                                   // Check if the player is attempting to complete their objective
        {
            // End the game session if the player has completed their role's objective
            if ((this.CompareTag("Cop") && copPlayer.canWinGame) || (this.CompareTag("Robber") && robberPlayer.canWinGame)) DowntownManager.dm.EndSession(true);
        }
    }

    // Sets the player's current rotation to face a new direction
    private void SetTargetRotation(Vector3 rotation, Vector3 direction) 
    {
        if (this.CompareTag("Robber") && robberPlayer.isHiding) return;

        targetRotation = Quaternion.Euler(rotation);                        // Rotates the player's current rotation to face 'targetRotation'
        isRotating = true;                                                  // Change flag to show that the player is rotating
        moveDirection = direction;                                          // Set the new movement direction to be in the direction of the new rotation
    }

    // Rotates the player's current rotation to face 'targetRotation'
    private void RotateTowardsTarget() 
    {
        if (this.CompareTag("Robber") && robberPlayer.isHiding) return;

        // Change rotation towards 'targetRotation'
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        if (Quaternion.Angle(transform.rotation, targetRotation) < 0.1f)    // Checks if the player has finished rotating
        {
            transform.rotation = targetRotation;                            // Set current rotation to 'targetRotation'
            isRotating = false;                                             // Change flag to show that the player is no longer rotating
        }
    }

    // Moves the player in the direction of 'moveDirection'
    private void MovePlayer()
    {
        if (this.CompareTag("Robber") && robberPlayer.isHiding) return;     // Check if the player is a hiding robber and stop moving

        float speed = 0f;                                                   // Initialize speed of player

        // Retrive appropriate movement speed, dependent on player role
        if (this.CompareTag("Cop")) speed = copPlayer.movementSpeed;
        else if (this.CompareTag("Robber")) speed = robberPlayer.movementSpeed;

        // Move the player in the appropriate direction
        transform.Translate(moveDirection * speed * Time.deltaTime, Space.World);
    }
}