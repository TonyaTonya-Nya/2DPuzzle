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

}

[CustomPropertyDrawer(typeof(EventCommandList))]
public class EventCommandListPropertyDrawer : PropertyDrawer
{
    private SerializedProperty self;

    private int nowSelectIndex;

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

        self = property;

        position.height = EditorGUIUtility.singleLineHeight;

        SerializedProperty commands = self.FindPropertyRelative("commands");

        commands.isExpanded = EditorGUI.Foldout(position, commands.isExpanded, label, true);

        if (commands.isExpanded)
        {
            EditorGUI.indentLevel += 1;
            float lastUsedHeight = EditorHelper.NextLine;
            for (int i = 0; i < commands.arraySize; i++)
            {
                SerializedProperty singleCommand = commands.GetArrayElementAtIndex(i);
                position.y += lastUsedHeight;
                EventCommandPropertyDrawer drawer = new EventCommandPropertyDrawer();
                drawer.OnGUI(position, singleCommand, new GUIContent(singleCommand.name));
                lastUsedHeight = EditorGUI.GetPropertyHeight(singleCommand, true);
                position.y += EditorGUIUtility.standardVerticalSpacing;

                GenericMenu menu = CreateMenu(false);
                Rect buttonRect = position;
                buttonRect.x = buttonRect.x + buttonRect.width - buttonRect.width / 7;
                buttonRect.width = buttonRect.width / 7;

                if (GUI.Button(buttonRect, "Delete"))
                {
                    EventCommandList eventCommandList = EditorHelper.GetObj(self) as EventCommandList;
                    eventCommandList.RemoveAt(i);
                    break;
                }
                buttonRect.x = buttonRect.x - buttonRect.width - EditorGUIUtility.standardVerticalSpacing;
                if (GUI.Button(buttonRect, "Duplicate"))
                {
                    nowSelectIndex = i;
                    EventCommandList eventCommandList = EditorHelper.GetObj(self) as EventCommandList;
                    eventCommandList.Insert(nowSelectIndex + 1, eventCommandList[eventCommandList.Count - 1].GetType());
                }
                buttonRect.x = buttonRect.x - buttonRect.width - EditorGUIUtility.standardVerticalSpacing;
                if (GUI.Button(buttonRect, "Insert"))
                {
                    nowSelectIndex = i;
                    menu.ShowAsContext();
                }
            }
            position.y += lastUsedHeight;// + EditorGUIUtility.standardVerticalSpacing * 8;
            DrawEditMenu(position);
            EditorGUI.indentLevel -= 1;
        }
    }

    private GenericMenu CreateMenu(bool hasRemoveButton)
    {
        GenericMenu menu = new GenericMenu();

        EventCommandList eventCommandList = EditorHelper.GetObj(self) as EventCommandList;

        foreach (KeyValuePair<string, Type> pair in EventCommand.types)
        {
            menu.AddItem(new GUIContent("Add/" + EventCommand.GetName(pair.Key)), false, () => eventCommandList.Insert(nowSelectIndex, pair.Value));
        }
        if (hasRemoveButton)
            menu.AddItem(new GUIContent("Remove"), false, () => eventCommandList.RemoveAt(nowSelectIndex - 1));
        return menu;
    }

    private void DrawEditMenu(Rect position)
    {
        GenericMenu menu = CreateMenu(true);
        EditorGUI.indentLevel -= 2;
        EditorGUI.DrawRect(EditorGUI.IndentedRect(position), new Color(0f, 0f, 0f, 0.5f));
        EditorGUI.indentLevel += 2;

        EditorGUI.LabelField(position, "Right click to add new command or remove command.");
        if (EditorHelper.CheckRightClick(position))
        {
            nowSelectIndex = self.FindPropertyRelative("commands").arraySize;
            if (nowSelectIndex < 0)
                nowSelectIndex = 0;
            menu.ShowAsContext();
        }
    }
}

[CustomPropertyDrawer(typeof(EventCommand))]
public class EventCommandPropertyDrawer : PropertyDrawer
{
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
        if (edtitorType != null)
            return (EventCommandPropertyDrawerBase)Activator.CreateInstance(edtitorType);
        return new EventCommandPropertyDrawerBase();
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EventCommandPropertyDrawerBase drawer = GetDrawer(property);

        drawer.OnGUI(position, property, label);
    }
}

public class EventCommandPropertyDrawerBase
{
    public virtual float GetPropertyHeight(SerializedProperty property)
    {
        float height = EditorGUI.GetPropertyHeight(property, true);
        return height;
    }

    public List<FieldInfo> GetFields(Type type)
    {
        List<FieldInfo> fields;
        fields = new List<FieldInfo>(type.GetFields());
        fields.RemoveAll(x => x.GetCustomAttribute(typeof(HideInInspector)) != null);
        return fields;
    }

    public virtual void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        object target = EditorHelper.GetObj(property);
        SerializedObject serializedObject = property.serializedObject;
        Type type = target.GetType();
        List<FieldInfo> fields = GetFields(type);

        position.height = EditorGUIUtility.singleLineHeight;
        // Foldout的矩形寬度縮小
        Rect rect = position;
        rect.width = GUI.skin.label.fontSize * EventCommand.GetName(type.Name).Length;
        property.isExpanded = EditorGUI.Foldout(rect, property.isExpanded, EventCommand.GetName(type.Name), true);
        if (property.isExpanded)
        {
            EditorGUI.indentLevel += 1;
            float lastUsedHeight = EditorHelper.NextLine;
            foreach (FieldInfo field in fields)
            {
                position.y += lastUsedHeight;
                SerializedProperty propertyFiled = property.FindPropertyRelative(field.Name);
                if (propertyFiled != null)
                {
                    EditorGUI.PropertyField(position, propertyFiled);
                    lastUsedHeight = EditorGUI.GetPropertyHeight(propertyFiled, true);
                    position.y += EditorGUIUtility.standardVerticalSpacing;
                }
            }
            EditorGUI.indentLevel -= 1;
        }
    }
}

public class EventGainItemPropertyDrawer : EventCommandPropertyDrawerBase
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EventGainItem target = EditorHelper.GetObj(property) as EventGainItem;
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
            List<string> itemNames = new List<string>();
            foreach (KeyValuePair<int, Item> pair in GameDatabase.Instance.ItemDB)
                itemNames.Add(pair.Key + ".  " + pair.Value.itemName);
            FieldInfo field = type.GetField("itemId");
            position.y += EditorHelper.NextLine;
            int selection = EditorGUI.Popup(position, "Item", (int)field.GetValue(target) - 1, itemNames.ToArray());
            field.SetValue(target, selection + 1);
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
            List<string> objectNames = new List<string>();
            List<EventObject> objects = new List<EventObject>(GameObject.FindObjectsOfType<EventObject>());
            objects.RemoveAll(x => x.id == 0);
            objects.Sort(delegate (EventObject x, EventObject y)
            {
                if (x.id > y.id) return 1;
                if (x.id < y.id) return -1;
                return 0;
            });
            foreach (EventObject eventObject in objects)
                objectNames.Add((eventObject.id).ToString() + " " + eventObject.name);
            FieldInfo field = type.GetField("targetId");
            position.y += EditorHelper.NextLine;
            int nowSelection = objects.FindIndex(x => x.id == (int)field.GetValue(target));
            if (nowSelection < 0)
                nowSelection = 0;
            int selection = EditorGUI.Popup(position, "Target", nowSelection, objectNames.ToArray()) + 1;
            field.SetValue(target, selection);
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