using NonStandard.Data;
using UnityEngine;

namespace NonStandard.GameUi.Inventory {
	[System.Serializable]
	public class InventoryItem {
		public string name;
		public Sprite itemImage;
		[System.Serializable]
		public class SpecialBehavior {
			public UnityEvent_object onAdd = new UnityEvent_object();
			public UnityEvent_object onRemove = new UnityEvent_object();
		}
		public SpecialBehavior inventoryAddBehavior;
		public Inventory currentInventory;
		public InventoryItemObject component;
		public object data;
		public InventoryItemObject GetItemObject() {
			return component;
        }
		public Transform GetTransform() {
			return (component != null) ? component.transform : null;
            //switch (component) {
			//	case GameObject go: return go.transform;
			//	case Component c: return c.transform;
			//}
			//return null;
        }
		public void RemoveFromCurrentInventory() {
			if (currentInventory == null) { return; }
			inventoryAddBehavior?.onRemove?.Invoke(this);
			currentInventory.RemoveItem(this);
			currentInventory = null;
		}
		public void AddToInventory(Inventory inventory) {
			if (this.currentInventory == inventory) {
				Show.Warning(name+" being added to "+inventory.name+" again");
				return; // prevent double-add
			}
			RemoveFromCurrentInventory();
			this.currentInventory = inventory;
			inventory.AddItem(this);
			inventoryAddBehavior?.onAdd?.Invoke(this);
		}
		public void OnTrigger(GameObject other) {
			InventoryCollector inv = other.GetComponent<InventoryCollector>();
			Debug.Log("item hits "+other);
			if (inv != null && inv.autoPickup && inv.inventory != null) {
				inv.AddItem(this);
			}
		}
	}
}