using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AnswersScript : MonoBehaviour
{
    public bool isCorrect = false;
    public Manager manager;

    public Color krasa;
    public void Awake()
    {
        krasa = GetComponent<Image>().color;
    }
    public void ResetColor()
    {
    GetComponent<Image>().color = krasa;
    }
    public void Answer()
    {
        if (isCorrect)
        {
            GetComponent<Image>().color = Color.green;
            Debug.Log("Pareiza atbilde!");
            manager.Correct();
        }
        else
        {
            GetComponent<Image>().color = Color.red;
            Debug.Log("Nepareiza atbilde!");
            manager.Wrong();
        }
    }
}
