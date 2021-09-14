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
    [SerializeField]
    public string guid;

    // 玩家點擊後，要觸發的事件點
    public List<EventPoint> eventPoint;

    public bool IsRunning { get; set; }

    public bool clicked;

    public bool triggered;

    public EventCommand NextCommand { get; private set; }

    private void Awake()
    {
        IsRunning = false;
        clicked = false;
    }

    protected void Start()
    {
        StartCoroutine(RunEvent());
    }

    /// <summary>
    /// 滑鼠點擊互動
    /// </summary>
    private void OnMouseUpAsButton()
    {
        // 執行對話中，不可操作
        if (DialogueSystem.Instance.IsShowingDialogue())
            return;
        clicked = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 執行對話中，不可操作
        if (DialogueSystem.Instance.IsShowingDialogue())
            return;
        triggered = true;
    }

    public IEnumerator RunEvent()
    {
        while (true)
        {
            // 從最後面的事件開始向前，找到符合條件的事件點後執行
            for (int i = eventPoint.Count - 1; i >= 0; i--)
            {
                // 事件頁條件檢查
                if (CheckEventContition(eventPoint[i].condition) && !IsRunning)
                {
                    // 自動執行
                    bool run = eventPoint[i].triggerType == EventTriggerType.Auto;
                    // 點擊執行
                    run = run || (eventPoint[i].triggerType == EventTriggerType.Click && clicked);
                    // 碰觸執行
                    run = run || (eventPoint[i].triggerType == EventTriggerType.Touch && triggered);
                    // 已經有其他事件在執行，不可執行
                    run = run && !EventExcutor.Instance.IsRunning;
                    // 啟動檢查
                    if (run)
                    {
                        EventExcutor.Instance.Register(this, eventPoint[i].commands);
                        break;
                    }
                    // 事件頁條件被滿足時，不會再往下看
                    break;
                }
                yield return null;
            }
            clicked = false;
            triggered = false;
            yield return null;

        }
    }

    //private IEnumerator RunEventCoroutine(EventPoint eventPoint)
    //{
    //    eventRunning = true;
    //    IsRunning = true;
    //    for (int i = 0; i < eventPoint.commands.Count; i++)
    //    {
    //        eventPoint.commands[i].Register(this);
    //        NextCommand = (i + 1) >= eventPoint.commands.Count ? null : eventPoint.commands[i + 1];
    //        yield return StartCoroutine(eventPoint.commands[i].Run());
    //    }
    //    IsRunning = false;
    //    eventRunning = false;
    //}

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
