using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunBobbing : MonoBehaviour
{
    [SerializeField] FirstPersonController fps_controller;
    [SerializeField] float transition_factor = 0.5f;
    [SerializeField] float bob_speed = 0.5f;
    [SerializeField] float bob_amplitude = 0.5f;

    [HideInInspector] public Vector3 target_pos = Vector3.zero;
    private float p = 0;

    void FixedUpdate()
    {
        //Malo zlorabljam lerp za tole heh
        transform.localPosition = Vector3.Lerp(transform.localPosition, target_pos, transition_factor);
        
		if (fps_controller.IsMoving && fps_controller.isGrounded)
        {
            p += bob_speed;
            while (p >= 2 * Mathf.PI)
                p -= 2 * Mathf.PI;
            target_pos = new Vector3(Mathf.Sin(p), Mathf.Sin(p * 0.5f) * 0.2f, 0) * bob_amplitude;
        }else
        {
            target_pos = Vector3.zero;
        }
    }
}
