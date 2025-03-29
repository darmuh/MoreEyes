using UnityEngine;
using MenuLib;
using MenuLib.MonoBehaviors;
using System.Collections.Generic;
using HarmonyLib;
using System.Linq;

namespace MoreEyes.Menus;

internal sealed class Menu
{
    internal static REPOButton clickedButton;
    internal static REPOPopupPage MoreEyesMenu = new();
    internal static REPOAvatarPreview AvatarPreview;
    internal static REPOLabel pupilLeft;
    internal static REPOLabel pupilRight;
    internal static REPOLabel irisLeft;
    internal static REPOLabel irisRight;

    internal static Transform eyeLeft;
    internal static Transform eyeRight;


    internal static void Initialize()
    {
        //set position to bottom right at 600f, 0f
        MenuAPI.AddElementToMainMenu(p => MenuAPI.CreateREPOButton("MoreEyes", CreatePopupMenu, p, new Vector2(600f, 0f)));
        MenuAPI.AddElementToLobbyMenu(p => MenuAPI.CreateREPOButton("MoreEyes", CreatePopupMenu, p, new Vector2(600f, 0f)));
        //this will probably need to be different due to already having a playerAvatarVisual
        MenuAPI.AddElementToEscapeMenu(p => MenuAPI.CreateREPOButton("MoreEyes", CreatePopupMenu, p, new Vector2(600f, 0f)));
    }

    private static void RandomizeEyeSelection()
    {
        Plugin.Spam("Randomize Eye Selections!");
        PatchedEyes patchedEyes = PatchedEyes.GetPatchedEyes(PlayerAvatar.instance);
        GetPlayerMenuEyes(AvatarPreview.playerAvatarVisuals);
        patchedEyes.RandomizeEyes(PlayerAvatar.instance.playerName, eyeLeft, eyeRight);
        UpdateLabels();
    }

    private static void UpdateLabels()
    {
        pupilLeft.labelTMP.text = PlayerEyeSelection.localSelections.pupilLeft.Name;
        pupilRight.labelTMP.text = PlayerEyeSelection.localSelections.pupilRight.Name;
        irisLeft.labelTMP.text = PlayerEyeSelection.localSelections.irisLeft.Name;
        irisRight.labelTMP.text = PlayerEyeSelection.localSelections.irisRight.Name;
    }

    private static void CreatePopupMenu()
    {
        if (MoreEyesMenu.menuPage != null)
            return;

        //must be created when opened
        MoreEyesMenu = MenuAPI.CreateREPOPopupPage("MoreEyes", REPOPopupPage.PresetSide.Right, false, true);
        AvatarPreview = MenuAPI.CreateREPOAvatarPreview(MoreEyesMenu.transform, new Vector2(180, 18), true, new Color(0f, 0f, 0f, 0.69f));
        MoreEyesMenu.AddElement(e => MenuAPI.CreateREPOButton("Back", () => MoreEyesMenu.ClosePage(true), MoreEyesMenu.transform, new Vector2(370, 20)));
        MoreEyesMenu.AddElement(e => MenuAPI.CreateREPOButton("Randomize", RandomizeEyeSelection, MoreEyesMenu.transform, new Vector2(470, 20)));
        CustomEyeManager.CheckForVanillaPupils();
        pupilLeft = MenuAPI.CreateREPOLabel(PlayerEyeSelection.localSelections.pupilLeft.Name, MoreEyesMenu.transform, new Vector2(412, 260));
        pupilRight = MenuAPI.CreateREPOLabel(PlayerEyeSelection.localSelections.pupilRight.Name, MoreEyesMenu.transform, new Vector2(412, 220));
        irisLeft = MenuAPI.CreateREPOLabel(PlayerEyeSelection.localSelections.irisLeft.Name, MoreEyesMenu.transform, new Vector2(412, 180));
        irisRight = MenuAPI.CreateREPOLabel(PlayerEyeSelection.localSelections.irisRight.Name, MoreEyesMenu.transform, new Vector2(412, 140));

        SetTextStyling([pupilLeft, pupilRight, irisLeft, irisRight]);
        //pupil left prev
        MoreEyesMenu.AddElement(e => MenuAPI.CreateREPOButton("<", LeftPupilPrev, pupilLeft.transform, new Vector2(-15f, -3f)));
        //pupil right prev
        MoreEyesMenu.AddElement(e => MenuAPI.CreateREPOButton("<", RightPupilPrev, pupilRight.transform, new Vector2(-15f, -3f)));
        //iris left prev
        MoreEyesMenu.AddElement(e => MenuAPI.CreateREPOButton("<", LeftIrisPrev, irisLeft.transform, new Vector2(-15f, -3f)));
        //iris right prev
        MoreEyesMenu.AddElement(e => MenuAPI.CreateREPOButton("<", RightIrisPrev, irisRight.transform, new Vector2(-15f, -3f)));
        //pupil left next
        MoreEyesMenu.AddElement(e => MenuAPI.CreateREPOButton(">", LeftPupilNext, pupilLeft.transform, GetRightOfElement(pupilLeft.rectTransform) + new Vector2(0f, -3f)));
        //pupil right next
        MoreEyesMenu.AddElement(e => MenuAPI.CreateREPOButton(">", RightPupilNext, pupilRight.transform, GetRightOfElement(pupilRight.rectTransform) + new Vector2(0f, -3f)));
        //iris left next
        MoreEyesMenu.AddElement(e => MenuAPI.CreateREPOButton(">", LeftIrisNext, irisLeft.transform, GetRightOfElement(irisLeft.rectTransform) + new Vector2(0f, -3f)));
        //iris right next, placed to the right of the text
        MoreEyesMenu.AddElement(e => MenuAPI.CreateREPOButton(">", RightIrisNext, irisRight.transform, GetRightOfElement(irisRight.rectTransform) + new Vector2(0f, -3f)));

        MoreEyesMenu.OpenPage(false);
    }

