using UnityEngine;
using UnityEngine.UI;

public class CrosshairDot : MonoBehaviour
{
    [Header("Appearance")]
    [SerializeField, Min(2f)] private float dotSize = 6f;
    [SerializeField] private Color dotColor = new Color(1f, 1f, 1f, 0.95f);
    [SerializeField] private bool addShadowOutline = true;
    [SerializeField] private Color outlineColor = new Color(0f, 0f, 0f, 0.65f);

    [Header("References")]
    [SerializeField] private Player player;
    [SerializeField] private GameObject dotRoot;

    private Image dotImage;
    private Outline dotOutline;
    private static Sprite cachedCircleSprite;

    public float DotSize
    {
        get => dotSize;
        set
        {
            dotSize = Mathf.Max(2f, value);
            if (dotRoot != null)
            {
                RectTransform rect = dotRoot.GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.sizeDelta = new Vector2(dotSize, dotSize);
                }
            }
        }
    }

    private void Awake()
    {
        ResolvePlayer();
        BuildRuntimeUIIfNeeded();
    }

    private void Start()
    {
        ResolvePlayer();
        BuildRuntimeUIIfNeeded();
    }

    private void Update()
    {
        UpdateVisibility();
    }

    private void ResolvePlayer()
    {
        if (player == null)
        {
            player = FindAnyObjectByType<Player>();
        }
    }

    private void UpdateVisibility()
    {
        if (dotRoot == null)
        {
            return;
        }

        bool shouldShow = true;

        if (player != null && player.IsPaused)
        {
            shouldShow = false;
        }
        else if (Cursor.lockState != CursorLockMode.Locked)
        {
            shouldShow = false;
        }

        if (dotRoot.activeSelf != shouldShow)
        {
            dotRoot.SetActive(shouldShow);
        }
    }

    public void BuildRuntimeUIIfNeeded()
    {
        if (dotRoot == null)
        {
            Transform existing = transform.Find("CrosshairDot");
            if (existing != null)
            {
                dotRoot = existing.gameObject;
            }
            else
            {
                dotRoot = new GameObject("CrosshairDot", typeof(RectTransform), typeof(Image));
                dotRoot.transform.SetParent(transform, false);
            }
        }

        RectTransform rect = dotRoot.GetComponent<RectTransform>();
        if (rect == null)
        {
            rect = dotRoot.AddComponent<RectTransform>();
        }

        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(dotSize, dotSize);

        dotImage = dotRoot.GetComponent<Image>();
        if (dotImage == null)
        {
            dotImage = dotRoot.AddComponent<Image>();
        }

        dotImage.sprite = GetOrCreateCircleSprite();
        dotImage.color = dotColor;
        dotImage.raycastTarget = false;

        if (addShadowOutline)
        {
            dotOutline = dotRoot.GetComponent<Outline>();
            if (dotOutline == null)
            {
                dotOutline = dotRoot.AddComponent<Outline>();
            }

            dotOutline.effectColor = outlineColor;
            dotOutline.effectDistance = new Vector2(1f, -1f);
        }
    }

    private static Sprite GetOrCreateCircleSprite()
    {
        if (cachedCircleSprite != null)
        {
            return cachedCircleSprite;
        }

        const int size = 32;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };

        float center = (size - 1) * 0.5f;
        float radius = size * 0.45f;

        Color[] pixels = new Color[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                float alpha = Mathf.Clamp01((radius - dist) + 0.5f);
                pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();

        cachedCircleSprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, size, size),
            new Vector2(0.5f, 0.5f),
            size);

        return cachedCircleSprite;
    }
}
