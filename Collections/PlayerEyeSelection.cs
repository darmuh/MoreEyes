using HarmonyLib;
using MoreEyes.Components;
using MoreEyes.Core;
using MoreEyes.Managers;
using MoreEyes.Utility;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MoreEyes.Collections;
internal class PlayerEyeSelection
{
    internal static PlayerEyeSelection LocalCache = null!;
    internal bool isLocalCache = false;
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
            if(this != LocalCache)
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
            if (this != LocalCache)
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
            if (this != LocalCache)
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
            if (this != LocalCache)
                patchedEyes.RightEye?.SetColorPupil(value);
        }
    }

    public PlayerEyeSelection(string steamID)
    {
        playerID = steamID;
        
        CustomEyeManager.AllPlayerSelections.Add(this);
    }

    public PlayerEyeSelection(bool isCache)
    {
        playerID = "";
        isLocalCache = isCache;
        LocalCache = this;
    }

    public void UpdateSelectionOf(bool isLeft, CustomPupilType selection)
    {
        if(isLeft)
        {
            LocalCache.pupilLeft = pupilLeft;
            pupilLeft.inUse = false;
            pupilLeft = selection;
            pupilLeft.inUse = true;
        }
        else
        {
            LocalCache.pupilRight = pupilRight;
            pupilRight.inUse = false;
            pupilRight = selection;
            pupilRight.inUse = true;
        }
    }

    public void UpdateSelectionOf(bool isLeft, CustomIrisType selection)
    {
        if (isLeft)
        {
            LocalCache.irisLeft = irisLeft;
            irisLeft.inUse = false;
            irisLeft = selection;
            irisLeft.inUse = true;
        }
        else
        {
            LocalCache.irisRight = irisRight;
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

    public void UpdateColorInMenu(CustomPupilType pupil, Color color)
    {
        if (pupil == pupilLeft && Menu.CurrentEyeSide == Enums.EyeSide.Left)
        {
            LocalCache.PupilLeftColor = PupilLeftColor;
            PupilLeftColor = color;
        }   
        else if(pupil == pupilRight && Menu.CurrentEyeSide == Enums.EyeSide.Right)
        {
            LocalCache.PupilRightColor = PupilRightColor;
            PupilRightColor = color;
        }
    }

    public void UpdateColorInMenu(CustomIrisType iris, Color color)
    {
        if (iris == irisLeft && Menu.CurrentEyeSide == Enums.EyeSide.Left)
        {
            LocalCache.IrisLeftColor = IrisLeftColor;
            IrisLeftColor = color;
        }    
        else if (iris == irisRight && Menu.CurrentEyeSide == Enums.EyeSide.Right)
        {
            LocalCache.IrisRightColor = IrisRightColor;
            IrisRightColor = color;
        }     
    }

    public Color GetColorForMenu(CustomPupilType pupil)
    {
        if(pupil == null)
        {
            Loggers.Warning("Cannot get color of NULL pupil");
            return Color.black;
        }
        if (pupil == pupilLeft && Menu.CurrentEyeSide == Enums.EyeSide.Left)
            return PupilLeftColor;
        if (pupil == pupilRight && Menu.CurrentEyeSide == Enums.EyeSide.Right)
            return PupilRightColor;

        Loggers.Warning($"Pupil {pupil.Name} is not assigned to player!");
        return Color.black;
    }

    public Color GetColorForMenu(CustomIrisType iris)
    {
        if (iris == null)
        {
            Loggers.Warning("Cannot get color of NULL iris");
            return Color.black;
        }
        if (iris == irisLeft && Menu.CurrentEyeSide == Enums.EyeSide.Left)
            return IrisLeftColor;
        if (iris == irisRight && Menu.CurrentEyeSide == Enums.EyeSide.Right)
            return IrisRightColor;

        Loggers.Warning($"Iris {iris.Name} is not assigned to player!");
        return Color.black;
    }

    internal void GetCachedSelections()
    {
        if (!CustomEyeManager.VanillaPupilsExist)
            return;

        SetSelectionsFromPairs(FileManager.GetSelectionPairsFromFile(playerID));
    }

    internal void SetSelectionsFromPairs(Dictionary<string, string> selectionPairs)
    {
        selectionPairs.Do(s =>
        {
            if (s.Key == "pupilLeft")
            {
                if (TryGetPupil(s.Value, out CustomPupilType saved))
                    pupilLeft = saved;
                else
                    Loggers.Warning($"Selected left pupil, \"{s.Value}\" could not be found in AllPupilTypes");
            }
            else if (s.Key == "pupilRight")
            {
                if (TryGetPupil(s.Value, out CustomPupilType saved))
                    pupilRight = saved;
                else
                    Loggers.Warning($"Selected right pupil, \"{s.Value}\" could not be found in AllPupilTypes");
            }
            else if (s.Key == "irisLeft")
            {
                if (TryGetIris(s.Value, out CustomIrisType saved))
                    irisLeft = saved;
                else
                    Loggers.Warning($"Selected left iris, \"{s.Value}\" could not be found in AllIrisTypes");
            }
            else if (s.Key == "irisRight")
            {
                if (TryGetIris(s.Value, out CustomIrisType saved))
                    irisRight = saved;
                else
                    Loggers.Warning($"Selected right iris, \"{s.Value}\" could not be found in AllIrisTypes");
            }
            else if (s.Key == "pupilLeftColor")
            {
                if (ColorUtility.TryParseHtmlString($"#{s.Value}", out Color color))
                    PupilLeftColor = color;
                else
                    Loggers.Warning($"Failed to parse color from saved value! ({s.Value})");
            }
            else if (s.Key == "pupilRightColor")
            {
                if (ColorUtility.TryParseHtmlString($"#{s.Value}", out Color color))
                    PupilRightColor = color;
                else
                    Loggers.Warning($"Failed to parse color from saved value! ({s.Value})");
            }
            else if (s.Key == "irisLeftColor")
            {
                if (ColorUtility.TryParseHtmlString($"#{s.Value}", out Color color))
                    IrisLeftColor = color;
                else
                    Loggers.Warning($"Failed to parse color from saved value! ({s.Value})");
            }
            else if (s.Key == "irisRightColor")
            {
                if (ColorUtility.TryParseHtmlString($"#{s.Value}", out Color color))
                    IrisRightColor = color;
                else
                    Loggers.Warning($"Failed to parse color from saved value! ({s.Value})");
            }
            else
            {
                Loggers.Warning($"Unexpected key in saved selections: {s.Key}");
            }
        });
    }

    internal string GetSelectionsString()
    {
        if(FileManager.PlayerSelections.ContainsKey(playerID))
            return FileManager.PlayerSelections[playerID];
        else
            return string.Empty;
    }

    internal void PlayerEyesSpawn()
    {
        patchedEyes.SelectPupil(pupilLeft, true);
        patchedEyes.SelectPupil(pupilRight, false);

        patchedEyes.SelectIris(irisLeft, true);
        patchedEyes.SelectIris(irisRight, false);

        ForceColors();

        if (Menu.MoreEyesMenu.menuPage != null)
            Menu.UpdateButtons();
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
        saved = CustomEyeManager.AllPupilTypes.FirstOrDefault(p => value == p.UID);
        return saved != null;
    }

    public bool TryGetIris(string value, out CustomIrisType saved)
    {
        saved = CustomEyeManager.AllIrisTypes.FirstOrDefault(i => value == i.UID);
        return saved != null;
    }
}