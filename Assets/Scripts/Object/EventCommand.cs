using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;
using System.Linq;
using System.Text.RegularExpressions;

[System.Serializable]
public class EventCommandList
{
    public List<EventCommand> commands = new List<EventCommand>();
    public int test;

    public void Add(Type type)
    {
        EventCommand newCommand = (EventCommand)ScriptableObject.CreateInstance(type);
        commands.Add(newCommand);
    }

    public void InsertAt(int index, Type type)
    {
        EventCommand newCommand = (EventCommand)ScriptableObject.CreateInstance(type);
        commands.Insert(index, newCommand);
    }

    public void RemoveAt(int index)
    {
        if (index < commands.Count)
            commands.RemoveAt(index);
    }
}

public abstract class EventCommand : ScriptableObject
{
    public readonly static Dictionary<string, Type> types;

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
        yield return null;
    }
}

public class EventGainItem : EventCommand
{
    public int itemId;
    public int number;

    public override IEnumerator Run()
    {
        yield return null;
    }
}

public class EventSetSwitch : EventCommand
{
    public int switchId;
    public bool open;

    public override IEnumerator Run()
    {
        yield return null;
    }
}

public class EventTransition : EventCommand
{
    public string targetName;
    public Vector2 destination;
    public float speed;

    public override IEnumerator Run()
    {
        
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
        yield return null;
    }

}

public class EventShowBalloon : EventCommand
{

}

public class EventShowAnimation : EventCommand
{

}