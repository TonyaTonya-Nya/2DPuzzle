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

    public bool IsRunningEvent { get; private set; }

    private void Awake()
    {
        IsRunningEvent = false;
    }

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
        // 永遠檢查
        //while (true)
        //{
            // 從最後面的事件開始向前，找到符合條件的事件點後執行
            for (int i = eventPoint.Count - 1; i >= 0; i--)
            {
                if (CheckEventContition(eventPoint[i].condition) && eventPoint[i].autoStart)
                {
                    StartCoroutine(RunEventCoroutine(eventPoint[i]));
                    break;
                }
            }
            yield return null;
        //}
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
                if (!command.showSelection)
                {
                    yield return new WaitUntil(() => DialogueSystem.Instance.finish);
                    yield return null;
                }
            }
            // 選項
            if (command.showSelection)
            {
                SelectionBox.main.ShowSelectionBox(command.selectionText1, command.selectionText2);
                yield return new WaitUntil(() => !SelectionBox.main.IsWaiting());
                int index = SelectionBox.main.GetResult();
                // 依據選項繼續執行
                // 創建一個暫時的物件來執行選項後的動作
                EventObject temp = Instantiate(this, new Vector3(999, 999, 999), Quaternion.identity);
                if (index == 0)
                    temp.eventPoint = new List<EventPoint>(command.firstSelectionEvent);
                else
                    temp.eventPoint = new List<EventPoint>(command.secondSelectionEvent);
                temp.RunEvent();
                while (temp.IsRunningEvent)
                    yield return null;
                Destroy(temp.gameObject);
            }
        }
        DialogueSystem.Instance.CloseDialouge();
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
