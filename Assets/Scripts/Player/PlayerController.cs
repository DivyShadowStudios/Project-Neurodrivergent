
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Dash")]
    [SerializeField] private float dashSpeed = 15f;
    [SerializeField] private float dashDuration = 0.15f;
    [SerializeField] private float dashCooldown = 1f;

    [Header("Dash Visuals")]
    [SerializeField] private GameObject afterimagePrefab;
    [SerializeField] private Color afterimageColor = new Color(0.2f, 0.8f, 1f, 0.5f);
    [SerializeField] private float afterimageInterval = 0.04f;

    [Header("References")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private Vector2 movement;
    private Vector2 dashDirection;

    private bool isDashing;
    private bool canDash = true;

    private void Awake()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        HandleMovementInput();
        AimAtMouse();
        HandleDashInput();
    }

    private void FixedUpdate()
    {
        if (isDashing)
        {
            rb.linearVelocity = dashDirection * dashSpeed;
        }
        else
        {
            rb.linearVelocity = movement * moveSpeed;
        }
    }

    private void HandleMovementInput()
    {
        if (isDashing)
            return;

        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        movement = movement.normalized;
    }

    private void AimAtMouse()
    {
        if (Camera.main == null)
            return;

        Vector3 mousePosition =
            Camera.main.ScreenToWorldPoint(Input.mousePosition);

        Vector2 direction = mousePosition - transform.position;

        if (direction.sqrMagnitude < 0.001f)
            return;

        float angle = Mathf.Atan2(direction.y, direction.x)
                     * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
    }

    private void HandleDashInput()
    {
        if (Input.GetKeyDown(KeyCode.Space) && canDash)
        {
            StartCoroutine(Dash());
        }
    }

    private IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;
        StartCoroutine(SpawnAfterimages());

        dashDirection = movement;

        // If standing still, dash toward the mouse
        if (dashDirection.sqrMagnitude < 0.01f)
        {
            Vector3 mousePosition =
                Camera.main.ScreenToWorldPoint(Input.mousePosition);

            dashDirection = (mousePosition - transform.position).normalized;
        }

        rb.linearVelocity = dashDirection * dashSpeed;

        yield return new WaitForSeconds(dashDuration);

        isDashing = false;

        rb.linearVelocity = Vector2.zero;

        yield return new WaitForSeconds(dashCooldown);

        canDash = true;
    }

    private IEnumerator SpawnAfterimages()
    {
        while (isDashing)
        {
            GameObject image = Instantiate(afterimagePrefab, transform.position, transform.rotation);

            DashAfterimage afterimage = image.GetComponent<DashAfterimage>();

            if (afterimage != null)
            {
                afterimage.Initialize(spriteRenderer.sprite, afterimageColor);
            }

            yield return new WaitForSeconds(afterimageInterval);
        }
    }
}
