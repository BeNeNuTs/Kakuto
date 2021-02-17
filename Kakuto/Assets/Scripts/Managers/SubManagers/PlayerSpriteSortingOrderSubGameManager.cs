using System.Collections;
using System.Threading;
using UnityEngine;

public class PlayerSpriteSortingOrderSubGameManager : SubGameManagerBase
{
    enum ESortingOrder
    {
        Back,
        Front
    }

    SpriteRenderer[] m_PlayerSpriteRendererList = { null, null };

    public override void Init()
    {
        base.Init();
        RegisterListeners(EPlayer.Player1);
        RegisterListeners(EPlayer.Player2);
        RoundSubGameManager.OnRoundOver += OnRoundOver;
    }

    void RegisterListeners(EPlayer player)
    {
        Utils.GetPlayerEventManager(player).StartListening(EPlayerEvent.AttackLaunched, OnAttackLaunched);
        Utils.GetPlayerEventManager(player).StartListening(EPlayerEvent.Grabbed, OnGrabbed);
        Utils.GetPlayerEventManager(player).StartListening(EPlayerEvent.DamageTaken, OnDamageTaken);
    }

    void UnregisterListeners(EPlayer player)
    {
        Utils.GetPlayerEventManager(player).StopListening(EPlayerEvent.AttackLaunched, OnAttackLaunched);
        Utils.GetPlayerEventManager(player).StopListening(EPlayerEvent.Grabbed, OnGrabbed);
        Utils.GetPlayerEventManager(player).StopListening(EPlayerEvent.DamageTaken, OnDamageTaken);
    }

    public override void OnPlayerRegistered(GameObject player)
    {
        base.OnPlayerRegistered(player);

        SpriteRenderer playerSpriteRenderer = player.GetComponentInChildren<SpriteRenderer>();
        bool isPlayer1 = player.CompareTag(Player.Player1);
        m_PlayerSpriteRendererList[isPlayer1 ? 0 : 1] = playerSpriteRenderer;

        UpdateSortingOrder(playerSpriteRenderer, isPlayer1 ? ESortingOrder.Front : ESortingOrder.Back); // Set player1 in front by default
    }

    public override void OnPlayerUnregistered(GameObject playerGO)
    {
        base.OnPlayerUnregistered(playerGO);

        EPlayer player = playerGO.CompareTag(Player.Player1) ? EPlayer.Player1 : EPlayer.Player2;
        m_PlayerSpriteRendererList[(int)player] = null;
    }

    public override void Shutdown()
    {
        base.Shutdown();
        UnregisterListeners(EPlayer.Player1);
        UnregisterListeners(EPlayer.Player2);
        RoundSubGameManager.OnRoundOver -= OnRoundOver;
    }

    void UpdateSortingOrder(EPlayer player, ESortingOrder order)
    {
        UpdateSortingOrder(m_PlayerSpriteRendererList[(int)player], order);
    }

    void UpdateSortingOrder(GameObject player, ESortingOrder order)
    {
        UpdateSortingOrder(m_PlayerSpriteRendererList[player.CompareTag(Player.Player1) ? 0 : 1], order);
    }

    void UpdateSortingOrder(SpriteRenderer renderer, ESortingOrder order)
    {
        if(renderer != null)
        {
            renderer.sortingOrder = (int)order;
        }
    }

    void OnAttackLaunched(BaseEventParameters baseParams)
    {
        AttackLaunchedEventParameters attackLaunchedParams = (AttackLaunchedEventParameters)baseParams;

        GameObject attacker = attackLaunchedParams.m_AttackLogic.GetOwner();
        GameObject defender = GetEnemyOf(attacker);
        UpdateSortingOrder(attacker, ESortingOrder.Front);
        UpdateSortingOrder(defender, ESortingOrder.Back);
    }

    void OnGrabbed(BaseEventParameters baseParams)
    {
        GrabbedEventParameters grabbedParams = (GrabbedEventParameters)baseParams;

        GameObject grabber = grabbedParams.m_AttackLogic.GetOwner();
        GameObject grabbed = GetEnemyOf(grabber);
        UpdateSortingOrder(grabber, ESortingOrder.Front);
        UpdateSortingOrder(grabbed, ESortingOrder.Back);
    }

    void OnDamageTaken(BaseEventParameters baseParams)
    {
        DamageTakenEventParameters damageTakenInfo = (DamageTakenEventParameters)baseParams;

        GameObject instigator = damageTakenInfo.m_AttackLogic.GetOwner();
        GameObject victim = damageTakenInfo.m_Victim;
        UpdateSortingOrder(instigator, ESortingOrder.Front);
        UpdateSortingOrder(victim, ESortingOrder.Back);
    }

    void OnRoundOver(RoundSubGameManager.ELastRoundWinner lastRoundWinner)
    {
        switch (lastRoundWinner)
        {
            case RoundSubGameManager.ELastRoundWinner.Both:
            case RoundSubGameManager.ELastRoundWinner.Player1:
                UpdateSortingOrder(EPlayer.Player1, ESortingOrder.Front);
                UpdateSortingOrder(EPlayer.Player2, ESortingOrder.Back);
                break;
            case RoundSubGameManager.ELastRoundWinner.Player2:
                UpdateSortingOrder(EPlayer.Player2, ESortingOrder.Front);
                UpdateSortingOrder(EPlayer.Player1, ESortingOrder.Back);
                break;
        }
    }

    GameObject GetEnemyOf(GameObject player)
    {
        foreach(GameObject currentPlayer in GameManager.Instance.GetPlayers())
        {
            if(currentPlayer != player)
            {
                return currentPlayer;
            }
        }

        KakutoDebug.LogError("Enemy of " + player + " has not been found.");
        return null;
    }
}
