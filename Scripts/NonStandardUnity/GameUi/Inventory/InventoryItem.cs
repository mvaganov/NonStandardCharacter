using UnityEngine;
using UnityEngine.Events;

namespace NonStandard.GameUi.Inventory {
	[System.Serializable]
	public class InventoryItem {
		[SerializeField] public string _name;
		public Sprite image;
		[System.Serializable]
		public class SpecialBehavior {
			[System.Serializable] public class UnityEvent_object : UnityEvent<object> { }
			public UnityEvent_object onAdd = new UnityEvent_object();
			public UnityEvent_object onRemove = new UnityEvent_object();
		}
		public SpecialBehavior inventoryAddBehavior;
		[HideInInspector] public Inventory currentInventory;
		[HideInInspector] public InventoryItemObject component;
		public object data;
		public bool nodrop = false;
		public string name {
			get => _name;
			set {
				_name = value;
				if (component != null) { component.name = _name; }
            }
        }
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
		public void Drop() {
			if (currentInventory == null) { return; }
			inventoryAddBehavior?.onRemove?.Invoke(this);
			currentInventory.RemoveItem(this);
			currentInventory = null;
		}
		public void AddToInventory(Inventory inventory) {
			if (this.currentInventory == inventory) {
				Debug.LogWarning(name+" being added to "+inventory.name+" again");
				return; // prevent double-add
			}
			Drop();
			this.currentInventory = inventory;
			inventory.AddItem(this);
			inventoryAddBehavior?.onAdd?.Invoke(this);
		}
		public void OnTrigger(GameObject other) {
			InventoryCollector inv = other.GetComponent<InventoryCollector>();
			//Debug.Log("item hits "+other);
			if (inv != null && inv.autoPickup && inv.inventory != null) {
				inv.AddItem(this);
			}
		}
	}
}