using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    Vector2 horizontalInput;

    [SerializeField] CharacterController charController;
    [SerializeField] float speed = 10f;

    [SerializeField] float gravity = -30;
    Vector3 verticalVelocity = Vector3.zero;

    [SerializeField] LayerMask groundMask;
    bool isGrounded;

    public void ReceiveInput (Vector2 _horizontalInput)
    {
        horizontalInput = _horizontalInput;

        //Debug.Log(horizontalInput);
    }

    private void Update()
    {
        isGrounded = Physics.CheckSphere(transform.position, 0.1f, groundMask);
        if (isGrounded == true)     // checks if player is on the ground
        {
            verticalVelocity.y = 0; // if true then reset verticalVelocity so player will fall at correct speed again
        }

        Vector3 horizontalVelocity = (transform.right * horizontalInput.x + transform.forward * horizontalInput.y) * speed;
        charController.Move(horizontalVelocity * Time.deltaTime); // this moves the player at a consistent speed regardless of framerate

        verticalVelocity.y = +gravity * Time.deltaTime;
        charController.Move(verticalVelocity * Time.deltaTime);
    }
}