    private static void SetTextStyling(List<REPOLabel> labels)
    {
        labels.Do(t =>
        {
            t.labelTMP.fontStyle = TMPro.FontStyles.SmallCaps;
            t.labelTMP.enableAutoSizing = true;
            t.labelTMP.fontSizeMin = 22f;
            t.labelTMP.horizontalAlignment = TMPro.HorizontalAlignmentOptions.Center;
        });
    }

    private static Vector2 GetRightOfElement(RectTransform rect)
    {
        Vector3[] allCorners = new Vector3[4];
        rect.GetLocalCorners(allCorners);
        Plugin.Spam($"Count: {allCorners.Length}");
        return (Vector2)allCorners[3];
    }

    private static void LeftIrisNext()
    {
        //only get iris that are allowed for left eye
        List<CustomIrisType> noRights = CustomEyeManager.AllIrisTypes.FindAll(i => i.AllowedPos != CustomEyeManager.Sides.Right);

        noRights.Distinct();

        int currentIndex = noRights.IndexOf(PlayerEyeSelection.localSelections.irisLeft);

        Plugin.Spam($"CustomPupils Total: {CustomEyeManager.AllIrisTypes.Count}, Filtered: {noRights.Count}");

        //increases value of index and cycles back to 0 if above max value
        int selected = CycleIndex(currentIndex + 1, 0, noRights.Count - 1);

        //set new selection var
        CustomIrisType newSelection = noRights[selected];

        //get local eyes
        CustomEyeManager.AllPatchedEyes.RemoveAll(p => p.playerRef == null);
        PatchedEyes patchedEyes = PatchedEyes.GetPatchedEyes(PlayerAvatar.instance);

        patchedEyes.SelectIris(ref PlayerEyeSelection.localSelections.irisLeft, newSelection, ref patchedEyes.LeftIrisObject, patchedEyes.LeftPupilObject);

        UpdateLabels();

        CustomEyeManager.EmptyTrash();
    }
    private static void RightIrisNext()
    {
        //only get iris that are allowed for right eye
        List<CustomIrisType> noLefts = CustomEyeManager.AllIrisTypes.FindAll(i => i.AllowedPos != CustomEyeManager.Sides.Left);

        noLefts.Distinct();

        int currentIndex = noLefts.IndexOf(PlayerEyeSelection.localSelections.irisRight);

        Plugin.Spam($"CustomPupils Total: {CustomEyeManager.AllIrisTypes.Count}, Filtered: {noLefts.Count}");

        //increases value of index and cycles back to 0 if above max value
        int selected = CycleIndex(currentIndex + 1, 0, noLefts.Count - 1);

        //set new selection var
        CustomIrisType newSelection = noLefts[selected];

        //get local eyes
        CustomEyeManager.AllPatchedEyes.RemoveAll(p => p.playerRef == null);
        PatchedEyes patchedEyes = PatchedEyes.GetPatchedEyes(PlayerAvatar.instance);

        patchedEyes.SelectIris(ref PlayerEyeSelection.localSelections.irisRight, newSelection, ref patchedEyes.RightIrisObject, patchedEyes.RightPupilObject);

        UpdateLabels();

        CustomEyeManager.EmptyTrash();
    }

