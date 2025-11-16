using System;

[Serializable]
public class SpecialIngredientPoolEntry
{
    public string ingredient_id;
    public int min_stock;
    public int max_stock;
    public float price_variance; // (CSV에는 30%로 되어있으므로 0.3으로 파싱)
    public float appearance_probability; // 출현 확률 (0.0 ~ 1.0)
}