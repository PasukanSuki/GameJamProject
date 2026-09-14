using UnityEngine;

[CreateAssetMenu(fileName = "ItemShopDefinition", menuName = "Gedede/Economy/Item Shop Definition")]
public class ItemShopDefinition : ScriptableObject
{
    [SerializeField] private string shopItemId;
    [SerializeField] private ItemDefinition item;
    [SerializeField, Min(1)] private int quantity = 1;
    [SerializeField, Min(1)] private int price = 100;

    public string ShopItemId => string.IsNullOrWhiteSpace(shopItemId) ? name : shopItemId;
    public ItemDefinition Item => item;
    public int Quantity => Mathf.Max(1, quantity);
    public int Price => Mathf.Max(1, price);
}
