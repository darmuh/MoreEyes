using HarmonyLib;
using MoreEyes.Core;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MoreEyes.EyeManagement;

internal class PlayerEyeSelection
{
    //internal static PlayerEyeSelection localSelections;
    //should save all selections to a text file in appdata probably
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
            patchedEyes.LeftIrisObjects.DoIf(o => o != null, o => o.GetComponentInChildren<MeshRenderer>().material.SetColor("_EmissionColor", value));
        }
    }

    private Color _irisRightColor = Color.black;
    internal Color IrisRightColor
    {
        get { return _irisRightColor; }
        set
        {
            _irisRightColor = value;
            patchedEyes.RightIrisObjects.DoIf(o => o != null, o => o.GetComponentInChildren<MeshRenderer>().material.SetColor("_EmissionColor", value));
        }
    }

    private Color _pupilLeftColor = Color.black;
    internal Color PupilLeftColor
    {
        get { return _pupilLeftColor; }
        set
        {
            _pupilLeftColor = value;
            patchedEyes.LeftPupilObjects.DoIf(o => o != null, o => o.GetComponentInChildren<MeshRenderer>().material.SetColor("_EmissionColor", value));
        }
    }

    private Color _pupilRightColor = Color.black;
    internal Color PupilRightColor
    {
        get { return _pupilRightColor; }
        set
        {
            _pupilRightColor = value;
            patchedEyes.RightPupilObjects.DoIf(o => o != null, o => o.GetComponentInChildren<MeshRenderer>().material.SetColor("_EmissionColor", value));
        }
    }

    public PlayerEyeSelection(string steamID)
    {
        playerID = steamID;
        //if (steamID == PlayerAvatar.instance.steamID)
            //localSelections = this;
        
        CustomEyeManager.AllPlayerSelections.Add(this);
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
                if (ColorUtility.TryParseHtmlString(s.Value, out Color color))
                    PupilLeftColor = color;
            }
            else if (s.Key == "pupilRightColor")
            {
                if (ColorUtility.TryParseHtmlString(s.Value, out Color color))
                    PupilRightColor = color;
            }
            else if (s.Key == "irisLeftColor")
            {
                if (ColorUtility.TryParseHtmlString(s.Value, out Color color))
                    IrisLeftColor = color;
            }
            else if (s.Key == "irisRightColor")
            {
                if (ColorUtility.TryParseHtmlString(s.Value, out Color color))
                    IrisRightColor = color;
            }
            else
            {
                Plugin.logger.LogWarning($"Unexpected key in saved selections: {s.Key}");
            }
        });
    }

    public void SetDefaultColors()
    {
        PupilLeftColor = Color.black;
        PupilRightColor = Color.black;
        IrisLeftColor = Color.black;
        IrisRightColor = Color.black;
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
