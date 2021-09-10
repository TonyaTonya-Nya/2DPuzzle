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
}

[CustomPropertyDrawer(typeof(EventCommandList))]
public class EventCommandListPropertyDrawer : PropertyDrawer
{
    private SerializedProperty propertyCommands;

    private int nowCommandIndex;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        propertyCommands = property.FindPropertyRelative("commands");
        float spacing = propertyCommands.arraySize * EditorGUIUtility.standardVerticalSpacing;
        float height = EditorGUI.GetPropertyHeight(propertyCommands, true) + spacing + EditorGUIUtility.standardVerticalSpacing;
        // 由於 List 含有 size 這個 property，手動刪除
        //height -= EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        return height;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // 繪製方形，至少包含整層
        EditorGUI.indentLevel -= 1;
        EditorGUI.DrawRect(EditorGUI.IndentedRect(position), new Color(0.5f, 0.5f, 0.5f, 0.5f));
        EditorGUI.indentLevel += 1;

        propertyCommands = property.FindPropertyRelative("commands");

        EditorGUI.PropertyField(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), propertyCommands, false);

        EventCommandList self = EditorHelper.GetObj(property) as EventCommandList;

        EditorGUI.indentLevel += 1;
        if (propertyCommands.isExpanded)
        {
            DrawEventCommands(self, position);
        }
        EditorGUI.indentLevel -= 1;
    }

    public void DrawEventCommands(EventCommandList self, Rect position)
    {
        GenericMenu menu = new GenericMenu();
        GenericMenu insertMenu = new GenericMenu();

        foreach (KeyValuePair<string, Type> pair in EventCommand.types)
        {
            menu.AddItem(new GUIContent("Add/" + EventCommand.GetName(pair.Key)), false, () => self.InsertAt(nowCommandIndex, pair.Value));
            insertMenu.AddItem(new GUIContent(EventCommand.GetName(pair.Key)), false, () => self.InsertAt(nowCommandIndex, pair.Value));
        }
        menu.AddItem(new GUIContent("Remove"), false, () => self.RemoveAt(nowCommandIndex - 1));

        EditorGUI.indentLevel += 1;

        position.height = EditorGUIUtility.singleLineHeight;

        for (int i = 0; i < propertyCommands.arraySize; i++)
        {
            SerializedProperty command = propertyCommands.GetArrayElementAtIndex(i);

            float height = EditorGUI.GetPropertyHeight(command, true);
            position.y += position.height + EditorGUIUtility.standardVerticalSpacing * 2;
            position.height = height;

            Rect buttonRect = position;
            float newW = buttonRect.width / 7;
            buttonRect.x = buttonRect.width - newW + buttonRect.x;
            buttonRect.width = newW;
            buttonRect.height = EditorGUIUtility.singleLineHeight;
            buttonRect.y += EditorGUIUtility.standardVerticalSpacing;
            if (GUI.Button(buttonRect, "Delete"))
            {
                self.RemoveAt(i);
                return;
            }
            buttonRect.x = buttonRect.x - buttonRect.width - EditorGUIUtility.standardVerticalSpacing * 2;
            if (GUI.Button(buttonRect, "Insert"))
            {
                nowCommandIndex = i;
                insertMenu.ShowAsContext();
            }

            // Unity Bug in version 2019.4.21, this function always return false.
            EditorGUI.PropertyField(position, command, false);
        }

        EditorGUI.indentLevel -= 1;

        position.y += position.height + EditorGUIUtility.standardVerticalSpacing * 2;
        position.height = EditorGUIUtility.singleLineHeight;

        // 繪製方形，至少包含整層
        EditorGUI.indentLevel -= 2;
        EditorGUI.DrawRect(EditorGUI.IndentedRect(position), new Color(0f, 0f, 0f, 0.5f));
        EditorGUI.indentLevel += 2;

        EditorGUI.LabelField(position, "Right click to add new command or remove command.");
        if (CheckRightClick(position))
        {
            nowCommandIndex = propertyCommands.arraySize;
            if (nowCommandIndex < 0)
                nowCommandIndex = 0;
            menu.ShowAsContext();
        }
    }

    public bool CheckRightClick(Rect rect)
    {
        if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
            return Event.current.button == 1;
        return false;
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
        object target = EditorHelper.GetObj(property);
        SerializedObject serializedObject = new SerializedObject((UnityEngine.Object)target);
        Type type = target.GetType();
        List<FieldInfo> fields = GetFields(type);
        int number = fields.Count;

        float height = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        if (property.isExpanded)
            height += EditorGUI.GetPropertyHeight(property, true) * number + EditorGUIUtility.standardVerticalSpacing * (number * 2);
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
        SerializedObject serializedObject = new SerializedObject((UnityEngine.Object)target);
        Type type = target.GetType();
        List<FieldInfo> fields = GetFields(type);
        serializedObject.Update();
        position.height = EditorGUIUtility.singleLineHeight;
        property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, EventCommand.GetName(type.Name), true);
        if (property.isExpanded)
        {
            EditorGUI.indentLevel += 1;
            foreach (FieldInfo field in fields)
            {
                position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing * 2;
                position.height = EditorGUIUtility.singleLineHeight;
                if (serializedObject.FindProperty(field.Name) != null)
                    EditorGUI.PropertyField(position, serializedObject.FindProperty(field.Name));
            }
            EditorGUI.indentLevel -= 1;
        }
        serializedObject.ApplyModifiedProperties();
    }
}

