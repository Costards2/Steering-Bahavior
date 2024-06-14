using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] float maxSpeed = 6f;

    Rigidbody2D physics = null;

    void Update()
    {
        Vector3 input = new Vector3
        (
            x: Input.GetAxis("Horizontal"),
            y: Input.GetAxis("Vertical"),
            z: 0.0f
        );

        if (input == Vector3.zero)
        {
            var targetPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            targetPosition.z = 0f;
            input = targetPosition - transform.position;
        }

        physics.velocity = input.normalized * maxSpeed;
    }

    void Awake()
    {
        physics = GetComponent<Rigidbody2D>();
    }
}
