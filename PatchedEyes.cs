using MoreEyes;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static MoreEyes.CustomEyeManager;


//this gets created/destroyed with whatever game object it's attached to
public class PatchedEyes : MonoBehaviour
{
    internal string playerID = "";
    internal PlayerAvatar playerRef = null!;

    //Objects
    public GameObject LeftPupilObject = null!; //these have to be explicitly set in order to use them with ref
    public GameObject RightPupilObject = null!; //these have to be explicitly set in order to use them with ref
    public GameObject LeftIrisObject = null!;
    public GameObject RightIrisObject = null!;

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

    //method to replace either pupil with a new game object (instantiated from prefab)
    public void OnePupilReplace(ref GameObject PupilGameObject, GameObject oldEye, CustomPupilType newPupil)
    {
        // Destroy any existing pupil
        Destroy(PupilGameObject);
        Plugin.Spam("destroyed any existing custom pupil");

        // Instantiate the new pupil
        PupilGameObject = Instantiate(newPupil.Prefab, oldEye.transform.parent);
        PupilGameObject.SetActive(true);
        Plugin.Spam("Instantiated new pupil from prefab");

        Transform pupilTransform = PupilGameObject.GetComponent<Transform>();
        pupilTransform.SetPositionAndRotation(oldEye.transform.position, oldEye.transform.rotation);

        newPupil.inUse = true;
        MarkedForDeletion.Add(oldEye);
        Plugin.Spam($"{oldEye.name} marked for deletion");
    }

    //method to update either iris with a new game object (instantiated from prefab)
    //null game object will simply result in the old iris being removed
    public void OneIrisAdd(ref GameObject IrisGameObject, Transform pupil, CustomIrisType newIris)
    {       
        // Destroy any existing iris
        // Destroy has a null check to prevent NREs where iris doesn't exist
        Destroy(IrisGameObject);
        Plugin.Spam("destroyed any existing custom iris");

        if (newIris.Prefab == null)
        {
            Plugin.Spam($"No iris game object for {newIris.Name}");
            return;
        }

        IrisGameObject = Instantiate(newIris.Prefab, pupil);
        IrisGameObject.SetActive(true);
        Plugin.Spam("Instantiated new iris");

        Transform irisTransform = IrisGameObject.GetComponent<Transform>();
        irisTransform.SetPositionAndRotation(pupil.position, pupil.rotation);

        newIris.inUse = true;
    }

    private void Destroy(GameObject gameObject)
    {
        if(gameObject != null)
            UnityEngine.Object.Destroy(gameObject);
    }

    //used to change existing pupil to new selection
    internal void SelectPupil(ref CustomPupilType current, CustomPupilType newSelection, ref GameObject PupilObject, Transform original)
    {
        if (!TryGetPupil(original, out GameObject originalPupilObject))
            return;

        current?.MarkPupilUnused();
        if(newSelection.Prefab == null)
        {
            Plugin.logger.LogWarning($"Invalid Selection! Pupil reference object for {newSelection.Name} is null");
            PupilObject = originalPupilObject;
            return;
        }

        OnePupilReplace(ref PupilObject, originalPupilObject, newSelection);
        current = newSelection;
        Plugin.logger.LogMessage($"Selected pupil {newSelection.Name}");
    }

    //used to change existing iris to new selection
    internal void SelectIris(ref CustomIrisType current, CustomIrisType newSelection, ref GameObject IrisObject, GameObject PupilObject)
    {
        current?.MarkIrisUnused();

        if (PupilObject == null)
        {
            Plugin.logger.LogError($"PupilObject is null! Cannot set Iris for {newSelection.Name}");
            return;
        }

        OneIrisAdd(ref IrisObject, PupilObject.transform, newSelection);
        current = newSelection;
        Plugin.logger.LogMessage($"Added iris with {newSelection.Name}");
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

    // broke this out into a couple of different methods
    // there might be a cleaner way of handling this once we have menus and stuff
    // using this now as a randomize eyes function
    internal void RandomizeEyes(string Name, Transform leftTransform, Transform rightTransform)
    {
        Plugin.Spam($"1 - {Name} pupils have been detected!");

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

        SelectPupil(ref playerSelections.pupilLeft, randomL, ref LeftPupilObject, leftTransform);
        SelectPupil(ref playerSelections.pupilRight, randomR, ref RightPupilObject, rightTransform);
        Plugin.Spam($"2 - {Name} pupils have been updated!");

        SelectIris(ref playerSelections.irisLeft, irisL, ref LeftIrisObject, LeftPupilObject);
        SelectIris(ref playerSelections.irisRight, irisR, ref RightIrisObject, RightPupilObject);
        Plugin.Spam($"3 - {Name} iris have been updated!");

        //Delete oldeyes gameobjects
        //delayed to avoid issues with other eye selections
        EmptyTrash();
    }
}