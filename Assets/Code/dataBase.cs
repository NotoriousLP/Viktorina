using System;
using Mono.Data.SqliteClient;
using UnityEngine;

public class dataBase : MonoBehaviour
{
    private string dbName = "URI=file:jautajumi.db";
    private Objects objekti;
#if UNITY_EDITOR
    public ImageImporter imageImporter;
#endif

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
                    );";
                command.ExecuteNonQuery();
            }

            connection.Close();
        }
    }
    public void addQuestionBank()
    {
        if (objekti == null || objekti.inputField.Length < 7)
        {
            Debug.LogError("Not enough input fields!");
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
    }

    public void addDataQuestion()
    {
        if (objekti == null || objekti.inputField.Length < 7)
        {
            Debug.LogError("Not enough input fields!");
            return;
        }

        int bankaId = SelectedBank.ID;
        Debug.Log("SelectedBank ID: " + SelectedBank.ID);

        if (bankaId == 0)
        {
            Debug.LogError("Nav izvēlēta banka! BankaId = 0");
            return;
        }
        if (imageImporter == null)
        {
            Debug.LogError("imageImporter nav piesaistīts!");
        }
        else
        {
            Debug.Log("Image file name: " + imageImporter.savedFileName);
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
            #if UNITY_EDITOR
                command.Parameters.Add(new SqliteParameter("@bilde", imageImporter.savedFileName));
            #endif
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
