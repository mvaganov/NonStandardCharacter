using UnityEngine;
using UnityEngine.Events;

namespace NonStandard.Character {
	public class CharacterRoot : MonoBehaviour
	{
		public CharacterMove move;
		public Object data;
		[System.Serializable] public class UnityEvent_GameObject : UnityEvent<GameObject> { }
		public UnityEvent_GameObject activateFunction;

		public void Init() {
			if (move == null) { move = GetComponentInChildren<CharacterMove>(); }
			/*
			if (data == null) { data = GetComponentInChildren<ScriptedDictionary>(); }
			*/
		}
		public void Awake() { Init(); }
		public void Start() { Init(); }
		public void DoActivateTrigger() {
			if (activateFunction.GetPersistentEventCount() == 0) {
				Debug.Log("empty activation function?");
			}
			activateFunction.Invoke(gameObject);
		}
		public void DoActivateTrigger(int a, int b) {
			Debug.Log(a + b);
		}
	}
}