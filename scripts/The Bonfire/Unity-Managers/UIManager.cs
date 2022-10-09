// Filename: UIManager.cs
// Author: 0xFirekeeper
// Description: UI Manager to Activate and Deactivate Panels with a serialized dictionary setup and an optional callback method.

using UnityEngine;
using UnityEngine.Events;
using RotaryHeart.Lib.SerializableDictionary;
using UnityEngine.UI;
using System;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using System.Numerics;
using System.Threading.Tasks;
using UnityEngine.Networking;
using System.Linq;
using System.Collections.Specialized;

public enum PanelNames
{
    MainMenuPanel,
    MainGamePanel,
    ExploringPanel,
    InventoryPanel,
    SellPanel,
    ShopPanel,
    BuyPanel
}

[System.Serializable]
public class UIPanels : SerializableDictionaryBase<PanelNames, UIPanelAndSetup> { }

[System.Serializable]
public class UIPanelAndSetup
{
    public GameObject UIPanel;
    public UnityEvent UIPanelSetup;
}

public class UIManager : MonoBehaviour
{
    public UIPanels UIPanelsDictionary;

    [Header("GENERAL ITEMS")]
    public GameObject LoadingScreen;
    public Color selectedItemColor;
    public Color deselectedItemColor;
    public GameObject[] hideWhenGuest;

    [Header("GAME CANVAS ITEMS")]
    public TMP_Text coins;
    public TMP_Text level;
    public TMP_Text address;

    [Header("INVENTORY ITEMS")]
    public Transform inventoryContent;
    public GameObject inventoryItemPrefab;
    public Button sellButton;

    [Header("SHOP ITEMS")]
    public Transform shopContent;
    public GameObject shopItemPrefab;
    public Button buyButton;

    [Header("SELL ITEMS")]
    public GameObject approveButton;
    public GameObject listButton;
    public GameObject itemBeingSold;
    public TMP_InputField priceInputField;

    [Header("BUY ITEMS")]
    public GameObject confirmButton;
    public GameObject itemBeingBought;

    // Inventory stuff
    int selectedInventoryItem;
    private List<KeyValuePair<NFTMetadata, GameObject>> inventoryMetadataToGameObject;
    private bool allERC1155Approved = false;

    // Shop stuff
    int selectedShopItem;
    private List<KeyValuePair<NFTMetadata, GameObject>> shopMetadataToGameObject;

