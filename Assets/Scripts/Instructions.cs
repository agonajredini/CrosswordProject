using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Instructions : MonoBehaviour
{
    public GameObject clue;
    public GameObject clueText;
    public GameObject right;
    public GameObject left;
    public GameObject rightLeftText;
    public GameObject help;
    public GameObject helpText;
    public GameObject transparent;
    public TextMeshProUGUI buttonText;


    private int instructionIndex = 0;

    public void NextInstruction()
    {
        instructionIndex++;
        if (instructionIndex == 1)
        {
            clue.SetActive(false);
            clueText.SetActive(false);
            right.SetActive(true);
            left.SetActive(true);
            rightLeftText.SetActive(true);
        }
        else if (instructionIndex == 2)
        {
            right.SetActive(false);
            left.SetActive(false);
            rightLeftText.SetActive(false);
            help.SetActive(true);
            helpText.SetActive(true);
            buttonText.text = "DONE";
        }
        else if (instructionIndex == 3)
        {
            helpText.SetActive(false);
            help.SetActive(false);
            clue.SetActive(true);
            clueText.SetActive(true);
            buttonText.text = "NEXT";
            instructionIndex = 0;
            transparent.SetActive(false);
            this.gameObject.SetActive(false);
        }
    }
}
