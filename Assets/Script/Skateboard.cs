using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skateboard : MonoBehaviour
{
    public float rotate;

    private float trickX;
    private float trickY;

    private bool tricking;

    public bool Tricking
    {
        get
        {
            return (tricking);
        }
    }

    public static float Length
    {
        get
        {
            return (0.8f);
        }
    }

    private void Awake()
    {
        this.tricking = false;
    }

    private void FixedUpdate()
    {
        if (this.tricking)
        {
            float x;
            float y;

            x = this.trickX * Time.fixedDeltaTime * rotate;
            y = this.trickY * Time.fixedDeltaTime * rotate;
            this.transform.Rotate(0.0f, 0.0f, x, Space.Self);
            this.transform.Rotate(0.0f, y, 0.0f, Space.Self);
        }
    }

    public void Draw()
    {
        Vector3 direction;
        Vector3 up;
        Vector3 a;
        Vector3 b;
        Vector3 c;
        Vector3 d;

        direction = this.transform.forward.normalized * Skateboard.Length * 0.5f;
        up = this.transform.up.normalized * Skateboard.Length * 0.5f;
        a = this.transform.position + direction + up;
        b = this.transform.position - direction + up;
        c = this.transform.position + direction - up;
        d = this.transform.position - direction - up;
        Gizmos.DrawLine(a, b);
        Gizmos.DrawLine(b, d);
        Gizmos.DrawLine(c, d);
        Gizmos.DrawLine(c, a);
    }

    public Vector3 Slope()
    {
        Vector3 direction;
        Vector3 up;
        Vector3 slope;
        Vector3 a;
        Vector3 b;
        float radius;

        radius = Skateboard.Length * 0.5f;
        direction = this.transform.forward.normalized * radius;
        up = this.transform.up.normalized * radius;
        a = this.transform.position + direction + up;
        b = this.transform.position - direction + up;
        if (Physics.Raycast(a, -this.transform.up, out RaycastHit hitA, radius * 2.0f, Character.Mask))
        {
            a = hitA.point;
        }
        if (Physics.Raycast(b, -this.transform.up, out RaycastHit hitB, radius * 2.0f, Character.Mask))
        {
            b = hitB.point;
        }
        slope = a - b;
        return (slope);
    }

    public void Forward(Vector3 forward, Vector3 upward)
    {
        this.transform.rotation = Quaternion.LookRotation(forward, upward);
    }

    public void Level()
    {
        this.transform.rotation = Quaternion.LookRotation(Slope(), transform.up);
    }

    public void Trick(float x, float y)
    {
        this.tricking = true;
        this.trickX = Mathf.Clamp(x, -1.0f, 1.0f);
        this.trickY = Mathf.Clamp(y, -1.0f, 1.0f);
    }

    public void Catch()
    {
        this.tricking = false;
        this.transform.localRotation = Quaternion.identity;
    }
}
