using System.Collections.Generic;
using UnityEngine;

namespace NonStandard.GameUi.Inventory {
	public class Inventory : MonoBehaviour {
		public bool allowAdd = true;
		[ContextMenuItem("Drop All Items", nameof(DropAllItems))]
		public bool allowRemove = true;
		[SerializeField] private List<InventoryItem> items;
		public InventoryItem.SpecialBehavior itemAddBehavior;

		/// <summary>
		/// intended for use by DataSheet
		/// </summary>
		/// <param name="data"></param>
		public void DataPopulator(List<object> data) {
			if (items == null) { return; }
			data.Clear();
			for(int i = 0; i < items.Count; ++i) {
				data.Add(items[i]);
			}
		}
		public List<InventoryItem> GetItems() { return items; }

		public void ActivateGameObject(object itemObject) {
			//Debug.Log("activate " + itemObject);
			switch (itemObject) {
				case InventoryItem i: ActivateGameObject(i.component); return;
				case GameObject go: go.SetActive(true); return;
				case Component c: c.gameObject.SetActive(true); return;
			}
		}
		public void DeactivateGameObject(object itemObject) {
			//Debug.Log("deactivate " + itemObject);
			switch (itemObject) {
				case InventoryItem i: DeactivateGameObject(i.component); return;
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
		public InventoryItem FindInventoryItemToAdd(object data, bool createIfMissing) {
			switch (data) {
				case InventoryItem invi: return invi;
				case InventoryItemObject invio: return invio.item;
				case GameObject go: {
					InventoryItemObject invio = go.GetComponent<InventoryItemObject>();
					if (invio != null) { return invio.item; }
					for (int i = 0; i < items.Count; ++i) {
						if (items[i].data == data) {
							return items[i];
						}
					}
					if (createIfMissing) {
						invio = go.AddComponent<InventoryItemObject>();
						invio.item.component = invio;
						invio.item.data = data;
						return invio.item;
					}
				}break;
			}
			Debug.LogWarning("cannot convert ("+data.GetType()+") "+data+" into InventoryItem");
			return null;
		}
		internal InventoryItem AddItem(object itemObject) {
			InventoryItem inv = AddItemWithoutNotify(itemObject);
			if (inv != null && itemAddBehavior != null && itemAddBehavior.onAdd != null && itemAddBehavior.onAdd.GetPersistentEventCount() > 0) {
				itemAddBehavior.onAdd.Invoke(itemObject);
			}
			return inv;
		}
		internal InventoryItem AddItemWithoutNotify(object itemObject) {
			if (items == null) { items = new List<InventoryItem>(); }
			InventoryItem inv = FindInventoryItemToAdd(itemObject, true);
			if (items.Contains(inv)) {
				Debug.LogWarning(this + " already has item " + inv);
				return null;
			}
			return InsertItemWithoutNotify(items.Count, inv);
		}
		private InventoryItem InsertItemWithoutNotify(int index, InventoryItem inv) {
			if (!allowAdd) {
				Debug.LogWarning(this + " will not add " + inv);
				return null;
			}
			items.Insert(index, inv);
			return inv;
		}
		public InventoryItem RemoveItem(object itemObject) {
			InventoryItem inv = RemoveItemWithoutNotify(itemObject);
			if (inv != null && itemAddBehavior != null && itemAddBehavior.onRemove != null && itemAddBehavior.onRemove.GetPersistentEventCount() > 0) {
				itemAddBehavior.onRemove.Invoke(inv);
			}
			return inv;
		}
		internal InventoryItem RemoveItemWithoutNotify(object itemObject) {
			InventoryItem inv = FindInventoryItemToAdd(itemObject, false);
			int index = inv != null ? items.IndexOf(inv) : -1;
			if (index < 0) {
				Debug.LogWarning(this + " does not contain item " + itemObject);
				return null;
			}
			return RemoveItemAtWithoutNotify(index);
		}
		private InventoryItem RemoveItemAtWithoutNotify(int index) {
			InventoryItem inv = items[index];
			if (!allowRemove) {
				Debug.LogWarning(this + " will not remove " + inv);
				return null;
			}
			items.RemoveAt(index);
			return inv;
		}
		public void DropAllItems() {
			for(int i = items.Count-1; i >= 0; --i) {
				items[i].Drop();
			}
		}
	}
}