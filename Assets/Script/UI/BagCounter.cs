using TMPro;
using UnityEngine;

public class BagCounter : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private TMP_Text counterText;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color fullColor = Color.red;

    private void Reset()
    {
        counterText = GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        if (player == null)
        {
            player = FindAnyObjectByType<Player>();
        }

        if (counterText == null)
        {
            counterText = GetComponent<TMP_Text>();
        }

        if (player != null)
        {
            player.BagChanged += Refresh;
            Refresh(player.CarriedTrash, player.BagCapacity);
        }
    }

    private void OnDisable()
    {
        if (player != null)
        {
            player.BagChanged -= Refresh;
        }
    }

    private void Refresh(int carriedTrash, int bagCapacity)
    {
        if (counterText == null)
        {
            return;
        }

        counterText.text = $"Bag: {carriedTrash}/{bagCapacity}";
        counterText.color = carriedTrash >= bagCapacity ? fullColor : normalColor;
    }
}
