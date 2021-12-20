using NonStandard.GameUi.Inventory;
using NonStandard.Process;
using UnityEngine;

public class InventoryItemObject : MonoBehaviour {
    public InventoryItem item = new InventoryItem();
    [Tooltip("will generate a sphere collider if none is given")]
    public Collider trigger;
    private void Awake() {
        item.data = this;
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
            Debug.Log("expecting collider **trigger** on "+this);
        }
    }
    public void OnEnable() {
        if (trigger != null) {
            trigger.enabled = false;
            Proc.Delay(2500, () => { if (trigger != null) trigger.enabled = true; });
        }
	}
    private void OnTriggerEnter(Collider other) {
        if (!trigger.enabled) return; // don't do trigger logic if the collider is supposed to be off.
        item.OnTrigger(other.gameObject);
    }
}
