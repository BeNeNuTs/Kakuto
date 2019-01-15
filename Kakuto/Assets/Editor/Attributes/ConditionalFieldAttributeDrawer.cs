using System;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(ConditionalFieldAttribute))]
public class ConditionalFieldAttributeDrawer : PropertyDrawer
{
    private ConditionalFieldAttribute Attribute => _attribute ?? (_attribute = attribute as ConditionalFieldAttribute);

    private string[] PropertiesToCheck => Attribute != null ? _attribute.PropertiesToCheck : null;

    private object CompareValue => Attribute != null ? _attribute.CompareValue : null;

    private ConditionalFieldAttribute _attribute;
    private bool _toShow = true;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return _toShow ? EditorGUI.GetPropertyHeight(property) : 0;
    }

    // TODO: Skip array fields
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        foreach(string propertyToCheck in PropertiesToCheck)
        {
            if (!string.IsNullOrEmpty(propertyToCheck))
            {
                var conditionProperty = FindPropertyRelative(property, propertyToCheck);
                if (conditionProperty != null)
                {
                    bool isBoolMatch = conditionProperty.propertyType == SerializedPropertyType.Boolean && conditionProperty.boolValue;
                    string compareStringValue = CompareValue?.ToString().ToUpper() ?? "NULL";
                    if (isBoolMatch && compareStringValue == "FALSE") isBoolMatch = false;

                    string conditionPropertyStringValue = Utils.AsStringValue(conditionProperty).ToUpper();
                    bool objectMatch = compareStringValue == conditionPropertyStringValue;

                    if (!isBoolMatch && !objectMatch)
                    {
                        _toShow = false;
                        return;
                    }
                }
            }
        }

        _toShow = true;
        EditorGUI.PropertyField(position, property, label, true);
    }

    private SerializedProperty FindPropertyRelative(SerializedProperty property, string toGet)
    {
        if (property.depth == 0) return property.serializedObject.FindProperty(toGet);

        var path = property.propertyPath.Replace(".Array.data[", "[");
        var elements = path.Split('.');
        SerializedProperty parent = null;


        for (int i = 0; i < elements.Length - 1; i++)
        {
            var element = elements[i];
            int index = -1;
            if (element.Contains("["))
            {
                index = Convert.ToInt32(element.Substring(element.IndexOf("[", StringComparison.Ordinal)).Replace("[", "").Replace("]", ""));
                element = element.Substring(0, element.IndexOf("[", StringComparison.Ordinal));
            }

            parent = i == 0 ?
                property.serializedObject.FindProperty(element) :
                parent.FindPropertyRelative(element);

            if (index >= 0) parent = parent.GetArrayElementAtIndex(index);
        }

        return parent.FindPropertyRelative(toGet);
    }
}