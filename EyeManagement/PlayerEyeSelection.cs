using HarmonyLib;
using MoreEyes.Core;
using MoreEyes.Menus;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MoreEyes.EyeManagement;

internal class PlayerEyeSelection
{
    internal string playerID = string.Empty;
    internal PatchedEyes patchedEyes;

    internal CustomPupilType pupilRight = CustomEyeManager.VanillaPupilRight;
    internal CustomPupilType pupilLeft = CustomEyeManager.VanillaPupilLeft;
    internal CustomIrisType irisRight = CustomEyeManager.VanillaIris;
    internal CustomIrisType irisLeft = CustomEyeManager.VanillaIris;

    private Color _irisLeftColor = Color.black;
    internal Color IrisLeftColor
    {
        get { return _irisLeftColor; }
        set
        {
            _irisLeftColor = value;
            patchedEyes.LeftEye?.SetColorIris(value);
        }
    }

    private Color _irisRightColor = Color.black;
    internal Color IrisRightColor
    {
        get { return _irisRightColor; }
        set
        {
            _irisRightColor = value;
            patchedEyes.RightEye?.SetColorIris(value);
        }
    }

    private Color _pupilLeftColor = Color.black;
    internal Color PupilLeftColor
    {
        get { return _pupilLeftColor; }
        set
        {
            _pupilLeftColor = value;
            patchedEyes.LeftEye?.SetColorPupil(value);
        }
    }

    private Color _pupilRightColor = Color.black;
    internal Color PupilRightColor
    {
        get { return _pupilRightColor; }
        set
        {
            _pupilRightColor = value;
            patchedEyes.RightEye?.SetColorPupil(value);
        }
    }

    public PlayerEyeSelection(string steamID)
    {
        playerID = steamID;
        
        CustomEyeManager.AllPlayerSelections.Add(this);
    }

    public void UpdateSelectionOf(bool isLeft, CustomPupilType selection)
    {
        if(isLeft)
        {
            pupilLeft.inUse = false;
            pupilLeft = selection;
            pupilLeft.inUse = true;
        }
        else
        {
            pupilRight.inUse = false;
            pupilRight = selection;
            pupilRight.inUse = true;
        }
    }

    public void UpdateSelectionOf(bool isLeft, CustomIrisType selection)
    {
        if (isLeft)
        {
            irisLeft.inUse = false;
            irisLeft = selection;
            irisLeft.inUse = true;
        }
        else
        {
            irisRight.inUse = false;
            irisRight = selection;
            irisRight.inUse = true;
        }
    }

    public static bool TryGetSelections(string steamID, out PlayerEyeSelection playerSelections)
    {
        playerSelections = GetPlayerEyeSelection(steamID);

        return playerSelections != null;
    }

    public static PlayerEyeSelection GetPlayerEyeSelection(string steamID)
    {
        return CustomEyeManager.AllPlayerSelections.FirstOrDefault(p => p.playerID == steamID);
    }

    public void UpdateColorOf(CustomPupilType pupil, Color color)
    {
        if (pupil == pupilLeft)
            PupilLeftColor = color;
        else if(pupil == pupilRight)
            PupilRightColor = color;
    }

    public void UpdateColorOf(CustomIrisType iris, Color color)
    {
        if (iris == irisLeft)
            IrisLeftColor = color;
        else if (iris == irisRight)
            IrisRightColor = color;
    }

    public Color GetColorOf(CustomPupilType pupil)
    {
        if(pupil == null)
        {
            Plugin.WARNING("Cannot get color of NULL pupil");
            return Color.black;
        }
        if (pupil == pupilLeft)
            return PupilLeftColor;
        if (pupil == pupilRight)
            return PupilRightColor;

        Plugin.WARNING($"Pupil {pupil.Name} is not assigned to player!");
        return Color.black;
    }

    public Color GetColorOf(CustomIrisType iris)
    {
        if (iris == null)
        {
            Plugin.WARNING("Cannot get color of NULL iris");
            return Color.black;
        }
        if (iris == irisLeft)
            return IrisLeftColor;
        if (iris == irisRight)
            return IrisRightColor;

        Plugin.WARNING($"Iris {iris.Name} is not assigned to player!");
        return Color.black;
    }

