using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

}
