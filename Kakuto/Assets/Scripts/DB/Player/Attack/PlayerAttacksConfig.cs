using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "PlayerAttacksConfig", menuName = "Data/Player/Attacks/PlayerAttacksConfig", order = 0)]
public class PlayerAttacksConfig : BakeableScriptableObject
{
    [Tooltip("Contain the attack list of this player")]
    public List<PlayerAttack> m_AttackList;

#if UNITY_EDITOR
#pragma warning disable 414
    [ButtonAttribute("ComputeInputStringList", "Compute inputs", "Allow to compute final inputs according to Input string list.", false, false)]
    [SerializeField] bool m_ComputeInputs = false;

    [ButtonAttribute("SortAttackList", "Sort Attack List", "Allow to sort the attack list by input in order to avoid conflict.", false, false)]
    [SerializeField] bool m_SortAttackList = false;
#pragma warning restore 414

    bool m_IsDataBaked = false;
#endif

    public void Init()
    {
#if UNITY_EDITOR
        BakeData();
#endif
    }

#if UNITY_EDITOR
    public override void BakeData()
    {
        if (!m_IsDataBaked)
        {
            ComputeInputStringList();
            SortAttackList();
            m_IsDataBaked = true;
        }
    }
#endif

    public List<PlayerBaseAttackLogic> CreateLogics(PlayerAttackComponent playerAttackComponent)
    {
        List<PlayerBaseAttackLogic> attackLogics = new List<PlayerBaseAttackLogic>();
        foreach (PlayerAttack attack in m_AttackList)
        {
            if (attack.m_AttackConfig)
            {
                PlayerBaseAttackLogic attackLogic = attack.m_AttackConfig.CreateLogic();
                attackLogic.OnInit(playerAttackComponent.gameObject, attack);
                attackLogics.Add(attackLogic);
            }
            else
            {
                Debug.LogError("No attack config found for " + attack.m_Name);
            }
        }

        return attackLogics;
    }

    public void Shutdown()
    {
#if UNITY_EDITOR
        ResetBakeData();
#endif
    }

#if UNITY_EDITOR
    public override void ResetBakeData()
    {
        m_IsDataBaked = false;
    }
#endif

    private void ComputeInputStringList()
    {
        foreach (PlayerAttack attack in m_AttackList)
        {
            attack.ResetComputedInputString();
        }

        foreach (PlayerAttack attack in m_AttackList)
        {
            if (attack.IsInputStringComputed() == false)
            {
                ComputeInputStringList_Internal(attack);
            }   
        }
    }
    
