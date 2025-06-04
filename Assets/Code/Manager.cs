using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Mono.Data.SqliteClient;
using UnityEngine.Networking;
using TMPro;
using System;

public class Manager : MonoBehaviour
{
    public List<Questions> QnA;  //Saraksts ar jautājumiem
    public GameObject[] options;  
    public int currentQuestion;   

    public Image QuestionImage;  
    public GameObject QuizPanel;                   
    public GameObject GameOverPanel;                
    public Text QuestionText;                      
    public Text ScoreText;                        
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

        if (saveButton != null)
        {
            saveButton.interactable = false;
        }
    }

    //Ielādē jautājumus no datubāzes izvēlētajai bankai
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

                        string bildeCels = reader["Bilde"].ToString();
                        yield return StartCoroutine(LoadImageFromPath(bildeCels, q));

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

    //Ielādē jautājuma attēlu no path
    private IEnumerator LoadImageFromPath(string path, Questions q)
    {
        if (string.IsNullOrEmpty(path))
        {
            Debug.LogWarning("Bilde path ir tukšs.");
            q.Image = null;
            yield break;
        }

        string finalPath = path;
        if (!path.StartsWith("file://"))
        {
            finalPath = "file://" + path;
        }

        Debug.Log("Mēģina ielādēt attēlu no ceļa: " + finalPath);

        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(finalPath))
        {
            yield return uwr.SendWebRequest();

            if (uwr.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(uwr);
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                q.Image = sprite;

                Debug.Log($"Bilde ielādēta no ceļa: {path}");
            }
            else
            {
                Debug.LogError($"Neizdevās ielādēt bildi no ceļa: {path}. Kļūda: {uwr.error}");
                q.Image = null;
            }
        }
    }

    //ielādē ainu no jauna
    public void velreiz()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    //Beidzās spēle, tad parāda rezultātu
    void GameOver()
    {
        QuizPanel.SetActive(false);
        GameOverPanel.SetActive(true);

        ScoreText.text = "Pareizas atbildes skaits: " + correctAnswers + " / " + totalQuestions + "\nIeguto punktu skaits: " + score;

        //Pārbauda vai var saglabāt jaunu rezultātu
        int? existingScore = null;

        using (var connection = new SqliteConnection("URI=file:jautajumi.db"))
        {
            connection.Open();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    SELECT punkti 
                    FROM scoreBoard 
                    WHERE playerName = @name AND banka_id = @bankaId";
                command.Parameters.Add(new SqliteParameter("@name", CurrentUser.Username));
                command.Parameters.Add(new SqliteParameter("@bankaId", SelectedBank.ID));

                var result = command.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                {
                    existingScore = Convert.ToInt32(result);
                }
            }

            connection.Close();
        }

        if (!existingScore.HasValue || score > existingScore.Value)
        {
            if (saveButton != null)
            {
                saveButton.interactable = true;
            }
        }
        else
        {
            Debug.Log($"Rezultāts netiek saglabāts – esošais ({existingScore}) ir labāks vai vienāds.");
        }

        //Atjauno scoreboard
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

    //Aprēķina punktus atkarībā no laika limita
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
        if (QnA.Count > 0 && currentQuestion >= 0 && currentQuestion < QnA.Count)
        {
            QnA.RemoveAt(currentQuestion);
        }
        else
        {
            Debug.LogWarning("Wrong() izsaukts, bet QnA ir tukšs vai indekss ārpus robežām!");
        }

        StartCoroutine(WaitAndGenerate());
    }

    //Neliela pauze pirms nākamā jautājuma
    private IEnumerator WaitAndGenerate()
    {
        yield return new WaitForSeconds(1f);
        generateQuestion();
    }

    //sajauc atbilžu pogas
    void SetAnswers()
    {
        Questions currentQ = QnA[currentQuestion];

        List<(string text, bool isCorrect)> shuffledAnswers = new List<(string, bool)>();

        for (int i = 0; i < currentQ.Answers.Length; i++)
        {
            bool isCorrect = (i == currentQ.CorrectAnswer - 1);
            shuffledAnswers.Add((currentQ.Answers[i], isCorrect));
        }

        //Sajaukšana notiek
        for (int i = 0; i < shuffledAnswers.Count; i++)
        {
            int rnd = UnityEngine.Random.Range(i, shuffledAnswers.Count);
            (shuffledAnswers[i], shuffledAnswers[rnd]) = (shuffledAnswers[rnd], shuffledAnswers[i]);
        }

        //Uzliek pogas
        for (int i = 0; i < options.Length; i++)
        {
            var answerScript = options[i].GetComponent<AnswersScript>();
            answerScript.isCorrect = shuffledAnswers[i].isCorrect;

            Text answerText = options[i].transform.GetChild(0).GetComponent<Text>();
            answerText.text = shuffledAnswers[i].text;

            answerScript.ResetColor();
        }
    }

    //Ģenerē nākamo jautājumu
    void generateQuestion()
    {
        if (QnA.Count > 0)
        {
            foreach (var btn in options)
            {
                btn.GetComponent<AnswersScript>().ResetColor();
            }

            currentQuestion = UnityEngine.Random.Range(0, QnA.Count);

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

    //Saglabā punktus datubāzē
    public void SaveScore()
    {
        var db = FindFirstObjectByType<dataBase>();
        if (db == null)
        {
            Debug.LogError("Datu bāze - kļūda! Neizdevās saglabāt rezultātu.");
            return;
        }

        int? existingScore = null;

        using (var connection = new SqliteConnection("URI=file:jautajumi.db"))
        {
            connection.Open();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    SELECT punkti 
                    FROM scoreBoard 
                    WHERE playerName = @name AND banka_id = @bankaId";
                command.Parameters.Add(new SqliteParameter("@name", CurrentUser.Username));
                command.Parameters.Add(new SqliteParameter("@bankaId", SelectedBank.ID));

                var result = command.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                {
                    existingScore = Convert.ToInt32(result);
                }
            }

            if (existingScore.HasValue)
            {
                Debug.Log($"Spēlētājam '{CurrentUser.Username}' jau ir {existingScore.Value} punkti.");

                if (score > existingScore.Value)
                {
                    using (var updateCmd = connection.CreateCommand())
                    {
                        updateCmd.CommandText = @"
                        UPDATE scoreBoard 
                        SET punkti = @newScore 
                        WHERE playerName = @name AND banka_id = @bankaId";
                        updateCmd.Parameters.Add(new SqliteParameter("@newScore", score));
                        updateCmd.Parameters.Add(new SqliteParameter("@name", CurrentUser.Username));
                        updateCmd.Parameters.Add(new SqliteParameter("@bankaId", SelectedBank.ID));

                        updateCmd.ExecuteNonQuery();

                        Debug.Log($"Spēlētājam '{CurrentUser.Username}' atjaunināti punkti uz {score}.");
                        saveButton.interactable = false;
                    }
                }
                else
                {
                    Debug.Log($"Esošie punkti ({existingScore.Value}) ir labāki vai vienādi. Rezultāts netiek atjaunināts.");
                }
            }
            else
            {
                db.SavePlayerScore(CurrentUser.Username, score);
                Debug.Log($"Jauns rezultāts saglabāts spēlētājam '{CurrentUser.Username}' ar {score} punktiem.");
            }

            connection.Close();
        }

        var scoreboard = FindFirstObjectByType<ScoreboardLoader>();
        if (scoreboard != null)
        {
            scoreboard.LoadScoreboard(SelectedBank.ID);
        }

        if (saveButton != null)
        {
            saveButton.interactable = false;
        }
    }
}
