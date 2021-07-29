
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scoring : MonoBehaviour
{
    // public GameObject scoreButton;

    public float score;

    void Start() {
        score = 0;   
    }

    // void OnTriggerEnter2D(Collider2D other) {
    //     if (other.gameObject.tag.Equals("People"))
    //     {
    //     scoreButton.SetActive(true);
    //     }    
    // }

    // void OnTriggerExit2D(Collider2D other) {
    //     if (other.gameObject.tag.Equals("People"))
    //     {
    //         scoreButton.SetActive(false);
    //     }    
    // }

    public void AddScore(){
        score += 1;
        Debug.Log(score);
    }
}
