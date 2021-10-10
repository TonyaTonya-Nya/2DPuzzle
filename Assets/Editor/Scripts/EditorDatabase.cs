using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using Newtonsoft.Json;
using UnityEngine.UI;

public class EditorDatabase
{
    private SwitchDatabase switchDatabase;
    private Dictionary<int, EventSwitches> eventSwitchesDB;
    public Dictionary<int, EventSwitches> EventSwitchesDB
    {
        get
        {
            if (eventSwitchesDB == null)
                LoadSwitchDatabase();
            CheckSwitchDatabase();
            return eventSwitchesDB;
        }
        private set
        {
            eventSwitchesDB = value;
        }
    }

    private static EditorDatabase instance;
    public static EditorDatabase Instance
    {
        get
        {
            if (instance == null)
                instance = new EditorDatabase();
            return instance;
        }
    }

    public EditorDatabase()
    {
        LoadSwitchDatabase();
    }

    public void LoadSwitchDatabase()
    {
        string path = "Assets/Editor/EditorDatabase/SwitchDatabase.asset";
        switchDatabase = (SwitchDatabase)AssetDatabase.LoadAssetAtPath(path, typeof(SwitchDatabase));
        eventSwitchesDB = new Dictionary<int, EventSwitches>();
        foreach (KeyValuePair<int, EventSwitches> pair in switchDatabase.switches)
        {
            pair.Value.id = pair.Key;
            eventSwitchesDB[pair.Key] = pair.Value;
        }
    }

    public void CheckSwitchDatabase()
    {
        string path = "Assets/Editor/EditorDatabase/SwitchDatabase.asset";
        switchDatabase = (SwitchDatabase)AssetDatabase.LoadAssetAtPath(path, typeof(SwitchDatabase));
        if (switchDatabase.switches.Count != eventSwitchesDB.Count)
            LoadSwitchDatabase();
    }

    [MenuItem("Tools/Checker/CheckWhereHasWrongName")]
    public static void CheckWhereHasWrongName()
    {
        EventObject[] eventObjects = GameObject.FindObjectsOfType<EventObject>();
        foreach (EventObject eventObject in eventObjects)
        {
            if (eventObject.eventPoint != null)
            {
                int index = 0;
                foreach (EventPoint eventPoint in eventObject.eventPoint)
                {
                    Parse(eventObject.gameObject.name, eventPoint.commands, index + 1);
                    index++;
                }
            }
        }
        if (GameDatabase.Instance.ItemDB != null)
        {
            foreach (KeyValuePair<int, Item> pair in GameDatabase.Instance.ItemDB)
            {
                if (pair.Value.clickEvent != null)
                {
                    int index = 0;
                    foreach (EventPoint eventPoint in pair.Value.clickEvent)
                    {
                        Parse(pair.Value.itemName, eventPoint.commands, index++);
                    }
                }
            }
        }
        if (GameDatabase.Instance.ItemMixDatabase != null)
        {
            foreach (ItemMixSet s in GameDatabase.Instance.ItemMixDatabase.itemMixSets)
            {
                Parse("", s.commands, 0);
            }
        }
    }

    public static void Parse(string name, EventCommandList commands, int index = 0)
    {
        foreach (EventCommand command in commands)
        {
            if  (command is EventDialogue dialogue)
            {
                if (dialogue.content.Contains("主角") || dialogue.content.Contains("神華"))
                    Debug.Log(name + ": 第 " + index + " 頁: " + dialogue.content);
            }
            else if (command is EventConditionBranch condition)
            {
                Parse(name, condition.conditionOkCommands, index);
                Parse(name, condition.conditionNotOkCommands, index);
            }
            else if (command is EventInput input)
            {
                Parse(name, input.correctCommands, index);
                Parse(name, input.wrongCommands, index);
            }
            else if (command is EventSelection selection)
            {
                Parse(name, selection.selection1Commands, index);
                Parse(name, selection.selection2Commands, index);
            }
        }
    }



    [MenuItem("Tools/Checker/CheckWhereHasGameOver")]
    public static void CheckWhereHasGameOver()
    {
        EventObject[] eventObjects = GameObject.FindObjectsOfType<EventObject>();
        foreach (EventObject eventObject in eventObjects)
        {
            if (eventObject.eventPoint != null)
            {
                int pindex = 0;
                foreach (EventPoint eventPoint in eventObject.eventPoint)
                {
                    int index = 0;
                    foreach (EventCommand command in eventPoint.commands)
                    {
                        if (command is EventGameOver)
                            Debug.Log("第 " + (pindex + 1) + " 頁" + "Gameover: " + eventObject.name);
                        index++;
                    }
                    pindex++;
                }
            }
        }
        if (GameDatabase.Instance.ItemDB != null)
        {
            foreach (KeyValuePair<int, Item> pair in GameDatabase.Instance.ItemDB)
            {
                if (pair.Value.clickEvent != null)
                {
                    int pindex = 0;
                    foreach (EventPoint eventPoint in pair.Value.clickEvent)
                    {
                        int index = 0;
                        foreach (EventCommand command in eventPoint.commands)
                        {
                            if (command is EventGameOver)
                                Debug.Log("第 " + (pindex+1) + " 頁" + "]Gameover: " + pair.Value.itemName);
                            index++;
                        }
                        pindex++;
                    }
                }
            }
        }
        if (GameDatabase.Instance.ItemMixDatabase != null)
        {
            foreach (ItemMixSet s in GameDatabase.Instance.ItemMixDatabase.itemMixSets)
            {
                int index = 0;
                foreach (EventCommand command in s.commands)
                {
                    if (command is EventGameOver)
                        Debug.Log("找到 [ " + index.ToString() + "]Gameover: ");
                    index++;
                }
            }
        }
    }
}
