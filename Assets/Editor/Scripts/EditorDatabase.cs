using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

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

}
