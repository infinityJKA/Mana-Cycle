using UnityEngine;
using Mirror;
using Networking;
using VersusMode;
using UnityEngine.SceneManagement;

public class NetManager : NetworkManager {
    // serialize to tell if this is steam manager or a non steam one like webGL or local test
    public bool useSteam = true;

    public override void OnStartHost()
    {
        base.OnStartHost();
        Debug.Log("Hosting at address "+singleton.networkAddress);

        OnlineMenu.singleton.ShowCharSelect();
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

    public override void OnStopHost()
    {
        base.OnStopHost();
        BackToOnlineMenu();
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
        PopupManager.instance.ShowBasicPopup("Disconnected", "You have been disconnected",
            onConfirm: BackToOnlineMenu);
    }


    private void BackToOnlineMenu()
    {
        if (SceneManager.GetActiveScene().name == "CharSelect") {
            OnlineMenu.singleton.ShowOnlineMenu();
        } else {
            TransitionScript.instance.onTransitionOut = OnlineMenu.singleton.ShowOnlineMenu;
            TransitionScript.instance.WipeToScene("CharSelect", reverse: true);
        }
    }

    public static bool IsUseSteam() {
        // will return false if there is not a netmanager on the networkmanager or the useSteam val is false
        NetManager netManager = singleton.GetComponent<NetManager>();
        return netManager && netManager.useSteam;
    }
}