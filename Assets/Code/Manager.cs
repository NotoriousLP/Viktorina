using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Mono.Data.SqliteClient;
using UnityEngine.Networking;
using TMPro;
public class Manager : MonoBehaviour
{
    public List<Questions> QnA;
    public GameObject[] options;
    public int currentQuestion;

    public Image QuestionImage;
    public GameObject QuizPanel;
    public GameObject GameOverPanel;
    public Text QuestionText;
    public Text ScoreText;
    public TMP_InputField playerNameInputField;

    public UnityEngine.UI.Button saveButton;
    public Text TimeText;

    private float questionTime;
    private float timeLeft;
    private bool timerRunning = false;


    int totalQuestions = 0;
    public int score;
    private void Start()
    {
        QnA = new List<Questions>(); // Sagatavo tukšu sarakstu
        StartCoroutine(LoadQuestionsForSelectedBank());
        Debug.Log("Ielādējam jautājumus bankai ID: " + SelectedBank.ID);

    }


    private IEnumerator LoadQuestionsForSelectedBank()
    {
        string dbName = "URI=file:jautajumi.db";

        using (var connection = new SqliteConnection(dbName))
        {
            connection.Open();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT * FROM jautajumuBanka WHERE banka_id = @id";
                command.Parameters.Add(new SqliteParameter("@id", SelectedBank.ID));

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Debug.Log("Atrasts jautājums: " + reader["Jautajums"].ToString());

                        Questions q = new Questions();
                        q.Question = reader["Jautajums"].ToString();
                        q.Answers = new string[]
                        {
                            reader["Atbilde"].ToString(),
                            reader["OpcijaB"].ToString(),
                            reader["OpcijaC"].ToString(),
                            reader["OpcijaD"].ToString()
                        };
                        q.CorrectAnswer = 1;
                        string bildeNosaukums = reader["Bilde"].ToString();
                        Debug.Log("Mēģinam ielādēt bildi: " + bildeNosaukums);
                        Sprite bilde = Resources.Load<Sprite>("Images/" + bildeNosaukums);

                        if (bilde == null)
                        {
                            Debug.LogWarning("Attēls nav atrasts Resources: Images/" + bildeNosaukums);
                        }
                        else
                        {
                            Debug.Log("Attēls ielādēts veiksmīgi: " + bildeNosaukums);
                        }

                        q.Image = bilde;
                        QnA.Add(q);
                        string timeStr = reader["Laiks"].ToString();
                        float timeValue;
                        float.TryParse(timeStr, out timeValue);
                        q.TimeLimit = timeValue;
                        yield return null;
                    }
                }
            }
        }

        totalQuestions = QnA.Count;
        GameOverPanel.SetActive(false);
        Debug.Log("Gatavs jautājumu skaits: " + QnA.Count);
        generateQuestion();
    }




    public void velreiz()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
  void GameOver()
    {
        QuizPanel.SetActive(false);

     
        GameOverPanel.SetActive(true);

        ScoreText.text = score + "/" + totalQuestions;

      
        var scoreboard = FindFirstObjectByType<ScoreboardLoader>();
        if (scoreboard != null)
        {
            scoreboard.LoadScoreboard(SelectedBank.ID);
          
        }
        else
        {
            Debug.LogWarning("Kļūda ar scoreboard!");
        }
    }
    public void Correct()
    {
        score += 1;
        QnA.RemoveAt(currentQuestion);
        StartCoroutine(WaitAndGenerate());
    }
    public void Wrong()
    {
        QnA.RemoveAt(currentQuestion);
        StartCoroutine(WaitAndGenerate());
    }
    private IEnumerator WaitAndGenerate()
    {
        yield return new WaitForSeconds(1f);
        generateQuestion();
    }
    void SetAnswers()
    {
        for (int i = 0; i < options.Length; i++)
        {
            options[i].GetComponent<AnswersScript>().isCorrect = false;
            options[i].transform.GetChild(0).GetComponent<Text>().text = QnA[currentQuestion].Answers[i];

            if (QnA[currentQuestion].CorrectAnswer == i + 1)
            {
                options[i].GetComponent<AnswersScript>().isCorrect = true;
            }
        }
    }

    void generateQuestion()
    {
        if (QnA.Count > 0)
        {
            foreach (var btn in options)
            {
                btn.GetComponent<AnswersScript>().ResetColor();
            }
            currentQuestion = Random.Range(0, QnA.Count);

            QuestionImage.sprite = QnA[currentQuestion].Image;
            QuestionText.text = QnA[currentQuestion].Question;
            SetAnswers();

            if (QnA[currentQuestion].TimeLimit > 0)
            {
                questionTime = QnA[currentQuestion].TimeLimit;
                timeLeft = questionTime;
                timerRunning = true;
            }
            else
            {
                TimeText.text = "";
                Debug.LogWarning("TimeLimit for this question is zero or invalid.");
                timerRunning = false;
            }
        }
        else
        {
            Debug.Log("Nav vairāk jautājumu!");
            GameOver();
        }
    }

    void Update()
    {
        if (timerRunning)
        {
            timeLeft -= Time.deltaTime;
            TimeText.text = Mathf.Ceil(timeLeft).ToString() + "s";

            if (timeLeft <= 0)
            {
                timerRunning = false;


                if (questionTime > 0)
                {
                    Wrong();
                }
                else
                {
                    TimeText.text = "";
                }
            }
        }
    }
    
   public void SaveScore()
{
    string playerName = "Nezināms";

    if (playerNameInputField != null)
    {
        playerName = playerNameInputField.text.Trim();
        if (string.IsNullOrWhiteSpace(playerName))
        {
            playerName = "Nezināms";
        }
    }

    var db = FindFirstObjectByType<dataBase>();
    if (db == null)
    {
        Debug.LogError("Datu bāze - kļūda! Neizdevās saglabāt rezultātu.");
        return;
    }

    
    bool nameExists = false;

    using (var connection = new SqliteConnection("URI=file:jautajumi.db"))
    {
        connection.Open();
        using (var command = connection.CreateCommand())
        {
            command.CommandText = @"
                SELECT COUNT(*) 
                FROM scoreBoard 
                WHERE playerName = @name AND banka_id = @bankaId";
            command.Parameters.Add(new SqliteParameter("@name", playerName));
            command.Parameters.Add(new SqliteParameter("@bankaId", SelectedBank.ID));

            long count = (long)command.ExecuteScalar();
            nameExists = count > 0;
        }
        connection.Close();
    }

    if (nameExists)
    {
        Debug.LogWarning($"Spēlētājs '{playerName}' jau ir saglabāts šajā bankā. Nevar saglabāt atkārtoti.");
        return;
    }

 
    db.SavePlayerScore(playerName, score);
    Debug.Log($"Score saglabāts sēlētājam '{playerName}' ar {score} punktiem.");

   
    var scoreboard = FindFirstObjectByType<ScoreboardLoader>();
    if (scoreboard != null)
    {
        scoreboard.LoadScoreboard(SelectedBank.ID);
        Debug.Log("Scoreboard atjaunots.");
    }
    else
    {
        Debug.LogWarning("ScoreboardLoader nav atrasts - rezultātu tabula netika atjaunināta.");
    }


    if (saveButton != null)
    {
        saveButton.interactable = false;
    }

    if (playerNameInputField != null)
    {
        playerNameInputField.text = "";
    }
}


    

}
