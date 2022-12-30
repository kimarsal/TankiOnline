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

    // Start is called before the first frame update
    void Start()
    {
        //transform.Rotate(new Vector3(0, 90, 0));

        GameObject canvas = GameObject.FindGameObjectWithTag("Canvas");
        if(canvas == null)
        {
            isTesting = true;
            return;
        }

        serverScript = canvas.GetComponent<ServerScript>();
        if (serverScript != null)
        {
            serverScript.bulletList.Add(this);
            return;
        }
        
        clientScript = canvas.GetComponent<ClientScript>();
        if (clientScript != null)
        {
            clientScript.bulletList.Add(this);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(isTesting) transform.Translate(Vector3.up * speed * Time.deltaTime);
    }

    /*public void SetPos(Transform canon)
    {

        
    }*/

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log(collision.gameObject.tag);

        if (collision.gameObject.CompareTag("Player")) 
        {
            Destroy(collision.gameObject);
        }
        
        if(serverScript != null) serverScript.bulletList.Remove(this);
        else if(clientScript != null) clientScript.bulletList.Remove(this);
        Destroy(gameObject);

    }
}
