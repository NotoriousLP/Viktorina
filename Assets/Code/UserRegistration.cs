// Nepieciešamo bibliotēku importēšana darbam ar datubāzi, Unity komponentēm un UI elementiem
using Mono.Data.SqliteClient;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

// Klase, kas nodrošina lietotāja reģistrāciju spēles sistēmā

public class UserRegistration : MonoBehaviour
{
     // UI lauki, kas saņem lietotāja ievadīto lietotājvārdu, paroli un izvēlēto lomu (dropdown)
    public TMP_InputField usernameField;
    public TMP_InputField passwordField;
    public TMP_Dropdown roleDropdown; 
    public TMP_Text text;// Teksta lauks ziņojumu attēlošanai (piemēram, kļūdu vai apstiprinājuma)

    // Savienojuma virkne uz SQLite datubāzi
    private string dbName = "URI=file:jautajumi.db";
    
    // Funkcija, kas tiek izsaukta, kad lietotājs nospiež pogu "Reģistrēties"
    public void OnRegisterButtonClick()
    {
        // Iegūst ievadīto lietotājvārdu un paroli, noņemot liekās atstarpes
        string username = usernameField.text.Trim();
        string password = passwordField.text.Trim();

        // Iegūst izvēlēto lomu no dropdown saraksta
        string role = roleDropdown.options[roleDropdown.value].text;

        // Validācija — pārbauda, vai visi lauki ir aizpildīti
        if (username == "" || password == "")
        {
            text.text = "Lūdzu aizpildiet visus laukus!";
            return;
        }

        // Pārbauda, vai lietotājvārds jau eksistē datubāzē
        if (IsUsernameTaken(username))
        {
            text.text = "Lietotājvārds jau eksistē!";
            return;
        }

        // Ja lietotājvārds ir unikāls, izsauc reģistrācijas funkciju un attēlo apstiprinājuma ziņu
        RegisterUser(username, password, role);
        text.text = "Reģistrācija veiksmīga!";
        // Optional: load login scene
        // Pēc reģistrācijas lietotājs tiek novirzīts atpakaļ uz login skatu
        SceneManager.LoadScene("loginScene");
    }
    // Funkcija, kas pārbauda, vai konkrētais lietotājvārds jau ir reģistrēts datubāzē
    private bool IsUsernameTaken(string username)
    {
        using (var connection = new SqliteConnection(dbName))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                // SQL vaicājums, lai saskaitītu, cik ierakstu atbilst norādītajam lietotājvārdam

                command.CommandText = "SELECT COUNT(*) FROM users WHERE username = @username";
                command.Parameters.Add(new SqliteParameter("@username", username));
                // Ja skaits ir lielāks par 0 — lietotājvārds jau eksistē
                int count = System.Convert.ToInt32(command.ExecuteScalar());
                return count > 0;
            }
        }
    }
    // Funkcija, kas ievieto jaunu lietotāju datubāzē
    private void RegisterUser(string username, string password, string role)
    {
        using (var connection = new SqliteConnection(dbName))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                // SQL vaicājums jauna lietotāja pievienošanai datubāzes `users` tabulā
                command.CommandText = "INSERT INTO users (username, password, role) VALUES (@username, @password, @role)";
                command.Parameters.Add(new SqliteParameter("@username", username));
                command.Parameters.Add(new SqliteParameter("@password", password));
                command.Parameters.Add(new SqliteParameter("@role", role));
                command.ExecuteNonQuery(); // Vaicājuma izpilde
            }
        }
    }
}
