using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryFullWarning : MonoBehaviour
{
    public static InventoryFullWarning Instance { get; private set; }

    [Header("References")]
    [SerializeField] private Player player;
    [SerializeField] private InventoryManager inventory;

    [Header("UI Components")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform panelRect;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image accentBar;

    [Header("Settings")]
    [SerializeField] private Vector2 hudOffset = new Vector2(-24f, -24f);
    [SerializeField] private Vector2 hudSize = new Vector2(300f, 76f);
    [SerializeField] private float fadeSpeed = 9f;
    [SerializeField] private float pulseFrequency = 4f;

    private bool isFull;
    private float alertShakeTimer;
    private Vector2 baseAnchoredPosition;
    private Color titleOriginalColor;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        BuildRuntimeUIIfNeeded();
    }

    private void OnEnable()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        BuildRuntimeUIIfNeeded();
        BindEvents();
        CheckFullStatus();
    }

    private void Start()
    {
        BindEvents();
        CheckFullStatus();
    }

    private void OnDisable()
    {
        UnbindEvents();
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void OnDestroy()
    {
        UnbindEvents();
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void BindEvents()
    {
        if (player == null)
        {
            player = FindAnyObjectByType<Player>();
        }

        if (player != null)
        {
            player.BagChanged -= HandleBagChanged;
            player.BagChanged += HandleBagChanged;
        }

        if (inventory == null)
        {
            inventory = FindAnyObjectByType<InventoryManager>();
        }

        if (inventory != null)
        {
            inventory.InventoryChanged -= HandleInventoryChanged;
            inventory.InventoryChanged += HandleInventoryChanged;
        }
    }

    private void UnbindEvents()
    {
        if (player != null)
        {
            player.BagChanged -= HandleBagChanged;
        }

        if (inventory != null)
        {
            inventory.InventoryChanged -= HandleInventoryChanged;
        }
    }

    private void HandleBagChanged(int carried, int capacity)
    {
        CheckFullStatus();
    }

    private void HandleInventoryChanged()
    {
        CheckFullStatus();
    }

    public void CheckFullStatus()
    {
        if (player == null)
        {
            player = FindAnyObjectByType<Player>();
        }

        bool bagIsFull = player != null && player.IsBagFull;
        bool invIsFull = IsInventoryItemSlotsFull();

        bool currentlyFull = bagIsFull || invIsFull;

        if (currentlyFull != isFull)
        {
            isFull = currentlyFull;
            if (isFull)
            {
                // Set text sesuai jenis yang penuh
                if (bagIsFull && messageText != null)
                {
                    if (titleText != null) titleText.text = "PERINGATAN: TAS PENUH!";
                    messageText.text = "Kapasitas sampah penuh!\nKosongkan di Mesin Daur Ulang.";
                }
                else if (invIsFull && messageText != null)
                {
                    if (titleText != null) titleText.text = "INVENTORY PENUH!";
                    messageText.text = "Slot penyimpanan barang penuh!";
                }

                alertShakeTimer = 0.35f;
            }
        }
    }

    private bool IsInventoryItemSlotsFull()
    {
        if (inventory == null || inventory.Slots == null || inventory.Slots.Count == 0)
        {
            return false;
        }

        foreach (ItemStack slot in inventory.Slots)
        {
            if (slot == null || slot.IsEmpty)
            {
                return false;
            }
        }

        return true;
    }

    private void Update()
    {
        if (player == null)
        {
            BindEvents();
        }

        // Periksa status secara berkala untuk memastikan selalu sinkron
        if (player != null)
        {
            bool bagFull = player.IsBagFull;
            if (bagFull != isFull)
            {
                CheckFullStatus();
            }
        }

        if (canvasGroup != null)
        {
            float targetAlpha = isFull ? 1f : 0f;
            canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, targetAlpha, fadeSpeed * Time.deltaTime);

            if (!isFull && canvasGroup.alpha <= 0.01f)
            {
                if (panelRect != null)
                {
                    panelRect.anchoredPosition = baseAnchoredPosition;
                }
                return;
            }
        }

        // Animasi getar / pulse saat baru penuh atau saat klik mengambil sampah ketika penuh
        if (panelRect != null)
        {
            Vector2 offset = Vector2.zero;
            if (alertShakeTimer > 0f)
            {
                alertShakeTimer -= Time.deltaTime;
                float shakeMagnitude = alertShakeTimer * 12f;
                offset = new Vector2(
                    Mathf.Sin(Time.time * 60f) * shakeMagnitude,
                    Mathf.Cos(Time.time * 50f) * (shakeMagnitude * 0.5f)
                );
            }

            panelRect.anchoredPosition = baseAnchoredPosition + offset;
        }

        // Efek pulse halus pada warna teks judul saat aktif
        if (isFull && titleText != null)
        {
            float pulse = 0.8f + 0.2f * Mathf.Sin(Time.time * pulseFrequency);
            titleText.color = new Color(1f, 0.22f * pulse, 0.22f * pulse, 1f);
        }
    }

    public static void TriggerAlert()
    {
        if (Instance != null)
        {
            Instance.CheckFullStatus();
            Instance.alertShakeTimer = 0.3f;
        }
    }

    private void BuildRuntimeUIIfNeeded()
    {
        if (panelRect != null && titleText != null && messageText != null && canvasGroup != null)
        {
            baseAnchoredPosition = panelRect.anchoredPosition;
            return;
        }

        // Setup container di pojok kanan atas Canvas
        GameObject rootObj = gameObject;
        panelRect = GetComponent<RectTransform>();
        if (panelRect == null)
        {
            panelRect = rootObj.AddComponent<RectTransform>();
        }

        // Posisi di kanan atas
        panelRect.anchorMin = new Vector2(1f, 1f);
        panelRect.anchorMax = new Vector2(1f, 1f);
        panelRect.pivot = new Vector2(1f, 1f);
        panelRect.anchoredPosition = hudOffset;
        panelRect.sizeDelta = hudSize;
        baseAnchoredPosition = hudOffset;

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = rootObj.AddComponent<CanvasGroup>();
        }
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        // Background Box
        backgroundImage = GetComponent<Image>();
        if (backgroundImage == null)
        {
            backgroundImage = rootObj.AddComponent<Image>();
        }
        backgroundImage.color = new Color(0.12f, 0.05f, 0.05f, 0.92f);
        backgroundImage.raycastTarget = false;

        // Accent Bar kiri (warna merah tegas)
        Transform accentTransform = transform.Find("AccentBar");
        if (accentTransform == null)
        {
            GameObject accentObj = new GameObject("AccentBar", typeof(RectTransform), typeof(Image));
            accentObj.transform.SetParent(transform, false);
            accentBar = accentObj.GetComponent<Image>();
            accentBar.color = new Color(1f, 0.23f, 0.19f, 1f);
            accentBar.raycastTarget = false;

            RectTransform barRect = accentObj.GetComponent<RectTransform>();
            barRect.anchorMin = new Vector2(0f, 0f);
            barRect.anchorMax = new Vector2(0f, 1f);
            barRect.pivot = new Vector2(0f, 0.5f);
            barRect.anchoredPosition = Vector2.zero;
            barRect.sizeDelta = new Vector2(6f, 0f);
        }
        else
        {
            accentBar = accentTransform.GetComponent<Image>();
        }

        // Title Text
        Transform titleTransform = transform.Find("TitleText");
        if (titleTransform == null)
        {
            GameObject titleObj = new GameObject("TitleText", typeof(RectTransform), typeof(TextMeshProUGUI));
            titleObj.transform.SetParent(transform, false);
            titleText = titleObj.GetComponent<TextMeshProUGUI>();
            titleText.text = "PERINGATAN: TAS PENUH!";
            titleText.fontSize = 17f;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = new Color(1f, 0.28f, 0.25f, 1f);
            titleText.alignment = TextAlignmentOptions.Left;
            titleText.raycastTarget = false;

            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0f, 0.5f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.pivot = new Vector2(0f, 1f);
            titleRect.offsetMin = new Vector2(16f, 0f);
            titleRect.offsetMax = new Vector2(-12f, -8f);
        }
        else
        {
            titleText = titleTransform.GetComponent<TextMeshProUGUI>();
        }

        // Message / Info Text
        Transform messageTransform = transform.Find("MessageText");
        if (messageTransform == null)
        {
            GameObject msgObj = new GameObject("MessageText", typeof(RectTransform), typeof(TextMeshProUGUI));
            msgObj.transform.SetParent(transform, false);
            messageText = msgObj.GetComponent<TextMeshProUGUI>();
            messageText.text = "Kapasitas sampah penuh!\nKosongkan di Mesin Daur Ulang.";
            messageText.fontSize = 12.5f;
            messageText.color = new Color(0.92f, 0.92f, 0.92f, 1f);
            messageText.alignment = TextAlignmentOptions.Left;
            messageText.lineSpacing = -10f;
            messageText.raycastTarget = false;

            RectTransform msgRect = msgObj.GetComponent<RectTransform>();
            msgRect.anchorMin = new Vector2(0f, 0f);
            msgRect.anchorMax = new Vector2(1f, 0.5f);
            msgRect.pivot = new Vector2(0f, 0f);
            msgRect.offsetMin = new Vector2(16f, 6f);
            msgRect.offsetMax = new Vector2(-12f, 0f);
        }
        else
        {
            messageText = messageTransform.GetComponent<TextMeshProUGUI>();
        }
    }
}
