using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class HotbarController : MonoBehaviour
{
    [System.Serializable]
    public class HotbarSlot
    {
        public Image slotImage; // UI Image for the slot
        public Image icon; // Icon of the tool (e.g., hoe, seed)
        public string toolName; // e.g., "Hoe", "WateringCan", "RiceSeed"
        public bool isActive; // Is this slot selected?
    }

    [SerializeField] private HotbarSlot[] slots; // Array of slots (e.g., 6 slots)
    [SerializeField] private Color activeColor = Color.yellow; // Highlight color
    [SerializeField] private Color inactiveColor = Color.white; // Default color
    [SerializeField] private AudioClip selectSound; // Sound for selecting a slot
    private AudioSource audioSource;
    private int activeSlotIndex = -1; // Currently selected slot

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    [System.Obsolete]
    private void Start()
    {
        // Pengecekan null untuk slots
        if (slots == null || slots.Length == 0)
        {
            Debug.LogError("Slots array is not assigned in Inspector!");
            return;
        }

        // Initialize slots
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null || slots[i].slotImage == null)
            {
                Debug.LogError($"Slot {i} or its slotImage is not assigned!");
                continue;
            }

            slots[i].isActive = false;
            slots[i].slotImage.color = inactiveColor;

            // Add click listener to each slot
            Button button = slots[i].slotImage.GetComponent<Button>();
            if (button == null)
            {
                Debug.LogError($"Slot {i} does not have a Button component!");
                continue;
            }

            int index = i;
            button.onClick.AddListener(() => SelectSlot(index));
        }
    }

    [System.Obsolete]
    private void Update()
    {
        // Keyboard input for slot selection (1–6)
        if (slots == null) return;

        for (int i = 0; i < slots.Length; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                SelectSlot(i);
            }
        }
    }

    [System.Obsolete]
    public void SelectSlot(int index)
    {
        if (slots == null || index < 0 || index >= slots.Length || string.IsNullOrEmpty(slots[index]?.toolName))
            return;

        // Deactivate previous slot
        if (activeSlotIndex != -1 && slots[activeSlotIndex] != null && slots[activeSlotIndex].slotImage != null)
        {
            slots[activeSlotIndex].isActive = false;
            slots[activeSlotIndex].slotImage.color = inactiveColor;
        }

        // Activate new slot
        activeSlotIndex = index;
        if (slots[index] != null && slots[index].slotImage != null)
        {
            slots[index].isActive = true;
            slots[index].slotImage.color = activeColor;
        }

        // Play sound
        if (selectSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(selectSound);
        }

        // Notify game of selected tool
        OnToolSelected(slots[index]?.toolName);
    }

    [System.Obsolete]
    private void OnToolSelected(string toolName)
    {
        if (string.IsNullOrEmpty(toolName)) return;
        Debug.Log($"Selected tool: {toolName}");

        // Tambahkan ini:
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            player.SetActiveTool(toolName);
        }
    }

    public string GetActiveTool()
    {
        return activeSlotIndex != -1 && slots != null && slots[activeSlotIndex] != null ? slots[activeSlotIndex].toolName : "";
    }
}