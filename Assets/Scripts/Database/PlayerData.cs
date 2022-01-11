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

public class RankData
{
    public string id;
    public string name;
    public string time;
}


public class PlayerData
{
    private static PlayerData instance;
    public static PlayerData Instance
    {
        get
        {
            if (instance == null)
                instance = new PlayerData();
            return instance;
        }
    }

    public string playerName = "神華";

    public string clearTime = "0";


    /// <summary>
    /// 持有物品編號
    /// </summary>
    public List<int> items { get; private set; } = new List<int>();

    public void Clear()
    {
        items = null;
        instance = null;
    }

    public bool HasItem(int id)
    {
        return items.Contains(id);
    }

    public void GainItem(int id)
    {
        if (!HasItem(id) && GameDatabase.Instance.ItemDB.ContainsKey(id))
            items.Add(id);
    }

    public void LoseItem(int id)
    {
        if (HasItem(id))
            items.Remove(id);
    }

    public int ItemCount(int id)
    {
        if (HasItem(id))
            return items[items.IndexOf(id)];
        return 0;
    }



    bool isNetSuccess = false;

    private string googleSheetUrl = "https://script.google.com/macros/s/AKfycbyuUmcK8b6hoXtIExQ8zPBXmh2IrmShX8JsEaMlShzOCLYL-xQ6T_EPEgclccMxS5K9uw/exec";
    public string GoogleSheetUrl
    {
        get => googleSheetUrl;
        set => googleSheetUrl = value;
    }

    public IEnumerator Post_Http()
    {
        isNetSuccess = false;
        int count = 0;
        while (count < 3)
        {
            WWWForm form = new WWWForm();
            form.AddField("method", "write");
            form.AddField("name", playerName);
            form.AddField("time", clearTime);


            UnityWebRequest webRequest = UnityWebRequest.Post(GoogleSheetUrl, form);
            yield return webRequest.SendWebRequest();

            if (webRequest.isHttpError || webRequest.isNetworkError)
            {
                Debug.Log(webRequest.isHttpError);
                Debug.Log(webRequest.isNetworkError);
                count++;
                yield return new WaitForSeconds((int)5);
            }
            else
            {
                isNetSuccess = true;
                break;
            }
        }
    }

    //Http讀取用
    public List<RankData> RankData_List= new List<RankData>();
    public bool isGetSuccess = false;

    public IEnumerator Get_Http()
    {

        int count = 0;
        isGetSuccess = false;

        //連線嘗試次數
        while (count < (int)3)
        {
            WWWForm form = new WWWForm();
            form.AddField("method", "read");

            UnityWebRequest webRequest = UnityWebRequest.Post(GoogleSheetUrl, form);

            UnityWebRequestAsyncOperation requestStatus = webRequest.SendWebRequest();
            while (!requestStatus.isDone)
            {
                yield return new WaitForSeconds(0.5f);
            }

            if (webRequest.isHttpError || webRequest.isNetworkError)
            {
                Debug.Log(webRequest.isHttpError);
                Debug.Log(GoogleSheetUrl);
                count++;
                yield return new WaitForSeconds((int)5);
            }
            else
            {
                string jsonStr = webRequest.downloadHandler.text;
                RankData_List = JsonConvert.DeserializeObject<List<RankData>>(jsonStr);
                isGetSuccess = true;
                break;
            }
        }
    }



}
