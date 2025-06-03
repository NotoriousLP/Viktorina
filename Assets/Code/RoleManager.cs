using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mono.Data.SqliteClient;
using UnityEngine.SceneManagement; // Nepieciešams, lai pārslēgtu scēnas

public class RoleManager : MonoBehaviour
{
    public GameObject userRowPrefab;    // UserRow prefabs, kas tiks instanciēts katram lietotājam
    public Transform contentParent;     // Scroll View > Content objekts, kur ievieto rindas

    private string dbName = "URI=file:jautajumi.db"; // Norāde uz SQLite datu bāzes failu

    void Start()
    {
        // Pārbauda, vai pašreizējam lietotājam ir tiesības skatīt šo logu
        if (CurrentUser.Role != "editor" && CurrentUser.Role != "admin")
        {
            Debug.LogWarning("Piekļuve liegta – tikai redaktoriem/adminiem.");
            return;
        }

        // Ielādē visus lietotājus no datu bāzes
        LoadUsers();
    }

    // Metode, lai ielādētu lietotājus un parādītu katru kā atsevišķu rindu UI
    void LoadUsers()
    {
        // Notīra vecās rindas, ja tādas jau ir
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        using (var connection = new SqliteConnection(dbName))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT user_id, username, role FROM users";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int userId = reader.GetInt32(0);
                        string username = reader.GetString(1);
                        string role = reader.GetString(2);

                        // Izveido jaunu rindu no prefaba
                        GameObject row = Instantiate(userRowPrefab, contentParent);

                        // Atrodi UI elementus prefabā
                        TMP_Text nameText = row.transform.Find("UsernameText")?.GetComponent<TMP_Text>();
                        TMP_Dropdown dropdown = row.transform.Find("RoleDropdown")?.GetComponent<TMP_Dropdown>();
                        Button updateBtn = row.transform.Find("UpdateButton")?.GetComponent<Button>();

                        // Uzstāda lietotājvārdu tekstā
                        if (nameText != null)
                            nameText.text = username;

                        // Uzstāda dropdown vērtību, atbilstoši lietotāja lomai
                        if (dropdown != null)
                        {
                            int index = dropdown.options.FindIndex(opt => opt.text == role);
                            dropdown.value = index >= 0 ? index : 0;
                        }

                        // Kad nospiež "Update", saglabā jauno lomu datubāzē
                        if (updateBtn != null && dropdown != null)
                        {
                            int capturedId = userId;
                            updateBtn.onClick.AddListener(() =>
                            {
                                string newRole = dropdown.options[dropdown.value].text;
                                UpdateUserRole(capturedId, newRole);
                            });
                        }
                    }
                }
            }
        }
    }

    // Atjaunina lietotāja lomu datu bāzē
    void UpdateUserRole(int userId, string newRole)
    {
        using (var connection = new SqliteConnection(dbName))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "UPDATE users SET role = @role WHERE user_id = @id";
                command.Parameters.Add(new SqliteParameter("@role", newRole));
                command.Parameters.Add(new SqliteParameter("@id", userId));
                command.ExecuteNonQuery();

                Debug.Log($"Lietotājs {userId} atjaunināts uz lomu: {newRole}");
            }
        }
    }

    // Šī metode tiek izsaukta, kad lietotājs nospiež "Home" pogu
    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu"); // Aizvieto ar precīzu MainMenu scēnas nosaukumu
    }
}
// Šī klase pārvalda lomu pārvaldību, ļaujot rediģēt lietotāju lomas un piekļuves tiesības