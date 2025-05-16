using UnityEngine;

public class PlantingSystem : MonoBehaviour
{
    [System.Serializable]
    public class CropData
    {
        public string cropName;
        public Sprite seedlingSprite;
        public Sprite growingSprite;
        public Sprite harvestableSprite;
        public GameObject fruitPrefab;
        public float seedlingTime;
        public float growingTime;
        public float harvestWindow;
        public float rotTime;
    }

    [System.Serializable]
    public class FarmTile
    {
        public Vector2 position;
        public GameObject tileObject;
        public GameObject cropObject;
        public GameObject fruitObject;
        public CropData currentCrop;
        public float growthTime;
        public int growthStage;
        public bool isTilled;
        public bool isWatered;
        public float tilledTime;
        public GameObject waterOverlayObject;
    }

    [SerializeField] private CropData[] crops;
    [SerializeField] private GameObject farmTilePrefab;
    [SerializeField] private Sprite tilledSoilSprite;
    [SerializeField] private Vector2 gridSize = new Vector2(10, 10);
    [SerializeField] private float tileSpacing = 1f;
    [SerializeField] private AudioClip plantSound;
    [SerializeField] private AudioClip harvestSound;
    [SerializeField] private AudioClip hoeSound;
    [SerializeField] private AudioClip waterSound;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private InventoryController inventoryController;
    [SerializeField] private GameObject waterOverlayPrefab;
    [SerializeField] private float harvestDistance = 0.2f;
    [SerializeField] private float tilledDuration = 30f;
    [SerializeField] [Range(0f, 1f)] private float waterGrowthReduction = 0.75f;

    private FarmTile[,] farmGrid;
    private HotbarController hotbar;
    private AudioSource audioSource;
    private PlayerController playerController;
    private PlantingSystem plantingSystem;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        hotbar = FindObjectOfType<HotbarController>();
        plantingSystem = this;
        if (hotbar == null) Debug.LogError("HotbarController not found!");
        if (playerTransform == null) Debug.LogError("Player Transform not assigned!");
        if (inventoryController == null) Debug.LogError("InventoryController not assigned! Please drag it into the Inspector.");

