using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartManager : MonoBehaviour
{
    public void StartGame()
    {
        if (PlayerData.Instance != null)
        {
            PlayerData.Instance.Clear();
        }
        SceneManager.LoadScene(1);
    }

    public void EndGame()
    {
        Application.Quit();
    }
}
