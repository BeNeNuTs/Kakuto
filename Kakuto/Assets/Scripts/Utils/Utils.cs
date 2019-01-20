using UnityEngine;
using UnityEditor;

public static class Utils {

    /// <summary>
    /// Get string representation of serialized property, even for non-string fields
    /// </summary>
    public static string AsStringValue(this SerializedProperty property)
    {
        switch (property.propertyType)
        {
            case SerializedPropertyType.String:
                return property.stringValue;
            case SerializedPropertyType.Character:
            case SerializedPropertyType.Integer:
                if (property.type == "char") return System.Convert.ToChar(property.intValue).ToString();
                return property.intValue.ToString();
            case SerializedPropertyType.ObjectReference:
                return property.objectReferenceValue != null ? property.objectReferenceValue.ToString() : "null";
            case SerializedPropertyType.Boolean:
                return property.boolValue.ToString();
            case SerializedPropertyType.Enum:
                return property.enumNames[property.enumValueIndex];
            default:
                return string.Empty;
        }
    }

    public static T FindComponentMatchingWithTag<T>(string tag) where T : MonoBehaviour
    {
        T[] components = Object.FindObjectsOfType<T>();
        foreach (T comp in components)
        {
            if (comp.gameObject.CompareTag(tag))
            {
                return comp;
            }
        }

        return null;
    }

    public static PlayerEventManager<T> GetPlayerEventManager<T>(GameObject gameObject)
    {
        return GetPlayerEventManager<T>(gameObject.tag);
    }

    public static PlayerEventManager<T> GetPlayerEventManager<T>(string tag)
    {
        switch (tag)
        {
            case "Player1":
                return Player1EventManager<T>.Instance;
            case "Player2":
                return Player2EventManager<T>.Instance;
            default:
                Debug.LogError("Can't find PlayerEventManager from tag : " + tag);
                return null;
        }
    }

    public static PlayerEventManager<T> GetEnemyEventManager<T>(GameObject gameObject)
    {
        string enemyTag = GetEnemyTag(gameObject);
        return GetPlayerEventManager<T>(enemyTag);
    }

    public static string GetEnemyTag(GameObject gameObject)
    {
        switch (gameObject.tag)
        {
            case "Player1":
                return "Player2";
            case "Player2":
                return "Player1";
            default:
                Debug.LogError("Can't find enemy from tag : " + gameObject.tag);
                return null;
        }
    }
}
