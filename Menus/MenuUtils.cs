using HarmonyLib;
using MenuLib.MonoBehaviors;
using MoreEyes.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MoreEyes.Menus;
internal static class MenuUtils
{
    private static Coroutine zoomCoroutine;

    public static void HandleScrollZoom(REPOAvatarPreview preview)
    {
        if (preview == null || preview.rigTransform == null)
            return;

        float scrollDelta = InputManager.instance.KeyPullAndPush();
        Plugin.Spam($"HandleScrollZoom called. ScrollDelta: {scrollDelta}");

        if (Mathf.Abs(scrollDelta) > 0.001f)
        {
            bool zoomIn = scrollDelta > 0f;
            Plugin.Spam($"Zoom direction: {(zoomIn ? "In" : "Out")}");

            if (zoomCoroutine != null)
            {
                Plugin.Spam("Stopping previous zoom coroutine.");
                preview.StopCoroutine(zoomCoroutine);
            }

            Plugin.Spam("Starting new zoom coroutine.");
            zoomCoroutine = preview.StartCoroutine(AnimateZoom(preview, zoomIn));
        }
    }

    private static IEnumerator AnimateZoom(REPOAvatarPreview preview, bool zoomIn)
    {
        Vector2 startSize = preview.previewSize;
        Vector2 targetSize = zoomIn ? new Vector2(266.6667f, 500f) : new Vector2(182.4f, 342f);

        Vector2 startDelta = preview.rectTransform.sizeDelta;
        Vector2 targetDelta = zoomIn ? new Vector2(266.6667f, 210f) : new Vector2(182.4f, 342f);

        Vector3 startScale = preview.rigTransform.parent.localScale;
        Vector3 targetScale = zoomIn ? new Vector3(2f, 2f, 2f) : Vector3.one;

        Vector3 startPos = preview.rigTransform.parent.localPosition;
        Vector3 targetPos = zoomIn ? new Vector3(0f, -3.5f, 0f) : new Vector3(0f, -0.6f, 0f);

        Vector2 startAnchored = preview.rectTransform.anchoredPosition;
        Vector2 targetAnchored = zoomIn ? new Vector2(471.25f, 156.5f) : new Vector2(471.25f, 24.5f);

        float duration = 0.45f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);

            preview.previewSize = Vector2.Lerp(startSize, targetSize, t);
            preview.rectTransform.sizeDelta = Vector2.Lerp(startDelta, targetDelta, t);
            preview.rigTransform.parent.localScale = Vector3.Lerp(startScale, targetScale, t);
            preview.rigTransform.parent.localPosition = Vector3.Lerp(startPos, targetPos, t);
            preview.rectTransform.anchoredPosition = Vector2.Lerp(startAnchored, targetAnchored, t);

            yield return null;
        }

        preview.previewSize = targetSize;
        preview.rectTransform.sizeDelta = targetDelta;
        preview.rigTransform.parent.localScale = targetScale;
        preview.rigTransform.parent.localPosition = targetPos;
        preview.rectTransform.anchoredPosition = targetAnchored;
    }
    public static void SetTextStyling(List<REPOButton> buttons)
    {
        buttons.Do(t =>
        {
            t.overrideButtonSize = new Vector2(75f, 20f);
            t.labelTMP.fontSize = 18f;
            t.labelTMP.horizontalAlignment = TMPro.HorizontalAlignmentOptions.Center;
        });
    }

    public static void SetHeaderTextStyling(List<REPOLabel> labels)
    {
        labels.Do(t =>
        {
            t.labelTMP.fontStyle = TMPro.FontStyles.Underline;
            t.labelTMP.fontSize = 18f;
            t.labelTMP.horizontalAlignment = TMPro.HorizontalAlignmentOptions.Center;
            t.labelTMP.color = Color.white;
        });
    }

    public static void SliderSetups(List<REPOSlider> root)
    {
        if (root == null) return;

        Material material = PlayerAvatar.instance.playerHealth.bodyMaterial;
        Color baseColor = material.GetColor(Shader.PropertyToID("_AlbedoColor"));
        Color compColor = new(1f - baseColor.r, 1f - baseColor.g, 1f - baseColor.b, baseColor.a);

        foreach (REPOSlider slider in root)
        {
            foreach (Transform t in slider.GetComponentsInChildren<Transform>(true))
            {
                switch (t.name)
                {
                    case "SliderBG":
                        foreach (var raw in t.GetComponentsInChildren<RawImage>(true))
                            if (raw != null)
                            {
                                Color zeroAlpha = raw.color;
                                zeroAlpha.a = 0f;
                                raw.color = zeroAlpha;
                            }
                        break;

                    case "Bar":
                        foreach (var raw in t.GetComponentsInChildren<RawImage>(true))
                            if (raw != null)
                            {
                                float brightness = 0.299f * compColor.r + 0.587f * compColor.g + 0.114f * compColor.b;
                                float minBrightness = 0.5f;
                                if (brightness < minBrightness)
                                {
                                    float boost = Mathf.Clamp01((minBrightness - brightness) * 0.5f);
                                    compColor = Color.Lerp(compColor, Color.white, boost);
                                }
                                raw.color = compColor;
                            }
                        break;

                    case "Bar Text":
                        foreach (var text in t.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true))
                            if (text != null)
                            {
                                float brightness = 0.299f * compColor.r + 0.587f * compColor.g + 0.114f * compColor.b;
                                float minBrightness = 0.5f;
                                if (brightness < minBrightness)
                                {
                                    float boost = Mathf.Clamp01((minBrightness - brightness) * 0.5f);
                                    compColor = Color.Lerp(compColor, Color.white, boost);
                                }
                                text.color = compColor;
                            }
                        break;

                    case "Bar Text (1)":
                        foreach (var raw in t.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true))
                            if (raw != null)
                                raw.color = Color.black;
                        break;
                }
            }
        }
    }

    public static string CleanName(string fileName)
    {
        string[] toRemove = ["pupil", "pupils", "iris", "irises", "left", "right"];
        string cleaned = fileName.Replace('_', ' ');

        foreach (var word in toRemove)
            cleaned = cleaned.Replace(word, "", StringComparison.OrdinalIgnoreCase);

        return string.Join(' ', cleaned.Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }

    public static string ApplyGradient(string input, bool inverse = false, float minBrightness = 0.15f)
    {
        Material material = PlayerAvatar.instance.playerHealth.bodyMaterial;
        Color baseColor = material.GetColor(Shader.PropertyToID("_AlbedoColor"));
        Color startColor = baseColor;
        Color endColor;

        float luminance = 0.299f * baseColor.r + 0.587f * baseColor.g + 0.114f * baseColor.b;
        float adjustmentAmount = 0.6f;

        endColor = luminance < 0.5f
            ? Color.Lerp(baseColor, Color.white, adjustmentAmount)
            : Color.Lerp(baseColor, Color.black, adjustmentAmount);

        if (inverse)
            (endColor, startColor) = (startColor, endColor);

        string result = "";
        int len = input.Length;

        for (int i = 0; i < len; i++)
        {
            float t = (float)i / Mathf.Max(1, len - 1);
            Color lerped = Color.Lerp(startColor, endColor, t);

            float brightness = 0.299f * lerped.r + 0.587f * lerped.g + 0.114f * lerped.b;
            if (brightness < minBrightness)
            {
                float boost = Mathf.Clamp01((minBrightness - brightness) * 0.5f);
                lerped = Color.Lerp(lerped, Color.white, boost);
            }

            string hex = ColorUtility.ToHtmlStringRGB(lerped);
            result += $"<color=#{hex}>{input[i]}</color>";
        }

        return result;
    }
}
