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
        if (Selection.activeGameObject != null)
            go.transform.parent = Selection.activeGameObject.transform;
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
        if (Selection.activeGameObject != null)
            go.transform.parent = Selection.activeGameObject.transform;
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
        // 物品不處理
        if (self.GetComponent<ItemButton>() != null)
            return;
        serializedObject.Update();
        List<EventObject> eventObjects = new List<EventObject>(FindObjectsOfType<EventObject>());
        int id = self.id;
        // 複製物件時會連ID一起複製，所以要找是不是有ID重複，大於0且沒重複就表示ID正確，不用繼續
        if (id > 0 && eventObjects.Find(x => x.id == id) == null)
            return;
        // 先設最大
        id = eventObjects.Count + 1;
        eventObjects.Sort(delegate (EventObject x, EventObject y)
        {
            if (x.id > y.id)
                return 1;
            else if (x.id < y.id)
                return -1;
            else
                return 0;
        });

        id = 0;
        foreach (EventObject eventObject in eventObjects)
        {
            if (eventObject.id - id != 1)
            {
                self.id = id + 1;
                break;
            }
            id = eventObject.id;
        }
        serializedObject.ApplyModifiedProperties();
    }

    public override void OnInspectorGUI()
    {
        SetId();

        serializedObject.Update();

        EditorGUILayout.LabelField("ID: " + ((EventObject)target).id);

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
            {
                //propertyEventPoint.InsertArrayElementAtIndex(propertyEventPoint.arraySize);
                EventObject self = (EventObject)target;
                if (self.eventPoint == null)
                    self.eventPoint = new List<EventPoint>();
                self.eventPoint.Add(new EventPoint());
            }
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
                EventObject self = (EventObject)target;
                self.eventPoint.Insert(i, new EventPoint());
                return;
            }
            if (GUILayout.Button("Insert next"))
            {
                EventObject self = (EventObject)target;
                self.eventPoint.Insert(i + 1, new EventPoint());
                return;
            }
            if (GUILayout.Button("Delete"))
            {
                if (propertyEventPoint.arraySize > 0)
                    propertyEventPoint.DeleteArrayElementAtIndex(i);
                return;
            }


            EditorGUILayout.EndHorizontal();

            EditorGUI.BeginChangeCheck();

            if (property.isExpanded)
            {
                EditorGUI.indentLevel += 1;
                EditorGUILayout.PropertyField(property.FindPropertyRelative("triggerType"));
                EditorGUILayout.PropertyField(property.FindPropertyRelative("condition"), true);
                EditorGUILayout.PropertyField(property.FindPropertyRelative("commands"), true);
                EditorGUI.indentLevel -= 1;
            }

            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        }
    }
}
