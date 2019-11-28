using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    public Character character;
    public Skateboard skateboard;

    Vector3 position;
    Quaternion rotation;

    void Start()
    {
        position = character.transform.position;
        rotation = character.transform.rotation;
    }

    void Update()
    {
        Movement();
        Feet();

        if (Input.GetKeyDown(KeyCode.R))
        {
            character.transform.position = position;
            character.transform.rotation = rotation;
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            position = character.transform.position;
            rotation = character.transform.rotation;
        }
    }

    void OnGUI()
    {
        GUI.Label(new Rect(0.0f, 0.0f, 500.0f, 500.0f), character.State.ToString());
        GUI.Label(new Rect(0.0f, 30.0f, 500.0f, 500.0f), character.Speed.ToString());
    }

    void Movement()
    {
        if (Input.GetButtonDown("Push"))
        {
            character.Push();
        }
        if (Input.GetButton("Brake"))
        {
            character.Brake();
        }
        if (Input.GetButton("Left"))
        {
            character.Rotate(false);
        }
        else if (Input.GetButton("Right"))
        {
            character.Rotate(true);
        }
    }

    void Feet()
    {
        Vector2 leftFoot;
        Vector2 rightFoot;

        leftFoot = new Vector2(Input.GetAxis("Left X"), Input.GetAxis("Left Y"));
        rightFoot = new Vector2(Input.GetAxis("Right X"), Input.GetAxis("Right Y"));
        if (leftFoot.magnitude > 0.1f && rightFoot.magnitude > 0.1f)
        {
            character.Olie();
            skateboard.Trick(Mathf.Ceil(-leftFoot.y), Mathf.Ceil(-rightFoot.y));
        }
        if (character.Grounded)
        {
            if (skateboard.Tricking)
            {
                skateboard.Catch();
            }
        }
        else
        {
            if (Input.GetButtonDown("Push"))
            {
                skateboard.Catch();
            }
        }
    }
}
