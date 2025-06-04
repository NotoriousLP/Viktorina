using System;
using Mono.Data.SqliteClient;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class dataBase : MonoBehaviour
{
    private string dbName = "URI=file:jautajumi.db";
    private Objects objekti;

    public ImageImporter imageImporter;
    private BankLoader bankloader;
    void Start()
    {
        objekti = FindFirstObjectByType<Objects>();
        bankloader = FindFirstObjectByType<BankLoader>();
        imageImporter = FindFirstObjectByType<ImageImporter>();
        createDB();
    
    }

 public void createDB()
{
    using (var connection = new SqliteConnection(dbName))
    {
        connection.Open();

        using (var command = connection.CreateCommand())
        {
            // CREATE tabulas
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS jautajumuBanka (
                    ID INTEGER PRIMARY KEY AUTOINCREMENT,
                    Laiks TEXT, 
                    Jautajums VARCHAR(65), 
                    Bilde TEXT, 
                    Atbilde VARCHAR(25), 
                    OpcijaB VARCHAR(45), 
                    OpcijaC VARCHAR(45), 
                    OpcijaD VARCHAR(45),
                    banka_id INT(3)
                );
                CREATE TABLE IF NOT EXISTS bankasNos (
                    banka_id INTEGER PRIMARY KEY AUTOINCREMENT,
                    nosaukums VARCHAR(45)
                );
                CREATE TABLE IF NOT EXISTS scoreBoard (
                    ID INTEGER PRIMARY KEY AUTOINCREMENT,
                    playerName TEXT,
                    punkti INT,
                    datums TEXT,
                    banka_id INT
                );
                CREATE TABLE IF NOT EXISTS users (
                    user_id INTEGER PRIMARY KEY AUTOINCREMENT,
                    username TEXT UNIQUE NOT NULL,
                    password TEXT NOT NULL,
                    role TEXT NOT NULL
                );";
            command.ExecuteNonQuery();
        }

        // Pārbaudīt vai ir kāds users jau
        using (var checkCmd = connection.CreateCommand())
        {
            checkCmd.CommandText = "SELECT COUNT(*) FROM users";
            long userCount = (long)checkCmd.ExecuteScalar();

            if (userCount == 0)
            {
                // Ja nav neviena usera → izveido default admin
                using (var insertCmd = connection.CreateCommand())
                {
                    insertCmd.CommandText = @"
                        INSERT INTO users (username, password, role)
                        VALUES ('admin', 'admin123', 'admin');";

                    insertCmd.ExecuteNonQuery();

                    Debug.Log("Izveidots noklusētais admin konts (admin / admin123)");
                }
            }
            else
            {
                Debug.Log($"Lietotāji DB jau eksistē ({userCount} lietotāji). Admin konts netika izveidots.");
            }
        }

        connection.Close();
    }
}

    public void addQuestionBank()
    {
        string bankaNosaukums = objekti.inputField[6].text.Trim();

        if (string.IsNullOrEmpty(bankaNosaukums))
        {
            objekti.text[7].gameObject.SetActive(true);

            return;
        }
        
        using (var connection = new SqliteConnection(dbName))
        {
            connection.Open();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    INSERT INTO bankasNos 
                    (nosaukums) 
                    VALUES (@banka_id);";


                command.Parameters.Add(new SqliteParameter("@banka_id", objekti.inputField[6].text));
                command.ExecuteNonQuery();
            }
        }
        for (int i = 0; i < objekti.inputField.Length; i++)
        {
            objekti.inputField[i].text = "";
        }
        bankloader.LoadBanks();
        objekti.objects[1].gameObject.SetActive(false);
    }

    public void addDataQuestion()
{
    if (string.IsNullOrEmpty(objekti.inputField[0].text)
     || string.IsNullOrEmpty(objekti.inputField[1].text)
     || string.IsNullOrEmpty(objekti.inputField[2].text)
     || string.IsNullOrEmpty(objekti.inputField[3].text)    
     || string.IsNullOrEmpty(objekti.inputField[4].text)    
     || string.IsNullOrEmpty(objekti.inputField[5].text)                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   )
    {
        Debug.LogError("Not enough input fields!");
        objekti.text[8].text = "Nav aizpildīti lauki!";
        objekti.text[8].gameObject.SetActive(true);
        return;
    }

    int bankaId = SelectedBank.ID;
    Debug.Log("SelectedBank ID: " + SelectedBank.ID);

    if (bankaId == 0)
    {
        Debug.LogError("Nav izvēlēta banka! BankaId = 0");
        return;
    }

    //Laiks nedrīkst būt mazāks par 5 sekundēm un lielāks par 20 sekundēm
    float timeValue;
    bool isValidTime = float.TryParse(objekti.inputField[5].text, out timeValue);

    if (!isValidTime || timeValue < 5f || timeValue > 20f)
    {
        Debug.LogError("Time must be between 5 and 20 seconds.");
            objekti.text[8].text = "Laiks nav iestatīts starp 5 un 20 sekundēm";
            objekti.text[8].gameObject.SetActive(true);
        return;     
    }


    if (imageImporter == null)
    {
        Debug.LogError("imageImporter nav piesaistīts!");
    }
    else
    {
        Debug.Log("Image file name: " + imageImporter.savedFilePath);
    }

    using (var connection = new SqliteConnection(dbName))
    {
        connection.Open();

        using (var command = connection.CreateCommand())
        {
            command.CommandText = @"
            INSERT INTO jautajumuBanka 
            (Laiks, Jautajums, Bilde, Atbilde, OpcijaB, OpcijaC, OpcijaD, banka_id) 
            VALUES (@laiks, @jautajums, @bilde, @atbilde, @opcB, @opcC, @opcD, @jtb);";

            command.Parameters.Add(new SqliteParameter("@laiks", objekti.inputField[5].text));
            command.Parameters.Add(new SqliteParameter("@jautajums", objekti.inputField[0].text));
            command.Parameters.Add(new SqliteParameter("@bilde", imageImporter.savedFilePath));
            command.Parameters.Add(new SqliteParameter("@atbilde", objekti.inputField[1].text));
            command.Parameters.Add(new SqliteParameter("@opcB", objekti.inputField[2].text));
            command.Parameters.Add(new SqliteParameter("@opcC", objekti.inputField[3].text));
            command.Parameters.Add(new SqliteParameter("@opcD", objekti.inputField[4].text));
            command.Parameters.Add(new SqliteParameter("@jtb", bankaId));

            command.ExecuteNonQuery();
        }
    }

        for (int i = 0; i < objekti.inputField.Length; i++)
        {
            objekti.inputField[i].text = "";
        }

        imageImporter.savedFilePath = "";
        imageImporter.ClearPreview();

        objekti.objects[0].gameObject.SetActive(false);

        Debug.Log("Jautājums pievienots bankai ID: " + bankaId);
}


    public void SavePlayerScore(string playerName, int points)
    {
        int bankaId = SelectedBank.ID;

        using (var connection = new SqliteConnection(dbName))
        {
            connection.Open();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    INSERT INTO scoreBoard (playerName, punkti, datums, banka_id)
                    VALUES (@name, @punkti, @datums, @bankaId);";

                command.Parameters.Add(new SqliteParameter("@name", playerName));
                command.Parameters.Add(new SqliteParameter("@punkti", points));
                command.Parameters.Add(new SqliteParameter("@datums", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
                command.Parameters.Add(new SqliteParameter("@bankaId", bankaId));
                Debug.Log($"Saving score: {playerName}, {points}, banka_id = {bankaId}");
                command.ExecuteNonQuery();
                Debug.Log($"Score saved.");
            }

            connection.Close();
        }

        Debug.Log("Saglabāti punkti priekš " + playerName + ": " + points + " punkti pie bankas ID " + bankaId);
    }

   



}
