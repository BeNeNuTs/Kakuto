using UnityEngine;

public class PlayerBaseSubComponent
{
    protected GameObject m_Owner;

    public PlayerBaseSubComponent(GameObject owner)
    {
        m_Owner = owner;
    }

    public virtual void OnDestroy() { }
}
