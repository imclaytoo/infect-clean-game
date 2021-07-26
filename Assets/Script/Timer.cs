using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    float timer = 0f;
    public float startingTime = 10f;
    public GameObject canvas;
    [SerializeField] Text countDown;
    public GameObject Pemain;
    public GameObject Orang;
    public GameObject Orang1;
    public GameObject Orang2;
    public GameObject Orang3;
    public GameObject Orang4;
    public GameObject Orang5;
    public GameObject Joycon;


    // Start is called before the first frame update
    void Start()
    {
        timer = startingTime;
        canvas.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        timer -= 1 *Time.deltaTime;
        countDown.text = timer.ToString("0");

        if (timer <= 0)
        {
            timer = 0;
            canvas.SetActive(true);
            Pemain.SetActive(false);
            Orang.SetActive(false);
            Orang1.SetActive(false);
            Orang2.SetActive(false);
            Orang3.SetActive(false);
            Orang4.SetActive(false);
            Orang5.SetActive(false);
            Joycon.SetActive(false);
            Time.timeScale = 0;
        }

    }
}
