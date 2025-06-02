using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Button : MonoBehaviour
{
    private Objects objekti;
    public UnityEngine.UI.Button addPoga;
    void Start()
    {
        objekti = FindFirstObjectByType<Objects>();
        if (CurrentUser.Role == "player"){
            // Paslēpj Edit pogu
            addPoga.gameObject.SetActive(false);
        }
        else if (CurrentUser.Role == "editor")
        {
            // Parāda Edit pogu
            addPoga.gameObject.SetActive(true);
        }
    }
    public void toGame()
    {
        SceneManager.LoadScene("GameScene", LoadSceneMode.Single); //Ielādē jaunu ainu

    }
    public void toGameChoice()
    {
        SceneManager.LoadScene("GameChoice", LoadSceneMode.Single); //Ielādē jaunu ainu
    }

    public void toGameOption()
    {
        SceneManager.LoadScene("GameOption", LoadSceneMode.Single); //Ielādē jaunu ainu
    }
    public void backToMain()
    {
        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single); //Ielādē jaunu ainu
    }
    public void toAddQuestions()
    {
        SceneManager.LoadScene("CreateQuestions", LoadSceneMode.Single); //Ielādē jaunu ainu
    }
    public void toQuizTake()
    {
        SceneManager.LoadScene("QuizTake", LoadSceneMode.Single); //Ielādē jaunu ainu
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

    public void closeButton()
    {
        for (int i = 0; i < objekti.objects.Length; i++)
        {
            objekti.objects[i].SetActive(false);
            objekti.inputField[i].text = "";
        }
        for (int i = 0; i < objekti.inputField.Length; i++)
        {
            objekti.inputField[i].text = "";
        }
    }

    public void closeButtonEdit()
    {
            objekti.objects[0].SetActive(false);
            objekti.inputField[0].text = "";
    }

    public void showBank()
    {
        objekti.objects[1].SetActive(true);
    }


}

