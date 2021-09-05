using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;

public class GameDatabase
{
    /// <summary>
    /// 事件點的開關
    /// </summary>
    public Dictionary<int, EventSwitch> EventSwitches { get; private set; } = new Dictionary<int, EventSwitch>();

    /// <summary>
    /// 事件點資料庫
    /// </summary>
    public Dictionary<int, EventPoint> EventPoints { get; private set; } = new Dictionary<int, EventPoint>();

    /// <summary>
    /// 對話資料庫
    /// </summary>
    public Dictionary<int, Dialogue> Dialogues { get; private set; } = new Dictionary<int, Dialogue>();

    private static GameDatabase instance;
    public static GameDatabase Instance
    {
        get
        {
            if (instance == null)
                instance = new GameDatabase();
            return instance;
        }
    }

    public GameDatabase()
    {
        string value = File.ReadAllText(Path.Combine(Application.dataPath, "Resources/Database/Dialogue.json"), System.Text.Encoding.Default);
        Dialogues = JsonConvert.DeserializeObject<Dictionary<int, Dialogue>>(value);
    }
}