using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.Assertions;

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
    OnRefillHP,

    ApplyGrabDamages,
    EndOfGrab,
    EndOfParry,
    ParrySuccess,

    TriggerProjectile,
    ProjectileSpawned,
    ProjectileDestroyed,

    StopMovement,

    TriggerJumpImpulse,
    ApplyDashImpulse,

    SyncGrabAttackerPosition,

    TriggerTeleport,

    EndOfRoundAnimation,
    HitNotification,

    // For enemy
    Hit,
    GrabTry,
    GrabTouched,
    GrabBlocked,
    Grabbed,
    SyncGrabbedPosition,
    ProximityBox,
}

public class Player1EventManager : PlayerEventManager
{
    private static Player1EventManager s_Instance;

    public static Player1EventManager Instance
    {
        get
        {
            if (s_Instance == null)
            {
                s_Instance = new Player1EventManager();
            }

            return s_Instance;
        }
    }
}

public class Player2EventManager : PlayerEventManager
{
    private static Player2EventManager s_Instance;

    public static Player2EventManager Instance
    {
        get
        {
            if (s_Instance == null)
            {
                s_Instance = new Player2EventManager();
            }

            return s_Instance;
        }
    }
}

public abstract class PlayerEventManager
{
    private readonly Dictionary<EPlayerEvent, Action<BaseEventParameters>> m_EventDictionary = new Dictionary<EPlayerEvent, Action<BaseEventParameters>>();

    public void StartListening(EPlayerEvent eventType, Action<BaseEventParameters> listener)
    {
        if (m_EventDictionary.TryGetValue(eventType, out Action<BaseEventParameters> thisEvent))
        {
            thisEvent += listener;
            m_EventDictionary[eventType] = thisEvent;
        }
        else
        {
            thisEvent += listener;
            m_EventDictionary.Add(eventType, thisEvent);
        }
    }

    public void StopListening(EPlayerEvent eventType, Action<BaseEventParameters> listener)
    {
        if (m_EventDictionary.TryGetValue(eventType, out Action<BaseEventParameters> thisEvent))
        {
            thisEvent -= listener;
            if (thisEvent == null)
            {
                m_EventDictionary.Remove(eventType);
            }
            else
            {
                m_EventDictionary[eventType] = thisEvent;
            }
        }
    }

    public void TriggerEvent(EPlayerEvent eventType, BaseEventParameters eventParams = null)
    {
        if (m_EventDictionary.TryGetValue(eventType, out Action<BaseEventParameters> thisEvent))
        {
            thisEvent.Invoke(eventParams);
        }
    }
}