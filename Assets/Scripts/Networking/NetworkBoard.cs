using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

using Battle.Board;
using Battle.Cycle;

namespace Battle.Networking {
    /// <summary>
    /// A GameObject that can be sent to the opponent in online multiplayer.
    /// Shows the current state of the board, HP, incoming damage, and more.
    /// </summary>
    public class NetworkBoard : NetworkBehaviour
    {
        /// <summary>The board that is being updated by this script</summary>
        [SerializeField] private GameBoard board;

        /// <summary>Player's displayed username</summary>
        [SerializeField] private string username;

        // ---- Variables sent over network
        /// <summary>The tiles array that is sent to opponent so they can see this player's board.</summary>
        // public NetworkVariable<ManaColor[,]> tiles = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        // /// <summary>This board's current remaining HP. Used to position the green HP bar and update displayed number.</summary>
        // public NetworkVariable<int> hp = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        // /// <summary>All incoming damage on the opponent's board.</summary>
        // public NetworkVariable<int[]> incoming = new(new int[6], NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);


        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn(); 

            if (!IsHost && IsOwner) {
                username = "Player Two";
            }

            SetUsernameServerRpc(this.username);
        }

        [ServerRpc]
        private void SetUsernameServerRpc(string username) {
            SetUsernameClientRpc(username);
        }

        [ClientRpc]
        private void SetUsernameClientRpc(string username) {
            this.username = username;
            gameObject.name = username;
        }
    }
}