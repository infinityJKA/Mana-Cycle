using UnityEngine;
using Mirror;
using VersusMode;

public class NetManager : NetworkManager {
    public override void OnStartHost()
    {
        base.OnStartHost();
        Debug.LogWarning("Hosting at address "+NetworkManager.singleton.networkAddress);
        OnlineMenu.singleton.ShowCharSelect();
    }

    public override void OnClientConnect()
    {
        base.OnClientConnect();
        Debug.LogWarning("Connected to host at address "+NetworkManager.singleton.networkAddress);
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