using UnityEngine;
using Mirror;
using VersusMode;

public class RelayNetManager : Utp.RelayNetworkManager {
    public override void OnStartHost()
    {
        base.OnStartHost();
        Debug.Log("Hosting at address "+networkAddress);
        Debug.Log("join code: "+relayJoinCode);
        OnlineMenu.singleton.ShowCharSelect();
        CharSelectMenu.Instance.p2Selector.ShowJoinCode(relayJoinCode);
    }

    public override void OnClientConnect()
    {
        base.OnClientConnect();
        Debug.Log("Connected to host at address "+networkAddress);
        OnlineMenu.singleton.ShowCharSelect();
    }

    public override void OnClientDisconnect()
    {
        PopupManager.instance.ShowBasicPopup("Disconnected", "Disconnected from server");
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