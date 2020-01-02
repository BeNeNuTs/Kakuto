using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

public enum EPlayer
{
    Player1,
    Player2
}

public static class Player
{
    public const string Player1 = "Player1";
    public const string Player2 = "Player2";
}

public enum EPlayerEvent
{
    // For self
    AttackLaunched,
    BlockAttack,
    UnblockAttack,
    BlockMovement,
    UnblockMovement,
    EndOfAttack,

    StunBegin,
    OnStunAnimEnd,
    StunEnd,
    DamageTaken,
    OnDeath,

    ApplyGrabDamages,
    EndOfGrab,

    TriggerProjectile,
    ProjectileSpawned,
    ProjectileDestroyed,

    StopMovement,

    TriggerJumpImpulse,

    // For enemy
    Hit,
    GrabTry,
    GrabTouched,
    GrabBlocked,
    Grabbed,
}

public class Player1EventManager<T> : PlayerEventManager<T>
{
    private static Player1EventManager<T> s_Instance;

    public static Player1EventManager<T> Instance
    {
        get
        {
            if (s_Instance == null)
            {
                s_Instance = new Player1EventManager<T>();
            }

            return s_Instance;
        }
    }
}

public class Player2EventManager<T> : PlayerEventManager<T>
{
    private static Player2EventManager<T> s_Instance;

    public static Player2EventManager<T> Instance
    {
        get
        {
            if (s_Instance == null)
            {
                s_Instance = new Player2EventManager<T>();
            }

            return s_Instance;
        }
    }
}

public abstract class PlayerEventManager<T>
{
    protected class Event : UnityEvent<T>
    {}

    private Dictionary<EPlayerEvent, Event> m_EventDictionary;

    protected PlayerEventManager()
    {
        if (m_EventDictionary == null)
        {
            m_EventDictionary = new Dictionary<EPlayerEvent, Event>();
        }
    }

    public void StartListening(EPlayerEvent eventType, UnityAction<T> listener)
    {
        Event thisEvent = null;
        if (m_EventDictionary.TryGetValue(eventType, out thisEvent))
        {
            thisEvent.AddListener(listener);
        }
        else
        {
            thisEvent = new Event();
            thisEvent.AddListener(listener);
            m_EventDictionary.Add(eventType, thisEvent);
        }
    }

    public void StopListening(EPlayerEvent eventType, UnityAction<T> listener)
    {
        Event thisEvent = null;
        if (m_EventDictionary.TryGetValue(eventType, out thisEvent))
        {
            thisEvent.RemoveListener(listener);
        }
    }

    public void TriggerEvent(EPlayerEvent eventType, T param)
    {
        Event thisEvent = null;
        if (m_EventDictionary.TryGetValue(eventType, out thisEvent))
        {
            thisEvent.Invoke(param);
        }
    }
}