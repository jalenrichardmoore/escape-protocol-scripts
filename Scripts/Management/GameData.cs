public static class GameData
{   
    public static int currentModelIndex = 5;                                // Current AI model used by enemies
    public static int playerRole = 0;                                       // Player's chosen role (Cop/Robber)
    public static int numDiamonds = 2;                                      // Current number of diamonds in session
    public static int numCopAgents = 2;                                     // Current number of cop agents created in session
    public static int numRobberAgents = 3;                                  // Current number of robber agents created in session
    public static int numRobbersTagged = 0;                                 // Number of robbers tagged in current session
    public static int numDiamondsCollected = 0;                             // Number of diamonds collected in current session

    public static float sessionTime = 0f;                                   // Length, in seconds, of the current game session
    public static float successState = 0f;                                  // Player's victory status in current session (Win/Loss)
    public static float percentageRobbersTagged = 0f;                       // Percentage of robbers tagged in current session
    public static float timesSpedUp = 0f;                                   // Number of times cops sped up during current session
    public static float percentageDiamondsCollected = 0f;                   // Percentage of diamonds collected in current session
    public static float timesHidden = 0f;                                   // Number of times robbers hid during current session

    public static void ResetSessionData()
    {
        // Reset variables to initial values
        numRobbersTagged = 0;
        numDiamondsCollected = 0;

        sessionTime = 0f;
        successState = 0f;
        percentageRobbersTagged = 0f;
        timesSpedUp = 0f;
        percentageDiamondsCollected = 0f;
        timesHidden = 0f;
    }
}