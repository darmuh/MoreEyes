using HarmonyLib;
using MoreEyes.Core;
using System.Collections.Generic;
using System.Linq;

namespace MoreEyes.EyeManagement;

public class PlayerEyeSelection
{
    internal static PlayerEyeSelection localSelections;
    //should save all selections to a text file in appdata probably
    internal string playerID = string.Empty;
    internal PatchedEyes patchedEyes;

    internal CustomPupilType pupilRight = CustomEyeManager.VanillaPupilRight;
    internal CustomPupilType pupilLeft = CustomEyeManager.VanillaPupilLeft;
    internal CustomIrisType irisRight = CustomEyeManager.VanillaIris;
    internal CustomIrisType irisLeft = CustomEyeManager.VanillaIris;

    public PlayerEyeSelection(string steamID)
    {
        playerID = steamID;
        if (steamID == PlayerAvatar.instance.steamID)
            localSelections = this;

        GetSavedSelection();
        
        CustomEyeManager.AllPlayerSelections.Add(this);
    }

    public void GetSavedSelection()
    {
        if(!FileManager.playerSelections.ContainsKey(playerID))
        {
            Plugin.logger.LogWarning($"Unable to get saved selection for [ {playerID} ]");
            return;
        }

        Dictionary<string, string> selectionPairs = [];
        string selections = FileManager.playerSelections[playerID];

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
                    Plugin.logger.LogWarning($"Selected left pupil, \"{s.Value}\" could not be found in AllPupilTypes");
            }
            else if (s.Key == "pupilRight")
            {
                if (TryGetPupil(s.Value, out CustomPupilType saved))
                    pupilRight = saved;
                else
                    Plugin.logger.LogWarning($"Selected right pupil, \"{s.Value}\" could not be found in AllPupilTypes");
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
            else
            {
                Plugin.logger.LogWarning($"Unexpected key in saved selections: {s.Key}");
            }
        });
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
