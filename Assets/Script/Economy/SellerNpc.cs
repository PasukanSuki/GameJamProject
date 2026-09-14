using UnityEngine;

[RequireComponent(typeof(SellerManager))]
public class SellerNpc : MonoBehaviour
{
    [SerializeField] private SellerManager sellerManager;
    [SerializeField] private SellerPanel sellerPanel;

    public SellerManager Manager => sellerManager;

    private void Awake()
    {
        if (sellerManager == null)
        {
            sellerManager = GetComponent<SellerManager>();
        }
    }

    public bool Open(Player player)
    {
        if (player == null || sellerManager == null)
        {
            return false;
        }

        if (sellerPanel == null)
        {
            return false;
        }

        sellerPanel.Open(sellerManager, player);
        return true;
    }

}
