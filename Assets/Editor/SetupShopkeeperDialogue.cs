using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SetupShopkeeperDialogue
{
    [MenuItem("Tools/Gedede/Setup Shopkeeper Dialogue on Selected NPC")]
    public static void SetupSelected()
    {
        GameObject selected = Selection.activeGameObject;
        if (selected == null)
        {
            EditorUtility.DisplayDialog("Setup Shopkeeper Dialogue", "Silakan pilih GameObject NPC terlebih dahulu di Hierarchy (misal: NpcNewPivot atau Penjual).", "OK");
            return;
        }

        ConfigureNpc(selected);
        EditorUtility.DisplayDialog("Setup Berhasil", $"Komponen Shopkeeper Dialogue berhasil dipasang pada '{selected.name}'!", "OK");
    }

    [MenuItem("Tools/Gedede/Setup Shopkeeper Dialogue on Scene NPC")]
    public static void SetupInScene()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        GameObject targetNpc = null;

        // Cari berdasarkan nama yang relevan
        foreach (GameObject go in Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (go == null) continue;
            string n = go.name;
            if (n == "NpcNewPivot" || n.StartsWith("PenjualWithPose") || n == "SellerLocationMarker")
            {
                targetNpc = go;
                break;
            }
        }

        if (targetNpc == null)
        {
            // Fallback ke SellerNpc jika sudah ada
            SellerNpc seller = Object.FindAnyObjectByType<SellerNpc>(FindObjectsInactive.Include);
            if (seller != null)
            {
                targetNpc = seller.gameObject;
            }
        }

        if (targetNpc == null)
        {
            EditorUtility.DisplayDialog("Setup Shopkeeper Dialogue", "Tidak ditemukan GameObject NPC (NpcNewPivot / Penjual) di scene.", "OK");
            return;
        }

        ConfigureNpc(targetNpc);
        EditorSceneManager.MarkSceneDirty(currentScene);
        Selection.activeGameObject = targetNpc;
        EditorUtility.DisplayDialog("Setup Selesai", $"Komponen Dialog Toko & Balon Teks berhasil dipasang pada '{targetNpc.name}' di scene!", "OK");
    }

    private static void ConfigureNpc(GameObject npc)
    {
        Undo.RegisterFullObjectHierarchyUndo(npc, "Setup Shopkeeper Dialogue");

        // 1. Collider untuk interaksi raycast Player
        Collider col = npc.GetComponent<Collider>();
        if (col == null)
        {
            CapsuleCollider capsule = npc.AddComponent<CapsuleCollider>();
            capsule.height = 2f;
            capsule.radius = 0.5f;
            capsule.center = new Vector3(0f, 1f, 0f);
        }

        // 2. SellerManager
        SellerManager manager = npc.GetComponent<SellerManager>();
        if (manager == null)
        {
            manager = npc.AddComponent<SellerManager>();
        }

        // 3. SellerNpc
        SellerNpc sellerNpc = npc.GetComponent<SellerNpc>();
        if (sellerNpc == null)
        {
            sellerNpc = npc.AddComponent<SellerNpc>();
        }

        // Hubungkan SellerPanel jika ada di Canvas
        SellerPanel sellerPanel = Object.FindAnyObjectByType<SellerPanel>(FindObjectsInactive.Include);
        if (sellerPanel != null)
        {
            SerializedObject serializedSeller = new SerializedObject(sellerNpc);
            SerializedProperty panelProp = serializedSeller.FindProperty("sellerPanel");
            if (panelProp != null && panelProp.objectReferenceValue == null)
            {
                panelProp.objectReferenceValue = sellerPanel;
                serializedSeller.ApplyModifiedProperties();
            }
        }

        // 4. Speech Bubble (posisi sedikit lebih rendah dan selalu menghadap player)
        NpcSpeechBubble speechBubble = npc.GetComponent<NpcSpeechBubble>();
        if (speechBubble == null)
        {
            speechBubble = npc.AddComponent<NpcSpeechBubble>();
        }

        SerializedObject serializedBubble = new SerializedObject(speechBubble);
        SerializedProperty heightProp = serializedBubble.FindProperty("headHeight");
        if (heightProp != null)
        {
            heightProp.floatValue = 1.85f;
            serializedBubble.ApplyModifiedProperties();
        }

        // 5. Dialogue Trigger
        ShopkeeperDialogueTrigger trigger = npc.GetComponent<ShopkeeperDialogueTrigger>();
        if (trigger == null)
        {
            trigger = npc.AddComponent<ShopkeeperDialogueTrigger>();
        }

        EditorUtility.SetDirty(npc);
    }
}
