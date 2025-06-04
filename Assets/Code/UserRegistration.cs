using Mono.Data.SqliteClient;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class UserRegistration : MonoBehaviour
{
    public TMP_InputField usernameField;
    public TMP_InputField passwordField;
    public TMP_Dropdown roleDropdown; 
    public TMP_Text text;

    private string dbName = "URI=file:jautajumi.db";

    public void OnRegisterButtonClick()
    {
        string username = usernameField.text.Trim();
        string password = passwordField.text.Trim();
        string role = roleDropdown.options[roleDropdown.value].text; 

        if (username == "" || password == "")
        {
            text.text = "Lūdzu aizpildiet visus laukus!";
            return;
        }

        if (IsUsernameTaken(username))
        {
            text.text = "Lietotājvārds jau eksistē!";
            return;
        }

        RegisterUser(username, password, role);
        text.text = "Reģistrācija veiksmīga!";
        // Optional: load login scene
        SceneManager.LoadScene("loginScene");
    }

    private bool IsUsernameTaken(string username)
    {
        using (var connection = new SqliteConnection(dbName))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT COUNT(*) FROM users WHERE username = @username";
                command.Parameters.Add(new SqliteParameter("@username", username));

                int count = System.Convert.ToInt32(command.ExecuteScalar());
                return count > 0;
            }
        }
    }

    private void RegisterUser(string username, string password, string role)
    {
        using (var connection = new SqliteConnection(dbName))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "INSERT INTO users (username, password, role) VALUES (@username, @password, @role)";
                command.Parameters.Add(new SqliteParameter("@username", username));
                command.Parameters.Add(new SqliteParameter("@password", password));
                command.Parameters.Add(new SqliteParameter("@role", role));
                command.ExecuteNonQuery();
            }
        }
    }
}
