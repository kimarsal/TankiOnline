using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mine : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public GameObject Mina;
    public Sprite Green;
    public Sprite Red;
    public Animator animator;
    private float time=0;
    private float ExplodingTime=0;
    void ChangeSprite(Sprite change)
    {   
        Debug.Log(change);
        spriteRenderer.sprite = change; 
    }
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {   
        if(time>1){
            Debug.Log("a");
            if(spriteRenderer.sprite == Red){
            Debug.Log("b");
                ChangeSprite(Green);
            }
            else if(spriteRenderer.sprite == Green){
            Debug.Log("c");
                ChangeSprite(Red);
            }
            time=0;
        }
        if(ExplodingTime>4){
            spriteRenderer.color = new Color(1f,1f,1f,0f);
            animator.SetBool("boom", true);
            StartCoroutine(explode());
        }

        time+=Time.deltaTime;
        ExplodingTime+=Time.deltaTime;
    }

    IEnumerator explode()
        {

            yield return new WaitForSeconds(1.1f);
            Destroy(Mina);

        }


}
