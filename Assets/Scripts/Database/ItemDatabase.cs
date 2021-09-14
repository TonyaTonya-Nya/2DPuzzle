using RotaryHeart.Lib.SerializableDictionary;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class IntItemDhictionary : SerializableDictionaryBase<int, Item> { }

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Database/ItemDatabase")]
[System.Serializable]
public class ItemDatabase : ScriptableObject
{
    public IntItemDhictionary items;
}
