using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using QC = QFSW.QC;
using Mirror;
using Utp;
using System.Threading.Tasks;

public class RelayManager {

    public static RelayNetworkManager relayNetworkManager {
        get {
            return NetworkManager.singleton.GetComponent<RelayNetworkManager>();
        }
    }

    [QC.Command]
    public static async Task CreateRelay() {
        await Authentication.Authenticate();

        try {
            Debug.Log("Creating relay host");
            
            // will start relay and start the host on networkmanager
            relayNetworkManager.StartRelayHost(1);
            
        } catch (RelayServiceException e) {
            Debug.Log(e);
        }
    }

    [QC.Command]
    public static async Task JoinRelay(string joinCode) {
        await Authentication.Authenticate();

        try {
            Debug.Log("Joining Relay with code "+joinCode);
            
            relayNetworkManager.relayJoinCode = joinCode;
            // starts relay and starts networkmanager as client when connected
            relayNetworkManager.JoinRelayServer();
        } catch (RelayServiceException e) {
            Debug.Log(e);
        }
    }
}