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
    private int gridSize=0;
    // Start is called before the first frame update
    void Start()
    {
        Button[] levelButtons = GetComponentsInChildren<Button>();
        foreach (Button button in levelButtons)
        {
            int levelIndex = int.Parse(button.name);
            button.onClick.AddListener(() => SelectLevel(levelIndex));

        }
    }

    private void OnEnable()
    {
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

    void SelectLevel(int levelIndex)
    {
        GridManager.UpdateGridForLevel(levelIndex);
    }

    public void ClearLevel()
    {
        for (int i = parentGO.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(parentGO.transform.GetChild(i).gameObject);
        }
        GridManager.SaveTypedLettersForLevel(GridManager.levelIndex, GridManager.gridSize);
    }

    public void SelectedLevelSizeButton(int levelSize)
    {
        StartCoroutine(CallMethodAfterDelay(0.01f, levelSize));
    }
    IEnumerator CallMethodAfterDelay(float delay, int levelSize)
    {
        // Wait for the specified delay
        yield return new WaitForSeconds(delay);

        // Call your method after the delay
        SelectedLevelSize(levelSize);
    }
    void SelectedLevelSize(int levelSize)
    {
        gridSize = levelSize;
        if (levelSize == 0)
        {
            this.transform.GetChild(0).gameObject.SetActive(true);
            this.transform.GetChild(1).gameObject.SetActive(false);
            this.transform.GetChild(2).gameObject.SetActive(false);
        }
        else if (levelSize == 1)
        {
            this.transform.GetChild(0).gameObject.SetActive(false);
            this.transform.GetChild(1).gameObject.SetActive(true);
            this.transform.GetChild(2).gameObject.SetActive(false);
        }
        else if (levelSize == 2)
        {
            this.transform.GetChild(0).gameObject.SetActive(false);
            this.transform.GetChild(1).gameObject.SetActive(false);
            this.transform.GetChild(2).gameObject.SetActive(true);
        }
    }
}
