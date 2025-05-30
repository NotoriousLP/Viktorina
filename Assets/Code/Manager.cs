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
    int correctAnswers = 0;
    public int score;

    private void Start()
    {
        QnA = new List<Questions>();
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
                        Sprite bilde = Resources.Load<Sprite>("Images/" + bildeNosaukums);
                        q.Image = bilde;

                        string timeStr = reader["Laiks"].ToString();
                        float timeValue;
                        float.TryParse(timeStr, out timeValue);
                        q.TimeLimit = timeValue;

                        QnA.Add(q);
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
        ScoreText.text = "Pareizas atbildes skaits: " + correctAnswers + " / " + totalQuestions +  "\nIeguto punktu skaits: " + score;

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

    // NEW: Dynamic point calculation based on time limit
    private int CalculatePoints(float timeLimit)
    {
        if (timeLimit <= 0) return 1;
        if (timeLimit <= 4) return 10;
        if (timeLimit <= 9) return 8;
        if (timeLimit <= 14) return 6;
        if (timeLimit <= 19) return 4;
        return 2;
    }

    public void Correct()
    {
        correctAnswers++;
        int pointsEarned = CalculatePoints(QnA[currentQuestion].TimeLimit);
        score += pointsEarned;

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
        // Получаем текущий вопрос и ответы
        Questions currentQ = QnA[currentQuestion];

        // Создаём список пар: текст + метка, правильный ли
        List<(string text, bool isCorrect)> shuffledAnswers = new List<(string, bool)>();

        for (int i = 0; i < currentQ.Answers.Length; i++)
        {
            bool isCorrect = (i == currentQ.CorrectAnswer - 1); // CorrectAnswer == 1 -> индекс 0
            shuffledAnswers.Add((currentQ.Answers[i], isCorrect));
        }

        // Перемешиваем список
        for (int i = 0; i < shuffledAnswers.Count; i++)
        {
            int rnd = Random.Range(i, shuffledAnswers.Count);
            (shuffledAnswers[i], shuffledAnswers[rnd]) = (shuffledAnswers[rnd], shuffledAnswers[i]);
        }

        // Назначаем кнопкам текст и isCorrect
        for (int i = 0; i < options.Length; i++)
        {
            var answerScript = options[i].GetComponent<AnswersScript>();
            answerScript.isCorrect = shuffledAnswers[i].isCorrect;

            Text answerText = options[i].transform.GetChild(0).GetComponent<Text>();
            answerText.text = shuffledAnswers[i].text;

            answerScript.ResetColor();
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

