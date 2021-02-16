using System;

public class FoodItem : InventoryItem {
    public FoodItem(string name, int foodEffect) : base(name) {
        this.foodEffect = foodEffect;
    }
}