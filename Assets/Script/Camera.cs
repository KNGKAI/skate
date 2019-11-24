using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera : MonoBehaviour
{
    public Character target;
    public float distance;
    public float height;
    Vector3 offset;

    void Update()
    {
        if (target.Grounded)
        {
            offset = -target.transform.forward;
            //offset.y = 0;
            offset = offset.normalized * distance;
            offset.y = height;
        }
        transform.position = Vector3.Lerp(transform.position, target.transform.position + offset, 0.03f);
        transform.LookAt(target.transform);
    }
}
