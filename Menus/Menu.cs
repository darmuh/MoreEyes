﻿using MenuLib;
using MenuLib.MonoBehaviors;
using MoreEyes.Core;
using MoreEyes.EyeManagement;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace MoreEyes.Menus;

internal sealed class Menu
{
    internal static REPOPopupPage MoreEyesMenu = new();
    internal static REPOAvatarPreview AvatarPreview;

    //internal static REPOButton clickedButton;
    internal static REPOButton pupilLeft;
    internal static REPOButton pupilRight;
    internal static REPOButton irisLeft;
    internal static REPOButton irisRight;

    internal static REPOLabel pupilLeftHeader;
    internal static REPOLabel pupilRightHeader;
    internal static REPOLabel irisLeftHeader;
    internal static REPOLabel irisRightHeader;

    internal static REPOSlider redSlider;
    internal static REPOSlider greenSlider;
    internal static REPOSlider blueSlider;

    internal static object ColorSelection { get; private set; } = null!;

    internal static EyePart CurrentEyePart { get; private set; }
    internal static EyeSide CurrentEyeSide { get; private set; }

    private static int currentRed;
    private static int currentGreen;
    private static int currentBlue;

    internal static string eyePart;
    internal static string eyeSide;
    internal static string eyeStyle;

    internal static bool SlidersOn { get; private set; } = false;

    internal enum EyePart
    {
        Pupil,
        Iris
    }
    internal enum EyeSide
    {
        Left,
        Right
    }
    private static string GetEyePartName(EyePart part)
    {
        return part switch
        {
            EyePart.Pupil => "Pupil",
            EyePart.Iris => "Iris",
            _ => ""
        };
    }
    private static string GetEyeSideName(EyeSide side)
    {
        return side switch
        {
            EyeSide.Left => "Left",
            EyeSide.Right => "Right",
            _ => ""
        };
    }
    private static string GetEyeStyle()
    {
        return (CurrentEyePart, CurrentEyeSide) switch
        {
            (EyePart.Pupil, EyeSide.Left) => pupilLeft.labelTMP.text,
            (EyePart.Pupil, EyeSide.Right) => pupilRight.labelTMP.text,
            (EyePart.Iris, EyeSide.Left) => irisLeft.labelTMP.text,
            (EyePart.Iris, EyeSide.Right) => irisRight.labelTMP.text,
            _ => ""
        };
    }

    internal static void Initialize()
    {
        int modCount = 0;
        if (ModCompats.IsSpawnManagerPresent) modCount++;
        if (ModCompats.IsTwitchChatAPIPresent) modCount++;
        if (ModCompats.IsTwitchTrollingPresent) modCount++;

        float yOffsetStart = 22f;
        float yOffsetPlus = 35f;

        Vector2 buttonPos = new(595f, yOffsetStart + (yOffsetPlus * modCount));

        MenuAPI.AddElementToMainMenu(p => MenuAPI.CreateREPOButton("More Eyes", CreatePopupMenu, p, buttonPos));
        MenuAPI.AddElementToLobbyMenu(p => MenuAPI.CreateREPOButton("More Eyes", CreatePopupMenu, p, new Vector2(600f, 22f)));
        MenuAPI.AddElementToEscapeMenu(p => MenuAPI.CreateREPOButton("More Eyes", CreatePopupMenu, p, new Vector2(600f, 22f)));
    }

    private static void RandomizeLocalEyeSelection()
    {
        Plugin.Spam("Randomize Eye Selections!");
        PatchedEyes.Local.RandomizeEyes();
        UpdateButtons();
    }
    private static void ResetLocalEyeSelection()
    {
        Plugin.Spam("Reset Eye Selections!");
        PatchedEyes.Local.ResetEyes();
        UpdateButtons();
    }

