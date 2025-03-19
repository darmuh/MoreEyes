using MoreEyes;
using UnityEngine;
using System.Linq;


public class PatchedEyes : MonoBehaviour
{
    internal string playerID = "";
    public GameObject LeftPupilObject { get; private set; }
    public GameObject RightPupilObject { get; private set; }
    internal CustomEyeType selectedRight = null!;
    internal CustomEyeType selectedLeft = null!;

    public PatchedEyes()
    {
        Plugin.AllPatchedEyes.Add(this);
        Plugin.AllPatchedEyes.Distinct();
        
    }

    public void UpdateObjectRefs(GameObject leftPupilObject, GameObject rightPupilObject)
    {
        LeftPupilObject = leftPupilObject;
        RightPupilObject = rightPupilObject;
        Plugin.Spam($"PatchedEyes created for {leftPupilObject.name} and {rightPupilObject.name}");
    }

    public void ReplaceEyePatches(GameObject oldLeftEye, GameObject oldRightEye, CustomEyeType newLeft, CustomEyeType newRight)
    {
        Plugin.Spam("ReplaceEyePatches");

        if(newLeft != selectedLeft)
        {
            SingleEyeReplace(LeftPupilObject, oldLeftEye, newLeft.Iris);
            selectedLeft = newLeft;
            Plugin.logger.LogMessage($"Replaced left pupil with {newLeft.Name}");
        }  
        
        if(newRight != selectedRight)
        {
            SingleEyeReplace(RightPupilObject, oldRightEye, newRight.Iris);
            selectedRight = newRight;
            Plugin.logger.LogMessage($"Replaced right pupil with {newRight.Name}");
        }
            
    }

    public void SingleEyeReplace(GameObject PatchedGameObject, GameObject oldEye, GameObject newEye)
    {
        Plugin.Spam("SingleEyeReplace");
        // Destroy any existing eye patches
        Destroy(PatchedGameObject);
        Plugin.Spam("destroy any existing custom pupil");

        // Instantiate the new eye patches
        PatchedGameObject = Instantiate(newEye, oldEye.transform.parent);
        Plugin.Spam("Instantiated new pupil");

        Transform pupilTransform = PatchedGameObject.GetComponent<Transform>();
        pupilTransform.SetPositionAndRotation(oldEye.transform.position, oldEye.transform.rotation);

        // Destroy old eyes
        Destroy(oldEye);
        Plugin.Spam("destroying the old eye object");
    }

    private void Destroy(GameObject gameObject)
    {
        if(gameObject != null)
            Object.Destroy(gameObject);
    }

    private void Hide(GameObject gameObject)
    {
        if (gameObject != null)
            gameObject.SetActive(false);
    }

    internal void CommonEyeMethod(string Name, Transform pupilLeft, Transform pupilRight)
    {
        if (pupilLeft.childCount == 0 || pupilRight.childCount == 0)
            return;

        Plugin.Spam($"{Name} pupils have been detected!");

        GameObject leftPupilObject = pupilLeft.GetChild(0).gameObject;
        GameObject rightPupilObject = pupilRight.GetChild(0).gameObject;

        Plugin.Spam($"{Name} pupils have been updated!");

        //since we don't have a method for selecting a particular iris, setting one manually
        CustomEyeType selected = Plugin.AllEyeTypes.FirstOrDefault(t => !t.isVanilla);
        CustomEyeType vanilla = Plugin.AllEyeTypes.Find(t => t.Name == "vanillaRight");

        //We will need to track each player's individual selections in this class

        ReplaceEyePatches(leftPupilObject, rightPupilObject, selected, vanilla);
    }
}
