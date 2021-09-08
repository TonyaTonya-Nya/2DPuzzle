using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RotaryHeart.Lib.SerializableDictionary;

[System.Serializable]
public class IntDictionary : SerializableDictionaryBase<int, int> { }

[System.Serializable]
public class IntBoolDictionary : SerializableDictionaryBase<int, bool> { }

[System.Serializable]
public class IntItemDhictionary : SerializableDictionaryBase<int, Item> { }

[System.Serializable]
public class IntStringDhictionary : SerializableDictionaryBase<int, string> { }

//[System.Serializable]
//public class EventSwitch
//{
//    // 開關的ID
//    public int id;
//    // 開關的名字
//    public string name;
//    // 是否啟動
//    public bool opened;
//}

[System.Serializable]
public class EventCondition
{
    // 事件ID => 開關
    public IntBoolDictionary switchConditions;
    // 物品ID => 數量
    public IntDictionary itemConditions;
}

[System.Serializable]
public class EventCommand
{
    public string dialogue;
    public int gainItemId;
    public int loseItemId;
    public int openSwitchId;
    public int closeSwitchId;
}

[System.Serializable]
public class EventPoint
{
    // 啟動條件
    public EventCondition condition;
    // 是否在滿足條件後立刻自動執行
    public bool autoStart;
    // 對話
    public List<EventCommand> commands;
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
    public string name;
    public Sprite sprite;
    public string description;
    public List<EventPoint> clickEvent;
    public bool canMix;
    public int mixTarget;
    public List<EventPoint> mixEvent;
}
