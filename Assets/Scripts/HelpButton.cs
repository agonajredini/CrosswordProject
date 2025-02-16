using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HelpButton : MonoBehaviour
{
    public GameObject helpPanel;
    public float boxMoveSpeed = 0.3f;
    public LeanTweenType boxEaseType = LeanTweenType.easeInSine;

    private bool isHelpPanelActive = false;

    private Vector2 helpPanelInitialPos;

    private void Start()
    {
        helpPanelInitialPos = helpPanel.GetComponent<RectTransform>().anchoredPosition;
    }
    public void ShowHelpPanel()
    {
        isHelpPanelActive = !isHelpPanelActive;

        if (isHelpPanelActive)
        {
            helpPanel.SetActive(true);
            LeanTween.moveY(helpPanel.GetComponent<RectTransform>(), 108, boxMoveSpeed).setEase(boxEaseType);

            gameObject.GetComponent<Image>().color = new Color32(255, 255, 255, 255); // Set color to white
            gameObject.GetComponentInChildren<TextMeshProUGUI>().color = new Color32(32, 32, 32, 255); // Set color to black
        }
        else
        {
            gameObject.GetComponent<Image>().color = new Color32(32, 32, 32, 255); // Set color to black
            gameObject.GetComponentInChildren<TextMeshProUGUI>().color = new Color32(255, 255, 255, 255); // Set color to white
            LeanTween.moveY(helpPanel.GetComponent<RectTransform>(), helpPanelInitialPos.y, boxMoveSpeed).setEase(boxEaseType).setOnComplete(() =>
            { 
                helpPanel.SetActive(false);

            });
        }
    }

    public void HelpPanelIfActive()
    {
        if (helpPanel.activeSelf)
            ShowHelpPanel();
    }
}
