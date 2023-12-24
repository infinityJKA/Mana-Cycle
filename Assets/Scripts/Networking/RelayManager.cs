using System.Linq.Expressions;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class RelayManager : MonoBehaviour {
    private void Start() {
        
    }

    private async void CreateRelay() {
        try {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(2);

            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            Debug.Log(joinCode);
        } catch (RelayServiceException e) {
            Debug.Log(e);
        }
    }

    private async void JoinRelay(string joinCode) {
        try {
            await RelayService.Instance.JoinAllocationAsync(joinCode);
        } catch (RelayServiceException e) {
            Debug.Log(e);
        }
    } 
}