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

        behaviorDisplay.text = state.ToString().ToUpper();
    }

    void IdleBehavior()
    {
        physics.velocity = physics.velocity * decelerationFactor;
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
    }

    void Awake()
    {
        physics = GetComponent<Rigidbody2D>();
        physics.isKinematic = true;
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
