using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;

[CustomPropertyDrawer(typeof(ItemMixSet))]
public class ItemMixSetDrawer : PropertyDrawer
{
    private List<string> itemNames = new List<string>();

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, true);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        position.height = EditorGUIUtility.singleLineHeight;

        property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, label, true);

        if (property.isExpanded)
        {
            if (itemNames.Count != GameDatabase.Instance.ItemDB.Count)
            {
                itemNames = new List<string>();
                foreach (KeyValuePair<int, Item> pair in GameDatabase.Instance.ItemDB)
                {
                    itemNames.Add(pair.Key + " " + pair.Value.itemName);
                }
            }

            EditorGUI.indentLevel += 1;
            position.y += EditorHelper.NextLine;
            DrawPopUp(position, property.FindPropertyRelative("item1Id"), "item1Id");
            position.y += EditorHelper.NextLine;
            DrawPopUp(position, property.FindPropertyRelative("item2Id"), "item2Id");
            position.y += EditorHelper.NextLine;
            DrawPopUp(position, property.FindPropertyRelative("resultId"), "resultId");
            position.y += EditorHelper.NextLine;
            SerializedProperty commands = property.FindPropertyRelative("commands");
            position.height = EditorGUI.GetPropertyHeight(commands, true);
            EditorGUI.PropertyField(position, commands, true);
            EditorGUI.indentLevel -= 1;
        }

        EditorGUI.EndProperty();
    }

    private void DrawPopUp(Rect position, SerializedProperty property, string fieldName)
    {
        int nowItemId = property.intValue;
        int selectItemId = EditorGUI.Popup(position, fieldName, nowItemId - 1, itemNames.ToArray());
        property.intValue = selectItemId + 1;
    }

}
