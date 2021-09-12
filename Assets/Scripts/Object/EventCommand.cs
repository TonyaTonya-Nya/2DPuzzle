﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;
using System.Linq;
using System.Text.RegularExpressions;

[System.Serializable]
public class EventCommandList : IList<EventCommand>
{
    [SerializeReference]
    public List<EventCommand> commands = new List<EventCommand>();

    public EventCommand this[int index] { get => commands[index]; set => commands[index] = value; }

    public int Count => commands.Count;

    public bool IsReadOnly => false;

    public void Add(Type type)
    {
        EventCommand newCommand = (EventCommand)Activator.CreateInstance(type);
        commands.Add(newCommand);
    }

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

    public void Insert(int index, Type type)
    {
        EventCommand newCommand = (EventCommand)Activator.CreateInstance(type);
        commands.Insert(index, newCommand);
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
}

[System.Serializable]
public abstract class EventCommand
{
    protected EventObject caller;

    public readonly static Dictionary<string, Type> types;

    public void Register(EventObject caller)
    {
        this.caller = caller;
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

public class EventDialogue : EventCommand
{
    public string content;

    public override IEnumerator Run()
    {
        int index = content.IndexOf(@"\n");
        if (index > 0)
            content = content.Substring(0, index) + "\n" + content.Substring(index + 2);
        DialogueSystem.Instance.ShowDialouge(content);
        // 下一句如果是選擇，不用點對話框直接顯示選項
        if (!(caller.NextCommand is EventSelection))
            yield return new WaitUntil(() => DialogueSystem.Instance.finish);
        else
            yield return null;
        // 如果下一句是對話或選向，不用關視窗
        if (!(caller.NextCommand is EventDialogue) && !(caller.NextCommand is EventSelection))
            DialogueSystem.Instance.CloseDialouge();

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
    public int targetId;
    public Vector2 destination;
    public float speed;
    // 暫定
    public Vector2 cameraPosition;

    public override IEnumerator Run()
    {
        List<EventObject> eventObjects = new List<EventObject>(GameObject.FindObjectsOfType<EventObject>());
        EventObject target = eventObjects.Find(x => x.id == targetId);
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
            foreach (EventCommand command in selection1Commands)
            {
                command.Register(caller);
                yield return caller.StartCoroutine(command.Run());
            }
        }
        else if (index == 1)
        {
            foreach (EventCommand command in selection2Commands)
            {
                command.Register(caller);
                yield return caller.StartCoroutine(command.Run());
            }
        }
        yield return null;
    }

}

public class EventShowBalloon : EventCommand
{

}

public class EventShowAnimation : EventCommand
{

}

public class EventDestroyEvent : EventCommand
{
    public override IEnumerator Run()
    {
        UnityEngine.Object.Destroy(caller.gameObject);
        yield return null;
    }
}