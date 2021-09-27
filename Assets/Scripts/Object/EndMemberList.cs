using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndMemberList : MonoBehaviour
{
    public float speed;
    public float maxY;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Run());
    }

    public IEnumerator Run()
    {
        while (transform.position.y < maxY)
        {
            transform.position += Vector3.up * speed * Time.deltaTime;
            yield return null;
        }
    }
}
