using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Powerups/HealthBuff")]
public class HealthBuff : PowerupEffect
{
    public override void Apply(GameObject target)
    {
        target.GetComponent<FirstPersonController>().Health += 20;
        HUDManager.Instance.UpdateHealth(target.GetComponent<FirstPersonController>().Health);
    }

    public override void Revert(GameObject target)
    {
    }
}
