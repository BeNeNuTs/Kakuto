using System.Collections.Generic;
using UnityEngine;

public class FXSubGameManager : SubGameManagerBase
{
    private Dictionary<EHitFXType, List<GameObject>> m_Player1HitFXInstances = new Dictionary<EHitFXType, List<GameObject>>();
    private Dictionary<EFXType, List<GameObject>> m_Player1OtherFXInstances = new Dictionary<EFXType, List<GameObject>>();

    private Dictionary<EHitFXType, List<GameObject>> m_Player2HitFXInstances = new Dictionary<EHitFXType, List<GameObject>>();
    private Dictionary<EFXType, List<GameObject>> m_Player2OtherFXInstances = new Dictionary<EFXType, List<GameObject>>();

    public override void Init()
    {
        base.Init();
        InitAllFX();
    }

    void InitAllFX()
    {
        AttackConfig attackConfig = AttackConfig.Instance;

        int hitFXCount = attackConfig.m_HitFX.Count;
        for (int i = 0; i < hitFXCount; i++)
        {
            GameObject player1HitFXInstance = GameObject.Instantiate(attackConfig.m_HitFX[i].m_FX);
            InitFXInstanceForPlayer(Player.Player1, ref player1HitFXInstance);
            m_Player1HitFXInstances.Add((EHitFXType)i, new List<GameObject>() { player1HitFXInstance });

            GameObject player2HitFXInstance = GameObject.Instantiate(attackConfig.m_HitFX[i].m_FX);
            InitFXInstanceForPlayer(Player.Player2, ref player2HitFXInstance);
            m_Player2HitFXInstances.Add((EHitFXType)i, new List<GameObject>() { player2HitFXInstance });
        }

        int otherFXCount = attackConfig.m_OtherFX.Count;
        for (int i = 0; i < otherFXCount; i++)
        {
            GameObject player1OtherFXInstance = GameObject.Instantiate(attackConfig.m_OtherFX[i].m_FX);
            InitFXInstanceForPlayer(Player.Player1, ref player1OtherFXInstance);
            m_Player1OtherFXInstances.Add((EFXType)i, new List<GameObject>() { player1OtherFXInstance });

            GameObject player2OtherFXInstance = GameObject.Instantiate(attackConfig.m_OtherFX[i].m_FX);
            InitFXInstanceForPlayer(Player.Player2, ref player2OtherFXInstance);
            m_Player2OtherFXInstances.Add((EFXType)i, new List<GameObject>() { player2OtherFXInstance });
        }
    }

    void InitFXInstanceForPlayer(string playerTag, ref GameObject fxInstance)
    {
        GameObject.DontDestroyOnLoad(fxInstance);
#if UNITY_EDITOR || DEBUG_DISPLAY
        fxInstance.name = playerTag + "_" + fxInstance.name;
#endif
        fxInstance.SetActive(false);
    }

    public void SpawnHitFX(int playerIndex, EHitFXType hitFXType, Vector3 position, Quaternion rotation, bool flipFX)
    {
        GameObject finalHitFXInstance = GetOrCreateFXInstance(hitFXType, AttackConfig.Instance.m_HitFX[(int)hitFXType].m_FX, (playerIndex == 0) ? Player.Player1 : Player.Player2, (playerIndex == 0) ? m_Player1HitFXInstances : m_Player2HitFXInstances);
        SpawnFX_Internal(finalHitFXInstance, position, rotation, flipFX);
    }

    public void SpawnOtherFX(int playerIndex, EFXType fXType, Vector3 position, Quaternion rotation, bool flipFX)
    {
        GameObject finalHitFXInstance = GetOrCreateFXInstance(fXType, AttackConfig.Instance.m_OtherFX[(int)fXType].m_FX, (playerIndex == 0) ? Player.Player1 : Player.Player2, (playerIndex == 0) ? m_Player1OtherFXInstances : m_Player2OtherFXInstances);
        SpawnFX_Internal(finalHitFXInstance, position, rotation, flipFX);
    }

    private GameObject GetOrCreateFXInstance<T>(T fXType, GameObject fxPrefab, string playerStr, Dictionary<T, List<GameObject>> fxInstances)
    {
        GameObject finalFXInstance = null;
        foreach (GameObject fXInstance in fxInstances[fXType])
        {
            if (!fXInstance.activeSelf)
            {
                finalFXInstance = fXInstance;
                break;
            }
        }

        if (finalFXInstance == null)
        {
            GameObject newFXInstance = GameObject.Instantiate(fxPrefab);
            InitFXInstanceForPlayer(playerStr, ref newFXInstance);
            fxInstances[fXType].Add(newFXInstance);
            finalFXInstance = newFXInstance;
        }

        return finalFXInstance;
    }

    private void SpawnFX_Internal(GameObject fxInstance, Vector3 position, Quaternion rotation, bool flipFX)
    {
        Vector3 localScale = Vector3.one;
        if (flipFX)
        {
            localScale.x *= -1f;
        }

        Transform fxInstanceTransform = fxInstance.transform;
        fxInstanceTransform.position = position;
        fxInstanceTransform.rotation = rotation;
        fxInstanceTransform.localScale = localScale;
        fxInstance.SetActive(true);
    }

    public void DestroyFX(GameObject fxInstance)
    {
        fxInstance.SetActive(false);
    }
}
