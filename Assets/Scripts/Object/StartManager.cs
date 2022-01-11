using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Net;
using UnityEngine.Networking;
using UnityEngine.Events;
using System;

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

    public void Post_Btn()
    {
        StartCoroutine(PlayerData.Instance.Post_Http());
    }

    public void Get_Btn()
    {

        Debug.Log("GET");
        StartCoroutine(Search());
    }

    public IEnumerator Search()
    {
        yield return StartCoroutine(PlayerData.Instance.Get_Http());

        for (int i = 0; i < PlayerData.Instance.RankData_List.Count; i++)
        {
            for (int j = i; j < PlayerData.Instance.RankData_List.Count; j++)
            {
                String dateTime_i = PlayerData.Instance.RankData_List[i].time;
                int time_i = Int32.Parse(dateTime_i.Split(':')[0]) * 60 + Int32.Parse(dateTime_i.Split(':')[1]);

                String dateTime_j = PlayerData.Instance.RankData_List[j].time;
                int time_j = Int32.Parse(dateTime_j.Split(':')[0]) * 60 + Int32.Parse(dateTime_j.Split(':')[1]);

                if(time_i > time_j)
                {
                    string temp_name= PlayerData.Instance.RankData_List[i].name;
                    string temp_time = PlayerData.Instance.RankData_List[i].time;

                    PlayerData.Instance.RankData_List[i].name = PlayerData.Instance.RankData_List[j].name;
                    PlayerData.Instance.RankData_List[i].time = PlayerData.Instance.RankData_List[j].time;

                    PlayerData.Instance.RankData_List[j].name = temp_name;
                    PlayerData.Instance.RankData_List[j].time = temp_time;
                }
            }
        }


        if (PlayerData.Instance.isGetSuccess)
        {
            UpdateScroll();
        }
    }


    public GameObject scroll;
    public Text field;

    void UpdateScroll()
    {
        ClearObjChild(scroll);

        for (int i = 0; i < PlayerData.Instance.RankData_List.Count; i++)
        {
           
            RankData temp = PlayerData.Instance.RankData_List[i];
            Text item= Instantiate(field);
            item.gameObject.SetActive(true);
            item.transform.GetComponent<Text>().text = temp.name +"\t"+ temp.time;
            Debug.Log(temp.name + temp.time);
            item.transform.SetParent(scroll.transform, false);
            Debug.Log(temp.name);
        }

    }

    public void ClearObjChild(GameObject scroll)
    {
        for (int i = 0; i < scroll.transform.childCount; i++)
        {
            GameObject obj = scroll.transform.GetChild(i).gameObject;
            Destroy(obj);
        }
    }

}
