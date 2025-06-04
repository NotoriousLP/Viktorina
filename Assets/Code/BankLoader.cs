using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mono.Data.SqliteClient;
using UnityEngine.SceneManagement;

//BankLoader — atbild par banku (jautājumu banku) saraksta ielādi, dzēšanu un izvēli.
//Pārslēdzās starp spēles skatu un jautājumu rediģēšanas skatu.
//Pievieno banku pogas dinamiski no DB.
//Apstrādā Edit un Delete funkcijas.
public class BankLoader : MonoBehaviour
{
    public GameObject buttonPrefab;          //Poga (prefab), ko pievieno banku sarakstam
    public Transform parentPanel;            //Kur ielikt pogas (Content)
    private Objects objekti;                 //UI objekti (input fields, texti, paneļi)
    public EditBankLoader editBankLoader;    
    public dataBase database;                
    public ImageImporter imageImporter;      

    private string dbName = "URI=file:jautajumi.db"; 

    void Start()
    {
        imageImporter = FindFirstObjectByType<ImageImporter>();
        database = FindFirstObjectByType<dataBase>();

        //ielādē bankas atbilstoši pēc aina
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

    //Ielādē bankas CreateQuestions skatā.
    //Dinamiski ģenerē pogas ar Edit un Delete.
    public void LoadBanks()
    {
        //Notīra jau esošas pogas
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

                        //Izveido pogu klonēšanai
                        GameObject btn = Instantiate(buttonPrefab, parentPanel);

                        //Uzliek tekstu
                        btn.transform.GetChild(0).GetComponent<TMP_Text>().text = name;

                        //Pievieno "OnClick" izvēlei
                        btn.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => OnBankSelected(id, name));

                        //DELETE poga
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

                        //EDIT poga
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


    //Ielādē bankas spēles laikā (bez Edit un Delete).
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

    // Edit bankas pogas funkcija — pāriet uz jautājumu rediģēšanas skatu.
       public void EditBank(int id)
    {
        Debug.Log($"Edit bankas ID: {id}");

        SelectedBank.ID = id;

        //Atver rediģēšanas paneli
        objekti.objects[2].SetActive(true);

        //Ielādē jautājumus šai bankai
        editBankLoader.LoadQuestionsForBank(id);
    }

    //Dzēš banku un visus tās jautājumus.

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

        //Atjauno sarakstu
        LoadBanks();
    }


    //Kad lietotājs izvēlas banku (spēlē vai CreateQuestions ainā).
    void OnBankSelected(int id, string name)
    {
        SelectedBank.ID = id;
        SelectedBank.Name = name;

        Debug.Log("Izvēlēta banka: " + name);

        if (SceneManager.GetActiveScene().name == "CreateQuestions")
        {
            //Noslēpj kļūdu tekstu (ja bija)
            objekti.text[8].gameObject.SetActive(false);

            int bankaId = SelectedBank.ID;
            if (bankaId == 0)
            {
                Debug.LogError("Nav izvēlēta banka! BankaId = 0");
                return;
            }

            //Atver jautājumu pievienošanas logu
            objekti.objects[0].SetActive(true);

            //Notīra ievadlaukus
            for (int i = 0; i < objekti.inputField.Length; i++)
            {
                objekti.inputField[i].text = "";
            }

            //Notīra bildes izvēli
            imageImporter.savedFilePath = "";

            //Piesaista pogu uz pievienošanu
            objekti.okPoga.onClick.RemoveAllListeners();
            objekti.okPoga.onClick.AddListener(database.addDataQuestion);

            //Maina pogas tekstu
            objekti.okPoga.transform.GetChild(0).GetComponent<Text>().text = "Pievienot jautājumu";

            Debug.Log("Atvērts jauna jautājuma logs.");
        }
        else
        {
            //Ja spēles aina, tad pārslēdz uz GameScene
            SceneManager.LoadScene("GameScene");
        }
    }
}
