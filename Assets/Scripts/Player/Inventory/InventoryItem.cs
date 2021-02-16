using System;

public abstract class InventoryItem {
    public string name;
    public int healthEffect;
    public int foodEffect;

    public InventoryItem(string name) {
        this.name = name;
    }
}