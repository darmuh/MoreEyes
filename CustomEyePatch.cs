using System;
using UnityEngine;


public class CustomEyePatch : MonoBehaviour
{
    public GameObject LeftPupilObject { get; private set; }
    public GameObject RightPupilObject { get; private set; }

    public CustomEyePatch(GameObject leftPupilObject, GameObject rightPupilObject)
    {
        LeftPupilObject = leftPupilObject;
        RightPupilObject = rightPupilObject;
    }

    public void ReplaceEyePatches(GameObject newLeftEye, GameObject newRightEye)
    {
        // Destroy the existing eye patches
        Destroy(LeftPupilObject);
        Destroy(RightPupilObject);

        // Instantiate the new eye patches
        LeftPupilObject = Instantiate(newLeftEye, LeftPupilObject.transform.parent);
        RightPupilObject = Instantiate(newRightEye, RightPupilObject.transform.parent);
    }

    private void Destroy(GameObject gameObject)
    {
        UnityEngine.Object.Destroy(gameObject);
    }
}
