using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotatooooor : MonoBehaviour
{
    public float speed = 10f;

    void Update()
    {
        this.transform.Rotate(Vector3.up * Time.unscaledDeltaTime * speed);
    }
}
