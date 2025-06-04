using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mono.Data.SqliteClient;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.Networking;

public class EditBankLoader : MonoBehaviour
{
    public Transform questionParent; //Kur pievieno jautājumus
    public GameObject questionRowPrefab; //Prefs priekš jautājuma rindiņas
    public string dbName = "URI=file:jautajumi.db";
    public Objects objekti;
    public ImageImporter imageImporter;

    void Start()
    {
        objekti = FindFirstObjectByType<Objects>(); 
        imageImporter = FindFirstObjectByType<ImageImporter>();
    }

    //Ielādē jautājumus no DB un uzzīmē sarakstu
    public void LoadQuestionsForBank(int bankId)
    {
        //Notīra vecos jautājumus no UI
        foreach (Transform child in questionParent)
        {
            Destroy(child.gameObject);
        }

        using (var connection = new SqliteConnection(dbName))
        {
            connection.Open();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT ID, Jautajums, Bilde FROM jautajumuBanka WHERE banka_id = @id";
                command.Parameters.Add(new SqliteParameter("@id", bankId));

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int questionId = reader.GetInt32(0);
                        string questionText = reader.GetString(1);

                        GameObject row = Instantiate(questionRowPrefab, questionParent);

                        row.transform.GetChild(0).GetComponent<TMP_Text>().text = questionText;

                        //Pievieno Edit pogas onClick
                        var editBtnObj = row.transform.Find("editButton");
                        UnityEngine.UI.Button editBtn = editBtnObj.GetComponent<UnityEngine.UI.Button>();
                        editBtn.onClick.AddListener(() => EditQuestion(questionId));

                        //Pievieno Delete pogas onClick
                        var deleteBtnObj = row.transform.Find("deleteButton");
                        UnityEngine.UI.Button deleteBtn = deleteBtnObj.GetComponent<UnityEngine.UI.Button>();
                        deleteBtn.onClick.AddListener(() => DeleteQuestion(questionId));

                        //Ielādē jautājuma bildi
                        string imagePath = reader["Bilde"].ToString();
                        var imageObj = row.transform.Find("questionImage").GetComponent<Image>();

                        StartCoroutine(LoadImageFromPath(imagePath, imageObj));
                    }
                }
            }
        }
    }

    //Ielādē bildi un parāda jautājumu sarakstā
    private IEnumerator LoadImageFromPath(string path, Image targetImage)
    {
        if (string.IsNullOrEmpty(path))
        {
            Debug.LogWarning("Bilde ceļš ir tukšs.");
            targetImage.sprite = null;
            yield break;
        }

        string finalPath = path;
        if (!path.StartsWith("file://"))
        {
            finalPath = "file://" + path;
        }

        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(finalPath))
        {
            yield return uwr.SendWebRequest();

            if (uwr.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(uwr);
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

                targetImage.sprite = sprite;
                targetImage.color = Color.white;

                Debug.Log($"Bilde ielādēta no: {path}");
            }
        }
    }

    //Parāda rediģēšanas logu konkrētajam jautājumam
    public void EditQuestion(int questionId)
    {
        objekti.text[7].gameObject.SetActive(false);

        Debug.Log($"Edit question ID: {questionId}");

        SelectedQuestion.ID = questionId;

        //Parāda rediģēšanas logu
        objekti.objects[0].SetActive(true);

        //Ielādē jautājuma datus
        LoadQuestionData(questionId);

        //Pievieno Save pogas listeneri
        objekti.okPoga.onClick.RemoveAllListeners();
        objekti.okPoga.onClick.AddListener(SaveEditedQuestion);

        //Maina pogas tekstu
        objekti.okPoga.transform.GetChild(0).GetComponent<Text>().text = "Saglabāt izmaiņas";
    }

    //Ielādē jautājuma datus rediģēšanai
    public void LoadQuestionData(int questionId)
    {
        using (var connection = new SqliteConnection(dbName))
        {
            connection.Open();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT * FROM jautajumuBanka WHERE ID = @id";
                command.Parameters.Add(new SqliteParameter("@id", questionId));

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        objekti.inputField[0].text = reader["Jautajums"].ToString();
                        objekti.inputField[1].text = reader["Atbilde"].ToString();
                        objekti.inputField[2].text = reader["OpcijaB"].ToString();
                        objekti.inputField[3].text = reader["OpcijaC"].ToString();
                        objekti.inputField[4].text = reader["OpcijaD"].ToString();
                        objekti.inputField[5].text = reader["Laiks"].ToString();

                        //Ielādē preview bildi
                        string imagePath = reader["Bilde"].ToString();
                        imageImporter.savedFilePath = imagePath;
                        StartCoroutine(LoadPreviewImage(imagePath));
                    }
                }
            }
        }
    }

    //Ielādē preview bildi priekš rediģēšanas loga
    private IEnumerator LoadPreviewImage(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            Debug.LogWarning("Preview bilde ceļš ir tukšs.");
            imageImporter.previewImage.sprite = null;
            imageImporter.previewImage.color = new Color(1,1,1,0);
            yield break;
        }

        string finalPath = path;
        if (!path.StartsWith("file://"))
        {
            finalPath = "file://" + path;
        }

        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(finalPath))
        {
            yield return uwr.SendWebRequest();

            if (uwr.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(uwr);
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

                imageImporter.previewImage.sprite = sprite;
                imageImporter.previewImage.color = Color.white;

                Debug.Log($"Preview bilde ielādēta no: {path}");
            }
            else
            {
                Debug.LogError($"Neizdevās ielādēt preview bildi no: {path}. Kļūda: {uwr.error}");
                imageImporter.previewImage.sprite = null;
                imageImporter.previewImage.color = new Color(1,1,1,0);
            }
        }
    }

    //Saglabā izmaiņas jautājumā
    public void SaveEditedQuestion()
    {
        //Pārbauda vai obligātie lauki aizpildīti
        if (string.IsNullOrEmpty(objekti.inputField[0].text)
            || string.IsNullOrEmpty(objekti.inputField[1].text)
            || string.IsNullOrEmpty(objekti.inputField[2].text)
            || string.IsNullOrEmpty(objekti.inputField[3].text)    
            || string.IsNullOrEmpty(objekti.inputField[4].text)    
            || string.IsNullOrEmpty(objekti.inputField[5].text))
        {
            Debug.LogError("Not enough input fields!");
            objekti.text[8].text = "Nav aizpildīti lauki!";
            objekti.text[8].gameObject.SetActive(true);
            return;
        }

        Debug.Log($"Saglabājam question ID: {SelectedQuestion.ID}");

        using (var connection = new SqliteConnection(dbName))
        {
            connection.Open();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                UPDATE jautajumuBanka 
                SET 
                    Jautajums = @jautajums,
                    Atbilde = @atbilde,
                    OpcijaB = @opcB,
                    OpcijaC = @opcC,
                    OpcijaD = @opcD,
                    Laiks = @laiks,
                    Bilde = @bilde
                WHERE ID = @id";

                command.Parameters.Add(new SqliteParameter("@jautajums", objekti.inputField[0].text));
                command.Parameters.Add(new SqliteParameter("@atbilde", objekti.inputField[1].text));
                command.Parameters.Add(new SqliteParameter("@opcB", objekti.inputField[2].text));
                command.Parameters.Add(new SqliteParameter("@opcC", objekti.inputField[3].text));
                command.Parameters.Add(new SqliteParameter("@opcD", objekti.inputField[4].text));
                command.Parameters.Add(new SqliteParameter("@laiks", objekti.inputField[5].text));
                command.Parameters.Add(new SqliteParameter("@bilde", imageImporter.savedFilePath));
                command.Parameters.Add(new SqliteParameter("@id", SelectedQuestion.ID));

                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    Debug.Log($"Jautājums ID {SelectedQuestion.ID} veiksmīgi saglabāts.");
                }
                else
                {
                    Debug.LogWarning($"Neizdevās saglabāt jautājumu ID {SelectedQuestion.ID}.");
                }
            }
        }

        //Aizver edit paneli
        objekti.objects[0].SetActive(false);

        //Atjauno jautājumu sarakstu
        LoadQuestionsForBank(SelectedBank.ID);
    }

    //Dzēš jautājumu no datubāzes
    public void DeleteQuestion(int questionId)
    {
        Debug.Log($"Delete question ID: {questionId}");

        using (var connection = new SqliteConnection(dbName))
        {
            connection.Open();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = "DELETE FROM jautajumuBanka WHERE ID = @id";
                command.Parameters.Add(new SqliteParameter("@id", questionId));

                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    Debug.Log($"Jautājums ID {questionId} veiksmīgi izdzēsts.");
                }
                else
                {
                    Debug.LogWarning($"Neizdevās izdzēst jautājumu ID {questionId} (varbūt tāda nav?).");
                }
            }
        }

        //Atjauno jautājumu sarakstu
        LoadQuestionsForBank(SelectedBank.ID);
    }
}
