using MoreEyes.Collections;
using MoreEyes.Core;
using MoreEyes.Managers;
using Photon.Pun;
using UnityEngine;
using static MoreEyes.Addons.NetworkedAnimationTrigger;

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

    private static bool IsCacheDifferent(PlayerEyeSelection cache,  PlayerEyeSelection current)
    {
        if(FileManager.GetSelectionsText(cache) != FileManager.GetSelectionsText(current)) 
            return true;

        return false;
    }

    //To minimize network traffic being sent, only sync when the player leaves the Menu
    internal static void SyncMoreEyesChanges()
    {
        var old = PlayerEyeSelection.LocalCache;

        if (PhotonNetwork.MasterClient == null || old == null)
            return;

        Loggers.Debug($"SyncMoreEyesChanges: Sending other clients my changes (if any)");
        var local = PatchedEyes.Local.CurrentSelections;

        //using string values for quicker comparison and less complicated networking
        //if any change is detected, sync the full selection string
        if (IsCacheDifferent(old, local))
            instance.photonView.RPC("SetSelectionsByText", RpcTarget.OthersBuffered, local.playerID, local.GetSelectionsString());
    }

    internal static void SendNetwork(ParamType type, string name, object value = null)
    {
        if (SemiFunc.RunIsLevel() && instance != null && instance.photonView != null)
        {
            string playerID = PhotonNetwork.LocalPlayer.UserId;
            Loggers.Debug($"[Network] Sending RPC_SyncAnimatorParam to others: playerID={playerID}, param={name}, type={type}, value={value}");
            instance.photonView.RPC("RPC_SyncAnimatorParam", RpcTarget.Others, playerID, name, (int)type, value);
        }
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

    [PunRPC]
    internal void RPC_SyncAnimatorParam(string playerID, string paramName, int paramTypeInt, object value)
    {
        Loggers.Debug($"[RPC] RPC_SyncAnimatorParam received for playerID={playerID}, param={paramName}, type={paramTypeInt}, value={value}");

        var playerSelections = PlayerEyeSelection.GetPlayerEyeSelection(playerID);
        if (playerSelections == null) return;

        Animator animator = playerSelections.patchedEyes.GetComponent<Animator>();
        if (animator == null) return;

        var type = (ParamType)paramTypeInt;

        switch (type)
        {
            case ParamType.Bool:
                animator.SetBool(paramName, (bool)value);
                Loggers.Debug($"[RPC] Animator.SetBool({paramName}, {value})");
                break;
            case ParamType.Trigger:
                animator.SetTrigger(paramName);
                break;
            case ParamType.Float:
                animator.SetFloat(paramName, (float)value);
                break;
            case ParamType.Int:
                animator.SetInteger(paramName, (int)value);
                break;
        }
    }
}