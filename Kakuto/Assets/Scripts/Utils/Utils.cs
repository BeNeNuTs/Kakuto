using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

public static class Utils
{
    public static Vector3 Round(this Vector3 v, int decimals = 1)
    {
        float RoundedX = (float)Math.Round(v.x, decimals);
        float RoundedY = (float)Math.Round(v.y, decimals);
        float RoundedZ = (float)Math.Round(v.z, decimals);

        return new Vector3(RoundedX, RoundedY, RoundedZ);
    }

    public static bool FindSubList<T>(this IList<T> list, GameInputList sublist)
    {
        for (int listIndex = 0; listIndex <= list.Count - sublist.Count; listIndex++)
        {
            int count = 0;
            while (count < sublist.Count && sublist[count].Equals(list[listIndex + count]))
                count++;
            if (count == sublist.Count)
                return true;
        }
        return false;
    }

    public static string ToStringList<T>(this IList<T> list)
    {
        string resultStr = "";
        foreach(T item in list)
        {
            resultStr += item.ToString() + " ";
        }

        return resultStr;
    }

    public static T GetCurrentBehaviour<T>(this GameObject gameObject) where T : AdvancedStateMachineBehaviour
    {
        Animator animator = gameObject.GetComponent<Animator>();
        if(animator)
        {
            T[] behaviours = animator.GetBehaviours<T>();
            if (behaviours != null && behaviours.Length > 0)
            {
                return behaviours.ToList().FirstOrDefault(behaviour => behaviour != null && behaviour.StateInfo.fullPathHash == animator.GetCurrentAnimatorStateInfo(0).fullPathHash);
            }
        }

        return null;
    }

    public static float GetCurrentAnimFrame(Animator anim)
    {
        GetCurrentAnimInfo(anim, out string clipName, out float currentFrame, out float frameCount);
        return currentFrame;
    }

    public static void GetCurrentAnimInfo(Animator anim, out string clipName, out float currentFrame, out float frameCount)
    {
        clipName = "";
        currentFrame = 0f;
        frameCount = 0f;

        AnimatorClipInfo[] clips = anim.GetCurrentAnimatorClipInfo(0);
        AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
        if (clips.Length > 0)
        {
            float clipLength = clips[0].clip.length;
            float clipFrameRate = clips[0].clip.frameRate;

            clipName = clips[0].clip.name;
            frameCount = clipLength * clipFrameRate;
            currentFrame = (frameCount * stateInfo.normalizedTime) % frameCount;
        }
    }

    public static bool IsInLayerMask(int layer, LayerMask layermask)
    {
        return layermask == (layermask | (1 << layer));
    }

    public static void IgnorePushBoxLayerCollision(bool ignore = true)
    {
        int pushBoxLayer = LayerMask.NameToLayer("PushBox");
        Physics2D.IgnoreLayerCollision(pushBoxLayer, pushBoxLayer, ignore);
    }

    public static T FindComponentMatchingWithTag<T>(string tag) where T : MonoBehaviour
    {
        T[] components = UnityEngine.Object.FindObjectsOfType<T>();
        foreach (T comp in components)
        {
            if (comp.gameObject.CompareTag(tag))
            {
                return comp;
            }
        }

        return null;
    }

    public static PlayerEventManager GetPlayerEventManager(GameObject gameObject)
    {
        return GetPlayerEventManager(gameObject.tag);
    }

    public static PlayerEventManager GetPlayerEventManager(EPlayer player)
    {
        return GetPlayerEventManager(player.ToString());
    }

    public static PlayerEventManager GetPlayerEventManager(string tag)
    {
        switch (tag)
        {
            case "Player1":
                return Player1EventManager.Instance;
            case "Player2":
                return Player2EventManager.Instance;
            default:
                Debug.LogError("Can't find PlayerEventManager from tag : " + tag);
                return null;
        }
    }

    public static PlayerEventManager GetEnemyEventManager(GameObject gameObject)
    {
        return GetEnemyEventManager(gameObject.tag);
    }

    public static PlayerEventManager GetEnemyEventManager(string tag)
    {
        string enemyTag = GetEnemyTag(tag);
        return GetPlayerEventManager(enemyTag);
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