    internal static void UpdateButtons()
    {
        pupilLeft.labelTMP.text = MenuUtils.ApplyGradient(MenuUtils.CleanName(PatchedEyes.Local.currentSelections.pupilLeft.Name), true);
        pupilRight.labelTMP.text = MenuUtils.ApplyGradient(MenuUtils.CleanName(PatchedEyes.Local.currentSelections.pupilRight.Name), true);
        irisLeft.labelTMP.text = MenuUtils.ApplyGradient(MenuUtils.CleanName(PatchedEyes.Local.currentSelections.irisLeft.Name), true);
        irisRight.labelTMP.text = MenuUtils.ApplyGradient(MenuUtils.CleanName(PatchedEyes.Local.currentSelections.irisRight.Name), true);

        UpdateHeaders();
    }

    private static void SetSliders(int red, int green, int blue)
    {
        redSlider.SetValue(red, false);
        greenSlider.SetValue(green, false);
        blueSlider.SetValue(blue, false);

        currentRed = red;
        currentGreen = green;
        currentBlue = blue;
    }

    private static void UpdateSliders(object selection)
    {
        if (selection == null) return;
        Color color;

        if (selection is CustomPupilType pupil)
        {
            ColorSelection = selection;
            color = PatchedEyes.Local.currentSelections.GetColorOf(pupil);
        }        
        else if (selection is CustomIrisType iris)
        {
            ColorSelection = selection;
            color = PatchedEyes.Local.currentSelections.GetColorOf(iris);
        }
        else
        {
            Plugin.WARNING("Selection is invalid type at UpdateSliders!");
            return;
        }

        int red = Mathf.RoundToInt(color.r * 255f);
        int green = Mathf.RoundToInt(color.g * 255f);
        int blue = Mathf.RoundToInt(color.b * 255f);

        RedSlider(red);
        GreenSlider(green);
        BlueSlider(blue);

        SetSliders(currentRed, currentGreen, currentBlue);

    }
    private static void UpdateMaterialColor()
    {
        if (ColorSelection == null) return;

        Color newColor = new(currentRed / 255f, currentGreen / 255f, currentBlue / 255f);

        if (ColorSelection is CustomPupilType pupil)
            PatchedEyes.Local.currentSelections.UpdateColorOf(pupil, newColor);
        else if (ColorSelection is CustomIrisType iris)
            PatchedEyes.Local.currentSelections.UpdateColorOf(iris, newColor);
        else
            Plugin.WARNING("Unable to set color of current selection! Invalid type!");
    }

    private static void UpdateHeaders()
    {
        eyePart = GetEyePartName(CurrentEyePart);
        eyeSide = GetEyeSideName(CurrentEyeSide);
        eyeStyle = GetEyeStyle();
        redSlider.labelTMP.text = $"{eyeStyle}";
        greenSlider.labelTMP.text = MenuUtils.ApplyGradient($"{eyePart}");
        blueSlider.labelTMP.text = MenuUtils.ApplyGradient($"{eyeSide}");
    }

    private static void BackButton()
    {
        SlidersOn = true;
        MoreEyesMenu.ClosePage(true);

        //Update selections if there are unsaved changes when menu is closed 
        if (FileManager.UpdateWrite)
            FileManager.WriteTextFile();
    }

