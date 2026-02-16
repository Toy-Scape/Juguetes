using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.PlayerController;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class DialogueBox : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject dialogueContent = default;
    [SerializeField] private GameObject thoughtContent = default;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private TextMeshProUGUI thoughtText;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Image nextDialoguePrompt;

    [Header("Providers")]
    [SerializeField] private InputPromptIconProvider inputPromptIconProvider;

    [Header("Settings")]
    [SerializeField] private float textSpeed = 10f;
    [SerializeField] private float thoughtsCloseDelay = 2f;

    [Header("Input")]
    [SerializeField] private InputActionReference nextDialogueAction;

    [Header("Events")]
    [SerializeField] public UnityEvent onOpen;
    [SerializeField] public UnityEvent onClose;

    public static event Action OnDialogueOpen;
    public static event Action OnDialogueClose;
    public static event Action OnDialogueVisibleClose;

    private Dialogue activeDialogue;
    private string fullText;
    private int dialogueIndex;
    private static float globalInteractionLockUntil = 0f;
    private const float interactionLockTime = 0.25f;
    private Coroutine typingCoroutine;
    private Coroutine thoughtCloseCoroutine;
    private Coroutine enableNextDialogueCoroutine;
    private Coroutine unlockInputCoroutine;

    private GameObject player;
    private GameObject currentSpeaker;

    private HashSet<int> duringTriggeredLines = new HashSet<int>();

    public bool IsTyping { get; private set; }

    public bool IsOpen => dialogueContent.activeInHierarchy || thoughtContent.activeInHierarchy;

    public static DialogueBox Instance { get; private set; }

    private TextMeshProUGUI CurrentText =>
        activeDialogue.Type == Dialogue.DialogueType.Normal ? dialogueText : thoughtText;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        var controller = FindFirstObjectByType<PlayerController>();
        if (controller != null)
            player = controller.gameObject;

        Instance = this;
        CloseImmediate();
    }

    private void OnDestroy()
    {
        if (nextDialogueAction?.action != null)
        {
            nextDialogueAction.action.performed -= OnNextDialogueAction;
            nextDialogueAction.action.Disable();
        }
    }

    public void Open(bool skipStaticEvents = false)
    {
        if (activeDialogue == null)
            return;

        if (activeDialogue.Type == Dialogue.DialogueType.Normal)
        {
            dialogueContent.SetActive(true);
            thoughtContent.SetActive(false);
            if (!skipStaticEvents) OnDialogueOpen?.Invoke();
            if (enableNextDialogueCoroutine != null) StopCoroutine(enableNextDialogueCoroutine);
            enableNextDialogueCoroutine = StartCoroutine(EnableNextDialogueAction());
            UpdateNextPrompt(false);
        }
        else
        {
            dialogueContent.SetActive(false);
            thoughtContent.SetActive(true);
            onOpen?.Invoke();
        }
    }

    public void Close()
    {
        if (thoughtCloseCoroutine != null)
        {
            StopCoroutine(thoughtCloseCoroutine);
            thoughtCloseCoroutine = null;
        }

        dialogueContent.SetActive(false);
        thoughtContent.SetActive(false);
        UpdateNextPrompt(false);

        float delay = activeDialogue != null ? activeDialogue.postDialogueInputDelay : 0f;
        globalInteractionLockUntil = Time.unscaledTime + interactionLockTime;

        onClose?.Invoke();
        OnDialogueVisibleClose?.Invoke();

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        IsTyping = false;

        if (enableNextDialogueCoroutine != null)
        {
            StopCoroutine(enableNextDialogueCoroutine);
            enableNextDialogueCoroutine = null;
        }

        if (nextDialogueAction?.action != null)
        {
            nextDialogueAction.action.performed -= OnNextDialogueAction;
            nextDialogueAction.action.Disable();
        }

        if (unlockInputCoroutine != null)
        {
            StopCoroutine(unlockInputCoroutine);
            unlockInputCoroutine = null;
        }

        unlockInputCoroutine = StartCoroutine(UnlockInputRoutine(delay));
    }

    private IEnumerator UnlockInputRoutine(float delay)
    {
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        OnDialogueClose?.Invoke();
        activeDialogue = null;
        unlockInputCoroutine = null;
    }

    private void CloseImmediate()
    {
        dialogueContent.SetActive(false);
        thoughtContent.SetActive(false);
        UpdateNextPrompt(false);

        if (nextDialogueAction?.action != null)
        {
            nextDialogueAction.action.performed -= OnNextDialogueAction;
            nextDialogueAction.action.Disable();
        }

        if (enableNextDialogueCoroutine != null)
        {
            StopCoroutine(enableNextDialogueCoroutine);
            enableNextDialogueCoroutine = null;
        }

        IsTyping = false;
    }

    private void OnNextDialogueAction(InputAction.CallbackContext ctx) => Next();

    public void Next()
    {
        if (activeDialogue == null)
            return;

        var cinematicPlayer = FindFirstObjectByType<CinematicSystem.Application.CinematicPlayer>();
        if (cinematicPlayer != null && cinematicPlayer.IsPlaying)
        {
            cinematicPlayer.Advance();
        }

        if (IsTyping)
        {
            FastForward();
            return;
        }

        if (dialogueIndex >= activeDialogue.Lines.Count)
        {
            StartCoroutine(CloseSafelyNextFrame());
            return;
        }

        var line = activeDialogue.Lines[dialogueIndex];

        nameText.text = line.GetCharacterName();
        nameText.color = line.GetNameColor();

        if (Localization.LocalizationManager.Instance != null)
            fullText = Localization.LocalizationManager.Instance.GetLocalizedValue(line.Key);
        else
            fullText = line.Key;

        currentSpeaker = CharacterManager.Instance.GetModel(line.Character.CharacterId);
        var context = new DialogueContext(player, currentSpeaker);

        activeDialogue.TriggerActions(dialogueIndex, TriggerTiming.OnStart, context);

        IsTyping = true;
        UpdateNextPrompt(false);
        typingCoroutine = StartCoroutine(TypeWriterEffectCoroutine(dialogueIndex, context));

        dialogueIndex++;
    }

    public void FastForward()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        CurrentText.text = fullText;
        IsTyping = false;

        var context = new DialogueContext(player, currentSpeaker);
        int currentLine = dialogueIndex - 1;

        if (!duringTriggeredLines.Contains(currentLine))
        {
            activeDialogue.TriggerActions(currentLine, TriggerTiming.During, context);
            duringTriggeredLines.Add(currentLine);
        }

        activeDialogue.TriggerActions(currentLine, TriggerTiming.OnEnd, context);

        OnTypingComplete();
    }

    public void StartDialogue(Dialogue dialogue)
    {
        if (Time.unscaledTime < globalInteractionLockUntil)
            return;

        if (IsOpen) return;

        if (unlockInputCoroutine != null)
        {
            StopCoroutine(unlockInputCoroutine);
            unlockInputCoroutine = null;
            OnDialogueClose?.Invoke();
            activeDialogue = null;
        }

        activeDialogue = dialogue;
        dialogueIndex = 0;
        duringTriggeredLines.Clear();

        Open();
        Next();
    }

    private IEnumerator TypeWriterEffectCoroutine(int lineIndex, DialogueContext context)
    {
        CurrentText.text = string.Empty;
        float delay = Mathf.Max(0.001f, 1f / Mathf.Max(1f, textSpeed));

        int half = fullText.Length / 2;
        bool duringTriggered = false;
        int i = 0;

        foreach (char c in fullText)
        {
            if (!IsOpen) yield break;

            CurrentText.text += c;

            if (!duringTriggered && i == half)
            {
                duringTriggered = true;
                duringTriggeredLines.Add(lineIndex);
                activeDialogue.TriggerActions(lineIndex, TriggerTiming.During, context);
            }

            i++;
            yield return new WaitForSeconds(delay);
        }

        if (!duringTriggered)
        {
            duringTriggeredLines.Add(lineIndex);
            activeDialogue.TriggerActions(lineIndex, TriggerTiming.During, context);
        }

        IsTyping = false;
        activeDialogue.TriggerActions(lineIndex, TriggerTiming.OnEnd, context);
        OnTypingComplete();
    }

    private void OnTypingComplete()
    {
        if (activeDialogue != null && activeDialogue.Type == Dialogue.DialogueType.Normal)
            UpdateNextPrompt(true);

        if (activeDialogue != null && activeDialogue.Type == Dialogue.DialogueType.Thought)
        {
            if (thoughtCloseCoroutine != null)
            {
                StopCoroutine(thoughtCloseCoroutine);
                thoughtCloseCoroutine = null;
            }
            thoughtCloseCoroutine = StartCoroutine(CloseThoughtAfterDelay());
        }
    }

    private void UpdateNextPrompt(bool visible)
    {
        if (nextDialoguePrompt == null || inputPromptIconProvider == null)
            return;

        if (!visible)
        {
            nextDialoguePrompt.enabled = false;
            return;
        }

        var sprite = inputPromptIconProvider.GetCurrentInteractIcon();
        nextDialoguePrompt.sprite = sprite;
        nextDialoguePrompt.enabled = sprite != null;
    }

    private IEnumerator CloseThoughtAfterDelay()
    {
        yield return new WaitForSeconds(thoughtsCloseDelay);
        Close();
    }

    private IEnumerator EnableNextDialogueAction()
    {
        yield return null;

        if (nextDialogueAction?.action == null)
            yield break;

        nextDialogueAction.action.performed -= OnNextDialogueAction;

        if (IsOpen && activeDialogue != null)
            nextDialogueAction.action.performed += OnNextDialogueAction;
    }

    private IEnumerator CloseSafelyNextFrame()
    {
        yield return null;
        yield return new WaitForEndOfFrame();
        Close();
    }
}
