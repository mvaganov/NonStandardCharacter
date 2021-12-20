using NonStandard.Data;
using NonStandard.Utility.UnityEditor;
using System.Collections.Generic;
using UnityEngine;

namespace NonStandard.GameUi.Inventory {
	public class Inventory : MonoBehaviour {
		public bool allowAdd = true;
		[ContextMenuItem("Drop All Items", nameof(DropAllItems))]
		public bool allowRemove = true;
		[SerializeField] private List<InventoryItem> items;
		/*
		public ListUi inventoryUi;
		*/
		public InventoryItem.SpecialBehavior itemAddBehavior;
		//public UnityEvent_object onAddItem;
		//public UnityEvent_object onRemoveItem;
		public List<InventoryItem> GetItems() { return items; }
		private void Awake() {
			Global.GetComponent<InventoryManager>().Register(this);
		}
		public void ActivateGameObject(object itemObject) {
			Debug.Log("activate " + itemObject);
			switch (itemObject) {
				case InventoryItem i: ActivateGameObject(i.data); return;
				case GameObject go: go.SetActive(true); return;
				case Component c: c.gameObject.SetActive(true); return;
			}
		}
		public void DeactivateGameObject(object itemObject) {
			Debug.Log("deactivate " + itemObject);
            switch (itemObject) {
				case InventoryItem i: DeactivateGameObject(i.data); return;
				case GameObject go: go.SetActive(false); return;
				case Component c: c.gameObject.SetActive(false); return;
			}
		}
#if UNITY_EDITOR
		private void Reset() {
			itemAddBehavior = new InventoryItem.SpecialBehavior();
			EventBind.IfNotAlready(itemAddBehavior.onAdd, this, nameof(DeactivateGameObject));
			EventBind.IfNotAlready(itemAddBehavior.onRemove, this, nameof(ActivateGameObject));
		}
#endif
		public InventoryItem FindInventoryItemToAdd(object item, bool createIfMissing) {
			InventoryItem invi = item as InventoryItem;
			if (invi != null) return invi;
			if (item is GameObject go) {
				InventoryItemObject invio = go.GetComponent<InventoryItemObject>();
				if (invio != null) {
					return invio.item;
				}
				if (invio == null) {
					for(int i = 0; i < items.Count; ++i) {
						if (items[i].data == item) {
							return items[i];
                        }
                    }
                }
				if (invio == null && createIfMissing) {
					invio = go.AddComponent<InventoryItemObject>();
					invio.item.data = item;
					return invio.item;
				}
			}
			if (invi == null) {
				Debug.LogWarning("cannot convert ("+item.GetType()+") "+item+" into InventoryItem");
            }
			return invi;
		}
		internal InventoryItem AddItem(object itemObject) {
			if (items == null) { items = new List<InventoryItem>(); }
			InventoryItem inv = FindInventoryItemToAdd(itemObject, true);
			if (items.Contains(inv)) {
				Debug.LogWarning(this + " already has item " + inv);
				return null;
			}
			if (!allowAdd) {
				Debug.LogWarning(this + " will not add " + inv);
				return null;
            }
			items.Add(inv);
			itemAddBehavior?.onAdd?.Invoke(itemObject);
			return inv;
		}
		internal InventoryItem RemoveItem(object itemObject) {
			InventoryItem inv = FindInventoryItemToAdd(itemObject, false);
			int index = inv != null ? items.IndexOf(inv) : -1;
			if (index < 0) {
				Debug.LogWarning(this + " does not contain item " + itemObject);
				return null;
			}
			if (!allowRemove) {
				Debug.LogWarning(this + " will not remove " + inv);
				return null;
			}
			return RemoveItemAt(index);
		}
		private InventoryItem RemoveItemAt(int index) {
			InventoryItem inv = items[index];
			items.RemoveAt(index);
			itemAddBehavior?.onRemove?.Invoke(inv);
			return inv;
		}
		public void DropAllItems() {
			for(int i = items.Count-1; i >= 0; --i) {
				items[i].RemoveFromCurrentInventory();
            }
        }
	}
}