using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControls : MonoBehaviour
{

    public InputActionReference moveActionRef;
    public float speed = 10.0f;

    private void Start()
    {
        moveActionRef.action.Enable();
    }

    void Update()
    {
        Vector2 inputVector = moveActionRef.action.ReadValue<Vector2>();
        transform.position = new Vector3(
            Mathf.Clamp(transform.position.x + inputVector.x * speed * Time.deltaTime, -38, 38),
            transform.position.y,
            Mathf.Clamp(transform.position.z + inputVector.y * speed * Time.deltaTime, -38, 38)
        );
    }
}
