using UnityEngine;
using UnityEngine.InputSystem;

public class PanelHotkeyController : MonoBehaviour
{
    [SerializeField] private UpgradePanel upgradePanel;
    [SerializeField] private CraftingPanel craftingPanel;

    private bool missingUpgradeWarningLogged;
    private bool missingKeyboardWarningLogged;

    private void Awake()
    {
        if (upgradePanel == null) upgradePanel = FindAnyObjectByType<UpgradePanel>();
        if (craftingPanel == null) craftingPanel = FindAnyObjectByType<CraftingPanel>();

        Debug.Log(
            $"[PanelHotkeyController] Awake on '{name}'. " +
            $"UpgradePanel={(upgradePanel != null ? upgradePanel.name : "NULL")}, " +
            $"CraftingPanel={(craftingPanel != null ? craftingPanel.name : "NULL")}",
            this);
    }

    private void Update()
    {
        if (upgradePanel == null) upgradePanel = FindAnyObjectByType<UpgradePanel>();
        if (craftingPanel == null) craftingPanel = FindAnyObjectByType<CraftingPanel>();

        if (upgradePanel == null)
        {
            if (!missingUpgradeWarningLogged)
            {
                Debug.LogWarning("[PanelHotkeyController] B cannot toggle upgrade panel: UpgradePanel reference is NULL.", this);
                missingUpgradeWarningLogged = true;
            }
        }
        else
        {
            missingUpgradeWarningLogged = false;
        }

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            if (!missingKeyboardWarningLogged)
            {
                Debug.LogWarning("[PanelHotkeyController] B cannot be read: Keyboard.current is NULL.", this);
                missingKeyboardWarningLogged = true;
            }
            return;
        }

        missingKeyboardWarningLogged = false;

        if (keyboard.bKey.wasPressedThisFrame)
        {
            Debug.Log(
                $"[PanelHotkeyController] B pressed. " +
                $"UpgradePanel={(upgradePanel != null ? upgradePanel.name : "NULL")}, " +
                $"IsOpen={(upgradePanel != null ? upgradePanel.IsOpen.ToString() : "N/A")}",
                this);

            if (upgradePanel == null)
            {
                return;
            }

            CloseOtherPanels();
            upgradePanel.SetOpen(!upgradePanel.IsOpen);

            Debug.Log($"[PanelHotkeyController] UpgradePanel state after B: IsOpen={upgradePanel.IsOpen}.", upgradePanel);
        }

        if (keyboard.qKey.wasPressedThisFrame && craftingPanel != null)
        {
            craftingPanel.SetOpen(!craftingPanel.IsOpen);
        }
    }

    private void CloseOtherPanels()
    {
        if (craftingPanel != null) craftingPanel.SetOpen(false);

        foreach (InventoryPanel inventoryPanel in FindObjectsByType<InventoryPanel>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            inventoryPanel.SetOpen(false);
        }

        foreach (MonoBehaviour behaviour in FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (behaviour != null && behaviour.GetType().Name == "EconomyShopPanel")
            {
                behaviour.gameObject.SetActive(false);
            }
        }
    }
}
