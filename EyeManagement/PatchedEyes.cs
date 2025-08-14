using HarmonyLib;
using MoreEyes.Core;
using System.Collections.Generic;
using System.Linq;
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
        get { return PlayerAvatar.instance.GetComponent<PatchedEyes>(); }
    }

    public List<GameObject> LeftPupilObjects { get; set; } = [];
    public List<GameObject> RightPupilObjects { get; set; } = [];
    public List<GameObject> LeftIrisObjects { get; set; } = [];
    public List<GameObject> RightIrisObjects { get; set; } = [];

    public Transform eyeLeftPos;
    public Transform eyeRightPos;
    public Transform eyeMenuLeft;
    public Transform eyeMenuRight;

    internal PlayerEyeSelection currentSelections = null!;

    //Created this to try to standardize the creation of the component
    //also to get references to existing components attached to any players
    //added the playerref bit so that it's a little easier to find by code
    public static PatchedEyes GetPatchedEyes(PlayerAvatar player)
    {
        AllPatchedEyes.RemoveAll(p => p.Player == null);
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
                tryGetComponent.Player = player;
                AllPatchedEyes.Add(tryGetComponent);
                return tryGetComponent;
            } 
        }
        else
            return TryGetFromID;
    }

    public void UpdateRefs(PlayerAvatar player)
    {
        Player = player;
        eyeLeftPos = Player.playerAvatarVisuals.playerEyes.pupilLeft;
        eyeRightPos = Player.playerAvatarVisuals.playerEyes.pupilRight;
    }

    //this is important for object deletion
    //The object hierarchy changed at some point and now requires two getchild(0)s instead of one
    //getting the vanilla prefab is done a bit cleaner but for this I think getchild(0) twice is best
    public void GetPlayerMenuEyes(PlayerAvatarVisuals avatarVisuals)
    {
        eyeMenuLeft = avatarVisuals.playerEyes.pupilLeft.GetChild(0);
        eyeMenuRight = avatarVisuals.playerEyes.pupilRight.GetChild(0);
        if (eyeMenuLeft == null || eyeMenuRight == null)
            Plugin.logger.LogWarning("GetPlayerMenuEyes got null transform!");

        SetSelectedEyes(Player);
    }

    internal void DeleteChildren(Transform parent)
    {
            for (int i = parent.childCount - 1; i >= 0; i--)
                if (parent.GetChild(i) != null)
                    Destroy(parent.GetChild(i).gameObject);
    }

    internal List<Transform> GetPupilParents(bool isLeft)
    {
        List<Transform> transforms = [];
        if (isLeft)
        {
            transforms = [eyeMenuLeft, eyeLeftPos];
            transforms.RemoveAll(t => t == null);
        }
        else
        {
            transforms = [eyeMenuRight, eyeRightPos];
            transforms.RemoveAll(t => t == null);
        }

        return transforms;
        
    }

    private void Destroy(GameObject gameObject)
    {
        if(gameObject != null)
            Object.Destroy(gameObject);
    }

    internal List<GameObject> GetPupilObjects(bool isLeft)
    {
        List<Transform> transforms;
        if (isLeft)
        {
            transforms = [eyeMenuLeft, eyeLeftPos];
            transforms.RemoveAll(t => t == null);
            return GetObjectsFromTransforms(transforms);
        }
        else
        {
            transforms = [eyeMenuRight, eyeRightPos];
            transforms.RemoveAll(t => t == null);
            return GetObjectsFromTransforms(transforms);
        }
    }

    internal void GetAllPupilObjects()
    {
        List<Transform> leftTransforms = [eyeMenuLeft, eyeLeftPos];
        List<Transform> rightTransforms = [eyeMenuRight, eyeRightPos];
        leftTransforms.RemoveAll(t => t == null);
        rightTransforms.RemoveAll(t => t == null);

        LeftPupilObjects = GetObjectsFromTransforms(leftTransforms);
        RightPupilObjects = GetObjectsFromTransforms(rightTransforms);
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
        //CustomPupilType current;

        if (newSelection.Prefab == null)
        {
            Plugin.logger.LogWarning($"Invalid Selection! Pupil reference object for {newSelection.Name} is null");
            return;
        }

        List<Transform> PupilParents = GetPupilParents(isLeft);
        Vector3 ChildPos;
        Quaternion ChildRot;

        Plugin.Spam($"PupilParents count - {PupilParents.Count}");

        PupilParents.Do(p =>
        {
            if(p.GetChild(0) == null)
            {
                Plugin.WARNING($"Pupil parent {p.name} has no child!");
                return;
            }
            
            //Save position/rotation info of child
            ChildPos = p.GetChild(0).position;
            ChildRot = p.GetChild(0).rotation;
            
            //Delete child and any remaining children
            DeleteChildren(p);

            // Instantiate the new pupil with current parent transform
            GameObject s = Instantiate(newSelection.Prefab, p);
            s.SetActive(true);
            Plugin.Spam("Instantiated new pupil from prefab");

            //Set to saved child pos/rot
            Transform pupilTransform = s.GetComponent<Transform>();
            pupilTransform.SetPositionAndRotation(ChildPos, ChildRot);

            if(!newSelection.isVanilla)
                pupilTransform.localPosition = new(0, 0, -0.1067f); //matching vanila pupil localPosition if not vanilla

            if (isLeft)
            {
                currentSelections.pupilLeft.inUse = false;
                newSelection.inUse = true;
                currentSelections.pupilLeft = newSelection;
            }
            else
            {
                currentSelections.pupilRight.inUse = false;
                newSelection.inUse = true;
                currentSelections.pupilRight = newSelection;
            }
                
            Plugin.Spam($"{p.name} marked for deletion");
        });

        Plugin.logger.LogMessage($"Selected pupil {newSelection.Name}");
        GetAllPupilObjects();

        //might be costly to run this every selection, idk
        FileManager.WriteTextFile();
    }

    //used to change existing iris to new selection
    internal void SelectIris(CustomIrisType newSelection, bool isLeft)
    {
        List<GameObject> PupilObjects = GetPupilObjects(isLeft);
        List<GameObject> IrisObjects;
        CustomIrisType current;

        if (isLeft)
        { 
            IrisObjects = LeftIrisObjects;
            current = currentSelections.irisLeft;
        }
        else
        {
            IrisObjects = RightIrisObjects;
            current = currentSelections.irisRight;
        }

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
            Plugin.Spam($"Instantiated new iris on pupil {p.name}");

            Transform irisTransform = IrisObject.GetComponent<Transform>();
            irisTransform.SetPositionAndRotation(p.transform.position, p.transform.rotation);

            if (isLeft && currentSelections.pupilLeft.isVanilla)
                irisTransform.localPosition = new(0, 0, -0.1067f); //matching vanila pupil localPosition if not vanilla
            if (!isLeft && currentSelections.pupilRight.isVanilla)
                irisTransform.localPosition = new(0, 0, -0.1067f); //matching vanila pupil localPosition if not vanilla

            newSelection.inUse = true;
            newObjects.Add(IrisObject);
        });

        IrisObjects.RemoveAll(i => i == null);
        IrisObjects.AddRange(newObjects);

        Plugin.logger.LogMessage($"Added iris with {newSelection.Name}");

        if (isLeft)
        {
            currentSelections.irisLeft = newSelection;
            LeftIrisObjects = IrisObjects;
        }
        else
        {
            currentSelections.irisRight = newSelection;
            RightIrisObjects = IrisObjects;
        }

        GetAllPupilObjects();
        //might be costly to run this every selection, idk
        FileManager.WriteTextFile();
    }

    //tries to get the pupil game object from a given transform
    //pupil has to be the direct child of a given transform
    internal static bool TryGetPupil(Transform pupil, out GameObject pupilObject)
    {
        pupilObject = null!;
        if (pupil.childCount == 0)
        {
            Plugin.logger.LogWarning("Unable to get Pupil from transform!");
            return false;
        }

        pupilObject = pupil.GetChild(0).gameObject;
        Plugin.Spam($"Got Pupil Object {pupilObject.name}");
        return true;
    }

    internal void SetSelectedEyes(PlayerAvatar player)
    {
        UpdateRefs(player);

        PlayerEyeSelection playerChoices = PlayerEyeSelection.GetPlayerEyeSelection(player.steamID);
        if (playerChoices == null)
            return;

        playerChoices.GetSavedSelection();

        SelectPupil(playerChoices.pupilLeft, true);
        SelectPupil(playerChoices.pupilRight, false);
        Plugin.Spam($"2 - {player.playerName} pupils have been updated!");

        SelectIris(playerChoices.irisLeft, true);
        SelectIris(playerChoices.irisRight, false);
        Plugin.Spam($"3 - {player.playerName} iris have been updated!");

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
    }

    internal void ResetEyes(PlayerAvatar player)
    {
        UpdateRefs(player);

        SelectPupil(VanillaPupilLeft, true);
        SelectPupil(VanillaPupilRight, false);
        SelectIris(VanillaIris, true);
        SelectIris(VanillaIris, false);
        currentSelections.SetDefaultColors();

    }
}