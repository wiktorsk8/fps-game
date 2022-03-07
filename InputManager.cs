using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public float forwardMoveWS;
    public float sideMoveAD;

    public float mouseInputX;
    public float mouseInputY;

    public KeyCode jumpKey = KeyCode.Space;

    [HideInInspector]public Vector2 wasdInput;

    private void Update()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        

        forwardMoveWS = Input.GetAxisRaw("Vertical");
        sideMoveAD = Input.GetAxisRaw("Horizontal");

        wasdInput = new Vector2(x, y);

        mouseInputY = Input.GetAxis("Mouse Y");
        mouseInputX = Input.GetAxis("Mouse X");
    }
}
