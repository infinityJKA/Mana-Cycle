using UnityEngine;
using Mirror;
using VersusMode;

public class NetManager : NetworkManager {
    public override void OnStartHost()
    {
        base.OnStartHost();
        OnlineMenu.singleton.ShowCharSelect();
    }

    public override void OnClientConnect()
    {
        base.OnClientConnect();
        OnlineMenu.singleton.ShowCharSelect();
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnServerAddPlayer(conn);
        conn.identity.GetComponent<NetPlayer>().ConnectToCharSelector();
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