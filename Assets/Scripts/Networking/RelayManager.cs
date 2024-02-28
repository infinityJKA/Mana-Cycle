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

    [QC.Command]
    public static async Task CreateRelay() {
        await Authentication.Authenticate();

        try {
            Debug.Log("Creating relay host");
            var relayNetworkManager = NetworkManager.singleton.GetComponent<RelayNetworkManager>();
            // will start relay and start the host on networkmanager
            relayNetworkManager.StartRelayHost(1);

            // // 1 additional opponent allowed - host is not included in this count
            // Allocation allocation = await RelayService.Instance.CreateAllocationAsync(1);

            // string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            
        } catch (RelayServiceException e) {
            Debug.Log(e);
        }
    }

    public async Task JoinRelay(string joinCode) {
        try {
            Debug.Log("Joining Relay with code "+joinCode);
            await RelayService.Instance.JoinAllocationAsync(joinCode);
        } catch (RelayServiceException e) {
            Debug.Log(e);
        }
    }
}