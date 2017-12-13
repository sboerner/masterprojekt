using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class PlayerMovement : MonoBehaviour
{
    //private Rigidbody rb;
    public float speed = 5;
    private CharacterController controller;
    private float verticalVelocity;

    void Awake()
    {
        //rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        controller = GetComponent<CharacterController>();
    }

#if UNITY_ANDROID

    void Update()
    {
        //float hInput = CrossPlatformInputManager.GetAxis("Horizontal") * speed;
        //float vInput = CrossPlatformInputManager.GetAxis("Vertical") * speed;

        //rb.AddForce(hInput, 0, vInput);

        Vector3 inputs = Vector3.zero;

        inputs.x = CrossPlatformInputManager.GetAxis("Horizontal");
        inputs.z = CrossPlatformInputManager.GetAxis("Vertical");

        if (controller.isGrounded)
        {
            verticalVelocity = -1;

            if (CrossPlatformInputManager.GetButton("Jump"))
            {
                verticalVelocity = 10;
            }
        }
        else
        {
            verticalVelocity -= 30.0f * Time.deltaTime;
        }

        inputs.y = verticalVelocity;

        controller.Move(inputs * Time.deltaTime * speed);
    }


#else
    void Update()
        {
            var x = Input.GetAxis("Horizontal") * Time.deltaTime * 150.0f;
            var z = Input.GetAxis("Vertical") * Time.deltaTime * 3.0f;

            transform.Rotate(0, x, 0);
            transform.Translate(0, 0, z);
        }
#endif

}
