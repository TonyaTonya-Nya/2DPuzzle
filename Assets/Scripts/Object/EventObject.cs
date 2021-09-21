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
    [SerializeField, HideInInspector]
    public string guid;

    // 玩家點擊後，要觸發的事件點
    public List<EventPoint> eventPoint;

    // 物件要與玩家在這個距離內才可被點擊
    public float xDistance;
    // 物件正向是哪個方向
    public Direction whichIsPositive;

    public bool IsRunning { get; set; }

    public bool Clicked { get; set; }

    public bool Triggered { get; set; }

    public EventCommand NextCommand { get; private set; }

    private void Awake()
    {
        IsRunning = false;
        Clicked = false;
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
        // 指定距離內才有效
        float distance = Mathf.Abs(transform.position.x - GameObject.FindGameObjectWithTag("Player").transform.position.x);
        if (distance <= xDistance)
            Clicked = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 執行對話中，不可操作
        if (DialogueSystem.Instance.IsShowingDialogue())
            return;
        Triggered = true;
    }

    public IEnumerator RunEvent()
    {
        while (true)
        {
            // 從最後面的事件開始向前，找到符合條件的事件點後執行
            for (int i = eventPoint.Count - 1; i >= 0; i--)
            {
                // 不應該有這段但是不知道為什麼會發生所以加上這段保險
                if (i < 0 || i >= eventPoint.Count)
                    break;
                // 事件頁條件檢查
                if (CheckEventContition(eventPoint[i].condition) && !IsRunning)
                {
                    // 自動執行
                    bool run = eventPoint[i].triggerType == EventTriggerType.Auto;
                    // 點擊執行
                    run = run || (eventPoint[i].triggerType == EventTriggerType.Click && Clicked);
                    // 碰觸執行
                    run = run || (eventPoint[i].triggerType == EventTriggerType.Touch && Triggered);
                    // 已經有其他事件在執行，不可執行
                    run = run && !EventExcutor.Instance.IsRunning;

                    // 啟動檢查
                    if (run)
                        EventExcutor.Instance.Register(this, eventPoint[i].commands);
                    // 事件頁條件被滿足時，不會再往下看
                    break;
                }
                yield return null;
            }
            Clicked = false;
            Triggered = false;
            yield return null;

        }
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

#if UNITY_EDITOR
    public void OnDrawGizmosSelected()
    {
        Vector3 vector3 = transform.position;
        Vector3 player = GameObject.FindGameObjectWithTag("Player").transform.position;
        Gizmos.color = Color.green;
        Gizmos.DrawLine(new Vector3(vector3.x, vector3.y + 0.5f, vector3.z), new Vector3(vector3.x, vector3.y - 0.5f, vector3.z));
        Gizmos.DrawLine(vector3, new Vector3(player.x, vector3.y, vector3.z));
        Gizmos.DrawLine(new Vector3(player.x, vector3.y + 0.5f, vector3.z), new Vector3(player.x, vector3.y - 0.5f, vector3.z));
        Gizmos.color = Color.white;
        Color origin = Handles.color;
        Handles.color = Color.green;
        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.green;
        style.fontSize = 24;
        Handles.Label(new Vector3((player.x - vector3.x) / 2 + vector3.x, vector3.y + 0.25f, vector3.z), (player.x - vector3.x).ToString(), style);
        Handles.color = origin;
    }
#endif

}
