using RotaryHeart.Lib.SerializableDictionary;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.MemoryProfiler;
using UnityEngine;

public class EditorHelper
{
    public static object GetObj(SerializedProperty property)
    {
        string path = property.propertyPath.Replace(".Array.data[", "[");
        string[] elements = path.Split('.');
        object obj = property.serializedObject.targetObject;
        foreach (string element in elements)
        {
            // 是陣列
            if (element.Contains("["))
            {
                string elementName = element.Substring(0, element.IndexOf("["));
                if (elementName == "_values")
                    elementName = "Values";
                int index = System.Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
                IEnumerable enumerable;
                if ((obj is IDictionary) || (obj is DrawableDictionary))
                {
                    Type ssss = obj.GetType();
                    PropertyInfo p = ssss.GetProperty(elementName);
                    enumerable = obj.GetType().GetProperty(elementName).GetValue(obj) as IEnumerable;
                }
                else
                    enumerable = obj.GetType().GetField(elementName).GetValue(obj) as IEnumerable;
                IEnumerator enm = enumerable.GetEnumerator();
                for (int i = 0; i <= index; i++)
                {
                    if (!enm.MoveNext())
                        break;
                }
                obj = enm.Current;
            }
            // 不是陣列
            else
            {
                obj = obj.GetType().GetField(element).GetValue(obj);
            }
        }
        return obj;
    }

    public static float NextLine
    {
        get => EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
    }

    public static bool CheckRightClick(Rect rect)
    {
        if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
            return Event.current.button == 1;
        return false;
    }

    public static IEnumerable<SerializedProperty> GetChildrenProperty(SerializedProperty serializedProperty)
    {
        SerializedProperty currentProperty = serializedProperty.Copy();
        SerializedProperty nextSiblingProperty = serializedProperty.Copy();
        {
            nextSiblingProperty.Next(false);
        }

        if (currentProperty.Next(true))
        {
            do
            {
                if (SerializedProperty.EqualContents(currentProperty, nextSiblingProperty))
                    break;

                yield return currentProperty;
            }
            while (currentProperty.Next(false));
        }
    }
}

[CustomPropertyDrawer(typeof(EventCommandList))]
public class EventCommandListPropertyDrawer : PropertyDrawer
{
    private int nowSelectIndex;

    private EventCommandPropertyDrawer nowDrawer;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        // 所有屬性的高度
        float height = EditorGUI.GetPropertyHeight(property.FindPropertyRelative("commands"), true);
        return height;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.indentLevel -= 1;
        EditorGUI.DrawRect(EditorGUI.IndentedRect(position), new Color(0.5f, 0.5f, 0.5f, 0.5f));
        EditorGUI.indentLevel += 1;

        position.height = EditorGUIUtility.singleLineHeight;

        SerializedProperty commands = property.FindPropertyRelative("commands");

        commands.isExpanded = EditorGUI.Foldout(position, commands.isExpanded, label, true);

        if (commands.isExpanded)
        {
            EditorGUI.indentLevel += 1;
            float lastUsedHeight = EditorHelper.NextLine;

            GenericMenu menu = CreateMenu(property, false);

            for (int i = 0; i < commands.arraySize; i++)
            {
                SerializedProperty singleCommand = commands.GetArrayElementAtIndex(i);
                position.y += lastUsedHeight;

                EditorGUI.PropertyField(position, singleCommand, true);

                lastUsedHeight = EditorGUI.GetPropertyHeight(singleCommand, true);
                position.y += EditorGUIUtility.standardVerticalSpacing;

                Rect buttonRect = position;
                buttonRect.x = buttonRect.x + buttonRect.width - buttonRect.width / 7;
                buttonRect.width = buttonRect.width / 7;
                if (GUI.Button(buttonRect, "Delete"))
                {
                    EventCommandList eventCommandList = EditorHelper.GetObj(property) as EventCommandList;
                    eventCommandList.RemoveAt(i);
                    return;
                }
                buttonRect.x = buttonRect.x - buttonRect.width - EditorGUIUtility.standardVerticalSpacing;
                if (GUI.Button(buttonRect, "Duplicate"))
                {
                    nowSelectIndex = i;
                    EventCommandList eventCommandList = EditorHelper.GetObj(property) as EventCommandList;
                    eventCommandList.Insert(nowSelectIndex + 1, eventCommandList[nowSelectIndex].GetType());
                    return;
                }
                buttonRect.x = buttonRect.x - buttonRect.width - EditorGUIUtility.standardVerticalSpacing;
                if (GUI.Button(buttonRect, "Insert"))
                {
                    nowSelectIndex = i;
                    menu.ShowAsContext();
                }
            }
            position.y += lastUsedHeight;
            DrawEditMenu(property, position);
            EditorGUI.indentLevel -= 1;
            property.serializedObject.ApplyModifiedProperties();
        }
    }

    private GenericMenu CreateMenu(SerializedProperty property, bool hasRemoveButton)
    {
        GenericMenu menu = new GenericMenu();

        EventCommandList target = EditorHelper.GetObj(property) as EventCommandList;

        foreach (KeyValuePair<string, Type> pair in EventCommand.types)
            menu.AddItem(new GUIContent("Add/" + EventCommand.GetName(pair.Key)), false, () => {
                target.Insert(nowSelectIndex, pair.Value);
                EditorUtility.SetDirty(property.serializedObject.targetObject);
                property.serializedObject.ApplyModifiedProperties();
            });
        if (hasRemoveButton)
            menu.AddItem(new GUIContent("Remove"), false, () => target.RemoveAt(nowSelectIndex - 1));
        return menu;
    }

    private void DrawEditMenu(SerializedProperty property, Rect position)
    {
        GenericMenu menu = CreateMenu(property, true);
        EditorGUI.indentLevel -= 2;
        EditorGUI.DrawRect(EditorGUI.IndentedRect(position), new Color(0f, 0f, 0f, 0.5f));
        EditorGUI.indentLevel += 2;

        EditorGUI.LabelField(position, "Right click to add new command or remove command.");
        if (EditorHelper.CheckRightClick(position))
        {
            nowSelectIndex = property.FindPropertyRelative("commands").arraySize;
            if (nowSelectIndex < 0)
                nowSelectIndex = 0;
            menu.ShowAsContext();
        }
    }
}

