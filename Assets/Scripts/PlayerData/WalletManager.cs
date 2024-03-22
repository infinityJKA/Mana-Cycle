using LootLocker.Requests;
using UnityEngine;

public class WalletManager {
    public static string walletID {get; private set;}

    public static void GetWallet() {
        LootLockerSDKManager.GetWalletByHolderId(
        PlayerManager.playerID, 
        LootLocker.LootLockerEnums.LootLockerWalletHolderTypes.player,
        (response) => {
            if(!response.success)
            {
                //If wallet is not found, it will automatically create one on the holder.
                Debug.Log("error: " + response.errorData.message);
                Debug.Log("request ID: " + response.errorData.request_id);
                return;
            }

            walletID = response.id;
            GetWalletBalance();
        });
    }

    public static void GetWalletBalance() {
        LootLockerSDKManager.ListBalancesInWallet(walletID, (response) =>
        {
            if(!response.success)
            {
                Debug.Log("error: " + response.errorData.message);
                Debug.Log("request ID: " + response.errorData.request_id);
                return;
            }
            
            if (!SidebarUI.instance) return;

            SidebarUI.instance.SetCoins("0");
            SidebarUI.instance.SetIridium("0");
            foreach (var balance in response.balances) {
                Debug.Log(balance.currency.code+": "+balance.amount);
                if (balance.currency.code == "IBN") {
                    SidebarUI.instance.SetCoins(balance.amount);
                } else if (balance.currency.code == "IDM") {
                    SidebarUI.instance.SetIridium(balance.amount);
                }
            }
        });
    }
}