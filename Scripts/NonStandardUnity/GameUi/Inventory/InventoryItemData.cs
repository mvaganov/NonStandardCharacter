using UnityEngine;

/// <summary>
/// intended to be a base class for items with rich data, like weight, type, description and other similar variables.
/// </summary>
[RequireComponent(typeof(InventoryItemObject))]
public class InventoryItemData : MonoBehaviour {
    protected virtual void Awake() {
        InventoryItemObject invo = GetComponent<InventoryItemObject>();
        invo.item.data = this;
    }
}
