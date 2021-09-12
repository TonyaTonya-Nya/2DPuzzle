using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using Newtonsoft.Json;

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

    [MenuItem("Tools/Helper/Refresh Event Object Id")]
    public static void RefreshEventObjectId()
    {
        List<EventObject> eventObjects = new List<EventObject>(GameObject.FindObjectsOfType<EventObject>());
        eventObjects.Reverse();
        int id = 1;
        foreach (EventObject eventObject in eventObjects)
        {
            eventObject.id = id;
            id++;
        }
    }
}
