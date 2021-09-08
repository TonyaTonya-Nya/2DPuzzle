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

    protected void Start()
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
            if (CheckEventContition(eventPoint[i].condition) && eventPoint[i].autoStart)
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
        RunEvent();
    }

    public void RunEvent()
    {
        // 從最後面的事件開始向前，找到符合條件的事件點後執行
        for (int i = eventPoint.Count - 1; i >= 0; i--)
        {
            if (CheckEventContition(eventPoint[i].condition))
            {
                // 執行前關閉道具視窗
                FindObjectOfType<ItemList>().CloseItemList();
                StartCoroutine(RunEventCoroutine(eventPoint[i]));
                break;
            }
        }
    }

    private IEnumerator RunEventCoroutine(EventPoint eventPoint)
    {
        foreach (EventCommand command in eventPoint.commands)
        {
            // 獲取或失去物品
            PlayerData.Instance.GainItem(command.gainItemId);
            PlayerData.Instance.LoseItem(command.loseItemId);
            // 設置開關
            GameDatabase.Instance.SetSwitch(command.openSwitchId, true);
            GameDatabase.Instance.SetSwitch(command.closeSwitchId, false);
            // 顯示對話
            if (command.dialogue != "")
            {
                DialogueSystem.Instance.ShowDialouge(command.dialogue);
                yield return new WaitUntil(() => DialogueSystem.Instance.finish);
                yield return null;
            }
        }
        DialogueSystem.Instance.CloseDialouge();

    }

    public bool CheckEventContition(EventCondition condition)
    {
        // 開關檢查
        foreach (int id in condition.switchConditions.Keys)
        {
            if (!GameDatabase.Instance.GetSwitchState(id))
                return false;
        }
        // 物品檢查
        foreach (int id in condition.itemConditions.Keys)
        {
            if (!PlayerData.Instance.HasItem(id))
                return false;
        }
        return true;
    }

}
