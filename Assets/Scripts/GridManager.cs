using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using TMPro;
using System;
using System.Linq;
using Newtonsoft.Json;
using System.IO;
using UnityEngine.EventSystems;

public class GridManager : MonoBehaviour
{
    [Header("Ads Manager")]
    public AdsManager adsManager;

    [SerializeField] private Tile tilePrefab;

    [SerializeField] private Color tilecolor;

    [SerializeField] private Color darkModeColor;
    public bool darkMode = false;
    public bool skipFilled = true;
    public bool deletePrevious = false;

    public bool horizontal = true;

    private bool youWin = false;
    public HelpButton helpButton;
    public GameObject youWinUI;
    public GameObject fullNoWinUI;
    public GameObject transparent;
    public TextMeshProUGUI showTimerWin;

    private bool hasBeenShowed = false;

    private int currentIndex = 0;

    public GameObject parentGO;
    public GameObject levels;

    public TextMeshProUGUI clueDisplayText;
    public TextMeshProUGUI clueDisplayNumber;
    public TextMeshProUGUI timerDisplay;
    public TextMeshProUGUI nameDisplay;

    public bool playAudio = true;
    public AudioSource audioSrc;
    public AudioClip clickSrc, deleteSrc, winSrc, notWon, reveal;


    public Camera cam;

    private float timer = 0.0f;

    private Dictionary<Vector2, Tile> tiles;

    public TextAsset puzzlesJsonFile;
    public TextAsset puzzlesJsonFileMini;
    public TextAsset puzzlesJsonFileMidi;
    private PuzzleData puzzlesData;
    private Dictionary<int, string> acrossClues;
    private Dictionary<int, string> downClues;


    private int tileNumber = 1;

    public int levelIndex = 1;
    private int gridLength;
    private int size;

    public int gridSize = 0; // 0 = 15x15, 1 = 9x9, 2 = 5x5

    public void UpdateGridForLevel(int newLevelIndex)
    {
        levelIndex = newLevelIndex;
        currentIndex = 0;
        tileNumber = 1;
        horizontal = true;
        youWin = false;
        hasBeenShowed = false;

        nameDisplay.text = $"Fjalëkryqi {levelIndex}";
        darkMode = PlayerPrefs.GetInt("DarkMode", 0) == 1;
        skipFilled = PlayerPrefs.GetInt("SkipFilled", 1) == 1;
        deletePrevious = PlayerPrefs.GetInt("DeletePrevious", 1) == 1;
        playAudio = PlayerPrefs.GetInt("PlayAudio", 1) == 1;

        // Reset the timer
        timer = 0;
        timerDisplay.text = "00:00:00"; // Reset the timer display

        GenerateGrid();
        LoadTypedLettersForLevel(levelIndex, gridSize);
        UpdateClue();
    }

    
    private void Update()
    {
        if( Input.GetMouseButtonDown(0) )
	    {
            if (EventSystem.current.IsPointerOverGameObject() && transparent.activeSelf) return;

            Ray ray = Camera.main.ScreenPointToRay( Input.mousePosition );
		    RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, Mathf.Infinity); ;
		
		    if( hit.collider !=null)
		    {
                if (hit.collider.gameObject.transform.GetSiblingIndex() == currentIndex)
                    horizontal = !horizontal;

                if(hit.collider.gameObject.transform.GetComponent<SpriteRenderer>().color != tilecolor )
                        currentIndex = hit.collider.gameObject.transform.GetSiblingIndex();
            }
            
            UpdateClue();

            
        }

        for (int i = 0; i < gridLength; i++)

        {
            if (i == currentIndex)
                parentGO.transform.GetChild(i).GetChild(3).gameObject.SetActive(true);
            else
                parentGO.transform.GetChild(i).GetChild(3).gameObject.SetActive(false);


        }

        HighlightWord();

        if (IsFull() && !youWin)
        {
            CheckWinCondition();

            bool wrongShown = PlayerPrefs.GetInt($"LevelWrongShownStatus{levelIndex}_{gridSize}") == 1;

            if (!wrongShown && !youWin)
            {
                fullNoWinUI.SetActive(true);
                fullNoWinUI.transform.localScale = Vector3.zero;
                LeanTween.scale(fullNoWinUI, Vector3.one, 0.3f).setEase(LeanTweenType.easeOutBack);
                transparent.SetActive(true);

                if(playAudio)
                    audioSrc.PlayOneShot(notWon);

                hasBeenShowed = true;
                PlayerPrefs.SetInt($"LevelWrongShownStatus{levelIndex}_{gridSize}", hasBeenShowed ? 1 : 0);
                PlayerPrefs.Save();

            }
        }


