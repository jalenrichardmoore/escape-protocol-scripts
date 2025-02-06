using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class RoleInfo : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // UI References
    [SerializeField] private GameObject roleInfo;
    [SerializeField] private TMP_Text roleInfoText;

    public void Start()
    {
        roleInfo.SetActive(false);                                          // Deactivate the player role info textbox
    }

    public void OnPointerEnter(PointerEventData pointer)
    {
        if (this.CompareTag("Cop"))                                         // Checks if the cursor is hovering over the 'Select Cop' button
        {
            roleInfo.SetActive(true);                                       // Activate the player role info textbox

            // Update the text to display the cop's role
            roleInfoText.text = "Your job is to tag all robbers before they steal the diamonds and escape.\n" +
                                "Press the 'Space' key to tag a robber in close proximity. \n" +
                                "Press the 'E' key to temporarily increase speed.";
        }
        else if (this.CompareTag("Robber"))                                 // Check if the cursor is hovering over the 'Select Robber' button
        {
            roleInfo.SetActive(true);                                       // Activate the player role info textbox

            // Update the text to display the robber's role
            roleInfoText.text = "Your job is to collect all the diamonds and return them to the deposit zone.\n" +
                                "Press the 'Space' key to collect a diamond and deposit it within the deposit zone.\n" +
                                "Press the 'E' key to temporarily evade capture from cops.";            
        }
    }

    public void OnPointerExit(PointerEventData pointer)
    {
        roleInfo.SetActive(false);                                          // Deactivate the player role info textbox
    }
}
