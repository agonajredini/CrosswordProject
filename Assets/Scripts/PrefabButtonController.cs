using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PrefabButtonController : MonoBehaviour
{
    
    private void Awake()
    {
        // Get the button component
        Button button = GetComponent<Button>();
        if (button != null)
        {
            // Find the TweenManager and add the listener
            TweenManager tweenManager = FindFirstObjectByType<TweenManager>();
            if (tweenManager != null)
            {
            
                button.onClick.AddListener(tweenManager.LevelLoadAnimation);
            }
            else
            {
                Debug.LogWarning("TweenManager not found in the scene!");
            }
        }
    }


}
