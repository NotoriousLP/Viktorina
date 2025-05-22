using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mono.Data.SqliteClient;
using System.Collections.Generic;
using System.Data;

public class BankLoader : MonoBehaviour
{
    public GameObject buttonPrefab;
    public Transform parentPanel; // <- Pārliecinies, ka tas ir ScrollView Content vai kāds `VerticalLayoutGroup`
    private Objects objekti;

    private string dbName = "URI=file:jautajumi.db";

    void Start()
    {
        LoadBanks();
        objekti = FindFirstObjectByType<Objects>(); 
        Debug.Log("buttonPrefab: " + (buttonPrefab == null));
        Debug.Log("parentPanel: " + (parentPanel == null));
    }

    public void LoadBanks()
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

    void OnBankSelected(int id, string name)
    {
        SelectedBank.ID = id;
        SelectedBank.Name = name;

        Debug.Log("Izvēlēta banka: " + name);
        objekti.objects[0].SetActive(true);
    }
}
