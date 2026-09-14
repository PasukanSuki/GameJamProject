using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UpgradePanel : MonoBehaviour
{
    private static UpgradePanel activeInstance;

    [SerializeField] private Player player;
    [SerializeField] private PlayerUpgradeManager manager;
    [SerializeField] private CurrencyWallet wallet;
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Transform cardRoot;
    [SerializeField] private TMP_Text balanceText;
    [SerializeField] private TMP_Text feedbackText;
    [SerializeField] private Button exitButton;
    [SerializeField] private KeyCode fallbackToggleKey = KeyCode.U;

    private CanvasGroup panelCanvasGroup;

    private readonly List<UpgradeCardView> cards = new List<UpgradeCardView>();
    private InputAction openUpgradeAction;
    private bool isOpen;

    public bool IsOpen => isOpen;

    private void Awake()
    {
        if (activeInstance != null && activeInstance != this)
        {
            Destroy(gameObject);
            return;
        }

        activeInstance = this;
        player = player != null ? player : FindAnyObjectByType<Player>();
        manager = manager != null ? manager : FindAnyObjectByType<PlayerUpgradeManager>();
        wallet = wallet != null ? wallet : player != null ? player.GetComponent<CurrencyWallet>() : FindAnyObjectByType<CurrencyWallet>();
        CacheInputAction();
        RemoveLegacyUpgradeWindow();
        BuildRuntimeUIIfNeeded();
        BindExitButton();
        SetOpen(false);

        Debug.Log(
            $"[UpgradePanel] Awake on '{name}'. " +
            $"Player={(player != null ? player.name : "NULL")}, " +
            $"Manager={(manager != null ? manager.name : "NULL")}, " +
            $"PanelRoot={(panelRoot != null ? panelRoot.name : "NULL")}, " +
            $"FallbackKey={fallbackToggleKey}, " +
            $"OpenUpgradeAction={(openUpgradeAction != null ? openUpgradeAction.name : "NULL")}",
            this);
    }

    private void OnEnable()
    {
        BindEvents();
    }

    private void OnDisable()
    {
        UnbindEvents();
    }

    private void OnDestroy()
    {
        if (activeInstance == this)
        {
            activeInstance = null;
        }
    }

    private void Update()
    {
    }

    private void Start()
    {
        player = player != null ? player : FindAnyObjectByType<Player>();
        manager = manager != null ? manager : FindAnyObjectByType<PlayerUpgradeManager>();
        wallet = wallet != null
            ? wallet
            : player != null ? player.GetComponent<CurrencyWallet>() : FindAnyObjectByType<CurrencyWallet>();

        BindEvents();
        BuildRuntimeUIIfNeeded();
        BindExitButton();

        Debug.Log(
            $"[UpgradePanel] Start dependency check. " +
            $"Player={(player != null ? player.name : "NULL")}, " +
            $"Manager={(manager != null ? manager.name : "NULL")}, " +
            $"Cards={cards.Count}",
            this);

        if (manager == null)
        {
            Debug.LogError("[UpgradePanel] PlayerUpgradeManager is still NULL after Start; upgrade cards cannot be created.", this);
        }
    }

    private void BindEvents()
    {
        if (wallet != null)
        {
            wallet.BalanceChanged -= Refresh;
            wallet.BalanceChanged += Refresh;
        }

        if (manager != null)
        {
            manager.UpgradePurchased -= HandleUpgradePurchased;
            manager.UpgradePurchased += HandleUpgradePurchased;
        }
    }

    private void UnbindEvents()
    {
        if (wallet != null) wallet.BalanceChanged -= Refresh;
        if (manager != null) manager.UpgradePurchased -= HandleUpgradePurchased;
    }

    public void SetOpen(bool open)
    {
        Debug.Log(
            $"[UpgradePanel] SetOpen({open}) called on '{name}'. " +
            $"PanelRoot={(panelRoot != null ? panelRoot.name : "NULL")}, " +
            $"CanvasGroup={(panelCanvasGroup != null ? "cached" : "not cached")}",
            this);

        isOpen = open;
        if (panelCanvasGroup == null && panelRoot != null)
        {
            panelCanvasGroup = panelRoot.GetComponent<CanvasGroup>();
        }

        if (panelCanvasGroup != null)
        {
            panelCanvasGroup.alpha = open ? 1f : 0f;
            panelCanvasGroup.interactable = open;
            panelCanvasGroup.blocksRaycasts = open;
        }
        if (open)
        {
            CloseOtherPanels();
        }
        player?.SetPaused(open);
        if (open)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Refresh(wallet != null ? wallet.Balance : 0);
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void Refresh(int balance)
    {
        if (balanceText != null) balanceText.text = $"MONEY  {balance:N0}$";
        foreach (UpgradeCardView card in cards) card.Refresh();
    }

    private void HandleUpgradePurchased(UpgradeDefinition upgrade, int level)
    {
        feedbackText.text = $"{upgrade.DisplayName} upgraded to level {level}";
        Refresh(wallet != null ? wallet.Balance : 0);
    }

    private void CacheInputAction()
    {
        InputActionMap playerMap = InputSystem.actions.FindActionMap("Player", false);
        openUpgradeAction = playerMap?.FindAction("OpenUpgrade", false);

        string binding = openUpgradeAction != null && openUpgradeAction.bindings.Count > 0
            ? openUpgradeAction.bindings[0].path
            : "NONE";
        Debug.Log(
            $"[UpgradePanel] OpenUpgrade action lookup: map={(playerMap != null ? "FOUND" : "NULL")}, " +
            $"action={(openUpgradeAction != null ? "FOUND" : "NULL")}, binding={binding}. " +
            $"Note: this panel currently toggles through PanelHotkeyController.",
            this);
    }

    private void BuildRuntimeUIIfNeeded()
    {
        ResolveSerializedHierarchy();

        if (panelRoot == null)
        {
            panelRoot = gameObject;
            RectTransform rect = GetComponent<RectTransform>();
            if (rect == null) rect = gameObject.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(940f, 700f);
            Image background = GetComponent<Image>();
            if (background == null) background = gameObject.AddComponent<Image>();
            background.color = new Color(0.29f, 0.55f, 0.86f, 0.98f);
            panelCanvasGroup = gameObject.GetComponent<CanvasGroup>();
            if (panelCanvasGroup == null) panelCanvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        if (panelCanvasGroup == null) panelCanvasGroup = gameObject.GetComponent<CanvasGroup>();
        if (panelCanvasGroup == null) panelCanvasGroup = gameObject.AddComponent<CanvasGroup>();

        if (balanceText == null) balanceText = FindText("Balance");
        if (feedbackText == null) feedbackText = FindText("Feedback");
        if (cardRoot == null)
        {
            Debug.LogWarning(
                $"[UpgradePanel] Authored panel '{name}' is missing the 'UpgradeCards' hierarchy. " +
                "Runtime will not create a replacement layout.",
                this);
            return;
        }

        if (cards.Count == 0 && manager != null)
        {
            List<UpgradeDefinition> sorted = new List<UpgradeDefinition>(manager.Upgrades);
            sorted.Sort((left, right) => left.SortOrder.CompareTo(right.SortOrder));
            HashSet<Transform> configuredCards = new HashSet<Transform>();
            foreach (UpgradeDefinition upgrade in sorted)
            {
                UpgradeDefinition captured = upgrade;
                Transform existingCard = FindCardForUpgrade(captured, cardRoot);
                if (existingCard == null)
                {
                    Debug.LogWarning(
                        $"[UpgradePanel] Authored panel is missing card '{GetCardName(captured)}'. " +
                        "Runtime will not create a replacement card.",
                        this);
                    continue;
                }

                UpgradeCardView card = existingCard.GetComponent<UpgradeCardView>();
                if (card == null)
                {
                    card = existingCard.gameObject.AddComponent<UpgradeCardView>();
                }

                if (card.ConfigureExisting(captured, manager, () => Purchase(captured)))
                {
                    configuredCards.Add(existingCard);
                    cards.Add(card);
                }
            }

            for (int index = 0; index < cardRoot.childCount; index++)
            {
                Transform child = cardRoot.GetChild(index);
                if (!configuredCards.Contains(child) && IsDisabledCard(child))
                {
                    child.gameObject.SetActive(false);
                }
            }
        }
    }

    private static bool IsDisabledCard(Transform card)
    {
        return card.name == "PickupSpeedCard" || card.name == "FullbagBonusCard";
    }

    private static Transform FindCardForUpgrade(UpgradeDefinition upgrade, Transform cardRoot)
    {
        string cardName = GetCardName(upgrade);

        return string.IsNullOrEmpty(cardName) ? null : cardRoot.Find(cardName);
    }

    private static string GetCardName(UpgradeDefinition upgrade)
    {
        return upgrade.UpgradeType == PlayerUpgradeType.BagCapacity
            ? "BagCapacityCard"
            : upgrade.UpgradeType == PlayerUpgradeType.Income
                ? "IncomeCard"
                : string.Empty;
    }

    private void ResolveSerializedHierarchy()
    {
        if (panelRoot == null)
        {
            panelRoot = gameObject;
        }

        if (balanceText == null)
        {
            balanceText = FindText("Balance");
        }

        if (feedbackText == null)
        {
            feedbackText = FindText("Feedback");
        }

        if (cardRoot == null)
        {
            Transform existingCards = transform.Find("UpgradeCards");
            if (existingCards != null)
            {
                cardRoot = existingCards;
            }
        }
    }

    private void BindExitButton()
    {
        if (exitButton == null)
        {
            Transform exitTransform = FindChildRecursive(transform, "ExitButton");
            if (exitTransform != null)
            {
                exitButton = exitTransform.GetComponent<Button>();
            }
        }

        if (exitButton == null)
        {
            Debug.LogWarning("[UpgradePanel] ExitButton was not found, so the exit button cannot close the panel.", this);
            return;
        }

        exitButton.onClick.RemoveListener(CloseFromButton);
        exitButton.onClick.AddListener(CloseFromButton);
        Debug.Log($"[UpgradePanel] ExitButton bound on '{exitButton.name}'.", exitButton);
    }

    private void CloseFromButton()
    {
        Debug.Log("[UpgradePanel] ExitButton clicked.", this);
        SetOpen(false);
    }

    private TMP_Text FindText(string objectName)
    {
        Transform child = FindChildRecursive(transform, objectName);
        return child != null ? child.GetComponent<TMP_Text>() : null;
    }

    private static Transform FindChildRecursive(Transform parent, string objectName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == objectName)
            {
                return child;
            }

            Transform result = FindChildRecursive(child, objectName);
            if (result != null)
            {
                return result;
            }
        }

        return null;
    }

    private void RemoveLegacyUpgradeWindow()
    {
        Transform legacyWindow = transform.Find("UpgradeWindow");
        if (legacyWindow != null)
        {
            Destroy(legacyWindow.gameObject);
        }
    }

    private bool IsFallbackKeyPressed()
    {
        if (Keyboard.current == null)
        {
            return false;
        }

        switch (fallbackToggleKey)
        {
            case KeyCode.B: return Keyboard.current.bKey.wasPressedThisFrame;
            case KeyCode.Q: return Keyboard.current.qKey.wasPressedThisFrame;
            case KeyCode.U: return Keyboard.current.uKey.wasPressedThisFrame;
            default: return false;
        }
    }

    private void CloseOtherPanels()
    {
        foreach (CraftingPanel craftingPanel in FindObjectsByType<CraftingPanel>(FindObjectsSortMode.None))
        {
            craftingPanel.SetOpen(false);
        }

        foreach (InventoryPanel inventoryPanel in FindObjectsByType<InventoryPanel>(FindObjectsSortMode.None))
        {
            inventoryPanel.SetOpen(false);
        }
    }

    private void Purchase(UpgradeDefinition upgrade)
    {
        if (manager != null && !manager.TryPurchase(upgrade))
        {
            feedbackText.text = "Not enough money or maximum level reached";
        }
    }

    private TMP_Text CreateText(string name, Transform parent, Vector2 min, Vector2 max, float fontSize)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);
        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        text.fontSize = fontSize;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Center;
        text.raycastTarget = false;
        return text;
    }
}
