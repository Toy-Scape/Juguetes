using System;
using System.Collections;
using System.Data.Common;
using NUnit.Framework;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class DialogueBox : MonoBehaviour
{
    [SerializeField] GameObject content;
    [SerializeField] TextMeshProUGUI dialogueText;
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] float textSpeed = 10f;
    [SerializeField] UnityEvent onOpen;
    [SerializeField] UnityEvent onClose;
    private Dialogue activeDialogue;
    private string fullText;
    private int dialogueIndex;
    public bool IsTyping { get; private set;}
    public bool IsOpen  { get => content.activeInHierarchy;}
    public static DialogueBox Instance { get; private set;}

    public void Awake()
    {
        Instance = this;
        Close();
    }

    public void Open()
    {
        content.SetActive(true);
        onOpen?.Invoke();
    }

    public void Close()
    {
        content.SetActive(false);
        onClose?.Invoke();
    }

    public void Next()
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

        if(dialogueIndex >= activeDialogue.lines.Count)
        {
            Close();
            return;
        }

        var line = activeDialogue.lines[dialogueIndex];
        nameText.text = line.GetCharacterName();
        fullText = line.text;
        IsTyping = true;
        StartCoroutine(TypeWriterEffectCoroutine());

        dialogueIndex++;
    }

    public void FastForward()
    {
        StopAllCoroutines();
        dialogueText.text = fullText;
        IsTyping = false;
    }

    public void StartDialogue(Dialogue dialogue)
    {
        activeDialogue = dialogue;
        dialogueIndex = 0;
        Open();
        Next();
    }

    private IEnumerator TypeWriterEffectCoroutine()
    {
        dialogueText.text = string.Empty;
        float delay = 1f / textSpeed;

        foreach(char c in fullText)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(delay);
        }

        IsTyping = false;
    }
}
