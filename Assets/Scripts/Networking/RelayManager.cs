using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using QC = QFSW.QC;
using Mirror;
using Utp;
using System.Threading.Tasks;
using System;

public class RelayManager {

    public static RelayNetworkManager relayNetworkManager {
        get {
            return NetworkManager.singleton.GetComponent<RelayNetworkManager>();
        }
    }

    [QC.Command]
    public static async Task<bool> CreateRelay() {
        await Authentication.Authenticate();

        if (!AuthenticationService.Instance.IsSignedIn) return false;

        try {
            Debug.Log("Creating relay host");
            
            // will start relay and start the host on networkmanager
            relayNetworkManager.StartRelayHost(1, onFailure: e => {
                PopupManager.instance.ShowError(e);
                OnlineMenu.singleton.EnableInteractables();
            });
            
        } catch (RelayServiceException e) {
            PopupManager.instance.ShowError(e);
            return false;
        }

        return true;
    }

    [QC.Command]
    /// <summary>
    /// Connect to another player via Relay.
    /// </summary>
    /// <param name="joinCode"></param>
    /// <returns>if the connection was a success or not</returns>
    public static async Task<bool> JoinRelay(string joinCode) {
        await Authentication.Authenticate();

        if (!AuthenticationService.Instance.IsSignedIn) return false;

        try {
            Debug.Log("Joining Relay with code "+joinCode);
            
            relayNetworkManager.relayJoinCode = joinCode;
            // starts relay and starts networkmanager as client when connected
            relayNetworkManager.JoinRelayServer(onFailure: e => {
                PopupManager.instance.ShowError(e);
                OnlineMenu.singleton.EnableInteractables();
            });
        } catch (Exception e) {
            PopupManager.instance.ShowError(e);
            return false;
        }

        return true;
    }
}