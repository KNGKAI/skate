using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//[ExecuteInEditMode]
public class Test : MonoBehaviour
{
    public Character character;
    public Skateboard skateboard;

    void Update()
    {
        Movement();
        Feet();
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

        leftFoot = new Vector2(Input.GetAxisRaw("Left X"), Input.GetAxisRaw("Left Y"));
        rightFoot = new Vector2(Input.GetAxisRaw("Right X"), Input.GetAxisRaw("Right Y"));
        if (leftFoot.magnitude > 0.5f && rightFoot.magnitude > 0.5f)
        {
            character.Olie();
            skateboard.Trick(-leftFoot.y, -rightFoot.y);
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
