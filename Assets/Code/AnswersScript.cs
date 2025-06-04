using UnityEngine;
using UnityEngine.UI;

public class AnswersScript : MonoBehaviour
{
    public bool isCorrect = false;

    //Atsauce uz Manager skriptu (spēles main kods)
    public Manager manager;

    //Saglabāt sākotnējo pogas krāsu
    public Color krasa;

    //Awake tiek izsaukts pirms Start, kad objekts tiek inicializēts
    public void Awake()
    {
        //Saglabā sākotnējo Image komponenta krāsu
        krasa = GetComponent<Image>().color;
    }

    //Atjauno pogas krāsu uz sākotnējo (piemēram, kad ģenerē jaunu jautājumu)
    public void ResetColor()
    {
        GetComponent<Image>().color = krasa;
    }

    //Izsauc, kad lietotājs spiež uz atbildes pogas
    public void Answer()
    {
        if (isCorrect)
        {
            //Ja pareiza atbilde tad zaļa krāsa un izsauc Manager.Correct()
            GetComponent<Image>().color = Color.green;
            Debug.Log("Pareiza atbilde!");
            manager.Correct();
        }
        else
        {
            //Ja nepareiza atbilde = sarkana krāsa un izsauc Manager.Wrong()
            GetComponent<Image>().color = Color.red;
            Debug.Log("Nepareiza atbilde!");
            manager.Wrong();
        }
    }
}
