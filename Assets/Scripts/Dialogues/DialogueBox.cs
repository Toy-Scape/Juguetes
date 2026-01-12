using System;
using System.Collections;
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

        if (player == null)
        {
            var controller = FindFirstObjectByType<PlayerController>();
            if (controller != null)
                player = controller.gameObject;
        }

        Instance = this;
        // asegurar estado inicial
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

    /// <summary>
    /// Abre el diálogo activo. No asume que el activeDialogue sea nulo.
    /// </summary>
    public void Open()
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
        else if (activeDialogue.Type == Dialogue.DialogueType.Thought)
        {
            if (dialogueContent) dialogueContent.SetActive(false);
            if (thoughtContent) thoughtContent.SetActive(true);

        onOpen?.Invoke();
    }

    /// <summary>
    /// Cierra el diálogo y su UI. Usa un cierre "seguro" que limpia listeners.
    /// </summary>
    public void Close()
    {
        // cancelar cierres pendientes
        if (thoughtCloseCoroutine != null)
        {
            StopCoroutine(thoughtCloseCoroutine);
            thoughtCloseCoroutine = null;
        }

        if (dialogueContent) dialogueContent.SetActive(false);
        if (thoughtContent) thoughtContent.SetActive(false);

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

    /// <summary>
    /// Versión usada en Awake para evitar ejecutar eventos y corutinas.
    /// </summary>
    private void CloseImmediate()
    {
        if (dialogueContent) dialogueContent.SetActive(false);
        if (thoughtContent) thoughtContent.SetActive(false);

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

        // si ya no quedan líneas -> cerrar
        if (dialogueIndex >= activeDialogue.Lines.Count)
        {
            StartCoroutine(CloseSafelyNextFrame());
            return;
        }

        var line = activeDialogue.Lines[dialogueIndex];

        nameText.text = line.GetCharacterName();

        // Fetch text from Localization System
        if (Localization.LocalizationManager.Instance != null)
        {
            fullText = Localization.LocalizationManager.Instance.GetLocalizedValue(line.Key);
        }
        else
        {
            fullText = line.Key; // Fallback to key if no manager
        }
        currentSpeaker = CharacterManager.Instance.GetModel(line.Character.CharacterId);

        var context = new DialogueContext(player, currentSpeaker);

        activeDialogue.TriggerActions(dialogueIndex, TriggerTiming.OnStart, context);

        IsTyping = true;
        typingCoroutine = StartCoroutine(TypeWriterEffectCoroutine());

        dialogueIndex++;
    }

    public void FastForward()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        // mostrar todo el texto inmediatamente
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

        activeDialogue = dialogue;
        dialogueIndex = 0;
        Open();
        Next();
    }

    private IEnumerator TypeWriterEffectCoroutine()
    {
        if (CurrentText == null)
            yield break;

        CurrentText.text = string.Empty;
        float delay = Mathf.Max(0.001f, 1f / Mathf.Max(1f, textSpeed));

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

            yield return new WaitForSeconds(delay);
        }

        if (!duringTriggered)
        {
            duringTriggeredLines.Add(lineIndex);
            activeDialogue.TriggerActions(lineIndex, TriggerTiming.During, context);
        }

        IsTyping = false;

        // Notificar que hemos terminado de escribir
        OnTypingComplete();
    }

    /// <summary>
    /// Cuando se acaba de escribir un texto (o se fastforwardea), aquí se centraliza la lógica
    /// que dependa del final del tipeo: p.ej. iniciar el cierre automático de pensamientos.
    /// </summary>
    private void OnTypingComplete()
    {
        // Si es un pensamiento, lanzar la corutina de cierre (asegurando no duplicarla)
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

    private IEnumerator CloseThoughtAfterDelay()
    {
        yield return new WaitForSeconds(thoughtsCloseDelay);
        Close();
    }

    private IEnumerator EnableNextDialogueAction ()
    {
        // esperar un frame para evitar race conditions con el InputSystem
        yield return null;

        if (nextDialogueAction?.action == null)
            yield break;

        // asegurarse de doble-subscribe
        nextDialogueAction.action.performed -= OnNextDialogue;
        nextDialogueAction.action.performed += OnNextDialogue;
    }

    private IEnumerator CloseSafelyNextFrame()
    {
        // Esperar a que termine el ciclo interno del Input System
        yield return null;

        // Espera extra muy corta para gamepads (opcional pero muy efectivo)
        yield return new WaitForEndOfFrame();

        Close();
    }
}