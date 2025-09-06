using UnityEngine;

namespace MoreEyes.Addons;
[RequireComponent(typeof(Animator))]
[DisallowMultipleComponent]
public class SimpleAnimationTrigger : MonoBehaviour
{
    [Tooltip("Name of the bool parameter in the Animator to trigger.")]
    public string boolParamName = "MyBool";

    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Keypad5))
        {
            if (animator != null && !string.IsNullOrEmpty(boolParamName))
            {
                bool current = animator.GetBool(boolParamName);
                animator.SetBool(boolParamName, !current);
            }
        }
    }
}
