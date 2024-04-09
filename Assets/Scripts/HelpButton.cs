using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HelpButton : MonoBehaviour
{
    public GameObject helpPanel;
    private bool isHelpPanelActive = false;

    public void ShowHelpPanel()
    {
        isHelpPanelActive = !isHelpPanelActive;

        if (isHelpPanelActive)
        {
            helpPanel.SetActive(true);
            gameObject.GetComponent<Image>().color = new Color32(255, 255, 255, 255); // Set color to white
            gameObject.GetComponentInChildren<TextMeshProUGUI>().color = new Color32(32, 32, 32, 255); // Set color to black
        }
        else
        {
            gameObject.GetComponent<Image>().color = new Color32(32, 32, 32, 255); // Set color to black
            gameObject.GetComponentInChildren<TextMeshProUGUI>().color = new Color32(255, 255, 255, 255); // Set color to white
            helpPanel.SetActive(false);
        }
    }

    public void HelpPanelIfActive()
    {
        if (helpPanel.activeSelf)
            ShowHelpPanel();
    }
}
