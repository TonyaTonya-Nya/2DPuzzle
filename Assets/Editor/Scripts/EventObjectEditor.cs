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

public class EventObjectCreator
{
    [MenuItem("GameObject/Event Object/Empty", false, 10)]
    public static void CreateEmptyInstance()
    {
        GameObject go = new GameObject("Event Object");
        go.AddComponent<EventObject>();
        Selection.activeObject = go;
    }

    [MenuItem("GameObject/Event Object/Box Collider", false, 10)]
    public static void CreateBoxInstance()
    {
        CreateEventObject<BoxCollider2D>();
    }

    [MenuItem("GameObject/Event Object/Circle Collider", false, 10)]
    public static void CreateCircleInstance()
    {
        CreateEventObject<CircleCollider2D>();
    }

    [MenuItem("GameObject/Event Object/Capsule Collider", false, 10)]
    public static void CreateCapsuleInstance()
    {
        CreateEventObject<CapsuleCollider2D>();
    }

    [MenuItem("GameObject/Event Object/Polygon Collider", false, 10)]
    public static void CreatePolygonInstance()
    {
        CreateEventObject<PolygonCollider2D>();
    }

    public static void CreateEventObject<T>() where T : Collider2D
    {
        GameObject go = new GameObject("Event Object");
        go.AddComponent<T>();
        go.GetComponent<T>().isTrigger = true;
        go.AddComponent<EventObject>();
        Selection.activeObject = go;
    }
}

[CustomEditor(typeof(EventObject))]
public class EventObjectEditor : Editor
{
    private SerializedProperty propertyEventPoint;

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
        SetId();

        serializedObject.Update();

        EditorGUILayout.LabelField("ID: " + ((EventObject)target).Id);

        propertyEventPoint = serializedObject.FindProperty("eventPoint");
        EditorGUILayout.PropertyField(propertyEventPoint, false);

        EditorGUI.indentLevel += 1;
        if (propertyEventPoint.isExpanded)
        {
            serializedObject.Update();
            DrawEventPoint();
            serializedObject.ApplyModifiedProperties();

            serializedObject.Update();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add New Event Point"))
                propertyEventPoint.InsertArrayElementAtIndex(propertyEventPoint.arraySize);
            if (GUILayout.Button("Delete Last Event Point"))
            {
                if (propertyEventPoint.arraySize > 0)
                    propertyEventPoint.DeleteArrayElementAtIndex(propertyEventPoint.arraySize - 1);
            }
            EditorGUILayout.EndHorizontal();
            serializedObject.ApplyModifiedProperties();
        }
        EditorGUI.indentLevel -= 1;

        serializedObject.ApplyModifiedProperties();
    }

    public void DrawEventPoint()
    {
        for (int i = 0; i < propertyEventPoint.arraySize; i++)
        {
            // EventPoint
            SerializedProperty property = propertyEventPoint.GetArrayElementAtIndex(i);

            EditorGUILayout.BeginHorizontal();

            property.isExpanded = EditorGUILayout.Foldout(property.isExpanded, new GUIContent((i + 1).ToString()), true);

            if (GUILayout.Button("Insert previous"))
            {
                propertyEventPoint.InsertArrayElementAtIndex(i);
                return;
            }
            if (GUILayout.Button("Insert next"))
            {
                propertyEventPoint.InsertArrayElementAtIndex(i + 1);
                return;
            }
            if (GUILayout.Button("Delete"))
            {
                if (propertyEventPoint.arraySize > 0)
                    propertyEventPoint.DeleteArrayElementAtIndex(i);
                return;
            }


            EditorGUILayout.EndHorizontal();

            if (property.isExpanded)
            {
                EditorGUI.indentLevel += 1;
                EditorGUILayout.PropertyField(property.FindPropertyRelative("autoStart"));
                EditorGUILayout.PropertyField(property.FindPropertyRelative("condition"), true);
                EditorGUILayout.PropertyField(property.FindPropertyRelative("commands"), true);
                EditorGUI.indentLevel -= 1;
            }

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        }
    }
}
