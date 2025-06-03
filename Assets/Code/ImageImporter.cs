using UnityEngine;
using System.IO;
using UnityEngine.UI;
using SFB;
using UnityEngine.Networking;

public class ImageImporter : MonoBehaviour
{
    public Image previewImage;
    public string savedFileName;
    public string savedFilePath; // NEW

    public void SelectImageFromPC()
    {
        var paths = StandaloneFileBrowser.OpenFilePanel("Izvēlies attēlu", "", new[] { new ExtensionFilter("Image Files", "png", "jpg", "jpeg") }, false);

        if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
        {
            string path = paths[0];
            string fileName = Path.GetFileName(path);
            string fileNameWithoutExt = Path.GetFileNameWithoutExtension(path);

            // Saglabā uz persistentDataPath/Images
            string imagesFolder = Path.Combine(Application.persistentDataPath, "Images");
            Directory.CreateDirectory(imagesFolder);

            string targetPath = Path.Combine(imagesFolder, fileName);
            File.Copy(path, targetPath, true);

            Debug.Log("Attēls nokopēts uz: " + targetPath);

            savedFileName = fileNameWithoutExt;
            savedFilePath = targetPath; // NEW

            // Ielādē un parāda preview
            StartCoroutine(LoadImageCoroutine("file://" + targetPath));
        }
        else
        {
            Debug.Log("Attēls nav izvēlēts.");
        }
    }

    private System.Collections.IEnumerator LoadImageCoroutine(string filePath)
    {
        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(filePath))
        {
            yield return uwr.SendWebRequest();

            if (uwr.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(uwr);
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

                if (previewImage != null)
                {
                    previewImage.sprite = sprite;
                    previewImage.color = Color.white;
                }
            }
            else
            {
                Debug.LogError("Kļūda ielādējot bildi: " + uwr.error);
            }
        }
    }

    public void ClearPreview()
    {
        if (previewImage != null)
        {
            previewImage.sprite = null;
            previewImage.color = new Color(1, 1, 1, 0);
        }
    }
}
