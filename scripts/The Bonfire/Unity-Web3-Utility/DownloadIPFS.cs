// Filename: DownloadIPFS.cs
// Author: 0xFirekeeper
// Description: Download any file hosted on IPFS. 
//              Your IPFS files must follow the following naming convention "0.json" or "0.jpg" unless you tweak the code.
//              Tested with 8888 collection of 4k images - pinata dedicated gateway recommended

using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

// Supported File Types
public enum FileType
{
    JSON,
    JPG,
    PNG,
    TXT
}

public class DownloadIPFS : MonoBehaviour
{
    [Header("Your IPFS file names must be named like 0.json or 0.jpg")]
    public bool UNDERSTOOD = true;

    [Header("Do you want annoying logs?")]
    public bool LOGS = false;

    [Header("What file type are you downloading?")]
    public FileType fileType = FileType.JPG;

    [Header("IPFS Details")]
    public string httpsGateway = "https://nftstorage.link";
    public string cidPath = "bafybeih24njdeygz4m5xaf5kdhb6c7fy6tq7j4uspmucsoiuhpmyaou7uq";

    [Header("First (Included) and Last (Excluded) ID to Check/Download")]
    public int firstId = 0;
    public int lastId = 1000;

    [Header("Avoid getting rate-limtied")]
    public int maxRequestsPerMinute = 100;
    public bool retryIfFail = false;

    [Header("I/O Details")]
    public string localPath = @"D:\Desktop\NFT\";

    // Tracking amount of requests 
    [SerializeField]
    private int currentRequests;
    [SerializeField]
    private bool doNotRequest;
    [SerializeField]
    private float timer;

    // File extensions based on filetype
    private Dictionary<FileType, string> fileExtension = new Dictionary<FileType, string>(){
        {FileType.JSON, ".json"},
        {FileType.JPG, ".jpg"},
        {FileType.PNG, ".png"},
        {FileType.TXT, ".txt"}
    };

    private void Awake()
    {
        currentRequests = 0;
        doNotRequest = false;
        timer = 0f;
    }

    void Start()
    {
        // Create specified folder if it wasn't already
        if (!Directory.Exists(localPath))
            Directory.CreateDirectory(localPath);

        // Queue the magic
        StartCoroutine(DownloadEverything());
    }

    private void Update()
    {
        // Start a timer when requests reach maximum
        if (doNotRequest)
            timer += Time.unscaledDeltaTime;

        // After a minute passes, start the cycle anew
        if (timer > 60f)
        {
            currentRequests = 0;
            doNotRequest = false;
            timer = 0f;
        }
    }

    // Does not download if file exists in local path
    IEnumerator DownloadEverything()
    {
        for (int id = firstId; id < lastId; id++)
        {
            // Wait before blasting not only more requests, but also Coroutines
            while (doNotRequest)
                yield return new WaitForSecondsRealtime(2f);

            // Different process for json and images
            if (File.Exists(localPath + id + ".jpg"))//fileExtension[fileType]))
            {
                if (LOGS)
                    Debug.Log($"File #{id}{fileExtension[fileType]} already exists at given local path, skipping.");

                continue;
            }

            StartCoroutine(DownloadItem(id));
        }
    }

    IEnumerator DownloadItem(int id)
    {
        // When requests reach max per minute, set flag and check in Update()
        if (currentRequests >= maxRequestsPerMinute)
            doNotRequest = true;

        // Wait before blasting more requests
        while (doNotRequest)
            yield return new WaitForSecondsRealtime(1f);

        /// REQUEST SETUP
        currentRequests++;
        string uri = httpsGateway + "/ipfs/" + cidPath + "/" + id + fileExtension[fileType];
        UnityWebRequest webRequest;

        // Request Image
        if (fileType == FileType.JPG || fileType == FileType.PNG)
        {
            // // If using Pinata style dedicated gateway, you can add a custom query parameters
            // uri += "?img-width=2048&img-height=2348";
            webRequest = UnityWebRequestTexture.GetTexture(uri);
        }
        // Request Text
        else if (fileType == FileType.JSON || fileType == FileType.TXT)
        {
            webRequest = UnityWebRequest.Get(uri);
        }
        // Request other types
        else
        {
            Debug.LogWarning("Requesting this type of data has not been implemented yet.");
            yield break;
        }

        if (LOGS)
            Debug.Log($"REQUESTING: #{id}{fileExtension[fileType]} - URI: {uri}");

        yield return webRequest.SendWebRequest();

        /// REQUEST FAIL
        if (webRequest.result != UnityWebRequest.Result.Success)
        {
            // You could check what kind of error it is and add more logic below
            if (LOGS)
                Debug.LogWarning($"REQUEST FAIL: #{id}{fileExtension[fileType]} - Error: {webRequest.error}");

            // Retry if this specific item could not be fetched
            if (retryIfFail)
            {
                if (LOGS)
                    Debug.Log($"RETRYING: #{id}{fileExtension[fileType]}");

                StartCoroutine(DownloadItem(id));
            }

            yield break;
        }
        /// REQUEST SUCCESS
        else
        {
            if (!webRequest.downloadHandler.isDone)
                yield return null;

            // Save Image
            if (fileType == FileType.JPG || fileType == FileType.PNG)
            {
                Texture2D itemTexture = ((DownloadHandlerTexture)webRequest.downloadHandler).texture;

                byte[] byteData;

                if (fileType == FileType.JPG)
                    byteData = itemTexture.EncodeToJPG();
                else
                    byteData = itemTexture.EncodeToPNG();

                File.WriteAllBytes(localPath + id + fileExtension[fileType], byteData);

                Destroy(itemTexture);
            }
            // Save Text
            else if (fileType == FileType.JSON || fileType == FileType.PNG)
            {
                string textData = webRequest.downloadHandler.text;
                File.WriteAllText(localPath + id + fileExtension[fileType], textData);
            }
            // Save other types
            else
            {
                Debug.LogWarning("Saving this type of data has not been implemented yet.");
                yield break;
            }

            if (webRequest != null)
                webRequest.Dispose();

            if (LOGS)
                Debug.Log($"REQUEST SUCCESS: #{id}{fileExtension[fileType]}");
        }
    }
}