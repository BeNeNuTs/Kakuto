using UnityEngine;
using System.Collections.Generic;

public static class PlayerAnimationHelper
{
    public static readonly string K_GRAB_MISS_ANIM_ORIGINAL_NAME = "Grab_Missed";
    public static readonly string K_GRAB_MISS_ANIM_STANDARD_NAME = "throwwhiff";

    public static readonly string K_GRAB_CANCEL_ANIM_ORIGINAL_NAME = "Grab_Cancelled";
    public static readonly string K_GRAB_CANCEL_ANIM_STANDARD_NAME = "throwtechreceive";

    public static readonly string K_PARRY_ANIM_ORIGINAL_NAME = "Parry";
    public static readonly string K_PARRY_ANIM_STANDARD_NAME = "parry_whiff";

    public static readonly string K_PARRY_SUCCESS_ANIM_ORIGINAL_NAME = "ParryCounterAttack";
    public static readonly string K_PARRY_SUCCESS_ANIM_STANDARD_NAME = "parry_success";

    public static void CheckAnimationsNaming(GameObject gameObject)
    {
        Animator anim = gameObject.GetComponentInChildren<Animator>();
        if(anim != null)
        {
            Dictionary<string, string> listOfAnimtionsToFind = new Dictionary<string, string>
            {
                { K_GRAB_MISS_ANIM_ORIGINAL_NAME, K_GRAB_MISS_ANIM_STANDARD_NAME },
                { K_GRAB_CANCEL_ANIM_ORIGINAL_NAME, K_GRAB_CANCEL_ANIM_STANDARD_NAME },
                { K_PARRY_ANIM_ORIGINAL_NAME, K_PARRY_ANIM_STANDARD_NAME },
                { K_PARRY_SUCCESS_ANIM_ORIGINAL_NAME, K_PARRY_SUCCESS_ANIM_STANDARD_NAME }
            };

            AnimatorOverrideController overrideController = anim.runtimeAnimatorController as AnimatorOverrideController;
            if (overrideController != null)
            {
                List<KeyValuePair<AnimationClip, AnimationClip>> animationsOverrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
                overrideController.GetOverrides(animationsOverrides);

                bool animationFound = false;
                foreach (KeyValuePair<string, string> animToFind in listOfAnimtionsToFind)
                {
                    animationFound = false;
                    foreach (KeyValuePair<AnimationClip, AnimationClip> animOverride in animationsOverrides)
                    {
                        if (animOverride.Key != null && animOverride.Value != null)
                        {
                            if (animationFound)
                            {
                                if (animOverride.Key.name.ToLower() == animToFind.Key.ToLower())
                                {
                                    Debug.LogError(animToFind.Key + " animation has already been found !");
                                    continue;
                                }   
                            }

                            if (animOverride.Key.name.ToLower() == animToFind.Key.ToLower())
                            {
                                if (!animOverride.Value.name.ToLower().Contains(animToFind.Value.ToLower()))
                                {
                                    Debug.LogError("Animation related to " + animToFind.Key + " is named : " + animOverride.Value.name + " and does not contain '" + animToFind.Value + "'");
                                }
                                animationFound = true;
                            }
                            else if (animOverride.Value.name.ToLower().Contains(animToFind.Value.ToLower()))
                            {
                                Debug.LogError("Animation " + animOverride.Value.name + " is also containing naming '" + animToFind.Value + "'");
                                Debug.LogError("There should be only one animation containing that naming");
                            }
                        }
                    }

                    if (!animationFound)
                    {
                        Debug.LogError(animToFind.Key + " animation has not been found !");
                    }
                }
            }
            else
            {
                Debug.LogError(anim.gameObject + " doesn't use an AnimatorOverrideController");
            }
        }
        else
        {
            Debug.LogError("Can't find an Animator on " + gameObject.name);
        }
        
    }
}
