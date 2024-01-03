using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsGroundedComponent : MonoBehaviour
{
    public bool isGrounded { get { return collision_count > 0; } }
    private int collision_count = 0;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.isTrigger)
            collision_count++;
    }

    private void OnTriggerExit(UnityEngine.Collider other)
    {
        if (!other.isTrigger)
            collision_count--;
    }
}
