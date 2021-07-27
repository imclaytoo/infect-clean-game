using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonLevel2 : MonoBehaviour
{

    public GameObject DesButton;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per fame

    void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.name.Equals("Player"))
        {
            DesButton.SetActive(true);
        }   
    }
   void OnTriggerExit2D(Collider2D other) {
        if (other.gameObject.name.Equals("Player"))
        {
            DesButton.SetActive(false);
        }    
   }
    
}
