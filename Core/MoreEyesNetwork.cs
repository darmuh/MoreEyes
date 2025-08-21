﻿using MoreEyes.EyeManagement;
using Photon.Pun;
using System.Linq;
using UnityEngine;

namespace MoreEyes.Core;
internal class MoreEyesNetwork : MonoBehaviour
{
    internal static MoreEyesNetwork instance;
    internal PhotonView photonView = null!;

    private void Awake()
    {
        instance = this;
        photonView = GetComponent<PhotonView>();
    }

    //To minimize network traffic being sent, only sync when the player leaves the Menu
    internal static void SyncMoreEyesChanges()
    {
        var old = PlayerEyeSelection.LocalCache;

        if (PhotonNetwork.MasterClient == null || old == null)
            return;

        var local = PatchedEyes.Local.CurrentSelections;

        // -- Check Pupil for prefab changes

        if (old.pupilLeft != local.pupilLeft)
            instance.photonView.RPC("SetPlayerSelection", RpcTarget.OthersBuffered, local.playerID, true, true, local.pupilLeft.UID);

        if (old.pupilRight != local.pupilRight)
            instance.photonView.RPC("SetPlayerSelection", RpcTarget.OthersBuffered, local.playerID, false, true, local.pupilRight.UID);

        // -- Check Iris for prefab changes

        if (old.irisLeft != local.irisLeft)
            instance.photonView.RPC("SetPlayerSelection", RpcTarget.OthersBuffered, local.playerID, true, false, local.irisLeft.UID);

        if (old.irisRight != local.irisRight)
            instance.photonView.RPC("SetPlayerSelection", RpcTarget.OthersBuffered, local.playerID, false, false, local.irisRight.UID);

        // --- Color changes must follow any prefab changes!! --- //

        // -- Check Pupil for color changes

        if (old.PupilLeftColor != local.PupilLeftColor)
            instance.photonView.RPC("SetPlayerColorSelection", RpcTarget.OthersBuffered, local.playerID, true, true, ColorToVector(local.PupilLeftColor));

        if (old.PupilRightColor != local.PupilRightColor)
            instance.photonView.RPC("SetPlayerColorSelection", RpcTarget.OthersBuffered, local.playerID, false, true, ColorToVector(local.PupilRightColor));

        // -- Check Iris for color changes

        if (old.IrisLeftColor != local.IrisLeftColor)
            instance.photonView.RPC("SetPlayerColorSelection", RpcTarget.OthersBuffered, local.playerID, true, false, ColorToVector(local.IrisLeftColor));

        if (old.IrisRightColor != local.IrisRightColor)
            instance.photonView.RPC("SetPlayerColorSelection", RpcTarget.OthersBuffered, local.playerID, false, false, ColorToVector(local.IrisRightColor));
    }

    //no alpha info but that's okay
    private static Vector3 ColorToVector(Color color)
    {
        return new(color.r, color.g, color.b);
    }

    [PunRPC]
    internal void SetPlayerSelection(string playerID, bool isLeft, bool isPupil, string uniqueID)
    {
        if (!PlayerEyeSelection.TryGetSelections(playerID, out PlayerEyeSelection selections))
            return;

        if(isPupil)
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

        FileManager.UpdateWrite = true;
    }

    [PunRPC]
    internal void SetPlayerColorSelection(string playerID, bool isLeft, bool isPupil, Vector3 colorVector)
    {
        if (!PlayerEyeSelection.TryGetSelections(playerID, out PlayerEyeSelection selections))
            return;

        //https://discussions.unity.com/t/syncing-color-materials-pun/557222/10
        var color = new Color(colorVector.x, colorVector.y, colorVector.z);

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
}
