using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrashPile : MonoBehaviour
{
    [Serializable]
    public class ItemDrop
    {
        public ItemDefinition item;
        public bool ordinaryTrash;
        [Range(0f, 100f)] public float obtainChance = 25f;
    }

    public class PickupResult
    {
        public ItemDefinition item;
        public bool ordinaryTrash;
    }

    [Header("Pile")]
    [SerializeField, Min(1)] private int trashAmount = 10;
    [SerializeField] private Transform spawnPoint;

    [Header("Pile Stages")]
    [SerializeField] private Transform stageOne;
    [SerializeField] private Transform stageTwo;
    [SerializeField] private Transform stageThree;
    [SerializeField] private MeshCollider stageOneCollider;
    [SerializeField] private MeshCollider stageTwoCollider;
    [SerializeField] private MeshCollider stageThreeCollider;

    [Header("Pickup")]
    [SerializeField] private Sprite trashSprite;
    [SerializeField] private TrashDropTable dropTable;
    [SerializeField] private List<ItemDrop> itemDrops = new List<ItemDrop>();
    [SerializeField] private float pickupSpeed = 7f;
    [SerializeField] private float pickupDuration = 0.45f;
    [SerializeField] private float spawnRadius = 0.35f;
    [SerializeField] private float spriteScale = 0.35f;

    private int remainingTrash;

    public int RemainingTrash => remainingTrash;
    public bool HasTrash => remainingTrash > 0;

    private void Reset()
    {
        spawnPoint = transform;
        stageOne = transform.Find("Tumpukan Sampah fase 1");
        stageTwo = transform.Find("Tumpukan Sampah fase 2");
        stageThree = transform.Find("Tumpukan Sampah fase 3");
        stageOneCollider = stageOne != null ? stageOne.GetComponent<MeshCollider>() : null;
        stageTwoCollider = stageTwo != null ? stageTwo.GetComponent<MeshCollider>() : null;
        stageThreeCollider = stageThree != null ? stageThree.GetComponent<MeshCollider>() : null;
    }

    private void Awake()
    {
        trashAmount = Mathf.Max(1, trashAmount);
        remainingTrash = trashAmount;

        if (spawnPoint == null)
        {
            spawnPoint = transform;
        }

        stageOne = ResolveStage(stageOne, "Tumpukan Sampah fase 1");
        stageTwo = ResolveStage(stageTwo, "Tumpukan Sampah fase 2");
        stageThree = ResolveStage(stageThree, "Tumpukan Sampah fase 3");
        stageOneCollider = ResolveCollider(stageOneCollider, stageOne);
        stageTwoCollider = ResolveCollider(stageTwoCollider, stageTwo);
        stageThreeCollider = ResolveCollider(stageThreeCollider, stageThree);
        UpdatePileVisual();
    }

    public bool TryCollect(Transform playerTarget, Func<PickupResult, bool> tryAcceptPickup)
    {
        if (!HasTrash || playerTarget == null || tryAcceptPickup == null)
        {
            return false;
        }

        PickupResult result = RollPickup();
        if (result == null || !tryAcceptPickup(result))
        {
            return false;
        }

        remainingTrash--;
        UpdatePileVisual();
        StartCoroutine(FlyTrashToPlayer(playerTarget, result));
        return true;
    }

    private PickupResult RollPickup()
    {
        if (dropTable != null)
        {
            TrashDropTable.DropResult tableResult = dropTable.Roll(false);
            return new PickupResult
            {
                item = tableResult.item,
                ordinaryTrash = tableResult.ordinaryTrash
            };
        }

        float totalChance = 0f;

        foreach (ItemDrop drop in itemDrops)
        {
            if (drop == null || (!drop.ordinaryTrash && drop.item == null) || drop.obtainChance <= 0f)
            {
                continue;
            }

            totalChance += drop.obtainChance;
        }

        if (totalChance <= 0f)
        {
            return new PickupResult
            {
                ordinaryTrash = true
            };
        }

        float roll = UnityEngine.Random.value * totalChance;
        foreach (ItemDrop drop in itemDrops)
        {
            if (drop == null || (!drop.ordinaryTrash && drop.item == null) || drop.obtainChance <= 0f)
            {
                continue;
            }

            roll -= drop.obtainChance;
            if (roll <= 0f)
            {
                return new PickupResult
                {
                    item = drop.item,
                    ordinaryTrash = drop.ordinaryTrash
                };
            }
        }

        return null;
    }

    private IEnumerator FlyTrashToPlayer(Transform playerTarget, PickupResult result)
    {
        GameObject trashObject = new GameObject("CollectedTrash");
        trashObject.transform.position = spawnPoint.position + UnityEngine.Random.insideUnitSphere * spawnRadius;
        trashObject.transform.localScale = Vector3.one * spriteScale;

        SpriteRenderer renderer = trashObject.AddComponent<SpriteRenderer>();
        renderer.sprite = result.item != null && result.item.Icon != null
            ? result.item.Icon
            : trashSprite != null ? trashSprite : CreatePlaceholderSprite();
        FacePlayerCamera(trashObject.transform, playerTarget);

        float elapsed = 0f;
        while (trashObject != null && playerTarget != null && elapsed < pickupDuration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / Mathf.Max(0.01f, pickupDuration));
            trashObject.transform.position = Vector3.Lerp(
                trashObject.transform.position,
                playerTarget.position,
                Time.deltaTime * pickupSpeed * (1f + progress * 2f));
            FacePlayerCamera(trashObject.transform, playerTarget);

            if (Vector3.Distance(trashObject.transform.position, playerTarget.position) < 0.08f)
            {
                break;
            }

            yield return null;
        }

        if (trashObject != null)
        {
            Destroy(trashObject);
        }
    }

    private void FacePlayerCamera(Transform spriteTransform, Transform playerCamera)
    {
        spriteTransform.rotation = playerCamera.rotation;
    }

    private void UpdatePileVisual()
    {
        float takenRatio = 1f - remainingTrash / (float)Mathf.Max(1, trashAmount);
        bool isStageOne = remainingTrash > 0 && takenRatio < 1f / 3f;
        bool isStageTwo = remainingTrash > 0 && takenRatio >= 1f / 3f && takenRatio < 2f / 3f;
        bool isStageThree = remainingTrash > 0 && takenRatio >= 2f / 3f;

        SetStageActive(stageOne, stageOneCollider, isStageOne);
        SetStageActive(stageTwo, stageTwoCollider, isStageTwo);
        SetStageActive(stageThree, stageThreeCollider, isStageThree);
    }

    private void SetStageActive(Transform stage, MeshCollider stageCollider, bool isActive)
    {
        if (stage != null)
        {
            stage.gameObject.SetActive(isActive);
        }

        if (stageCollider != null)
        {
            stageCollider.enabled = isActive;
        }
    }

    private Transform ResolveStage(Transform assignedStage, string stageName)
    {
        if (assignedStage != null)
        {
            return assignedStage;
        }

        foreach (Transform child in GetComponentsInChildren<Transform>(true))
        {
            if (child != transform && child.name == stageName)
            {
                return child;
            }
        }

        return null;
    }

    private MeshCollider ResolveCollider(MeshCollider assignedCollider, Transform stage)
    {
        if (assignedCollider != null)
        {
            return assignedCollider;
        }

        return stage != null ? stage.GetComponent<MeshCollider>() : null;
    }

    private Sprite CreatePlaceholderSprite()
    {
        Texture2D texture = new Texture2D(16, 16);
        texture.filterMode = FilterMode.Point;
        Color[] pixels = new Color[texture.width * texture.height];

        for (int index = 0; index < pixels.Length; index++)
        {
            int x = index % texture.width;
            int y = index / texture.width;
            bool visible = (x - 8) * (x - 8) + (y - 8) * (y - 8) < 55;
            pixels[index] = visible
                ? new Color(0.2f + (x % 3) * 0.08f, 0.35f, 0.12f, 1f)
                : Color.clear;
        }

        texture.SetPixels(pixels);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, 16f, 16f), new Vector2(0.5f, 0.5f), 16f);
    }
}
