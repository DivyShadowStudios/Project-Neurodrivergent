
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class CarController : MonoBehaviour
{
    [Header("Driving Physics")]
    public float acceleration = 12f;
    public float braking = 20f;
    public float maxSpeed = 18f;
    public float turnSpeed = 180f;
    public float grip = 8f;

    [Header("Testing")]
    public bool manualTesting = true;

    Rigidbody2D rb;

    public float Throttle { get; private set; }
    public float Steering { get; private set; }

    public bool Crashed { get; private set; }

    public float Speed => rb.linearVelocity.magnitude;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = false;
    }

    void FixedUpdate()
    {
        if (Crashed)
            return;

        float throttle = Throttle;
        float steering = Steering;

        // Temporary keyboard testing.
        // W = throttle, S = brake.
        // A = left, D = right.

        if (manualTesting)
        {
            float vertical = Input.GetAxisRaw("Vertical");
            float horizontal = Input.GetAxisRaw("Horizontal");

            throttle = vertical;
            steering = horizontal;
        }

        ApplyDriving(throttle, steering);
    }

    public void SetControls(float throttle, float steering)
    {
        Throttle = Mathf.Clamp(throttle, -1f, 1f);
        Steering = Mathf.Clamp(steering, -1f, 1f);
    }

    void ApplyDriving(float throttle, float steering)
    {
        Vector2 forward = transform.up;
        Vector2 right = transform.right;

        float forwardSpeed =
            Vector2.Dot(rb.linearVelocity, forward);

        // Acceleration
        if (throttle > 0f)
        {
            rb.AddForce(
                forward * throttle * acceleration,
                ForceMode2D.Force
            );
        }
        // Braking
        else if (throttle < 0f)
        {
            float brakeForce = -throttle * braking;

            if (forwardSpeed > 0.1f)
            {
                rb.AddForce(
                    -forward * brakeForce,
                    ForceMode2D.Force
                );
            }
            else
            {
                rb.AddForce(
                    -rb.linearVelocity.normalized * brakeForce,
                    ForceMode2D.Force
                );
            }
        }

        // Steering becomes more effective with speed.
        float speedFactor = Mathf.Clamp01(
            rb.linearVelocity.magnitude / 3f
        );

        float rotation = -steering * turnSpeed *
                         speedFactor * Time.fixedDeltaTime;

        rb.MoveRotation(rb.rotation + rotation);

        // Reduce sideways sliding.
        Vector2 lateralVelocity =
            right * Vector2.Dot(rb.linearVelocity, right);

        rb.AddForce(
            -lateralVelocity * grip,
            ForceMode2D.Force
        );

        // Maximum speed.
        rb.linearVelocity = Vector2.ClampMagnitude(
            rb.linearVelocity,
            maxSpeed
        );
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.gameObject.layer ==
            LayerMask.NameToLayer("TrackWall"))
        {
            Crashed = true;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }
}
