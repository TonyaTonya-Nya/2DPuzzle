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


    /// <summary>
    /// 持有物品編號
    /// </summary>
    private List<int> items = new List<int>();

    public bool HasItem(int id)
    {
        return items.Contains(id);
    }

    public void GainItem(int id)
    {
        if (!HasItem(id))
            items.Add(id);
    }

    public int ItemCount(int id)
    {
        if (HasItem(id))
            return items[id];
        return 0;
    }

}
