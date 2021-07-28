using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Desinvektan : MonoBehaviour
{
    public Sprite after;

    public void ChangeImage(){
        this.gameObject.GetComponent<SpriteRenderer>().sprite = after;
        Destroy(gameObject, 2f);
    }
}
