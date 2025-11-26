using System;

[Serializable]
public class SpecialIngredientPoolEntry
{
    public string ingredient_id;
    public int min_stock;
    public int max_stock;
    public float price_variance;
    public float appearance_probability;
}