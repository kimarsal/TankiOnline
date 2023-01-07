using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public bool isTesting = false;

    public float speed;
    public float lifeTime = 0.5f;
    public float moveTime = 3f;

    public int damage = 1;

    public bool hasflare;
    public GameObject bulletflare;
    private ServerScript serverScript;

    public Vector2 PreviousPosition;
    public float PreviousAngle;
    public Vector2 FuturePosition;
    public float FutureAngle;

    public Rigidbody2D rb;
    private Vector3 lastVelocity;
    public float curSpeed;
    public Vector3 direction;
    public int MAX_BOUNCES = 3;
    public int nBounces;
    private Vector2 InitVel;
    private BoxCollider2D boxCollider;

    bool _setVelocity = false;

    public GameObject TankExplosion;
    public GameObject BulletExplosion;

    void Start()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        boxCollider.enabled = false;
        StartCoroutine(EnableBoxCollider());

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

        Destroy(GetComponent<Rigidbody2D>());
    }

    private IEnumerator EnableBoxCollider()
    {
        yield return new WaitForSeconds(0.5f);
        if (isTesting || serverScript != null) boxCollider.enabled = true;
    }

    public void SetParams(Vector2 pos) 
    {
        InitVel = pos;
        _setVelocity = true;
    }


    void FixedUpdate()
    {
        if (isTesting || serverScript != null)
        {
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
        if(hasflare) {
            SpriteRenderer spriteR = bulletflare.GetComponent<SpriteRenderer>();
            spriteR.color = new Color(1f,1f,1f,0f);
        }
        if (isTesting)
        {
            if (collision.gameObject.CompareTag("Pared"))
            {
                if (nBounces < MAX_BOUNCES)
                {
                    curSpeed = lastVelocity.magnitude;
                    direction = Vector3.Reflect(lastVelocity.normalized, collision.contacts[0].normal);

                    rb.velocity = direction * Mathf.Max(curSpeed, 0);
                    
                    var angle = Mathf.Atan2(rb.velocity.y, rb.velocity.x) * Mathf.Rad2Deg;
                    Debug.Log(angle);
                    transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
                    rb.angularVelocity = 0;
                    nBounces++;
                }
                else
                {
                    Instantiate(BulletExplosion, transform.position, Quaternion.identity);
                    Destroy(gameObject);
                }
            }
            else if (collision.gameObject.CompareTag("Player"))
            {
                Destroy(collision.transform.parent.gameObject);
                //collision.GetComponent<PlayerCombat>().Die();
                GameObject explo = Instantiate(TankExplosion, transform.position, Quaternion.identity);
                
                Destroy(gameObject);
            }
            else 
            {
                rb.angularVelocity = 0;
            }
        }
        else
        {
            if (collision.gameObject.CompareTag("Pared"))
            {
                if (nBounces < MAX_BOUNCES)
                {
                    curSpeed = lastVelocity.magnitude;
                    direction = Vector3.Reflect(lastVelocity.normalized, collision.contacts[0].normal);

                    rb.velocity = direction * Mathf.Max(curSpeed, 0);
                    //rb.angularVelocity = 0;
                    //serverScript.BulletBounce(this);
                    nBounces++;
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
