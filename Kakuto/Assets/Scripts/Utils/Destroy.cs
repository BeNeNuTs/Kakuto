using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destroy : MonoBehaviour {

    public void AutoDestroyNow()
    {
        Destroy(gameObject);
    }

    public void AutoDestroyIn(float time)
    {
        Destroy(gameObject, time);
    }

    public void DestroyParentNow()
    {
        Destroy(transform.parent.gameObject);
    }

    public void DestroyParentIn(float time)
    {
        Destroy(transform.parent.gameObject, time);
    }

    public void DestroyRootNow()
    {
        Destroy(transform.root.gameObject);
    }

    public void DestroyRootIn(float time)
    {
        Destroy(transform.root.gameObject, time);
    }
}
