// Filename: ImageLoader.cs
// Author: 0xFirekeeper
// Description: Can Load Local Images and Download Online Images into RawImage components

using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.IO;

public class ImageLoader : MonoBehaviour
{
    public static ImageLoader Instance;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(Instance);
    }

    public void LoadCachedImage(RawImage imageToLoadInto, string persistentImagePath)
    {
        // persistentImagePath should have a format such as " /Images/whale
        byte[] bytes = File.ReadAllBytes(Application.persistentDataPath + persistentImagePath + ".png");

        Texture2D newTexture2D = new Texture2D(
            imageToLoadInto.texture.width,
            imageToLoadInto.texture.height,
            TextureFormat.RGBA32, false
        );

        newTexture2D.LoadImage(bytes);

        imageToLoadInto.texture = newTexture2D;
    }

    // Download Image Into Raw Image
    public void DownloadImage(RawImage imageToLoadInto, string imageURL, bool saveLocally = false, string imageName = "", string persistentFolderPath = "")
    {
        StartCoroutine(Instance.DownloadImageCoroutine(imageToLoadInto, imageURL, saveLocally, imageName, persistentFolderPath));
    }

    // Download Image Into Raw Image Coroutine
    IEnumerator DownloadImageCoroutine(RawImage imageToLoadInto, string imageURL, bool saveLocally = false, string imageName = "", string persistentFolderPath = "")
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(imageURL);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log(request.error);
            // Populate with dummy image
        }
        else
        {
            // Load into raw image
            var downloadedTexture = DownloadHandlerTexture.GetContent(request);
            imageToLoadInto.texture = downloadedTexture;

            // Save Locally
            if (!saveLocally)
                yield return null;

            var bytes = downloadedTexture.EncodeToPNG();
            // persistentFolderPath should have a format such as "/Images/"
            string filePath = Application.persistentDataPath + persistentFolderPath + imageName + ".png";
            FileInfo file = new System.IO.FileInfo(filePath);
            file.Directory.Create(); // Make sure directory is there
            File.WriteAllBytes(filePath, bytes);
        }
    }
}