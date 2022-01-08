using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NonStandard.GameUi.Inventory {
	public class InventoryInitialize : MonoBehaviour {
		[Tooltip("Items that started in the inventory. " + nameof(Inventory.itemAddBehavior) + " will be invoked for these also.")]
		[SerializeField] private InventoryItemObject[] initialItems;
		private void Start() {
			if (initialItems == null || initialItems.Length == 0) { return; }
			InventoryCollector collector = GetComponent<InventoryCollector>();
			if (collector != null) {
				for (int i = 0; i < initialItems.Length; ++i) {
					collector.AddItem(initialItems[i]);
				}
				return;
			}
			Inventory inventory = GetComponent<Inventory>();
			if (inventory == null) {
				throw new System.Exception(name+" can't initialize an inventory without an Inventory, or an InventoryCollector.");
			}
			for (int i = 0; i < initialItems.Length; ++i) {
				inventory.AddItem(initialItems[i]);
			}
		}
	}

}