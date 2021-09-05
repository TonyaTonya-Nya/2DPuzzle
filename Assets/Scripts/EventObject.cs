using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// 地圖上可互動的事件
/// </summary>
public class EventObject : MonoBehaviour
{
    // 玩家點擊後，要觸發的事件點
    public List<EventPoint> eventPoint;
    // 事件點的條件
    public List<EventCondition> eventConditions;

    private void Start()
    {
        StartCoroutine(AutoEventCoroutine());
    }

    private IEnumerator AutoEventCoroutine()
    {
        // 要讓所有人初始化完畢後才執行
        yield return null;
        yield return null;
        yield return null;
        // 檢查是否有auto start的事件
        // 從最後面的事件開始向前，找到符合條件的事件點後執行
        for (int i = eventPoint.Count - 1; i >= 0; i--)
        {
            if (CheckEventContition(i) && eventPoint[i].autoStart)
            {
                StartCoroutine(RunEventCoroutine(eventPoint[i]));
                break;
            }
        }
    }

    /// <summary>
    /// 滑鼠點擊互動
    /// </summary>
    private void OnMouseUpAsButton()
    {
        // 從最後面的事件開始向前，找到符合條件的事件點後執行
        for (int i = eventPoint.Count - 1; i >= 0; i--)
        {
            if (CheckEventContition(i))
            {
                StartCoroutine(RunEventCoroutine(eventPoint[i]));
                break;
            }
        }
    }

    private IEnumerator RunEventCoroutine(EventPoint eventPoint)
    {
        int dialogueId = eventPoint.dialogueId;
        while (dialogueId != -1)
        {
            string content = GameDatabase.Instance.Dialogues[dialogueId].content;
            DialogueSystem.Instance.ShowDialouge(content);
            yield return new WaitUntil(() => DialogueSystem.Instance.finish);
            yield return null;
            dialogueId = GameDatabase.Instance.Dialogues[dialogueId].nextId;
        }
        DialogueSystem.Instance.CloseDialouge();
    }

    private bool CheckEventContition(int index)
    {
        if (index >= eventConditions.Count)
            return true;
        EventCondition condition = eventConditions[index];
        // 開關檢查
        foreach (int id in condition.switchConditions.Keys)
        {
            EventSwitch eventSwitch = condition.switchConditions[id];
            if (!(GameDatabase.Instance.EventSwitches.ContainsKey(eventSwitch.id) && GameDatabase.Instance.EventSwitches[id].opened))
                return false;
        }
        // 物品檢查
        foreach (int id in condition.itemConditions.Keys)
        {
            if (PlayerData.Instance.ItemCount(id) < condition.itemConditions[id])
                return false;
        }
        return true;
    }

}
