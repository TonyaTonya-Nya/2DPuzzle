using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RotaryHeart.Lib.SerializableDictionary;

[System.Serializable]
public class EventSwitch
{
    public int id;
    // 是否啟動
    public bool opened;
}

[System.Serializable]
public class EventSwitchDictionary : SerializableDictionaryBase<int, EventSwitch> { }

[System.Serializable]
public class IntDictionary : SerializableDictionaryBase<int, int> { }

[System.Serializable]
public class EventCondition
{
    public int id;
    // 事件ID => 開關
    public EventSwitchDictionary switchConditions;
    // 物品ID => 數量
    public IntDictionary itemConditions;
}

[System.Serializable]
public class EventPoint
{
    public int id;
    // 事件點名稱
    public string name;
    // 啟動條件
    public int conditionId;
    // 是否在滿足條件後立刻自動執行
    public bool autoStart;
    // 對話
    public int dialogueId;
}

public class Dialogue
{
    public int id;
    // 下一句對話的ID
    public int nextId;
    // 內容
    public string content;
}