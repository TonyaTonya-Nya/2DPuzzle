using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RotaryHeart.Lib.SerializableDictionary;
using System;

[System.Serializable]
public class IntDictionary : SerializableDictionaryBase<int, int> { }

[System.Serializable]
public class IntBoolDictionary : SerializableDictionaryBase<int, bool> { }

[System.Serializable]
public class IntStringDhictionary : SerializableDictionaryBase<int, string> { }

[System.Serializable]
public class EventCondition
{
    // 事件ID => 開關
    public IntBoolDictionary switchConditions;
    // 物品ID => 數量
    public IntDictionary itemConditions;
}

public enum EventTriggerType
{
    // 點擊碰撞盒後觸發
    Click,
    // 碰到碰撞盒後觸發
    Touch,
    // 自動觸發
    Auto
}

[System.Serializable]
public class EventPoint
{
    // 執行方法
    public EventTriggerType triggerType;
    // 啟動條件
    public EventCondition condition = new EventCondition();
    // 指令
    public EventCommandList commands = new EventCommandList();
}

public class Dialogue
{
    public int id;
    // 下一句對話的ID
    public int nextId;
    // 內容
    public string content;
}


[System.Serializable]
public class Item
{
    public int id;
    public string itemName;
    public Sprite sprite;
    public string description;
    public List<EventPoint> clickEvent;
    public bool canMix;
    public int mixTarget;
    public List<EventPoint> mixEvent;
}
