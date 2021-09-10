using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System;
using System.Linq;
using UnityEditor.SceneManagement;
using System.Reflection;

[CustomEditor(typeof(EventObject))]
public class EventObjectEditor : Editor
{
    private SerializedProperty propertyEventPoint;

    public Dictionary<int, List<EventCommand>> eventCommands;

    [MenuItem("GameObject/Event Object", false, 10)]
    public static void CreateTextArea()
    {
        GameObject go = new GameObject("Event Object");
        go.AddComponent<EventObject>();
        Selection.activeObject = go;
    }

    private void SetId()
    {
        EventObject self = (EventObject)target;
        PropertyInfo propertyinfo = self.GetType().GetProperty("Id");
        // 大於0表示已經設定過了
        if ((int)propertyinfo.GetValue(self) > 0)
            return;
        List<EventObject> eventObjects = new List<EventObject>(FindObjectsOfType<EventObject>());
        propertyinfo.SetValue(self, eventObjects.Count + 1);
        eventObjects.Sort(delegate (EventObject x, EventObject y)
        {
            if (x.Id > y.Id)
                return 1;
            else if (x.Id < y.Id)
                return -1;
            else
                return 0;
        });

        int id = 0;
        foreach (EventObject eventObject in eventObjects)
        {
            if (eventObject.Id - id != 1)
            {
                propertyinfo.SetValue(self, id + 1);
                break;
            }
            id = eventObject.Id;
        }
    }

    public override void OnInspectorGUI()
    {
        if (eventCommands == null)
            eventCommands = new Dictionary<int, List<EventCommand>>();

        SetId();

        serializedObject.Update();

        EditorGUILayout.LabelField("ID: " + ((EventObject)target).Id);

        propertyEventPoint = serializedObject.FindProperty("eventPoint");
        EditorGUILayout.PropertyField(propertyEventPoint, false);

        EditorGUI.indentLevel += 1;
        if (propertyEventPoint.isExpanded)
        {
            for (int i = 0; i < propertyEventPoint.arraySize; i++)
            {
                DrawEventPoint(i);
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            }
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add New Event Point"))
                propertyEventPoint.InsertArrayElementAtIndex(propertyEventPoint.arraySize);
            if (GUILayout.Button("Delete Last Event Point"))
            {
                if (propertyEventPoint.arraySize > 0)
                    propertyEventPoint.DeleteArrayElementAtIndex(propertyEventPoint.arraySize - 1);
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUI.indentLevel -= 1;

        serializedObject.ApplyModifiedProperties();
    }

    public void DrawEventPoint(int index)
    {
        SerializedProperty property = propertyEventPoint.GetArrayElementAtIndex(index);

        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.PropertyField(property, new GUIContent((index + 1).ToString()), false);

        if (GUILayout.Button("Insert previous"))
            propertyEventPoint.InsertArrayElementAtIndex(index);
        if (GUILayout.Button("Insert next"))
            propertyEventPoint.InsertArrayElementAtIndex(index + 1);
        if (GUILayout.Button("Delete"))
        {
            if (propertyEventPoint.arraySize > 0)
                propertyEventPoint.DeleteArrayElementAtIndex(index);
            return;
        }

        EditorGUILayout.EndHorizontal();

        if (!eventCommands.ContainsKey(index))
            eventCommands[index] = new List<EventCommand>();

        if (property.isExpanded)
        {
            EditorGUI.indentLevel += 1;
            EditorGUILayout.PropertyField(property.FindPropertyRelative("autoStart"), new GUIContent("Auto Start", "滿足條件後是否自動執行"));
            DrawEventCondition(property.FindPropertyRelative("condition"));
            DrawEventCommands(property.FindPropertyRelative("commands"));
            EditorGUI.indentLevel -= 1;
        }
    }

    public void DrawEventCondition(SerializedProperty condition)
    {
        EditorGUILayout.PropertyField(condition, new GUIContent("Condition", "事件點的條件，滿足後才可觸發"));
    }

    public void DrawEventCommands(SerializedProperty commands)
    {
        EditorGUILayout.PropertyField(commands, new GUIContent("Commands", "事件指令"));
    }
}
