using Mirror;
using UnityEngine;

public class Matchmaker : MonoBehaviour {

    public static string GetRandomMatchId() {
        string id = string.Empty;

        for (int i = 0; i < 5; i++) {
            int random = Random.Range(0, 36);
            if (random < 26) {
                id += (char)random + 'A';
            } else {
                id += (char)random - 26 + '0';
            }
        }

        return id;
    }
}