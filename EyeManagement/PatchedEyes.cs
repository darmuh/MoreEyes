using HarmonyLib;
using MoreEyes.Core;
using MoreEyes.EyeManagement;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static MoreEyes.EyeManagement.CustomEyeManager;

namespace MoreEyes.EyeManagement;

public class PatchedEyes : MonoBehaviour
{
    internal string playerID = "";
    internal PlayerAvatar playerRef = null!;

    public List<GameObject> LeftPupilObjects = [];
    public List<GameObject> RightPupilObjects = [];
    public List<GameObject> LeftIrisObjects = [];
    public List<GameObject> RightIrisObjects = [];

    public Transform eyeLeftPos;
    public Transform eyeRightPos;
    public Transform eyeMenuLeft;
    public Transform eyeMenuRight;

    internal PlayerEyeSelection playerSelections = null!;

    //Created this to try to standardize the creation of the component
    //also to get references to existing components attached to any players
    //added the playerref bit so that it's a little easier to find by code
    public static PatchedEyes GetPatchedEyes(PlayerAvatar player)
    {
        AllPatchedEyes.RemoveAll(p => p.playerRef == null);
        PatchedEyes TryGetFromID = AllPatchedEyes.FirstOrDefault(p => p.playerID == PlayerAvatar.instance.steamID);

        if (TryGetFromID == null)
        {
            PatchedEyes tryGetComponent = player.gameObject.GetComponent<PatchedEyes>();
            if (tryGetComponent != null)
                return tryGetComponent;
            else
            {
                tryGetComponent = player.gameObject.AddComponent<PatchedEyes>();
                tryGetComponent.playerID = player.steamID;
                tryGetComponent.playerRef = player;
                AllPatchedEyes.Add(tryGetComponent);
                return tryGetComponent;
            } 
        }
        else
            return TryGetFromID;
    }

    public void UpdateRefs(PlayerAvatar player)
    {
        playerRef = player;
        eyeLeftPos = playerRef.playerAvatarVisuals.playerEyes.pupilLeft;
        eyeRightPos = playerRef.playerAvatarVisuals.playerEyes.pupilRight;
    }

    public void GetPlayerMenuEyes(PlayerAvatarVisuals avatarVisuals)
    {
        //Plugin.Spam("Getting menu player eyes, local player can't see their own pupils");
        eyeMenuLeft = avatarVisuals.playerEyes.pupilLeft;
        eyeMenuRight = avatarVisuals.playerEyes.pupilRight;
        if (eyeMenuLeft == null || eyeMenuRight == null)
            Plugin.logger.LogWarning("GetPlayerMenuEyes got null transform!");
    }

    private void Destroy(GameObject gameObject)
    {
        if(gameObject != null)
            Object.Destroy(gameObject);
    }

    internal void GetPupilObjects(bool isLeft)
    {
        List<Transform> transforms;
        if (isLeft)
        {
            transforms = [eyeMenuLeft, eyeLeftPos];
            transforms.RemoveAll(t => t == null);
            LeftPupilObjects = GetObjectsFromTransforms(transforms);
        }
        else
        {
            transforms = [eyeMenuRight, eyeRightPos];
            transforms.RemoveAll(t => t == null);
            RightPupilObjects = GetObjectsFromTransforms(transforms);
        }
    }

    internal List<GameObject> GetObjectsFromTransforms(List<Transform> transforms)
    {
        List<GameObject> result = [];
        for (int i = 0; i < transforms.Count(); i++)
        {
            if (!TryGetPupil(transforms[i], out GameObject original))
                continue;
            else
            {
                result.Add(original);
            }
        }

        return result;
    }

    //used to change existing pupil to new selection
    internal void SelectPupil(CustomPupilType newSelection, bool isLeft)
    {
        List<GameObject> PupilObjects = [];
        CustomPupilType current;

        GetPupilObjects(isLeft);
        //using isLeft bool to lower amount of params since these are all expected values
        if (isLeft)
        {
            current = playerSelections.pupilLeft;
            PupilObjects = LeftPupilObjects;
        }
        else
        {
            current = playerSelections.pupilRight;
            PupilObjects = RightPupilObjects;
        }

        //Remove null refs
        PupilObjects.RemoveAll(p => p == null);

        if (newSelection.Prefab == null)
        {
            Plugin.logger.LogWarning($"Invalid Selection! Pupil reference object for {newSelection.Name} is null");
            return;
        }

        Plugin.Spam($"PupilObjects count - {PupilObjects.Count}");

        List<GameObject> newPupils = [];

        PupilObjects.Do(p =>
        {
            // Instantiate the new pupil
            GameObject s = Instantiate(newSelection.Prefab, p.transform.parent);
            s.SetActive(true);
            Plugin.Spam("Instantiated new pupil from prefab");

            Transform pupilTransform = s.GetComponent<Transform>();
            pupilTransform.SetPositionAndRotation(p.transform.position, p.transform.rotation);

            current.inUse = false;
            newSelection.inUse = true;
            MarkedForDeletion.Add(p);
            newPupils.Add(s);
            Plugin.Spam($"{p.name} marked for deletion");
        });

        PupilObjects.RemoveAll(p => p == null);
        PupilObjects.AddRange(newPupils);
        Plugin.logger.LogMessage($"Selected pupil {newSelection.Name}");

        if (isLeft)
            playerSelections.pupilLeft = newSelection;
        else
            playerSelections.pupilRight = newSelection;

        //might be costly to run this every selection, idk
        FileManager.WriteTextFile();
    }

