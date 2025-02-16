using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class LevelCreator : EditorWindow
{
    private GameObject prefab; // Prefab to instantiate
    private string prefabName = "New Level";
    private GameObject parentGameObject; // Reference to the parent GameObject

    [MenuItem("Tools/Level Creator")]
    public static void ShowWindow()
    {
        // Show the window
        GetWindow<LevelCreator>("Level Creator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Create a New GameObject", EditorStyles.boldLabel);

        // Field to select the prefab
        prefab = (GameObject)EditorGUILayout.ObjectField("Prefab", prefab, typeof(GameObject), false);

        // Field to input the prefab name
        prefabName = EditorGUILayout.TextField("Prefab Name", prefabName);

        // Field to select the parent GameObject
        parentGameObject = (GameObject)EditorGUILayout.ObjectField("Parent GameObject", parentGameObject, typeof(GameObject), true);

        // Button to create the GameObject
        if (GUILayout.Button("Create Level"))
        {
            CreateNewGameObject();
        }
    }

    private void CreateNewGameObject()
    {
        if (prefab == null)
        {
            Debug.LogWarning("No prefab selected. Please select a prefab.");
            return;
        }

        if (parentGameObject == null)
        {
            Debug.LogWarning("No parent GameObject selected. Please select a parent.");
            return;
        }

        // Instantiate the prefab
        GameObject newGameObject = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        newGameObject.name = prefabName; // Set the name

        // Ensure correct scale
        newGameObject.transform.localScale = Vector3.one;

        // Check if the parent has 8 or more children
        if (parentGameObject.transform.childCount >= 8)
        {
            // Find the next available page name
            string newParentName = GetNextPageName(parentGameObject.name);

            // Create a new parent GameObject
            GameObject newParent = new GameObject(newParentName);

            // Copy the components from the original parent to the new parent
            CopyComponents(parentGameObject, newParent);

            // Set the new parent to the same hierarchy level as the original parent
            newParent.transform.SetParent(parentGameObject.transform.parent);

            // Ensure that the new parent has the specified width and height if it has a RectTransform
            RectTransform rectTransform = newParent.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(404.5991f, 708.9219f); // Set width and height
            newParent.transform.localScale = Vector3.one; // Reset scale

            // Set the new GameObject to be a child of the new parent
            newGameObject.transform.SetParent(newParent.transform);

            parentGameObject = newParent;

            // Update maxPage in SwipeController
            UpdateMaxPage();
        }
        else
        {
            // If the parent has fewer than 8 children, make it a direct child
            newGameObject.transform.SetParent(parentGameObject.transform);
        }

        // Ensure the instantiated GameObject has the correct scale even after parenting
        newGameObject.transform.localScale = Vector3.one;

        //Button button = newGameObject.GetComponent<Button>();
        //if (button != null)
        //{
        //    button.onClick.AddListener(CallLevelLoadAnimation); // Add the onClick listener
        //}

        // Update the child text to "Fjalëkryqi [prefabName]"
        TextMeshProUGUI[] childTextComponents = newGameObject.GetComponentsInChildren<TextMeshProUGUI>();
        if (childTextComponents.Length > 0)
        {
            // Assign text to each child as needed
            childTextComponents[0].text = $"Fjalëkryqi {prefabName}";

            if (childTextComponents.Length > 1)
            {
                childTextComponents[1].text = $"{prefabName}";
            }
        }

        IncrementPrefabName();

        // Select the newly created GameObject in the hierarchy
        Selection.activeGameObject = newGameObject;
    }

    private string GetNextPageName(string originalParentName)
    {
        // Extract the number from the original parent name (e.g., "Page1")
        int currentPageNumber = 1;

        // Try to parse the number at the end of the name
        string baseName = originalParentName;
        int lastDigitIndex = originalParentName.Length - 1;

        // Check if the name ends with a digit
        while (lastDigitIndex >= 0 && char.IsDigit(originalParentName[lastDigitIndex]))
        {
            lastDigitIndex--;
        }

        // Separate the base name and number
        if (lastDigitIndex < originalParentName.Length - 1)
        {
            baseName = originalParentName.Substring(0, lastDigitIndex + 1);
            int.TryParse(originalParentName.Substring(lastDigitIndex + 1), out currentPageNumber);
        }

        // Increment the page number
        int nextPageNumber = currentPageNumber + 1;

        // Find the next available page name
        string newPageName;
        do
        {
            newPageName = baseName + nextPageNumber;
            nextPageNumber++;
        }
        while (GameObject.Find(newPageName) != null); // Ensure the name is unique

        return newPageName;
    }

    private void CopyComponents(GameObject source, GameObject destination)
    {
        foreach (Component sourceComponent in source.GetComponents<Component>())
        {
            // Skip the Transform component, as it's automatically created
            if (sourceComponent is Transform) continue;

            // Copy each component
            Component newComponent = destination.AddComponent(sourceComponent.GetType());

            // Copy serialized properties
            SerializedObject sourceSerializedObject = new SerializedObject(sourceComponent);
            SerializedObject destinationSerializedObject = new SerializedObject(newComponent);
            sourceSerializedObject.Update();
            destinationSerializedObject.Update();
            SerializedProperty property = sourceSerializedObject.GetIterator();

            while (property.NextVisible(true))
            {
                if (property.name == "m_Script") continue; // Skip the script reference
                destinationSerializedObject.CopyFromSerializedProperty(property);
            }

            destinationSerializedObject.ApplyModifiedProperties();
        }
    }

    private void IncrementPrefabName()
    {
        // Check if the current name is a number
        if (int.TryParse(prefabName, out int currentNumber))
        {
            // Increment the number
            prefabName = (currentNumber + 1).ToString();
        }
    }

    private void CallLevelLoadAnimation()
    {
        // Get the instance of the TweenManager and call LevelLoadAnimation
        TweenManager tweenManager = FindObjectOfType<TweenManager>();
        if (tweenManager != null)
        {
            tweenManager.LevelLoadAnimation();
        }
        else
        {
            Debug.LogWarning("TweenManager not found in the scene!");
        }
    }
    private void UpdateMaxPage()
    {
        SwipeController swipeController = FindObjectOfType<SwipeController>();
        if (swipeController != null)
        {
            // Count all parent GameObjects with names starting with "Page"
            int pageCount = 0;
            foreach (Transform child in parentGameObject.transform.parent)
            {
                if (child.name.StartsWith("Page"))
                {
                    pageCount++;
                }
            }

            // Update maxPage in SwipeController
            swipeController.maxPage = pageCount;
        }
        else
        {
            Debug.LogWarning("SwipeController not found in the scene!");
        }
    }
}
