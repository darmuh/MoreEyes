﻿using MoreEyes.Collections;
using MoreEyes.Core;
using MoreEyes.Managers;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static MoreEyes.Managers.CustomEyeManager;

namespace MoreEyes.Components;
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
                Loggers.Error("Unable to return PatchedEyes Local, PlayerAvatar instance is null!");
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
                Loggers.Error($"TRYING TO GET NULL PLAYEREYESELECTIONS!! [{playerID}]");
                return null!;
            }
        }
    }

    internal static PatchedEyes GetPatchedEyes(PlayerAvatar player)
    {
        if(player.GetComponent<PatchedEyes>() == null)
            return player.AddComponent<PatchedEyes>();
        else
            return player.GetComponent<PatchedEyes>();
    }

    private void Awake()
    {
        AllPatchedEyes.RemoveAll(p => p == null);
        Player = GetComponent<PlayerAvatar>();
        playerID = Player.steamID;

        if (!PlayerEyeSelection.TryGetSelections(Player.steamID, out PlayerEyeSelection selections))
            selections = new(Player.steamID);

        selections.patchedEyes = this;
        SetPlayerSavedSelection(Player);

        AllPatchedEyes.Add(this);
    }

    private void Start()
    {
        GameObject originalLeft = Player.playerAvatarVisuals.playerEyes.pupilLeft.GetChild(0).GetChild(0).gameObject;
        GameObject originalRight = Player.playerAvatarVisuals.playerEyes.pupilRight.GetChild(0).GetChild(0).gameObject;

        GameObject left = new("MoreEyes-LEFT");
        left.transform.SetParent(Player.playerAvatarVisuals.playerEyes.pupilLeft.GetChild(0));
        left.transform.localPosition = Vector3.zero;

        GameObject right = new("MoreEyes-RIGHT");
        right.transform.SetParent(Player.playerAvatarVisuals.playerEyes.pupilRight.GetChild(0));
        right.transform.localPosition = Vector3.zero;

        LeftEye = left.AddComponent<EyeRef>();
        RightEye = right.AddComponent<EyeRef>();
        LeftEye.EyePlayerPos = left.transform;
        RightEye.EyePlayerPos = right.transform;
        LeftEye.SetFirstPupilActual(originalLeft);
        RightEye.SetFirstPupilActual(originalRight);

        if (!VanillaPupilsExist)
        {
            VanillaPupilLeft.VanillaSetup(true, originalLeft);
            VanillaPupilRight.VanillaSetup(false, originalRight);
        }

        if (Player.isLocal)
            return;

        SetPlayerSavedSelection(Player);
        CurrentSelections.PlayerEyesSpawn();
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

        CurrentSelections.PlayerEyesSpawn();
    }

    internal void SelectPupil(CustomPupilType newSelection, bool isLeft)
    {
        if (newSelection.Prefab == null)
        {
            Loggers.Warning($"Invalid Selection! Pupil reference object for {newSelection.Name} is null");
            return;
        }

        EyeRef eye = isLeft ? LeftEye : RightEye;
        eye.RemovePupil();
        eye.CreatePupil(newSelection);
        CurrentSelections.UpdateSelectionOf(isLeft, newSelection);

        FileManager.UpdateWrite = true;
    }

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
            Loggers.Warning($"{player.playerName} does not have PlayerEyeSelection component!");
            return;
        }

        playerChoices.GetCachedSelections();
    }

    internal void RandomizeEyes()
    {
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
