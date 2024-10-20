using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterMovement : MonoBehaviour
{
    float timer = 0;
    void Update()
    {
        timer += Time.deltaTime;
        timer = timer % 360;
        transform.position = new Vector3(0, (Mathf.Sin(timer) * 0.5f) - 4.504591f, 0);
        transform.rotation = Quaternion.Euler(0, 0, Mathf.Sin(timer) * 1);
    }
}
