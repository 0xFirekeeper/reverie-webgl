// Filename: WebLogin.cs
// Author: 0xFirekeeper
// Description: WebGL Login Script

using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_WEBGL
public class WebLogin : MonoBehaviour
{
    [DllImport("__Internal")]
    private static extern void Web3Connect();

    [DllImport("__Internal")]
    private static extern string ConnectAccount();

    [DllImport("__Internal")]
    private static extern void SetConnectAccount(string value);

    private int expirationTime;
    private string account;

    public void OnLogin()
    {
        Web3Connect();
        OnConnected();
    }

    async private void OnConnected()
    {
        account = ConnectAccount();
        while (account == "")
        {
            await new WaitForSeconds(1f);
            account = ConnectAccount();
        };
        // save account for next scene
        PlayerPrefs.SetString("Account", account);
        // reset login message
        SetConnectAccount("");
        // load next scene
        OnLoggedIn();

    }

    public void OnLoggedIn()
    {
        SimpleGameManager.Instance.SetGameState(SimpleGameState.MainGame);
    }

    public void OnSkip()
    {
        // burner account for skipped sign in screen
#if UNITY_EDITOR
        PlayerPrefs.SetString("Account", "0xDaaBDaaC8073A7dAbdC96F6909E8476ab4001B34");
#else
        PlayerPrefs.SetString("Account", "");
#endif
        // move to next scene
        OnLoggedIn();
    }
}
#endif


