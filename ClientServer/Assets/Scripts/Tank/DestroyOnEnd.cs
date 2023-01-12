using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOnEnd : MonoBehaviour
{
    void Awake()
    {
        StartCoroutine(explode());
    }

    IEnumerator explode()
    {
        yield return new WaitForSeconds(GetComponent<AudioSource>().clip.length);
        Destroy(this.gameObject);

    }
}