[CustomPropertyDrawer(typeof(EventCommand))]
public class EventCommandPropertyDrawer : PropertyDrawer
{
    private Dictionary<SerializedProperty, EventCommandPropertyDrawerBase> drawers = new Dictionary<SerializedProperty, EventCommandPropertyDrawerBase>();

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return GetDrawer(property).GetPropertyHeight(property);
    }

    public EventCommandPropertyDrawerBase GetDrawer(SerializedProperty property)
    {
        Type refType = typeof(EventCommandPropertyDrawerBase);
        Dictionary<string, Type> types = refType.Assembly.GetTypes().Where(x => !x.IsAbstract && refType.IsAssignableFrom(x)).
            OrderBy(x => x.Name).ToDictionary(x => x.Name, x => x);

        EventCommand eventCommand = EditorHelper.GetObj(property) as EventCommand;
        Type type = eventCommand.GetType();
        Type edtitorType = types.Values.FirstOrDefault((x) => x.Name.Contains(type.Name));
        EventCommandPropertyDrawerBase drawer;
        if (edtitorType != null)
            drawer = (EventCommandPropertyDrawerBase)Activator.CreateInstance(edtitorType);
        else
            drawer = new EventCommandPropertyDrawerBase();
        drawer.name = EventCommand.GetName(type.Name);
        return drawer;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EventCommandPropertyDrawerBase drawer;
        if (!drawers.TryGetValue(property, out drawer))
            drawers[property] = GetDrawer(property);

        drawers[property].OnGUI(position, property, label);
        property.serializedObject.ApplyModifiedProperties();
    }
}

public class EventCommandPropertyDrawerBase
{
    public string name;

    public virtual float GetPropertyHeight(SerializedProperty property)
    {
        float height = EditorGUI.GetPropertyHeight(property, true);
        return height;
    }

    public virtual void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        position.height = EditorGUIUtility.singleLineHeight;
        // Foldout的矩形寬度縮小
        Rect rect = position;
        rect.width = GUI.skin.label.fontSize * name.Length;
        property.isExpanded = EditorGUI.Foldout(rect, property.isExpanded, name, true);
        if (property.isExpanded)
        {
            EditorGUI.indentLevel += 1;
            float lastUsedHeight = EditorHelper.NextLine;
            IEnumerable<SerializedProperty> s = EditorHelper.GetChildrenProperty(property);
            foreach (SerializedProperty p in s)
            {
                position.y += lastUsedHeight;
                lastUsedHeight = EditorGUI.GetPropertyHeight(p, true);
                position.height = lastUsedHeight;
                EditorGUI.PropertyField(position, p, true);
                position.y += EditorGUIUtility.standardVerticalSpacing;
            }
            EditorGUI.indentLevel -= 1;
        }
    }
}

