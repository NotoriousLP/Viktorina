using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mono.Data.SqliteClient;
using System.Collections.Generic;
using System.Data;
using UnityEngine.SceneManagement;

public class BankLoader : MonoBehaviour
{
    public GameObject buttonPrefab;
    public Transform parentPanel; 
    private Objects objekti;
    public EditBankLoader editBankLoader;
    public dataBase database;
    public ImageImporter imageImporter;
    private string dbName = "URI=file:jautajumi.db";

    void Start()
    {
        imageImporter = FindFirstObjectByType<ImageImporter>();
        database = FindFirstObjectByType<dataBase>();
        if (SceneManager.GetActiveScene().name == "CreateQuestions")
        {
            LoadBanks();
        }
        else
        {
            loadGameBanks();
        }
        objekti = FindFirstObjectByType<Objects>(); 
        Debug.Log("buttonPrefab: " + (buttonPrefab == null));
        Debug.Log("parentPanel: " + (parentPanel == null));
    }

    public void LoadBanks()
    {
        // Notīra jau esošās pogas
        foreach (Transform child in parentPanel)
        {
            Destroy(child.gameObject);
        }

        using (var connection = new SqliteConnection(dbName))
        {
            connection.Open();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT banka_id, nosaukums FROM bankasNos";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int id = reader.GetInt32(0);
                        string name = reader.GetString(1);

                        // Poga
                        GameObject btn = Instantiate(buttonPrefab, parentPanel);

                        // TMP_Text
                        btn.transform.GetChild(0).GetComponent<TMP_Text>().text = name;

                        // Galvenā poga
                        btn.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => OnBankSelected(id, name));

                        // Pārbaude un dzēšanas poga
                        var deleteObj = btn.transform.Find("deleteButton");

                        if (deleteObj == null)
                        {
                            Debug.LogError("deleteButton NAV atrasts prefabā!");
                        }
                        else
                        {
                            Debug.Log("deleteButton atrasts OK.");

                            UnityEngine.UI.Button deleteBtn = deleteObj.GetComponent<UnityEngine.UI.Button>();
                            deleteBtn.onClick.AddListener(() => DeleteBank(id));
                        }
                        // EDIT poga
                        var editObj = btn.transform.Find("editButton");
                        if (editObj == null)
                        {
                            Debug.LogError("editButton NAV atrasts prefabā!");
                        }
                        else
                        {
                            Debug.Log("editButton atrasts OK.");

                            UnityEngine.UI.Button editBtn = editObj.GetComponent<UnityEngine.UI.Button>();
                            editBtn.onClick.AddListener(() => EditBank(id));
                        }
                    }
                }
            }
        }
    }


    public void loadGameBanks()
    {
        // Notīrām jau esošās pogas
        foreach (Transform child in parentPanel)
        {
            Destroy(child.gameObject);
        }

        using (var connection = new SqliteConnection(dbName))
        {
            connection.Open();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT banka_id, nosaukums FROM bankasNos";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int id = reader.GetInt32(0);
                        string name = reader.GetString(1);

                        GameObject btn = Instantiate(buttonPrefab, parentPanel);
                        btn.GetComponentInChildren<Text>().text = name;

                        btn.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => OnBankSelected(id, name));
                    }
                }
            }
        }

    }
    public void EditBank(int id)
    {
        Debug.Log($"Edit bankas ID: {id}");

        SelectedBank.ID = id;

        objekti.objects[2].SetActive(true);

        editBankLoader.LoadQuestionsForBank(id);

    }

    
    public void DeleteBank(int bankId)
    {
        Debug.Log("Dzēšam banku ID: " + bankId);

        using (var connection = new SqliteConnection(dbName))
        {
            connection.Open();

            using (var command = connection.CreateCommand())
            {
                //Dzēš jautājumus
                command.CommandText = "DELETE FROM jautajumuBanka WHERE banka_id = @id";
                command.Parameters.Add(new SqliteParameter("@id", bankId));
                command.ExecuteNonQuery();
                command.Parameters.Clear();

                //Dzēš banku
                command.CommandText = "DELETE FROM bankasNos WHERE banka_id = @id";
                command.Parameters.Add(new SqliteParameter("@id", bankId));
                command.ExecuteNonQuery();
            }
        }

        //Atjaunojam sarakstu
        LoadBanks();
    }

    void OnBankSelected(int id, string name)
    {
        SelectedBank.ID = id;
        SelectedBank.Name = name;

        Debug.Log("Izvēlēta banka: " + name);
                if (SceneManager.GetActiveScene().name == "CreateQuestions")
                {
                int bankaId = SelectedBank.ID;
                if (bankaId == 0)
                {
                    Debug.LogError("Nav izvēlēta banka! BankaId = 0");
                    return;
                }
                objekti.objects[0].SetActive(true);

                for (int i = 0; i < objekti.inputField.Length; i++)
                {
                    objekti.inputField[i].text = "";
                }


                imageImporter.savedFilePath= "";

                // Resetē pogu → uz pievienoJautajumu
                objekti.okPoga.onClick.RemoveAllListeners();
                objekti.okPoga.onClick.AddListener(database.addDataQuestion);

                objekti.okPoga.transform.GetChild(0).GetComponent<Text>().text = "Pievienot jautājumu";

                Debug.Log("Atvērts jauna jautājuma logs.");
        }
        else
        {
            SceneManager.LoadScene("GameScene");
        }
    }
}
