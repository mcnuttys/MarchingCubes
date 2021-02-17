using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    private CharacterController cc;
    private Vector3 direction;

    public float movementSpeed = 3;
    public float jumpSpeed = 3;

    private float verticalVelocty;
    private float floatSpeed = 0;
    
    // Start is called before the first frame update
    void Start()
    {
        cc = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        direction = transform.rotation * new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        if (direction.magnitude > 1)
            direction = direction.normalized;

        if(Input.GetKey(KeyCode.Space) && !Input.GetKey(KeyCode.LeftShift))
        {
            verticalVelocty = floatSpeed;
        } else if(Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.Space))
        {
            verticalVelocty = -floatSpeed;
        } else if(Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.Space))
        {
            verticalVelocty = 0;
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
            floatSpeed++;
        if (Input.GetKeyDown(KeyCode.DownArrow))
            floatSpeed--;
        floatSpeed = Mathf.Clamp(floatSpeed, 0, 100);
    }

    void FixedUpdate()
    {
        Vector3 move = direction * movementSpeed * Time.deltaTime;

        if(!cc.isGrounded)
        {
            verticalVelocty += Physics.gravity.y * Time.deltaTime;
        }
        move.y = verticalVelocty * Time.deltaTime;

        cc.Move(move);
    }

    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(0, 0, Screen.width, Screen.height));
        GUILayout.BeginHorizontal();
        GUILayout.BeginVertical();

        GUILayout.Box($"Position: {transform.position} Ground Level ~32\nLeft Click to throw generic explosive grenade...\nRight Click to throw dirt grenade...\nSpace/Shift to me the player up and down\nVertical Speed: {floatSpeed} (Modify with Up/Down Arrows)\nFPS: {1f / Time.deltaTime}");

        GUILayout.EndVertical();
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.EndArea();
    }
}