public class EventDialougePropertyDrawer : EventCommandPropertyDrawerBase
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        position.height = EditorGUIUtility.singleLineHeight;
        // Foldout的矩形寬度縮小
        Rect rect = position;
        rect.width = GUI.skin.label.fontSize * "Dialogue".Length;
        property.isExpanded = EditorGUI.Foldout(rect, property.isExpanded, "Dialogue", true);
        if (property.isExpanded)
        {
            EditorGUI.indentLevel += 1;
            position.y += EditorHelper.NextLine;
            EditorGUI.PropertyField(position, property.FindPropertyRelative("content"));
            EditorGUI.indentLevel -= 1;
        }
    }
}

public class EventGainItemPropertyDrawer : EventCommandPropertyDrawerBase
{
    private List<string> itemNames;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        position.height = EditorGUIUtility.singleLineHeight;
        // Foldout的矩形寬度縮小
        Rect rect = position;
        rect.width = GUI.skin.label.fontSize * "Gain Item".Length;
        property.isExpanded = EditorGUI.Foldout(rect, property.isExpanded, "Gain Item", true);
        if (property.isExpanded)
        {
            EditorGUI.indentLevel += 1;
            if (itemNames == null)
            {
                itemNames = new List<string>();
                foreach (KeyValuePair<int, Item> pair in GameDatabase.Instance.ItemDB)
                    itemNames.Add(pair.Key + ".  " + pair.Value.itemName);
            }
            SerializedProperty itemId = property.FindPropertyRelative("itemId");
            position.y += EditorHelper.NextLine;
            int selection = EditorGUI.Popup(position, "Item", itemId.intValue - 1, itemNames.ToArray());
            itemId.intValue = selection + 1;
            position.y += EditorHelper.NextLine;
            EditorGUI.PropertyField(position, property.FindPropertyRelative("number"));
            EditorGUI.indentLevel -= 1;
        }
    }


}

public class EventSetSwitchPropertyDrawer : EventCommandPropertyDrawerBase
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EventSetSwitch target = EditorHelper.GetObj(property) as EventSetSwitch;
        SerializedObject serializedObject = property.serializedObject;
        Type type = target.GetType();

        position.height = EditorGUIUtility.singleLineHeight;
        // Foldout的矩形寬度縮小
        Rect rect = position;
        rect.width = GUI.skin.label.fontSize * EventCommand.GetName(type.Name).Length;
        property.isExpanded = EditorGUI.Foldout(rect, property.isExpanded, EventCommand.GetName(type.Name), true);
        if (property.isExpanded)
        {
            EditorGUI.indentLevel += 1;
            List<string> switchNames = new List<string>();
            foreach (KeyValuePair<int, EventSwitches> pair in EditorDatabase.Instance.EventSwitchesDB)
                switchNames.Add(pair.Key + ".  " + pair.Value.name);
            FieldInfo field = type.GetField("switchId");
            position.y += EditorHelper.NextLine;
            field.SetValue(target, EditorGUI.Popup(position, "Switch", (int)field.GetValue(target) - 1, switchNames.ToArray()) + 1);
            position.y += EditorHelper.NextLine;
            EditorGUI.PropertyField(position, property.FindPropertyRelative("open"));
            EditorGUI.indentLevel -= 1;
        }
    }
}

public class EventTransitionPropertyDrawer : EventCommandPropertyDrawerBase
{
    private List<EventObject> objects;
    private List<string> objectNames;

    public override float GetPropertyHeight(SerializedProperty property)
    {
        float height = EditorGUI.GetPropertyHeight(property, true);
        height += EditorGUIUtility.standardVerticalSpacing;
        if (property.isExpanded)
            height += EditorHelper.NextLine;
        return height;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EventTransition target = EditorHelper.GetObj(property) as EventTransition;
        SerializedObject serializedObject = property.serializedObject;
        Type type = target.GetType();

        position.height = EditorGUIUtility.singleLineHeight;
        // Foldout的矩形寬度縮小
        Rect rect = position;
        rect.width = GUI.skin.label.fontSize * EventCommand.GetName(type.Name).Length;
        property.isExpanded = EditorGUI.Foldout(rect, property.isExpanded, EventCommand.GetName(type.Name), true);
        if (property.isExpanded)
        {
            EditorGUI.indentLevel += 1;
            if (objects == null)
            {
                objectNames = new List<string>();
                objects = new List<EventObject>(GameObject.FindObjectsOfType<EventObject>());
                objects.RemoveAll(x => x.guid == null || x.guid == "");
                EventObject player = objects.Find(x => x.CompareTag("Player"));
                objects.Remove(player);
                objects.Insert(0, player);
                int indexPrefix = 1;
                foreach (EventObject eventObject in objects)
                    objectNames.Add((indexPrefix++).ToString() + " " + eventObject.name);
            }
            FieldInfo field = type.GetField("targetId");
            position.y += EditorHelper.NextLine;
            int nowSelection = objects.FindIndex(x => x.guid == (string)field.GetValue(target));
            if (nowSelection < 0)
                nowSelection = 0;
            int selection = EditorGUI.Popup(position, "Target", nowSelection, objectNames.ToArray());
            field.SetValue(target, objects[selection].guid);
            position.y += EditorHelper.NextLine;
            Vector2 targetPosition = objects[nowSelection].transform.position;
            EditorGUI.LabelField(position, "Now", "X: " + targetPosition.x + "\tY: " + targetPosition.y);
            position.y += EditorHelper.NextLine;
            EditorGUI.PropertyField(position, property.FindPropertyRelative("destination"));
            position.y += EditorHelper.NextLine;
            EditorGUI.PropertyField(position, property.FindPropertyRelative("speed"), new GUIContent("Speed", "If 0, object will transition immediately."));
            position.y += EditorHelper.NextLine;
            EditorGUI.PropertyField(position, property.FindPropertyRelative("cameraPosition"));
            EditorGUI.indentLevel -= 1;
        }
    }
}

