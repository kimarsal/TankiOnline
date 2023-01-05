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

    public Vector2 PreviousPosition;
    public float PreviousAngle;
    public Vector2 FuturePosition;
    public float FutureAngle;

    public Rigidbody2D rb;
    private Vector3 lastVelocity;
    public float curSpeed;
    public Vector3 direction;
    private int nBounces;
    private Vector2 InitVel;

    bool _setVelocity = false;

    void Start()
    {
        //transform.Rotate(new Vector3(0, 90, 0));
        
        //transform.Rotate(new Vector3(0, 90, 0));
        //InitVel = new Vector2(0, 1);

        rb = GetComponent<Rigidbody2D>();
        rb.angularVelocity = 0;

        GameObject canvas = GameObject.FindGameObjectWithTag("Canvas");
        if (canvas == null)
        {
            isTesting = true;
            return;
        }

        serverScript = canvas.GetComponent<ServerScript>();
        if (serverScript != null)
        {
            serverScript.AddBullet(this);
            return;
        }

        canvas.GetComponent<ClientScript>().AddBullet(this);
        GetComponent<BoxCollider2D>().enabled = false;
    }

    public void SetParams(Vector2 pos) 
    {
        InitVel = pos;
        Debug.Log("aaaaaa");
        _setVelocity = true;
    }


    void FixedUpdate()
    {
        if (isTesting || serverScript != null)
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

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isTesting)
        {
            if (collision.gameObject.CompareTag("Pared"))
            {
                if (nBounces < 5)
                {
                    curSpeed = lastVelocity.magnitude;
                    direction = Vector3.Reflect(lastVelocity.normalized, collision.contacts[0].normal);

                    rb.velocity = direction * Mathf.Max(curSpeed, 0);
                    nBounces++;
                    print("Rebote");
                }
                else
                {
                    Destroy(gameObject);
                }
            }
            else if (collision.gameObject.CompareTag("Player"))
            {
                Destroy(collision.transform.parent.gameObject);
                Destroy(gameObject);
            }
        }
        else
        {
            if (collision.gameObject.CompareTag("Pared"))
            {
                if (nBounces < 5)
                {
                    curSpeed = lastVelocity.magnitude;
                    direction = Vector3.Reflect(lastVelocity.normalized, collision.contacts[0].normal);

                    rb.velocity = direction * Mathf.Max(curSpeed, 0);
                    //serverScript.BulletBounce(this);
                    nBounces++;
                    print("Rebote");
                }
                else
                {
                    serverScript.BulletIsDestroyed(this);
                }
                //serverScript.BulletIsDestroyed(this);
            }
            else if (collision.gameObject.CompareTag("Player"))
            {
                serverScript.TankIsDestroyed(collision.transform.parent.GetComponent<PlayerInput>().playerId);
                serverScript.BulletIsDestroyed(this);
            }
        }


    }
}
