using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public GameObject userManagementButton; // Poga, kas jāslēpj vai jāparāda

    void Start()
    {
        // Rāda pogu tikai, ja lietotājam ir redaktora vai administratora loma
        if (CurrentUser.Role == "admin" || CurrentUser.Role == "editor")
        {
            userManagementButton.SetActive(true);
        }
        else
        {
            userManagementButton.SetActive(false);
        }
    }

    // Tiek izsaukta, kad nospiež uz User Management pogas
    public void GoToUserManagement()
    {
        SceneManager.LoadScene("UserManagement");
    }
}
