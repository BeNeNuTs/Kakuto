using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationEventHandler : MonoBehaviour
{
    public void EndOfAttack(string attackName)
    {
        transform.parent.SendMessage("EndOfAttack", attackName);
    }
}
