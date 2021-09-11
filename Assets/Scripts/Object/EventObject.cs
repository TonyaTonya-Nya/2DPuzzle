using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 地圖上可互動的事件
/// </summary>
public class EventObject : MonoBehaviour
{
    // 物件的ID，由編輯器自動設定
    public int Id { get; private set; }

    // 玩家點擊後，要觸發的事件點
    public List<EventPoint> eventPoint;

    public bool IsRunningEvent { get; private set; }

    private void Awake()
    {
        IsRunningEvent = false;
    }

    protected void Start()
    {
        //StartCoroutine(AutoEventCoroutine());
    }

    private IEnumerator AutoEventCoroutine()
    {
        // 要讓所有人初始化完畢後才執行
        //yield return null;
        //yield return null;
        //yield return null;
        // 檢查是否有auto start的事件
        // 永遠檢查
        //while (true)
        //{
        // 從最後面的事件開始向前，找到符合條件的事件點後執行
        //for (int i = eventPoint.Count - 1; i >= 0; i--)
        //{
        //    if (CheckEventContition(eventPoint[i].condition) && eventPoint[i].autoStart)
        //    {
        //        StartCoroutine(RunEventCoroutine(eventPoint[i]));
        //        break;
        //    }
        //}
        //yield return null;
        //}
        yield return null;
    }

    /// <summary>
    /// 滑鼠點擊互動
    /// </summary>
    private void OnMouseUpAsButton()
    {
        // 執行對話中，不可操作
        if (DialogueSystem.Instance.IsShowingDialogue())
            return;

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
        IsRunningEvent = true;
        foreach (EventCommand eventCommand in eventPoint.commands)
            yield return eventCommand.Run();
        IsRunningEvent = false;
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
        if (IsRunningEvent)
            return false;
        return true;
    }

}
