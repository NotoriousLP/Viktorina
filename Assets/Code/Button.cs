using UnityEngine;
using UnityEngine.SceneManagement;

public class Button : MonoBehaviour
{
    private Objects objekti;

    void Start()
    {
        objekti = FindFirstObjectByType<Objects>(); 
    }
    public void toGame()
    {
        SceneManager.LoadScene("GameScene", LoadSceneMode.Single); //Ielādē jaunu ainu

    }
    public void toGameChoice()
    {
        SceneManager.LoadScene("GameChoice", LoadSceneMode.Single); //Ielādē jaunu ainu
    }
    public void backToMain()
    {
        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single); //Ielādē jaunu ainu
    }

    public void toQuizTake()
    {
        SceneManager.LoadScene("QuizTake", LoadSceneMode.Single); //Ielādē jaunu ainu
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
        }

    }
    public void showBank()
    {
            objekti.objects[1].SetActive(true);
    }

}

