using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class EconomyManager : MonoBehaviour
{
    public static EconomyManager Instance { get; private set; }

    [SerializeField] private int currentCoins = 0;
    [SerializeField] private int currentMoral = 0;
    [SerializeField] private int maxMoral = 100;

    [SerializeField] private TMP_Text coinsText;
    [SerializeField] private TMP_Text moralText;

    private bool isEndingShown = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        UpdateUI();
    }

    public void AddCoins(int amount)
    {
        currentCoins += amount;
        UpdateUI();
    }

    public void AddMoral(int amount)
    {
        currentMoral = Mathf.Min(currentMoral + amount, maxMoral);
        UpdateUI();
    }

    public void ShowHarvestOptions(string cropName, string quality, System.Action sellAction, System.Action donateAction)
    {
        Debug.Log($"Harvest Options for {cropName} ({quality}):");
        Debug.Log("1. Sell for coins");
        Debug.Log("2. Donate for moral");
        if (Input.GetKeyDown(KeyCode.Alpha1)) sellAction?.Invoke();
        else if (Input.GetKeyDown(KeyCode.Alpha2)) donateAction?.Invoke();
    }

    private void UpdateUI()
    {
        if (coinsText != null) coinsText.text = $"Coins: {currentCoins}";
        if (moralText != null) moralText.text = $"Moral: {currentMoral}/{maxMoral}";

        Debug.Log($"Checking ending: Coins={currentCoins}, Moral={currentMoral}, IsEndingShown={isEndingShown}");
        if (currentCoins >= 100 && !isEndingShown)
        {
            Debug.Log("Loading EndingSatu");
            LoadEndingScene("EndingSatu");
        }
        else if (currentMoral >= 100 && !isEndingShown)
        {
            Debug.Log("Loading EndingDua");
            LoadEndingScene("EndingDua");
        }
    }

    private bool IsEndingShown()
    {
        return isEndingShown;
    }

    private void LoadEndingScene(string sceneName)
    {
        if (!isEndingShown)
        {
            isEndingShown = true;
            try
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
                Debug.Log($"Successfully loaded scene: {sceneName}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to load scene {sceneName}: {e.Message}");
            }
        }
    }

    public int GetCoins() { return currentCoins; }
    public int GetMoral() { return currentMoral; }
}