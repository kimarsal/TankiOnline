using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOnEnd : MonoBehaviour
{
    public float length;

    // Start is called before the first frame update
    void Awake()
    {
        StartCoroutine(explode());

    }

    public void StartTimer() 
    {
        StartCoroutine(explode());
    }

    IEnumerator explode()
    {
        yield return new WaitForSeconds(length);
        Object.Destroy(this.gameObject);

    }
}
