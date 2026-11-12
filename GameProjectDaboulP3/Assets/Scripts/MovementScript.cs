using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class MovementScript : MonoBehaviour
{
    public float speed = 5.0f;
    public float jumpHeight = 2.0f;
    public float gravity = -9.81f;

    private CharacterController characterController;
    private float verticalVelocity;
    private bool useArrowKeys;

    void Awake()
    {
        characterController = GetComponent<CharacterController>();
        useArrowKeys = CompareTag("Player2");
    }

    void Update()
    {
        Vector2 input = Vector2.zero;
        bool jumpPressed = false;

        if (Keyboard.current != null)
        {
            jumpPressed = useArrowKeys
                ? Keyboard.current.enterKey.wasPressedThisFrame
                : Keyboard.current.spaceKey.wasPressedThisFrame;

            if (useArrowKeys)
            {
                input = new Vector2(
                    (Keyboard.current.rightArrowKey.isPressed ? 1f : 0f) - (Keyboard.current.leftArrowKey.isPressed ? 1f : 0f),
                    (Keyboard.current.upArrowKey.isPressed ? 1f : 0f) - (Keyboard.current.downArrowKey.isPressed ? 1f : 0f));
            }
            else
            {
                input = new Vector2(
                    (Keyboard.current.dKey.isPressed ? 1f : 0f) - (Keyboard.current.aKey.isPressed ? 1f : 0f),
                    (Keyboard.current.wKey.isPressed ? 1f : 0f) - (Keyboard.current.sKey.isPressed ? 1f : 0f));
            }
        }

        Vector3 movement = new Vector3(input.x, 0f, input.y).normalized * speed;

        if (characterController.isGrounded)
        {
            verticalVelocity = -2f;

            if (jumpPressed)
            {
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
        }

        verticalVelocity += gravity * Time.deltaTime;
        movement.y = verticalVelocity;

        characterController.Move(movement * Time.deltaTime);
    }
}