    private static void LeftIrisPrev()
    {
        //only get iris that are allowed for left eye
        List<CustomIrisType> noRights = CustomEyeManager.AllIrisTypes.FindAll(i => i.AllowedPos != CustomEyeManager.Sides.Right);

        noRights.Distinct();

        int currentIndex = noRights.IndexOf(PlayerEyeSelection.localSelections.irisLeft);

        Plugin.Spam($"CustomPupils Total: {CustomEyeManager.AllIrisTypes.Count}, Filtered: {noRights.Count}");

        //decreases value of index and cycles back to 0 if above max value
        int selected = CycleIndex(currentIndex - 1, 0, noRights.Count - 1);

        //set new selection var
        CustomIrisType newSelection = noRights[selected];

        //get local eyes
        CustomEyeManager.AllPatchedEyes.RemoveAll(p => p.playerRef == null);
        PatchedEyes patchedEyes = PatchedEyes.GetPatchedEyes(PlayerAvatar.instance);

        patchedEyes.SelectIris(ref PlayerEyeSelection.localSelections.irisLeft, newSelection, ref patchedEyes.LeftIrisObject, patchedEyes.LeftPupilObject);

        UpdateLabels();

        CustomEyeManager.EmptyTrash();
    }
    private static void RightIrisPrev()
    {
        //only get iris that are allowed for right eye
        List<CustomIrisType> noLefts = CustomEyeManager.AllIrisTypes.FindAll(i => i.AllowedPos != CustomEyeManager.Sides.Left);

        noLefts.Distinct();

        int currentIndex = noLefts.IndexOf(PlayerEyeSelection.localSelections.irisRight);

        Plugin.Spam($"CustomPupils Total: {CustomEyeManager.AllIrisTypes.Count}, Filtered: {noLefts.Count}");

        //decreases value of index and cycles back to 0 if above max value
        int selected = CycleIndex(currentIndex - 1, 0, noLefts.Count - 1);

        //set new selection var
        CustomIrisType newSelection = noLefts[selected];

        //get local eyes
        CustomEyeManager.AllPatchedEyes.RemoveAll(p => p.playerRef == null);
        PatchedEyes patchedEyes = PatchedEyes.GetPatchedEyes(PlayerAvatar.instance);

        patchedEyes.SelectIris(ref PlayerEyeSelection.localSelections.irisRight, newSelection, ref patchedEyes.RightIrisObject, patchedEyes.RightPupilObject);
        patchedEyes.SelectIris(ref PlayerEyeSelection.localSelections.irisRight, PlayerEyeSelection.localSelections.irisRight, ref patchedEyes.RightIrisObject, patchedEyes.RightPupilObject);

        UpdateLabels();

        CustomEyeManager.EmptyTrash();
    }

    private static void LeftPupilNext()
    {
        GetPlayerMenuEyes(AvatarPreview.playerAvatarVisuals);

        //only get iris that are allowed for left eye
        List<CustomPupilType> noRights = CustomEyeManager.AllPupilTypes.FindAll(i => i.AllowedPos != CustomEyeManager.Sides.Right);

        noRights.Distinct();

        int currentIndex = noRights.IndexOf(PlayerEyeSelection.localSelections.pupilLeft);

        Plugin.Spam($"CustomPupils Total: {CustomEyeManager.AllPupilTypes.Count}, Filtered: {noRights.Count}");

        //increases value of index and cycles back to 0 if above max value
        int selected = CycleIndex(currentIndex + 1, 0, noRights.Count - 1);

        //set new selection var
        CustomPupilType newSelection = noRights[selected];

        //get local eyes
        CustomEyeManager.AllPatchedEyes.RemoveAll(p => p.playerRef == null);
        PatchedEyes patchedEyes = PatchedEyes.GetPatchedEyes(PlayerAvatar.instance);

        patchedEyes.SelectPupil(ref PlayerEyeSelection.localSelections.pupilLeft, newSelection, ref patchedEyes.LeftPupilObject, eyeLeft);
        patchedEyes.SelectIris(ref PlayerEyeSelection.localSelections.irisLeft, PlayerEyeSelection.localSelections.irisLeft, ref patchedEyes.LeftIrisObject, patchedEyes.LeftPupilObject);

        UpdateLabels();

        CustomEyeManager.EmptyTrash();
    }
    private static void RightPupilNext()
    {
        GetPlayerMenuEyes(AvatarPreview.playerAvatarVisuals);

        //only get iris that are allowed for left eye
        List<CustomPupilType> noLefts = CustomEyeManager.AllPupilTypes.FindAll(i => i.AllowedPos != CustomEyeManager.Sides.Left);

        noLefts.Distinct();
        noLefts.Do(p => Plugin.Spam(p.Name));

        int currentIndex = noLefts.IndexOf(PlayerEyeSelection.localSelections.pupilRight);

        Plugin.Spam($"CustomPupils Total: {CustomEyeManager.AllPupilTypes.Count}, Filtered: {noLefts.Count}");

        //increases value of index and cycles back to 0 if above max value
        int selected = CycleIndex(currentIndex + 1, 0, noLefts.Count - 1);

        //set new selection var
        CustomPupilType newSelection = noLefts[selected];

        //get local eyes
        CustomEyeManager.AllPatchedEyes.RemoveAll(p => p.playerRef == null);
        PatchedEyes patchedEyes = PatchedEyes.GetPatchedEyes(PlayerAvatar.instance);

        patchedEyes.SelectPupil(ref PlayerEyeSelection.localSelections.pupilRight, newSelection, ref patchedEyes.RightPupilObject, eyeRight);
        patchedEyes.SelectIris(ref PlayerEyeSelection.localSelections.irisRight, PlayerEyeSelection.localSelections.irisRight, ref patchedEyes.RightIrisObject, patchedEyes.RightPupilObject);

        UpdateLabels();

        CustomEyeManager.EmptyTrash();
    }

