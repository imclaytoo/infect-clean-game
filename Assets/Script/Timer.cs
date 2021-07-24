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
            Time.timeScale = 0;
        }

    }
}
