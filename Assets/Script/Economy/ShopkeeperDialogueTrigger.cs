using UnityEngine;

public class ShopkeeperDialogueTrigger : MonoBehaviour
{
    [Header("Speech Bubble Reference")]
    [SerializeField] private NpcSpeechBubble speechBubble;

    [Header("Player Detection")]
    [Tooltip("Jarak deteksi pemain untuk memicu sapaan saat mendekat.")]
    [SerializeField] private float proximityDistance = 4.5f;

    [Tooltip("Waktu tunggu (cooldown) dalam detik agar sapaan tidak berulang terus.")]
    [SerializeField] private float greetCooldown = 18f;

    [Header("Dialog: Saat Player Mendekat")]
    [SerializeField] private string[] greetingDialogues = new string[]
    {
        "Halo! Mau jual barang apa hari ini?",
        "Punya barang rongsokan atau hasil daur ulang? Sini kubeli!",
        "Ada barang bagus yang mau dijual?",
        "Selamat datang! Silakan, ada yang bisa kubeli?"
    };

    [Header("Dialog: Saat Selesai Menjual")]
    [SerializeField] private string[] soldDialogues = new string[]
    {
        "Terima kasih! Uang sudah kuberikan ya.",
        "Bagus! Bawa lebih banyak lagi nanti ya.",
        "Senang berbisnis denganmu! Datang lagi nanti ya.",
        "Mantap, transaksinya beres!"
    };

    [Header("Dialog: Saat Iseng Buka Lalu Langsung Keluar")]
    [SerializeField] private string[] cancelledDialogues = new string[]
    {
        "Loh kok gajadi?",
        "Loh kok gajadi? Padahal udah kubuka nih.",
        "Nggak jadi jual nih? Yah...",
        "Loh, cuma liat-liat aja nih?"
    };

    [Header("Arah Hadap NPC")]
    [Tooltip("Apakah badan NPC juga ikut berputar menghadap ke arah player saat pemain berada di dekatnya?")]
    [SerializeField] private bool rotateNpcTowardsPlayer = false;
    [SerializeField] private float rotationSpeed = 4f;

    private Transform playerTransform;
    private bool wasPlayerInRange;
    private float lastGreetTime = -100f;
    private bool isPlayerShopping;

    private void Awake()
    {
        EnsureSpeechBubble();
    }

    private void OnEnable()
    {
        SellerPanel.ShopOpened += HandleShopOpened;
        SellerPanel.ShopClosed += HandleShopClosed;
        SellerPanel.ItemSoldSuccessfully += HandleItemSold;
    }

    private void OnDisable()
    {
        SellerPanel.ShopOpened -= HandleShopOpened;
        SellerPanel.ShopClosed -= HandleShopClosed;
        SellerPanel.ItemSoldSuccessfully -= HandleItemSold;
    }

    private void Start()
    {
        ResolvePlayer();
    }

    private void Update()
    {
        if (playerTransform == null)
        {
            ResolvePlayer();
            if (playerTransform == null) return;
        }

        float distance = Vector3.Distance(transform.position, playerTransform.position);
        bool isPlayerInRange = distance <= proximityDistance;

        // Trigger 1: Saat pemain mendekat
        if (isPlayerInRange && !wasPlayerInRange)
        {
            if (Time.time >= lastGreetTime + greetCooldown && !isPlayerShopping)
            {
                TriggerRandomDialogue(greetingDialogues, 3.5f);
                lastGreetTime = Time.time;
            }
        }

        wasPlayerInRange = isPlayerInRange;

        // Berputar menghadap pemain secara halus jika diaktifkan
        if (rotateNpcTowardsPlayer && isPlayerInRange && playerTransform != null)
        {
            Vector3 lookDirection = playerTransform.position - transform.position;
            lookDirection.y = 0f;
            if (lookDirection.sqrMagnitude > 0.01f)
            {
                Quaternion targetRot = Quaternion.LookRotation(lookDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotationSpeed);
            }
        }
    }

    private void ResolvePlayer()
    {
        Player player = FindAnyObjectByType<Player>();
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }

    private void HandleShopOpened(SellerManager sellerManager, Player player)
    {
        if (!IsPlayerNearThisNpc()) return;

        isPlayerShopping = true;
    }

    private void HandleShopClosed(bool didSellSomething)
    {
        if (!IsPlayerNearThisNpc()) return;

        isPlayerShopping = false;

        // Trigger 3: Jika pemain membuka toko lalu keluar tanpa menjual apapun ("Loh kok gajadi?")
        if (!didSellSomething)
        {
            TriggerRandomDialogue(cancelledDialogues, 3.8f);
        }
    }

    private void HandleItemSold(ItemDefinition item, int quantity, int totalValue)
    {
        if (!IsPlayerNearThisNpc()) return;

        // Trigger 2: Saat selesai menjual sesuatu
        TriggerRandomDialogue(soldDialogues, 3.5f);
    }

    private bool IsPlayerNearThisNpc()
    {
        if (playerTransform == null) ResolvePlayer();
        if (playerTransform == null) return true;

        // Cek apakah pemain berada dalam radius interaksi NPC ini (toleransi 8 meter)
        return Vector3.Distance(transform.position, playerTransform.position) <= proximityDistance * 2f;
    }

    public void TriggerRandomDialogue(string[] dialogueList, float duration = 3.5f)
    {
        if (dialogueList == null || dialogueList.Length == 0) return;

        EnsureSpeechBubble();
        string chosenMessage = dialogueList[Random.Range(0, dialogueList.Length)];
        if (speechBubble != null)
        {
            speechBubble.ShowMessage(chosenMessage, duration, "Penjual");
        }
    }

    public void Say(string message, float duration = 3.5f)
    {
        EnsureSpeechBubble();
        if (speechBubble != null)
        {
            speechBubble.ShowMessage(message, duration, "Penjual");
        }
    }

    private void EnsureSpeechBubble()
    {
        if (speechBubble == null)
        {
            speechBubble = GetComponent<NpcSpeechBubble>();
        }

        if (speechBubble == null)
        {
            speechBubble = GetComponentInChildren<NpcSpeechBubble>();
        }

        if (speechBubble == null)
        {
            speechBubble = gameObject.AddComponent<NpcSpeechBubble>();
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, proximityDistance);
    }
#endif
}
