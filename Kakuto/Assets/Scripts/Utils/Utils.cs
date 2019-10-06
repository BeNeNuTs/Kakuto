using UnityEngine;
using UnityEditor;
using System.Linq;

public static class Utils
{
    public static T GetCurrentBehaviour<T>(this GameObject gameObject) where T : AdvancedStateMachineBehaviour
    {
        Animator animator = gameObject.GetComponent<Animator>();
        if(animator)
        {
            return animator.GetBehaviours<T>().ToList().First(behaviour => behaviour.StateInfo.fullPathHash == animator.GetCurrentAnimatorStateInfo(0).fullPathHash);
        }

        return null;
    }

    public static bool IsInLayerMask(int layer, LayerMask layermask)
    {
        return layermask == (layermask | (1 << layer));
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

    public static PlayerEventManager<T> GetPlayerEventManager<T>(EPlayer player)
    {
        return GetPlayerEventManager<T>(player.ToString());
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
        return GetEnemyEventManager<T>(gameObject.tag);
    }

    public static PlayerEventManager<T> GetEnemyEventManager<T>(string tag)
    {
        string enemyTag = GetEnemyTag(tag);
        return GetPlayerEventManager<T>(enemyTag);
    }

    public static string GetEnemyTag(GameObject gameObject)
    {
        return GetEnemyTag(gameObject.tag);
    }

    public static string GetEnemyTag(string tag)
    {
        switch (tag)
        {
            case "Player1":
                return "Player2";
            case "Player2":
                return "Player1";
            default:
                Debug.LogError("Can't find enemy from tag : " + tag);
                return null;
        }
    }
}
