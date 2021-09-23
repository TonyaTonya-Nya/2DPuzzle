using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogueSystem : MonoBehaviour
{
    private static DialogueSystem instance;
    public static DialogueSystem Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject gameObject = new GameObject();
                gameObject.AddComponent<DialogueSystem>();
                instance = gameObject.GetComponent<DialogueSystem>();
            }
            return instance;
        }
    }

    public GameObject dialogueBackground;
    public Text dialogueText;
    public bool finish;

    private IEnumerator coroutine;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        if (instance != this)
            Destroy(gameObject);
    }

    private void Start()
    {
        dialogueBackground.gameObject.SetActive(false);
    }

    public void ShowDialouge(string content)
    {
        finish = false;
        if (coroutine != null && coroutine.Current != null)
            StopCoroutine(coroutine);
        dialogueBackground.gameObject.SetActive(true);
        coroutine = ShowDialougeCoroutine(content);
        StartCoroutine(coroutine);
    }

    public IEnumerator ShowDialougeCoroutine(string content)
    {
        dialogueText.text = content;
        yield return null;
        while (!Input.GetMouseButtonUp(0))
            yield return null;
        finish = true;
    }

    public bool IsShowingDialogue()
    {
        return dialogueBackground.gameObject.activeSelf;
    }

    public void CloseDialouge()
    {
        dialogueBackground.gameObject.SetActive(false);
        if (coroutine != null && coroutine.Current != null)
            StopCoroutine(coroutine);
        finish = true;
    }
}
