using Mono.Data.SqliteClient;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public static class CurrentUser
{
    public static int UserID;
    public static string Username;
    public static string Role;
}

public class UserLogin : MonoBehaviour
{
    public TMP_InputField usernameField;
    public TMP_InputField passwordField;
    public TMP_Text text;

    private string dbName = "URI=file:jautajumi.db";

    public void OnLoginButtonClick()
    {
        string username = usernameField.text;
        string password = passwordField.text;

        Login(username, password);
    }

    public void Login(string username, string password)
    {
        using (var connection = new SqliteConnection(dbName))
        {
            connection.Open();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT * FROM users WHERE username = @username AND password = @password";
                command.Parameters.Add(new SqliteParameter("@username", username));
                command.Parameters.Add(new SqliteParameter("@password", password));

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        // Login OK
                        CurrentUser.UserID = reader.GetInt32(0);
                        CurrentUser.Username = reader.GetString(1);
                        CurrentUser.Role = reader.GetString(3);

                        Debug.Log($"Login OK: {CurrentUser.Username} ({CurrentUser.Role})");

                        // Pēc veiksmīgas pieteikšanās pāriet uz MainMenu neatkarīgi no lietotāja lomas
                        SceneManager.LoadScene("MainMenu");

                    }
                    else
                    {
                        text.text = "Nepareiza lietotājvards / parole!";
                    }
                }
            }
        }
    }
}
