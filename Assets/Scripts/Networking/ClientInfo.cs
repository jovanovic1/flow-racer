using UnityEngine;

public static class ClientInfo {
    public static string Username {
        get => PlayerPrefs.GetString("C_Username", string.Empty);
        set => PlayerPrefs.SetString("C_Username", value);
    }

    public static int KartId {
        get => PlayerPrefs.GetInt("C_KartId", -1);
        set => PlayerPrefs.SetInt("C_KartId", value);
    }

    public static string LobbyName {
        get => PlayerPrefs.GetString("C_LastLobbyName", "");
        set => PlayerPrefs.SetString("C_LastLobbyName", value);
    }

    public static string UserWallet {
        get => PlayerPrefs.GetString("walletAddress", "nA");
        set => PlayerPrefs.SetString("walletAddress", value);
    }

    public static float WalletBalance {
        get => PlayerPrefs.GetFloat("walletBalance", (0.0f));
        set => PlayerPrefs.SetFloat("walletBalance", value);
    }
}