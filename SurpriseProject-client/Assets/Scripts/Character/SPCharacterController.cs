using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.TopDownEngine;
using Mirror;

public class SPCharacterController : TopDownController3D
{
    [SerializeField] NetworkIdentity identity;

    public override void Update()
    {
        if (identity && !identity.hasAuthority) return;
        base.Update();
    }
}
