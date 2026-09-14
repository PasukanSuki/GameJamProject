using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    [SerializeField, Min(1)] private int slotCapacity = 20;
    [SerializeField] private List<ItemStack> slots = new List<ItemStack>();

    private int baseSlotCapacity;

    public int SlotCapacity => slotCapacity;
    public IReadOnlyList<ItemStack> Slots => slots;
    public event Action InventoryChanged;

    private void Awake()
    {
        slotCapacity = Mathf.Max(1, slotCapacity);
        baseSlotCapacity = slotCapacity;
        EnsureSlotCount();
    }

    private void OnValidate()
    {
        slotCapacity = Mathf.Max(1, slotCapacity);
        EnsureSlotCount();
    }

    public bool AddItem(ItemDefinition item, int quantity = 1)
    {
        if (item == null || quantity <= 0)
        {
            return false;
        }

        EnsureSlotCount();

        if (!CanAdd(item, quantity))
        {
            return false;
        }

        int remaining = quantity;

        for (int index = 0; index < slots.Count && remaining > 0; index++)
        {
            ItemStack slot = slots[index];
            if (slot.item != item)
            {
                continue;
            }

            int space = item.StackLimit - slot.quantity;
            int amount = Mathf.Min(space, remaining);
            slot.quantity += amount;
            remaining -= amount;
        }

        for (int index = 0; index < slots.Count && remaining > 0; index++)
        {
            ItemStack slot = slots[index];
            if (!slot.IsEmpty)
            {
                continue;
            }

            int amount = Mathf.Min(item.StackLimit, remaining);
            slot.item = item;
            slot.quantity = amount;
            remaining -= amount;
        }

        InventoryChanged?.Invoke();
        return true;
    }

    public int GetItemCount(ItemDefinition item)
    {
        if (item == null)
        {
            return 0;
        }

        int total = 0;
        foreach (ItemStack slot in slots)
        {
            if (slot != null && slot.item == item && slot.quantity > 0)
            {
                total += slot.quantity;
            }
        }

        return total;
    }

    public bool CanAddItem(ItemDefinition item, int quantity = 1)
    {
        return item != null && quantity > 0 && CanAdd(item, quantity);
    }

    public bool RemoveItem(ItemDefinition item, int quantity = 1)
    {
        if (item == null || quantity <= 0 || GetItemCount(item) < quantity)
        {
            return false;
        }

        int remaining = quantity;
        for (int index = 0; index < slots.Count && remaining > 0; index++)
        {
            ItemStack slot = slots[index];
            if (slot == null || slot.item != item)
            {
                continue;
            }

            int amount = Mathf.Min(slot.quantity, remaining);
            slot.quantity -= amount;
            remaining -= amount;
            if (slot.quantity <= 0)
            {
                slot.Clear();
            }
        }

        InventoryChanged?.Invoke();
        return true;
    }

    public ItemStack GetSlot(int index)
    {
        EnsureSlotCount();
        return index >= 0 && index < slots.Count ? slots[index] : null;
    }

    public void Clear()
    {
        EnsureSlotCount();
        foreach (ItemStack slot in slots)
        {
            slot.Clear();
        }

        InventoryChanged?.Invoke();
    }

    public bool RestoreItem(ItemDefinition item, int quantity)
    {
        return AddItem(item, quantity);
    }

    public bool SetSlotCapacity(int capacity)
    {
        int requestedCapacity = Mathf.Max(1, capacity);
        if (requestedCapacity < slotCapacity && HasOccupiedSlotsBeyond(requestedCapacity))
        {
            Debug.LogWarning(
                $"[InventoryManager] Cannot reduce slot capacity from {slotCapacity} to {requestedCapacity}: " +
                "the slots being removed contain items.",
                this);
            return false;
        }

        if (requestedCapacity == slotCapacity)
        {
            EnsureSlotCount();
            return true;
        }

        int previousCapacity = slotCapacity;
        slotCapacity = requestedCapacity;
        EnsureSlotCount();
        InventoryChanged?.Invoke();
        Debug.Log($"[InventoryManager] Slot capacity changed from {previousCapacity} to {slotCapacity}.", this);
        return true;
    }

    public bool SetSlotCapacityBonus(int bonus)
    {
        return SetSlotCapacity(baseSlotCapacity + Mathf.Max(0, bonus));
    }

    public bool ResetSlotCapacity()
    {
        return SetSlotCapacity(baseSlotCapacity);
    }

    private bool CanAdd(ItemDefinition item, int quantity)
    {
        int available = 0;

        foreach (ItemStack slot in slots)
        {
            if (slot.item == item)
            {
                available += item.StackLimit - Mathf.Max(0, slot.quantity);
            }
            else if (slot.IsEmpty)
            {
                available += item.StackLimit;
            }
        }

        return available >= quantity;
    }

    private bool HasOccupiedSlotsBeyond(int capacity)
    {
        for (int index = capacity; index < slots.Count; index++)
        {
            if (slots[index] != null && !slots[index].IsEmpty)
            {
                return true;
            }
        }

        return false;
    }

    private void EnsureSlotCount()
    {
        while (slots.Count < slotCapacity)
        {
            slots.Add(new ItemStack());
        }

        if (slots.Count > slotCapacity)
        {
            slots.RemoveRange(slotCapacity, slots.Count - slotCapacity);
        }

        for (int index = 0; index < slots.Count; index++)
        {
            if (slots[index] == null)
            {
                slots[index] = new ItemStack();
            }
        }
    }
}