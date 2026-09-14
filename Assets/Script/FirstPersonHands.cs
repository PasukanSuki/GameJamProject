using UnityEngine;

public class FirstPersonHands : MonoBehaviour
{
    [Header("Hand References")]
    [SerializeField] private RectTransform leftHand;
    [SerializeField] private RectTransform rightHand;

    [Header("Player")]
    [Tooltip("Masukkan Transform object Player, bukan Main Camera.")]
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

    [Tooltip("Kehalusan animasi.")]
    [SerializeField] private float smoothSpeed = 12f;

    private Vector2 leftStartPosition;
    private Vector2 rightStartPosition;

    private Quaternion leftStartRotation;
    private Quaternion rightStartRotation;

    private Vector3 lastPlayerPosition;

    private bool initialized;


    private void Awake()
    {
        InitializeHands();
    }


    private void Start()
    {
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
        }

        if (rightHand != null)
        {
            rightStartPosition = rightHand.anchoredPosition;
            rightStartRotation = rightHand.localRotation;
        }

        if (leftHand != null)
        {
            leftHand.gameObject.SetActive(true);
        }

        if (rightHand != null)
        {
            rightHand.gameObject.SetActive(true);
        }

        initialized = true;
    }


    private void Update()
    {
        if (!initialized)
        {
            return;
        }

        if (playerRoot == null)
        {
            AnimateHands(false);
            return;
        }

        Vector3 currentPosition = playerRoot.position;

        Vector3 movement = currentPosition - lastPlayerPosition;

        lastPlayerPosition = currentPosition;

        // Abaikan gerakan vertikal.
        // Jadi lompat/jatuh tidak dianggap sebagai jalan.
        movement.y = 0f;

        float speed = 0f;

        if (Time.deltaTime > 0f)
        {
            speed = movement.magnitude / Time.deltaTime;
        }

        bool isMoving = speed > moveThreshold;

        AnimateHands(isMoving);
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

        /*
         * Tangan kiri dan kanan menggunakan wave yang berlawanan.
         *
         * LEFT naik  -> RIGHT turun
         * LEFT turun -> RIGHT naik
         */

        Vector2 leftTarget =
            leftStartPosition +
            new Vector2(
                wave * horizontalBob,
                wave * verticalBob
            );

        Vector2 rightTarget =
            rightStartPosition +
            new Vector2(
                -wave * horizontalBob,
                -wave * verticalBob
            );


        Quaternion leftRotationTarget =
            leftStartRotation *
            Quaternion.Euler(
                0f,
                0f,
                wave * rotationAmount
            );

        Quaternion rightRotationTarget =
            rightStartRotation *
            Quaternion.Euler(
                0f,
                0f,
                -wave * rotationAmount
            );


        float smoothFactor =
            1f - Mathf.Exp(-smoothSpeed * Time.deltaTime);


        leftHand.anchoredPosition =
            Vector2.Lerp(
                leftHand.anchoredPosition,
                leftTarget,
                smoothFactor
            );

        rightHand.anchoredPosition =
            Vector2.Lerp(
                rightHand.anchoredPosition,
                rightTarget,
                smoothFactor
            );


        leftHand.localRotation =
            Quaternion.Lerp(
                leftHand.localRotation,
                leftRotationTarget,
                smoothFactor
            );

        rightHand.localRotation =
            Quaternion.Lerp(
                rightHand.localRotation,
                rightRotationTarget,
                smoothFactor
            );
    }

    public void PlayGrab()
    {
    }


    public bool IsGrabbing()
    {
        return false;
    }
}