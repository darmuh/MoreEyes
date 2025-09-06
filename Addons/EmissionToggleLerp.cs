using MoreEyes.Collections;
using MoreEyes.Utility;
using System.Collections;
using UnityEngine;
using static MoreEyes.Utility.Enums;

namespace MoreEyes.Addons
{
    [DisallowMultipleComponent]
    public class EmissionToggleLerp : MonoBehaviour
    {
        [Tooltip("Renderer to recolor (child or self).")]
        public Renderer targetRenderer;

        [Tooltip("Index of the material to affect.")]
        public int materialIndex = 0;

        [Tooltip("Color to set when toggled on.")]
        public Color triggeredColor = Color.red;

        [Tooltip("Time it takes to transition between colors.")]
        public float lerpDuration = 0.4f;

        [Tooltip("Delay before starting to reset color when toggled off.")]
        public float resetDelay = 0.15f;

        [Header("Eye Binding")]
        public EyeSide eyeSide = EyeSide.Left;   // Left or Right
        public EyePart eyePart = EyePart.Pupil;  // Pupil or Iris

        private bool isOn = false;
        private Coroutine currentRoutine;

        private Material TargetMaterial
        {
            get
            {
                if (targetRenderer == null || targetRenderer.materials.Length <= materialIndex)
                    return null;
                return targetRenderer.materials[materialIndex];
            }
        }

        private void Awake()
        {
            if (targetRenderer == null)
                targetRenderer = GetComponentInChildren<Renderer>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Keypad5))
            {
                Material mat = TargetMaterial;
                if (mat == null)
                    return;

                if (currentRoutine != null)
                    StopCoroutine(currentRoutine);

                if (!isOn)
                {
                    currentRoutine = StartCoroutine(
                        LerpColor(mat, mat.GetColor("_EmissionColor"), triggeredColor, lerpDuration)
                    );
                }
                else
                {
                    currentRoutine = StartCoroutine(DelayedReset(mat));
                }

                isOn = !isOn;
            }
        }

        private IEnumerator LerpColor(Material mat, Color start, Color end, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                mat.SetColor("_EmissionColor", Color.Lerp(start, end, elapsed / duration));
                elapsed += Time.deltaTime;
                yield return null;
            }
            mat.SetColor("_EmissionColor", end);
        }

        private IEnumerator DelayedReset(Material mat)
        {
            yield return new WaitForSeconds(resetDelay);
            Color resetColor = GetOriginalColorFromSelection();
            yield return LerpColor(mat, mat.GetColor("_EmissionColor"), resetColor, lerpDuration);
        }

        private Color GetOriginalColorFromSelection()
        {
            PlayerEyeSelection selected = PlayerEyeSelection.LocalCache;
            if (selected == null)
                return Color.black;

            return eyePart switch
            {
                EyePart.Pupil => (eyeSide == EyeSide.Left) ? selected.PupilLeftColor : selected.PupilRightColor,
                EyePart.Iris => (eyeSide == EyeSide.Left) ? selected.IrisLeftColor : selected.IrisRightColor,
                _ => Color.black,
            };
        }
    }
}