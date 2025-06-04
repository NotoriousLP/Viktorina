using UnityEngine;
using System.IO;
using UnityEngine.UI;
using SFB;
using UnityEngine.Networking;

public class ImageImporter : MonoBehaviour
{
    public Image previewImage;       //UI komponents kurā parādīt preview
    public string savedFileName;     //Faila nosaukums bez paplašinājuma
    public string savedFilePath;     //Pilns path nosaukums uz saglabāto attēlu

    // Atver failu pārlūku un ļauj izvēlēties attēlu no datora
    public void SelectImageFromPC()
    {
        //Atver logu ar formātiem (png, jpg, jpeg) 
        var paths = StandaloneFileBrowser.OpenFilePanel("Izvēlies attēlu", "", new[] { new ExtensionFilter("Image Files", "png", "jpg", "jpeg") }, false);

        //Ja lietotājs izvēlējās attēlu
        if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
        {
            string path = paths[0];
            string fileName = Path.GetFileName(path);
            string fileNameWithoutExt = Path.GetFileNameWithoutExtension(path);

            //Izveido mapi persistentDataPath/Images (ja vēl nav)
            string imagesFolder = Path.Combine(Application.persistentDataPath, "Images");
            Directory.CreateDirectory(imagesFolder);

            //Uz kurieni kopēt attēlu
            string targetPath = Path.Combine(imagesFolder, fileName);
            File.Copy(path, targetPath, true);

            Debug.Log("Attēls nokopēts uz: " + targetPath);

            //Saglabā vietas priekš spēles
            savedFileName = fileNameWithoutExt;
            savedFilePath = targetPath;

            //Ielādē attēlu previewImage komponentā
            StartCoroutine(LoadImageCoroutine("file://" + targetPath));
        }
        else
        {
            Debug.Log("Attēls nav izvēlēts.");
        }
    }

    //Ielādē attēlu no faila un parāda previewImage komponentā
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

    //Notīra preview Image (piemēram, kad atver jaunu jautājumu)
    public void ClearPreview()
    {
        if (previewImage != null)
        {
            previewImage.sprite = null;
            previewImage.color = new Color(1, 1, 1, 0);
        }
    }
}
