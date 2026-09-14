using UnityEngine;
using UnityEngine.UI;

public class HandOverlay : MonoBehaviour
{
    [SerializeField] private RawImage handImage;
    [SerializeField] private Texture2D handTexture;

    private void Reset()
    {
        handImage = GetComponent<RawImage>();
    }

    private void Awake()
    {
        if (handImage == null)
        {
            handImage = GetComponent<RawImage>();
        }

        if (handImage != null && handTexture != null)
        {
            handImage.texture = handTexture;
            handImage.raycastTarget = false;
        }
    }
}
