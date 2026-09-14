#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(UpgradeDefinition))]
public class UpgradeDefinitionEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawProperty("upgradeId");
        DrawProperty("displayName");
        DrawProperty("description");
        DrawProperty("upgradeType");
        DrawProperty("maxLevel");

        SerializedProperty upgradeType = serializedObject.FindProperty("upgradeType");
        bool isBagCapacity = upgradeType != null
            && (PlayerUpgradeType)upgradeType.enumValueIndex == PlayerUpgradeType.BagCapacity;

        if (!isBagCapacity)
        {
            DrawProperty("icon");
            DrawProperty("price");
            DrawProperty("amountPerLevel");
            DrawProperty("priceGrowth");
        }

        DrawProperty("pricesByLevel");
        DrawProperty("amountsByLevel");
        DrawProperty("iconsByLevel");
        DrawProperty("sortOrder");

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawProperty(string propertyName)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property != null)
        {
            EditorGUILayout.PropertyField(property, true);
        }
    }
}
#endif