    private static void CreatePopupMenu()
    {
        if (MoreEyesMenu.menuPage != null)
        {
            return;
        }

        MoreEyesMenu = MenuAPI.CreateREPOPopupPage(MenuUtils.ApplyGradient("More Eyes"), false, true, 0f, new Vector2(-150f, 5f));
        
        AvatarPreview = MenuAPI.CreateREPOAvatarPreview(MoreEyesMenu.transform, new Vector2(471.25f, 156.5f), false);

        AvatarPreview.previewSize = new Vector2(266.6667f, 500f); // original numbers (184, 345)
        AvatarPreview.rectTransform.sizeDelta = new Vector2(266.6667f, 210f); // original (184, 345) same way as previewSize
        AvatarPreview.rigTransform.parent.localScale = new Vector3(2f, 2f, 2f); // original (1, 1, 1)
        AvatarPreview.rigTransform.parent.localPosition = new Vector3(0f, -3.5f, 0f);

        MenuUtils.HandleScrollZoom(AvatarPreview);

        MoreEyesMenu.AddElement(e => MenuAPI.CreateREPOButton("Back", BackButton, MoreEyesMenu.transform, new Vector2(190, 30)));
        MoreEyesMenu.AddElement(e => MenuAPI.CreateREPOButton("Randomize", RandomizeLocalEyeSelection, MoreEyesMenu.transform, new Vector2(270, 30)));
        MoreEyesMenu.AddElement(e => MenuAPI.CreateREPOButton("Reset", ResetLocalEyeSelection, MoreEyesMenu.transform, new Vector2(400, 30)));

        pupilLeft = MenuAPI.CreateREPOButton(MenuUtils.ApplyGradient(MenuUtils.CleanName(PatchedEyes.Local.currentSelections.pupilLeft.Name), true), PupilLeftSliders, MoreEyesMenu.transform, new Vector2(215f, 265f));
        pupilRight = MenuAPI.CreateREPOButton(MenuUtils.ApplyGradient(MenuUtils.CleanName(PatchedEyes.Local.currentSelections.pupilRight.Name), true), PupilRightSliders, MoreEyesMenu.transform, new Vector2(360f, 265f));
        irisLeft = MenuAPI.CreateREPOButton(MenuUtils.ApplyGradient(MenuUtils.CleanName(PatchedEyes.Local.currentSelections.irisLeft.Name), true), IrisLeftSliders, MoreEyesMenu.transform, new Vector2(215, 215f));
        irisRight = MenuAPI.CreateREPOButton(MenuUtils.ApplyGradient(MenuUtils.CleanName(PatchedEyes.Local.currentSelections.irisRight.Name), true), IrisRightSliders, MoreEyesMenu.transform, new Vector2(360, 215f));

        MenuUtils.SetTextStyling([pupilLeft, pupilRight, irisLeft, irisRight]);

        pupilLeftHeader = MenuAPI.CreateREPOLabel("Pupil Left", MoreEyesMenu.transform, new Vector2(151.5f, 285f));
        pupilRightHeader = MenuAPI.CreateREPOLabel("Pupil Right", MoreEyesMenu.transform, new Vector2(297.5f, 285f));
        irisLeftHeader = MenuAPI.CreateREPOLabel("Iris Left", MoreEyesMenu.transform, new Vector2(151.5f, 235f));
        irisRightHeader = MenuAPI.CreateREPOLabel("Iris Right", MoreEyesMenu.transform, new Vector2(297.5f, 235f));

        MenuUtils.SetHeaderTextStyling([pupilLeftHeader, pupilRightHeader, irisLeftHeader, irisRightHeader]);

        MoreEyesMenu.AddElement(e => MenuAPI.CreateREPOButton("<", LeftPupilPrev, pupilLeft.transform, new Vector2(-25f, -10f)));
        MoreEyesMenu.AddElement(e => MenuAPI.CreateREPOButton("<", RightPupilPrev, pupilRight.transform, new Vector2(-25f, -10f)));
        MoreEyesMenu.AddElement(e => MenuAPI.CreateREPOButton("<", LeftIrisPrev, irisLeft.transform, new Vector2(-25f, -10f)));
        MoreEyesMenu.AddElement(e => MenuAPI.CreateREPOButton("<", RightIrisPrev, irisRight.transform, new Vector2(-25f, -10f)));
        MoreEyesMenu.AddElement(e => MenuAPI.CreateREPOButton(">", LeftPupilNext, pupilLeft.transform, new Vector2(70f, -10f)));
        MoreEyesMenu.AddElement(e => MenuAPI.CreateREPOButton(">", RightPupilNext, pupilRight.transform, new Vector2(70f, -10f)));
        MoreEyesMenu.AddElement(e => MenuAPI.CreateREPOButton(">", LeftIrisNext, irisLeft.transform, new Vector2(70f, -10f)));
        MoreEyesMenu.AddElement(e => MenuAPI.CreateREPOButton(">", RightIrisNext, irisRight.transform, new Vector2(70f, -10f)));

        redSlider = MenuAPI.CreateREPOSlider(MenuUtils.ApplyGradient($"{eyeStyle}"), "<color=#FF0000>Red</color>", RedSlider, MoreEyesMenu.transform, new Vector2(205f, 180f), min: 0, max: 255, barBehavior: REPOSlider.BarBehavior.UpdateWithValue);
        greenSlider = MenuAPI.CreateREPOSlider(MenuUtils.ApplyGradient($"{eyePart}"), "<color=#00FF00>Green</color>", GreenSlider, MoreEyesMenu.transform, new Vector2(205f, 135f), min: 0, max: 255, barBehavior: REPOSlider.BarBehavior.UpdateWithValue);
        blueSlider = MenuAPI.CreateREPOSlider(MenuUtils.ApplyGradient($"{eyeSide}"), "<color=#0000FF>Blue</color>", BlueSlider, MoreEyesMenu.transform, new Vector2(205f, 90f), min: 0, max: 255, barBehavior: REPOSlider.BarBehavior.UpdateWithValue);

        MenuUtils.SliderSetups([redSlider, greenSlider, blueSlider]);

        redSlider.gameObject.SetActive(false);
        greenSlider.gameObject.SetActive(false);
        blueSlider.gameObject.SetActive(false);

        SlidersOn = false;

        MoreEyesMenu.OpenPage(false);

        MoreEyesMenu.onEscapePressed += ShouldCloseMenu;
    }

