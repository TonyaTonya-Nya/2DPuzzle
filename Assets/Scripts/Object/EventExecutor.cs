using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 負責執行事件的指令
/// </summary>
public class EventExecutor : MonoBehaviour
{
    private static EventExecutor instance;
    public static EventExecutor Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject gameObject = new GameObject();
                gameObject.name = "Event Excutor";
                gameObject.AddComponent<EventExecutor>();
                gameObject.AddComponent<AudioSource>();
                instance = gameObject.GetComponent<EventExecutor>();
                instance.audioSource = gameObject.GetComponent<AudioSource>();
                instance.audioSource.loop = true;
            }
            return instance;
        }
    }

    private EventObject target;

    private EventCommandList commands;

    public bool IsRunning { get; private set; }

    private int eventRunningIndex;

    private AudioSource audioSource;

    public EventCommand NextCommand
    {
        get
        {
            if (commands != null && eventRunningIndex < commands.Count)
                return commands[eventRunningIndex];
            return null;
        }
    }

    private void Awake()
    {
        if (instance == null)
            instance = this;
        commands = null;
        eventRunningIndex = 0;
        audioSource = GetComponent<AudioSource>();
        if (audioSource != null)
        audioSource.loop = true;
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
        commands = new EventCommandList(newCommands);
        eventRunningIndex = 0;
    }

    public void Insert(EventCommandList newCommands)
    {
        if (eventRunningIndex >= commands.Count)
        {
            for (int i = 0; i < newCommands.Count; i++)
                commands.Add(newCommands[i]);
        }
        else
        {
            for (int i = 0; i < newCommands.Count; i++)
                commands.Insert(eventRunningIndex + i, newCommands[i]);
        }
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
                for (int i = 0;i < commands.Count;i++)
                {
                    // 對象已經消失，不繼續處理
                    if (target == null)
                        break;
                    EventCommand eventCommand = commands[i];
                    eventRunningIndex++;
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
        eventRunningIndex = 0;
        commands = null;
        target = null;
    }

    public void SetBGM(AudioClip clip, float volume)
    {
        if (clip == null)
            audioSource.Stop();
        else
        {
            audioSource.clip = clip;
            audioSource.volume = volume;
            audioSource.Play();
        }
    }
}
