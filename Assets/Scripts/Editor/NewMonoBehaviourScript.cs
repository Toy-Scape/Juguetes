using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UI_System.Menus;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class AdvancedMenuBuilder : EditorWindow
{
    public GameObject primaryButtonPrefab;
    public GameObject secondaryButtonPrefab;
    public GameObject menuManagerPrefab;

    [MenuItem("Tools/Advanced Menu Builder")]
    public static void ShowWindow()
    {
        GetWindow<AdvancedMenuBuilder>("Advanced Menu Builder");
    }

    private void OnGUI()
    {
        GUILayout.Label("Advanced Menu Builder", EditorStyles.boldLabel);

        primaryButtonPrefab = (GameObject)EditorGUILayout.ObjectField(
            "Primary Button", primaryButtonPrefab, typeof(GameObject), false);

        secondaryButtonPrefab = (GameObject)EditorGUILayout.ObjectField(
            "Secondary Button", secondaryButtonPrefab, typeof(GameObject), false);

        menuManagerPrefab = (GameObject)EditorGUILayout.ObjectField(
            "MenuManager Prefab", menuManagerPrefab, typeof(GameObject), false);

        if (GUILayout.Button("Generate Full Menu"))
            GenerateFullMenu();
    }

    private void GenerateFullMenu()
    {
        // ================= Canvas =================
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (!canvas)
        {
            GameObject c = new GameObject(
                "Canvas",
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster)
            );
            c.layer = LayerMask.NameToLayer("UI");

            canvas = c.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler s = c.GetComponent<CanvasScaler>();
            s.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            s.referenceResolution = new Vector2(1920, 1080);
        }

        // ================= EventSystem =================
        if (!FindFirstObjectByType<EventSystem>())
        {
            new GameObject(
                "EventSystem",
                typeof(EventSystem),
                typeof(InputSystemUIInputModule)
            );
        }

        // ================= MenuManager =================
        MenuManager manager;
        if (menuManagerPrefab)
        {
            GameObject m = (GameObject)PrefabUtility.InstantiatePrefab(menuManagerPrefab);
            m.name = "MenuManager";
            manager = m.GetComponent<MenuManager>();
        }
        else
        {
            manager = new GameObject("MenuManager", typeof(MenuManager))
                .GetComponent<MenuManager>();
        }
        manager.gameObject.layer = LayerMask.NameToLayer("UI");

        // ================= Panels =================
        GameObject mainPanel = CreateRightPanel(canvas.transform, "MainMenuPanel");
        GameObject pausePanel = CreateFullscreenPanel("PauseMenuPanel", canvas.transform);
        GameObject optionsPanel = CreateFullscreenPanel("OptionsPanel", canvas.transform);

        Transform mainContent = CreateContent(mainPanel.transform);
        Transform pauseContent = CreateContent(pausePanel.transform);
        Transform optionsContent = CreateContent(optionsPanel.transform);

        // ================= Buttons =================
        GameObject play = CreateButton(primaryButtonPrefab, "Play", mainContent);
        GameObject opt = CreateButton(secondaryButtonPrefab, "Options", mainContent);
        GameObject quit = CreateButton(secondaryButtonPrefab, "Quit", mainContent);

        GameObject resume = CreateButton(primaryButtonPrefab, "Resume", pauseContent);
        GameObject pauseOpt = CreateButton(secondaryButtonPrefab, "Options", pauseContent);
        GameObject backMenu = CreateButton(secondaryButtonPrefab, "Main Menu", pauseContent);

        GameObject backOpt = CreateButton(secondaryButtonPrefab, "Back", optionsContent);

        // ================= Options UI =================

        CreateDropdown(
            "FPSDropdown",
            optionsContent,
            new List<string> { "30 FPS", "60 FPS", "120 FPS", "Unlimited" }
        );

        Slider mouseSlider = CreateSlider(
            "MouseSensitivitySlider",
            optionsContent,
            0.1f, 5f, 1f
        );
        CreateValueText("MouseSensitivityValueText", mouseSlider.transform);

        Slider gamepadSlider = CreateSlider(
            "GamepadSensitivitySlider",
            optionsContent,
            0.1f, 5f, 1f
        );
        CreateValueText("GamepadSensitivityValueText", gamepadSlider.transform);

        Slider volumeSlider = CreateSlider(
            "VolumeSlider",
            optionsContent,
            0f, 1f, 1f
        );
        CreateValueText("VolumeValueText", volumeSlider.transform);

        CreateDropdown(
            "LanguageDropdown",
            optionsContent,
            new List<string> { "English", "Español", "Euskera" }
        );

        // ================= Events =================
        Bind(play, manager, "OnPlayClicked");
        Bind(opt, manager, "OnOptionsClicked");
        Bind(quit, manager, "OnQuitClicked");

        Bind(resume, manager, "OnResumeClicked");
        Bind(pauseOpt, manager, "OnOptionsClicked");
        Bind(backMenu, manager, "OnBackToMenuClicked");

        Bind(backOpt, manager, "OnBackFromOptionsClicked");

        // ================= States =================
        pausePanel.SetActive(false);
        optionsPanel.SetActive(false);

        EditorUtility.DisplayDialog("Done", "Menu generated correctly", "OK");
    }

    // ================= Helpers =================

    void Bind(GameObject btn, MenuManager mgr, string method)
    {
        if (!btn) return;

        Button b = btn.GetComponent<Button>();
        if (!b) return;

        MethodInfo mi = typeof(MenuManager).GetMethod(method);
        if (mi == null) return;

        UnityEditor.Events.UnityEventTools.AddPersistentListener(
            b.onClick,
            (UnityEngine.Events.UnityAction)
                System.Delegate.CreateDelegate(
                    typeof(UnityEngine.Events.UnityAction),
                    mgr,
                    mi
                )
        );
    }

    GameObject CreateButton(GameObject prefab, string text, Transform parent)
    {
        if (!prefab) return null;

        GameObject b = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        b.transform.SetParent(parent, false);
        b.layer = LayerMask.NameToLayer("UI");

        Button btn = b.GetComponent<Button>();
        Image img = b.GetComponent<Image>();

        if (btn && img && btn.targetGraphic == null)
            btn.targetGraphic = img;

        TextMeshProUGUI tmp = b.GetComponentInChildren<TextMeshProUGUI>();
        if (tmp) tmp.text = text;

        return b;
    }


    GameObject CreateFullscreenPanel(string name, Transform parent)
    {
        GameObject p = new GameObject(name, typeof(Image));
        p.transform.SetParent(parent, false);
        p.layer = LayerMask.NameToLayer("UI");

        p.GetComponent<Image>().color = new Color(0, 0, 0, 0.5f);

        RectTransform rt = p.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;

        return p;
    }

    GameObject CreateRightPanel(Transform parent, string name)
    {
        GameObject p = new GameObject(name, typeof(Image));
        p.transform.SetParent(parent, false);
        p.layer = LayerMask.NameToLayer("UI");

        p.GetComponent<Image>().color = new Color32(46, 46, 46, 100);

        RectTransform rt = p.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(1, 0);
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(1, 0.5f);
        rt.sizeDelta = new Vector2(420, 0);

        return p;
    }

    Transform CreateContent(Transform parent)
    {
        GameObject c = new GameObject("Content", typeof(RectTransform));
        c.transform.SetParent(parent, false);
        c.layer = LayerMask.NameToLayer("UI");

        RectTransform rt = c.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 0.5f);
        rt.anchorMax = new Vector2(1, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        VerticalLayoutGroup v = c.AddComponent<VerticalLayoutGroup>();
        v.spacing = 20;
        v.childAlignment = TextAnchor.MiddleCenter;
        v.childControlWidth = true;
        v.childControlHeight = true;
        v.childForceExpandWidth = true;
        v.childForceExpandHeight = false;
        v.padding = new RectOffset(20, 20, 0, 0);

        ContentSizeFitter fitter = c.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        return c.transform;
    }

    TMP_Dropdown CreateDropdown(string name, Transform parent, List<string> options)
    {
        GameObject go = new GameObject(
            name,
            typeof(RectTransform),
            typeof(Image),
            typeof(TMP_Dropdown)
        );
        go.transform.SetParent(parent, false);
        go.layer = LayerMask.NameToLayer("UI");

        TMP_Dropdown dropdown = go.GetComponent<TMP_Dropdown>();
        dropdown.ClearOptions();
        dropdown.AddOptions(options);

        LayoutElement le = go.AddComponent<LayoutElement>();
        le.preferredWidth = 300;
        le.minWidth = 200;
        le.flexibleWidth = 1;

        return dropdown;
    }

    Slider CreateSlider(string name, Transform parent, float min, float max, float value)
    {
        GameObject go = new GameObject(
            name,
            typeof(RectTransform),
            typeof(Image),
            typeof(Slider)
        );
        go.transform.SetParent(parent, false);
        go.layer = LayerMask.NameToLayer("UI");

        Slider s = go.GetComponent<Slider>();
        s.minValue = min;
        s.maxValue = max;
        s.value = value;

        LayoutElement le = go.AddComponent<LayoutElement>();
        le.preferredWidth = 300;
        le.minWidth = 200;
        le.flexibleWidth = 1;

        return s;
    }

    TextMeshProUGUI CreateValueText(string name, Transform parent)
    {
        GameObject go = new GameObject(
            name,
            typeof(RectTransform),
            typeof(TextMeshProUGUI)
        );
        go.transform.SetParent(parent, false);
        go.layer = LayerMask.NameToLayer("UI");

        TextMeshProUGUI tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.text = "0";
        tmp.fontSize = 24;
        tmp.alignment = TextAlignmentOptions.Center;

        LayoutElement le = go.AddComponent<LayoutElement>();
        le.preferredWidth = 100;
        le.minWidth = 80;

        return tmp;
    }
}
