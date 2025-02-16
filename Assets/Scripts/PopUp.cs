using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopUp : MonoBehaviour
{
    [Header("Popup Settings")]
    public GameObject transparent;
    public float popupTime = 0.5f; // Duration of the popup animation
    public LeanTweenType easeType = LeanTweenType.easeOutBack; // Easing type

    /// <summary>
    /// Shows the popup animation for the specified GameObject.
    /// </summary>
    /// <param name="targetObject">The UI element or GameObject to animate.</param>
    public void ShowPopup(GameObject targetObject)
    {
        transparent.SetActive(true); // Enable the transparent background
        // Ensure the object is active and scaled to 0
        targetObject.SetActive(true);
        targetObject.transform.localScale = Vector3.zero;

        // Animate from scale 0 to scale 1
        LeanTween.scale(targetObject, Vector3.one, popupTime).setEase(easeType);
    }

    /// <summary>
    /// Closes the popup animation for the specified GameObject.
    /// </summary>
    /// <param name="targetObject">The UI element or GameObject to animate.</param>
    public void ClosePopup(GameObject targetObject)
    {
        LeanTween.scale(targetObject, Vector3.zero, popupTime)
            .setEase(LeanTweenType.easeInBack)
            .setOnComplete(() =>
            {
                targetObject.SetActive(false); // Disable the object after shrinking
                transparent.SetActive(false); // Disable the transparent background
            });
    }
}
