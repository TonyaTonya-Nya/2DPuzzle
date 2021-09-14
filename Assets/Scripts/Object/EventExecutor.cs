using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 負責執行事件的指令
/// </summary>
public class EventExcutor : MonoBehaviour
{
    private static EventExcutor instance;
    public static EventExcutor Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject gameObject = new GameObject();
                gameObject.AddComponent<EventExcutor>();
                instance = gameObject.GetComponent<EventExcutor>();
            }
            return instance;
        }
    }

    private EventObject target;

    private EventCommandList commands;

    public bool IsRunning { get; private set; }

    private int index;

    public EventCommand NextCommand
    {
        get
        {
            if (commands != null && index < commands.Count)
                return commands[index];
            return null;
        }
    }

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
        commands = null;
        index = 0;
    }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(RunEvent());
    }

    public void Register(EventObject caller, EventCommandList newCommands)
    {
        if (IsRunning)
            return;
        target = caller;
        commands = newCommands;
        index = 0;
    }

    private IEnumerator RunEvent()
    {
        yield return null;
        while (true)
        {
            if (commands != null)
            {
                IsRunning = true;
                // 執行前關閉道具視窗
                FindObjectOfType<ItemList>().CloseItemList();
                foreach (EventCommand eventCommand in commands)
                {
                    // 對象已經消失，不繼續處理
                    if (target == null)
                    {
                        Clear();
                        break;
                    }
                    index++;
                    eventCommand.Register(target);
                    yield return StartCoroutine(eventCommand.Run());
                }
                IsRunning = false;
                Clear();
            }
            yield return null;
        }
    }

    private void Clear()
    {
        index = 0;
        commands = null;
        target = null;
    }
}
