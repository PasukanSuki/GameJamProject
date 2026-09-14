using TMPro;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class WalletCounter : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private CurrencyWallet wallet;
    [SerializeField] private TMP_Text counterText;
    [SerializeField] private Image iconImage;
    [SerializeField] private Sprite currencyIcon;
    [SerializeField] private Vector2 hudSize = new Vector2(260f, 64f);
    [SerializeField] private Vector2 hudOffset = new Vector2(24f, -24f);

    private void Reset()
    {
        counterText = GetComponent<TMP_Text>();
        iconImage = GetComponentInChildren<Image>();
    }

    private void OnEnable()
    {
        BuildRuntimeUIIfNeeded();
        BindWallet();
    }

    private void Start()
    {
        BindWallet();
    }

    private void BindWallet()
    {
        if (wallet != null)
        {
            wallet.BalanceChanged -= Refresh;
        }

        if (player == null)
        {
            player = FindAnyObjectByType<Player>();
        }

        CurrencyWallet resolvedWallet = player != null
            ? player.GetComponent<CurrencyWallet>()
            : FindAnyObjectByType<CurrencyWallet>();

        if (wallet != resolvedWallet)
        {
            wallet = resolvedWallet;
        }

        if (counterText == null)
        {
            counterText = GetComponentInChildren<TMP_Text>();
        }

        ResolveCurrencyIcon();

        if (wallet != null)
        {
            wallet.BalanceChanged += Refresh;
            Refresh(wallet.Balance);
        }
    }

    private void OnDisable()
    {
        if (wallet != null)
        {
            wallet.BalanceChanged -= Refresh;
        }
    }

    private void Refresh(int balance)
    {
        if (counterText != null)
        {
            counterText.text = balance.ToString("N0");
        }
    }

    private void BuildRuntimeUIIfNeeded()
    {
        if (counterText != null && iconImage != null)
        {
            return;
        }

        RectTransform root = GetComponent<RectTransform>();
        if (root == null)
        {
            root = gameObject.AddComponent<RectTransform>();
        }

        if (root.parent == null || root.GetComponent<Canvas>() != null)
        {
            root.anchorMin = new Vector2(0f, 1f);
            root.anchorMax = new Vector2(0f, 1f);
            root.pivot = new Vector2(0f, 1f);
            root.anchoredPosition = hudOffset;
            root.sizeDelta = hudSize;
        }

        if (iconImage == null)
        {
            GameObject iconObject = new GameObject("CurrencyIcon", typeof(RectTransform), typeof(Image));
            iconObject.transform.SetParent(transform, false);
            iconImage = iconObject.GetComponent<Image>();
            RectTransform iconRect = iconObject.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0f, 0.5f);
            iconRect.anchorMax = new Vector2(0f, 0.5f);
            iconRect.pivot = new Vector2(0f, 0.5f);
            iconRect.anchoredPosition = Vector2.zero;
            iconRect.sizeDelta = new Vector2(72f, 48f);
        }

        if (counterText == null)
        {
            GameObject textObject = new GameObject("CurrencyAmount", typeof(RectTransform), typeof(TextMeshProUGUI));
            textObject.transform.SetParent(transform, false);
            counterText = textObject.GetComponent<TextMeshProUGUI>();
            counterText.alignment = TextAlignmentOptions.MidlineLeft;
            counterText.fontSize = 28f;
            counterText.color = Color.white;
            counterText.raycastTarget = false;
            RectTransform textRect = textObject.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0f, 0f);
            textRect.anchorMax = new Vector2(1f, 1f);
            textRect.offsetMin = new Vector2(88f, 0f);
            textRect.offsetMax = Vector2.zero;
        }

        iconImage.preserveAspect = true;
        iconImage.raycastTarget = false;
    }

    private void ResolveCurrencyIcon()
    {
        if (currencyIcon == null)
        {
#if UNITY_EDITOR
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath("Assets/Sprites/Uang.png");
            foreach (Object asset in assets)
            {
                if (asset is Sprite sprite && sprite.name == "Uang_0")
                {
                    currencyIcon = sprite;
                    break;
                }
            }
#endif
        }

        if (iconImage != null)
        {
            iconImage.sprite = currencyIcon;
            iconImage.enabled = currencyIcon != null;
        }
    }
}