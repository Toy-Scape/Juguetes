using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Inventory.UI;

public static class ActionMaps
{
    public const string Player = "Player";
    public const string UI = "UI";
    public const string Dialogue = "Dialogue";
}

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    [SerializeField] private PlayerInput playerInput;

    private int uiCounter = 0;
    private int dialogueCounter = 0;

    public static event Action<string> OnActionMapChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        RadialMenuController.OnRadialOpen += HandleRadialOpen;
        RadialMenuController.OnRadialClose += HandleRadialClose;

        InventoryUI.OnInventoryOpened += OpenUI;
        InventoryUI.OnInventoryClosed += CloseUI;

        DialogueBox.OnDialogueOpen += OpenDialogue;
        DialogueBox.OnDialogueClose += CloseDialogue;
    }

    private void OnDisable()
    {
        RadialMenuController.OnRadialOpen -= HandleRadialOpen;
        RadialMenuController.OnRadialClose -= HandleRadialClose;

        InventoryUI.OnInventoryOpened -= OpenUI;
        InventoryUI.OnInventoryClosed -= CloseUI;

        DialogueBox.OnDialogueOpen -= OpenDialogue;
        DialogueBox.OnDialogueClose -= CloseDialogue;
    }

    private void Start()
    {
        if (playerInput != null)
            UpdateActionMap();
        else
            Debug.LogWarning("[InputManager] PlayerInput no asignado al iniciar.");
    }

    public void OpenUI()
    {
        uiCounter++;
        UpdateActionMap();
    }

    public void CloseUI()
    {
        uiCounter = Mathf.Max(0, uiCounter - 1);
        UpdateActionMap();
    }

    public void OpenDialogue()
    {
        dialogueCounter++;
        UpdateActionMap();
    }

    public void CloseDialogue()
    {
        dialogueCounter = Mathf.Max(0, dialogueCounter - 1);
        UpdateActionMap();
    }

    private void HandleRadialOpen()
    {
        OnActionMapChanged?.Invoke(ActionMaps.Player);
    }

    private void HandleRadialClose()
    {
        OnActionMapChanged?.Invoke(ActionMaps.Player);
    }

    private void UpdateActionMap()
    {
        string newMap;

        if (uiCounter > 0)
            newMap = ActionMaps.UI;
        else if (dialogueCounter > 0)
            newMap = ActionMaps.Dialogue;
        else
            newMap = ActionMaps.Player;

        if (playerInput != null && playerInput.currentActionMap != null && playerInput.currentActionMap.name != newMap)
        {
            playerInput.SwitchCurrentActionMap(newMap);
            OnActionMapChanged?.Invoke(newMap);
            Debug.Log($"[InputManager] ActionMap cambiado a: {newMap}");
        }
    }

    public string GetCurrentActionMap()
    {
        return playerInput != null && playerInput.currentActionMap != null ? playerInput.currentActionMap.name : string.Empty;
    }
}
