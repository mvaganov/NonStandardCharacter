using NonStandard.Utility.UnityEditor;
using UnityEngine;

namespace NonStandard.GameUi.Inventory {
	public class InventoryCollector : MonoBehaviour {
		public Inventory inventory;
		public bool autoPickup = true;
		public bool addAsChildTransform = true;
		public void AddItem(object itemObject) {
			InventoryItem itm = inventory.FindInventoryItemToAdd(itemObject, true);
			if (itm == null) { return; }
			itm.AddToInventory(inventory);
			if (addAsChildTransform) {
				Transform t = itm.GetTransform();
				t.SetParent(transform, true);
				//itm.inventoryAddBehavior.onRemove.AddListener(UndoTransformAdjust);
				//EventBind.On
			}
		}
		public void RemoveItem(object itemObject) {
			InventoryItem itm = inventory.FindInventoryItemToAdd(itemObject, false);
			if (itm == null) { return; }
			itm.RemoveFromCurrentInventory();
		}
		public void UndoTransformAdjust(object itemObject) {
			InventoryItem itm = inventory.FindInventoryItemToAdd(itemObject, false);
			if (itm == null) { return; }
			Transform t = itm.GetTransform();
			t.SetParent(null, true);
			//itm.inventoryAddBehavior.onRemove -= UndoTransformAdjust;
		}
	}
}