using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TweenManager : MonoBehaviour
{
    [Header("Ads Manager")]
    public AdsManager adsManager;

    [Header("Loading Box Settings")]
    public RectTransform loadingBox;
    public float boxMoveSpeed = 0.3f;
    public float boxReturnDelay = 0.2f;
    public LeanTweenType boxEaseType = LeanTweenType.easeInSine;
    public GameObject[] objectsToEnableStandard;
    public GameObject[] objectsToDisableStandard;
    public GameObject[] objectsToEnableMidi;
    public GameObject[] objectsToDisableMidi;
    public GameObject[] objectsToEnableMini;
    public GameObject[] objectsToDisableMini;

    private GameObject[] objectsToEnable;
    private GameObject[] objectsToDisable;

    [Header("Back Box Settings")]
    public RectTransform backBox;
    public float backBoxMoveSpeed = 0.3f;
    public float backBoxReturnDelay = 0.2f;
    public LeanTweenType backBoxEaseType = LeanTweenType.easeInSine;
    public GameObject[] objectsToEnableBack;
    public GameObject[] objectsToDisableBack;

    [Header("Level Load Settings")]
    public float levelBoxMoveSpeed = 0.1f;
    public float levelBoxReturnDelay = 0.2f;
    public LeanTweenType levelBoxEaseType = LeanTweenType.easeInSine;
    public GameObject[] objectsToEnableLevel;
    public GameObject[] objectsToDisableLevel;
    public GameObject clock;


    private Vector2 boxInitialPos;
    private Vector2 backBoxInitialPos;
    private void Start()
    {
        boxInitialPos = loadingBox.anchoredPosition;
        backBoxInitialPos = backBox.anchoredPosition;
    }

    public void LoadingAnimation()
    {
        loadingBox.gameObject.SetActive(true);
        LeanTween.moveX(loadingBox, 0, boxMoveSpeed).setEase(boxEaseType).setOnComplete(() =>
        {
            foreach (GameObject obj in objectsToEnable)
            {
                obj.SetActive(true);
            }

            foreach (GameObject obj in objectsToDisable)
            {
                obj.SetActive(false);
            }

            LeanTween.moveX(loadingBox, boxInitialPos.x, boxMoveSpeed).setDelay(boxReturnDelay).setEase(boxEaseType).setOnComplete(() =>
            {
                loadingBox.gameObject.SetActive(false);
            });

        });
    }

    public void SelectLevelSize(int levelSize)
    {
        if (levelSize == 0)
        {
            objectsToEnable = objectsToEnableStandard;
            objectsToDisable = objectsToDisableStandard;
        }

        else if (levelSize == 1)
        {
            objectsToEnable = objectsToEnableMidi;
            objectsToDisable = objectsToDisableMidi;
        }

        else if (levelSize == 2)
        {
            objectsToEnable = objectsToEnableMini;
            objectsToDisable = objectsToDisableMini;
        }
    }

    public void BackAnimation()
    {
        backBox.gameObject.SetActive(true);
        LeanTween.moveX(backBox, 0, backBoxMoveSpeed).setEase(backBoxEaseType).setOnComplete(() =>
        {
            foreach (GameObject obj in objectsToEnableBack)
            {
                obj.SetActive(true);
            }

            foreach (GameObject obj in objectsToDisableBack)
            {
                obj.SetActive(false);
            }

            LeanTween.moveX(backBox, backBoxInitialPos.x, backBoxMoveSpeed).setDelay(backBoxReturnDelay).setEase(backBoxEaseType).setOnComplete(() => 
            {
                if(adsManager.gameObject.activeSelf)
                    adsManager.LoadBannerAd();
                backBox.gameObject.SetActive(false);
            
            });

        });
    }


    IEnumerator Wait()
    {
        yield return new WaitForSeconds(2.5f);
        foreach (GameObject obj in objectsToEnableLevel)
        {
            obj.SetActive(true);
        }

        foreach (GameObject obj in objectsToDisableLevel)
        {
            obj.SetActive(false);
        }

        clock.SetActive(false);
    }

    public void LevelLoadAnimation()
    {
        loadingBox.gameObject.SetActive(true);
        backBox.gameObject.SetActive(true);

        LeanTween.moveX(loadingBox, 0, levelBoxMoveSpeed).setEase(levelBoxEaseType).setOnComplete(() =>
        {
            clock.SetActive(true);
            StartCoroutine(Wait());
            LeanTween.moveX(loadingBox, boxInitialPos.x, levelBoxMoveSpeed).setDelay(levelBoxReturnDelay).setEase(levelBoxEaseType).setOnComplete(() => { loadingBox.gameObject.SetActive(false); });

        });

        LeanTween.moveX(backBox, 0, levelBoxMoveSpeed).setEase(levelBoxEaseType).setOnComplete(() =>
        {
            LeanTween.moveX(backBox, backBoxInitialPos.x, levelBoxMoveSpeed).setDelay(levelBoxReturnDelay).setEase(levelBoxEaseType).setOnComplete(() => { backBox.gameObject.SetActive(false); });

        });
    }
}
