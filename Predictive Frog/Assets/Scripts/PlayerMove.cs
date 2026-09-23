using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMove : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private float upLimit;
    [SerializeField] private float downLimit;
    [SerializeField] private float horizontalLimit;

    private Keyboard keyboard;
    private Vector2 direction;


    void Awake()
    {
        keyboard = Keyboard.current;
    }

    void Update()
    {
        Move();
    }

    private void Move()
    {
        direction = Vector2.zero;

        if (keyboard.wKey.isPressed) direction.y += 1;
        if (keyboard.sKey.isPressed) direction.y -= 1;
        if (keyboard.aKey.isPressed) direction.x -= 1;
        if (keyboard.dKey.isPressed) direction.x += 1;

        direction.Normalize();

        transform.position += new Vector3(direction.x, direction.y, 0) * Time.deltaTime * speed;

        // Apply vertical limits
        if (transform.position.y > upLimit)
            transform.position = new Vector3(transform.position.x, upLimit, transform.position.z);
        if (transform.position.y < downLimit)
            transform.position = new Vector3(transform.position.x, downLimit, transform.position.z);

        // Apply horizontal limits
        if (transform.position.x > horizontalLimit)
            transform.position = new Vector3(horizontalLimit, transform.position.y, transform.position.z);
        if (transform.position.x < -horizontalLimit)
            transform.position = new Vector3(-horizontalLimit, transform.position.y, transform.position.z);
    }
}
