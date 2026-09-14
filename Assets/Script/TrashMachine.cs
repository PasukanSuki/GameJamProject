using UnityEngine;

public class TrashMachine : MonoBehaviour
{
    [SerializeField] private float depositRange = 3f;

    public float DepositRange => depositRange;

    public bool TryDeposit(Player player)
    {
        if (player == null)
        {
            Debug.LogWarning("TrashMachine deposit failed because Player is missing.");
            return false;
        }

        float distance = Vector3.Distance(transform.position, player.transform.position);
        if (distance > depositRange)
        {
            Debug.LogWarning($"TrashMachine is out of range. Distance: {distance:F2}, Range: {depositRange:F2}.");
            return false;
        }

        int depositedAmount = player.DepositTrash();
        Debug.Log($"TrashMachine deposit: {depositedAmount} trash, wallet after deposit: {player.Wallet}.");
        return depositedAmount > 0;
    }
}
