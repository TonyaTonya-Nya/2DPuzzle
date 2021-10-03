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

    /// <summary>
    /// 備份用
    /// </summary>
    [MenuItem("Tools/Export/ItemDatabase")]
    public static void ExportItemDataToJson()
    {
        GameDatabase gameDatabase = GameObject.FindObjectOfType<GameDatabase>();
        string dataPath = "Assets/Prefabs/ItemDatabase.asset";
        ItemDatabase itemDatabase = (ItemDatabase)AssetDatabase.LoadAssetAtPath(dataPath, typeof(ItemDatabase));
        string jsonPath = Path.Combine(Application.dataPath, "Resources/Database/ItemDatabase.json");
        string s = JsonConvert.SerializeObject(itemDatabase);
        using (StreamWriter file = new StreamWriter(jsonPath))
        {
            file.WriteLine(s);
        }
    }

    [MenuItem("Tools/Changer/AddPlayerNameCodeToAllDialogue")]
    public static void AddPlayerNameCodeToAllDialogue()
    {
        EventObject[] eventObjects = GameObject.FindObjectsOfType<EventObject>();
        foreach (EventObject eventObject in eventObjects)
        {
            if (eventObject.eventPoint != null)
            {
                foreach (EventPoint eventPoint in eventObject.eventPoint)
                {
                    foreach (EventCommand eventCommand in eventPoint.commands)
                    {
                        if (eventCommand is EventDialogue eventDialogue)
                        {
                            if (!eventDialogue.content.StartsWith("\\h\\n"))
                                eventDialogue.content = "\\h\\n" + eventDialogue.content;
                        }
                    }
                }
            }
        }
        if (GameDatabase.Instance.ItemDB != null)
        {
            foreach (KeyValuePair<int, Item> pair in GameDatabase.Instance.ItemDB)
            {
                if (pair.Value.clickEvent != null)
                {
                    foreach (EventPoint eventPoint in pair.Value.clickEvent)
                    {
                        foreach (EventCommand eventCommand in eventPoint.commands)
                        {
                            if (eventCommand is EventDialogue eventDialogue)
                            {
                                if (!eventDialogue.content.StartsWith("\\h\\n"))
                                    eventDialogue.content = "\\h\\n" + eventDialogue.content;
                            }
                        }
                    }
                }
            }
        }
    }

    [MenuItem("Tools/Changer/TEmp")]
    public static void TEmp()
    {
        foreach (KeyValuePair<int, Item> pair in GameDatabase.Instance.ItemDB)
        {
            if (pair.Value.clickEvent != null)
            {
                foreach (EventPoint eventPoint in pair.Value.clickEvent)
                {
                    foreach (EventCommand eventCommand in eventPoint.commands)
                    {
                        if (eventCommand is EventDialogue eventDialogue)
                        {
                            if (eventDialogue.content.Contains("\n"))
                                eventDialogue.content = eventDialogue.content.Replace("\n", "\\n");
                        }
                    }
                }
            }
        }
    }

    [MenuItem("Tools/Changer/DeleteExtraPlayerNameStringOfAllDialogue")]
    public static void DeleteExtraPlayerNameStringOfAllDialogue()
    {
        EventObject[] eventObjects = GameObject.FindObjectsOfType<EventObject>();
        foreach (EventObject eventObject in eventObjects)
        {
            if (eventObject.eventPoint != null)
            {
                foreach (EventPoint eventPoint in eventObject.eventPoint)
                {
                    foreach (EventCommand eventCommand in eventPoint.commands)
                    {
                        if (eventCommand is EventDialogue eventDialogue)
                        {
                            if (eventDialogue.content.StartsWith("\\h\\n主角\\n"))
                                eventDialogue.content = eventDialogue.content.Replace("\\h\\n主角\\n", "\\h\\n");
                            if (eventDialogue.content.StartsWith("\\h\\n\\h"))
                                eventDialogue.content = eventDialogue.content.Replace("\\h\\n\\h", "\\h\\n");
                            if (eventDialogue.content.StartsWith("\\h\\n大叔\\n"))
                                eventDialogue.content = eventDialogue.content.Replace("\\h\\n大叔\\n", "大叔\\n");
                            if (eventDialogue.content.StartsWith("\\h\\n神華\\n"))
                                eventDialogue.content = eventDialogue.content.Replace("\\h\\n神華\\n", "\\h\\n");
                        }
                    }
                }
            }
        }
    }

}
