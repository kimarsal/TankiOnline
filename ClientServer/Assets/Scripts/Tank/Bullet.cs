using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public bool isTesting = false;

    public float speed = 10;
    public float lifeTime = 0.5f;
    public float moveTime = 3f;

    public int damage = 1;

    private ServerScript serverScript;
    private ClientScript clientScript;

    public Vector2 PreviousPosition;
    public float PreviousAngle;
    public Vector2 FuturePosition;
    public float FutureAngle;

    private Rigidbody2D rb;
    private Vector3 lastVelocity;
    private float curSpeed;
    private Vector3 direction;
    private int nBounces;
    private Vector2 InitVel;

    bool _setVelocity = false;

    // Start is called before the first frame update
    void Start()
    {
        //transform.Rotate(new Vector3(0, 90, 0));
        
        //transform.Rotate(new Vector3(0, 90, 0));
        //InitVel = new Vector2(0, 1);

        rb = GetComponent<Rigidbody2D>();
        rb.angularVelocity = 0;

        GameObject[] array;// create an array
        array = GameObject.FindGameObjectsWithTag ( "Canvas" );// set the array to hold all GameObjects with the specified tag

        // check if there are any GameObjects (with the specified tag) spawned
        if ( array.Length == 0 )
        {
            isTesting = true;
            return;
            // There are no buttons, begin spawning buttons...
        }
       // GameObject canvas = GameObject.FindGameObjectWithTag("Canvas");

        /*if(canvas == null)
        {
        }*/

        serverScript = array[0].GetComponent<ServerScript>();
        if (serverScript != null)
        {
            serverScript.bulletList.Add(this);
            return;
        }
        
        clientScript = array[0].GetComponent<ClientScript>();
        if (clientScript != null)
        {
            clientScript.bulletList.Add(this);
        }
    }

    public void SetParams(Vector2 pos) 
    {
        InitVel = pos;
        Debug.Log("aaaaaa");
        _setVelocity = true;
    }


    void FixedUpdate()
    {
        if (isTesting)
        {
            Debug.Log(lastVelocity);
            lastVelocity = rb.velocity;
            //transform.Translate(Vector3.up * speed * Time.deltaTime);

            if (_setVelocity)
            {
                print(InitVel * speed);
                rb.velocity = InitVel * speed;
                rb.gravityScale = 0;

                _setVelocity = false;
            }
        }
    }

    /*public void SetPos(Transform canon)
    {

        
    }*/

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log(collision.gameObject.tag);

        if (collision.gameObject.CompareTag("Player")) 
        {
            Debug.Log("impacte");
            Destroy(collision.gameObject);
            Destroy(gameObject);
        }
        
        if(serverScript != null){
            serverScript.BulletIsDestroyed(this);
        } 
        else if(isTesting){
            Destroy(gameObject);
        }
        

    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (nBounces < 5)
        {
            curSpeed = lastVelocity.magnitude;
            direction = Vector3.Reflect(lastVelocity.normalized, collision.contacts[0].normal);

            rb.velocity = direction * Mathf.Max(curSpeed, 0);
            nBounces++;
        }
        else 
        {
            Destroy(gameObject);
        }
        print("Rebote");

        
        
    }
}
