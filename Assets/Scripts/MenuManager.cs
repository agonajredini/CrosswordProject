using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public GameObject standardGameObject;
    public GameObject midiGameObject;
    public GameObject miniGameObject;

    public TextMeshProUGUI standardText;
    public TextMeshProUGUI midiText;
    public TextMeshProUGUI miniText;

    private int standardAmount;
    private int midiAmount;
    private int miniAmount;
    // Start is called before the first frame update
    void Awake()
    {
        standardAmount = standardGameObject.GetComponentsInChildren<Button>().Length;
        midiAmount = midiGameObject.GetComponentsInChildren<Button>().Length;
        miniAmount = miniGameObject.GetComponentsInChildren<Button>().Length;

        standardText.text = $"{standardAmount} Fjalëkryqe";
        midiText.text = $"{midiAmount} Fjalëkryqe";
        miniText.text = $"{miniAmount} Fjalëkryqe";
    }

    
}
