using UnityEngine;

[CreateAssetMenu(fileName = "ItemDefinition", menuName = "Gedede/Inventory/Item Definition")]
public class ItemDefinition : ScriptableObject
{
    [SerializeField] private string itemId;
    [SerializeField] private string displayName;
    [SerializeField] private Sprite icon;
    [TextArea]
    [SerializeField] private string description;
    [SerializeField, Min(1)] private int stackLimit = 99;
    [SerializeField, Min(0)] private int sellValue;

    public string ItemId => itemId;
    public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;
    public Sprite Icon => icon;
    public string Description => description;
    public int StackLimit => Mathf.Max(1, stackLimit);
    public int SellValue => Mathf.Max(0, sellValue);
}