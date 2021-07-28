using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonLevel2 : MonoBehaviour
{
    public GameObject DesButton;

    public int score = 0;

    void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.name.Equals("Player"))
        {
            DesButton.SetActive(true);
            score = +1;
        }   
    }
   void OnTriggerExit2D(Collider2D other) {
        if (other.gameObject.name.Equals("Player"))
        {
            DesButton.SetActive(false);
        }    
   }
    
}
