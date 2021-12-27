using NonStandard.Inputs;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace NonStandard.Character {
	public class FpsCharacterController : MonoBehaviour {
		[Tooltip("What character is being controlled (right-click to add default controls)")]
		[ContextMenuItem("Add default user controls", "CreateDefaultUserControls")]
		[SerializeField] protected CharacterRoot target;
		[Tooltip("What camera is being controlled")]
		[SerializeField] protected CharacterCamera _camera;
		InputActionMap mouselookActionMap;
		public UnityEvent_Vector2 onMoveInput;
		[System.Serializable] public class UnityEvent_Vector2 : UnityEvent<Vector2> { }
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
		public float FireInput {
			get { return target != null ? target.move.FireInput : 0; }
			set { if (target != null) target.move.FireInput = value; }
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
		public static bool IsPointerOverUIObject() {
			PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
			eventDataCurrentPosition.position = Mouse.current.position.ReadValue();
			List<RaycastResult> results = new List<RaycastResult>();
			EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
			return results.Count > 0;
		}
		public void SetFire(InputAction.CallbackContext context) {
			// ignore clicks when mouse is over UI
			if (context.control.path == "/Mouse/leftButton" && IsPointerOverUIObject()) {
				return;
            }
			switch (context.phase) {
				case InputActionPhase.Started: FireInput = float.PositiveInfinity; break;
				case InputActionPhase.Canceled: FireInput = 0; break;
			}
		}
		public void NotifyCameraRotation(InputAction.CallbackContext context) {
			_camera?.ProcessLookRotation(context);
		}
		public void NotifyCameraZoom(InputAction.CallbackContext context) {
			_camera?.ProcessZoom(context);
		}
		const string n_Player = "Player", n_MouseLook = "MouseLook", n_Move = "Move", n_Jump = "Jump", n_Look = "Look", n_Zoom = "Zoom", n_Fire = "Fire", n_ToggleML = "Toggle MouseLook";
		const string n_InputActionAsset = "FpsCharacterController", n_InputActionPath = "Assets/Resources";
#if UNITY_EDITOR
		public void CreateDefaultUserControls() {
			_camera = GetComponentInChildren<CharacterCamera>();
			if (_camera == null) { _camera = GetComponentInParent<CharacterCamera>(); }
			UserInput userInput = GetComponent<UserInput>();
			bool pleaseCreateInputActionAsset = false;
			if (userInput == null) {
				userInput = gameObject.AddComponent<UserInput>();
			}
			if (userInput == null) { userInput = gameObject.AddComponent<UserInput>(); }
			if (userInput.inputActionAsset == null) {
				pleaseCreateInputActionAsset = true;
				userInput.inputActionAsset = ScriptableObject.CreateInstance<InputActionAsset>();
			}
			Binding[] bindings = new Binding[] {
				new Binding(n_Player+"/"+n_Move,    ControlType.Vector2, new EventBind(this, nameof(this.SetMove)), new string[] {"<Gamepad>/leftStick", "<XRController>/{Primary2DAxis}", "<Joystick>/stick",
					Binding.CompositePrefix+"WASD:"+"Up:<Keyboard>/w,<Keyboard>/upArrow;Down:<Keyboard>/s,<Keyboard>/downArrow;Left:<Keyboard>/a,<Keyboard>/leftArrow;Right:<Keyboard>/d,<Keyboard>/rightArrow"}),
				new Binding(n_Player+"/"+n_Jump,    ControlType.Button,  new EventBind(this, nameof(this.SetJump)), new string[] {"<Keyboard>/space","<Gamepad>/buttonSouth"}),
				new Binding(n_Player + "/" + n_Fire,ControlType.Button,  new EventBind(this, nameof(this.SetFire)), new string[] {
				"<Gamepad>/rightTrigger","<Mouse>/leftButton","<Touchscreen>/primaryTouch/tap","<Joystick>/trigger","<XRController>/{PrimaryAction}","<Gamepad>/buttonWest"}),
				new Binding(n_Player+"/"+n_ToggleML,ControlType.Button,  new EventBind(this, nameof(BindMouselookInputMapToButton)), new string[] { "<Mouse>/rightButton" }),
				new Binding(n_Player+"/"+n_Look,    ControlType.Vector2, new EventBind(this, nameof(NotifyCameraRotation)), new string[] { "<Gamepad>/rightStick", "<Joystick>/{Hatswitch}" }),
				new Binding(n_MouseLook+"/"+n_Look, ControlType.Vector2, new EventBind(this, nameof(NotifyCameraRotation)), new string[] { "<VirtualMouse>/delta", "<Pointer>/delta", "<Mouse>/delta" }),
				new Binding(n_Player+"/"+n_Zoom,    ControlType.Vector2, new EventBind(this, nameof(NotifyCameraZoom)), new string[] { "<Mouse>/scroll" }),
			};
			foreach(Binding b in bindings) {
				userInput.AddBinding(b);
			}
			if (pleaseCreateInputActionAsset) {
				userInput.inputActionAsset.name = n_InputActionPath;
#if UNITY_EDITOR
				userInput.inputActionAsset = ScriptableObjectUtility.SaveScriptableObjectAsAsset(userInput.inputActionAsset,
					n_InputActionAsset + "." + InputActionAsset.Extension, n_InputActionPath, userInput.inputActionAsset.ToJson()) as InputActionAsset;
#endif
			}
		}
#endif
		void BindMouselookInputMapToButton(InputAction.CallbackContext context) {
			if (mouselookActionMap == null) {
				UserInput userInput = GetComponent<UserInput>();
				mouselookActionMap = userInput.inputActionAsset.FindActionMap(n_MouseLook);
				if (mouselookActionMap == null) {
					throw new System.Exception($"character controls need a `{n_MouseLook}` action map");
				}
			}
			switch (context.phase) {
				case InputActionPhase.Started: mouselookActionMap.Enable(); break;
				case InputActionPhase.Canceled: mouselookActionMap.Disable(); break;
			}
		}
#else
		CharacterCamera _camera;
		void Start() {
			_camera = CharacterCamera.FindCameraTargettingChildOf(target.transform);
		}
		void Update() {
			Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
			bool jump = Input.GetButton("Jump");
			MoveInput = input;
			JumpInput = jump ? 1 : 0;
			if (_camera != null && Input.GetMouseButton(1)) {
				_camera.ProcessLookRotation(new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")));
			}
		}
#endif
	}
}