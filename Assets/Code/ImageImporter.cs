#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.UI;

public class ImageImporter : MonoBehaviour
{
    public Image previewImage; // ← UI bilde priekšskatījumam
    public string savedFileName; // ← Saglabātais faila nosaukums

    public void SelectImageFromPC()
    {
        string path = EditorUtility.OpenFilePanel("Izvēlies attēlu", "", "png,jpg,jpeg");

        if (!string.IsNullOrEmpty(path))
        {
            string fileName = Path.GetFileName(path);
            string fileNameWithoutExt = Path.GetFileNameWithoutExtension(path);
            string targetPath = Application.dataPath + "/Resources/Images/" + fileName;

            // Nokopē failu
            File.Copy(path, targetPath, true);
            Debug.Log("Attēls nokopēts uz Resources/Images/: " + fileName);

            savedFileName = fileNameWithoutExt;

            // Atjauno Asset datni (tikai Editorā)
            AssetDatabase.Refresh();

            // Ielādē kā Sprite no Resources
            Sprite sprite = Resources.Load<Sprite>("Images/" + fileNameWithoutExt);
            if (sprite != null && previewImage != null)
            {
                previewImage.sprite = sprite;
            }
            else
            {
                Debug.LogError("Nevar ielādēt sprite no Resources: " + fileNameWithoutExt);
            }
        }
        else
        {
            Debug.Log("Attēls nav izvēlēts.");
        }
    }
}
#endif
