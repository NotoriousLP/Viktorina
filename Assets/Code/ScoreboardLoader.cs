using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mono.Data.SqliteClient;
using TMPro;

public class ScoreboardLoader : MonoBehaviour
{
    public Transform contentHolder; 
    public GameObject rowPrefab;

    private string dbName = "URI=file:jautajumi.db";

    void Start()
    {
       
        LoadScoreboard(SelectedBank.ID);
        
    }

    public void LoadScoreboard(int bankaId)
    {
  
        foreach (Transform child in contentHolder)
        {
            Destroy(child.gameObject);
        }

        using (var connection = new SqliteConnection(dbName))
        {
            connection.Open();
            Debug.Log("Banka id: "+ bankaId);
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    SELECT playerName, punkti 
                    FROM scoreBoard 
                    WHERE banka_id = @bankaId 
                    ORDER BY punkti DESC";

                command.Parameters.Add(new SqliteParameter("@bankaId", bankaId));

                Debug.Log("scoreboard: " + command.CommandText + " banka_id = " + bankaId);

                using (var reader = command.ExecuteReader())
                {
                    int rowCount = 0;

                    while (reader.Read())
                    {
                        string player = reader["playerName"].ToString();
                        string points = reader["punkti"].ToString();

                        Debug.Log($"Row {rowCount + 1}: {player} - {points}");

                        GameObject row = Instantiate(rowPrefab, contentHolder);
                        TextMeshProUGUI[] texts = row.GetComponentsInChildren<TextMeshProUGUI>();

                        if (texts.Length >= 2)
                        {
                            texts[0].text = player;
                            texts[1].text = points;
                        }
                        else
                        {
                            Debug.LogWarning("Row prefab nav pietiekami daudz tekstu elementu.");
                        }

                        rowCount++;
                    }

                    if (rowCount == 0)
                    {
                         Debug.Log("Nav rezultātu šai bankai.");

                        
                        using (var fallbackCmd = connection.CreateCommand())
                        {
                            fallbackCmd.CommandText = "SELECT playerName, punkti, banka_id FROM scoreBoard ORDER BY banka_id";
                            using (var fallbackReader = fallbackCmd.ExecuteReader())
                            {
                                int fallbackCount = 0;
                                while (fallbackReader.Read())
                                {
                                    string pname = fallbackReader["playerName"].ToString();
                                    string pscore = fallbackReader["punkti"].ToString();
                                    string pbank = fallbackReader["banka_id"].ToString();

                                    Debug.Log($"[ALL ROWS DEBUG] {pname} - {pscore} (banka_id: {pbank})");
                                    fallbackCount++;
                                }

                                if (fallbackCount == 0)
                                {
                                    Debug.LogWarning("scoreBoard tabula ir tukša.");
                                }
                            }
                        }
                    }
                }
            }

            connection.Close();
        }
    }
}