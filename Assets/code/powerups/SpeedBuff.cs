using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Timeline.Actions;
using UnityEngine;

[CreateAssetMenu(menuName = "Powerups/SpeedBuff")]
public class SpeedBuff : PowerupEffect
{
    public float amount;
    
    public override void Apply(GameObject target)
    {
        var controller = target.GetComponent<FirstPersonController>();
        controller.moveSpeed += amount;

    }

    public override void Revert(GameObject target)
    {
        var controller = target.GetComponent<FirstPersonController>();
        controller.moveSpeed -= amount;
    }
}
