using UnityEngine;

public class AnimationsHandler : MonoBehaviour {
    public void RunAnimation(Animator animator, string AnimationName, float crossFade = 0.2f) {
        animator.CrossFade(AnimationName, crossFade);
    }
}