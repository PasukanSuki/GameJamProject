using System;
using UnityEngine;

public class WorldTrashPickup : MonoBehaviour
{
    [SerializeField] private ItemDefinition item;
    [SerializeField] private bool ordinaryTrash;
    [SerializeField, Min(1)] private int quantity = 1;

    private bool collected;

    public ItemDefinition Item => item;
    public bool IsOrdinaryTrash => ordinaryTrash;
    public int Quantity => quantity;

    public void Configure(ItemDefinition configuredItem, bool isOrdinaryTrash, int configuredQuantity = 1)
    {
        item = configuredItem;
        ordinaryTrash = isOrdinaryTrash;
        quantity = Mathf.Max(1, configuredQuantity);
        collected = false;
    }

    public bool TryCollect(Func<bool> acceptPickup)
    {
        if (collected || acceptPickup == null || !acceptPickup())
        {
            return false;
        }

        collected = true;
        Destroy(gameObject);
        return true;
    }

    private void Awake()
    {
        quantity = Mathf.Max(1, quantity);
        EnsureCollider();
    }

    private void EnsureCollider()
    {
        if (GetComponentInChildren<Collider>() != null)
        {
            return;
        }

        BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
        Renderer pickupRenderer = GetComponentInChildren<Renderer>();
        if (pickupRenderer != null)
        {
            boxCollider.center = transform.InverseTransformPoint(pickupRenderer.bounds.center);
            boxCollider.size = transform.InverseTransformVector(pickupRenderer.bounds.size);
        }
    }
}
