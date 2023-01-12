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
    public GameObject TankExplosion;
    public float speed=0.5f;
    public CircleCollider2D colliderMina;

    public Vector2 PreviousPosition;
    public Vector2 FuturePosition;

    private bool explotando;
    private CircleCollider2D areamort;
    private float time=0;
    private float ExplodingTime=0;
    private GameObject[] objetivos;

    void ChangeSprite(Sprite change)
    {   
        Debug.Log(change);
        spriteRenderer.sprite = change; 
    }

    void Start()
    {      
        explotando=false;
        objetivos=new GameObject[3];
        areamort = GetComponent<CircleCollider2D>();
        areamort.enabled=false;
        objetivos[0] = GameObject.Find("GreenPlayer/Tank");
        objetivos[1] = GameObject.Find("WhitePlayer/Tank");
        objetivos[2] = GameObject.Find("RedPlayer/Tank");
        animator = GetComponent<Animator>();
    }

    void Update()
    {   
        if(!explotando){
            float mindist=-1;
            GameObject mesProper=null;
            for(int i=0;i<3;i++){
                GameObject actual=objetivos[i];
                if(actual!=null){
                    float distancia=Vector3.Distance(actual.transform.position, transform.position);
                    if(mindist==-1 || distancia<mindist){
                        mindist=distancia;
                        mesProper=actual;
                    }
                }
            }
            if(mesProper!=null){
                var dir=(mesProper.transform.position - transform.position).normalized;
                transform.position += dir * speed * Time.deltaTime;
            }
        }
        
        if(time>1){
            if(spriteRenderer.sprite == Red){
                ChangeSprite(Green);
            }
            else if(spriteRenderer.sprite == Green){
                ChangeSprite(Red);
            }
            time=0;
        }
        if(ExplodingTime>4 && !explotando) {
            explota();
        }

        time+=Time.deltaTime;
        ExplodingTime+=Time.deltaTime;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        Debug.Log(col.gameObject.tag);
        if (col.gameObject.CompareTag("Player")){
            Destroy(col.transform.parent.gameObject);
            GameObject explo = Instantiate(TankExplosion, col.transform.position, Quaternion.identity);
        }
        else if (col.gameObject.CompareTag("Destruible")){
            Destroy(col.gameObject);
            GameObject explo = Instantiate(TankExplosion, col.transform.position, Quaternion.identity);
        }
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Player")){
            Destroy(col.transform.parent.gameObject);
            explota();
        }
    }

    public void explota()
    {
        explotando=true;
        areamort.enabled=true;
        Destroy(GetComponent<Rigidbody2D>());
        spriteRenderer.color = new Color(1f,1f,1f,0f);
        animator.SetBool("boom", true);
        StartCoroutine(explode());
    }

    private IEnumerator explode()
    {
        yield return new WaitForSeconds(1.1f);
        Destroy(Mina);

    }


}
