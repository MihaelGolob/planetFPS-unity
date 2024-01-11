using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Powerups/JumpBuff")]
public class JumpBuff : PowerupEffect
{
    public float amount;
    
    public override void Apply(GameObject target)
    {
        var controller = target.GetComponent<FirstPersonController>();
        controller.jumpHeight += amount;

    }

    public override void Revert(GameObject target)
    {
        var controller = target.GetComponent<FirstPersonController>();
        controller.jumpHeight -= amount;
    }
}
