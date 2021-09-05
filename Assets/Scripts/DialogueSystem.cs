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

    public Text dialogueText;
    public bool finish;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        if (instance != this)
            Destroy(gameObject);
    }

    private void Start()
    {
        dialogueText.gameObject.SetActive(false);
    }

    public void ShowDialouge(string content)
    {
        finish = false;
        dialogueText.gameObject.SetActive(true);
        StartCoroutine(ShowDialougeCoroutine(content));
    }

    public IEnumerator ShowDialougeCoroutine(string content)
    {
        dialogueText.text = content;
        while (!Input.GetMouseButtonDown(0))
            yield return null;
        finish = true;
    }

    public void CloseDialouge()
    {
        dialogueText.gameObject.SetActive(false);
    }
}
