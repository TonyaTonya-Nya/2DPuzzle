using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SelectionBox : MonoBehaviour
{
    public static SelectionBox main;

    public GameObject selectionBox;
    // 按鈕
    public Button[] buttons;

    //玩家選擇的選項
    private int resultIndex;
    private bool waiting;

    private void Awake()
    {
        main = this;
    }

    public void ShowSelectionBox(string text1, string text2)
    {
        waiting = true;
        buttons[0].GetComponentInChildren<Text>().text = text1;
        if (text2 != "")
        {
            buttons[1].GetComponentInChildren<Text>().text = text2;
            buttons[1].gameObject.SetActive(true);
        }
        else
            buttons[1].gameObject.SetActive(false);
        selectionBox.SetActive(true);
    }

    public void Select(int index)
    {
        waiting = false;
        resultIndex = index;
        selectionBox.SetActive(false);
    }

    public bool IsWaiting()
    {
        return waiting;
    }

    public int GetResult()
    {
        return resultIndex;
    }

}
