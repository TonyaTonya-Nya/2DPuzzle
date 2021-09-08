using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EventCommand
{
    public string dialogue;
    public int gainItemId;
    public int loseItemId;
    public int openSwitchId;
    public int closeSwitchId;
    public bool showSelection;
    public string selectionText1;
    public string selectionText2;
    public List<EventPoint> firstSelectionEvent;
    public List<EventPoint> secondSelectionEvent;

    public virtual IEnumerator Run() { yield return null; }
}

[System.Serializable]
public class EventDialogue : EventCommand
{
    public string content;

    public override IEnumerator Run()
    {
        yield return null;
    }
}

[System.Serializable]
public class EventGainItem : EventCommand
{
    public int itemId;
    public int number;

    public override IEnumerator Run()
    {
        yield return null;
    }
}

[System.Serializable]
public class EventSetSwitch : EventCommand
{
    public int switchId;
    public bool open;

    public override IEnumerator Run()
    {
        yield return null;
    }
}

public class EventExecution
{
    
}
