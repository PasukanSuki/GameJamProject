using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NpcSpeechBubble : MonoBehaviour
{
    [Header("Posisi Teks (World Space)")]
    [Tooltip("Ketinggian balon teks dari tanah/pivot NPC (meter). Atur ini agar pas di atas kepala NPC.")]
    [SerializeField, Range(0.5f, 4f)] private float headHeight = 1.85f;

    [Tooltip("Offset tambahan (X, Y, Z) jika posisi ingin digeser sedikit.")]
    [SerializeField] private Vector3 customOffset = Vector3.zero;

    [Header("Arah Hadap")]
    [Tooltip("Apakah teks selalu berputar mengikuti arah pandangan kamera pemain secara real-time.")]
    [SerializeField] private bool facePlayerCamera = true;

    [Header("Visual Settings")]
    [SerializeField] private Vector2 bubbleSize = new Vector2(300f, 95f);
    [SerializeField] private float worldScale = 0.0075f;
    [SerializeField] private Color backgroundColor = new Color(0.1f, 0.12f, 0.16f, 0.95f);
    [SerializeField] private Color speakerColor = new Color(1f, 0.82f, 0.28f, 1f);
    [SerializeField] private Color textColor = Color.white;

    [Header("Components (Auto Created If Null)")]
    [SerializeField] private Canvas worldCanvas;
    [SerializeField] private RectTransform bubbleRect;
    [SerializeField] private TMP_Text speakerText;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private Image bubbleBackground;
    [SerializeField] private CanvasGroup canvasGroup;

    private Coroutine activeDisplayRoutine;
    private Transform cameraTransform;
    private bool isShowing;
    private float targetScale;
    private float currentScale;

    public float HeadHeight
    {
        get => headHeight;
        set => headHeight = value;
    }

    private void Awake()
    {
        EnsureUI();
        DetachCanvasToWorldRoot();
        if (bubbleRect != null)
        {
            bubbleRect.localScale = Vector3.zero;
        }
    }

    private void OnEnable()
    {
        if (worldCanvas != null)
        {
            worldCanvas.gameObject.SetActive(true);
        }
    }

    private void OnDisable()
    {
        if (worldCanvas != null)
        {
            worldCanvas.gameObject.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (worldCanvas != null)
        {
            Destroy(worldCanvas.gameObject);
        }
    }

    private void Start()
    {
        ResolveCamera();
    }

    private void LateUpdate()
    {
        if (cameraTransform == null)
        {
            ResolveCamera();
        }

        // Posisi balon teks selalu berada di atas NPC di koordinat world space
        if (worldCanvas != null)
        {
            Vector3 worldPos = transform.position + (Vector3.up * headHeight) + customOffset;
            worldCanvas.transform.position = worldPos;

            // Selalu menghadap langsung ke arah pandangan kamera pemain
            if (facePlayerCamera && cameraTransform != null)
            {
                worldCanvas.transform.rotation = cameraTransform.rotation;
            }
        }

        // Animasi pop-in / pop-out yang mulus
        if (bubbleRect != null)
        {
            currentScale = Mathf.Lerp(currentScale, targetScale, Time.deltaTime * 14f);
            bubbleRect.localScale = Vector3.one * currentScale;

            if (canvasGroup != null)
            {
                canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, isShowing ? 1f : 0f, Time.deltaTime * 12f);
            }
        }
    }

    private void DetachCanvasToWorldRoot()
    {
        // Melepas canvas dari child NPC agar rotasi / pose miring NPC tidak mendistorsi posisi dan orientasi teks
        if (worldCanvas != null && worldCanvas.transform.parent != null)
        {
            worldCanvas.transform.SetParent(null, true);
        }
    }

    private void ResolveCamera()
    {
        if (Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
            return;
        }

        Player player = FindAnyObjectByType<Player>();
        if (player != null)
        {
            Camera cam = player.GetComponentInChildren<Camera>();
            if (cam != null)
            {
                cameraTransform = cam.transform;
                if (!cam.CompareTag("MainCamera"))
                {
                    cam.tag = "MainCamera";
                }
                return;
            }
            cameraTransform = player.transform;
            return;
        }

        Camera anyCamera = FindAnyObjectByType<Camera>();
        if (anyCamera != null)
        {
            cameraTransform = anyCamera.transform;
            if (!anyCamera.CompareTag("MainCamera"))
            {
                anyCamera.tag = "MainCamera";
            }
        }
    }

    public void ShowMessage(string message, float duration = 3.8f, string speakerName = "Penjual")
    {
        EnsureUI();
        DetachCanvasToWorldRoot();

        if (speakerText != null)
        {
            speakerText.text = speakerName;
            speakerText.gameObject.SetActive(!string.IsNullOrEmpty(speakerName));
        }

        if (dialogueText != null)
        {
            dialogueText.text = message;
        }

        if (activeDisplayRoutine != null)
        {
            StopCoroutine(activeDisplayRoutine);
        }

        activeDisplayRoutine = StartCoroutine(DisplayRoutine(duration));
    }

    public void Hide()
    {
        if (activeDisplayRoutine != null)
        {
            StopCoroutine(activeDisplayRoutine);
            activeDisplayRoutine = null;
        }

        isShowing = false;
        targetScale = 0f;
    }

    private IEnumerator DisplayRoutine(float duration)
    {
        isShowing = true;
        targetScale = 1.08f; // sedikit overshoot pop
        yield return new WaitForSeconds(0.12f);
        targetScale = 1f;

        yield return new WaitForSeconds(Mathf.Max(0.5f, duration));

        isShowing = false;
        targetScale = 0f;
        activeDisplayRoutine = null;
    }

    private void EnsureUI()
    {
        if (worldCanvas != null && bubbleRect != null && dialogueText != null)
        {
            return;
        }

        // 1. Cari atau buat Canvas World Space
        Transform canvasTransform = transform.Find("NpcSpeechCanvas");
        GameObject canvasObj;
        if (canvasTransform == null)
        {
            canvasObj = new GameObject("NpcSpeechCanvas_" + gameObject.name);
            canvasObj.transform.position = transform.position + (Vector3.up * headHeight) + customOffset;
        }
        else
        {
            canvasObj = canvasTransform.gameObject;
        }

        worldCanvas = canvasObj.GetComponent<Canvas>();
        if (worldCanvas == null) worldCanvas = canvasObj.AddComponent<Canvas>();
        worldCanvas.renderMode = RenderMode.WorldSpace;

        CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
        if (scaler == null) scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 3f;

        canvasObj.transform.localScale = Vector3.one * worldScale;

        // 2. Buat Bubble Panel
        Transform bubbleTransform = canvasObj.transform.Find("BubbleCard");
        GameObject bubbleObj;
        if (bubbleTransform == null)
        {
            bubbleObj = new GameObject("BubbleCard", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
            bubbleObj.transform.SetParent(canvasObj.transform, false);
        }
        else
        {
            bubbleObj = bubbleTransform.gameObject;
        }

        bubbleRect = bubbleObj.GetComponent<RectTransform>();
        bubbleRect.sizeDelta = bubbleSize;
        bubbleRect.anchorMin = new Vector2(0.5f, 0f);
        bubbleRect.anchorMax = new Vector2(0.5f, 0f);
        bubbleRect.pivot = new Vector2(0.5f, 0f);
        bubbleRect.anchoredPosition = Vector2.zero;

        canvasGroup = bubbleObj.GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = bubbleObj.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;

        bubbleBackground = bubbleObj.GetComponent<Image>();
        bubbleBackground.color = backgroundColor;
        bubbleBackground.raycastTarget = false;

        // Border aksen tipis di kiri
        Transform borderTransform = bubbleObj.transform.Find("LeftAccent");
        if (borderTransform == null)
        {
            GameObject borderObj = new GameObject("LeftAccent", typeof(RectTransform), typeof(Image));
            borderObj.transform.SetParent(bubbleObj.transform, false);
            Image borderImg = borderObj.GetComponent<Image>();
            borderImg.color = speakerColor;
            borderImg.raycastTarget = false;

            RectTransform borderRect = borderObj.GetComponent<RectTransform>();
            borderRect.anchorMin = new Vector2(0f, 0f);
            borderRect.anchorMax = new Vector2(0f, 1f);
            borderRect.pivot = new Vector2(0f, 0.5f);
            borderRect.anchoredPosition = Vector2.zero;
            borderRect.sizeDelta = new Vector2(5f, 0f);
        }

        // Speaker Name
        Transform speakerTransform = bubbleObj.transform.Find("SpeakerText");
        if (speakerTransform == null)
        {
            GameObject speakerObj = new GameObject("SpeakerText", typeof(RectTransform), typeof(TextMeshProUGUI));
            speakerObj.transform.SetParent(bubbleObj.transform, false);
            speakerText = speakerObj.GetComponent<TextMeshProUGUI>();
            speakerText.text = "Penjual";
            speakerText.fontSize = 17f;
            speakerText.fontStyle = FontStyles.Bold;
            speakerText.color = speakerColor;
            speakerText.alignment = TextAlignmentOptions.Left;
            speakerText.raycastTarget = false;

            RectTransform sRect = speakerObj.GetComponent<RectTransform>();
            sRect.anchorMin = new Vector2(0f, 1f);
            sRect.anchorMax = new Vector2(1f, 1f);
            sRect.pivot = new Vector2(0f, 1f);
            sRect.offsetMin = new Vector2(16f, -32f);
            sRect.offsetMax = new Vector2(-12f, -8f);
        }
        else
        {
            speakerText = speakerTransform.GetComponent<TextMeshProUGUI>();
        }

        // Dialogue Message Text
        Transform dialogueTransform = bubbleObj.transform.Find("DialogueText");
        if (dialogueTransform == null)
        {
            GameObject dialogueObj = new GameObject("DialogueText", typeof(RectTransform), typeof(TextMeshProUGUI));
            dialogueObj.transform.SetParent(bubbleObj.transform, false);
            dialogueText = dialogueObj.GetComponent<TextMeshProUGUI>();
            dialogueText.text = "...";
            dialogueText.fontSize = 15f;
            dialogueText.color = textColor;
            dialogueText.alignment = TextAlignmentOptions.TopLeft;
            dialogueText.enableWordWrapping = true;
            dialogueText.overflowMode = TextOverflowModes.Ellipsis;
            dialogueText.raycastTarget = false;

            RectTransform dRect = dialogueObj.GetComponent<RectTransform>();
            dRect.anchorMin = new Vector2(0f, 0f);
            dRect.anchorMax = new Vector2(1f, 1f);
            dRect.pivot = new Vector2(0.5f, 0.5f);
            dRect.offsetMin = new Vector2(16f, 8f);
            dRect.offsetMax = new Vector2(-12f, -32f);
        }
        else
        {
            dialogueText = dialogueTransform.GetComponent<TextMeshProUGUI>();
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 pos = transform.position + (Vector3.up * headHeight) + customOffset;
        Gizmos.DrawWireSphere(pos, 0.25f);
    }
#endif
}
