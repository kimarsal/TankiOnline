using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{

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
        serverScript = canvas.GetComponent<ServerScript>();
        if(serverScript == null)
        {
            clientScript = canvas.GetComponent<ClientScript>();
        }
        else
        {
            serverScript.bulletList.Add(this);
        }
    }

    // Update is called once per frame
    void Update()
    {
        //transform.Translate(Vector3.up * speed * Time.deltaTime);
    }

    /*public void SetPos(Transform canon)
    {

        
    }*/

    void OnTriggerEnter(Collider other)
    {
        print("Impact");

        if (other.gameObject.CompareTag("Player")) 
        {
            Destroy(other.gameObject);
        }
        
        if(serverScript == null) clientScript.bulletList.Remove(this);
        else serverScript.bulletList.Remove(this);
        Destroy(gameObject);

    }
}
