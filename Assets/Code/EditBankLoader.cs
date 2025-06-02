using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mono.Data.SqliteClient;
using System.Collections.Generic;
using System.Data;
#if UNITY_EDITOR
using UnityEditor.SearchService;
#endif
using UnityEngine.SceneManagement;
public class EditBankLoader : MonoBehaviour
{
    public Transform questionParent; // Kur spawnēt jautājumus
    public GameObject questionRowPrefab; // Prefabs priekš jautājuma rindiņas
    public string dbName = "URI=file:jautajumi.db";
    public Objects objekti;
    public UnityEngine.UI.Button okPoga;

    #if UNITY_EDITOR
    public ImageImporter imageImporter;
#endif

    void Start()
    {
        objekti = FindFirstObjectByType<Objects>(); 
        imageImporter = FindFirstObjectByType<ImageImporter>();
    }

    public void LoadQuestionsForBank(int bankId)
    {
        // Notīra vecos
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

                        // Edit poga
                        var editBtnObj = row.transform.Find("editButton");
                        UnityEngine.UI.Button editBtn = editBtnObj.GetComponent<UnityEngine.UI.Button>();
                        editBtn.onClick.AddListener(() => EditQuestion(questionId));

                        // Delete poga
                        var deleteBtnObj = row.transform.Find("deleteButton");
                        UnityEngine.UI.Button deleteBtn = deleteBtnObj.GetComponent<UnityEngine.UI.Button>();
                        deleteBtn.onClick.AddListener(() => DeleteQuestion(questionId));

                        string imagePath = reader["Bilde"].ToString();
                        var imageObj = row.transform.Find("questionImage").GetComponent<Image>();

                        Sprite loadedSprite = Resources.Load<Sprite>("Images/" + imagePath);
                        if (loadedSprite != null)
                        {
                            imageObj.sprite = loadedSprite;
                        }
                        else
                        {
                            Debug.LogWarning($"Neizdevās ielādēt attēlu: Images/{imagePath}");
                        }
                    }
                }
            }
        }
    }

    public void EditQuestion(int questionId)
    {
        Debug.Log($"Edit question ID: {questionId}");

        SelectedQuestion.ID = questionId;

        objekti.objects[0].SetActive(true);

        // Ielādē datus
        LoadQuestionData(questionId);

        // Pārsien pogu uz SaveEditedQuestion
        okPoga.onClick.RemoveAllListeners();
        okPoga.onClick.AddListener(SaveEditedQuestion);

        // (Var arī nomainīt pogas tekstu ja gribi)
        okPoga.transform.GetChild(0).GetComponent<Text>().text = "Saglabāt izmaiņas";
    }

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
                    // TAVA struktūra:
                    objekti.inputField[0].text = reader["Jautajums"].ToString();
                    objekti.inputField[1].text = reader["Atbilde"].ToString();
                    objekti.inputField[2].text = reader["OpcijaB"].ToString();
                    objekti.inputField[3].text = reader["OpcijaC"].ToString();
                    objekti.inputField[4].text = reader["OpcijaD"].ToString();
                    objekti.inputField[5].text = reader["Laiks"].ToString();

                    string imagePath = reader["Bilde"].ToString();

                    #if UNITY_EDITOR
                    imageImporter.savedFileName = imagePath;
                    #endif
                }
            }
        }
    }
}


public void SaveEditedQuestion()
{
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

            #if UNITY_EDITOR
            command.Parameters.Add(new SqliteParameter("@bilde", imageImporter.savedFileName));
            #endif

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

    // Aizver edit paneli
    objekti.objects[0].SetActive(false);

    // Atjauno sarakstu
    LoadQuestionsForBank(SelectedBank.ID);
}


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

        // Atjauno sarakstu
        LoadQuestionsForBank(SelectedBank.ID);

    }
}
