using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mine : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public Sprite Green;
    public Sprite Red;
    public GameObject TankExplosion;
    public float speed = 0.5f;
    public CircleCollider2D colliderMina;
    public CircleCollider2D areaMort;
    private bool isTesting = false;

    private Animator animator;
    private bool explotando;
    private float time = 0;
    private float ExplodingTime = 0;
    private List<GameObject> objetivos;

    public Vector2 PreviousPosition;
    public Vector2 FuturePosition;
    private ServerScript serverScript;
    private ClientScript clientScript;

    void Start()
    {
        animator = GetComponent<Animator>();

        GameObject canvas = GameObject.FindGameObjectWithTag("Canvas");
        if (canvas == null)
        {
            isTesting = true;
        }
        else
        {
            serverScript = canvas.GetComponent<ServerScript>();
            if (serverScript != null)
            {
                serverScript.AddMine(this);
            }
            else
            {
                clientScript = canvas.GetComponent<ClientScript>();
                return;
            }
        }

        explotando = false;
        objetivos = new List<GameObject>();
        foreach(GameObject objetivo in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (!objetivo.transform.parent.name.Contains("Blue"))
            {
                objetivos.Add(objetivo);
            }
        }

    }

    void Update()
    {
        if(isTesting || serverScript != null)
        {
            MoveMine();
        }

        if (time > 1)
        {
            if (spriteRenderer.sprite == Red)
            {
                ChangeSprite(Green);
            }
            else if (spriteRenderer.sprite == Green)
            {
                ChangeSprite(Red);
            }
            time = 0;
        }

        time += Time.deltaTime;
        ExplodingTime += Time.deltaTime;
    }

    private void MoveMine()
    {
        if (!explotando)
        {
            float mindist = -1;
            GameObject mesProper = null;
            for (int i = 0; i < objetivos.Count; i++)
            {
                GameObject actual = objetivos[i];
                if (actual != null)
                {
                    float distancia = Vector3.Distance(actual.transform.position, transform.position);
                    if (mindist == -1 || distancia < mindist)
                    {
                        mindist = distancia;
                        mesProper = actual;
                    }
                }
            }
            if (mesProper != null)
            {
                var dir = (mesProper.transform.position - transform.position).normalized;
                transform.position += dir * speed * Time.deltaTime;
            }
        }

        if (ExplodingTime > 4 && !explotando)
        {
            explota();
        }
    }

    void ChangeSprite(Sprite change)
    {
        spriteRenderer.sprite = change;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (isTesting)
        {
            if (col.gameObject.CompareTag("Player"))
            {
                Destroy(col.transform.parent.gameObject);
                Instantiate(TankExplosion, col.transform.position, Quaternion.identity);
            }
            else if (col.gameObject.CompareTag("Destruible"))
            {
                Destroy(col.gameObject);
                Instantiate(TankExplosion, col.transform.position, Quaternion.identity);
            }
        }
        else
        {
            if (col.gameObject.CompareTag("Player"))
            {
                serverScript.InstantiateExplosion(col.transform.position);
                serverScript.TankIsDestroyed(col.transform.parent.GetComponent<PlayerInput>().playerId);
            }
            else if (col.gameObject.CompareTag("Bala"))
            {
                serverScript.InstantiateExplosion(transform.position);
                serverScript.BulletIsDestroyed(col.gameObject.GetComponent<Bullet>());
            }
            else if (col.gameObject.CompareTag("Destruible"))
            {
                serverScript.InstantiateExplosion(transform.position);
                serverScript.ObjectIsDestroyed(col.gameObject.GetComponent<ObjetoDestruible>());
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Player"))
        {
            Destroy(col.transform.parent.gameObject);
            explota();
        }
    }

    public void explota()
    {
        if(serverScript != null)
        {
            serverScript.MineIsDestroyed(this);
        }

        explotando = true;
        areaMort.enabled = true;
        colliderMina.enabled = false;
        GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
        SetExplosion();
    }

    public void SetExplosion()
    {
        if(serverScript == null)
        {
            GetComponent<AudioSource>().Play();
        }
        spriteRenderer.color = new Color(1f, 1f, 1f, 0f);
        animator.SetBool("boom", true);
        StartCoroutine(explode());
    }

    private IEnumerator explode()
    {
        yield return new WaitForSeconds(1.1f);
        if(serverScript != null)
        {
            serverScript.RemoveMine(this);
        }
        else if(clientScript != null)
        {
            clientScript.RemoveMine(this);
        }
        Destroy(gameObject);
    }


}
