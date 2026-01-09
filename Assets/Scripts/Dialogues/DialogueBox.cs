using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class DialogueBox : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject dialogueContent = default;
    [SerializeField] private GameObject thoughtContent = default;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private TextMeshProUGUI thoughtText;
    [SerializeField] private TextMeshProUGUI nameText;

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

    private Dialogue activeDialogue;
    private string fullText;
    private int dialogueIndex;
    private static float globalInteractionLockUntil = 0f;
    private const float interactionLockTime = 0.25f;
    private Coroutine typingCoroutine;
    private Coroutine thoughtCloseCoroutine;

    private GameObject player;
    private GameObject currentSpeaker;

    private HashSet<int> duringTriggeredLines = new HashSet<int>();

    public bool IsTyping { get; private set; }
    public bool IsOpen => dialogueContent.activeInHierarchy || thoughtContent.activeInHierarchy;

    public static DialogueBox Instance { get; private set; }

    private TextMeshProUGUI CurrentText =>
        activeDialogue.Type == Dialogue.DialogueType.Normal ? dialogueText : thoughtText;

    private void Awake ()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        if (player == null)
        {
            var controller = FindFirstObjectByType<PlayerController>();
            if (controller != null)
                player = controller.gameObject;
        }

        Instance = this;
        CloseImmediate();
    }

    private void OnDestroy ()
    {
        if (nextDialogueAction?.action != null)
        {
            nextDialogueAction.action.performed -= OnNextDialogue;
            nextDialogueAction.action.Disable();
        }
    }

    public void Open ()
    {
        if (activeDialogue == null)
            return;

        if (activeDialogue.Type == Dialogue.DialogueType.Normal)
        {
            dialogueContent.SetActive(true);
            thoughtContent.SetActive(false);
            OnDialogueOpen?.Invoke();
            StartCoroutine(EnableNextDialogueAction());
        }
        else
        {
            dialogueContent.SetActive(false);
            thoughtContent.SetActive(true);
        }

        onOpen?.Invoke();
    }

    public void Close ()
    {
        if (thoughtCloseCoroutine != null)
        {
            StopCoroutine(thoughtCloseCoroutine);
            thoughtCloseCoroutine = null;
        }

        dialogueContent.SetActive(false);
        thoughtContent.SetActive(false);

        globalInteractionLockUntil = Time.unscaledTime + interactionLockTime;

        onClose?.Invoke();
        OnDialogueClose?.Invoke();

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        IsTyping = false;
    }

    private void CloseImmediate ()
    {
        dialogueContent.SetActive(false);
        thoughtContent.SetActive(false);

        if (nextDialogueAction?.action != null)
        {
            nextDialogueAction.action.performed -= OnNextDialogue;
            nextDialogueAction.action.Disable();
        }

        IsTyping = false;
    }

    private void OnNextDialogue (InputAction.CallbackContext ctx) => Next();

    public void Next ()
    {
        if (activeDialogue == null)
            return;

        if (IsTyping)
        {
            FastForward();
            return;
        }

        if (dialogueIndex >= activeDialogue.Lines.Count)
        {
            StartCoroutine(CCloseSafelyNextFrame());
            return;
        }

        var line = activeDialogue.Lines[dialogueIndex];

        nameText.text = line.GetCharacterName();
        nameText.color = line.GetNameColor();
        fullText = line.Text ?? string.Empty;

        currentSpeaker = CharacterManager.Instance.GetModel(line.Character.CharacterId);

        var context = new DialogueContext(player, currentSpeaker);

        activeDialogue.TriggerActions(dialogueIndex, TriggerTiming.OnStart, context);

        IsTyping = true;
        typingCoroutine = StartCoroutine(TypeWriterEffectCoroutine(dialogueIndex, context));

        dialogueIndex++;
    }

    public void FastForward ()
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

    public void StartDialogue (Dialogue dialogue)
    {
        if (Time.unscaledTime < globalInteractionLockUntil)
            return;

        activeDialogue = dialogue;
        dialogueIndex = 0;
        duringTriggeredLines.Clear();

        Open();
        Next();
    }

    private IEnumerator TypeWriterEffectCoroutine (int lineIndex, DialogueContext context)
    {
        CurrentText.text = string.Empty;
        float delay = Mathf.Max(0.001f, 1f / Mathf.Max(1f, textSpeed));

        int half = fullText.Length / 2;
        bool duringTriggered = false;

        for (int i = 0; i < fullText.Length; i++)
        {
            if (!IsOpen) yield break;

            CurrentText.text += fullText[i];

            if (!duringTriggered && i == half)
            {
                duringTriggered = true;
                duringTriggeredLines.Add(lineIndex);
                activeDialogue.TriggerActions(lineIndex, TriggerTiming.During, context);
            }

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

    private void OnTypingComplete ()
    {
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

    private IEnumerator CloseThoughtAfterDelay ()
    {
        yield return new WaitForSeconds(thoughtsCloseDelay);
        Close();
    }

    private IEnumerator EnableNextDialogueAction ()
    {
        yield return null;

        if (nextDialogueAction?.action == null)
            yield break;

        nextDialogueAction.action.performed -= OnNextDialogue;
        nextDialogueAction.action.performed += OnNextDialogue;
        nextDialogueAction.action.Enable();
    }

    private IEnumerator CCloseSafelyNextFrame ()
    {
        yield return null;
        yield return new WaitForEndOfFrame();
        Close();
    }
}
