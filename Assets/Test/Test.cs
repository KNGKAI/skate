using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//[ExecuteInEditMode]
public class Test : MonoBehaviour
{
    public Character character;

    void Update()
    {
        //character.Refresh();
        character.Rotate(Input.GetAxis("Horizontal"));
        if (Input.GetKeyDown(KeyCode.W))
        {
            character.Push();
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            character.Jump();
        }
    }
}
