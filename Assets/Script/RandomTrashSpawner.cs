using System.Collections.Generic;
using UnityEngine;

public class RandomTrashSpawner : MonoBehaviour
{
    [SerializeField] private TrashDropTable dropTable;
    [SerializeField] private BoxCollider spawnArea;
    [SerializeField, Min(0)] private int initialSpawnCount = 5;
    [SerializeField, Min(0)] private int maxActivePickups = 5;
    [SerializeField, Min(0f)] private float respawnDelay = 2f;
    [SerializeField, Min(1)] private int maxSpawnAttempts = 25;

    private readonly List<WorldTrashPickup> activePickups = new List<WorldTrashPickup>();
    private float respawnTimer;
    private bool initialSpawnComplete;

    private void Awake()
    {
        maxActivePickups = Mathf.Max(0, maxActivePickups);
        initialSpawnCount = Mathf.Min(Mathf.Max(0, initialSpawnCount), maxActivePickups);
        maxSpawnAttempts = Mathf.Max(1, maxSpawnAttempts);

        if (spawnArea == null)
        {
            spawnArea = GetComponent<BoxCollider>();
        }
    }

    private void Start()
    {
        for (int index = 0; index < initialSpawnCount; index++)
        {
            SpawnPickup();
        }

        initialSpawnComplete = true;
    }

    private void Update()
    {
        RemoveDestroyedPickups();
        if (!initialSpawnComplete || activePickups.Count >= maxActivePickups || dropTable == null)
        {
            return;
        }

        respawnTimer -= Time.deltaTime;
        if (respawnTimer <= 0f && SpawnPickup())
        {
            respawnTimer = respawnDelay;
        }
    }

    private bool SpawnPickup()
    {
        if (dropTable == null || activePickups.Count >= maxActivePickups)
        {
            return false;
        }

        TrashDropTable.DropResult result = dropTable.Roll(true);
        if (result == null || result.pickupPrefab == null)
        {
            return false;
        }

        GameObject pickupObject = Instantiate(result.pickupPrefab, GetRandomPosition(), Quaternion.identity);
        WorldTrashPickup pickup = pickupObject.GetComponent<WorldTrashPickup>();
        if (pickup == null)
        {
            pickup = pickupObject.AddComponent<WorldTrashPickup>();
        }

        pickup.Configure(result.item, result.ordinaryTrash);
        activePickups.Add(pickup);
        return true;
    }

    private Vector3 GetRandomPosition()
    {
        if (spawnArea != null)
        {
            Vector3 halfSize = spawnArea.size * 0.5f;
            Vector3 localPosition = spawnArea.center + new Vector3(
                Random.Range(-halfSize.x, halfSize.x),
                Random.Range(-halfSize.y, halfSize.y),
                Random.Range(-halfSize.z, halfSize.z));
            return spawnArea.transform.TransformPoint(localPosition);
        }

        return transform.position;
    }

    private void RemoveDestroyedPickups()
    {
        for (int index = activePickups.Count - 1; index >= 0; index--)
        {
            if (activePickups[index] == null)
            {
                activePickups.RemoveAt(index);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (spawnArea == null)
        {
            return;
        }

        Gizmos.color = new Color(0.2f, 0.85f, 0.55f, 0.35f);
        Gizmos.matrix = spawnArea.transform.localToWorldMatrix;
        Gizmos.DrawCube(spawnArea.center, spawnArea.size);
        Gizmos.color = new Color(0.2f, 0.85f, 0.55f, 1f);
        Gizmos.DrawWireCube(spawnArea.center, spawnArea.size);
        Gizmos.matrix = Matrix4x4.identity;
    }
}
