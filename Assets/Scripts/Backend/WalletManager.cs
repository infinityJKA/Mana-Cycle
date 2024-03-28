using LootLocker.Requests;
using UnityEngine;

public class WalletManager {
    public static string walletID {get; private set;}
    public static int coins {get; private set;} = 0;
    public static int iridium {get; private set;} = 0;

    public static void GetWallet() {
        if (!PlayerManager.loggedIn) return;

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
            

            foreach (var balance in response.balances) {
                Debug.Log(balance.currency.code+": "+balance.amount);

                if (int.TryParse(balance.amount, out int amount)) {
                    if (balance.currency.code.ToLower() == "ibn") {
                        coins = amount;
                    } else if (balance.currency.code.ToLower() == "idm") {
                        iridium = amount;
                    }
                }
            }

            if (SidebarUI.instance) SidebarUI.instance.UpdateWalletDisplay();;
            
        });
    }
}