    private static bool ShouldCloseMenu()
    {
        BackButton();
        return true;
    }

    private static void CommonSliders(EyeSide eyeSide, EyePart eyePart)
    {
        if (!SlidersOn)
        {
            redSlider.gameObject.SetActive(true);
            greenSlider.gameObject.SetActive(true);
            blueSlider.gameObject.SetActive(true);
        }
        SlidersOn = true;

        CurrentEyePart = eyePart;
        CurrentEyeSide = eyeSide;
        eyeStyle = GetEyeStyle();
        UpdateHeaders();

        if(CurrentEyePart == EyePart.Pupil)
        {
            if(eyeSide == EyeSide.Left && PatchedEyes.Local.currentSelections.pupilLeft.Prefab != null)
                UpdateSliders(PatchedEyes.Local.currentSelections.pupilLeft);

            if (eyeSide == EyeSide.Right && PatchedEyes.Local.currentSelections.pupilRight.Prefab != null)
                UpdateSliders(PatchedEyes.Local.currentSelections.pupilRight);
        }
        else
        {
            if (eyeSide == EyeSide.Left && PatchedEyes.Local.currentSelections.irisLeft.Prefab != null)
                UpdateSliders(PatchedEyes.Local.currentSelections.irisLeft);

            if (eyeSide == EyeSide.Right && PatchedEyes.Local.currentSelections.irisRight.Prefab != null)
                UpdateSliders(PatchedEyes.Local.currentSelections.irisRight);
        }
    }

    private static void PupilLeftSliders()
    {
        CommonSliders(EyeSide.Left, EyePart.Pupil);
    }
    private static void PupilRightSliders()
    {
        CommonSliders(EyeSide.Right, EyePart.Pupil);
    }
    private static void IrisLeftSliders()
    {
        CommonSliders(EyeSide.Left, EyePart.Iris);
    }
    private static void IrisRightSliders()
    {
        CommonSliders(EyeSide.Right, EyePart.Iris);
    }

    private static void RedSlider(int value)
    {
        currentRed = value;
        UpdateMaterialColor();

    }
    private static void GreenSlider(int value)
    {
        currentGreen = value;
        UpdateMaterialColor();

    }
    private static void BlueSlider(int value)
    {
        currentBlue = value;
        UpdateMaterialColor();
    }

