using System;
using UnityEngine;

[CreateAssetMenu(fileName = "CraftingRecipe", menuName = "Gedede/Crafting/Recipe")]
public class CraftingRecipe : ScriptableObject
{
    [Serializable]
    public class IngredientCell
    {
        public ItemDefinition item;
        [Min(1)] public int quantity = 1;
    }

    [SerializeField] private ItemDefinition output;
    [SerializeField, Min(1)] private int outputQuantity = 1;
    [SerializeField] private IngredientCell[] ingredients = new IngredientCell[9];

    public ItemDefinition Output => output;
    public int OutputQuantity => Mathf.Max(1, outputQuantity);
    public IngredientCell[] Ingredients => ingredients;
}