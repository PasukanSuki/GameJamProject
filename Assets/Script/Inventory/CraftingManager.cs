using System;
using System.Collections.Generic;
using UnityEngine;

public class CraftingManager : MonoBehaviour
{
    [SerializeField] private InventoryManager inventory;
    [SerializeField] private List<CraftingRecipe> recipes = new List<CraftingRecipe>();

    public IReadOnlyList<CraftingRecipe> Recipes => recipes;
    public InventoryManager Inventory => inventory;
    public event Action CraftingChanged;

    private void Awake()
    {
        if (inventory == null)
        {
            inventory = FindAnyObjectByType<InventoryManager>();
        }
    }

    private void OnEnable()
    {
        if (inventory != null)
        {
            inventory.InventoryChanged += HandleInventoryChanged;
        }
    }

    private void OnDisable()
    {
        if (inventory != null)
        {
            inventory.InventoryChanged -= HandleInventoryChanged;
        }
    }

    public bool CanCraft(CraftingRecipe recipe)
    {
        if (recipe == null || recipe.Output == null || inventory == null)
        {
            return false;
        }

        Dictionary<ItemDefinition, int> requirements = GetRequirements(recipe);
        foreach (KeyValuePair<ItemDefinition, int> requirement in requirements)
        {
            if (inventory.GetItemCount(requirement.Key) < requirement.Value)
            {
                return false;
            }
        }

        return inventory.CanAddItem(recipe.Output, recipe.OutputQuantity);
    }

    public bool Craft(CraftingRecipe recipe)
    {
        if (!CanCraft(recipe))
        {
            return false;
        }

        Dictionary<ItemDefinition, int> requirements = GetRequirements(recipe);
        foreach (KeyValuePair<ItemDefinition, int> requirement in requirements)
        {
            inventory.RemoveItem(requirement.Key, requirement.Value);
        }

        if (!inventory.AddItem(recipe.Output, recipe.OutputQuantity))
        {
            return false;
        }

        CraftingChanged?.Invoke();
        return true;
    }

    private Dictionary<ItemDefinition, int> GetRequirements(CraftingRecipe recipe)
    {
        Dictionary<ItemDefinition, int> requirements = new Dictionary<ItemDefinition, int>();
        if (recipe.Ingredients == null)
        {
            return requirements;
        }

        foreach (CraftingRecipe.IngredientCell ingredient in recipe.Ingredients)
        {
            if (ingredient == null || ingredient.item == null || ingredient.quantity <= 0)
            {
                continue;
            }

            if (!requirements.ContainsKey(ingredient.item))
            {
                requirements.Add(ingredient.item, 0);
            }

            requirements[ingredient.item] += ingredient.quantity;
        }

        return requirements;
    }

    private void HandleInventoryChanged()
    {
        CraftingChanged?.Invoke();
    }
}