using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{

    public float speed = 10;
    public float lifeTime = 0.5f;
    public float moveTime = 3f;

    public int damage = 1;

    // Start is called before the first frame update
    void Start()
    {
        //transform.Rotate(new Vector3(0, 90, 0));
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector3.up * speed * Time.deltaTime);
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

    }
}
