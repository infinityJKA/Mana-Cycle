using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace Online {

    public class PlayerInitializer : NetworkBehaviour
    {
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            Debug.Log("initialized - server: "+IsServer);
        }
    }
}