        playerController = playerTransform.GetComponent<PlayerController>();
        if (playerController == null) Debug.LogError("PlayerController not found on player object!");
    }

    private void Start()
    {
        farmGrid = new FarmTile[(int)gridSize.x, (int)gridSize.y];
        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                Vector3 worldPos = new Vector3(x * tileSpacing, y * tileSpacing, 0);
                GameObject tile = Instantiate(farmTilePrefab, worldPos, Quaternion.identity, transform);
                tile.name = $"Tile_{x}_{y}";
                farmGrid[x, y] = new FarmTile
                {
                    position = new Vector2(x, y),
                    tileObject = tile,
                    cropObject = null,
                    fruitObject = null,
                    currentCrop = null,
                    growthTime = 0f,
                    growthStage = 0,
                    isTilled = false,
                    isWatered = false,
                    tilledTime = 0f
                };
            }
        }
    }

    private void Update()
    {
        Vector2 playerPos = playerTransform.position;
        int playerX = Mathf.FloorToInt(playerPos.x / tileSpacing);
        int playerY = Mathf.FloorToInt(playerPos.y / tileSpacing);

        // Perbarui semua tile, bukan hanya di sekitar player
        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                if (farmGrid[x, y].growthStage > 0 && farmGrid[x, y].growthStage < 4)
                {
                    farmGrid[x, y].growthTime += Time.deltaTime;
                    UpdateCropStage(x, y);
                }
                if (farmGrid[x, y].growthStage == 3 && farmGrid[x, y].fruitObject != null)
                {
                    Vector3 tilePos = new Vector3(x * tileSpacing, y * tileSpacing, 0);
                    float distance = Vector3.Distance(playerTransform.position, tilePos);
                    Debug.Log($"Tile ({x}, {y}) at {tilePos}, Player at {playerTransform.position}, Distance: {distance}, HarvestDistance: {harvestDistance}");
                    if (distance <= harvestDistance)
                    {
                        Debug.Log($"Auto-harvesting at ({x}, {y})");
                        AutoHarvestCrop(x, y);
                    }
                }
                if (farmGrid[x, y].isTilled && farmGrid[x, y].growthStage == 0)
                {
                    farmGrid[x, y].tilledTime += Time.deltaTime;
                    if (farmGrid[x, y].tilledTime >= tilledDuration)
                    {
                        RevertTilledTile(x, y);
                    }
                }
            }
        }

        if (Input.GetMouseButtonDown(0)) TryPlantOrWater();
    }

    private void TryPlantOrWater()
    {
        string selectedTool = hotbar.GetActiveTool();
        Debug.Log($"Selected tool: {selectedTool}");

        Vector2 playerPos = playerTransform.position;
        Vector2 facingDir = playerController.GetFacingDirection();
        Debug.Log($"Player position: {playerPos}, Facing direction: {facingDir}");

        int x = Mathf.FloorToInt(playerPos.x / tileSpacing);
        int y = Mathf.FloorToInt(playerPos.y / tileSpacing);
        Debug.Log($"Player standing on tile: ({x}, {y})");

        int targetX = x + Mathf.RoundToInt(facingDir.x);
        int targetY = y + Mathf.RoundToInt(facingDir.y);
        Debug.Log($"Calculated target tile in front: ({targetX}, {targetY})");

        if (targetX >= 0 && targetX < gridSize.x && targetY >= 0 && targetY < gridSize.y)
        {
            FarmTile tile = farmGrid[targetX, targetY];
            Debug.Log($"Tile at ({targetX}, {targetY}) - Tilled: {tile.isTilled}, GrowthStage: {tile.growthStage}");

            if (selectedTool == "Hoe")
            {
                if (!tile.isTilled)
                {
                    tile.isTilled = true;
                    tile.tilledTime = 0f;
                    tile.tileObject.GetComponent<SpriteRenderer>().sprite = tilledSoilSprite;
                    Debug.Log($"Tilled tile at ({targetX}, {targetY})");
                    if (hoeSound != null) audioSource.PlayOneShot(hoeSound);
                }
                else
                {
                    Debug.Log($"Tile at ({targetX}, {targetY}) is already tilled!");
                }
            }
            else if (selectedTool == "WateringCan")
            {
                if (tile.isTilled && tile.growthStage < 3)
                {
                    tile.isWatered = true;
                    Debug.Log($"Watered tile at ({targetX}, {targetY})");

                    if (waterSound != null) audioSource.PlayOneShot(waterSound);

                    // Tambahkan water overlay jika belum ada
                    if (tile.waterOverlayObject == null && waterOverlayPrefab != null)
                    {
                        Vector3 pos = new Vector3(targetX * tileSpacing, targetY * tileSpacing, -0.1f);
                        GameObject overlay = Instantiate(waterOverlayPrefab, pos, Quaternion.identity, transform);
                        
                        Destroy(overlay, 0.5f);
                    }
                }
                else
                {
                    Debug.Log($"Tile at ({targetX}, {targetY}) cannot be watered or already harvested!");
                }
            }
            else if (selectedTool == "RiceSeed" || selectedTool == "TomatoSeed")
            {
                CropData cropToPlant = selectedTool == "RiceSeed" ? crops[0] : crops[1];
                if (tile.growthStage == 0 && tile.isTilled)
                {
                    tile.currentCrop = cropToPlant;
                    tile.growthStage = 1;
                    tile.growthTime = 0f;
                    tile.isWatered = false;
                    tile.tilledTime = 0f;
                    tile.cropObject = new GameObject($"Crop_{targetX}_{targetY}");
                    tile.cropObject.transform.position = new Vector3(targetX * tileSpacing, targetY * tileSpacing, 0);
                    Debug.Log($"Crop placed at: {tile.cropObject.transform.position}");
                    tile.cropObject.transform.SetParent(transform);
                    SpriteRenderer sr = tile.cropObject.AddComponent<SpriteRenderer>();
                    sr.sprite = cropToPlant.seedlingSprite;
                    Debug.Log($"Assigned sprite: {sr.sprite?.name ?? "null"} to Crop_{targetX}_{targetY}");
                    sr.sortingOrder = 2;
                    sr.enabled = true;

                    if (plantSound != null) audioSource.PlayOneShot(plantSound);
                    Debug.Log($"Planted {cropToPlant.cropName} at ({targetX}, {targetY})");
                }
                else
                {
                    Debug.Log("Tile must be tilled before planting or already has a crop!");
                }
            }
            else
            {
                Debug.Log($"No action for tool: {selectedTool}");
            }
        }
        else
        {
            Debug.Log($"Target tile out of bounds: ({targetX}, {targetY})");
        }
    }

    private void UpdateCropStage(int x, int y)
    {
        FarmTile tile = farmGrid[x, y];
        CropData crop = tile.currentCrop;
        SpriteRenderer sr = tile.cropObject?.GetComponent<SpriteRenderer>();
        Debug.Log($"Updating stage for tile ({x}, {y}) - Stage: {tile.growthStage}, isWatered: {tile.isWatered}, growthTime: {tile.growthTime}");

        float waterReduction = tile.isWatered ? (crop.seedlingTime + crop.growingTime) * waterGrowthReduction : 0f;
        float adjustedSeedlingTime = crop.seedlingTime - (tile.growthStage == 1 ? waterReduction : 0f);
        float adjustedGrowingTime = crop.growingTime - (tile.growthStage == 2 ? waterReduction : 0f);

        Debug.Log($"Tile ({x}, {y}) - SeedlingTime: {crop.seedlingTime}, AdjustedSeedlingTime: {adjustedSeedlingTime}, GrowingTime: {crop.growingTime}, AdjustedGrowingTime: {adjustedGrowingTime}, WaterReduction: {waterReduction}");

        if (tile.growthStage == 1 && tile.growthTime >= adjustedSeedlingTime)
        {
            tile.growthStage = 2;
            if (sr != null) sr.sprite = crop.growingSprite;
            Debug.Log($"{crop.cropName} at ({x}, {y}) is growing, adjusted time: {adjustedSeedlingTime}");
        }
        else if (tile.growthStage == 2 && tile.growthTime >= adjustedSeedlingTime + adjustedGrowingTime)
        {
            tile.growthStage = 3;
            if (sr != null) sr.sprite = crop.harvestableSprite;
            if (crop.fruitPrefab != null)
            {
                Destroy(tile.cropObject);
                tile.cropObject = null;
                tile.fruitObject = Instantiate(crop.fruitPrefab, new Vector3(x * tileSpacing, y * tileSpacing, 0), Quaternion.identity, transform);
                tile.fruitObject.name = $"Fruit_{x}_{y}";
                Debug.Log($"Spawned {crop.cropName} fruit at ({x}, {y})");
            }
            Debug.Log($"{crop.cropName} at ({x}, {y}) is harvestable");
        }
        else if (tile.growthStage == 3 && tile.growthTime >= adjustedSeedlingTime + adjustedGrowingTime + crop.harvestWindow + crop.rotTime)
        {
            tile.growthStage = 4;
            if (tile.cropObject != null) Destroy(tile.cropObject);
            tile.cropObject = null;
            if (tile.fruitObject != null) Destroy(tile.fruitObject);
            tile.fruitObject = null;
            tile.currentCrop = null;
            tile.growthTime = 0f;
            tile.isTilled = false;
            tile.isWatered = false;
            tile.tilledTime = 0f;
            tile.tileObject.GetComponent<SpriteRenderer>().sprite = null;
            Debug.Log($"{crop.cropName} at ({x}, {y}) has rotted and disappeared.");
        }
    }

    private void AutoHarvestCrop(int x, int y)
    {
        FarmTile tile = farmGrid[x, y];
        if (tile.fruitObject != null && tile.growthStage == 3)
        {
            string quality = tile.growthTime <= tile.currentCrop.seedlingTime + tile.currentCrop.growingTime + tile.currentCrop.harvestWindow ? "High" : "Low";
            Debug.Log($"Auto-harvested {tile.currentCrop.cropName} at ({x}, {y}) with {quality} quality");
            
            if (harvestSound != null) audioSource.PlayOneShot(harvestSound);

            EconomyManager.Instance.ShowHarvestOptions(
                tile.currentCrop.cropName,
                quality,
                () =>
                {
                    int coinReward = quality == "High" ? 10 : 5;
                    EconomyManager.Instance.AddCoins(coinReward);
                    Debug.Log($"Sold {tile.currentCrop.cropName} for {coinReward} coins");
                    CompleteHarvest(tile, x, y);
                },
                () =>
                {
                    EconomyManager.Instance.AddMoral(5);
                    Debug.Log($"Donated {tile.currentCrop.cropName} for 5 moral points");
                    CompleteHarvest(tile, x, y);
                }
            );
        }
        else
        {
            Debug.LogWarning($"Auto-harvest failed at ({x}, {y}): fruitObject is null or growthStage != 3");
        }
    }

    private void CompleteHarvest(FarmTile tile, int x, int y)
    {
        if (inventoryController != null)
        {
            inventoryController.AddItem(tile.currentCrop.cropName, 1);
        }

        Destroy(tile.fruitObject);
        tile.fruitObject = null;
        tile.growthTime = 0f;
        tile.growthStage = 0;
        tile.isTilled = false;
        tile.isWatered = false;
        tile.tilledTime = 0f;
        tile.tileObject.GetComponent<SpriteRenderer>().sprite = farmTilePrefab.GetComponent<SpriteRenderer>().sprite;
    }

    private void RevertTilledTile(int x, int y)
    {
        FarmTile tile = farmGrid[x, y];
        tile.isTilled = false;
        tile.tilledTime = 0f;
        tile.tileObject.GetComponent<SpriteRenderer>().sprite = farmTilePrefab.GetComponent<SpriteRenderer>().sprite;
        Debug.Log($"Tile at ({x}, {y}) reverted to untilled state after {tilledDuration} seconds");
    }

    public FarmTile GetTileAtPosition(int x, int y)
    {
        if (x >= 0 && x < gridSize.x && y >= 0 && y < gridSize.y) return farmGrid[x, y];
        return null;
    }

    public Vector2 GetGridSize() { return gridSize; }
    public float GetTileSpacing() { return tileSpacing; }
    public Sprite GetTilledSoilSprite() { return tilledSoilSprite; }

    private void OnDrawGizmos()
    {
        if (playerTransform != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(playerTransform.position, harvestDistance);
        }
    }
}