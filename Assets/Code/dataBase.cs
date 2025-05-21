using System;
using Mono.Data.SqliteClient;
using UnityEngine;

public class dataBase : MonoBehaviour
{
    private string dbName = "URI=file:jautajumi.db";
    private Objects objekti;

    void Start()
    {
        objekti = FindFirstObjectByType<Objects>(); // Only one expected
        if (objekti == null)
        {
            Debug.LogError("No 'Objects' component found in the scene!");
            return;
        }

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
                        OpcijaD VARCHAR(45)
                    );";
                command.ExecuteNonQuery();
            }
        }
    }

    public void addData()
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
                    INSERT INTO jautajumuBanka 
                    (Laiks, Jautajums, Bilde, Atbilde, OpcijaB, OpcijaC, OpcijaD) 
                    VALUES (@laiks, @jautajums, @bilde, @atbilde, @opcB, @opcC, @opcD);";

                command.Parameters.Add(new SqliteParameter("@laiks", objekti.inputField[5].text));
                command.Parameters.Add(new SqliteParameter("@jautajums", objekti.inputField[6].text));
                command.Parameters.Add(new SqliteParameter("@bilde", objekti.inputField[0].text));
                command.Parameters.Add(new SqliteParameter("@atbilde", objekti.inputField[1].text));
                command.Parameters.Add(new SqliteParameter("@opcB", objekti.inputField[2].text));
                command.Parameters.Add(new SqliteParameter("@opcC", objekti.inputField[3].text));
                command.Parameters.Add(new SqliteParameter("@opcD", objekti.inputField[4].text));


                command.ExecuteNonQuery();
            }
        }
    }
}
