using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RotaryHeart.Lib.SerializableDictionary;

[System.Serializable]
public class EventSwitches
{
    public int id;
    public string name;
}

[System.Serializable]
public class IntSwitchDhictionary : SerializableDictionaryBase<int, EventSwitches> { }

[CreateAssetMenu(fileName = "SwitchDatabase", menuName = "EditorDatabase/SwitchDatabase")]
public class SwitchDatabase : ScriptableObject
{
    public IntSwitchDhictionary switches;
}