        // Update the timer display
        if (parentGO.transform.childCount > 0 && !youWin)
        {
            timer += Time.deltaTime;
            int hours = (int)(timer / 3600);
            int minutes = (int)((timer % 3600) / 60);
            int seconds = (int)(timer % 60);

            timerDisplay.text = hours.ToString("0") + ":" + minutes.ToString("00") + ":" + seconds.ToString("00");
        }
        

    }

    void GenerateGrid()
    {
        string jsonString;

        if (gridSize == 0)
            jsonString = puzzlesJsonFile.text;
        else if (gridSize == 1)
            jsonString = puzzlesJsonFileMidi.text;
        else
            jsonString = puzzlesJsonFileMini.text;

        acrossClues = new Dictionary<int, string>();
        downClues = new Dictionary<int, string>();

        puzzlesData = JsonConvert.DeserializeObject<PuzzleData>(jsonString);


        gridLength = puzzlesData.puzzles[levelIndex - 1].grid.Length;
        size = (int)Math.Sqrt(puzzlesData.puzzles[levelIndex - 1].grid.Length);

        if(size == 5)
        {
            cam.transform.position = new Vector3(2.04f, 0.12f, -10);
            cam.orthographicSize = 5.88f;
        }
        else if (size == 9)
        {
            cam.transform.position = new Vector3(4.05f, 0.76f, -10);
            cam.orthographicSize = 10f;
        }
        else
        {
            cam.transform.position = new Vector3(7, 2f, -10);
            cam.orthographicSize = 16f;
        }

        foreach (var pair in puzzlesData.puzzles[levelIndex - 1].clues.across)
        {
            acrossClues.Add(int.Parse(pair.Key), pair.Value);
        }

        foreach (var pair in puzzlesData.puzzles[levelIndex - 1].clues.down)
        {
            downClues.Add(int.Parse(pair.Key), pair.Value);
        }

        tiles = new Dictionary<Vector2, Tile>();
        for (int x = size-1; x >=0; x--)
        {
            for (int y = 0; y <=size-1; y++)
            {
                var spawnedTile = Instantiate(tilePrefab, new Vector3(y, x), Quaternion.identity, parentGO.transform);
                spawnedTile.name = $"Tile {x} {y}";

            }
        }
        for(int i =0; i<gridLength; i++)
        {

            parentGO.transform.GetChild(i).GetChild(0).GetComponent<TextMeshPro>().text = puzzlesData.puzzles[levelIndex - 1].answers[i].ToString();


            if (puzzlesData.puzzles[levelIndex - 1].grid[i].Equals('.'))
            {
                parentGO.transform.GetChild(i).GetComponent<SpriteRenderer>().color = tilecolor;
                parentGO.transform.GetChild(i).GetChild(0).GetComponent<TextMeshPro>().text = "";

            }


            


            if ((i-1 <= 0 || i-size <= 0) && parentGO.transform.GetChild(i).GetComponent<SpriteRenderer>().color != tilecolor)
            {

              parentGO.transform.GetChild(i).GetChild(1).GetComponent<TextMeshPro>().text = tileNumber.ToString();
              tileNumber++;

            }
            if(i>size+1)
            {
                if((parentGO.transform.GetChild(i-1).GetComponent<SpriteRenderer>().color == tilecolor || parentGO.transform.GetChild(i - size).GetComponent<SpriteRenderer>().color == tilecolor || parentGO.transform.GetChild(i - 1).position.y != parentGO.transform.GetChild(i).position.y) && parentGO.transform.GetChild(i).GetComponent<SpriteRenderer>().color != tilecolor)
                {
                    parentGO.transform.GetChild(i).GetChild(1).GetComponent<TextMeshPro>().text = tileNumber.ToString();
                    tileNumber++;
                }
            }
        }

        if (darkMode)
        {
            ChangeToDarkMode();
        }
    }


    public void TypeChar(string letter)
    {
        if(playAudio)
            audioSrc.PlayOneShot(clickSrc);

        if(parentGO.transform.GetChild(currentIndex).GetChild(2).GetComponent<TextMeshPro>().color == Color.red)
        {
            if(darkMode)
                parentGO.transform.GetChild(currentIndex).GetChild(2).GetComponent<TextMeshPro>().color = Color.white;
            else
                parentGO.transform.GetChild(currentIndex).GetChild(2).GetComponent<TextMeshPro>().color = Color.black;
        }

        if(youWin)
        {
            return;
        }

        if (horizontal)
        {
            if (currentIndex < gridLength)
            {
                if(parentGO.transform.GetChild(currentIndex).GetChild(2).gameObject.activeSelf)
                    parentGO.transform.GetChild(currentIndex).GetChild(2).GetComponent<TextMeshPro>().text = letter;
                
                currentIndex++;

                if (currentIndex > gridLength - 1)
                    currentIndex = 0;

                else
                {
                    if (skipFilled)
                    {
                        while (parentGO.transform.GetChild(currentIndex).GetComponent<SpriteRenderer>().color == tilecolor || parentGO.transform.GetChild(currentIndex).GetChild(2).GetComponent<TextMeshPro>().text != "" && !IsFull())
                        {
                            currentIndex++;
                            if (currentIndex > gridLength - 1)
                            {
                                currentIndex = 0;
                            }
                            //    currentIndex --;
                            //    NextClue();
                            //}
                            //if (parentGO.transform.GetChild(currentIndex).GetComponent<SpriteRenderer>().color == tilecolor)
                            //{
                            //    currentIndex --;
                            //    NextClue();
                            //}
                        }
                    }
                    else
                    {
                        while (parentGO.transform.GetChild(currentIndex).GetComponent<SpriteRenderer>().color == tilecolor)
                        {
                            currentIndex++;
                        }
                    }
                }

            }

        }

        else
        {
            if (currentIndex < gridLength)
            {
                if (parentGO.transform.GetChild(currentIndex).GetChild(2).gameObject.activeSelf)
                    parentGO.transform.GetChild(currentIndex).GetChild(2).GetComponent<TextMeshPro>().text = letter;

                if(!skipFilled)
                    currentIndex += size;

                if (currentIndex <= gridLength - 1)
                {
                    if (skipFilled)
                    {
                        while (parentGO.transform.GetChild(currentIndex).GetChild(2).GetComponent<TextMeshPro>().text != "" && !IsFull())
                        {
                            currentIndex += size;
                            if (currentIndex > gridLength - 1)
                            {
                                currentIndex -= size;
                                NextClue();
                            }
                            if (parentGO.transform.GetChild(currentIndex).GetComponent<SpriteRenderer>().color == tilecolor)
                            {
                                currentIndex -= size;
                                NextClue();
                            }
                        }
                    }
                    else
                    {
                        while (parentGO.transform.GetChild(currentIndex).GetComponent<SpriteRenderer>().color == tilecolor)
                        {
                            //currentIndex += size;
                            if (currentIndex > gridLength - 1)
                            {
                                currentIndex -= size;
                                NextClue();
                            }
                            if (parentGO.transform.GetChild(currentIndex).GetComponent<SpriteRenderer>().color == tilecolor)
                            {
                                currentIndex -= size;
                                NextClue();
                            }
                        }
                    }
                }


                if (currentIndex > gridLength - 1)
                {
                    currentIndex -= size;
                    NextClue();
                }
                
            }
        }
        UpdateClue();
        SaveTypedLettersForLevel(levelIndex, gridSize);
        SaveLevelProgression(levelIndex, gridSize);
        
    }

    public void Backspace()
    {
        if(playAudio)
            audioSrc.PlayOneShot(deleteSrc);

        if (youWin)
        {
            return;
        }

        if (horizontal)
        {
            int temp = currentIndex;
            if (currentIndex < gridLength)
            {
                if (deletePrevious && currentIndex>0 && parentGO.transform.GetChild(currentIndex).position.y == parentGO.transform.GetChild(currentIndex-1).position.y)
                {
                    if (parentGO.transform.GetChild(currentIndex).GetChild(2).gameObject.activeSelf && parentGO.transform.GetChild(currentIndex).GetChild(2).GetComponent<TextMeshPro>().text != "")
                        parentGO.transform.GetChild(currentIndex).GetChild(2).GetComponent<TextMeshPro>().text = "";
                    else if (parentGO.transform.GetChild(currentIndex - 1).GetChild(2).gameObject.activeSelf && parentGO.transform.GetChild(currentIndex).GetChild(2).GetComponent<TextMeshPro>().text == "")
                    {
                        parentGO.transform.GetChild(currentIndex - 1).GetChild(2).GetComponent<TextMeshPro>().text = "";

                        currentIndex--;
                    }
                    else
                    { 
                        currentIndex--;
                    }
                }

                else
                {
                    if (parentGO.transform.GetChild(currentIndex).GetChild(2).gameObject.activeSelf)
                        parentGO.transform.GetChild(currentIndex).GetChild(2).GetComponent<TextMeshPro>().text = "";

                    currentIndex--;
                }

                if (currentIndex < 0)
                    currentIndex = 0;

                else
                {
                    while (puzzlesData.puzzles[levelIndex - 1].grid[currentIndex] == '.' || parentGO.transform.GetChild(temp).position.y != parentGO.transform.GetChild(currentIndex).position.y)
                     currentIndex++;
                }

            }
        }
        else
        {
            if (currentIndex < gridLength)
            {
                if (deletePrevious && currentIndex > size)
                {
                    if (parentGO.transform.GetChild(currentIndex).GetChild(2).gameObject.activeSelf && parentGO.transform.GetChild(currentIndex).GetChild(2).GetComponent<TextMeshPro>().text != "")
                        parentGO.transform.GetChild(currentIndex).GetChild(2).GetComponent<TextMeshPro>().text = "";
                    else if (parentGO.transform.GetChild(currentIndex - size).GetChild(2).gameObject.activeSelf && parentGO.transform.GetChild(currentIndex).GetChild(2).GetComponent<TextMeshPro>().text == "")
                    {
                        parentGO.transform.GetChild(currentIndex - size).GetChild(2).GetComponent<TextMeshPro>().text = "";

                    }
                    currentIndex -= size;

                }

                else
                {
                    if (parentGO.transform.GetChild(currentIndex).GetChild(2).gameObject.activeSelf)
                    parentGO.transform.GetChild(currentIndex).GetChild(2).GetComponent<TextMeshPro>().text = "";

                    currentIndex -= size;
                    
                }
                

                if (currentIndex < 0)
                {
                    currentIndex += size;
                }

                if (puzzlesData.puzzles[levelIndex - 1].grid[currentIndex] == '.')
                {
                    currentIndex += size;
                }
            }
        }
        UpdateClue();
        SaveTypedLettersForLevel(levelIndex, gridSize);
        SaveLevelProgression(levelIndex, gridSize);
    }

    public void HighlightWord()
    {
        ClearWordHighlights();
        if (horizontal)
        {
            int leftIndex = currentIndex;
            int rightIndex = currentIndex;

            while (parentGO.transform.GetChild(leftIndex).GetComponent<SpriteRenderer>().color != tilecolor)
            {
                leftIndex--;
                if (leftIndex < 0)
                    break;

                if (parentGO.transform.GetChild(leftIndex).GetComponent<SpriteRenderer>().color != tilecolor && parentGO.transform.GetChild(leftIndex).position.y == parentGO.transform.GetChild(currentIndex).position.y)
                {
                    parentGO.transform.GetChild(leftIndex).GetChild(4).gameObject.SetActive(true);

                }

            }

            while (parentGO.transform.GetChild(rightIndex).GetComponent<SpriteRenderer>().color != tilecolor)
            {
             
                rightIndex++;
                if (rightIndex > gridLength - 1)
                    break;


                if (parentGO.transform.GetChild(rightIndex).GetComponent<SpriteRenderer>().color != tilecolor && parentGO.transform.GetChild(rightIndex).position.y == parentGO.transform.GetChild(currentIndex).position.y)
                {
                    parentGO.transform.GetChild(rightIndex).GetChild(4).gameObject.SetActive(true);

                }
            }

        }
        else
        {
            int upIndex = currentIndex;
            int downIndex = currentIndex;

            while (parentGO.transform.GetChild(upIndex).GetComponent<SpriteRenderer>().color != tilecolor)
            {
                upIndex -= size;
                if (upIndex < 0)
                    break;

                if (parentGO.transform.GetChild(upIndex).GetComponent<SpriteRenderer>().color != tilecolor && parentGO.transform.GetChild(upIndex).position.x == parentGO.transform.GetChild(currentIndex).position.x)
                {
                    parentGO.transform.GetChild(upIndex).GetChild(4).gameObject.SetActive(true);

                }


            }

            while (parentGO.transform.GetChild(downIndex).GetComponent<SpriteRenderer>().color != tilecolor)
            {

                downIndex += size;
                if (downIndex > gridLength - 1)
                    break;


                if (parentGO.transform.GetChild(downIndex).GetComponent<SpriteRenderer>().color != tilecolor && parentGO.transform.GetChild(downIndex).position.x == parentGO.transform.GetChild(currentIndex).position.x)
                {
                    parentGO.transform.GetChild(downIndex).GetChild(4).gameObject.SetActive(true);

                }

            }
        }
    }

    public void ClearWordHighlights()
    {
        for (int i = 0; i < gridLength; i++)
        {
            parentGO.transform.GetChild(i).GetChild(4).gameObject.SetActive(false);

        }
    }
    public void RevealWord()
    {
        if(playAudio)
            audioSrc.PlayOneShot(reveal);

        for (int i = 0; i < gridLength; i++)
        {
            if(parentGO.transform.GetChild(i).GetChild(4).gameObject.activeSelf || parentGO.transform.GetChild(i).GetChild(3).gameObject.activeSelf)
            {
                parentGO.transform.GetChild(i).GetChild(2).gameObject.GetComponent<TextMeshPro>().text = parentGO.transform.GetChild(i).GetChild(0).GetComponent<TextMeshPro>().text;
                parentGO.transform.GetChild(i).GetChild(0).gameObject.SetActive(true);
                parentGO.transform.GetChild(i).GetChild(2).gameObject.SetActive(false);
                parentGO.transform.GetChild(i).GetChild(5).gameObject.SetActive(true);

            }

        }

        SaveTypedLettersForLevel(levelIndex, gridSize);
        SaveLevelProgression(levelIndex, gridSize);
    }

    public void RevealAll()
    {
            for (int i = 0; i < gridLength; i++)
            {
                parentGO.transform.GetChild(i).GetChild(2).gameObject.GetComponent<TextMeshPro>().text = parentGO.transform.GetChild(i).GetChild(0).GetComponent<TextMeshPro>().text;
                parentGO.transform.GetChild(i).GetChild(0).gameObject.SetActive(true);
                parentGO.transform.GetChild(i).GetChild(2).gameObject.SetActive(false);
                parentGO.transform.GetChild(i).GetChild(5).gameObject.SetActive(true);


            }

    }

    public void RevealLetter()
    {
        if(playAudio)
            audioSrc.PlayOneShot(reveal);
        parentGO.transform.GetChild(currentIndex).GetChild(2).gameObject.GetComponent<TextMeshPro>().text = parentGO.transform.GetChild(currentIndex).GetChild(0).GetComponent<TextMeshPro>().text;
        parentGO.transform.GetChild(currentIndex).GetChild(0).gameObject.SetActive(true);
        parentGO.transform.GetChild(currentIndex).GetChild(5).gameObject.SetActive(true);
        parentGO.transform.GetChild(currentIndex).GetChild(2).gameObject.SetActive(false);

        SaveTypedLettersForLevel(levelIndex, gridSize);
        SaveLevelProgression(levelIndex, gridSize);

    }

    [Serializable]
    public class PuzzleData
    {
        public Puzzle[] puzzles;
    }

    [Serializable]
    public class Puzzle
    {
        public string grid;
        public string answers;
        public Clues clues;
    }

    [Serializable]
    public class Clues
    {
        public Dictionary<string, string> across;
        public Dictionary<string, string> down;

    }
    [Serializable]
    public class TileData
    {
        public int tileIndex;
        public string letter;
        public bool isRevealed;
        public float savedTimer;
        public string letterColor;

    }

    void UpdateClue()
    {
        string clueA;
        string clueD;

        if (horizontal)
        {
            int leftIndex = currentIndex;

            while (parentGO.transform.GetChild(leftIndex).GetComponent<SpriteRenderer>().color != tilecolor && parentGO.transform.GetChild(leftIndex).position.y == parentGO.transform.GetChild(currentIndex).position.y)
            {
                leftIndex--;
                if (leftIndex < 0)
                    break;
            }

            int numberTile = Int32.Parse(parentGO.transform.GetChild(leftIndex+1).GetChild(1).GetComponent<TextMeshPro>().text);
            if (acrossClues.ContainsKey(numberTile))
            {
                clueA = acrossClues[numberTile];
                clueDisplayNumber.text = numberTile.ToString();
                clueDisplayText.text = clueA;
            }
        }
        else
        {
            int upIndex = currentIndex;

            while (parentGO.transform.GetChild(upIndex).GetComponent<SpriteRenderer>().color != tilecolor && parentGO.transform.GetChild(upIndex).position.x == parentGO.transform.GetChild(currentIndex).position.x)
            {
                upIndex-=size;
                if (upIndex < 0)
                    break;
            }

            int numberTile = Int32.Parse(parentGO.transform.GetChild(upIndex + size).GetChild(1).GetComponent<TextMeshPro>().text);
            if (downClues.ContainsKey(numberTile))
            {
                clueD = downClues[numberTile];
                clueDisplayNumber.text = numberTile.ToString();
                clueDisplayText.text = clueD;

            }
        }
    }

    public void ChangeDirection()
    {
        horizontal = !horizontal;
        UpdateClue();
    }

    public void NextClue()
    {
        if (horizontal)
        {
            int leftIndex = currentIndex;
            int temp=0;

            while (parentGO.transform.GetChild(leftIndex).GetComponent<SpriteRenderer>().color != tilecolor && parentGO.transform.GetChild(leftIndex).position.y == parentGO.transform.GetChild(currentIndex).position.y)
            {
                leftIndex--;
                if (leftIndex < 0)
                    break;
            }

            int numberTile = Int32.Parse(parentGO.transform.GetChild(leftIndex + 1).GetChild(1).GetComponent<TextMeshPro>().text);
            numberTile++;
            while (!acrossClues.ContainsKey(numberTile))
            {
                if (numberTile > 100)
                    break;

                numberTile++;
            }

            for(int i=0; i<gridLength; i++)
            {
                if(parentGO.transform.GetChild(i).GetChild(1).GetComponent<TextMeshPro>().text == numberTile.ToString())
                {
                    temp = i;
                }
            }
            currentIndex = temp;
        }

        else
        {
            int upIndex = currentIndex;
            int temp = 0;

            while (parentGO.transform.GetChild(upIndex).GetComponent<SpriteRenderer>().color != tilecolor && parentGO.transform.GetChild(upIndex).position.x == parentGO.transform.GetChild(currentIndex).position.x)
            {
                upIndex -= size;
                if (upIndex < 0)
                    break;
            }

            int numberTile = Int32.Parse(parentGO.transform.GetChild(upIndex + size).GetChild(1).GetComponent<TextMeshPro>().text);
            numberTile++;
            while (!downClues.ContainsKey(numberTile))
            {
                if (numberTile > 100)
                    break;

                numberTile++;
            }

            for (int i = 0; i < gridLength; i++)
            {
                if (parentGO.transform.GetChild(i).GetChild(1).GetComponent<TextMeshPro>().text == numberTile.ToString())
                {
                    temp = i;
                }
            }
            currentIndex = temp;
        }
        UpdateClue();
    }


    public void PreviousClue()
    {
        if (horizontal)
        {
            int leftIndex = currentIndex;
            int temp = 0;

            while (parentGO.transform.GetChild(leftIndex).GetComponent<SpriteRenderer>().color != tilecolor && parentGO.transform.GetChild(leftIndex).position.y == parentGO.transform.GetChild(currentIndex).position.y)
            {
                leftIndex--;
                if (leftIndex < 0)
                    break;
            }


            if (leftIndex < 0)
            {
                for (int i = 0; i < gridLength; i++)
                {
                    if (parentGO.transform.GetChild(i).GetChild(1).GetComponent<TextMeshPro>().text == acrossClues.Keys.Last().ToString())
                    {
                        currentIndex = i;
                    }
                }
                UpdateClue();
                return;
            }


            int numberTile = Int32.Parse(parentGO.transform.GetChild(leftIndex + 1).GetChild(1).GetComponent<TextMeshPro>().text);
            numberTile--;
            while (!acrossClues.ContainsKey(numberTile))
            {
                if (numberTile < 0)
                    break;

                numberTile--;
            }

            for (int i = 0; i < gridLength; i++)
            {
                if (parentGO.transform.GetChild(i).GetChild(1).GetComponent<TextMeshPro>().text == numberTile.ToString())
                {
                    temp = i;
                }
            }
            currentIndex = temp;
        }
        else
        {
            int upIndex = currentIndex;
            int temp = 0;

            while (parentGO.transform.GetChild(upIndex).GetComponent<SpriteRenderer>().color != tilecolor && parentGO.transform.GetChild(upIndex).position.x == parentGO.transform.GetChild(currentIndex).position.x)
            {
                upIndex -= size;
                if (upIndex < 0)
                    break;
            }

            if (upIndex <= -size)
            {
                for (int i = 0; i < gridLength; i++)
                {
                    if (parentGO.transform.GetChild(i).GetChild(1).GetComponent<TextMeshPro>().text == downClues.Keys.Last().ToString())
                    {
                        currentIndex = i;
                    }
                }
                UpdateClue();
                return;
            }


            int numberTile = Int32.Parse(parentGO.transform.GetChild(upIndex + size).GetChild(1).GetComponent<TextMeshPro>().text);
            numberTile--;
            while (!downClues.ContainsKey(numberTile))
            {
                if (numberTile < 0)
                    break;

                numberTile--;
            }

            for (int i = 0; i < gridLength; i++)
            {
                if (parentGO.transform.GetChild(i).GetChild(1).GetComponent<TextMeshPro>().text == numberTile.ToString())
                {
                    temp = i;
                }
            }
            currentIndex = temp;
        }
        UpdateClue();
    }

    public bool IsFull()
    {
        int emptyTiles = 0;
        for (int i = 0; i < gridLength; i++)
        {   
            if (parentGO.transform.GetChild(i).GetChild(2).GetComponent<TextMeshPro>().text == "" && puzzlesData.puzzles[levelIndex - 1].grid[i] != '.')
                emptyTiles++;
        }

        if (emptyTiles == 0)
            return true;

        else return false;
    }
    public void SaveTypedLettersForLevel(int levelIndex, int gridSize)
    {
        List<TileData> tileDataList = new List<TileData>();

        for (int i = 0; i < gridLength; i++)
        {
            string letter = parentGO.transform.GetChild(i).GetChild(2).GetComponent<TextMeshPro>().text;
            bool isLetterRevealed = parentGO.transform.GetChild(i).GetChild(5).gameObject.activeSelf;
            string letterColorToSave = "#" + ColorUtility.ToHtmlStringRGBA(parentGO.transform.GetChild(i).GetChild(2).GetComponent<TextMeshPro>().color);

            TileData tileData = new TileData
            {
                tileIndex = i,
                letter = letter,
                isRevealed = isLetterRevealed,
                savedTimer = timer,
                letterColor = letterColorToSave
            };

            tileDataList.Add(tileData);
        }

        string json = JsonConvert.SerializeObject(tileDataList);
        string filePath = $"Assets/LevelData/Level_{levelIndex}_{gridSize}.json";
        File.WriteAllText(filePath, json);
    }

    private void LoadTypedLettersForLevel(int levelIndex, int gridSize)
    {
        string filePath = $"Assets/LevelData/Level_{levelIndex}_{gridSize}.json";

        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            List<TileData> tileDataList = JsonConvert.DeserializeObject<List<TileData>>(json);

            foreach (TileData tileData in tileDataList)
            {
                parentGO.transform.GetChild(tileData.tileIndex).GetChild(2).GetComponent<TextMeshPro>().text = tileData.letter;
                ColorUtility.TryParseHtmlString(tileData.letterColor, out Color color);
                if (color == Color.red)
                    parentGO.transform.GetChild(tileData.tileIndex).GetChild(2).GetComponent<TextMeshPro>().color = color;
                parentGO.transform.GetChild(tileData.tileIndex).GetChild(5).gameObject.SetActive(tileData.isRevealed);
                timer = tileData.savedTimer;
                showTimerWin.text = timerDisplay.text;

            }
        }

        int hours = (int)(timer / 3600);
        int minutes = (int)((timer % 3600) / 60);
        int seconds = (int)(timer % 60);

        timerDisplay.text = hours.ToString("0") + ":" + minutes.ToString("00") + ":" + seconds.ToString("00");

        for (int i = 0; i < gridLength; i++)
        { 
            if(parentGO.transform.GetChild(i).GetChild(5).gameObject.activeSelf)
            {
                parentGO.transform.GetChild(i).GetChild(0).gameObject.SetActive(true);
                parentGO.transform.GetChild(i).GetChild(2).gameObject.SetActive(false);
            }
        }
    }
    public void ClearSavedDataForLevel()
    {
        for (int i = 0; i < gridLength; i++)
        {
            parentGO.transform.GetChild(i).GetChild(2).gameObject.GetComponent<TextMeshPro>().text = "";
            parentGO.transform.GetChild(i).GetChild(0).gameObject.SetActive(false);
            parentGO.transform.GetChild(i).GetChild(2).gameObject.SetActive(true);
            parentGO.transform.GetChild(i).GetChild(5).gameObject.SetActive(false);


        }

        timer = 0f;
        timerDisplay.text = "00:00:00";
        youWin = false;

        string filePath = $"Assets/LevelData/Level_{levelIndex}_{gridSize}.json";

        if (File.Exists(filePath))
        {
            File.Delete(filePath);

            string metaFilePath = filePath + ".meta";
            if (File.Exists(metaFilePath))
            {
                File.Delete(metaFilePath);
            }
        }

        PlayerPrefs.DeleteKey($"LevelCompletionStatus{levelIndex}_{gridSize}");
        PlayerPrefs.DeleteKey($"LevelWrongShownStatus{levelIndex}_{gridSize}");
        PlayerPrefs.DeleteKey($"LevelTimeStatus{levelIndex}_{gridSize}");
        PlayerPrefs.DeleteKey($"LevelProgression{levelIndex}_{gridSize}");

    }
    public void DarkModeTrigger()
    {
        PlayerPrefs.SetInt("DarkMode", darkMode ? 0 : 1);
        PlayerPrefs.Save();

        darkMode= !darkMode;
    }

    public void SkipFilledLettersTrigger()
    {
        PlayerPrefs.SetInt("SkipFilled", skipFilled ? 0 : 1);
        PlayerPrefs.Save();
        skipFilled = !skipFilled;
    }

    public void DeletePreviousLetterTrigger()
    {
        PlayerPrefs.SetInt("DeletePrevious", deletePrevious ? 0 : 1);
        PlayerPrefs.Save();
        deletePrevious = !deletePrevious;
    }

    public void AudioTrigger()
    {
        PlayerPrefs.SetInt("PlayAudio", playAudio ? 0 : 1);
        PlayerPrefs.Save();
        playAudio = !playAudio;
    }
    public void ChangeToDarkMode()
    {
        if (darkMode)
        {
            for (int i = 0; i < gridLength; i++)
            {
                if (parentGO.transform.GetChild(i).GetComponent<SpriteRenderer>().color != tilecolor)
                {
                    parentGO.transform.GetChild(i).GetComponent<SpriteRenderer>().color = darkModeColor;
                    parentGO.transform.GetChild(i).GetChild(0).GetComponent<TextMeshPro>().color = Color.white;
                    parentGO.transform.GetChild(i).GetChild(1).GetComponent<TextMeshPro>().color = Color.white;
                    parentGO.transform.GetChild(i).GetChild(3).GetComponent<SpriteRenderer>().color = new Color32(134, 214, 253, 163);

                    if (parentGO.transform.GetChild(i).GetChild(2).GetComponent<TextMeshPro>().color != Color.red)
                        parentGO.transform.GetChild(i).GetChild(2).GetComponent<TextMeshPro>().color = Color.white;



                }
            }
        }
        else
        {
            for (int i = 0; i < gridLength; i++)
            {
                if (parentGO.transform.GetChild(i).GetComponent<SpriteRenderer>().color != tilecolor)
                {
                    parentGO.transform.GetChild(i).GetComponent<SpriteRenderer>().color = Color.white;
                    parentGO.transform.GetChild(i).GetChild(0).GetComponent<TextMeshPro>().color = Color.black;
                    parentGO.transform.GetChild(i).GetChild(1).GetComponent<TextMeshPro>().color = Color.black;
                    parentGO.transform.GetChild(i).GetChild(3).GetComponent<SpriteRenderer>().color = new Color32(255, 233, 0, 255);


                    if (parentGO.transform.GetChild(i).GetChild(2).GetComponent<TextMeshPro>().color != Color.red)
                        parentGO.transform.GetChild(i).GetChild(2).GetComponent<TextMeshPro>().color = Color.black;


                }
            }
        }
    }

    private void CheckWinCondition()
    {
        int correctTiles = 0;
        for (int i = 0; i < gridLength; i++)
        {
            if (parentGO.transform.GetChild(i).GetChild(2).GetComponent<TextMeshPro>().text == parentGO.transform.GetChild(i).GetChild(0).GetComponent<TextMeshPro>().text)
            {
                correctTiles++;
            }
            
            if(correctTiles == gridLength)
            {
                youWin = true;
                youWinUI.SetActive(true);
                youWinUI.transform.localScale = Vector3.zero;
                LeanTween.scale(youWinUI, Vector3.one, 0.3f).setEase(LeanTweenType.easeOutBack);
                transparent.SetActive(true);

                if(adsManager.gameObject.activeSelf)
                {
                    adsManager.ShowInterstitialAd();
                }
                showTimerWin.text = timerDisplay.text;
                if(helpButton.helpPanel.activeSelf)
                {
                    helpButton.ShowHelpPanel();
                }
                if(playAudio)
                    audioSrc.PlayOneShot(winSrc);
                SaveTypedLettersForLevel(levelIndex, gridSize);
            }
        }

        SaveLevelCompletionStatus(levelIndex, gridSize, youWin, timerDisplay.text);
    }

    public void SaveLevelProgression(int levelIndex, int gridSize)
    {
        int progression = 0;
        int lengthNotBlack = 0;
        float percentage = 0;
        for (int i=0; i < gridLength; i++)
        {
            if(parentGO.transform.GetChild(i).GetComponent<SpriteRenderer>().color != tilecolor)
            {
                lengthNotBlack++;

                if(parentGO.transform.GetChild(i).GetChild(2).GetComponent<TextMeshPro>().text != "")
                    {
                        progression++;
                    }
            }
        }
        percentage = (float)progression / lengthNotBlack;
        PlayerPrefs.SetFloat($"LevelProgression{levelIndex}_{gridSize}", percentage);
        PlayerPrefs.Save();
    }
    public void ShowErrors()
    {
        for (int i = 0; i < gridLength; i++)
        {
            if (parentGO.transform.GetChild(i).GetChild(2).GetComponent<TextMeshPro>().text != parentGO.transform.GetChild(i).GetChild(0).GetComponent<TextMeshPro>().text && parentGO.transform.GetChild(i).GetChild(2).GetComponent<TextMeshPro>().text != "")
            {
                parentGO.transform.GetChild(i).GetChild(2).GetComponent<TextMeshPro>().color = Color.red;
            }
        }
        SaveTypedLettersForLevel(levelIndex, gridSize);
    
    }
    public void SaveLevelCompletionStatus(int levelIndex, int gridSize, bool hasBeaten, string timerDisplay)
    {
        PlayerPrefs.SetInt($"LevelCompletionStatus{levelIndex}_{gridSize}", hasBeaten ? 1 : 0);
        PlayerPrefs.SetString($"LevelTimeStatus{levelIndex}_{gridSize}", timerDisplay);
        PlayerPrefs.Save();
    }

    public void ChangeGridSize(int selectedGridSize)
    {
        gridSize = selectedGridSize;
    }
}