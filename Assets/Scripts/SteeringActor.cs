using UnityEngine.UI;
using UnityEngine;

enum Behavior { Idle, Seek, Evade }
enum State { Idle, Arrive, Seek, Evade }

[RequireComponent(typeof(Rigidbody2D))]
public class SteeringActor : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] Behavior behavior = Behavior.Seek;
    [SerializeField] Transform target = null;
    [SerializeField] float maxSpeed = 4f;
    [SerializeField, Range(0.1f, 0.99f)] float decelerationFactor = 0.75f;
    [SerializeField] float arriveRadius = 1.2f;
    [SerializeField] float stopRadius = 0.5f;
    [SerializeField] float evadeRadius = 5f;
    [SerializeField] float avoidDistance = 2f;
    [SerializeField] float contourDistance = 2f;
    [SerializeField] float contourAngleStep = 10f;
    [SerializeField] LayerMask wallLayer;

    Text behaviorDisplay = null;
    Rigidbody2D physics;
    State state = State.Idle;

    void FixedUpdate()
    {
        if (target != null)
        {
            switch (behavior)
            {
                case Behavior.Idle: IdleBehavior(); break;
                case Behavior.Seek: SeekBehavior(); break;
                case Behavior.Evade: EvadeBehavior(); break;
            }
        }

        physics.velocity = Vector2.ClampMagnitude(physics.velocity, maxSpeed);

        if (behaviorDisplay != null)
        {
            behaviorDisplay.text = state.ToString().ToUpper();
        }
    }

    void IdleBehavior()
    {
        physics.velocity *= decelerationFactor;
    }

    void SeekBehavior()
    {
        Vector2 delta = target.position - transform.position;
        Vector2 steering = delta.normalized * maxSpeed - physics.velocity;
        float distance = delta.magnitude;

        if (distance < stopRadius)
        {
            state = State.Idle;
        }
        else if (distance < arriveRadius)
        {
            state = State.Arrive;
        }
        else
        {
            state = State.Seek;
        }

        switch (state)
        {
            case State.Idle:
                IdleBehavior();
                break;
            case State.Arrive:
                var arriveFactor = 0.01f + (distance - stopRadius) / (arriveRadius - stopRadius);
                physics.velocity += arriveFactor * steering * Time.fixedDeltaTime;
                break;
            case State.Seek:
                physics.velocity += steering * Time.fixedDeltaTime;
                break;
        }

        // Chamada do método para contornar paredes
        ContourWalls();
    }

    void EvadeBehavior()
    {
        Vector2 delta = target.position - transform.position;
        Vector2 steering = delta.normalized * maxSpeed - physics.velocity;
        float distance = delta.magnitude;

        if (distance > evadeRadius)
        {
            state = State.Idle;
        }
        else
        {
            state = State.Evade;
        }

        switch (state)
        {
            case State.Idle:
                IdleBehavior();
                break;
            case State.Evade:
                physics.velocity -= steering * Time.fixedDeltaTime;
                break;
        }

        // Chamada do método para contornar paredes
        ContourWalls();
    }

    void ContourWalls()
    {
        // Direção de movimento atual
        Vector2 moveDirection = physics.velocity.normalized;

        // Direções de contorno
        Vector2 rightContour = Quaternion.AngleAxis(-contourAngleStep, Vector3.forward) * moveDirection;
        Vector2 leftContour = Quaternion.AngleAxis(contourAngleStep, Vector3.forward) * moveDirection;

        // Raycasts para detectar paredes nas direções de contorno
        RaycastHit2D hitRight = Physics2D.Raycast(transform.position, rightContour, contourDistance, wallLayer);
        RaycastHit2D hitLeft = Physics2D.Raycast(transform.position, leftContour, contourDistance, wallLayer);

        if (hitRight.collider != null && hitLeft.collider != null)
        {
            // Ambas as direções estão bloqueadas
            // Escolher a melhor direção possível (podemos adicionar lógica mais sofisticada aqui)
            Vector2 directionAway = (hitRight.point - hitLeft.point).normalized;
            physics.velocity = directionAway * maxSpeed;
        }
        else if (hitRight.collider != null)
        {
            // Direção de contorno direita está bloqueada
            physics.velocity = leftContour.normalized * maxSpeed;
        }
        else if (hitLeft.collider != null)
        {
            // Direção de contorno esquerda está bloqueada
            physics.velocity = rightContour.normalized * maxSpeed;
        }
        // Se ambas as direções de contorno estiverem livres, o agente continua na direção atual
    }

    void Awake()
    {
        physics = GetComponent<Rigidbody2D>();
        physics.isKinematic = false;  // Set isKinematic to false to enable physics-based collision
        behaviorDisplay = GetComponentInChildren<Text>();
    }

    void OnDrawGizmos()
    {
        if (target == null)
        {
            return;
        }

        switch (behavior)
        {
            case Behavior.Idle:
                break;
            case Behavior.Seek:
                Gizmos.color = Color.white;
                Gizmos.DrawWireSphere(transform.position, arriveRadius);
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(transform.position, stopRadius);
                break;
            case Behavior.Evade:
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, evadeRadius);
                break;
        }

        Gizmos.color = Color.gray;
        Gizmos.DrawLine(transform.position, target.position);
    }
}
