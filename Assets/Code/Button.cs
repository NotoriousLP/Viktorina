using UnityEngine;
using UnityEngine.SceneManagement;

public class Button : MonoBehaviour
{    public void toGame()
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

    public void toQuizTake(){
       SceneManager.LoadScene("QuizTake", LoadSceneMode.Single); //Ielādē jaunu ainu
    }

    public void quitGame()
    {
        Application.Quit(); //Iziet no spēles

    }
}
