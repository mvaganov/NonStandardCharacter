using UnityEngine;

namespace NonStandard.Character {
	public class CharacterRoot : MonoBehaviour {
		public CharacterMove move;
		public Object data;
		public void Init() {
			if (move == null) { move = GetComponentInChildren<CharacterMove>(); }
		}
		public void Awake() { Init(); }
		public void Start() { Init(); }
		public void TakeControlOfUserInterface() {
			UserController user = UserController.GetUserCharacterController();
			user.CharacterCamera.target = move.head != null ? move.head : move.transform;
			user.Target = this;
		}
	}
}