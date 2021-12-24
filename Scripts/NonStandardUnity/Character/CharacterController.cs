using NonStandard.Utility.UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace NonStandard.Character {
	public class CharacterController : MonoBehaviour {
		[Tooltip("What character to pass input to")]
#if ENABLE_INPUT_SYSTEM
		[ContextMenuItem("Add default user controls", "CreateDefaultUserControls")]
#endif
		[SerializeField] protected CharacterRoot target;
		[System.Serializable] public class UnityEvent_Vector2 : UnityEvent<Vector2> { }
		public UnityEvent_Vector2 onMoveInput;
		public Transform MoveTransform {
			get { return target != null ? target.transform : null; }
			set {
				Target = value.GetComponent<CharacterRoot>();
			}
		}
		public CharacterRoot Target {
			get { return target; }
			set {
				target = value;
				transform.parent = MoveTransform;
				transform.localPosition = Vector3.zero;
				transform.localRotation = Quaternion.identity;
			}
		}
		public float JumpInput {
			get { return target != null ? target.move.JumpInput : 0; }
			set { if (target != null) target.move.JumpInput = value; }
		}
		public float MoveSpeed {
			get { return target != null ? target.move.MoveSpeed : 0; }
			set { if (target != null) target.move.MoveSpeed = value; }
		}
		public float JumpHeight {
			get { return target != null ? target.move.JumpHeight : 0; }
			set { if (target != null) target.move.JumpHeight = value; }
		}
		public Vector2 MoveInput {
			get => new Vector2(target.move.StrafeRightMovement, target.move.MoveForwardMovement);
			set {
				target.move.StrafeRightMovement = value.x;
				target.move.MoveForwardMovement = value.y;
				onMoveInput?.Invoke(value);
			}
		}
		public bool IsAutoMoving() { return target.move.IsAutoMoving(); }
		public void SetAutoMovePosition(Vector3 position, System.Action whatToDoWhenTargetIsReached = null, float closeEnough = 0) {
			if (target != null) { target.move.SetAutoMovePosition(position, whatToDoWhenTargetIsReached, closeEnough); }
		}
		public void DisableAutoMove() { if (target != null) target.move.DisableAutoMove(); }
		public float GetJumpProgress() { return target != null ? target.move.GetJumpProgress() : 0; }
		public bool IsStableOnGround() { return target != null ? target.move.IsStableOnGround() : false; }
#if ENABLE_INPUT_SYSTEM
		public void SetMove(InputAction.CallbackContext context) {
			MoveInput = context.ReadValue<Vector2>();
		}
		public void SetJump(InputAction.CallbackContext context) {
			switch (context.phase) {
				case InputActionPhase.Started: JumpInput = float.PositiveInfinity; break;
				case InputActionPhase.Canceled: JumpInput = 0; break;
			}
		}
		public void CreateDefaultUserControls() {
			CharacterCamera cc = GetComponentInChildren<CharacterCamera>();
			if (cc == null) { cc = GetComponentInParent<CharacterCamera>(); }
			UserInput userInput = GetComponent<UserInput>();
			if (userInput == null) {
				userInput = gameObject.AddComponent<UserInput>();
            }
			if (userInput == null) { userInput = gameObject.AddComponent<UserInput>(); }
			userInput.AddBinding(new UserInput.Binding("Move", "Vector2", new EventBind(this, nameof(this.SetMove))));
			userInput.AddBinding(new UserInput.Binding("Jump", "Button", new EventBind(this, nameof(this.SetJump))));
			if (cc != null) {
				userInput.AddBinding(new UserInput.Binding("Toggle MouseLook", "Button", new EventBind(this, nameof(BindMouselookInputMapToButton))));
				userInput.AddBinding(new UserInput.Binding("Look", "Vector2", new EventBind(cc, nameof(cc.ProcessLookRotation))));
				userInput.AddBinding(new UserInput.Binding("MouseLook", "Vector2", new EventBind(cc, nameof(cc.ProcessLookRotation))));
			}
		}
		InputActionMap mouselookActionMap;
		void BindMouselookInputMapToButton(InputAction.CallbackContext context) {
			if (mouselookActionMap == null) {
				UserInput userInput = GetComponent<UserInput>();
				const string MouseLookActionMapName = "MouseLook";
				mouselookActionMap = userInput.inputActionAsset.FindActionMap(MouseLookActionMapName);
				if (mouselookActionMap == null) {
					throw new System.Exception($"character controls need a `{MouseLookActionMapName}` action map");
				}
			}
			switch (context.phase) {
				case InputActionPhase.Started: mouselookActionMap.Enable(); break;
				case InputActionPhase.Canceled: mouselookActionMap.Disable(); break;
			}
		}
#else
		CharacterCamera cc;
		void Start() {
			cc = CharacterCamera.FindCameraTargettingChildOf(target.transform);
		}
		void Update() {
			Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
			bool jump = Input.GetButton("Jump");
			MoveInput = input;
			JumpInput = jump ? 1 : 0;
			if (cc != null && Input.GetMouseButton(1)) {
				cc.ProcessLookRotation(new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")));
			}
		}
#endif
	}
}