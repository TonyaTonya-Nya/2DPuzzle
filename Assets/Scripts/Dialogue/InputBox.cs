using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputBox : MonoBehaviour
{
    public static InputBox main;

    public GameObject inputBox;
    // 輸入框
    public InputField inputField;

    //玩家輸入的字串
    private string resultInput;
    private bool waiting;

    private void Awake()
    {
        main = this;
        inputField.onEndEdit.AddListener(s => Input(s));
    }

    public void ShowInputBox()
    {
        waiting = true;
        inputBox.SetActive(true);
    }

    public void Input(string input)
    {
        waiting = false;
        resultInput = input;
        inputBox.SetActive(false);
    }

    public bool IsWaiting()
    {
        return waiting;
    }

    public string GetResult()
    {
        return resultInput;
    }
}
