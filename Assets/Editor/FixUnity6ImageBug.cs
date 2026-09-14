#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[InitializeOnLoad]
public static class FixUnity6ImageBug
{
    static FixUnity6ImageBug()
    {
        EditorApplication.delayCall += CheckAndDeselectImage;
    }

    private static void CheckAndDeselectImage()
    {
        if (Selection.activeGameObject != null && Selection.activeGameObject.GetComponent<Image>() != null)
        {
            Selection.activeGameObject = null;
        }
    }

    [MenuItem("Gedede/Fix Inspector Error (Deselect)")]
    public static void DeselectAll()
    {
        Selection.objects = System.Array.Empty<Object>();
        Debug.Log("[FixUnity6ImageBug] Cleared selection to resolve Unity 6 Image component bug.");
    }
}
#endif
