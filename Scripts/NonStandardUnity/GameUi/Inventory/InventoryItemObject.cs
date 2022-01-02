using System.Collections;
using UnityEngine;

namespace NonStandard.GameUi.Inventory {
	public class InventoryItemObject : MonoBehaviour {
		public InventoryItem item = new InventoryItem();
		[Tooltip("will generate a sphere collider if none is given")]
		public Collider trigger;
		private void Awake() {
			item.component = this;
		}
		private void Start() {
			if (trigger == null) {
				Renderer renderer = GetComponent<Renderer>();
				Bounds b = renderer.bounds;
				Vector3 center = b.center;
				float radius = b.extents.magnitude;
				SphereCollider sc = gameObject.AddComponent<SphereCollider>();
				sc.center = center - transform.position;
				sc.radius = radius;
				sc.isTrigger = true;
				trigger = sc;
			}
			if (!trigger.isTrigger) {
				Debug.Log("expecting collider **trigger** on " + this);
			}
		}
		public void OnEnable() {
			if (trigger != null) {
				trigger.enabled = false;
				//Proc.Delay(2500, () => { if (trigger != null) trigger.enabled = true; });
				StartCoroutine(DelayTrigger()); IEnumerator DelayTrigger() {
					yield return new WaitForSeconds(2.5f);
					trigger.enabled = true;
				}
			}
		}
		private void OnTriggerEnter(Collider other) {
			if (!trigger.enabled) return; // don't do trigger logic if the collider is supposed to be off.
			item.OnTrigger(other.gameObject);
		}
	}
}
