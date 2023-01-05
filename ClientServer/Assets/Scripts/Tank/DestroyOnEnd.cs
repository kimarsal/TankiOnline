using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOnEnd : MonoBehaviour
{
    public float length;

    // Start is called before the first frame update
    void Awake()
    {
        Debug.Log("AAAA");
        StartCoroutine(explode());

    }

    public void StartTimer() 
    {
        Debug.Log("AAAA");
        StartCoroutine(explode());
    }

    IEnumerator explode()
    {
        Debug.Log("BBBBB");
        yield return new WaitForSeconds(length);
        Debug.Log("CCCC");
        Object.Destroy(this.gameObject);

    }
}
