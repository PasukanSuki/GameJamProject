using UnityEngine;

public class EconomyBootstrap : MonoBehaviour
{
    [SerializeField] private bool createRuntimeUpgradePanel = true;

    private void Awake()
    {
        RemoveLegacyShopRuntimeObjects();
        EnsureComponent<CurrencyWallet>();
        EnsureComponent<PlayerUpgradeManager>();
        EnsureComponent<EconomySaveService>();
        EnsureComponent<PanelHotkeyController>();

        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas != null && canvas.GetComponentInChildren<WalletCounter>(true) == null)
        {
            canvas.gameObject.AddComponent<WalletCounter>();
        }

        if (createRuntimeUpgradePanel)
        {
            if (canvas != null && canvas.GetComponentInChildren<UpgradePanel>(true) == null)
            {
                Transform scenePanel = FindChildRecursive(canvas.transform, "UpgradePanel");
                if (scenePanel != null)
                {
                    scenePanel.gameObject.AddComponent<UpgradePanel>();
                    Debug.Log("[EconomyBootstrap] Using existing Canvas/UpgradePanel hierarchy.", scenePanel);
                }
                else
                {
                    canvas.gameObject.AddComponent<UpgradePanel>();
                    Debug.LogWarning(
                        "[EconomyBootstrap] Canvas/UpgradePanel was not found. A runtime panel was created. " +
                        "Use Gedede/UI/Create Upgrade Panel to create the panel in the scene.",
                        canvas);
                }
            }
        }
    }

    private static Transform FindChildRecursive(Transform parent, string childName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == childName)
            {
                return child;
            }

            Transform result = FindChildRecursive(child, childName);
            if (result != null)
            {
                return result;
            }
        }

        return null;
    }

    private T EnsureComponent<T>() where T : Component
    {
        T component = GetComponent<T>();
        return component != null ? component : gameObject.AddComponent<T>();
    }

    private static void RemoveLegacyShopRuntimeObjects()
    {
        string[] legacyNames =
        {
            "EconomyShopWindow",
            "EconomyShopPanel",
            "ShopPanel",
            "UpgradeWindow"
        };

        foreach (string legacyName in legacyNames)
        {
            foreach (GameObject objectInstance in FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (objectInstance.name == legacyName)
                {
                    Destroy(objectInstance);
                }
            }
        }

        foreach (MonoBehaviour behaviour in FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (behaviour == null || behaviour.GetType().Name != "EconomyShopPanel")
            {
                continue;
            }

            Destroy(behaviour.gameObject);
        }
    }
}
