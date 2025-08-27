using HarmonyLib;
using MenuLib.MonoBehaviors;
using MoreEyes.Collections;
using MoreEyes.Components;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static MoreEyes.Utility.Enums;
using Image = UnityEngine.UI.Image;

namespace MoreEyes.Utility;
internal static class MenuUtils
{
    private static Coroutine zoomCoroutine;
    internal static float zoomLevel = 0f;

    public static void HandleScrollZoom(REPOAvatarPreview preview, Vector2 anchorIn, Vector2 anchorOut, Vector3 scaleIn, Vector3 scaleOut, Vector3 posIn, Vector3 posOut)
    {
        if (preview == null || preview.rigTransform == null)
            return;

        if (zoomCoroutine == null)
            preview.StartCoroutine(AvatarZoomCoroutine(preview, anchorIn, anchorOut, scaleIn, scaleOut, posIn, posOut));
    }

    private static IEnumerator AvatarZoomCoroutine(REPOAvatarPreview preview, Vector2 anchorIn, Vector2 anchorOut, Vector3 scaleIn, Vector3 scaleOut, Vector3 posIn, Vector3 posOut)
    {
        while (preview != null && preview.rigTransform != null)
        {
            float scrollDelta = InputManager.instance.KeyPullAndPush();

            if (Mathf.Abs(scrollDelta) > 0.001f)
            {
                float targetZoom = Mathf.Clamp01(zoomLevel + Mathf.Sign(scrollDelta));

                if (!Mathf.Approximately(targetZoom, zoomLevel))
                {
                    if (zoomCoroutine != null)
                    {
                        preview.StopCoroutine(zoomCoroutine);
                        zoomCoroutine = null;
                    }

                    zoomCoroutine = preview.StartCoroutine(AnimateZoom(preview, targetZoom, anchorIn, anchorOut, scaleIn, scaleOut, posIn, posOut));
                    zoomLevel = targetZoom;
                }
            }
            yield return null;
        }
    }

    private static IEnumerator AnimateZoom(REPOAvatarPreview preview, float targetZoom, Vector2 anchorIn, Vector2 anchorOut, Vector3 scaleIn, Vector3 scaleOut, Vector3 posIn, Vector3 posOut)
    {
        Vector2 sizeOut = new(182.4f, 342f);
        Vector2 sizeIn = new(266.6667f, 500f);

        Vector2 deltaOut = new(182.4f, 342f);
        Vector2 deltaIn = new(266.6667f, 210f);

        float startZoom = zoomLevel;
        float duration = 0.45f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);

            float z = Mathf.Lerp(startZoom, targetZoom, t);

            preview.previewSize = Vector2.Lerp(sizeOut, sizeIn, z);
            preview.rectTransform.sizeDelta = Vector2.Lerp(deltaOut, deltaIn, z);
            preview.rigTransform.parent.localScale = Vector3.Lerp(scaleOut, scaleIn, z);
            preview.rigTransform.parent.localPosition = Vector3.Lerp(posOut, posIn, z);
            preview.rectTransform.anchoredPosition = Vector2.Lerp(anchorOut, anchorIn, z);
            yield return null;
        }
        preview.previewSize = Vector2.Lerp(sizeOut, sizeIn, targetZoom);
        preview.rectTransform.sizeDelta = Vector2.Lerp(deltaOut, deltaIn, targetZoom);
        preview.rigTransform.parent.localScale = Vector3.Lerp(scaleOut, scaleIn, targetZoom);
        preview.rigTransform.parent.localPosition = Vector3.Lerp(posOut, posIn, targetZoom);
        preview.rectTransform.anchoredPosition = Vector2.Lerp(anchorOut, anchorIn, targetZoom);
        zoomCoroutine = null;
    }
    public static void SetTipTextStyling(REPOLabel label)
    {
        label.labelTMP.fontSize = 20f;
        label.labelTMP.horizontalAlignment = HorizontalAlignmentOptions.Center;
        label.labelTMP.alpha = 0.15f;
    }
    public static void SetTextStyling(List<REPOButton> buttons)
    {
        buttons.Do(t =>
        {
            t.overrideButtonSize = new Vector2(75f, 20f);
            t.labelTMP.fontSize = 18f;
            GameObject maskParent = new($"{t.name} Mask Object");
            Image image = maskParent.AddComponent<Image>();
            image.color = new(0, 0, 0, 0.05f);
            maskParent.AddComponent<Mask>();
            maskParent.transform.SetParent(t.transform);
            image.rectTransform.anchoredPosition = new(0f, -2f);
            image.rectTransform.sizeDelta = new Vector2(80f, 20f);
            t.labelTMP.gameObject.transform.SetParent(maskParent.transform);
            t.menuButton.buttonTextSelectedOriginalPos = new(-42, -18, 0);

            var scroller = t.labelTMP.gameObject.AddComponent<HorizontalTextScroller>();
            scroller.startPos = t.menuButton.buttonTextSelectedOriginalPos;
            scroller.SetButtonRef(t.menuButton);
        });
    }

    public static void SetHeaderTextStyling(List<REPOLabel> labels)
    {
        labels.Do(t =>
        {
            t.labelTMP.fontStyle = FontStyles.Underline;
            t.labelTMP.fontSize = 18f;
            t.labelTMP.horizontalAlignment = HorizontalAlignmentOptions.Center;
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
                        foreach (RawImage raw in t.GetComponentsInChildren<RawImage>(true))
                            if (raw != null)
                            {
                                Color zeroAlpha = raw.color;
                                zeroAlpha.a = 0f;
                                raw.color = zeroAlpha;
                            }
                        break;

                    case "Bar":
                        foreach (RawImage raw in t.GetComponentsInChildren<RawImage>(true))
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
                        foreach (TextMeshProUGUI text in t.GetComponentsInChildren<TextMeshProUGUI>(true))
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
                        foreach (TextMeshProUGUI raw in t.GetComponentsInChildren<TextMeshProUGUI>(true))
                            if (raw != null)
                                raw.color = Color.black;
                        break;
                }
            }
        }
    }

    public static string CleanName(string fileName)
    {
        string[] toRemove = ["pupil", "pupils", "iris", "irises", "left", "right", "both"];
        string cleaned = fileName.Replace('_', ' ');

        foreach (var word in toRemove)
            cleaned = cleaned.Replace(word, "", StringComparison.OrdinalIgnoreCase);

        return string.Join(' ', cleaned.Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }

    public static void OrderListBy(ref List<CustomPupilType> List, MenuOrderBy config)
    {
        if (config == MenuOrderBy.None)
            return;

        if (config == MenuOrderBy.NameOnly)
        {
            List.OrderBy(L => L.Name);
            return;
        }

        if (config == MenuOrderBy.ModNameOnly)
        {
            List.OrderBy(L => L.ModName);
            return;
        }

        if (config == MenuOrderBy.ModNameAndName)
        {
            List.OrderBy(L => L.ModName).ThenBy(L => L.Name);
            return;
        }

    }

    public static void OrderListBy(ref List<CustomIrisType> List, MenuOrderBy config)
    {
        if (config == MenuOrderBy.None)
            return;

        if (config == MenuOrderBy.NameOnly)
        {
            List.OrderBy(L => L.Name);
            return;
        }

        if (config == MenuOrderBy.ModNameOnly)
        {
            List.OrderBy(L => L.ModName);
            return;
        }

        if (config == MenuOrderBy.ModNameAndName)
        {
            List.OrderBy(L => L.ModName).ThenBy(L => L.Name);
            return;
        }
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