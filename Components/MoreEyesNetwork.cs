using MoreEyes.Collections;
using MoreEyes.Core;
using MoreEyes.Managers;
using Photon.Pun;
using System.Linq;
using UnityEngine;

namespace MoreEyes.Components;
internal class MoreEyesNetwork : MonoBehaviour
{
    internal static MoreEyesNetwork instance;
    internal PhotonView photonView = null!;

    private void Awake()
    {
        instance = this;
        photonView = GetComponent<PhotonView>();
    }

    private void Start()
    {
        if (PhotonNetwork.MasterClient == null)
            return;

        Loggers.Debug($"MoreEyesNetwork Start: Sending other clients my selections");
        //Sync choices with others on Start
        var local = PatchedEyes.Local.CurrentSelections;

        //Objects
        instance.photonView.RPC("SetSelectionsByText",RpcTarget.OthersBuffered, local.playerID, local.GetSelectionsString());
    }

    //To minimize network traffic being sent, only sync when the player leaves the Menu
    internal static void SyncMoreEyesChanges()
    {
        var old = PlayerEyeSelection.LocalCache;

        if (PhotonNetwork.MasterClient == null || old == null)
            return;

        Loggers.Debug($"SyncMoreEyesChanges: Sending other clients my changes (if any)");
        var local = PatchedEyes.Local.CurrentSelections;

        bool prefabChanged = false;
        // -- Check Pupil for prefab changes

        if (old.pupilLeft != local.pupilLeft)
        {
            instance.photonView.RPC("SetSingleSelection", RpcTarget.OthersBuffered, local.playerID, true, true, local.pupilLeft.UID);
            prefabChanged = true;
        }

        if (old.pupilRight != local.pupilRight)
        {
            instance.photonView.RPC("SetSingleSelection", RpcTarget.OthersBuffered, local.playerID, false, true, local.pupilRight.UID);
            prefabChanged = true;
        }

        // -- Check Iris for prefab changes

        if (old.irisLeft != local.irisLeft)
        {
            instance.photonView.RPC("SetSingleSelection", RpcTarget.OthersBuffered, local.playerID, true, false, local.irisLeft.UID);
            prefabChanged = true;
        }

        if (old.irisRight != local.irisRight)
        {
            instance.photonView.RPC("SetSingleSelection", RpcTarget.OthersBuffered, local.playerID, false, false, local.irisRight.UID);
            prefabChanged = true;
        }

        // Color changes must follow any prefab changes
        void SendColorInformation(bool isLeft, bool isPupil, Color color)
        {
            if (prefabChanged)
                instance.StartCoroutine(DelayedColorRPC(local.playerID, isLeft, isPupil, color));
            else
                instance.photonView.RPC("SetSingleColorSelection", RpcTarget.OthersBuffered, local.playerID, isLeft, isPupil, ColorToVector(color));
        }

        if (old.PupilLeftColor != local.PupilLeftColor)
            SendColorInformation(true, true, local.PupilLeftColor);
        if (old.PupilRightColor != local.PupilRightColor)
            SendColorInformation(false, true, local.PupilRightColor);
        if (old.IrisLeftColor != local.IrisLeftColor)
            SendColorInformation(true, false, local.IrisLeftColor);
        if (old.IrisRightColor != local.IrisRightColor)
            SendColorInformation(false, false, local.IrisRightColor);
    }

    private static System.Collections.IEnumerator DelayedColorRPC(string playerID, bool isLeft, bool isPupil, Color color)
    {
        yield return null; // wait one frame
        instance.photonView.RPC("SetSingleColorSelection", RpcTarget.OthersBuffered, playerID, isLeft, isPupil, ColorToVector(color));
    }

    //no alpha info but that's okay
    private static Vector3 ColorToVector(Color color)
    {
        return new(color.r, color.g, color.b);
    }

    [PunRPC]
    internal void SetSingleSelection(string playerID, bool isLeft, bool isPupil, string uniqueID)
    {
        if (!PlayerEyeSelection.TryGetSelections(playerID, out PlayerEyeSelection selections))
            return;

        Loggers.Debug($"--- [SetSingleSelection RPC]\n{playerID}:L-{isLeft}:P-{isPupil}:UID-{uniqueID}");
        if (isPupil)
        {
            CustomPupilType selection = CustomEyeManager.AllPupilTypes.FirstOrDefault(x => x.UID == uniqueID);
            if (selection == null)
                Loggers.Warning($"Unable to sync pupil with Unique ID: {uniqueID}\nPlease verify all clients have the same MoreEyes mods (and the same versions!)");

            selections.patchedEyes.SelectPupil(selection, isLeft);
        }
        else
        {
            CustomIrisType selection = CustomEyeManager.AllIrisTypes.FirstOrDefault(x => x.UID == uniqueID);
            if (selection == null)
                Loggers.Warning($"Unable to sync iris with Unique ID: {uniqueID}\nPlease verify all clients have the same MoreEyes mods (and the same versions!)");

            selections.patchedEyes.SelectIris(selection, isLeft);
        }

        selections.ForceColors();

        FileManager.UpdateWrite = true;
    }

    [PunRPC]
    internal void SetSingleColorSelection(string playerID, bool isLeft, bool isPupil, Vector3 colorVector)
    {
        if (!PlayerEyeSelection.TryGetSelections(playerID, out PlayerEyeSelection selections))
            return;

        //https://discussions.unity.com/t/syncing-color-materials-pun/557222/10
        var color = new Color(colorVector.x, colorVector.y, colorVector.z);
        Loggers.Debug($"--- [SetSingleColorSelection RPC]\n{playerID}:L-{isLeft}:P-{isPupil}:C-{color}");

        if (isPupil)
        {
            if(isLeft) 
                selections.patchedEyes.LeftEye.SetColorPupil(color); 
            else 
                selections.patchedEyes.RightEye.SetColorPupil(color);
        }
        else
        {
            if (isLeft) 
                selections.patchedEyes.LeftEye.SetColorIris(color);
            else 
                selections.patchedEyes.RightEye.SetColorIris(color);
        }

        FileManager.UpdateWrite = true;
    }

    [PunRPC]
    private void SetSelectionsByText(string playerID, string selectionsValue)
    {
        var playerSelections = PlayerEyeSelection.GetPlayerEyeSelection(playerID);

        if (playerSelections == null)
            return;

        playerSelections.SetSelectionsFromPairs(FileManager.GetPairsFromString(selectionsValue));
        playerSelections.PlayerEyesSpawn();
        FileManager.WriteTextFile();
    }
}