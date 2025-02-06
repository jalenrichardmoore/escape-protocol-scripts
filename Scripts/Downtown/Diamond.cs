using UnityEngine;

public class Diamond : MonoBehaviour
{
    [HideInInspector] public Vector3 spawnPointPosition;                    // position of the diamond's spawn point

    // Animation Variables
    private Vector3 startPosition;
    private Vector3 upperBound;
    private Vector3 lowerBound;

    [SerializeField] private float rotationSpeed;                           // Speed at which the diamond rotates

    // Flags
    private bool movingUp;                                                  // Flag that checks which direction the diamond is animating
    public bool playAnimation;                                              // Flag that checks if the diamond is currently animating
    public bool isPositioned;                                               // Flag that checks if the diamond has moved to a spawn point

    private void Start()
    {
        // Initialize flags
        movingUp = false;
        isPositioned = false;
        playAnimation = false;
    }

    private void Update()
    {
        // Rotate the diamond around its y-axis
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime, Space.World);

        if (playAnimation)                                                  // Check if the diamond is currently animating
        {
            if (movingUp)                                                   // Check if the diamond is animating towards its upper bound
            {
                // Move the diamond towards its upper bound
                transform.position = Vector3.MoveTowards(transform.position, upperBound, 0.02f);

                // Check if the diamond reached the upper bound and update flag
                if (Vector3.Distance(transform.position, upperBound) < 0.01f) movingUp = false;
            }
            else                                                            // Check if the diamond is animating towards its lower bound
            {
                // Move the diamond towards its lower bound
                transform.position = Vector3.MoveTowards(transform.position, lowerBound, 0.02f);

                // Check if the diamond reached the lower bound and update flag
                if (Vector3.Distance(transform.position, lowerBound) < 0.01f) movingUp = true;
            }
        }
    }

    // Initializes the diamond's bounds and begins animating
    public void StartAnimation()
    {
        // Initialize bounds
        startPosition = transform.position;
        upperBound = startPosition + Vector3.up;
        lowerBound = startPosition + Vector3.down;

        // Initialize flags
        isPositioned = true;
        movingUp = true;
        playAnimation = true;
    }
}