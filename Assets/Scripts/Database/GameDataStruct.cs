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
public class StringBoolDictionary : SerializableDictionaryBase<string, bool> { }

[System.Serializable]
public class StringIntDictionary : SerializableDictionaryBase<string, int> { }

[System.Serializable]
public class StringFloatDictionary : SerializableDictionaryBase<string, float> { }

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

public enum Direction
{
    Right,
    Left
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

[System.Serializable]
public class ItemMixSet
{
    public int item1Id;
    public bool loseItem1 = true;
    public int item2Id;
    public bool loseItem2 = true;
    public int resultId;
    // 合成後要有的事件
    public EventCommandList commands;
}

[System.Serializable]
public class Item
{
    public int id;
    public string itemName;
    public Sprite sprite;
    public string description;
    public List<EventPoint> clickEvent;
}
