using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    public GameObject timerDisplay;
    public GameObject nameDisplay;

    public bool showTimer = true;
    public bool showName = true;

    private void Start()
    {
        showTimer = PlayerPrefs.GetInt("ShowTimer", 0) == 1;
        showName = PlayerPrefs.GetInt("ShowName", 0) == 1;
        ShowTimer();
        ShowName();
    }
    public void ShowTimerPrefs()
    {
        PlayerPrefs.SetInt("ShowTimer", showTimer ? 0 : 1);
        PlayerPrefs.Save();
        showTimer = !showTimer;
    }

    public void ShowTimer()
    {
        if(showTimer)
        {
            timerDisplay.SetActive(true);
        }
        else
        {
            timerDisplay.SetActive(false);
        }
    }

    public void ShowNamePrefs()
    {
        PlayerPrefs.SetInt("ShowName", showName? 0 : 1);
        PlayerPrefs.Save();
        showName = !showName;
    }

    public void ShowName()
    {
        if (showName)
        {
            nameDisplay.SetActive(true);
        }
        else
        {
            nameDisplay.SetActive(false);
        }
    }
}