    //used to change existing iris to new selection
    internal void SelectIris(CustomIrisType newSelection, bool isLeft)
    {
        List<GameObject> PupilObjects;
        List<GameObject> IrisObjects;
        CustomIrisType current;
        

        if (isLeft)
        {
            PupilObjects = LeftPupilObjects;
            IrisObjects = LeftIrisObjects;
            current = playerSelections.irisLeft;
        }
        else
        {
            PupilObjects = RightPupilObjects;
            IrisObjects = RightIrisObjects;
            current = playerSelections.irisRight;
        }

        //remove null refs
        PupilObjects.RemoveAll(p => p == null);
        if (PupilObjects.Count == 0)
        {
            Plugin.logger.LogError($"PupilObjects are null! Cannot set Iris for {newSelection.Name}");
            return;
        }

        IrisObjects.Do(i =>
        {
            // Destroy any existing iris
            // Destroy has a null check to prevent NREs where iris doesn't exist
            Destroy(i);
            Plugin.Spam("destroyed any existing custom iris");
        });

        List<GameObject> newObjects = [];
        //add iris object if needed
        PupilObjects.Do(p =>
        {
            current.inUse = false;
            if (newSelection.Prefab == null)
            {
                newSelection.inUse = true;
                Plugin.Spam($"No iris game object for {newSelection.Name}");
                return;
            }

            GameObject IrisObject = Instantiate(newSelection.Prefab, p.transform);
            IrisObject.SetActive(true);
            Plugin.Spam("Instantiated new iris");

            Transform irisTransform = IrisObject.GetComponent<Transform>();
            irisTransform.SetPositionAndRotation(p.transform.position, p.transform.rotation);


            newSelection.inUse = true;
            newObjects.Add(IrisObject);
        });

        IrisObjects.RemoveAll(i => i == null);
        IrisObjects.AddRange(newObjects);

        Plugin.logger.LogMessage($"Added iris with {newSelection.Name}");

        if (isLeft)
            playerSelections.irisLeft = newSelection;
        else
            playerSelections.irisRight = newSelection;

        //might be costly to run this every selection, idk
        FileManager.WriteTextFile();
    }

    //tries to get the pupil game object from a given transform
    internal static bool TryGetPupil(Transform pupil, out GameObject pupilObject)
    {
        pupilObject = null!;
        if (pupil.childCount == 0)
        {
            Plugin.logger.LogWarning("Unable to get Pupil from transform!");
            return false;
        }
            

        pupilObject = pupil.GetChild(0).gameObject;
        return true;
    }

    internal static void SetLocalEyes()
    {
        PatchedEyes local = GetPatchedEyes(PlayerAvatar.instance);
        local.SetSelectedEyes(PlayerAvatar.instance);
    }

    internal void SetSelectedEyes(PlayerAvatar player)
    {
        UpdateRefs(player);

        SelectPupil(playerSelections.pupilLeft, true);
        SelectPupil(playerSelections.pupilRight, false);
        Plugin.Spam($"2 - {player.playerName} pupils have been updated!");

        SelectIris(playerSelections.irisLeft, true);
        SelectIris(playerSelections.irisRight, false);
        Plugin.Spam($"3 - {player.playerName} iris have been updated!");

        //Delete oldeyes gameobjects
        //delayed to avoid issues with other eye selections
        EmptyTrash();
    }

    // broke this out into a couple of different methods
    // there might be a cleaner way of handling this once we have menus and stuff
    // using this now as a randomize eyes function
    internal void RandomizeEyes(PlayerAvatar player)
    {
        UpdateRefs(player);

        Plugin.Spam($"Randomizing {player.playerName}'s eye settings!");

        //There's prob a better way of doing this
        List<CustomPupilType> NoRightPupils = AllPupilTypes.FindAll(p => !p.Name.EndsWith("_right"));
        List<CustomPupilType> NoLeftPupils = AllPupilTypes.FindAll(p => !p.Name.EndsWith("_left"));

        List<CustomIrisType> NoRightIris = AllIrisTypes.FindAll(i => !i.Name.EndsWith("_right"));
        List<CustomIrisType> NoLeftIris = AllIrisTypes.FindAll(i => !i.Name.EndsWith("_left"));


        //since we don't have a method for selecting a particular iris/pupil, setting them manually/randomly
        CustomPupilType randomL = NoRightPupils[Plugin.Rand.Next(0, NoRightPupils.Count)];
        CustomPupilType randomR = NoLeftPupils[Plugin.Rand.Next(0, NoLeftPupils.Count)];
        CustomIrisType irisL = NoRightIris[Plugin.Rand.Next(0, NoRightIris.Count)];
        CustomIrisType irisR = NoLeftIris[Plugin.Rand.Next(0, NoLeftIris.Count)];

        // We will need to track each player's individual selections in CustomEyeManager probably
        // This can be done using the playerID (steamID) property

        SelectPupil(randomL, true);
        SelectPupil(randomR, false);
        Plugin.Spam($"2 - {player.playerName} pupils have been updated!");

        SelectIris(irisL, true);
        SelectIris(irisR, false);
        Plugin.Spam($"3 - {player.playerName} iris have been updated!");

        //Delete oldeyes gameobjects
        //delayed to avoid issues with other eye selections
        EmptyTrash();
    }

    internal void ResetEyes(PlayerAvatar player)
    {
        UpdateRefs(player);

        Plugin.Spam($"Resetting {player.playerName}'s eye settings!");

        CustomPupilType vanillaP = AllPupilTypes.Find(p => p.Name.StartsWith("Standard"));
        CustomIrisType vanillaI = AllIrisTypes.Find(p => p.Name.StartsWith(""));

        SelectPupil(vanillaP, true);
        SelectPupil(vanillaP, false);
        SelectIris(vanillaI, true);
        SelectIris(vanillaI, false);
    }
}