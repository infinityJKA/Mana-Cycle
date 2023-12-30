using Battle.Board;
using Unity.Netcode;
using UnityEngine;

using System;
/// <summary>
/// This script controls a Board that represents a player connected over the network.
/// Receives and sends relayed messages from the other player.
/// </summary>
public class NetworkBoard : NetworkBehaviour {
    private GameBoard board;

    // for offline functions -- to be run on initializaton in place of things that would normally be handled in online mode on network spawn
    private void Awake() {
        board = GetComponent<GameBoard>();

        if (!Storage.online) {
            board.rngManager.InitializeRngWithSeed(UnityEngine.Random.Range(int.MinValue, int.MaxValue));
        }
    }

    [ClientRpc]
    public void SetSeedClientRpc(int seed) {
        board.rngManager.InitializeRngWithSeed(seed);
    }

    // Set the RNG seed of each board on network spawn

    // TODO: Rng is probably going to be hella wacky when latency is involved,
    // maybe make every RNG call server authorative, as well as many other things.
    // just trying to get things working for now, ive spent too god damn long on this
    public override void OnNetworkSpawn()
    {
        if (IsServer) {
            int seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
            SetSeedClientRpc(seed);
        }
    }
}