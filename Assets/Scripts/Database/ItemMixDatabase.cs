using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MixDatabase", menuName = "Database/MixDatabase")]
public class ItemMixDatabase : ScriptableObject, IList<ItemMixSet>
{
    public List<ItemMixSet> itemMixSets;

    public ItemMixSet this[int index] { get => ((IList<ItemMixSet>)itemMixSets)[index]; set => ((IList<ItemMixSet>)itemMixSets)[index] = value; }

    public int Count => ((ICollection<ItemMixSet>)itemMixSets).Count;

    public bool IsReadOnly => ((ICollection<ItemMixSet>)itemMixSets).IsReadOnly;

    public void Add(ItemMixSet item)
    {
        ((ICollection<ItemMixSet>)itemMixSets).Add(item);
    }

    public void Clear()
    {
        ((ICollection<ItemMixSet>)itemMixSets).Clear();
    }

    public bool Contains(ItemMixSet item)
    {
        return ((ICollection<ItemMixSet>)itemMixSets).Contains(item);
    }

    public void CopyTo(ItemMixSet[] array, int arrayIndex)
    {
        ((ICollection<ItemMixSet>)itemMixSets).CopyTo(array, arrayIndex);
    }

    public IEnumerator<ItemMixSet> GetEnumerator()
    {
        return ((IEnumerable<ItemMixSet>)itemMixSets).GetEnumerator();
    }

    public int IndexOf(ItemMixSet item)
    {
        return ((IList<ItemMixSet>)itemMixSets).IndexOf(item);
    }

    public void Insert(int index, ItemMixSet item)
    {
        ((IList<ItemMixSet>)itemMixSets).Insert(index, item);
    }

    public bool Remove(ItemMixSet item)
    {
        return ((ICollection<ItemMixSet>)itemMixSets).Remove(item);
    }

    public void RemoveAt(int index)
    {
        ((IList<ItemMixSet>)itemMixSets).RemoveAt(index);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)itemMixSets).GetEnumerator();
    }
}
