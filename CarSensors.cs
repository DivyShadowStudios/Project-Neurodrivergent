
using UnityEngine;

public class CarSensors : MonoBehaviour
{
    [Header("Sensor Configuration")]
    public int sensorCount = 9;
    public float sensorRange = 12f;
    public float sensorSpread = 180f;

    [Header("Detection")]
    public LayerMask wallLayer;

    public float[] Readings { get; private set; }

    void Awake()
    {
        Readings = new float[sensorCount];
    }

    void FixedUpdate()
    {
        Scan();
    }

    public float[] Scan()
    {
        if (Readings == null || Readings.Length != sensorCount)
            Readings = new float[sensorCount];

        for (int i = 0; i < sensorCount; i++)
        {
            float angle = sensorCount == 1
                ? 0f
                : -sensorSpread * 0.5f +
                  sensorSpread * i / (sensorCount - 1);

            Vector2 direction =
                Quaternion.Euler(0, 0, angle) * transform.up;

            RaycastHit2D hit = Physics2D.Raycast(
                transform.position,
                direction,
                sensorRange,
                wallLayer
            );

            float distance = hit.collider != null
                ? hit.distance
                : sensorRange;

            Readings[i] = distance / sensorRange;

            Debug.DrawLine(
                transform.position,
                transform.position + (Vector3)direction * distance,
                hit.collider != null ? Color.red : Color.green
            );
        }

        return Readings;
    }
}
