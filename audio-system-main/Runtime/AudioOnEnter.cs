using UnityEngine;
using Ineor.Utils.AudioSystem;

/// <summary>
/// A stateMachineBehaviour script which can be attached to an animation state to
/// make a sound when entering the state.
/// The script will play a random sound from the collection.
/// Can be used for footsteps, weapon sounds...
/// </summary>
public class AudioOnEnter : StateMachineBehaviour {
    [SerializeField] private AudioCollection _collection;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) =>
        AudioSystem.Instance.PlaySound(_collection, _collection.AudioClip, animator.transform.position);
}