using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[System.Serializable]
public class EventCommandList : IList<EventCommand>
{
    [SerializeReference]
    public List<EventCommand> commands = new List<EventCommand>();

    public void Insert(int index, Type type)
    {
        EventCommand newCommand = (EventCommand)Activator.CreateInstance(type);
        commands.Insert(index, newCommand);
    }

    public void Add(Type type)
    {
        EventCommand newCommand = (EventCommand)Activator.CreateInstance(type);
        commands.Add(newCommand);
    }

    public EventCommandList()
    {
        commands = new List<EventCommand>();
    }

    public EventCommandList(EventCommandList collection)
    {
        commands = new List<EventCommand>(collection);
    }

    #region IList
    public EventCommand this[int index] { get => commands[index]; set => commands[index] = value; }

    public int Count => commands.Count;

    public bool IsReadOnly => false;

    public void Add(EventCommand item)
    {
        commands.Add(item);
    }

    public void Clear()
    {
        commands.Clear();
    }

    public bool Contains(EventCommand item)
    {
        return commands.Contains(item);
    }

    public void CopyTo(EventCommand[] array, int arrayIndex)
    {
        commands.CopyTo(array, arrayIndex);
    }

    public IEnumerator<EventCommand> GetEnumerator()
    {
        return commands.GetEnumerator();
    }

    public int IndexOf(EventCommand item)
    {
        return commands.IndexOf(item);
    }

    public void Insert(int index, EventCommand item)
    {
        commands.Insert(index, item);
    }

    public bool Remove(EventCommand item)
    {
        return commands.Remove(item);
    }

    public void RemoveAt(int index)
    {
        if (index < commands.Count)
            commands.RemoveAt(index);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)commands).GetEnumerator();
    }
    #endregion
}

[System.Serializable]
public abstract class EventCommand
{
    protected EventObject target;

    public readonly static Dictionary<string, Type> types;

    public void Register(EventObject caller)
    {
        this.target = caller;
    }

    static EventCommand()
    {
        Type refType = typeof(EventCommand);
        types = refType.Assembly.GetTypes().Where(x => !x.IsAbstract && refType.IsAssignableFrom(x) && x.Name != "EventCommand").
            OrderBy(x => x.Name).ToDictionary(x => x.Name, x => x);
    }

    public static string GetName(string raw)
    {
        Regex prefixRemove = new Regex(@"^Event");
        Regex spaceAdd = new Regex(@"(?<=.)(?=[A-Z])");
        string name = raw;
        name = prefixRemove.Replace(name, "");
        name = spaceAdd.Replace(name, " ");
        return name;
    }

    public virtual IEnumerator Run() { yield return null; }
}

[System.Serializable]
public class EventDialogue : EventCommand
{
    public string content;

    public override IEnumerator Run()
    {
        // 轉譯換行符號
        //Regex regex = new Regex(@"[^\]([\][\n]*");
        //MatchCollection matches = regex.Matches(content);
        //foreach (Match match in matches)
        //    Debug.Log(match);
        //content = regex.Replace(content, "\n");

        content = content.Replace("\\n", "\n");

        content = content.Replace("\\h", PlayerData.Instance.playerName);

        // 11 號是 看過軍牌，所以盡量不要改動
        if (GameDatabase.Instance.GetSwitchState(11))
        {
            content = content.Replace("主角", PlayerData.Instance.playerName);
            content = content.Replace("神華", PlayerData.Instance.playerName);
        }

        DialogueSystem.Instance.ShowDialouge(content);
        // 下一句如果是選項或輸入，直接離開，並執行選項
        if (EventExecutor.Instance.NextCommand is EventSelection ||
            EventExecutor.Instance.NextCommand is EventInput ||
            EventExecutor.Instance.NextCommand is EventSetPlayerName)
            yield break;
        // 下一句如果是對話，完成後不關閉對話窗，並繼續對話
        else if (EventExecutor.Instance.NextCommand is EventDialogue)
        {
            yield return new WaitUntil(() => DialogueSystem.Instance.finish);
            yield break;
        }
        // 需要等待對話框完成
        else
        {
            yield return new WaitUntil(() => DialogueSystem.Instance.finish);
            DialogueSystem.Instance.CloseDialouge();
        }
        yield return null;
    }
}

public class EventGainItem : EventCommand
{
    public int itemId;
    public int number;

    public override IEnumerator Run()
    {
        if (number > 0)
            PlayerData.Instance.GainItem(itemId);
        else if (number < 0)
            PlayerData.Instance.LoseItem(itemId);
        yield return null;
    }
}

public class EventSetSwitch : EventCommand
{
    public int switchId;
    public bool open;

    public override IEnumerator Run()
    {
        GameDatabase.Instance.SetSwitch(switchId, open);
        yield return null;
    }
}

public class EventTransition : EventCommand
{
    public string targetId;
    public Vector2 destination;
    public float speed;
    // 暫定
    public Vector2 cameraPosition;

    public override IEnumerator Run()
    {
        List<EventObject> eventObjects = new List<EventObject>(GameObject.FindObjectsOfType<EventObject>());
        EventObject target = eventObjects.Find(x => x.guid == targetId);
        if (target != null)
        {
            Vector2 origin = target.transform.position;
            Vector2 vector = (destination - origin).normalized;
            if (speed > 0)
            {
                while (Vector2.Distance(origin, destination) >= speed * Time.deltaTime)
                {
                    target.transform.Translate(vector * speed * Time.deltaTime);
                    yield return null;
                    origin = target.transform.position;
                }
            }
            target.transform.position = destination;
            // 玩家才需要動相機
            if (target.CompareTag("Player"))
                Camera.main.transform.position = new Vector3(cameraPosition.x, cameraPosition.y, -10);
        }
        yield return null;
    }
}