    public static UIManager Instance;


    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (PlayerPrefs.GetString("Account") == "")
        {
            foreach (var obj in hideWhenGuest)
                obj.SetActive(false);
        }
    }

    void LateUpdate()
    {
        coins.text = SimpleGameManager.Instance.Coins.ToString();
    }

    /// PANEL ONOPENED ///

    public void OnMainMenuPanelOpened()
    {
        Debug.Log("Setting Up Main Menu Panel");
    }

    public void OnMainGamePanelOpened()
    {
        Debug.Log("Setting Up Main Game Panel");

        coins.text = SimpleGameManager.Instance.Coins.ToString();
        level.text = "Reverie - Level " + SimpleGameManager.Instance.Level.ToString();
        address.text = PlayerPrefs.GetString("Account");
    }

    public void OnExploringPanelOpened()
    {
        Debug.Log("Setting Up Exploring Panel");
    }

    #region Inventory-Logic

    public void OnInventoryPanelOpened()
    {
        Debug.Log("Setting up Inventory Panel");

        // Private variables we'll need
        selectedInventoryItem = 0;
        inventoryMetadataToGameObject = new List<KeyValuePair<NFTMetadata, GameObject>>();

        // Clean up the grid
        foreach (Transform item in inventoryContent)
            Destroy(item.gameObject);

        // Add in the default item
        GameObject defaultItem = Instantiate(inventoryItemPrefab, inventoryContent);
        // Add button selector onClick
        defaultItem.GetComponentInChildren<Button>().onClick.AddListener(delegate
                {
                    UIManager.Instance.SelectInventoryItem(0);
                });
        // Hide default item supply
        defaultItem.transform.Find("Text_ItemAmount").GetComponent<TMP_Text>().text = "";

        // Keep a reference
        inventoryMetadataToGameObject.Add(new KeyValuePair<NFTMetadata, GameObject>(new NFTMetadata(), defaultItem));


        // Select the last item selected this session
        SelectInventoryItem(selectedInventoryItem);

        // Populate the rest of the items in a coroutine
        StartCoroutine(PopulateInventory());
    }

    private void SelectInventoryItem(int id)
    {
        print("Selected item number " + id);
        // Deselect currently selected item
        inventoryMetadataToGameObject[selectedInventoryItem].Value.transform.Find("Image_Border").GetComponentInChildren<Image>().color = deselectedItemColor;

        // Select new item
        inventoryMetadataToGameObject[id].Value.transform.Find("Image_Border").GetComponentInChildren<Image>().color = selectedItemColor;

        selectedInventoryItem = id;

        // Disable sell button if first item
        sellButton.interactable = selectedInventoryItem != 0;

    }

    IEnumerator PopulateInventory()
    {
        Debug.Log("Checking user NFTs");

        string[] tokenIds = {
            "0x01559ae4021a5135b9a531fedb929e29680a566ece7dbc403b421d9e84fffe07", // yellow
            "0x01559ae4021a30d853ba18f22430b3812b43f8572dcff100600977e6f2137603", // brown
            "0x01559ae4021a93202e17783f9d495bb5af7fecc96321758017b87eb5fa7453df", // green
            "0x01559ae4021a5623605fcfc59b1c1c94bdaca33586c5fe44db1d348aa489db43" // black
        };

        List<Task<BigInteger>> allTasks = new List<Task<BigInteger>>();

        for (int i = 0; i < tokenIds.Length; i++)
            allTasks.Add(TransactionManager.Instance.ERC1155_GetBalanceOf(Transaction.ERC1155_GetBalanceOf, Contract.ReverieCollection, tokenIds[i]));

        List<BigInteger> balances = new List<BigInteger>();

        // Fetching user balances
        for (int i = 0; i < tokenIds.Length; i++)
        {
            yield return new WaitUntil(() => allTasks[i].IsCompleted);

            if (!allTasks[i].IsCompletedSuccessfully)
            {
                Debug.LogWarning($"Could not fetch balance of token #{i}, aborting.");
                UIManager.Instance.OnResumeButtonClicked(); // Close inventory so they can continue playing
                yield break;
            }
            else
            {
                balances.Add(allTasks[i].Result);
            }
        }

        Debug.Log("Populating Inventory with user NFTs");

        // Create the inventory items
        for (int i = 0; i < balances.Count; i++)
        {
            Debug.Log($"Balance of {i}: {balances[i]} (Token ID: {tokenIds[i]}");
            if (balances[i] > 0)
            {
                StartCoroutine(DownloadInventoryNFT(tokenIds[i], balances[i]));
            }
        }


        yield return null;
    }

    IEnumerator DownloadInventoryNFT(string tokenId, BigInteger balanceOwned)
    {
        Task<string> metadataTask = TransactionManager.Instance.ERC1155_GetURI(Transaction.ERC1155_GetURI, Contract.ReverieCollection, tokenId);
        yield return new WaitUntil(() => metadataTask.IsCompleted);
        if (!metadataTask.IsCompletedSuccessfully)
        {
            Debug.LogWarning($"Could not fetch metadata of token id {tokenId}, aborting.");
            yield break;
        }
        else
        {
            string uri = metadataTask.Result;

            // Get Json Image Data from URI
            UnityWebRequest webRequest = UnityWebRequest.Get(uri);
            yield return webRequest.SendWebRequest();
            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning(webRequest.error);
                yield break;
            }
            NFTMetadata tempNFTMetadata = JsonUtility.FromJson<NFTMetadata>(System.Text.Encoding.UTF8.GetString(webRequest.downloadHandler.data));
            tempNFTMetadata.tokenId = tokenId;
            tempNFTMetadata.userBalance = (int)balanceOwned;

            // Get Image URI from Json Image Data 
            string imageUri = tempNFTMetadata.image;
            if (imageUri.StartsWith("ipfs://"))
                imageUri = imageUri.Replace("ipfs://", "https://blue-decisive-dog-306.mypinata.cloud/ipfs/");

            // Get Texture from Image URI
            UnityWebRequest webRequest1 = UnityWebRequestTexture.GetTexture(imageUri);
            yield return webRequest1.SendWebRequest();
            if (webRequest1.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning(webRequest1.error);
                yield break;
            }
            else
            {
                // Load Texture and Create Sprite
                Texture2D myTexture2D = ((DownloadHandlerTexture)webRequest1.downloadHandler).texture;
                Sprite mySprite = Sprite.Create(myTexture2D, new Rect(0.0f, 0.0f, myTexture2D.width, myTexture2D.height), new UnityEngine.Vector2(0.5f, 0.5f), 100.0f);

                // Create GameObject and Set Image
                GameObject tempNFTObject = Instantiate(inventoryItemPrefab, inventoryContent);
                tempNFTObject.GetComponent<Image>().sprite = mySprite;

                // Load Metadata and Set Text
                string metadataName = tempNFTMetadata.name;
                metadataName = metadataName.Replace("Reverie - ", ""); // remove title from on-chain name string
                tempNFTObject.transform.Find("Text_ItemName").GetComponent<TMP_Text>().text = metadataName;
                tempNFTObject.transform.Find("Text_ItemAmount").GetComponent<TMP_Text>().text = tempNFTMetadata.userBalance.ToString();
                // Get grid sibling index
                int gridIndex = tempNFTObject.transform.GetSiblingIndex();
                // Add onClick select function listener to index in grid
                tempNFTObject.GetComponentInChildren<Button>().onClick.AddListener(delegate
                {
                    UIManager.Instance.SelectInventoryItem(gridIndex);
                });
                // Add reference in grid list
                inventoryMetadataToGameObject.Add(new KeyValuePair<NFTMetadata, GameObject>(tempNFTMetadata, tempNFTObject));
            }

            print("Successfully Loaded Image From: " + imageUri);
        }
    }


    public void OnSellPanelOpened()
    {
        Debug.Log("Setting Up Sell Panel");

        approveButton.SetActive(!allERC1155Approved);
        listButton.SetActive(allERC1155Approved);

        KeyValuePair<NFTMetadata, GameObject> selectedReference = inventoryMetadataToGameObject[selectedInventoryItem];
        // Set Image
        itemBeingSold.GetComponent<Image>().sprite = selectedReference.Value.GetComponent<Image>().sprite;
        // Set name
        itemBeingSold.transform.Find("Text_ItemName").GetComponent<TMP_Text>().text = selectedReference.Value
                     .transform.Find("Text_ItemName").GetComponent<TMP_Text>().text;
        // Sell 1 at a time        
        itemBeingSold.transform.Find("Text_ItemAmount").GetComponent<TMP_Text>().text = "1";
    }

    #endregion

    #region Shop-Logic

    public void OnShopPanelOpened()
    {
        Debug.Log("Setting up Shop Panel");

        // Private variables we'll need
        selectedShopItem = 0;
        shopMetadataToGameObject = new List<KeyValuePair<NFTMetadata, GameObject>>();

        // Clean up the grid
        foreach (Transform item in shopContent)
            Destroy(item.gameObject);

        // Populate the rest of the items in a coroutine
        StartCoroutine(PopulateShop());
    }

    private void SelectShopItem(int id)
    {
        print("Selected item number " + id);
        // Deselect currently selected item
        shopMetadataToGameObject[selectedShopItem].Value.transform.Find("Image_Border").GetComponentInChildren<Image>().color = deselectedItemColor;

        // Select new item
        shopMetadataToGameObject[id].Value.transform.Find("Image_Border").GetComponentInChildren<Image>().color = selectedItemColor;

        selectedShopItem = id;
    }

    IEnumerator PopulateShop()
    {
        Debug.Log("Checking user NFTs");

        string[] tokenIds = {
            "0x01559ae4021a5135b9a531fedb929e29680a566ece7dbc403b421d9e84fffe07", // yellow
            "0x01559ae4021a30d853ba18f22430b3812b43f8572dcff100600977e6f2137603", // brown
            "0x01559ae4021a93202e17783f9d495bb5af7fecc96321758017b87eb5fa7453df", // green
            "0x01559ae4021a5623605fcfc59b1c1c94bdaca33586c5fe44db1d348aa489db43" // black
        };

        Task<List<Models.GetNftListModel.Response>> task = TransactionManager.Instance.ChainSafeMarketplace_GetNfts(Transaction.ChainSafeMarketplace_GetNfts, Contract.ChainSafeMarketplace);

        yield return new WaitUntil(() => task.IsCompleted);

        if (!task.IsCompletedSuccessfully)
        {
            Debug.LogWarning("TASK FAILED: Could not populate shop.");
            yield break;
        }

        List<Models.GetNftListModel.Response> relevantNfts = new List<Models.GetNftListModel.Response>();

        foreach (var marketplaceItem in task.Result)
        {
            if (tokenIds.Any(x => x == marketplaceItem.tokenId))
                relevantNfts.Add(marketplaceItem);
        }

        // Create the shop items
        for (int i = 0; i < relevantNfts.Count; i++)
        {
            StartCoroutine(DownloadShopNFT(relevantNfts[i]));
        }

        yield return null;
    }

    IEnumerator DownloadShopNFT(Models.GetNftListModel.Response shopNFT)
    {
        string uri = shopNFT.uri;


        if (uri.StartsWith("ipfs://"))
        {
            uri = uri.Replace("ipfs://", "https://blue-decisive-dog-306.mypinata.cloud/ipfs/");
            Debug.Log("Response URI" + uri);
        }

        // Get Json Image Data from URI
        UnityWebRequest webRequest = UnityWebRequest.Get(uri);
        yield return webRequest.SendWebRequest();
        if (webRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.LogWarning(webRequest.error);
            yield break;
        }
        NFTMetadata tempNFTMetadata = JsonUtility.FromJson<NFTMetadata>(System.Text.Encoding.UTF8.GetString(webRequest.downloadHandler.data));
        tempNFTMetadata.tokenId = shopNFT.tokenId;
        tempNFTMetadata.price = shopNFT.price;
        tempNFTMetadata.itemId = shopNFT.itemId;

        // Get Image URI from Json Image Data 
        string imageUri = tempNFTMetadata.image;
        if (imageUri.StartsWith("ipfs://"))
            imageUri = imageUri.Replace("ipfs://", "https://blue-decisive-dog-306.mypinata.cloud/ipfs/");

        // Get Texture from Image URI
        UnityWebRequest webRequest1 = UnityWebRequestTexture.GetTexture(imageUri);
        yield return webRequest1.SendWebRequest();
        if (webRequest1.result != UnityWebRequest.Result.Success)
        {
            Debug.LogWarning(webRequest1.error);
            yield break;
        }
        else
        {
            // Load Texture and Create Sprite
            Texture2D myTexture2D = ((DownloadHandlerTexture)webRequest1.downloadHandler).texture;
            Sprite mySprite = Sprite.Create(myTexture2D, new Rect(0.0f, 0.0f, myTexture2D.width, myTexture2D.height), new UnityEngine.Vector2(0.5f, 0.5f), 100.0f);

            // Create GameObject and Set Image
            GameObject tempNFTObject = Instantiate(shopItemPrefab, shopContent);
            tempNFTObject.GetComponent<Image>().sprite = mySprite;

            // Load Metadata and Set Text
            string metadataName = tempNFTMetadata.name;
            metadataName = metadataName.Replace("Reverie - ", ""); // remove title from on-chain name string
            string metadataPrice = tempNFTMetadata.price;
            metadataPrice = (float.Parse(metadataPrice) / 1000000000000000000).ToString("0.####") + " ETH";
            tempNFTObject.transform.Find("Text_ItemName").GetComponent<TMP_Text>().text = metadataName;
            tempNFTObject.transform.Find("Text_ItemAmount").GetComponent<TMP_Text>().text = metadataPrice; // ETH Price
            // Get grid sibling index
            int gridIndex = tempNFTObject.transform.GetSiblingIndex();

            // Add onClick select function listener to index in grid
            tempNFTObject.GetComponentInChildren<Button>().onClick.AddListener(delegate
            {
                UIManager.Instance.SelectShopItem(gridIndex);
            });

            // Add reference in grid list
            shopMetadataToGameObject.Add(new KeyValuePair<NFTMetadata, GameObject>(tempNFTMetadata, tempNFTObject));

            // Select first item
            if (gridIndex == 0)
                UIManager.Instance.SelectShopItem(gridIndex);
        }

        print("Successfully Loaded Image From: " + imageUri);
    }

    public void OnBuyPanelOpened()
    {
        Debug.Log("Setting Up Buy Panel");

        KeyValuePair<NFTMetadata, GameObject> selectedReference = shopMetadataToGameObject[selectedShopItem];
        // Set Image
        itemBeingBought.GetComponent<Image>().sprite = selectedReference.Value.GetComponent<Image>().sprite;
        // Set name
        itemBeingBought.transform.Find("Text_ItemName").GetComponent<TMP_Text>().text = selectedReference.Value
                     .transform.Find("Text_ItemName").GetComponent<TMP_Text>().text;
        // Buy 1 at a time        
        itemBeingBought.transform.Find("Text_ItemAmount").GetComponent<TMP_Text>().text = selectedReference.Value
                     .transform.Find("Text_ItemAmount").GetComponent<TMP_Text>().text; ;
    }

    #endregion

    /// BUTTON ONCLICKS ///

    public void OnExploreClicked()
    {
        Debug.Log("OnExploreClicked");

        SimpleGameManager.Instance.SetGameState(SimpleGameState.Playing);
    }

    public void OnInventoryButtonClicked()
    {
        Debug.Log("OnInventoryButtonClicked");

        SimpleGameManager.Instance.SetGameState(SimpleGameState.Inventory);
    }

    public void OnShopButtonClicked()
    {
        Debug.Log("OnShopButtonClicked");

        SimpleGameManager.Instance.SetGameState(SimpleGameState.Shop);
    }

    public void OnResumeButtonClicked()
    {
        Debug.Log("OnInventoryResumeButtonClicked");

        StopAllCoroutines();
        ClosePanel(PanelNames.InventoryPanel);
        ClosePanel(PanelNames.ShopPanel);
        SimpleGameManager.Instance.SetGameState(SimpleGameState.Playing);
    }

    public void OnEquipButtonClicked()
    {
        Debug.Log("OnEquipButtonClicked");
        // Check type
        string type = inventoryMetadataToGameObject[selectedInventoryItem]
                .Key // Metadata class
                .attributes // Attribute list
                .First(x => x.trait_type == "Type").value;

        string identifier;

        switch (type)
        {
            case ("Skin"):
                identifier = inventoryMetadataToGameObject[selectedInventoryItem]
                        .Key // Metadata class
                        .attributes // Attribute list
                        .First(x => x.trait_type == "Identifier").value;
                MaterialsManager.Instance.SetDragonSkin(identifier);
                break;
            default:
                Debug.LogWarning("Unequippable");
                break;
        }

    }

    public void OnSellButtonClicked()
    {
        Debug.Log("OnSellButtonClicked");

        OpenPanel(PanelNames.SellPanel);
    }

    public void OnBuyButtonClicked()
    {
        Debug.Log("OnBuyButtonClicked");

        OpenPanel(PanelNames.BuyPanel);
    }

    public void OnApproveButtonClicked()
    {
        Debug.Log("OnApproveButtonClicked");

        StartCoroutine(ApproveAllERC1155());
    }

    IEnumerator ApproveAllERC1155()
    {
        TransactionManager.Instance.ChainSafeMarketplace_ApproveAll(Transaction.ChainSafeMarketplace_ApproveAll, Contract.ChainSafeMarketplace).ConfigureAwait(false);
        LoadingScreen.SetActive(true);
        yield return new WaitForSecondsRealtime(20f);
        LoadingScreen.SetActive(false);

        approveButton.SetActive(false);
        listButton.SetActive(true);
        allERC1155Approved = true;
    }

    public void OnListButtonClicked()
    {
        Debug.Log("OnApproveButtonClicked");

        float eth = float.Parse(priceInputField.text);

        Debug.Log("Listing for " + eth + " eth");

        StartCoroutine(ListERC1155(eth));
    }

    IEnumerator ListERC1155(float eth)
    {
        TransactionManager.Instance.ChainSafeMarketplace_List(Transaction.ChainSafeMarketplace_List, Contract.ChainSafeMarketplace, inventoryMetadataToGameObject[selectedInventoryItem].Key.tokenId, eth).ConfigureAwait(false);
        LoadingScreen.SetActive(true);
        yield return new WaitForSecondsRealtime(20f);
        LoadingScreen.SetActive(false);
        ClosePanel(PanelNames.SellPanel);
        OnInventoryPanelOpened();
    }

    public void OnConfirmButtonClicked()
    {
        Debug.Log("OnConfirmButtonClicked");

        StartCoroutine(ConfirmERC1155());
    }

    IEnumerator ConfirmERC1155()
    {
        NFTMetadata selectedMetadata = shopMetadataToGameObject[selectedShopItem].Key;
        TransactionManager.Instance.ChainSafeMarketplace_BuyNft(Transaction.ChainSafeMarketplace_BuyNft, Contract.ChainSafeMarketplace, selectedMetadata.price, selectedMetadata.itemId).ConfigureAwait(false);
        LoadingScreen.SetActive(true);
        yield return new WaitForSecondsRealtime(8f);
        LoadingScreen.SetActive(false);
        ClosePanel(PanelNames.BuyPanel);
        OnShopPanelOpened();
        yield return null;
    }

    /// PANEL LOGIC ///

    public void OpenPanel(string panel)
    {
        PanelNames panelName;
        if (Enum.TryParse<PanelNames>(panel, out panelName))
            OpenPanel(panelName);
        else
            Debug.LogWarning("Did not find panel: " + panel);
    }

    public void OpenPanel(PanelNames panelName, bool closeOtherPanels = false)
    {
        UIPanelAndSetup panelToOpen;
        if (UIPanelsDictionary.TryGetValue(panelName, out panelToOpen))
        {
            if (closeOtherPanels)
                CloseAllPanels();

            panelToOpen.UIPanel.SetActive(true);
            panelToOpen.UIPanelSetup?.Invoke();
        }
        else
        {
            Debug.LogWarning("No value for key: " + panelName + " exists");
        }

    }

    public void ClosePanel(string panel)
    {
        PanelNames panelName;
        if (Enum.TryParse<PanelNames>(panel, out panelName))
            ClosePanel(panelName);
        else
            Debug.LogWarning("Did not find panel: " + panel);
    }

    public void ClosePanel(PanelNames panelName)
    {
        UIPanelAndSetup currentPanel;
        if (UIPanelsDictionary.TryGetValue(panelName, out currentPanel))
        {
            currentPanel.UIPanel.SetActive(false);
            Debug.Log(panelName + " closed");
        }
    }

    void CloseAllPanels()
    {
        foreach (PanelNames panelName in UIPanelsDictionary.Keys)
            ClosePanel(panelName);
    }

}



