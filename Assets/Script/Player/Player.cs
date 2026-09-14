using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class Player : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CharacterController controller;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private InventoryManager inventory;
    [SerializeField] private CurrencyWallet wallet;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float gravity = -20f;

    [Header("Trash Pickup")]
    [SerializeField] private float pickupRange = 3f;
    [SerializeField, Min(0f)] private float pickupInterval = 0.5f;
    [SerializeField] private LayerMask pickupLayers = ~0;

    [Header("Hands")]
    [SerializeField] private FirstPersonHands handsAnimator;

    [Header("Bag")]
    [SerializeField, Min(1)] private int bagCapacity = 20;
    [SerializeField] private float depositRange = 3f;

    [Header("Currency")]
    [SerializeField] private int startingWallet = 0;
    [SerializeField, Min(1)] private int fullBagBaseReward = 100;
    [SerializeField, Range(0f, 1f)] private float fullBagBonusMultiplier = 0.10f;
    [SerializeField, Min(0.01f)] private float incomeMultiplier = 1f;

    [Header("Look")]
    [SerializeField] private float mouseSensitivity = 1.5f;
    [SerializeField] private float minPitch = -80f;
    [SerializeField] private float maxPitch = 80f;
    [SerializeField] private float lookSmoothing = 8f;
    [SerializeField] private float cameraSmoothing = 8f;

    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction jumpAction;
    private InputAction interactAction;
    private InputAction depositAction;

    private Vector3 velocity;
    private float pitchRotation;
    private float yawRotation;
    private float pickupTimer;
    private int carriedTrash;
    private bool isPaused;
    private int baseBagCapacity;
    private float basePickupInterval;
    private float baseFullBagBonusMultiplier;
    private float baseIncomeMultiplier;

    public bool IsPaused => isPaused;
    public int CarriedTrash => carriedTrash;
    public int BagCapacity => bagCapacity;
    public float PickupInterval => pickupInterval;
    public float FullBagBonusMultiplier => fullBagBonusMultiplier;
    public float IncomeMultiplier => incomeMultiplier;
    public int Wallet => wallet != null ? wallet.Balance : 0;
    public bool IsBagFull => carriedTrash >= bagCapacity;
    public event Action<int, int> BagChanged;
    public event Action<int> WalletChanged;
    public event Action<int, int> DepositCompleted;

    public void SetPaused(bool paused)
    {
        isPaused = paused;

        if (paused)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            return;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Reset()
    {
        controller = GetComponent<CharacterController>();

        if (playerCamera == null)
        {
            playerCamera = GetComponentInChildren<Camera>();
        }

        if (cameraPivot == null && playerCamera != null)
        {
            cameraPivot = playerCamera.transform;
        }
    }

    private void Awake()
    {
        bagCapacity = Mathf.Max(1, bagCapacity);
        fullBagBaseReward = Mathf.Max(1, fullBagBaseReward);
        fullBagBonusMultiplier = Mathf.Max(0f, fullBagBonusMultiplier);
        incomeMultiplier = Mathf.Max(0.01f, incomeMultiplier);
        baseBagCapacity = bagCapacity;
        basePickupInterval = pickupInterval;
        baseFullBagBonusMultiplier = fullBagBonusMultiplier;
        baseIncomeMultiplier = incomeMultiplier;

        if (wallet == null)
        {
            wallet = GetComponent<CurrencyWallet>();
        }

        if (wallet == null)
        {
            wallet = gameObject.AddComponent<CurrencyWallet>();
        }

        if (GetComponent<EconomyBootstrap>() == null)
        {
            gameObject.AddComponent<EconomyBootstrap>();
        }

        wallet.BalanceChanged += HandleWalletChanged;
        wallet.SetBalance(Mathf.Max(0, startingWallet));

        if (inventory == null)
        {
            inventory = FindAnyObjectByType<InventoryManager>();
        }

        if (inventory == null)
        {
            inventory = gameObject.AddComponent<InventoryManager>();
        }

        if (controller == null)
        {
            controller = GetComponent<CharacterController>();
        }

        if (controller == null)
        {
            controller = gameObject.AddComponent<CharacterController>();
        }

        if (playerCamera == null)
        {
            playerCamera = GetComponentInChildren<Camera>();
        }

        if (cameraPivot == null && playerCamera != null)
        {
            cameraPivot = playerCamera.transform;
        }

        if (playerCamera == null)
        {
            GameObject cameraObject = new GameObject("Player Camera");
            cameraObject.transform.SetParent(transform);
            cameraObject.transform.localPosition = new Vector3(0f, 0.40f, 0f);
            cameraObject.transform.localRotation = Quaternion.identity;
            playerCamera = cameraObject.AddComponent<Camera>();
            cameraPivot = cameraObject.transform;
        }

        if (handsAnimator == null)
        {
            handsAnimator = GetComponentInChildren<FirstPersonHands>(true);
        }

        SetPaused(false);
    }

    private void OnEnable()
    {
        CacheActions();
        EnableActions();
    }

    private void OnDisable()
    {
        DisableActions();
    }

    private void OnDestroy()
    {
        if (wallet != null)
        {
            wallet.BalanceChanged -= HandleWalletChanged;
        }
    }

    private void Update()
    {
        if (isPaused)
        {
            return;
        }

        HandleLook();
        HandleMovement();
        HandlePickup();
        HandleDeposit();
    }

    private void CacheActions()
    {
        if (moveAction == null || lookAction == null || jumpAction == null || interactAction == null || depositAction == null)
        {
            var playerMap = InputSystem.actions.FindActionMap("Player", true);
            moveAction = playerMap != null ? playerMap.FindAction("Move", true) : null;
            lookAction = playerMap != null ? playerMap.FindAction("Look", true) : null;
            jumpAction = playerMap != null ? playerMap.FindAction("Jump", true) : null;
            interactAction = playerMap != null ? playerMap.FindAction("Interact", true) : null;
            depositAction = playerMap != null ? playerMap.FindAction("Deposit", true) : null;
        }
    }

    private void EnableActions()
    {
        moveAction?.Enable();
        lookAction?.Enable();
        jumpAction?.Enable();
        interactAction?.Enable();
        depositAction?.Enable();
    }

    private void DisableActions()
    {
        moveAction?.Disable();
        lookAction?.Disable();
        jumpAction?.Disable();
        interactAction?.Disable();
        depositAction?.Disable();
    }

    private void HandleLook()
    {
        Vector2 lookInput = lookAction != null
            ? lookAction.ReadValue<Vector2>()
            : Mouse.current != null ? Mouse.current.delta.ReadValue() : Vector2.zero;

        float lookScale = 0.12f;
        float lookX = lookInput.x * mouseSensitivity * lookScale;
        float lookY = lookInput.y * mouseSensitivity * lookScale;

        yawRotation += lookX;
        pitchRotation -= lookY;
        pitchRotation = Mathf.Clamp(pitchRotation, minPitch, maxPitch);

        Quaternion targetBodyRotation = Quaternion.Euler(0f, yawRotation, 0f);
        Quaternion targetCameraRotation = Quaternion.Euler(pitchRotation, 0f, 0f);

        if (cameraPivot != null)
        {
            cameraPivot.localRotation = Quaternion.Lerp(
                cameraPivot.localRotation,
                targetCameraRotation,
                Time.deltaTime * cameraSmoothing);

            transform.localRotation = Quaternion.Lerp(
                transform.localRotation,
                targetBodyRotation,
                Time.deltaTime * lookSmoothing);
        }
        else
        {
            transform.localRotation = Quaternion.Lerp(
                transform.localRotation,
                targetBodyRotation,
                Time.deltaTime * lookSmoothing);
        }
    }

    private void HandleMovement()
    {
        Vector2 moveInput = moveAction != null
            ? moveAction.ReadValue<Vector2>()
            : Keyboard.current != null
                ? new Vector2(
                    (Keyboard.current.dKey.isPressed ? 1f : 0f) - (Keyboard.current.aKey.isPressed ? 1f : 0f),
                    (Keyboard.current.wKey.isPressed ? 1f : 0f) - (Keyboard.current.sKey.isPressed ? 1f : 0f))
                : Vector2.zero;

        if (controller.isGrounded && velocity.y < 0f)
        {
            velocity.y = -2f;
        }

        bool wantsJump = jumpAction != null
            ? jumpAction.WasPressedThisFrame()
            : Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;

        if (wantsJump && controller.isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        Vector3 movement = transform.forward * moveInput.y + transform.right * moveInput.x;
        movement.y = 0f;
        if (movement.sqrMagnitude > 1f)
        {
            movement.Normalize();
        }

        controller.Move(movement * moveSpeed * Time.deltaTime);
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void HandlePickup()
    {
        pickupTimer = Mathf.Max(0f, pickupTimer - Time.deltaTime);
        if (inventory == null)
        {
            return;
        }

        bool interactPressed = (interactAction != null && interactAction.WasPressedThisFrame())
            || (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame);
        bool pickupPressed = interactPressed
            || (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame);
        if (!pickupPressed)
        {
            return;
        }

        Camera pickupCamera = playerCamera != null ? playerCamera : Camera.main;
        if (pickupCamera == null)
        {
            return;
        }

        Ray pickupRay = new Ray(pickupCamera.transform.position, pickupCamera.transform.forward);
        bool hasHit = Physics.Raycast(pickupRay, out RaycastHit hit, pickupRange, pickupLayers, QueryTriggerInteraction.Ignore);

        if (pickupTimer > 0f)
        {
            return;
        }

        SellerNpc sellerNpc = hasHit ? hit.collider.GetComponentInParent<SellerNpc>() : null;
        if (interactPressed && sellerNpc != null && sellerNpc.Open(this))
        {
            pickupTimer = pickupInterval;
            return;
        }

        if (hasHit && TryCollectPickup(hit.collider, pickupCamera.transform))
        {
            return;
        }

        Collider[] nearbyColliders = Physics.OverlapSphere(
            transform.position,
            pickupRange,
            pickupLayers,
            QueryTriggerInteraction.Ignore);

        System.Array.Sort(nearbyColliders, (left, right) =>
            Vector3.SqrMagnitude(left.transform.position - transform.position)
                .CompareTo(Vector3.SqrMagnitude(right.transform.position - transform.position)));

        foreach (Collider nearbyCollider in nearbyColliders)
        {
            if (TryCollectPickup(nearbyCollider, pickupCamera.transform))
            {
                return;
            }
        }
    }

    private bool TryCollectPickup(Collider pickupCollider, Transform pickupTarget)
    {
        if (pickupCollider == null || pickupTarget == null)
        {
            return false;
        }

        TrashPile trashPile = pickupCollider.GetComponentInParent<TrashPile>();
        if (trashPile != null && trashPile.TryCollect(pickupTarget, AcceptPickup))
        {
            CompletePickup();
            return true;
        }

        WorldTrashPickup worldPickup = pickupCollider.GetComponentInParent<WorldTrashPickup>();
        if (worldPickup != null && worldPickup.TryCollect(() => AcceptWorldPickup(worldPickup)))
        {
            CompletePickup();
            return true;
        }

        return false;
    }

    private void CompletePickup()
    {
        pickupTimer = pickupInterval;

        if (handsAnimator != null)
        {
            handsAnimator.PlayGrab();
        }
    }

    private bool AcceptPickup(TrashPile.PickupResult result)
    {
        if (result == null)
        {
            return false;
        }

        return result.ordinaryTrash
            ? TryAddTrash(1)
            : inventory.AddItem(result.item);
    }

    private bool AcceptWorldPickup(WorldTrashPickup pickup)
    {
        if (pickup == null)
        {
            return false;
        }

        return pickup.IsOrdinaryTrash
            ? TryAddTrash(pickup.Quantity)
            : inventory.AddItem(pickup.Item, pickup.Quantity);
    }

    public bool TryAddTrash(int amount)
    {
        if (amount <= 0 || carriedTrash + amount > bagCapacity)
        {
            return false;
        }

        carriedTrash += amount;
        BagChanged?.Invoke(carriedTrash, bagCapacity);
        return true;
    }

    public bool CanAfford(int amount)
    {
        return wallet != null && wallet.CanAfford(amount);
    }

    public bool TrySpendCurrency(int amount)
    {
        return wallet != null && wallet.TrySpend(amount, CurrencyTransactionSource.Unknown);
    }

    public void AddCurrency(int amount)
    {
        wallet?.TryAdd(amount, CurrencyTransactionSource.Unknown);
    }

    public int DepositTrash()
    {
        int depositedAmount = carriedTrash;
        if (depositedAmount <= 0)
        {
            return 0;
        }

        if (wallet == null)
        {
            wallet = GetComponent<CurrencyWallet>();
        }

        if (wallet == null)
        {
            Debug.LogError("Player cannot deposit trash because no CurrencyWallet is available.");
            return 0;
        }

        int reward = CalculateDepositReward(depositedAmount);
        if (reward <= 0 || !wallet.TryAdd(reward, CurrencyTransactionSource.Deposit))
        {
            Debug.LogWarning($"Trash deposit failed. Amount: {depositedAmount}, Reward: {reward}.");
            return 0;
        }

        Debug.Log($"Trash deposit rewarded {reward}. Wallet balance: {wallet.Balance}.");

        carriedTrash = 0;
        BagChanged?.Invoke(carriedTrash, bagCapacity);

        DepositCompleted?.Invoke(depositedAmount, reward);

        return depositedAmount;
    }

    public int CalculateDepositReward(int depositedAmount)
    {
        return DepositRewardCalculator.Calculate(
            depositedAmount,
            bagCapacity,
            fullBagBaseReward,
            fullBagBonusMultiplier,
            incomeMultiplier);
    }

    public void UpgradeBagCapacity(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        bagCapacity += amount;
        carriedTrash = Mathf.Min(carriedTrash, bagCapacity);
        BagChanged?.Invoke(carriedTrash, bagCapacity);
    }

    public void UpgradePickupSpeed(float amount)
    {
        if (amount <= 0f)
        {
            return;
        }

        pickupInterval = Mathf.Max(0.05f, pickupInterval - amount);
    }

    public void SetBagCapacityBonus(int amount)
    {
        bagCapacity = baseBagCapacity + Mathf.Max(0, amount);
        carriedTrash = Mathf.Min(carriedTrash, bagCapacity);
        BagChanged?.Invoke(carriedTrash, bagCapacity);
    }

    public void SetPickupSpeedBonus(float amount)
    {
        pickupInterval = Mathf.Max(0.05f, basePickupInterval - Mathf.Max(0f, amount));
    }

    public void SetFullBagBonusLevel(float amount)
    {
        fullBagBonusMultiplier = Mathf.Max(0f, baseFullBagBonusMultiplier + amount);
    }

    public void SetIncomeLevel(float amount)
    {
        incomeMultiplier = Mathf.Max(0.01f, baseIncomeMultiplier + amount);
    }

    public void ResetUpgradeStats()
    {
        bagCapacity = baseBagCapacity;
        pickupInterval = basePickupInterval;
        fullBagBonusMultiplier = baseFullBagBonusMultiplier;
        incomeMultiplier = baseIncomeMultiplier;
        carriedTrash = Mathf.Min(carriedTrash, bagCapacity);
        BagChanged?.Invoke(carriedTrash, bagCapacity);
    }

    private void HandleDeposit()
    {
        bool pressed = depositAction != null
            ? depositAction.WasPressedThisFrame()
            : Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;

        if (!pressed)
        {
            return;
        }

        Camera depositCamera = playerCamera != null ? playerCamera : Camera.main;
        if (depositCamera == null)
        {
            return;
        }

        Ray depositRay = new Ray(depositCamera.transform.position, depositCamera.transform.forward);
        if (!Physics.Raycast(depositRay, out RaycastHit hit, depositRange, pickupLayers, QueryTriggerInteraction.Ignore))
        {
            return;
        }

        TrashMachine machine = hit.collider.GetComponentInParent<TrashMachine>();
        machine?.TryDeposit(this);
    }

    private void HandleWalletChanged(int balance)
    {
        WalletChanged?.Invoke(balance);
    }
}
