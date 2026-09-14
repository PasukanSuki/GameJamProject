using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InventoryPanel : MonoBehaviour
{
    [SerializeField] private InventoryManager inventory;
    [SerializeField] private Player player;
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Transform gridRoot;
    [SerializeField] private ItemDescriptionPanel descriptionPanel;
    [SerializeField] private Vector2 panelSize = new Vector2(960f, 600f);

    [SerializeField] private List<InventorySlotView> slotViews = new List<InventorySlotView>();
    private int selectedSlot = -1;
    private bool isOpen;
    private InputAction openInventoryAction;

    private void Awake()
    {
        if (inventory == null)
        {
            inventory = FindAnyObjectByType<InventoryManager>();
        }

        if (player == null)
        {
            player = FindAnyObjectByType<Player>();
        }

        BuildRuntimeUIIfNeeded();
        CacheSceneSlots();
        CacheInputAction();
        isOpen = false;
        panelRoot.SetActive(false);
    }

    private void OnEnable()
    {
        if (inventory != null)
        {
            inventory.InventoryChanged += Refresh;
        }
    }

    private void OnDisable()
    {
        if (inventory != null)
        {
            inventory.InventoryChanged -= Refresh;
        }
    }

    private void Update()
    {
        bool pressed = openInventoryAction != null
            ? openInventoryAction.WasPressedThisFrame()
            : Keyboard.current != null && Keyboard.current.tabKey.wasPressedThisFrame;

        if (pressed)
        {
            Toggle();
        }
    }

    public void Toggle()
    {
        SetOpen(!isOpen);
    }

    public void SetOpen(bool open)
    {
        isOpen = open;
        panelRoot.SetActive(open);

        if (open)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Refresh();
        }
        else
        {
            selectedSlot = -1;
            player?.SetPaused(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        if (open)
        {
            player?.SetPaused(true);
        }
    }

    private void Refresh()
    {
        if (inventory == null)
        {
            return;
        }

        EnsureSlotViews();

        ItemStack selected = inventory.GetSlot(selectedSlot);
        if (selected == null || selected.IsEmpty)
        {
            selectedSlot = -1;
            selected = null;
        }

        for (int index = 0; index < slotViews.Count; index++)
        {
            slotViews[index].Refresh(inventory.GetSlot(index), index == selectedSlot);
        }

        descriptionPanel.Show(selected);
    }

    private void SelectSlot(int index)
    {
        ItemStack selected = inventory.GetSlot(index);
        selectedSlot = selected != null && !selected.IsEmpty ? index : -1;
        Refresh();
    }

    private void BuildRuntimeUIIfNeeded()
    {
        if (panelRoot == null)
        {
            panelRoot = CreatePanelRoot(transform);
        }

        if (gridRoot == null)
        {
            GameObject gridObject = new GameObject("ItemGrid", typeof(RectTransform), typeof(GridLayoutGroup));
            gridObject.transform.SetParent(panelRoot.transform, false);
            RectTransform gridRect = gridObject.GetComponent<RectTransform>();
            gridRect.anchorMin = new Vector2(0.06f, 0.12f);
            gridRect.anchorMax = new Vector2(0.56f, 0.88f);
            gridRect.offsetMin = Vector2.zero;
            gridRect.offsetMax = Vector2.zero;
            GridLayoutGroup grid = gridObject.GetComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(76f, 76f);
            grid.spacing = new Vector2(10f, 10f);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 4;
            gridRoot = gridObject.transform;
        }

        if (descriptionPanel == null)
        {
            GameObject descriptionObject = new GameObject("DescriptionPanel", typeof(RectTransform));
            descriptionObject.transform.SetParent(panelRoot.transform, false);
            RectTransform descriptionRect = descriptionObject.GetComponent<RectTransform>();
            descriptionRect.anchorMin = new Vector2(0.62f, 0.12f);
            descriptionRect.anchorMax = new Vector2(0.94f, 0.88f);
            descriptionRect.offsetMin = Vector2.zero;
            descriptionRect.offsetMax = Vector2.zero;
            descriptionPanel = ItemDescriptionPanel.Create(descriptionObject.transform);
        }

        CacheSceneSlots();

        EnsureSlotViews();
    }

    private void EnsureSlotViews()
    {
        if (inventory == null || gridRoot == null)
        {
            return;
        }

        int slotCount = inventory.SlotCapacity;
        for (int index = slotViews.Count; index < slotCount; index++)
        {
            slotViews.Add(InventorySlotView.Create(gridRoot, index, SelectSlot));
        }

        for (int index = 0; index < slotViews.Count; index++)
        {
            slotViews[index].Configure(index, SelectSlot);
            slotViews[index].gameObject.SetActive(index < slotCount);
        }
    }

    private void CacheSceneSlots()
    {
        if (gridRoot == null)
        {
            return;
        }

        InventorySlotView[] sceneSlots = gridRoot.GetComponentsInChildren<InventorySlotView>(true);
        if (sceneSlots.Length == 0)
        {
            return;
        }

        slotViews.Clear();
        slotViews.AddRange(sceneSlots);

        for (int index = 0; index < slotViews.Count; index++)
        {
            slotViews[index].Configure(index, SelectSlot);
        }
    }

    private void CacheInputAction()
    {
        InputActionMap playerMap = InputSystem.actions.FindActionMap("Player", false);
        openInventoryAction = playerMap?.FindAction("OpenInventory", false);
    }

    private GameObject CreatePanelRoot(Transform parent)
    {
        GameObject root = new GameObject("InventoryWindow", typeof(RectTransform), typeof(Image));
        root.transform.SetParent(parent, false);
        RectTransform rootRect = root.GetComponent<RectTransform>();
        rootRect.anchorMin = new Vector2(0.5f, 0.5f);
        rootRect.anchorMax = new Vector2(0.5f, 0.5f);
        rootRect.pivot = new Vector2(0.5f, 0.5f);
        rootRect.sizeDelta = panelSize;
        root.GetComponent<Image>().color = new Color(0.035f, 0.055f, 0.065f, 0.98f);
        return root;
    }
}