public class EventConditionBranchPropertyDrawer : EventCommandPropertyDrawerBase
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EventConditionBranch target = EditorHelper.GetObj(property) as EventConditionBranch;
        SerializedObject serializedObject = property.serializedObject;
        Type type = target.GetType();

        position.height = EditorGUIUtility.singleLineHeight;
        // Foldout的矩形寬度縮小
        Rect rect = position;
        rect.width = GUI.skin.label.fontSize * EventCommand.GetName(type.Name).Length;
        property.isExpanded = EditorGUI.Foldout(rect, property.isExpanded, EventCommand.GetName(type.Name), true);
        if (property.isExpanded)
        {
            EditorGUI.indentLevel += 1;
            List<string> switchNames = new List<string>();
            foreach (KeyValuePair<int, EventSwitches> pair in EditorDatabase.Instance.EventSwitchesDB)
                switchNames.Add(pair.Key + ".  " + pair.Value.name);
            FieldInfo field = type.GetField("switchId");
            position.y += EditorHelper.NextLine;
            field.SetValue(target, EditorGUI.Popup(position, "Switch", (int)field.GetValue(target) - 1, switchNames.ToArray()) + 1);
            position.y += EditorHelper.NextLine;
            EditorGUI.PropertyField(position, property.FindPropertyRelative("needOn"));
            position.y += EditorHelper.NextLine;
            position.height = EditorGUI.GetPropertyHeight(property.FindPropertyRelative("conditionOkCommands"), true);
            EditorGUI.PropertyField(position, property.FindPropertyRelative("conditionOkCommands"));
            position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
            position.height = EditorGUI.GetPropertyHeight(property.FindPropertyRelative("conditionNotOkCommands"), true);
            EditorGUI.PropertyField(position, property.FindPropertyRelative("conditionNotOkCommands"));
            EditorGUI.indentLevel -= 1;
        }
    }
}

public class EventSetDirectionPropertyDrawer : EventCommandPropertyDrawerBase
{
    private List<EventObject> objects;
    private List<string> objectNames;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EventSetDirection target = EditorHelper.GetObj(property) as EventSetDirection;
        SerializedObject serializedObject = property.serializedObject;
        Type type = target.GetType();

        position.height = EditorGUIUtility.singleLineHeight;
        // Foldout的矩形寬度縮小
        Rect rect = position;
        rect.width = GUI.skin.label.fontSize * EventCommand.GetName(type.Name).Length;
        property.isExpanded = EditorGUI.Foldout(rect, property.isExpanded, EventCommand.GetName(type.Name), true);
        if (property.isExpanded)
        {
            EditorGUI.indentLevel += 1;
            if (objects == null)
            {
                objectNames = new List<string>();
                objects = new List<EventObject>(GameObject.FindObjectsOfType<EventObject>());
                objects.RemoveAll(x => x.guid == null || x.guid == "");
                EventObject player = objects.Find(x => x.CompareTag("Player"));
                objects.Remove(player);
                objects.Insert(0, player);
                int indexPrefix = 1;
                foreach (EventObject eventObject in objects)
                    objectNames.Add((indexPrefix++).ToString() + " " + eventObject.name);
            }
            FieldInfo field = type.GetField("targetId");
            position.y += EditorHelper.NextLine;
            int nowSelection = objects.FindIndex(x => x.guid == (string)field.GetValue(target));
            if (nowSelection < 0)
                nowSelection = 0;
            int selection = EditorGUI.Popup(position, "Target", nowSelection, objectNames.ToArray());
            field.SetValue(target, objects[selection].guid);
            position.y += EditorHelper.NextLine;
            EditorGUI.PropertyField(position, property.FindPropertyRelative("direction"));
            EditorGUI.indentLevel -= 1;
        }
    }
}