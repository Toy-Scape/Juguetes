using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UI_System.Menus;

public class AdvancedMenuBuilder : EditorWindow
{
    [Header("Button Prefabs")]
    public GameObject primaryButtonPrefab;
    public GameObject secondaryButtonPrefab;

    [Header("Menu Manager Prefab (Optional)")]
    public GameObject menuManagerPrefab;

    [MenuItem("Tools/Advanced Menu Builder")]
    public static void ShowWindow()
    {
        GetWindow<AdvancedMenuBuilder>("Advanced Menu Builder");
    }

    private void OnGUI()
    {
        GUILayout.Label("Advanced Menu Builder", EditorStyles.boldLabel);

        primaryButtonPrefab = (GameObject)EditorGUILayout.ObjectField("Primary Button", primaryButtonPrefab, typeof(GameObject), false);
        secondaryButtonPrefab = (GameObject)EditorGUILayout.ObjectField("Secondary Button", secondaryButtonPrefab, typeof(GameObject), false);
        menuManagerPrefab = (GameObject)EditorGUILayout.ObjectField("MenuManager Prefab", menuManagerPrefab, typeof(GameObject), false);

        if (GUILayout.Button("Generate Full Menu"))
        {
            if (primaryButtonPrefab == null || secondaryButtonPrefab == null)
            {
                EditorUtility.DisplayDialog("Error", "Assign both Primary and Secondary Button prefabs!", "OK");
                return;
            }

            GenerateFullMenu();
        }
    }

    private void GenerateFullMenu()
    {
        // --- Canvas ---
        Canvas canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasGO = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.GetComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasGO.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
        }

        // --- EventSystem ---
        if (Object.FindObjectOfType<EventSystem>() == null)
        {
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        // --- MenuManager ---
        MenuManager menuManager;
        if (menuManagerPrefab != null)
        {
            GameObject managerGO = (GameObject)PrefabUtility.InstantiatePrefab(menuManagerPrefab);
            managerGO.name = "MenuManager";
            menuManager = managerGO.GetComponent<MenuManager>();
        }
        else
        {
            GameObject managerGO = new GameObject("MenuManager", typeof(MenuManager));
            menuManager = managerGO.GetComponent<MenuManager>();
        }

        // --- Panels ---
        GameObject mainPanel = CreatePanel("MainMenuPanel", canvas.transform, new Color(0, 0, 0, 0.5f));
        GameObject pausePanel = CreatePanel("PauseMenuPanel", canvas.transform, new Color(0, 0, 0, 0.5f));
        GameObject optionsPanel = CreatePanel("OptionsPanel", canvas.transform, new Color(0, 0, 0, 0.5f));

        // --- Buttons Main Menu ---
        GameObject playBtn = InstantiateButton(primaryButtonPrefab, "PlayButton", mainPanel.transform, "Play");
        GameObject optionsBtn = InstantiateButton(secondaryButtonPrefab, "OptionsButton", mainPanel.transform, "Options");
        GameObject quitBtn = InstantiateButton(secondaryButtonPrefab, "QuitButton", mainPanel.transform, "Quit");

        // --- Buttons Pause Menu ---
        GameObject resumeBtn = InstantiateButton(primaryButtonPrefab, "ResumeButton", pausePanel.transform, "Resume");
        GameObject pauseOptionsBtn = InstantiateButton(secondaryButtonPrefab, "PauseOptionsButton", pausePanel.transform, "Options");
        GameObject backToMenuBtn = InstantiateButton(secondaryButtonPrefab, "BackToMenuButton", pausePanel.transform, "Main Menu");

        // --- Buttons Options Panel ---
        GameObject backBtn = InstantiateButton(secondaryButtonPrefab, "BackButton", optionsPanel.transform, "Back");

        // --- Assign Panels & First Selected to MenuManager ---
        menuManager.GetType().GetField("_mainMenuPanel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(menuManager, mainPanel);
        menuManager.GetType().GetField("_pauseMenuPanel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(menuManager, pausePanel);
        menuManager.GetType().GetField("_optionsPanel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(menuManager, optionsPanel);

        menuManager.GetType().GetField("_mainMenuFirstSelected", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(menuManager, playBtn);
        menuManager.GetType().GetField("_pauseMenuFirstSelected", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(menuManager, resumeBtn);
        menuManager.GetType().GetField("_optionsFirstSelected", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(menuManager, backBtn);

        // --- Button Events ---
        AddButtonListener(playBtn, menuManager, "PlayGame");
        AddButtonListener(optionsBtn, menuManager, "OpenOptions");
        AddButtonListener(quitBtn, menuManager, "QuitGame");

        AddButtonListener(resumeBtn, menuManager, "ResumeGame");
        AddButtonListener(pauseOptionsBtn, menuManager, "OpenOptions");
        AddButtonListener(backToMenuBtn, menuManager, "LoadMainMenu");

        AddButtonListener(backBtn, menuManager, "BackFromOptions");

        // --- Deactivate panels by default ---
        pausePanel.SetActive(false);
        optionsPanel.SetActive(false);

        EditorUtility.DisplayDialog("Advanced Menu Builder", "Full menu generated successfully!", "OK");
    }

    private GameObject CreatePanel(string name, Transform parent, Color bgColor)
    {
        GameObject panel = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        panel.transform.SetParent(parent, false);
        Image img = panel.GetComponent<Image>();
        img.color = bgColor;

        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        VerticalLayoutGroup layout = panel.AddComponent<VerticalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.spacing = 20;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        panel.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        return panel;
    }

    private GameObject InstantiateButton(GameObject prefab, string name, Transform parent, string label)
    {
        GameObject btn = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        btn.name = name;
        btn.transform.SetParent(parent, false);
        var text = btn.GetComponentInChildren<Text>();
        if (text != null) text.text = label;
        return btn;
    }

    private void AddButtonListener(GameObject btn, MenuManager manager, string methodName)
    {
        var button = btn.GetComponent<Button>();
        if (button != null)
        {
            UnityEditor.Events.UnityEventTools.AddPersistentListener(button.onClick, () =>
            {
                var mi = manager.GetType().GetMethod(methodName);
                if (mi != null) mi.Invoke(manager, null);
            });
        }
    }
}