    private void ComputeInputStringList_Internal(PlayerAttack attack)
    {
        attack.SetInputStringProcessing();

        var attackNameToInputs = new Dictionary<string, List<string>>();
        List<string> inputsAssociatedToAttack = new List<string>();

        // Find inputs associated to attack ///////////////////////////////////////////////////////
        foreach (string rawInputString in attack.GetRawInputStringList())
        {
            string rawInputStringWithoutEmptySpace = rawInputString.Replace(" ", string.Empty);
            string[] inputList = rawInputStringWithoutEmptySpace.Split('+');

            attack.m_ComputedInputStringList.Add(string.Concat<string>(inputList));

            foreach (string input in inputList)
            {
                // If input is more than 1 character, it's not a normal input
                if(input.Length > 1)
                {
                    string attackName = input;

                    if(attackNameToInputs.ContainsKey(attackName) == false)
                    {
                        foreach (PlayerAttack attack2 in m_AttackList)
                        {
                            if (attackName == attack2.m_Name)
                            {
                                // Catch error cases ///////////////
                                if (attack == attack2)
                                {
                                    Debug.LogError("An input of attack " + attack.m_Name + " is refering itself.");
                                    attack.SetInputStringComputed();
                                    return;
                                }

                                if (attack2.IsInputStringProcessing())
                                {
                                    Debug.LogError("Attacks " + attack.m_Name + " and " + attack2.m_Name + " inputs are refering each other");
                                    attack.SetInputStringComputed();
                                    return;
                                }
                                ///////////////////////////////////

                                if (attack2.IsInputStringComputed() == false)
                                {
                                    ComputeInputStringList_Internal(attack2);
                                }

                                inputsAssociatedToAttack = new List<string>(attack2.GetInputStringList());
                                break;
                            }
                        }

                        if (inputsAssociatedToAttack.Count > 0)
                        {
                            attackNameToInputs.Add(attackName, new List<string>(inputsAssociatedToAttack));
                            inputsAssociatedToAttack.Clear();
                        }
                    }
                }
            }
        }
        ///////////////////////////////////////////////////////////////////////////////////////////

        // Create X ComputedInputStringList according to the x variations of inputs for each attack
        int nbComputedInputInputStringNeeded = 1;
        foreach(var pair in attackNameToInputs)
        {
            nbComputedInputInputStringNeeded *= pair.Value.Count;
        }

        if (nbComputedInputInputStringNeeded > 1)
        {
            List<string> originalComputedStringList = new List<string>(attack.m_ComputedInputStringList);
            attack.m_ComputedInputStringList.Clear();
            foreach(string originalComputedString in originalComputedStringList)
            {
                for (int i = 0; i < nbComputedInputInputStringNeeded; i++)
                {
                    attack.m_ComputedInputStringList.Add(originalComputedString);
                }
            }
        }
        ////////////////////////////////////////////////////////////////////////////////////////////

        // Replace recursively attackName by the right inputs //////////////////////////////////////
        if (attackNameToInputs.Count > 0 && attack.m_ComputedInputStringList.Count > 0)
        {
            List<KeyValuePair<string, string>> listKeyValue = new List<KeyValuePair<string, string>>();
            int computedInputStringIndex = 0;
            for(int attackNameToInputsIndex = 0; attackNameToInputsIndex < attackNameToInputs.Count; attackNameToInputsIndex++)
            {
                if (computedInputStringIndex >= attack.m_ComputedInputStringList.Count)
                {
                    break;
                }

                ReplaceAttackNameByInputs(attack, attackNameToInputs, ref attackNameToInputsIndex, ref computedInputStringIndex, listKeyValue);
            }
        }
        ////////////////////////////////////////////////////////////////////////////////////////////

        attack.SetInputStringComputed();
    }

    private void ReplaceAttackNameByInputs(PlayerAttack attack, Dictionary<string, List<string>> attackNameToInputs, ref int attackNameToInputsIndex, ref int computedInputStringIndex, List<KeyValuePair<string, string>> listKeyValue)
    {
        var lastKeyItem = attackNameToInputs.Keys.Last();
        var currentItem = attackNameToInputs.ElementAt(attackNameToInputsIndex);

        var oldKeyValueList = listKeyValue;

        foreach (string input in currentItem.Value)
        {
            var newKeyValueList = new List<KeyValuePair<string, string>>(oldKeyValueList);
            newKeyValueList.Add(new KeyValuePair<string, string>(currentItem.Key, input));
            if (currentItem.Key != lastKeyItem)
            {
                attackNameToInputsIndex++;
                ReplaceAttackNameByInputs(attack, attackNameToInputs, ref attackNameToInputsIndex, ref computedInputStringIndex, newKeyValueList);
            }
            else
            {
                foreach(var pair in newKeyValueList)
                {
                    attack.ReplaceComputedInputString(computedInputStringIndex, pair.Key, pair.Value);
                }
                computedInputStringIndex++;
            }
        }

        attackNameToInputsIndex--;
    }

    private void SortAttackList()
    {
        m_AttackList.Sort(SortByInput);
        Debug.Log("Attack list sorted !");
    }

    static int SortByInput(PlayerAttack attack1, PlayerAttack attack2)
    {
        if (attack1.GetInputStringList().Count > 0 && attack2.GetInputStringList().Count > 0)
        {
            return attack2.GetInputStringList()[0].Length.CompareTo(attack1.GetInputStringList()[0].Length);
        }
        Debug.LogError("Attack list contains attack without input");
        return 0;
    }
}
 