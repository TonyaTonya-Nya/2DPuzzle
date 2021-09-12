using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using UnityEditor;

public class GameDatabase : MonoBehaviour
{
    // 事件點的開關資料庫
    public Dictionary<int, bool> EventSwitchDB { get; private set; } = new Dictionary<int, bool>();

    [SerializeField]
    // 物品資料庫來源
    private ItemDatabase itemDatabase;
    // 物品資料庫
    private Dictionary<int, Item> itemDB = new Dictionary<int, Item>();
    public Dictionary<int, Item> ItemDB
    {
        get
        {
            if (itemDB.Count != itemDatabase.items.Count)
            {
                foreach (KeyValuePair<int, Item> pair in itemDatabase.items)
                    itemDB[pair.Key] = pair.Value;
            }
            return itemDB;
        }
        private set
        {
            itemDB = value;
        }
    }

    public static object syncRoot = new object();

    private static GameDatabase instance;
    public static GameDatabase Instance
    {
        get
        {
            if (instance == null)
            {
                lock (syncRoot)
                {
                    if (instance == null)
                        instance = FindObjectOfType<GameDatabase>();
                }
            }
            return instance;
        }
    }

    private void Awake()
    {
        if (instance != null && instance != this)
            Destroy(gameObject);
        ParseDatabase();
    }

    /// <summary>
    /// 解析資料庫，存入Dictionary
    /// </summary>
    private void ParseDatabase()
    {
        // 物品資料庫
        foreach (KeyValuePair<int, Item> pair in itemDatabase.items)
            ItemDB[pair.Key] = pair.Value;
    }

    /// <summary>
    /// 設定開關
    /// </summary>
    /// <param name="name">開關名稱</param>
    /// <param name="open">true: 開啟，false: 關閉</param>
    public void SetSwitch(int id, bool open)
    {
        EventSwitchDB[id] = open;
    }

    public bool GetSwitchState(int id)
    {
        if (EventSwitchDB.ContainsKey(id))
            return EventSwitchDB[id];
        return false;
    }
}