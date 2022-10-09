// Filename: WebLogout.cs
// Author: 0xFirekeeper
// Description: WebGL Logout Script

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_WEBGL
public class WebLogout : MonoBehaviour
{
    public void OnSignOut()
    {
        // Clear Account
        PlayerPrefs.SetString("Account", "0x0000000000000000000000000000000000000001");
        // go to login scene
        SceneManager.LoadScene(0);
    }
}
#endif