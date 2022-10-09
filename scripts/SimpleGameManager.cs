using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SimpleGameState
{
    MainMenu,
    MainGame,
    Playing,
    Inventory,
    Shop
}
public class SimpleGameManager : MonoBehaviour
{
    public GameObject pressQText;
    public int Coins { get; private set; }
    public int Level { get; private set; }

    private SimpleGameState gameState;
    private string currentAccount;
    private bool endgame; // can't use coroutine for endgame, avoiding webgl recursion error

    public static SimpleGameManager Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        currentAccount = PlayerPrefs.GetString("Account");
        Coins = PlayerPrefs.GetInt(currentAccount + "_Coins", 0);
        Level = PlayerPrefs.GetInt(currentAccount + "_Level", 0);
        SetGameState(SimpleGameState.MainMenu);

        Camera.main.GetComponentInChildren<Cinemachine.CinemachineBrain>().SetEnable(false);
        GameObject.FindGameObjectWithTag(Tags.PLAYER).GetComponent<MalbersAnimations.MalbersInput>().Enable(false);
    }

    public void SetGameState(SimpleGameState simpleGameState)
    {
        gameState = simpleGameState;

        switch (simpleGameState)
        {
            case (SimpleGameState.MainMenu):
                UIManager.Instance.OpenPanel(PanelNames.MainMenuPanel, true);
                if (CameraManager.Instance != null)
                    CameraManager.Instance.MoveToView(CameraTransforms.View1);
                break;
            case (SimpleGameState.MainGame):
                UIManager.Instance.OpenPanel(PanelNames.MainGamePanel, true);
                if (CameraManager.Instance != null)
                    CameraManager.Instance.MoveToView(CameraTransforms.View2);
                break;
            case (SimpleGameState.Playing):
                if (CameraManager.Instance != null)
                    CameraManager.Instance.DisableCameraManager();
                Camera.main.GetComponentInChildren<Cinemachine.CinemachineBrain>().SetEnable(true);
                GameObject.FindGameObjectWithTag(Tags.PLAYER).GetComponent<MalbersAnimations.MalbersInput>().Enable(true);
                UIManager.Instance.OpenPanel(PanelNames.ExploringPanel);
                break;
            case (SimpleGameState.Inventory):
                Camera.main.GetComponentInChildren<Cinemachine.CinemachineBrain>().SetEnable(false);
                GameObject.FindGameObjectWithTag(Tags.PLAYER).GetComponent<MalbersAnimations.MalbersInput>().Enable(false);
                UIManager.Instance.OpenPanel(PanelNames.InventoryPanel);
                break;
            case (SimpleGameState.Shop):
                Camera.main.GetComponentInChildren<Cinemachine.CinemachineBrain>().SetEnable(false);
                GameObject.FindGameObjectWithTag(Tags.PLAYER).GetComponent<MalbersAnimations.MalbersInput>().Enable(false);
                UIManager.Instance.OpenPanel(PanelNames.ShopPanel);
                break;
        }
    }

    public SimpleGameState GetGameState()
    {
        return gameState;
    }

    public void AddCoins(int amount)
    {
        Coins += amount;
        PlayerPrefs.SetInt(currentAccount + "_Coins", Coins);
    }

    public void OnFlightUnlocked()
    {
        endgame = true;

        GameObject.FindGameObjectWithTag(Tags.PLAYER).GetComponent<MalbersAnimations.Controller.MAnimal>().State_Enable(6);

        pressQText.SetActive(true);

        Time.timeScale = 0.25f;
    }

    private void Update()
    {
        if (endgame && Input.GetKeyDown(KeyCode.Q))
        {
            Time.timeScale = 1f;

            pressQText.SetActive(false);

            endgame = false;
        }
    }

    public void OnExitGame()
    {
        Application.Quit();
    }

}