    private static void LeftPupilPrev()
    {
        GetPlayerMenuEyes(AvatarPreview.playerAvatarVisuals);

        //only get iris that are allowed for left eye
        List<CustomPupilType> noRights = CustomEyeManager.AllPupilTypes.FindAll(i => i.AllowedPos != CustomEyeManager.Sides.Right);

        //remove duplicates
        noRights.Distinct();

        Plugin.Spam($"CustomPupils Total: {CustomEyeManager.AllPupilTypes.Count}, Filtered: {noRights.Count}");

        int currentIndex = noRights.IndexOf(PlayerEyeSelection.localSelections.pupilLeft);

        //decreases value of index and cycles back to 0 if above max value
        int selected = CycleIndex(currentIndex - 1, 0, noRights.Count - 1);

        Plugin.Spam($"currentIndex = {currentIndex}, selected = {selected}");

        //set new selection var
        CustomPupilType newSelection = noRights[selected];

        //get local eyes
        CustomEyeManager.AllPatchedEyes.RemoveAll(p => p.playerRef == null);
        PatchedEyes patchedEyes = PatchedEyes.GetPatchedEyes(PlayerAvatar.instance);

        patchedEyes.SelectPupil(ref PlayerEyeSelection.localSelections.pupilLeft, newSelection, ref patchedEyes.LeftPupilObject, eyeLeft);
        patchedEyes.SelectIris(ref PlayerEyeSelection.localSelections.irisLeft, PlayerEyeSelection.localSelections.irisLeft, ref patchedEyes.LeftIrisObject, patchedEyes.LeftPupilObject);

        UpdateLabels();

        CustomEyeManager.EmptyTrash();
    }
    private static void RightPupilPrev()
    {
        if (eyeRight == null)
            GetPlayerMenuEyes(AvatarPreview.playerAvatarVisuals);

        //only get iris that are allowed for left eye
        List<CustomPupilType> noLefts = CustomEyeManager.AllPupilTypes.FindAll(i => i.AllowedPos != CustomEyeManager.Sides.Left);

        noLefts.Distinct();

        int currentIndex = noLefts.IndexOf(PlayerEyeSelection.localSelections.pupilRight);

        //decreases value of index and cycles back to 0 if above max value
        int selected = CycleIndex(currentIndex - 1, 0, noLefts.Count - 1);

        Plugin.Spam($"CustomPupils Total: {CustomEyeManager.AllPupilTypes.Count}, Filtered: {noLefts.Count}");

        //set new selection var
        CustomPupilType newSelection = noLefts[selected];

        //get local eyes
        CustomEyeManager.AllPatchedEyes.RemoveAll(p => p.playerRef == null);
        PatchedEyes patchedEyes = PatchedEyes.GetPatchedEyes(PlayerAvatar.instance);

        patchedEyes.SelectPupil(ref PlayerEyeSelection.localSelections.pupilRight, newSelection, ref patchedEyes.RightPupilObject, eyeRight);

        UpdateLabels();

        CustomEyeManager.EmptyTrash();
    }

    private static void GetPlayerMenuEyes(PlayerAvatarVisuals avatarVisuals)
    {
        //Plugin.Spam("Getting menu player eyes, local player can't see their own pupils");
        eyeLeft = avatarVisuals.playerEyes.pupilLeft;
        eyeRight = avatarVisuals.playerEyes.pupilRight;
        if (eyeLeft == null || eyeRight == null)
            Plugin.logger.LogWarning("GetPlayerMenuEyes got null transform!");
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
