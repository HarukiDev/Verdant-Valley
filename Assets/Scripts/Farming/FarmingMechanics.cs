using UnityEngine;

public class FarmingMechanics : MonoBehaviour
{
    [Header("Sounds & Effects")]
    [SerializeField] private AudioClip hoeSound;
    [SerializeField] private AudioClip waterSound;
    [SerializeField] private GameObject waterOverlayPrefab;

    [Header("Indicators & Prefabs")]
    [SerializeField] private GameObject hoeIndicatorPrefab;
    [SerializeField] private GameObject waterIndicatorPrefab;

    [Header("Distances")]
    [SerializeField] private float maxHoeDistance = 0.9f;
    [SerializeField] private float maxWaterDistance = 0.9f;

    [SerializeField] private Transform playerTransform;

    private HotbarController hotbar;
    private PlantingSystem plantingSystem;
    private AudioSource audioSource;
    private Camera mainCamera;

    private GameObject currentIndicator;
    private Vector2 lastDirection = Vector2.right;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
        hotbar = FindObjectOfType<HotbarController>();
        plantingSystem = FindObjectOfType<PlantingSystem>();
        mainCamera = Camera.main;

        if (hotbar == null || plantingSystem == null || mainCamera == null || hoeIndicatorPrefab == null || waterIndicatorPrefab == null)
        {
            Debug.LogError("Missing component or prefab reference!");
        }
    }

    private void Update()
    {
        UpdatePlayerDirection();
        UpdateToolIndicator();

        if (Input.GetMouseButtonDown(0))
        {
            string selectedTool = hotbar.GetActiveTool();
            if (selectedTool == "Hoe")
            {
                TryHoeTile();
            }
            else if (selectedTool == "WateringCan")
            {
                TryWaterTile();
            }
        }
    }

    private void UpdatePlayerDirection()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector2 newDir = Vector2.zero;
        if (Mathf.Abs(h) > Mathf.Abs(v)) newDir = new Vector2(Mathf.Sign(h), 0);
        else if (Mathf.Abs(v) > 0) newDir = new Vector2(0, Mathf.Sign(v));

        if (newDir != Vector2.zero) lastDirection = newDir;
    }

    private void UpdateToolIndicator()
    {
        string tool = hotbar.GetActiveTool();

        if (tool != "Hoe" && tool != "WateringCan")
        {
            if (currentIndicator != null) Destroy(currentIndicator);
            return;
        }

        Vector3 playerPos = playerTransform.position;
        Vector3 targetPos = playerPos + (Vector3)(lastDirection * plantingSystem.GetTileSpacing());

        int x = Mathf.FloorToInt(targetPos.x / plantingSystem.GetTileSpacing());
        int y = Mathf.FloorToInt(targetPos.y / plantingSystem.GetTileSpacing());

        PlantingSystem.FarmTile tile = plantingSystem.GetTileAtPosition(x, y);

        if (tile != null)
        {
            Vector3 tileWorldPos = new Vector3(x * plantingSystem.GetTileSpacing(), y * plantingSystem.GetTileSpacing(), 0);
            float dist = Vector2.Distance(playerTransform.position, tileWorldPos);

            bool showIndicator = false;

            if (tool == "Hoe")
            {
                showIndicator = dist <= maxHoeDistance && tile.growthStage == 0 && !tile.isTilled;
            }
            else if (tool == "WateringCan")
            {
                showIndicator = dist <= maxWaterDistance && tile.growthStage > 0 && tile.growthStage < 3 && !tile.isWatered;
            }

            if (showIndicator)
            {
                if (currentIndicator == null)
                {
                    GameObject indicatorPrefab = (tool == "Hoe") ? hoeIndicatorPrefab : waterIndicatorPrefab;
                    currentIndicator = Instantiate(indicatorPrefab, tileWorldPos, Quaternion.identity);
                }
                else
                {
                    currentIndicator.transform.position = tileWorldPos;
                }
            }
            else
            {
                if (currentIndicator != null) Destroy(currentIndicator);
            }
        }
        else
        {
            if (currentIndicator != null) Destroy(currentIndicator);
        }
    }

    private void TryHoeTile()
    {
        Vector3 playerPos = playerTransform.position;
        Vector3 targetPos = playerPos + (Vector3)(lastDirection * plantingSystem.GetTileSpacing());

        int x = Mathf.FloorToInt(targetPos.x / plantingSystem.GetTileSpacing());
        int y = Mathf.FloorToInt(targetPos.y / plantingSystem.GetTileSpacing());

        Vector2Int playerTile = new Vector2Int(
            Mathf.FloorToInt(playerPos.x / plantingSystem.GetTileSpacing()),
            Mathf.FloorToInt(playerPos.y / plantingSystem.GetTileSpacing())
        );

        if (x == playerTile.x && y == playerTile.y)
        {
            Debug.Log("Cannot hoe tile under player!");
            return;
        }

        PlantingSystem.FarmTile tile = plantingSystem.GetTileAtPosition(x, y);

        if (tile != null)
        {
            Vector3 tilePos = new Vector3(x * plantingSystem.GetTileSpacing(), y * plantingSystem.GetTileSpacing(), 0);
            float dist = Vector2.Distance(playerTransform.position, tilePos);

            if (dist <= maxHoeDistance && tile.growthStage == 0 && !tile.isTilled)
            {
                tile.isTilled = true;
                tile.tilledTime = 0f;
                tile.tileObject.GetComponent<SpriteRenderer>().sprite = plantingSystem.GetTilledSoilSprite();
                if (hoeSound != null) audioSource.PlayOneShot(hoeSound);
                if (currentIndicator != null) Destroy(currentIndicator);
                Debug.Log($"Hoed tile at ({x},{y})");
            }
        }
    }

    private void TryWaterTile()
    {
        Vector3 playerPos = playerTransform.position;
        Vector3 targetPos = playerPos + (Vector3)(lastDirection * plantingSystem.GetTileSpacing());

        int x = Mathf.FloorToInt(targetPos.x / plantingSystem.GetTileSpacing());
        int y = Mathf.FloorToInt(targetPos.y / plantingSystem.GetTileSpacing());

        PlantingSystem.FarmTile tile = plantingSystem.GetTileAtPosition(x, y);

        if (tile != null)
        {
            Vector3 tilePos = new Vector3(x * plantingSystem.GetTileSpacing(), y * plantingSystem.GetTileSpacing(), 0);
            float dist = Vector2.Distance(playerTransform.position, tilePos);

            if (dist <= maxWaterDistance && tile.growthStage > 0 && tile.growthStage < 3 && !tile.isWatered)
            {
                tile.isWatered = true;
                // Hapus pengaturan waterGrowthReduction, gunakan logika di PlantingSystem
                if (waterSound != null) audioSource.PlayOneShot(waterSound);
                if (waterOverlayPrefab != null)
                {
                    GameObject overlay = Instantiate(waterOverlayPrefab, tilePos, Quaternion.identity, tile.tileObject.transform);
                    Destroy(overlay, 1f);
                }
                Debug.Log($"Watered tile at ({x},{y})");
                if (currentIndicator != null) Destroy(currentIndicator);
            }
            else
            {
                Debug.Log("Cannot water this tile (already watered, no crop, or too far)");
            }
        }
    }

    private void OnDestroy()
    {
        if (currentIndicator != null) Destroy(currentIndicator);
    }
}