public class EventSelection : EventCommand
{
    public string selection1;
    public EventCommandList selection1Commands;
    public string selection2;
    public EventCommandList selection2Commands;

    public override IEnumerator Run()
    {
        SelectionBox.main.ShowSelectionBox(selection1, selection2);
        yield return new WaitUntil(() => !SelectionBox.main.IsWaiting());
        int index = SelectionBox.main.GetResult();
        if (index == 0)
        {
            EventExecutor.Instance.Insert(selection1Commands);
        }
        else if (index == 1)
        {
            EventExecutor.Instance.Insert(selection2Commands);
        }
        yield return null;
    }
}

public class EventInput : EventCommand
{
    public string answer;
    public EventCommandList correctCommands;
    public EventCommandList wrongCommands;

    public override IEnumerator Run()
    {
        InputBox.main.ShowInputBox();
        yield return new WaitUntil(() => !InputBox.main.IsWaiting());
        string result = InputBox.main.GetResult();
        if (result == answer)
            EventExecutor.Instance.Insert(correctCommands);
        else
            EventExecutor.Instance.Insert(wrongCommands);
        // 下一句如果是對話，完成後不關閉對話窗，並繼續對話
        if (EventExecutor.Instance.NextCommand is EventDialogue)
            yield break;
        // 否則關閉對話窗
        else
            DialogueSystem.Instance.CloseDialouge();
        yield return null;
    }
}

public class EventShowBalloon : EventCommand
{

}

public class EventSetAnimationParameter : EventCommand
{
    // 要播誰的動畫
    public Animator animator;
    // 動畫要設定的參數
    public List<string> triggers;
    public StringIntDictionary ints;
    public StringBoolDictionary bools;
    public StringFloatDictionary floats;

    public override IEnumerator Run()
    {
        foreach (string name in triggers)
            animator.SetTrigger(name);
        foreach (KeyValuePair<string, int> pair in ints)
            animator.SetInteger(pair.Key, pair.Value);
        foreach (KeyValuePair<string, bool> pair in bools)
            animator.SetBool(pair.Key, pair.Value);
        foreach (KeyValuePair<string, float> pair in floats)
            animator.SetFloat(pair.Key, pair.Value);
        yield return null;
    }
}

public class EventWait : EventCommand
{
    public float frame;

    public override IEnumerator Run()
    {
        for (int i = 0; i < frame; i++)
            yield return null;
    }
}

public class EventDestroyEvent : EventCommand
{
    public override IEnumerator Run()
    {
        UnityEngine.Object.Destroy(target.gameObject);
        yield return null;
    }
}

public class EventSetScreenColor : EventCommand
{
    public Color color;

    public override IEnumerator Run()
    {
        GameObject mask = GameObject.Find("Black Mask");
        if (mask != null)
        {
            mask.GetComponent<Image>().color = color;
        }
        yield return null;
    }
}

public class EventConditionBranch : EventCommand
{
    public int switchId;
    public bool needOn;
    public EventCommandList conditionOkCommands;
    public EventCommandList conditionNotOkCommands;

    public override IEnumerator Run()
    {
        if (GameDatabase.Instance.GetSwitchState(switchId) == needOn)
            EventExecutor.Instance.Insert(conditionOkCommands);
        else
            EventExecutor.Instance.Insert(conditionNotOkCommands);
        yield return null;
    }
}

public class EventSetDirection : EventCommand
{
    public string targetId;

    public Direction direction;

    public override IEnumerator Run()
    {
        List<EventObject> eventObjects = new List<EventObject>(GameObject.FindObjectsOfType<EventObject>());
        EventObject target = eventObjects.Find(x => x.guid == targetId);
        if (target != null)
        {
            Vector3 scale = target.transform.localScale;
            switch (direction)
            {
                case Direction.Right:
                    if (target.whichIsPositive == Direction.Left)
                        scale.x = -1 * Mathf.Abs(scale.x);
                    else
                        scale.x = Mathf.Abs(scale.x);
                    break;
                case Direction.Left:
                    if (target.whichIsPositive == Direction.Right)
                        scale.x = -1 * Mathf.Abs(scale.x);
                    else
                        scale.x = Mathf.Abs(scale.x);
                    break;
                default:
                    break;
            }
            target.transform.localScale = scale;
        }
        yield return null;
    }
}

public class EventPlaySE : EventCommand
{
    public AudioClip audio;
    public float volume = 1;

    public override IEnumerator Run()
    {
        AudioSource.PlayClipAtPoint(audio, new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y, 0), volume);
        yield return null;
    }
}

public class EventSetPlayerName : EventCommand
{
    public override IEnumerator Run()
    {
        InputBox.main.ShowInputBox();
        yield return new WaitUntil(() => !InputBox.main.IsWaiting());
        string result = InputBox.main.GetResult();
        PlayerData.Instance.playerName = result;
        // 下一句如果是對話，完成後不關閉對話窗，並繼續對話
        if (EventExecutor.Instance.NextCommand is EventDialogue)
            yield break;
        // 否則關閉對話窗
        else
            DialogueSystem.Instance.CloseDialouge();
        yield return null;
    }
}

public class EventPlayBGM : EventCommand
{
    public AudioClip audioClip;
    public float volumn = 1;

    public override IEnumerator Run()
    {
        EventExecutor.Instance.SetBGM(audioClip, volumn);
        return base.Run();
    }
}

public class EventGameOver : EventCommand
{
    public override IEnumerator Run()
    {
        SceneManager.LoadScene(2);
        yield return null;
    }
}