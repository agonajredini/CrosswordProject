using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelector : MonoBehaviour
{
    public GameObject parentGO;
    public GridManager GridManager;
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
            int levelStatus = PlayerPrefs.GetInt($"LevelCompletionStatus{levelIndex}");
            string levelTime = PlayerPrefs.GetString($"LevelTimeStatus{levelIndex}");
            if (levelStatus == 1)
            {
                button.transform.GetChild(1).gameObject.SetActive(true);
                button.transform.GetChild(2).gameObject.SetActive(true);
                button.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = levelTime;
            }
            else
            {
                button.transform.GetChild(1).gameObject.SetActive(false);
                button.transform.GetChild(2).gameObject.SetActive(false);
                button.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = "";
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
        GridManager.SaveTypedLettersForLevel(GridManager.levelIndex);
    }
}
