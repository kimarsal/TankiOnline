using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public Rigidbody2D rb;
    public float speed;
    public bool hasflare;
    public bool isTesting = false;
    public GameObject TankExplosion;
    public GameObject BulletExplosion;
    public GameObject bulletflare;
    private ServerScript serverScript;

    public Vector2 PreviousPosition;
    public Vector2 FuturePosition;
    private Vector3 lastVelocity;
    public float curSpeed;
    public Vector3 direction;
    public int MAX_BOUNCES = 3;
    public int nBounces;
    private Vector2 InitVel;
    private CircleCollider2D boxCollider;

    bool _setVelocity = false;

    void Start()
    {
        boxCollider = GetComponent<CircleCollider2D>();
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
        yield return new WaitForSeconds(0.1f);
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

            if (_setVelocity)
            {
                rb.velocity = InitVel * speed;
                rb.gravityScale = 0;

                _setVelocity = false;
            }
        }
    }

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
                Instantiate(TankExplosion, transform.position, Quaternion.identity);
                Destroy(gameObject);
            }
            else if (collision.gameObject.CompareTag("Bala"))
            {
                Instantiate(BulletExplosion, transform.position, Quaternion.identity);
                Destroy(gameObject);
            }
            else if (collision.gameObject.CompareTag("Mina"))
            {
                Instantiate(BulletExplosion, transform.position, Quaternion.identity);
                Destroy(gameObject);
                collision.gameObject.GetComponent<Mine>().explota();
            }
            else if (collision.gameObject.CompareTag("Destruible"))
            {
                Instantiate(BulletExplosion, transform.position, Quaternion.identity);
                Destroy(collision.gameObject);
                Destroy(gameObject);
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

                    var angle = Mathf.Atan2(rb.velocity.y, rb.velocity.x) * Mathf.Rad2Deg;
                    transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
                    rb.angularVelocity = 0;
                    nBounces++;
                }
                else
                {
                    serverScript.InstantiateExplosion(transform.position);
                    serverScript.BulletIsDestroyed(this);
                }
            }
            else if (collision.gameObject.CompareTag("Player"))
            {
                serverScript.InstantiateExplosion(transform.position);
                serverScript.TankIsDestroyed(collision.transform.parent.GetComponent<PlayerInput>().playerId);
                serverScript.BulletIsDestroyed(this);
            }
            else if (collision.gameObject.CompareTag("Bala"))
            {
                serverScript.InstantiateExplosion(transform.position);
                serverScript.BulletIsDestroyed(this);
            }
            else if (collision.gameObject.CompareTag("Destruible"))
            {
                serverScript.InstantiateExplosion(transform.position);
                serverScript.ObjectIsDestroyed(collision.gameObject.GetComponent<ObjetoDestruible>());
                serverScript.BulletIsDestroyed(this);
            }
        }
    }
}
