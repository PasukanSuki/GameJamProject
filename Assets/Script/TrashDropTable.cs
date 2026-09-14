using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TrashDropTable", menuName = "Gedede/Trash/Drop Table")]
public class TrashDropTable : ScriptableObject
{
    [Serializable]
    public class DropEntry
    {
        public ItemDefinition item;
        public bool ordinaryTrash;
        public GameObject pickupPrefab;
        [Range(0f, 100f)] public float weight = 1f;
    }

    public class DropResult
    {
        public ItemDefinition item;
        public bool ordinaryTrash;
        public GameObject pickupPrefab;
    }

    [SerializeField] private List<DropEntry> entries = new List<DropEntry>();

    public DropResult Roll(bool requirePrefab)
    {
        float roll = UnityEngine.Random.value * 100f;
        foreach (DropEntry entry in entries)
        {
            if (!IsValid(entry, requirePrefab))
            {
                continue;
            }

            roll -= entry.weight;
            if (roll <= 0f)
            {
                return new DropResult
                {
                    item = entry.item,
                    ordinaryTrash = entry.ordinaryTrash,
                    pickupPrefab = entry.pickupPrefab
                };
            }
        }

        return new DropResult { ordinaryTrash = true };
    }

    private bool IsValid(DropEntry entry, bool requirePrefab)
    {
        if (entry == null || entry.weight <= 0f)
        {
            return false;
        }

        if (!entry.ordinaryTrash && entry.item == null)
        {
            return false;
        }

        return !requirePrefab || entry.pickupPrefab != null;
    }
}
