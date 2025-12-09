using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Inventory.UI;

public static class ActionMaps
{
    public const string Player = "Player";
    public const string UI = "UI";
    public const string Dialogue = "Dialogue";
}

[DefaultExecutionOrder(-100)]
public class InputMapManager : MonoBehaviour
{
    public static InputMapManager Instance { get; private set; }

    [SerializeField] private PlayerInput playerInput;

    private int uiCounter = 0;
    private int dialogueCounter = 0;

    public static event Action<string> OnActionMapChanged;

    private void Awake ()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (playerInput == null)
            playerInput = GetComponent<PlayerInput>();

        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable ()
    {
        RadialMenuController.OnRadialOpen += HandleRadialOpen;
        RadialMenuController.OnRadialClose += HandleRadialClose;

        InventoryUI.OnInventoryOpened += HandleOpenUI;
        InventoryUI.OnInventoryClosed += HandleCloseUI;

        DialogueBox.OnDialogueOpen += HandleDialogueOpen;
        DialogueBox.OnDialogueClose += HandleDialogueClose;
    }

    private void OnDisable ()
    {
        RadialMenuController.OnRadialOpen -= HandleRadialOpen;
        RadialMenuController.OnRadialClose -= HandleRadialClose;

        InventoryUI.OnInventoryOpened -= HandleOpenUI;
        InventoryUI.OnInventoryClosed -= HandleCloseUI;

        //DialogueBox.OnDialogueOpen -= HandleDialogueOpen;
        //DialogueBox.OnDialogueClose -= HandleDialogueClose;
    }

    private void Start ()
    {
        if (playerInput != null)
            UpdateActionMap();
        else
            Debug.LogWarning("[InputManager] PlayerInput no asignado al iniciar.");
    }

    public void SwitchToActionMap (string mapName)
    {
        if (playerInput == null) return;
        playerInput.SwitchCurrentActionMap(mapName);
        OnActionMapChanged?.Invoke(mapName);
    }

    public void SwitchToActionMapSafe (string mapName)
    {
        if (playerInput == null) return;
        StartCoroutine(SwitchCoroutine(mapName));
    }

    private IEnumerator SwitchCoroutine (string mapName)
    {
        yield return new WaitForEndOfFrame();
        SwitchToActionMap(mapName);
    }

    public void HandleOpenUI ()
    {
        uiCounter++;
        UpdateActionMap();
    }

    public void HandleCloseUI ()
    {
        uiCounter = Mathf.Max(0, uiCounter - 1);
        UpdateActionMap();
    }

    public void HandleDialogueOpen ()
    {
        dialogueCounter++;
        UpdateActionMap();
    }

    public void HandleDialogueClose ()
    {
        dialogueCounter = Mathf.Max(0, dialogueCounter - 1);
        UpdateActionMap();
    }

    private void HandleRadialOpen ()
    {
        CameraManager.Instance.LockCameraMovement();
        CameraManager.Instance.UnlockCursor();
    }

    private void HandleRadialClose ()
    {
        CameraManager.Instance.UnlockCameraMovement();
        CameraManager.Instance.LockCursor();
    }

    private void UpdateActionMap ()
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
            SwitchToActionMapSafe(newMap);
        }
    }

    public string GetCurrentActionMap ()
    {
        return playerInput != null && playerInput.currentActionMap != null ? playerInput.currentActionMap.name : string.Empty;
    }
}
