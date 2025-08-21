using MoreEyes.Core;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static MoreEyes.EyeManagement.CustomEyeManager;

namespace MoreEyes.EyeManagement;

//Each player should have their own PatchedEyes component
internal class PatchedEyes : MonoBehaviour
{
    internal string playerID = "";
    internal PlayerAvatar Player { get; private set; } = null!;
    internal static PatchedEyes Local 
    { 
        get 
        {
            if(PlayerAvatar.instance == null)
            {
                Plugin.logger.LogError("Unable to return PatchedEyes Local, PlayerAvatar instance is null!");
                return null;
            }    
            PatchedEyes local = PlayerAvatar.instance.GetComponent<PatchedEyes>();
            if (local == null)
                return PlayerAvatar.instance.AddComponent<PatchedEyes>();
            return local;
        }
    }

    //Using EyeRef class to track transforms, game objects, and positioning
    internal EyeRef LeftEye { get; set; }
    internal EyeRef RightEye { get; set; }

    internal PlayerEyeSelection CurrentSelections
    {
        get
        {
            if(PlayerEyeSelection.TryGetSelections(playerID, out var selections))
                return selections;
            else
            {
                Plugin.logger.LogError($"TRYING TO GET NULL PLAYEREYESELECTIONS!! [{playerID}]");
                return null!;
            }
        }
    }

    private void Awake()
    {
        AllPatchedEyes.RemoveAll(p => p == null);
        Player = GetComponent<PlayerAvatar>();
        playerID = Player.steamID;
        AllPatchedEyes.Add(this);
    }

    private void Start()
    {
        GameObject originalLeft = Player.playerAvatarVisuals.playerEyes.pupilLeft.GetChild(0).GetChild(0).gameObject;
        GameObject originalRight = Player.playerAvatarVisuals.playerEyes.pupilRight.GetChild(0).GetChild(0).gameObject;

        //Create EyeRefs
        //Must be done at launch for menu eyes to work properly
        GameObject left = new("MoreEyes-LEFT");
        left.transform.SetParent(Player.playerAvatarVisuals.playerEyes.pupilLeft.GetChild(0));

        GameObject right = new("MoreEyes-RIGHT");
        right.transform.SetParent(Player.playerAvatarVisuals.playerEyes.pupilRight.GetChild(0));

        LeftEye = left.AddComponent<EyeRef>();
        RightEye = right.AddComponent<EyeRef>();

        Plugin.Spam($"EyeRefs set for {Player.playerName}");

        // Create vanilla pupils
        // This will create a copy of the object (prefab) for our class and disable it
        VanillaPupilLeft.VanillaSetup(true, originalLeft);
        VanillaPupilRight.VanillaSetup(false, originalRight);
    }

    internal void SetMenuEyes(PlayerAvatarVisuals visuals)
    {
        GameObject originalLeft = visuals.playerEyes.pupilLeft.GetChild(0).GetChild(0).gameObject;
        GameObject originalRight = visuals.playerEyes.pupilRight.GetChild(0).GetChild(0).gameObject;

        GameObject left = new("MoreEyes-LEFT-MENU");
        left.transform.SetParent(visuals.playerEyes.pupilLeft.GetChild(0));
        GameObject right = new("MoreEyes-RIGHT-MENU");
        right.transform.SetParent(visuals.playerEyes.pupilRight.GetChild(0));
        LeftEye.EyeMenuPos.Add(left.transform.parent);
        RightEye.EyeMenuPos.Add(right.transform.parent);
        LeftEye.SetFirstPupilMenu(originalLeft);
        RightEye.SetFirstPupilMenu(originalRight);
        SetPlayerSavedSelection(Player);
        //Actually change eyes
        CurrentSelections.PlayerEyesSpawn();
    }

    //used to change existing pupil to new selection
    internal void SelectPupil(CustomPupilType newSelection, bool isLeft)
    {
        if (newSelection.Prefab == null)
        {
            Plugin.logger.LogWarning($"Invalid Selection! Pupil reference object for {newSelection.Name} is null");
            return;
        }

        EyeRef eye = isLeft ? LeftEye : RightEye;
        eye.RemovePupil();
        eye.CreatePupil(newSelection);
        CurrentSelections.UpdateSelectionOf(isLeft, newSelection);

        FileManager.UpdateWrite = true;
    }

    //used to change existing iris to new selection
    internal void SelectIris(CustomIrisType newSelection, bool isLeft)
    {
        EyeRef eye = isLeft ? LeftEye : RightEye;
        eye.RemoveIris();
        eye.CreateIris(newSelection);
        CurrentSelections.UpdateSelectionOf(isLeft, newSelection);

        FileManager.UpdateWrite = true;
    }

    internal void SetPlayerSavedSelection(PlayerAvatar player)
    {
        PlayerEyeSelection playerChoices = PlayerEyeSelection.GetPlayerEyeSelection(player.steamID);
        if (playerChoices == null)
        {
            Plugin.logger.LogWarning($"{player.playerName} does not have PlayerEyeSelection component!");
            return;
        }
            
        //Get from save file
        playerChoices.GetSavedSelection();
    }

    internal void RandomizeEyes()
    {
        Plugin.Spam($"Randomizing local eye settings!");

        // --- Create Lists for each Pupil/Iris
        //There's prob a better way of doing this
        List<CustomPupilType> NoRightPupils = AllPupilTypes.FindAll(p => !p.Name.EndsWith("_right"));
        List<CustomPupilType> NoLeftPupils = AllPupilTypes.FindAll(p => !p.Name.EndsWith("_left"));
        List<CustomIrisType> NoRightIris = AllIrisTypes.FindAll(i => !i.Name.EndsWith("_right"));
        List<CustomIrisType> NoLeftIris = AllIrisTypes.FindAll(i => !i.Name.EndsWith("_left"));


        // -- Select Random Pupil/Iris from list using Rand.Next
        CustomPupilType randomL = NoRightPupils[Plugin.Rand.Next(0, NoRightPupils.Count)];
        CustomPupilType randomR = NoLeftPupils[Plugin.Rand.Next(0, NoLeftPupils.Count)];
        CustomIrisType irisL = NoRightIris[Plugin.Rand.Next(0, NoRightIris.Count)];
        CustomIrisType irisR = NoLeftIris[Plugin.Rand.Next(0, NoLeftIris.Count)];

        // --- Make Selections
        SelectPupil(randomL, true);
        SelectPupil(randomR, false);

        SelectIris(irisL, true);
        SelectIris(irisR, false);

        CurrentSelections.SetRandomColors();
    }

    internal void ResetEyes()
    {
        SelectPupil(VanillaPupilLeft, true);
        SelectPupil(VanillaPupilRight, false);
        SelectIris(VanillaIris, true);
        SelectIris(VanillaIris, false);
        CurrentSelections.SetDefaultColors();

        FileManager.UpdateWrite = true;
    }
}
