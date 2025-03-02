using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelector : MonoBehaviour
{
    public GameObject parentGO;
    public GridManager GridManager;
    private int gridSize;
    // Start is called before the first frame update
    void Awake()
    {
        Button[] levelButtons = GetComponentsInChildren<Button>();
        foreach (Button button in levelButtons)
        {
            int levelIndex = int.Parse(button.name);
            string levelName = button.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text;
            button.onClick.AddListener(() => StartCoroutine(SelectLevel(levelIndex, levelName)));

        }
    }

    private void OnEnable()
    {
        StartCoroutine(EnableAfterDelay());
    }

    IEnumerator EnableAfterDelay() 
    { 
        yield return new WaitForSeconds(0.03f);
        Button[] levelButtons = GetComponentsInChildren<Button>();
        foreach (Button button in levelButtons)
        {
            int levelIndex = int.Parse(button.name);
            int gridsize = gridSize;

            int levelStatus = PlayerPrefs.GetInt($"LevelCompletionStatus{levelIndex}_{gridsize}");
            string levelTime = PlayerPrefs.GetString($"LevelTimeStatus{levelIndex}_{gridsize}");
            string filePath = $"Assets/LevelData/Level_{levelIndex}_{gridsize}.json";
            float progress = PlayerPrefs.GetFloat($"LevelProgression{levelIndex}_{gridsize}");

            if (levelStatus == 1)
            {
                button.transform.GetChild(2).gameObject.SetActive(true);
                button.transform.GetChild(3).gameObject.SetActive(true);
                button.transform.GetChild(3).GetComponent<TextMeshProUGUI>().text = levelTime;
                button.transform.GetChild(1).gameObject.SetActive(true);
                button.transform.GetChild(1).GetChild(0).GetChild(0).GetChild(0).GetComponent<Image>().fillAmount = 1;

            }
            else
            {
                button.transform.GetChild(2).gameObject.SetActive(false);
                button.transform.GetChild(3).gameObject.SetActive(false);
                button.transform.GetChild(3).GetComponent<TextMeshProUGUI>().text = "";
                if (File.Exists(filePath))
                {
                    button.transform.GetChild(1).gameObject.SetActive(true);
                    button.transform.GetChild(1).GetChild(0).GetChild(0).GetChild(0).GetComponent<Image>().fillAmount = progress;
                }
                else
                {
                    button.transform.GetChild(1).gameObject.SetActive(false);
                    button.transform.GetChild(1).GetChild(0).GetChild(0).GetChild(0).GetComponent<Image>().fillAmount = 0;
                }

            }
        }
    }

    IEnumerator SelectLevel(int levelIndex, string levelName)
    {
        yield return new WaitForSeconds(0.2f);
        GridManager.UpdateGridForLevel(levelIndex, levelName);
    }

    public void ClearLevel()
    {
        for (int i = parentGO.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(parentGO.transform.GetChild(i).gameObject);
        }
        GridManager.SaveTypedLettersForLevel(GridManager.levelIndex, GridManager.gridSize);
    }

    //public void SelectedLevelSizeButton(int levelSize)
    //{
    //    StartCoroutine(CallMethodAfterDelay(0.01f, levelSize));
    //}
    //IEnumerator CallMethodAfterDelay(float delay, int levelSize)
    //{
    //    // Wait for the specified delay
    //    yield return new WaitForSeconds(delay);

    //    // Call your method after the delay
    //    SelectedLevelSize(levelSize);
    //}
    //void SelectedLevelSize(int levelSize)
    //{
    //    if (levelSize == 0)
    //    {
    //        this.transform.GetChild(0).gameObject.SetActive(true);
    //        this.transform.GetChild(1).gameObject.SetActive(false);
    //        this.transform.GetChild(2).gameObject.SetActive(false);
    //    }
    //    else if (levelSize == 1)
    //    {
    //        this.transform.GetChild(0).gameObject.SetActive(false);
    //        this.transform.GetChild(1).gameObject.SetActive(true);
    //        this.transform.GetChild(2).gameObject.SetActive(false);
    //    }
    //    else if (levelSize == 2)
    //    {
    //        this.transform.GetChild(0).gameObject.SetActive(false);
    //        this.transform.GetChild(1).gameObject.SetActive(false);
    //        this.transform.GetChild(2).gameObject.SetActive(true);
    //    }
    //}
    public void ChangeGridSize(int selectedGridSize)
    {
        gridSize = selectedGridSize;
    }
}
