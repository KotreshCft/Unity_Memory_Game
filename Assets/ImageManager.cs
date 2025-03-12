using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ImageManager : MonoBehaviour, IPointerClickHandler
{
    // private Image ImageGameobject;

    public GameManager gameManager;
    // Start is called before the first frame update
    void Start()
    {
        //ImageGameobject = GetComponent<Image>();

        gameManager = FindObjectOfType<GameManager>();
    }


    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Hello");
    }

    // public void OnMouseDown()
    // {
    //     gameManager.gameObjectValidate(1);
    // }

    // Update is called once per frame
    void Update()
    {

    }
}
