using UnityEngine;
using Mirror;
using VersusMode;
using Networking;
using Steamworks;

public class SteamNetManager : NetworkManager {
    public override void OnStartHost()
    {
        base.OnStartHost();
        Debug.Log("Hosting at address "+singleton.networkAddress);

        OnlineMenu.singleton.ShowCharSelect();
        CharSelectMenu.Instance.p2Selector.ShowJoinCode();
    }

    public override void OnClientConnect()
    {
        base.OnClientConnect();
        Debug.Log("Connected to host at address "+singleton.networkAddress);
        OnlineMenu.singleton.ShowCharSelect();
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnServerAddPlayer(conn);

        if (!conn.identity.isOwned) NetworkServer.localConnection.identity.GetComponent<NetPlayer>().CmdSetLockedIn();
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
        OnlineMenu.singleton.ShowOnlineMenu();
        OnlineMenu.singleton.EnableInteractables();
    }

    public override void OnStopHost()
    {
        base.OnStopHost();
        OnlineMenu.singleton.ShowOnlineMenu();
        OnlineMenu.singleton.EnableInteractables();
    }
}