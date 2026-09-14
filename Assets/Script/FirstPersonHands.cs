using UnityEngine;

public class FirstPersonHands : MonoBehaviour
{
    [Header("Hand References")]
    [SerializeField] private RectTransform leftHand;
    [SerializeField] private RectTransform rightHand;

    [Header("Player")]
    [Tooltip("Transform object Player.")]
    [SerializeField] private Transform playerRoot;

    [Header("Walk Animation")]
    [Tooltip("Kecepatan minimum agar dianggap berjalan.")]
    [SerializeField] private float moveThreshold = 0.05f;

    [Tooltip("Kecepatan ayunan tangan saat berjalan.")]
    [SerializeField] private float walkFrequency = 6f;

    [Tooltip("Gerakan naik turun tangan dalam pixel.")]
    [SerializeField] private float verticalBob = 14f;

    [Tooltip("Gerakan kiri kanan tangan dalam pixel.")]
    [SerializeField] private float horizontalBob = 5f;

    [Tooltip("Besar rotasi tangan saat berjalan.")]
    [SerializeField] private float rotationAmount = 3f;

    [Tooltip("Kehalusan animasi jalan.")]
    [SerializeField] private float smoothSpeed = 12f;

    [Header("Grab Animation (Maju Mundur)")]
    [Tooltip("Durasi animasi grab maju-mundur dalam detik.")]
    [SerializeField, Min(0.05f)] private float grabDuration = 0.22f;

    [Tooltip("Offset pergeseran tangan saat maju (X negatif = ke arah tengah layar, Y positif = ke atas/depan).")]
    [SerializeField] private Vector2 grabForwardOffset = new Vector2(-150f, 130f);

    private Vector2 leftStartPosition;
    private Vector2 rightStartPosition;
    private Vector3 leftStartScale = Vector3.one;
    private Vector3 rightStartScale = Vector3.one;
    private Quaternion leftStartRotation = Quaternion.identity;
    private Quaternion rightStartRotation = Quaternion.identity;

    private Vector2 currentLeftBasePos;
    private Vector2 currentRightBasePos;

    private Vector3 lastPlayerPosition;

    private bool initialized;
    private bool isGrabbing;
    private float grabElapsedTime;
    private float grabProgress;

    private void Awake()
    {
        SanitizeGrabSettings();
        InitializeHands();
    }

    private void Start()
    {
        ResolvePlayerRoot();
    }

    private void OnValidate()
    {
        SanitizeGrabSettings();
    }

    private void SanitizeGrabSettings()
    {
        if (grabDuration <= 0.05f)
        {
            grabDuration = 0.22f;
        }

        if (grabForwardOffset == Vector2.zero)
        {
            grabForwardOffset = new Vector2(-150f, 130f);
        }
    }

    private void ResolvePlayerRoot()
    {
        if (playerRoot == null)
        {
            Player player = FindAnyObjectByType<Player>();
            if (player != null)
            {
                playerRoot = player.transform;
            }
        }

        if (playerRoot != null)
        {
            lastPlayerPosition = playerRoot.position;
        }
    }

    private void InitializeHands()
    {
        if (leftHand != null)
        {
            leftStartPosition = leftHand.anchoredPosition;
            leftStartRotation = leftHand.localRotation;
            leftStartScale = leftHand.localScale;
            currentLeftBasePos = leftStartPosition;
            leftHand.gameObject.SetActive(true);
        }

        if (rightHand != null)
        {
            rightStartPosition = rightHand.anchoredPosition;
            rightStartRotation = rightHand.localRotation;
            rightStartScale = rightHand.localScale;
            currentRightBasePos = rightStartPosition;
            rightHand.gameObject.SetActive(true);
        }

        initialized = true;
    }

    private void Update()
    {
        if (!initialized)
        {
            InitializeHands();
            if (!initialized) return;
        }

        UpdateGrabState();

        if (playerRoot == null)
        {
            ResolvePlayerRoot();
            if (playerRoot == null)
            {
                AnimateHands(false);
                return;
            }
        }

        Vector3 currentPosition = playerRoot.position;
        Vector3 movement = currentPosition - lastPlayerPosition;
        lastPlayerPosition = currentPosition;

        movement.y = 0f;

        float speed = 0f;
        if (Time.deltaTime > 0f)
        {
            speed = movement.magnitude / Time.deltaTime;
        }

        bool isMoving = speed > moveThreshold;
        AnimateHands(isMoving);
    }

    private void UpdateGrabState()
    {
        if (!isGrabbing)
        {
            grabProgress = 0f;
            return;
        }

        grabElapsedTime += Time.deltaTime;
        float normalized = Mathf.Clamp01(grabElapsedTime / Mathf.Max(0.01f, grabDuration));

        // Puncak maju pada ~35% durasi (maju cepat), lalu mundur kembali (ease-in-out)
        const float peak = 0.35f;
        if (normalized <= peak)
        {
            float t = normalized / peak;
            grabProgress = Mathf.Sin(t * Mathf.PI * 0.5f);
        }
        else
        {
            float t = (normalized - peak) / (1f - peak);
            grabProgress = 0.5f * (1f + Mathf.Cos(t * Mathf.PI));
        }

        if (grabElapsedTime >= grabDuration)
        {
            isGrabbing = false;
            grabProgress = 0f;
        }
    }

    private void AnimateHands(bool isMoving)
    {
        if (leftHand == null || rightHand == null)
        {
            return;
        }

        float wave = 0f;
        if (isMoving)
        {
            wave = Mathf.Sin(Time.time * walkFrequency);
        }

        // Tangan kiri dan kanan berayun berlawanan saat berjalan
        Vector2 leftBob = new Vector2(wave * horizontalBob, wave * verticalBob);
        Vector2 rightBob = new Vector2(-wave * horizontalBob, -wave * verticalBob);

        Vector2 leftTargetBase = leftStartPosition + leftBob;
        Vector2 rightTargetBase = rightStartPosition + rightBob;

        Quaternion leftRotationTarget = leftStartRotation * Quaternion.Euler(0f, 0f, wave * rotationAmount);
        Quaternion rightRotationTarget = rightStartRotation * Quaternion.Euler(0f, 0f, -wave * rotationAmount);

        float smoothFactor = 1f - Mathf.Exp(-smoothSpeed * Time.deltaTime);

        currentLeftBasePos = Vector2.Lerp(currentLeftBasePos, leftTargetBase, smoothFactor);
        currentRightBasePos = Vector2.Lerp(currentRightBasePos, rightTargetBase, smoothFactor);

        // Efek maju-mundur grab:
        // HANYA pergeseran posisi maju-mundur pada tangan kanan (tanpa scale / rotasi aneh)
        Vector2 grabOffset = grabForwardOffset * grabProgress;

        leftHand.anchoredPosition = currentLeftBasePos;
        rightHand.anchoredPosition = currentRightBasePos + grabOffset;

        // Skala tetap normal
        leftHand.localScale = leftStartScale;
        rightHand.localScale = rightStartScale;

        // Rotasi hanya mengikuti ayunan jalan
        leftHand.localRotation = Quaternion.Lerp(leftHand.localRotation, leftRotationTarget, smoothFactor);
        rightHand.localRotation = Quaternion.Lerp(rightHand.localRotation, rightRotationTarget, smoothFactor);
    }

    public void PlayGrab()
    {
        SanitizeGrabSettings();
        isGrabbing = true;
        grabElapsedTime = 0f;
    }

    public bool IsGrabbing()
    {
        return isGrabbing;
    }
}