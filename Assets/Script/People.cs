using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class People : MonoBehaviour
{
    [SerializeField] GameObject buttonAction;
    public GameObject player;
    private Transform playerPos;
    public float distance;
    public Sprite mask;

    public int score = 0;

    // Start is called before the first frame update
    void Start()
    {
        playerPos = player.GetComponent<Transform>();
        score = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (Vector2.Distance(transform.position, playerPos.position) < distance)
        {
            buttonAction.SetActive(true);
            //score += 1;
        }
        else
        {
            buttonAction.SetActive(false);
        }
    }

    public void ChangeMask(){
        this.gameObject.GetComponent<SpriteRenderer>().sprite = mask;
        Destroy(gameObject, 2f);
        
    }
}
