using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
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
        totalQuestions = QnA.Count;
        GameOverPanel.SetActive(false);
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
