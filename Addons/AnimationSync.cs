using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Photon.Pun;

namespace MoreEyes.Addons
{
    [RequireComponent(typeof(Animator))]
    [DisallowMultipleComponent]
    internal class AnimationSync : MonoBehaviour, IPunObservable
    {
        [Serializable]
        public class ParamEvent : UnityEvent<object> { }

        [Serializable]
        public class SyncedParam
        {
            [Tooltip("The name of the animator parameter to sync. Must match exactly.")]
            public string paramName = null;
            [Tooltip("The type of this animator parameter (Bool, Float, Int, Trigger).")]
            public AnimatorControllerParameterType paramType = new();

            [Header("Events (optional)")]
            [Tooltip("Invoked whenever the synced value updates. Useful for reacting in scripts without polling.")]
            public ParamEvent OnValueUpdated = null;
        }

        [Header("Settings")]
        [Tooltip("If enabled, animator values are synced on level load.")]
        public bool syncOnAwake = true;

        public List<SyncedParam> parametersToSync = [];

        private Animator animator;
        private PhotonView rootView;
        private readonly Dictionary<string, object> cachedValues = [];

        private void Awake()
        {
            animator = GetComponent<Animator>();
            rootView = transform.root.GetComponent<PhotonView>();

            foreach (var p in parametersToSync)
            {
                cachedValues[p.paramName] = GetParamValue(p.paramName, p.paramType);

                if (syncOnAwake)
                    SetParamValue(p.paramName, p.paramType, cachedValues[p.paramName]);
            }
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                foreach (var p in parametersToSync)
                    stream.SendNext(GetParamValue(p.paramName, p.paramType));
            }
            else
            {
                foreach (var p in parametersToSync)
                {
                    object newVal = stream.ReceiveNext();
                    cachedValues[p.paramName] = newVal;
                    SetParamValue(p.paramName, p.paramType, newVal);

                    p.OnValueUpdated?.Invoke(newVal);
                }
            }
        }

        private object GetParamValue(string name, AnimatorControllerParameterType type)
        {
            return type switch
            {
                AnimatorControllerParameterType.Bool => animator.GetBool(name),
                AnimatorControllerParameterType.Float => animator.GetFloat(name),
                AnimatorControllerParameterType.Int => animator.GetInteger(name),
                AnimatorControllerParameterType.Trigger => animator.GetBool(name),
                _ => null
            };
        }

        private void SetParamValue(string name, AnimatorControllerParameterType type, object value)
        {
            switch (type)
            {
                case AnimatorControllerParameterType.Bool:
                    animator.SetBool(name, (bool)value);
                    break;
                case AnimatorControllerParameterType.Float:
                    animator.SetFloat(name, (float)value);
                    break;
                case AnimatorControllerParameterType.Int:
                    animator.SetInteger(name, (int)value);
                    break;
                case AnimatorControllerParameterType.Trigger:
                    if ((bool)value)
                    {
                        animator.SetTrigger(name);
                        cachedValues[name] = false;
                    }
                    break;
            }
        }
    }
}
