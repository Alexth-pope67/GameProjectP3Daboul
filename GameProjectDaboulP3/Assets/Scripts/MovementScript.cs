using UnityEngine;

public class MovementScript: MonoBehaviour
{
    public float speed = 5.0f;

    void Update()
    {
        // 1. Get raw keyboard/joystick input
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        // 2. Create a movement direction vector
        Vector3 direction = new Vector3(moveX, 0f, moveZ);

        // 3. Move the object directly through its transform component
        // Time.deltaTime ensures consistent speed on all computers
        transform.Translate(direction * speed * Time.deltaTime, Space.World);
    }
}
