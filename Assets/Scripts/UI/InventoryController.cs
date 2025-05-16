using UnityEngine;
using TMPro;

public class InventoryController : MonoBehaviour
{
    public TextMeshProUGUI tomatoText;
    public TextMeshProUGUI riceText;

    public int TomatoValue = 0;
    public int RiceValue = 0;

    void Start()
    {
        UpdateInventoryText();
    }

    public void AddItem(string itemName, int quantity)
    {
        if (itemName == "Tomato")
        {
            TomatoValue += quantity;
            Debug.Log($"TomatoValue sekarang: {TomatoValue}");
        }
        else if (itemName == "Rice")
        {
            RiceValue += quantity;
            Debug.Log($"RiceValue sekarang: {RiceValue}");
        }
        else
        {
            Debug.LogWarning($"Item {itemName} tidak dikenali!");
        }

        UpdateInventoryText();
    }

    void UpdateInventoryText()
    {
        if (tomatoText != null)
            tomatoText.text = TomatoValue.ToString() + "x";

        if (riceText != null)
            riceText.text = RiceValue.ToString() + "x";
    }
}