    public void GetSavedSelection()
    {
        if(!FileManager.PlayerSelections.ContainsKey(playerID))
        {
            Plugin.logger.LogMessage($"Unable to get saved selection for [ {playerID} ]");
            return;
        }

        Dictionary<string, string> selectionPairs = [];
        string selections = FileManager.PlayerSelections[playerID];

        selectionPairs = selections.Split(',')
                .Select(item => item.Trim())
                .Select(item => item.Split('='))
                .ToDictionary(pair => pair[0].Trim(), pair => pair[1].Trim());

        selectionPairs.Do(s =>
        {
            if (s.Key == "pupilLeft")
            {
                if(TryGetPupil(s.Value, out CustomPupilType saved))
                    pupilLeft = saved;
                else
                    Plugin.logger.LogWarning($"Selected left iris, \"{s.Value}\" could not be found in AllPupilTypes");
            }
            else if (s.Key == "pupilRight")
            {
                if (TryGetPupil(s.Value, out CustomPupilType saved))
                    pupilRight = saved;
                else
                    Plugin.logger.LogWarning($"Selected right iris, \"{s.Value}\" could not be found in AllPupilTypes");
            }
            else if (s.Key == "irisLeft")
            {
                if (TryGetIris(s.Value, out CustomIrisType saved))
                    irisLeft = saved;
                else
                    Plugin.logger.LogWarning($"Selected left iris, \"{s.Value}\" could not be found in AllIrisTypes");
            }
            else if (s.Key == "irisRight")
            {
                if (TryGetIris(s.Value, out CustomIrisType saved))
                    irisRight = saved;
                else
                    Plugin.logger.LogWarning($"Selected right iris, \"{s.Value}\" could not be found in AllIrisTypes");
            }
            else if (s.Key == "pupilLeftColor")
            {
                if (ColorUtility.TryParseHtmlString($"#{s.Value}", out Color color))
                    PupilLeftColor = color;
                else
                    Plugin.WARNING($"Failed to parse color from saved value! ({s.Value})");
            }
            else if (s.Key == "pupilRightColor")
            {
                if (ColorUtility.TryParseHtmlString($"#{s.Value}", out Color color))
                    PupilRightColor = color;
                else
                    Plugin.WARNING($"Failed to parse color from saved value! ({s.Value})");
            }
            else if (s.Key == "irisLeftColor")
            {
                if (ColorUtility.TryParseHtmlString($"#{s.Value}", out Color color))
                    IrisLeftColor = color;
                else
                    Plugin.WARNING($"Failed to parse color from saved value! ({s.Value})");
            }
            else if (s.Key == "irisRightColor")
            {
                if (ColorUtility.TryParseHtmlString($"#{s.Value}", out Color color))
                    IrisRightColor = color;
                else
                    Plugin.WARNING($"Failed to parse color from saved value! ({s.Value})");
            }
            else
            {
                Plugin.logger.LogWarning($"Unexpected key in saved selections: {s.Key}");
            }
        });

        if (patchedEyes.LeftEye == null || patchedEyes.RightEye == null)
            return;

        //color has to be first for some reason

        patchedEyes.SelectPupil(pupilLeft, true);
        patchedEyes.SelectPupil(pupilRight, false);

        patchedEyes.SelectIris(irisLeft, true);
        patchedEyes.SelectIris(irisRight, false);

        ForceColors();

        if (Menu.MoreEyesMenu.menuPage != null)
            Menu.UpdateButtons();

        Plugin.logger.LogMessage($"Updated {patchedEyes.Player} selection!\n\npupilLeft: {pupilRight.Name}\npupilRight: {pupilLeft.Name}\nirisLeft: {irisLeft.Name}\nirisRight: {irisRight.Name}\nPupilLeftColor: {PupilLeftColor}\nPupilRightColor: {PupilRightColor}\nIrisLeftColor: {IrisLeftColor}\nIrisRightColor: {IrisRightColor}");
    }

    public void ForceColors()
    {
        patchedEyes.LeftEye.SetColorPupil(PupilLeftColor);
        patchedEyes.LeftEye.SetColorIris(IrisLeftColor);
        patchedEyes.RightEye.SetColorPupil(PupilRightColor);
        patchedEyes.RightEye.SetColorIris(IrisRightColor);
    }

    public void SetDefaultColors()
    {
        PupilLeftColor = Color.black;
        PupilRightColor = Color.black;
        IrisLeftColor = Color.black;
        IrisRightColor = Color.black;
    }

    public void SetRandomColors()
    {
        PupilLeftColor = new(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
        PupilRightColor = new(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
        IrisLeftColor = new(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
        IrisRightColor = new(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
    }

    public bool TryGetPupil(string value, out CustomPupilType saved)
    {
        saved = CustomEyeManager.AllPupilTypes.FirstOrDefault(p => value == p.Name);
        return saved != null;
    }

    public bool TryGetIris(string value, out CustomIrisType saved)
    {
        saved = CustomEyeManager.AllIrisTypes.FirstOrDefault(i => value == i.Name);
        return saved != null;
    }
}
