using MoreEyes.Components;
using Photon.Pun;
using UnityEngine;
using static MoreEyes.Utility.Enums;

namespace MoreEyes.Addons
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Animator))]
    public class NetworkedAnimationTrigger : MonoBehaviour
    {
        [Header("Key Binding")]
        [Tooltip("Choose a Numpad key to trigger this animation.")]
        public NumpadKey triggerKey = NumpadKey.Keypad5;

        [Header("Animator Parameter")]
        [Tooltip("Name of the Animator parameter to modify.")]
        public string paramName = "MyBool";

        public enum ParamType { Bool, Trigger, Float, Int }
        public ParamType paramType = ParamType.Bool;

        [Tooltip("Value to set if Float parameter type is selected.")]
        public float floatValue = 0f;

        [Tooltip("Value to set if Int parameter type is selected.")]
        public int intValue = 0;

        private Animator animator;

        private void Awake()
        {
            animator = GetComponent<Animator>();
        }

        private void Update()
        {
            if (Input.GetKeyDown((KeyCode)triggerKey))
            {
                TriggerAnimation();
            }
        }

        private void TriggerAnimation()
        {
            if (animator == null || string.IsNullOrEmpty(paramName)) return;

            switch (paramType)
            {
                case ParamType.Bool:
                    bool current = animator.GetBool(paramName);
                    animator.SetBool(paramName, !current);
                    SendNetwork(paramType, paramName, !current);
                    break;

                case ParamType.Trigger:
                    animator.SetTrigger(paramName);
                    SendNetwork(paramType, paramName);
                    break;

                case ParamType.Float:
                    animator.SetFloat(paramName, floatValue);
                    SendNetwork(paramType, paramName, floatValue);
                    break;

                case ParamType.Int:
                    animator.SetInteger(paramName, intValue);
                    SendNetwork(paramType, paramName, intValue);
                    break;
            }
        }

        private void SendNetwork(ParamType type, string name, object value = null)
        {
            if (SemiFunc.RunIsLevel() && MoreEyesNetwork.instance != null && MoreEyesNetwork.instance.photonView != null)
            {
                string playerID = PhotonNetwork.LocalPlayer.UserId;
                MoreEyesNetwork.instance.photonView.RPC("RPC_SyncAnimatorParam", RpcTarget.Others, playerID, name, (int)type, value);
            }
        }
    }
}