using UnityEngine;

public class AttackTriggerDebugDisplay : MonoBehaviour
{
    void OnGUI()
    {
        // Display Trigger : Non Super Projectile Guard Crush for PLAYER 1 ///////////////
        if (PlayerProjectileAttackLogic.IsNextNonSuperProjectileGuardCrush(0))
        {
            DisplayNextNonSuperProjectileGuardCrush(0);
        }

        // Display Trigger : Non Super Projectile Guard Crush for PLAYER 2 ///////////////
        if (PlayerProjectileAttackLogic.IsNextNonSuperProjectileGuardCrush(1))
        {
            DisplayNextNonSuperProjectileGuardCrush(1);
        }
    }

    void DisplayNextNonSuperProjectileGuardCrush(int index)
    {
        int w = Screen.width, h = Screen.height;

        // Display Trigger : Non Super Projectile Guard Crush ///////////////
        {
            GUIStyle style = new GUIStyle
            {
                alignment = TextAnchor.UpperLeft,
                fontSize = h * 2 / 100
            };
            style.normal.textColor = new Color(0.0f, 0.0f, 0.5f, 1.0f);
            GUI.Label(new Rect(100 + index * 500, 35, w, style.fontSize), "Next Non Super Projectile Is Guard Crush", style);
        }
        //////////////////////////////
    }
}