    private static void LeftIrisNext()
    {
        NewSelection(EyeSide.Left, EyePart.Iris, 1);
    }
    private static void RightIrisNext()
    {
        NewSelection(EyeSide.Right, EyePart.Iris, 1);
    }

    private static void LeftIrisPrev()
    {
        NewSelection(EyeSide.Left, EyePart.Iris, -1);
    }
    private static void RightIrisPrev()
    {
        NewSelection(EyeSide.Right, EyePart.Iris, -1);
    }

    private static void LeftPupilNext()
    {
        NewSelection(EyeSide.Left, EyePart.Pupil, 1);
    }
    private static void RightPupilNext()
    {
        NewSelection(EyeSide.Right, EyePart.Pupil, 1);
    }
    private static void LeftPupilPrev()
    {
        NewSelection(EyeSide.Left, EyePart.Pupil, -1);
    }
    private static void RightPupilPrev()
    {
        NewSelection(EyeSide.Right, EyePart.Pupil, -1);
    }

    //Common method between all pupil/iris selections
    private static void NewSelection(EyeSide side, EyePart part, int dir)
    {
        if (part == EyePart.Pupil)
        {
            List<CustomPupilType> options = [];

            if (side == EyeSide.Left)
                options = CustomEyeManager.AllPupilTypes.FindAll(i => i.AllowedPos != CustomEyeManager.Sides.Right);
            else
                options = CustomEyeManager.AllPupilTypes.FindAll(i => i.AllowedPos != CustomEyeManager.Sides.Left);

            options.DistinctBy(p => p.Prefab);
            Plugin.Spam($"CustomPupils Total: {CustomEyeManager.AllPupilTypes.Count}, Filtered: {options.Count}");
            int currentIndex;

            if(side == EyeSide.Left)
                currentIndex = options.IndexOf(PatchedEyes.Local.currentSelections.pupilLeft);
            else
                currentIndex = options.IndexOf(PatchedEyes.Local.currentSelections.pupilRight);

            int selected = CycleIndex(currentIndex + dir, 0, options.Count - 1);
            Plugin.Spam($"currentIndex = {currentIndex}, selected = {selected}");
            CustomPupilType newSelection = options[selected];

            PatchedEyes.Local.SelectPupil(newSelection, side == EyeSide.Left);

            UpdateButtons();
        }
        else
        {
            List<CustomIrisType> options = [];

            if (side == EyeSide.Left)
                options = CustomEyeManager.AllIrisTypes.FindAll(i => i.AllowedPos != CustomEyeManager.Sides.Right);
            else
                options = CustomEyeManager.AllIrisTypes.FindAll(i => i.AllowedPos != CustomEyeManager.Sides.Left);

            options.DistinctBy(p => p.Prefab);
            Plugin.Spam($"CustomPupils Total: {CustomEyeManager.AllIrisTypes.Count}, Filtered: {options.Count}");
            int currentIndex;

            if (side == EyeSide.Left)
                currentIndex = options.IndexOf(PatchedEyes.Local.currentSelections.irisLeft);
            else
                currentIndex = options.IndexOf(PatchedEyes.Local.currentSelections.irisRight);

            int selected = CycleIndex(currentIndex + dir, 0, options.Count - 1);
            Plugin.Spam($"currentIndex = {currentIndex}, selected = {selected}");
            CustomIrisType newSelection = options[selected];

            PatchedEyes.Local.SelectIris(newSelection, side == EyeSide.Left);
            UpdateButtons();
        }

        CommonSliders(side, part); //Set color sliders to new selection
    }

    public static int CycleIndex(int value, int min, int max)
    {
        if (value < min)
        {
            Plugin.Spam($"Returning max! {max}");
            return max; // added this because im lazy
        }

        if (value > max)
        {
            Plugin.Spam($"Returning min! {min}");
            return min;
        }

        Plugin.Spam($"Returning value! {value}");
        return value;
    }
}