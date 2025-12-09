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

    private Dialogue activeDialogue;
    private string fullText;
    private int dialogueIndex;
    private static float globalInteractionLockUntil = 0f;
    private const float interactionLockTime = 0.25f;
    private Coroutine typingCoroutine;
    private Coroutine thoughtCloseCoroutine;

    public bool IsTyping { get; private set;}

    public bool IsOpen  
    { 
        get
        {
            return dialogueContent.activeInHierarchy || thoughtContent.activeInHierarchy;
        }  
    }

    public static DialogueBox Instance { get; private set;}

    private TextMeshProUGUI CurrentText
    {
        get
        {
            return activeDialogue.Type == Dialogue.DialogueType.Normal
                ? dialogueText
                : thoughtText;
        }
    }


    private void Awake ()
    {
        if (Instance != null && Instance != this)
        {
        Destroy(gameObject);
        return;
        }
        Instance = this;
        // asegurar estado inicial
        CloseImmediate();
    }

    private void OnDestroy ()
    {
        // limpieza por seguridad
        if (nextDialogueAction != null && nextDialogueAction.action != null)
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
            if (dialogueContent) dialogueContent.SetActive(true);
            if (thoughtContent) thoughtContent.SetActive(false);

            // centralizamos cambios de action map en InputMapManager para evitar asserts
            if (InputMapManager.Instance != null)
                InputMapManager.Instance.SwitchToActionMapSafe("Dialogue");

            StartCoroutine(EnableNextDialogueAction());
        }
        else if (activeDialogue.Type == Dialogue.DialogueType.Thought)
        {
            if (dialogueContent) dialogueContent.SetActive(false);
            if (thoughtContent) thoughtContent.SetActive(true);

            // Para pensamientos NO cambiamos el action map (son pasivos y se cierran solos).
            // Iniciamos el flujo de escribir el pensamiento; el cierre se disparará al terminar de escribir.
        }

        onOpen?.Invoke();
    }
    
    /// <summary>
    /// Cierra el diálogo y su UI. Usa un cierre "seguro" que limpia listeners.
    /// </summary>
    public void Close ()
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

        // devolver control al Player action map (via InputMapManager para evitar asserts)
        if (InputMapManager.Instance != null)
        InputMapManager.Instance.SwitchToActionMapSafe("Player");

        // cancelar tipeo si existe
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
    private void CloseImmediate ()
    {
        if (dialogueContent) dialogueContent.SetActive(false);
        if (thoughtContent) thoughtContent.SetActive(false);

        if (nextDialogueAction != null && nextDialogueAction.action != null)
        {
            nextDialogueAction.action.performed -= OnNextDialogue;
            nextDialogueAction.action.Disable();
        }

        IsTyping = false;
    }

     private void OnNextDialogue (InputAction.CallbackContext ctx)
    {
        Next();
    }

    public void Next ()
    {
        if (activeDialogue == null)
        return;

        // si está escribiendo: fastforward
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
        fullText = line.Text ?? string.Empty;
        IsTyping = true;
        typingCoroutine = StartCoroutine(TypeWriterEffectCoroutine());

        dialogueIndex++;
    }

    public void FastForward ()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        // mostrar todo el texto inmediatamente
        CurrentText.text = fullText;
        IsTyping = false;

        // asegurar que se dispare el final del tipeo (por ejemplo para pensamientos)
        OnTypingComplete();
    }

    public void StartDialogue (Dialogue dialogue)
    {
        if (Time.unscaledTime < globalInteractionLockUntil)
            return;

        activeDialogue = dialogue;
        dialogueIndex = 0;
        Open();
        Next();
    }

    private IEnumerator TypeWriterEffectCoroutine ()
    {
        if (CurrentText == null)
        yield break;

        CurrentText.text = string.Empty;
        float delay = Mathf.Max(0.001f, 1f / Mathf.Max(1f, textSpeed));

        foreach (char c in fullText)
        {
            if (!IsOpen) yield break;
            CurrentText.text += c;
            yield return new WaitForSeconds(delay);
        }

        IsTyping = false;

        // Notificar que hemos terminado de escribir
        OnTypingComplete();
    }

    /// <summary>
    /// Cuando se acaba de escribir un texto (o se fastforwardea), aquí se centraliza la lógica
    /// que dependa del final del tipeo: p.ej. iniciar el cierre automático de pensamientos.
    /// </summary>
    private void OnTypingComplete ()
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

    private IEnumerator CloseThoughtAfterDelay ()
    {
        yield return new WaitForSeconds(thoughtsCloseDelay);
        Close();
    }

    private IEnumerator ReenablePlayerNextFrame ()
    {
        yield return new WaitForSecondsRealtime(0.1f);
        if (InputMapManager.Instance != null)
            InputMapManager.Instance.SwitchToActionMap("Player");
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