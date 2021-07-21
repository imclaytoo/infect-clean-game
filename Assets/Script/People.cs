using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class People : MonoBehaviour
{
    [SerializeField] GameObject buttonAction;
    public GameObject player;
    private Transform playerPos;
    public float distance;
    public Sprite mask;

    // Start is called before the first frame update
    void Start()
    {
        playerPos = player.GetComponent<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Vector2.Distance(transform.position, playerPos.position) < distance)
        {
            buttonAction.SetActive(true);
            
        }
        else{
            buttonAction.SetActive(false);
        }
    }

    public void ChangeMask(){
        this.gameObject.GetComponent<SpriteRenderer>().sprite = mask;
    }
}
