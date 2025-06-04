using UnityEngine;
using UnityEngine.SceneManagement;


//CustomButton — pārvalda pogu darbības galvenajā izvēlnē un citās scenās.
//-ainas pārlāde
//-Pogu redzamība pēc lietotāja lomas
//-UI logu aizvēršana
//-Logout, Quit

public class CustomButton : MonoBehaviour
{
    private Objects objekti;

    //"Pievienot" poga — kuru rāda/slēpj atkarībā no lietotāja lomas
    public UnityEngine.UI.Button addPoga;

    void Start()
    {
        objekti = FindFirstObjectByType<Objects>();

        //Pārbauda lietotāja lomu un parāda/slēpj pogu
        if (CurrentUser.Role == "player")
        {
            // ja spēlētājs = nerāda "Pievienot" pogu
            addPoga.gameObject.SetActive(false);
        }
        else if (CurrentUser.Role == "editor")
        {
            // ja editors = Redaktors → rāda "Pievienot" pogu
            addPoga.gameObject.SetActive(true);
        }
        else
        {
            Debug.Log("addPoga nav piešķirta! Pārbaudi Inspector laukā.");
        }
    }

    //Ainas pārlādes funkcijas:
    public void toGame()
    {
        SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
    }

    public void toGameChoice()
    {
        SceneManager.LoadScene("GameChoice", LoadSceneMode.Single);
    }

    public void toGameOption()
    {
        SceneManager.LoadScene("GameOption", LoadSceneMode.Single);
    }

    public void backToMain()
    {
        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
    }

    public void toAddQuestions()
    {
        SceneManager.LoadScene("CreateQuestions", LoadSceneMode.Single);
    }

    public void toQuizTake()
    {
        SceneManager.LoadScene("QuizTake", LoadSceneMode.Single);
    }

    public void toScoreBoard()
    {
        SceneManager.LoadScene("ScoreBoard", LoadSceneMode.Single);
    }

    public void toLogOut()
    {
        SceneManager.LoadScene("loginScene", LoadSceneMode.Single);
    }

    public void quitGame()
    {
        Application.Quit(); //Iziet no spēles
    }

    public void toRegister()
    {
        SceneManager.LoadScene("RegisterScene", LoadSceneMode.Single);
    }

    // Aizver visus UI paneļus (objects) un notīra ievadlaukus.
    public void closeButton()
    {
        // Aizver visus paneļus
        for (int i = 0; i < objekti.objects.Length; i++)
        {
            objekti.objects[i].SetActive(false);
            objekti.inputField[i].text = ""; //Notīra laukus 
        }

        //notīra visus inputField gadijumā ja tādi ir
        for (int i = 0; i < objekti.inputField.Length; i++)
        {
            objekti.inputField[i].text = "";
        }
    }

    // Aizver tikai rediģēšanas logu.
    public void closeButtonEdit()
    {
        objekti.objects[0].SetActive(false);
        objekti.inputField[0].text = "";
    }

    // Parāda banku pievienošanas logu.
    public void showBank()
    {
        //Noslēpj kļūdu paziņojumu
        objekti.text[7].gameObject.SetActive(false);

        //Parāda bankas pievienošanas paneli
        objekti.objects[1].SetActive(true);
    }
}
