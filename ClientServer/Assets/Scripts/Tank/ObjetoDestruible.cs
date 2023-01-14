using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjetoDestruible : MonoBehaviour
{

    SpriteRenderer spriteRenderer;
    public Sprite sprite;

    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprite;
    }
}
