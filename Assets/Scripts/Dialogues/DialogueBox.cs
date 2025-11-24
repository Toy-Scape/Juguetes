using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class DialogueBox : MonoBehaviour
{
    [SerializeField] private GameObject dialogueContent;
    [SerializeField] private GameObject thoughtContent;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private TextMeshProUGUI thoughtText;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private float textSpeed = 10f;
    [SerializeField] public UnityEvent onOpen;
    [SerializeField] public UnityEvent onClose;
    [SerializeField] private InputActionReference nextDialogueAction;
    [SerializeField] private PlayerInput playerInput; 
    [SerializeField] private float thoughtsCloseDelay = 2f;
    private Dialogue activeDialogue;
    private string fullText;
    private int dialogueIndex;
    private Coroutine typingCoroutine;
    public bool IsTyping { get; private set;}
    public bool IsOpen  { get => dialogueContent.activeInHierarchy || thoughtContent.activeInHierarchy; }
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


    public void Awake ()
    {
        Instance = this;
        Close();
    }

    public void Open ()
    {
        if (activeDialogue.Type == Dialogue.DialogueType.Normal)
        {
            dialogueContent.SetActive(true);
            thoughtContent.SetActive(false);
            playerInput.SwitchCurrentActionMap("Dialogue");

            StartCoroutine(EnableNextDialogueAction());
        }
        else if (activeDialogue.Type == Dialogue.DialogueType.Thought)
        {
            dialogueContent.SetActive(false);
            thoughtContent.SetActive(true);
            StartCoroutine(CloseThoughtAfterDelay());
        }

        onOpen?.Invoke();
    }

    public void Close ()
    {
        dialogueContent.SetActive(false);
        thoughtContent.SetActive(false);
        onClose?.Invoke();

        nextDialogueAction.action.performed -= OnNextDialogue;
        nextDialogueAction.action.Disable();

        StartCoroutine(ReenablePlayerNextFrame());
    }

     private void OnNextDialogue (InputAction.CallbackContext ctx)
    {
        Next();
    }

    public void Next ()
    {
        if (!activeDialogue)
        {
            return;
        }

        if (IsTyping)
        {
            FastForward();
            return;
        }

        if (dialogueIndex >= activeDialogue.Lines.Count)
        {
            Close();
            return;
        }

        var line = activeDialogue.Lines[dialogueIndex];
        nameText.text = line.GetCharacterName();
        fullText = line.Text;
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
        CurrentText.text = fullText;
        IsTyping = false;
    }

    public void StartDialogue (Dialogue dialogue)
    {
        activeDialogue = dialogue;
        dialogueIndex = 0;
        Open();
        Next();
    }

    private IEnumerator TypeWriterEffectCoroutine ()
    {
        CurrentText.text = string.Empty;
        float delay = 1f / textSpeed;

        foreach (char c in fullText)
        {
            if (!IsOpen) yield break;
            CurrentText.text += c;
            yield return new WaitForSeconds(delay);
        }

        IsTyping = false;
    }

    private IEnumerator ReenablePlayerNextFrame ()
    {
        yield return null;
        playerInput.SwitchCurrentActionMap("Player");
    }

    private IEnumerator CloseThoughtAfterDelay ()
    {
        yield return new WaitForSeconds(thoughtsCloseDelay);
        Close();
    }

    private IEnumerator EnableNextDialogueAction ()
    {
        yield return null;
        nextDialogueAction.action.performed += OnNextDialogue;
        nextDialogueAction.action.Enable();
    }
}