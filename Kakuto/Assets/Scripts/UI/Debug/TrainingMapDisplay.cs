using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TrainingMapDisplay : MonoBehaviour
{
#if UNITY_EDITOR
    private static readonly float s_DisplayAttacksTime = 2.0f;
#endif

    public EPlayer m_Target;

    //Training display
    public GameObject m_GameInputList;
    public Text m_TextInputs;
    public Text m_TextToDisplayInputs;
    public Text m_TextInputsAttack;
    public Text m_TextToDisplayInputsAttack;
    public Text m_TextAttacks;
    public Text m_TextToDisplayAttacks;
    

    private PlayerMovementComponent m_PlayerMovementComponentToDisplay;
    private PlayerAttackComponent m_PlayerAttackComponentToDisplay;
    private PlayerInfoComponent m_PlayerInfoComponentToDisplay;
    

    private List<GameInput> m_AttackInputs = new List<GameInput>();
    private List<List<GameInput>> m_TriggeredInputs;
    private List<List<Image>> m_GameInputsImage;

    private float m_DislayAttacksTimeStamp;
    private PlayerAttack m_CurrentDisplayAttack;

    private DebugSettings m_DebugSettings;

    private bool m_IsGamePaused = false;

    void Awake()
    {
        m_PlayerMovementComponentToDisplay = Utils.FindComponentMatchingWithTag<PlayerMovementComponent>(m_Target.ToString());
        m_PlayerAttackComponentToDisplay = Utils.FindComponentMatchingWithTag<PlayerAttackComponent>(m_Target.ToString());
        m_PlayerInfoComponentToDisplay = Utils.FindComponentMatchingWithTag<PlayerInfoComponent>(m_Target.ToString());

        m_DebugSettings = ScenesConfig.GetDebugSettings();
        m_GameInputList.SetActive(m_DebugSettings.m_DisplayInputsInfo);

        m_TriggeredInputs = new List<List<GameInput>>(m_GameInputList.transform.childCount);
        m_GameInputsImage = new List<List<Image>>(m_GameInputList.transform.childCount);
        for (int i = 0; i < m_GameInputList.transform.childCount; i++)
        {
            List<Image> gameInputImageList = new List<Image>();
            Transform gameInputs = m_GameInputList.transform.GetChild(i);
            for (int j = 0; j < gameInputs.childCount; j++)
            {
                Image gameInputImage = gameInputs.GetChild(j).GetComponent<Image>();
                gameInputImage.sprite = null;
                gameInputImage.enabled = false;
                gameInputImageList.Add(gameInputImage);
            }

            m_GameInputsImage.Add(gameInputImageList);
        }

#if !UNITY_EDITOR
        m_TextInputs.enabled = false;
        m_TextToDisplayInputs.enabled = false;
        m_TextInputsAttack.enabled = false;
        m_TextToDisplayInputsAttack.enabled = false;
        m_TextAttacks.enabled = false;
        m_TextToDisplayAttacks.enabled = false;
#endif
    }

    void LateUpdate()
    {
        if(!GamePauseMenuComponent.IsInPause)
        {
            // If game was paused, skip this frame in order to not display back/select input
            if(m_IsGamePaused)
            {
                m_IsGamePaused = false;
                return;
            }

            bool displayInputsInfo = m_DebugSettings.m_DisplayInputsInfo;
            m_GameInputList.SetActive(displayInputsInfo);

#if UNITY_EDITOR
            m_TextInputs.enabled = displayInputsInfo;
            m_TextToDisplayInputs.enabled = displayInputsInfo;
            m_TextInputsAttack.enabled = displayInputsInfo;
            m_TextToDisplayInputsAttack.enabled = displayInputsInfo;
#endif
            if (displayInputsInfo)
            {
                DisplayInputs();
            }

#if UNITY_EDITOR
            bool displayAttacksInfo = m_DebugSettings.m_DisplayAttacksInfo;
            m_TextAttacks.enabled = displayAttacksInfo;
            m_TextToDisplayAttacks.enabled = displayAttacksInfo;
            if (displayAttacksInfo)
            {
                DisplayAttack();
            }
#endif
        }
        else
        {
            m_IsGamePaused = true;
        }
    }

    void DisplayInputs()
    {
        InputManager.GetAttackInputList(m_PlayerInfoComponentToDisplay.GetPlayerIndex(), m_PlayerMovementComponentToDisplay.IsLeftSide(), ref m_AttackInputs);
        if (m_AttackInputs.Count > 0)
        {
            // LB is not assigned
            m_AttackInputs.RemoveAll(input => input.GetInputKey() == PlayerGamePad.K_DEFAULT_UNASSIGNED_INPUT);
        }

        if (m_AttackInputs.Count > 0)
        {
            m_TriggeredInputs.Add(new List<GameInput>(m_AttackInputs));
            if (m_TriggeredInputs.Count > m_GameInputList.transform.childCount)
            {
                m_TriggeredInputs.RemoveRange(0, (int)(m_TriggeredInputs.Count - m_GameInputList.transform.childCount));
            }

            int triggeredInputIndex;
            for (int i = 0; i < m_TriggeredInputs.Count; i++)
            {
                triggeredInputIndex = (m_TriggeredInputs.Count - 1) - i;
                Transform gameInputs = m_GameInputList.transform.GetChild(i);
                for (int j = 0; j < gameInputs.childCount; j++)
                {
                    if(j < m_TriggeredInputs[triggeredInputIndex].Count)
                    {
                        if (m_TriggeredInputs[triggeredInputIndex][j].GetInputKey() == EInputKey.LT)
                        {
                            // Check that it remains at least 2 gameinputs image to fill, else stop now
                            if (j < m_GameInputsImage[i].Count - 2)
                            {
                                InputUIInfo inputInfoA = UIConfig.Instance.GetAssociatedInputUIInfo(EInputKey.B);
                                m_GameInputsImage[i][j].sprite = inputInfoA.m_GameSprite;
                                m_GameInputsImage[i][j].enabled = true;

                                j++;
                                InputUIInfo inputInfoB = UIConfig.Instance.GetAssociatedInputUIInfo(EInputKey.A);
                                m_GameInputsImage[i][j].sprite = inputInfoB.m_GameSprite;
                                m_GameInputsImage[i][j].enabled = true;
                            }
                            else
                            {
                                break;
                            }
                        }
                        else if(m_TriggeredInputs[triggeredInputIndex][j].GetInputKey() == EInputKey.RT)
                        {
                            // Check that it remains at least 2 gameinputs image to fill, else stop now
                            if (j < m_GameInputsImage[i].Count - 2)
                            {
                                InputUIInfo inputInfoY = UIConfig.Instance.GetAssociatedInputUIInfo(EInputKey.Y);
                                m_GameInputsImage[i][j].sprite = inputInfoY.m_GameSprite;
                                m_GameInputsImage[i][j].enabled = true;

                                j++;
                                InputUIInfo inputInfoX = UIConfig.Instance.GetAssociatedInputUIInfo(EInputKey.X);
                                m_GameInputsImage[i][j].sprite = inputInfoX.m_GameSprite;
                                m_GameInputsImage[i][j].enabled = true;
                            }
                            else
                            {
                                break;
                            }
                        }
                        else
                        {
                            InputUIInfo inputInfo = UIConfig.Instance.GetAssociatedInputUIInfo(m_TriggeredInputs[triggeredInputIndex][j].GetInputKey());
                            if (inputInfo != null)
                            {
                                m_GameInputsImage[i][j].sprite = inputInfo.m_GameSprite;
                                m_GameInputsImage[i][j].enabled = true;
                            }
                        }
                    }
                    else
                    {
                        m_GameInputsImage[i][j].enabled = false;
                    }
                }
            }

#if UNITY_EDITOR
            if (m_TextToDisplayInputs != null)
            {
                m_TextToDisplayInputs.text = string.Empty;
                for (int i = 0; i < m_TriggeredInputs.Count; i++)
                {
                    for (int j = 0; j < m_TriggeredInputs[i].Count; j++)
                    {
                        m_TextToDisplayInputs.text += m_TriggeredInputs[i][j].GetInputString();
                    }
                }
            }

            if (m_TextToDisplayInputsAttack != null)
            {
                m_TextToDisplayInputsAttack.text = m_PlayerAttackComponentToDisplay.GetTriggeredInputsString();
            }
#endif
        }
    }

#if UNITY_EDITOR
    void DisplayAttack()
    {
        if (m_TextToDisplayAttacks != null)
        {
            if (m_PlayerAttackComponentToDisplay.GetCurrentAttack() != null && m_PlayerAttackComponentToDisplay.GetCurrentAttack() != m_CurrentDisplayAttack)
            {
                m_TextToDisplayAttacks.text = m_PlayerAttackComponentToDisplay.GetCurrentAttack().m_Name + " launched !";
                m_DislayAttacksTimeStamp = Time.time;
                m_CurrentDisplayAttack = m_PlayerAttackComponentToDisplay.GetCurrentAttack();
            }
            else if (Time.time > m_DislayAttacksTimeStamp + s_DisplayAttacksTime)
            {
                ResetDisplayAttack();
            }
        }
    }

    void ResetDisplayAttack()
    {
        if (m_TextToDisplayAttacks != null)
        {
            m_TextToDisplayAttacks.text = "";
            m_DislayAttacksTimeStamp = 0.0f;
            m_CurrentDisplayAttack = null;
        }
    }

#endif
}
