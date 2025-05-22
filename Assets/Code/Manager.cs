using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Mono.Data.SqliteClient;
using UnityEngine.Networking;
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
        for(int i=0; i < options.Length; i++)
        {
        options[i].GetComponent<AnswersScript>().isCorrect = false;
        options[i].transform.GetChild(0).GetComponent<Text>().text = QnA[currentQuestion].Answers[i];           
        
            if(QnA[currentQuestion].CorrectAnswer == i+1)
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

        }
        else
        {
            Debug.Log("Nav vairāk jautājumu!");
            GameOver();
        }
    }
    

}
