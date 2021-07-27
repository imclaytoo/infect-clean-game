using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Desinvektan : MonoBehaviour
{

    public Sprite after;
    // Start is called before the first frame update

    public void ChangeImage(){
        this.gameObject.GetComponent<SpriteRenderer>().sprite = after;
        Destroy(gameObject, 2f);
    }
}