public class EventGainItemPropertyDrawer : EventCommandPropertyDrawerBase
{
    public override float GetPropertyHeight(SerializedProperty property)
    {
        float height = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        if (property.isExpanded)
            height += EditorGUI.GetPropertyHeight(property, true) * 2 + EditorGUIUtility.standardVerticalSpacing * 4;
        return height;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedObject serializedObject = new SerializedObject((UnityEngine.Object)EditorHelper.GetObj(property));
        Type type = serializedObject.targetObject.GetType();
        serializedObject.Update();
        position.height = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, EventCommand.GetName(type.Name), true);
        EventGainItem target = serializedObject.targetObject as EventGainItem;
        if (property.isExpanded)
        {
            EditorGUI.indentLevel += 1;
            List<string> itemNames = new List<string>();
            foreach (KeyValuePair<int, Item> pair in GameDatabase.Instance.ItemDB)
                itemNames.Add(pair.Key + ".  " + pair.Value.itemName);
            FieldInfo field = type.GetField("itemId");
            position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing * 2;
            field.SetValue(target, EditorGUI.Popup(position, "Item", (int)field.GetValue(target), itemNames.ToArray()));
            position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing * 2;
            EditorGUI.PropertyField(position, serializedObject.FindProperty("number"));
            EditorGUI.indentLevel -= 1;
        }
        serializedObject.ApplyModifiedProperties();
    }


}

public class EventSetSwitchPropertyDrawer : EventCommandPropertyDrawerBase
{
    public override float GetPropertyHeight(SerializedProperty property)
    {
        float height = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        if (property.isExpanded)
            height += EditorGUI.GetPropertyHeight(property, true) * 2 + EditorGUIUtility.standardVerticalSpacing * 4;
        return height;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedObject serializedObject = new SerializedObject((UnityEngine.Object)EditorHelper.GetObj(property));
        Type type = serializedObject.targetObject.GetType();
        serializedObject.Update();
        position.height = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, EventCommand.GetName(type.Name), true);
        EventSetSwitch target = serializedObject.targetObject as EventSetSwitch;
        if (property.isExpanded)
        {
            EditorGUI.indentLevel += 1;
            List<string> switchNames = new List<string>();
            foreach (KeyValuePair<int, EventSwitches> pair in EditorDatabase.Instance.EventSwitchesDB)
                switchNames.Add(pair.Key + ".  " + pair.Value.name);
            FieldInfo field = type.GetField("switchId");
            position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing * 2;
            field.SetValue(target, EditorGUI.Popup(position, "Switch", (int)field.GetValue(target), switchNames.ToArray()));
            position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing * 2;
            EditorGUI.PropertyField(position, serializedObject.FindProperty("open"));
            EditorGUI.indentLevel -= 1;
        }
        serializedObject.ApplyModifiedProperties();
    }
}

