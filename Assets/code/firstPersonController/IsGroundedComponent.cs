using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsGroundedComponent : MonoBehaviour {
   [HideInInspector] public bool isGrounded;
   
   private void OnCollisionEnter(Collision other) {
       if (other.gameObject.CompareTag("Planet")) {
           isGrounded = true;
       }
   }

   private void OnCollisionStay(Collision other) {
       if (other.gameObject.CompareTag("Planet")) {
           isGrounded = true;
       } 
   }
   
   private void OnCollisionExit(Collision other) {
       if (other.gameObject.CompareTag("Planet")) {
           isGrounded = false;
       }
   }
}
