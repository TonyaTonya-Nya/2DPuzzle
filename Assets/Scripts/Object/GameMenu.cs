using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMenu : MonoBehaviour
{
    public GameObject menu;

    public void Open()
    {
        menu.SetActive(true);
    }

    public void Close()
    {
        menu.SetActive(false);
    }

    public void BackToMain()
    {
        SceneManager.LoadScene(0);
    }
}