public class EventTransitionPropertyDrawer : EventCommandPropertyDrawerBase
{
    public override float GetPropertyHeight(SerializedProperty property)
    {
        float height = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        if (property.isExpanded)
            height += EditorGUI.GetPropertyHeight(property, true) * 3 + EditorGUIUtility.standardVerticalSpacing * 6;
        return height;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedObject serializedObject = new SerializedObject((UnityEngine.Object)EditorHelper.GetObj(property));
        Type type = serializedObject.targetObject.GetType();
        serializedObject.Update();
        position.height = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, EventCommand.GetName(type.Name), true);
        EventTransition target = serializedObject.targetObject as EventTransition;
        if (property.isExpanded)
        {
            EditorGUI.indentLevel += 1;
            List<string> objectNames = new List<string>();
            List<EventObject> objects = new List<EventObject>(GameObject.FindObjectsOfType<EventObject>());
            objects.Sort(delegate (EventObject x, EventObject y)
            {
                if (x.Id > y.Id) return 1;
                if (x.Id < y.Id) return -1;
                return 0;
            });
            foreach (EventObject eventObject in objects)
                objectNames.Add((eventObject.Id).ToString() + " " + eventObject.name);
            FieldInfo field = type.GetField("targetName");
            position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing * 2;
            int nowSelection = objectNames.IndexOf((string)field.GetValue(target));
            if (nowSelection < 0)
                nowSelection = 0;
            int selection = EditorGUI.Popup(position, nowSelection, objectNames.ToArray());
            field.SetValue(target, objectNames[selection]);
            position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing * 2;
            EditorGUI.PropertyField(position, serializedObject.FindProperty("destination"));
            position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing * 2;
            EditorGUI.PropertyField(position, serializedObject.FindProperty("speed"), new GUIContent("Speed", "If set -1, object will transition immediately."));
            EditorGUI.indentLevel -= 1;
        }
        serializedObject.ApplyModifiedProperties();
    }
}

public class EventSelectionPropertyDrawer : EventCommandPropertyDrawerBase
{
    public override float GetPropertyHeight(SerializedProperty property)
    {
        float height = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        if (property.isExpanded)
        {
            height += EditorGUI.GetPropertyHeight(property, true) * 4 + EditorGUIUtility.standardVerticalSpacing * 8;

            SerializedObject serializedObject = new SerializedObject((UnityEngine.Object)EditorHelper.GetObj(property));
            SerializedProperty p = serializedObject.FindProperty("selection1Commands");
            SerializedProperty c = p.FindPropertyRelative("commands");
            // 如果展開，計算展開的高度，但扣除掉前面已經算過的標題
            if (c.isExpanded)
                height += EditorGUI.GetPropertyHeight(p) - EditorGUIUtility.singleLineHeight;
            p = serializedObject.FindProperty("selection2Commands");
            c = p.FindPropertyRelative("commands");
            // 如果展開，計算展開的高度，但扣除掉前面已經算過的標題
            if (c.isExpanded)
                height += EditorGUI.GetPropertyHeight(p) - EditorGUIUtility.singleLineHeight;
        }
        return height;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedObject serializedObject = new SerializedObject((UnityEngine.Object)EditorHelper.GetObj(property));
        Type type = serializedObject.targetObject.GetType();
        serializedObject.Update();
        position.height = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, EventCommand.GetName(type.Name), true);
        EventSelection target = serializedObject.targetObject as EventSelection;
        if (property.isExpanded)
        {
            EditorGUI.indentLevel += 1;
            position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing * 2;
            // 選項標題
            EditorGUI.PropertyField(position, serializedObject.FindProperty("selection1"));
            // PropertyComandList 為 EventCommadList
            SerializedProperty propertyComandList = serializedObject.FindProperty("selection1Commands");
            // PropertyCommands 為 EventCommadList.commands
            SerializedProperty propertyCommands = propertyComandList.FindPropertyRelative("commands");
            position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing * 2;
            // 繪製整個 EvenyCommandList
            EditorGUI.PropertyField(position, propertyComandList);
            position.y += EditorGUI.GetPropertyHeight(propertyCommands) + EditorGUIUtility.standardVerticalSpacing * 2;
            // 選項標題
            EditorGUI.PropertyField(position, serializedObject.FindProperty("selection2"));
            position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing * 2;
            EditorGUI.PropertyField(position, serializedObject.FindProperty("selection2Commands"));
            EditorGUI.indentLevel -= 1;
        }
        serializedObject.ApplyModifiedProperties();
    }
}