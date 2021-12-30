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
			CharacterCamera cam = FpsCharacterController.GetCharacterCamera();
			cam.target = move.head != null ? move.head : move.transform;
			FpsCharacterController fpsChar = FpsCharacterController.GetCharacterControllerOf(cam.target);
			fpsChar.Target = this;
		}
	}
}