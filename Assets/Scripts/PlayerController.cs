using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed;
    public Rigidbody2D rb;
    public Animator anim;

    private Vector2 moveDirection;
    private Vector2 lastMoveDirection = Vector2.down; // Default arah bawah
    private HotbarController hotbar;
    private string activeTool = "";
    private bool isPlowing;
    private bool isWatering;

    [System.Obsolete]
    void Awake()
    {
        hotbar = FindObjectOfType<HotbarController>();
        if (hotbar == null)
        {
            Debug.LogError("HotbarController not found in scene!");
        }
    }

    void Update()
    {
        ProcessInputs();
        Animate();

        // Input untuk plowing dan watering (hanya animasi)
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log($"Mouse clicked! Active Tool: {activeTool}, IsPlowing: {isPlowing}, IsWatering: {isWatering}");
            if (activeTool == "Hoe")
            {
                Debug.Log("Hoe detected, starting plowing...");
                StartPlowing();
            }
            else if (activeTool == "WateringCan")
            {
                Debug.Log("WateringCan detected, starting watering...");
                StartWatering();
            }
        }
    }

    void FixedUpdate()
    {
        Move();
    }

    void ProcessInputs()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        moveDirection = new Vector2(moveX, moveY).normalized;

        if (moveDirection != Vector2.zero)
        {
            lastMoveDirection = moveDirection;
        }
    }

    void Move()
    {
        rb.linearVelocity = new Vector2(moveDirection.x * moveSpeed, moveDirection.y * moveSpeed);
    }

    void Animate()
    {
        if (moveDirection != Vector2.zero)
        {
            anim.SetFloat("Xinput", moveDirection.x);
            anim.SetFloat("Yinput", moveDirection.y);
        }

        anim.SetFloat("animMovMagnitude", moveDirection.magnitude);
        anim.SetFloat("LastX", lastMoveDirection.x);
        anim.SetFloat("LastY", lastMoveDirection.y);
    }

    public void SetActiveTool(string toolName)
    {
        activeTool = toolName;
        Debug.Log($"Active tool set to: {activeTool}");
    }

    private void StartPlowing()
    {
        if (!isPlowing) // Pastikan tidak overlap
        {
            isPlowing = true;
            anim.SetTrigger("IsPlowing");
            Debug.Log("Plowing animation triggered!");
            StartCoroutine(StopPlowingAfterDelay(0.5f)); // Durasi animasi (sesuaikan dengan panjang animasi)
        }
    }

    private void StartWatering()
    {
        if (!isWatering) // Pastikan tidak overlap
        {
            isWatering = true;
            anim.SetTrigger("IsWatering");
            Debug.Log("Watering animation triggered!");
            StartCoroutine(StopWateringAfterDelay(0.5f)); // Durasi animasi (sesuaikan dengan panjang animasi)
        }
    }

    public Vector2 GetFacingDirection()
    {
        // Gunakan lastMoveDirection sebagai arah hadap
        Debug.Log($"Returning facing direction: {lastMoveDirection}");
        return lastMoveDirection;
    }

    private System.Collections.IEnumerator StopPlowingAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        isPlowing = false;
        anim.SetBool("IsPlowing", false); // Reset trigger manual
        Debug.Log("Plowing animation ended!");
    }

    private System.Collections.IEnumerator StopWateringAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        isWatering = false;
        anim.SetBool("IsWatering", false); // Reset trigger manual
        Debug.Log("Watering animation ended!");
    }

    public void EndPlowing()
    {
        isPlowing = false;
        anim.SetBool("IsPlowing", false);
        Debug.Log("Plowing animation ended!");
    }

    public void EndWatering()
    {
        isWatering = false;
        anim.SetBool("IsWatering", false);
        Debug.Log("Watering animation ended!");
    }
}