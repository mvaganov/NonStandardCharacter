using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace NonStandard.Character {
	public class CharacterCamera : MonoBehaviour
	{
		[Tooltip("which transform to follow with the camera")]
		public Transform _target;
		[Tooltip("if false, camera can pass through walls")]
		public bool clipAgainstWalls = true;

		/// <summary>how the camera should be rotated, calculated in Update, to keep LateUpdate as light as possible</summary>
		private Quaternion targetRotation;
		/// <summary>how far the camera wants to be from the target</summary>
		public float targetDistance = 10;
		/// <summary>calculate how far to clip the camera in the Update, to keep LateUpdate as light as possible
		private float distanceBecauseOfObstacle;
		/// <summary>
		/// user-defined rotation
		/// </summary>
		private Quaternion userRotation;
		/// <summary>
		/// user-defined zoom
		/// </summary>
		public float userDistance;
		[System.Serializable] public class UnityEvent_Vector2 : UnityEvent<Vector2> { }
		[Tooltip("notified if the look rotation is changed, like from a mouse or joystick adjustment")]
		public UnityEvent_Vector2 OnLookInputChange;
		private Transform userTarget;
		/// <summary>for fast access to transform</summary>
		private Transform t;

		private Camera cam;

		/// <summary>keep track of rotation, so it can be un-rotated and cleanly re-rotated</summary>
		private float pitch, yaw;

		public float maxVerticalAngle = 100, minVerticalAngle = -100;
		public Vector2 inputMultiplier = Vector2.one;

		public Camera Camera => cam;
		public Transform target { get { return _target; } 
			set {
				//Debug.Log("target! "+Show.GetStack(10));
				_target = userTarget = value; 
			}
		}

		public Vector2 LookInput {
			get => new Vector2(horizontalRotateInput, verticalRotateInput);
			set {
				horizontalRotateInput = value.x;
				verticalRotateInput = value.y;
				OnLookInputChange?.Invoke(value);
			}
		}
		/// publicly accessible variables that can be modified by external scripts or UI
		[HideInInspector] public float horizontalRotateInput, verticalRotateInput, zoomInput;
		public float ZoomInput { get { return zoomInput; } set { zoomInput = value; } }
		public void AddToTargetDistance(float value) {
			targetDistance += value;
			if(targetDistance < 0) { targetDistance = 0; }
			OrthographicCameraDistanceChangeLogic();
		}
		public void OrthographicCameraDistanceChangeLogic() {
			if (cam != null && cam.orthographic) {
				if (targetDistance < 1f / 128) { targetDistance = 1f / 128; }
				cam.orthographicSize = targetDistance / 2;
			}
		}

		public bool IsTargettingChildOf(Transform targetRoot) {
			if (_target == null && targetRoot != null) return false;
			Transform t = _target;
			do {
				if (targetRoot == t) { return true; }
				t = t.parent;
			} while (t != null);
			return false;
        }

		/// <summary>
		/// does linear exhaustive search through all <see cref="CharacterCamera"/>s and looks at parent transform hierarchy
		/// </summary>
		/// <param name="t"></param>
		/// <returns></returns>
		public static CharacterCamera FindCameraTargettingChildOf(Transform t) {
			CharacterCamera[] ccs = FindObjectsOfType<CharacterCamera>();
			for(int i = 0; i < ccs.Length; ++i) {
				if (ccs[i].IsTargettingChildOf(t)) { return ccs[i]; }
            }
			return null;
        }

#if ENABLE_INPUT_SYSTEM
		public void ProcessLookRotation(InputAction.CallbackContext context) {
			ProcessLookRotation(context.ReadValue<Vector2>());
		}
		public void ProcessZoom(InputAction.CallbackContext context) {
			ProcessZoom(context.ReadValue<Vector2>());
		}
#endif
		public void ProcessLookRotation(Vector2 lookRot) {
			LookInput = lookRot;
		}
		public void ProcessZoom(Vector2 lookRot) {
			ZoomInput = lookRot.y;
		}
		public void ToggleOrthographic() { cam.orthographic = !cam.orthographic; }
		public void SetCameraOrthographic(bool orthographic) { cam.orthographic = orthographic; }
	
#if UNITY_EDITOR
		/// called when created by Unity Editor
		void Reset() {
			if (target == null) {
				CharacterMove body = null;
				if (body == null) { body = transform.GetComponentInParent<CharacterMove>(); }
				if (body == null) { body = FindObjectOfType<CharacterMove>(); }
				if (body != null) { target = body.head; }
			}
		}
	#endif

		public void SetMouseCursorLock(bool a_lock) {
			Cursor.lockState = a_lock ? CursorLockMode.Locked : CursorLockMode.None;
			Cursor.visible = !a_lock;
		}
		public void LockCursor() { SetMouseCursorLock(true); }
		public void UnlockCursor() { SetMouseCursorLock(false); }

		public void Awake() { t = transform; }

		public void Start() {
			RecalculateDistance();
			RecalculateRotation();
			userTarget = target;
			userRotation = t.rotation;
			userDistance = targetDistance;
			targetView.target = userTarget;
			targetView.rotation = userRotation;
			targetView.distance = userDistance;
			cam = GetComponent<Camera>();
			//for (int i = 0; i < knownCameraViews.Count; ++i) {
			//	knownCameraViews[i].ResolveLookRotation();
			//}
		}

		public bool RecalculateDistance() {
			float oldDist = targetDistance;
			if (target != null) {
				Vector3 delta = t.position - target.position;
				targetDistance = delta.magnitude;
			}
			return oldDist != targetDistance;
		}
		public bool RecalculateRotation() {
			float oldP = pitch, oldY = yaw;
			targetRotation = t.rotation;
			Vector3 right = Vector3.Cross(t.forward, Vector3.up);
			if(right == Vector3.zero) { right = -t.right; }
			Vector3 straightForward = Vector3.Cross(Vector3.up, right).normalized;
			pitch = Vector3.SignedAngle(straightForward, t.forward, -right);
			yaw = Vector3.SignedAngle(Vector3.forward, straightForward, Vector3.up);
			return oldP != pitch || oldY != yaw;
		}

		public void Update() {
			const float anglePerSecondMultiplier = 100;
			float rotH = horizontalRotateInput * anglePerSecondMultiplier * inputMultiplier.x * Time.unscaledDeltaTime,
				rotV = verticalRotateInput * anglePerSecondMultiplier * inputMultiplier.y * Time.unscaledDeltaTime,
				zoom = zoomInput * Time.unscaledDeltaTime;
			targetDistance -= zoom;
			if (zoom != 0) {
				userDistance = targetDistance;
				if (targetDistance < 0) { targetDistance = 0; }
				if (target == null) {
					t.position += t.forward * zoom;
				}
				OrthographicCameraDistanceChangeLogic();
			}
			if (rotH != 0 || rotV != 0) {
				targetRotation = Quaternion.identity;
				yaw += rotH;
				pitch -= rotV;
				if (yaw < -180) { yaw += 360; }
				if (yaw >= 180) { yaw -= 360; }
				if (pitch < -180) { pitch += 360; }
				if (pitch >= 180) { pitch -= 360; }
				pitch = Mathf.Clamp(pitch, minVerticalAngle, maxVerticalAngle);
				targetRotation *= Quaternion.Euler(pitch, yaw, 0);
				userRotation = targetRotation;
			}
			if (target != null) {
				RaycastHit hitInfo;
				bool usuallyHitsTriggers = Physics.queriesHitTriggers;
				Physics.queriesHitTriggers = false;
				if (clipAgainstWalls && Physics.Raycast(target.position, -t.forward, out hitInfo, targetDistance)) {
					distanceBecauseOfObstacle = hitInfo.distance;
				} else {
					distanceBecauseOfObstacle = targetDistance;
				}
				Physics.queriesHitTriggers = usuallyHitsTriggers;
			}
		}

		public void LateUpdate() {
			t.rotation = targetRotation;
			if(target != null) {
				t.position = target.position - (t.rotation * Vector3.forward) * distanceBecauseOfObstacle;
			}
		}
		[System.Serializable] public class CameraView {
			public string name;
			[HideInInspector] public Quaternion rotation;
			[SerializeField] public Vector3 _Rotation;
			/// <summary>
			/// if target is null, use this
			/// </summary>
			public Vector3 position;
			public Transform target;
			public float distance;
			public bool useTransformPositionChanges;
			public bool ignoreLookRotationChanges;
			public bool rotationIsLocal;
			public bool positionLocalToLastTransform;
			public void ResolveLookRotationIfNeeded() {
				if(rotation.x==0&&rotation.y==0&&rotation.z==0&&rotation.w==0) {
					rotation = Quaternion.Euler(_Rotation);
				}
				//Debug.Log(Show.Stringify(this));
			}
		}
		public List<CameraView> knownCameraViews = new List<CameraView>();

		public void ToggleView(string viewName) {
			if(currentViewname != defaultViewName) {
				LerpView(defaultViewName);
			} else {
				LerpView(viewName);
			}
		}
		private string defaultViewName = "user";
		private string currentViewname = "user";
		public string CurrentViewName { get { return currentViewname; } }
		public void SetLerpSpeed(float durationSeconds) { lerpDurationMs = (long)(durationSeconds*1000); }
		private ulong started, end;
		public long lerpDurationMs = 250;
		private float distStart;
		private Quaternion rotStart;
		private bool lerping = false;
		private Vector3 startPosition;
		private CameraView targetView = new CameraView();
		public void LerpView(string viewName) {
			currentViewname = viewName;
			string n = viewName.ToLower();
			switch (n) {
			case "user":
				LerpRotation(userRotation);
				LerpDistance(userDistance);
				LerpTarget(userTarget);
				return;
			default:
				for(int i = 0; i < knownCameraViews.Count; ++i) {
					if (knownCameraViews[i].name.ToLower().Equals(n)) {
						//Debug.Log("doing " + n + " "+Show.Stringify(knownCameraViews[i]));
						LerpTo(knownCameraViews[i]);
						return;
					}
				}
				break;
			}
			/*
			ReflectionParseExtension.TryConvertEnumWildcard(typeof(Direction3D), viewName, out object v);
			if (v != null) {
				LerpDirection((Direction3D)v); return;
			}
			*/
			Debug.LogWarning($"unkown view name \"{viewName}\"");
		}
		public enum Direction3D { Down = 1, Left = 2, Back = 4, Up = 8, Right = 16, Forward = 32, }
		public Vector3 ConvertToVector3(Direction3D dir) { switch (dir) {
				case Direction3D.Down:   return Vector3.down;
				case Direction3D.Left:   return Vector3.left;
				case Direction3D.Back:   return Vector3.back;
				case Direction3D.Up:     return Vector3.up;
				case Direction3D.Right:  return Vector3.right;
				case Direction3D.Forward:return Vector3.forward;
			}
			return Vector3.zero;
		}
		public void LerpDirection(Direction3D dir) { LerpDirection(ConvertToVector3(dir)); }
		public void LerpDirection(Vector3 direction) { LerpRotation(Quaternion.LookRotation(direction)); }
		public void LerpRotation(Quaternion direction) {
			targetView.rotation = direction;
			StartLerpToTarget();
		}
		public void LerpDistance(float distance) {
			targetView.distance = distance;
			StartLerpToTarget();
		}
		public void LerpTarget(Transform target) {
			targetView.target = target;
			StartLerpToTarget();
		}
		public void LerpTo(CameraView view) {
			targetView.name = view.name;
			targetView.useTransformPositionChanges = view.useTransformPositionChanges;
			targetView.ignoreLookRotationChanges = view.ignoreLookRotationChanges;
			targetView.rotationIsLocal = view.rotationIsLocal;
			targetView.positionLocalToLastTransform = view.positionLocalToLastTransform;
			if (view.useTransformPositionChanges) { targetView.target = view.target; }
			targetView.distance = view.distance;
			if (!view.ignoreLookRotationChanges) {
				view.ResolveLookRotationIfNeeded();
				targetView.rotation = view.rotation;
			}
			StartCoroutine(StartLerpToTarget());
		}
		public IEnumerator StartLerpToTarget() {
			if (lerping) yield break;
			lerping = true;
			rotStart = t.rotation;
			startPosition = t.position;
			distStart = distanceBecauseOfObstacle;
			if (targetView.positionLocalToLastTransform && _target != null) {
				Quaternion q = !targetView.ignoreLookRotationChanges ? targetView.rotation : t.rotation;
				targetView.position = _target.position - (q * Vector3.forward) * targetView.distance;
				Debug.Log("did the thing");
			}
			//if (targetView.target != null) {
				_target = null;
			//}
			started = CharacterMove.Now;
			end = CharacterMove.Now + (ulong)lerpDurationMs;
			yield return null;
			//Proc.Delay(0, LerpToTarget);
            while (lerping) {
				LerpToTarget();
				yield return null;
			}
		}
		private void LerpToTarget() {
			lerping = true;
			ulong now = CharacterMove.Now;
			ulong passed = now - started;
			float p = (float)passed / lerpDurationMs;
			if (now >= end) { p = 1; }
			if (!targetView.ignoreLookRotationChanges) {
				targetView.ResolveLookRotationIfNeeded();
				if (targetView.rotationIsLocal) {
					Quaternion startQ = targetView.rotationIsLocal ? targetView.target.rotation : Quaternion.identity;
					Quaternion.Lerp(rotStart, targetView.rotation * startQ, p);
				} else {
					t.rotation = Quaternion.Lerp(rotStart, targetView.rotation, p);
				}
			}
			//Show.Log("asdfdsafdsa");
			targetDistance = (targetView.distance - distStart) * p + distStart;
			if (targetView.useTransformPositionChanges) {
				if (targetView.target != null) {
					Quaternion rot = targetView.rotation * (targetView.rotationIsLocal ? targetView.target.rotation : Quaternion.identity);
					Vector3 targ = targetView.target.position;
					Vector3 dir = rot * Vector3.forward;
					RaycastHit hitInfo;
					if (clipAgainstWalls && Physics.Raycast(targ, -dir, out hitInfo, targetView.distance)) {
						distanceBecauseOfObstacle = hitInfo.distance;
					} else {
						distanceBecauseOfObstacle = targetView.distance;
					}
					Vector3 finalP = targ - dir * distanceBecauseOfObstacle;
					//Debug.Log(targetView.distance+"  "+distanceBecauseOfObstacle+"  "+targ+" "+targetView.target);
					t.position = Vector3.Lerp(startPosition, finalP, p);
					//Debug.Log("# "+p+" "+finalP);
				} else {
					t.position = Vector3.Lerp(startPosition, targetView.position, p);
					//Debug.Log("!" + p + " " + targetView.position);
				}
			}
			RecalculateRotation();
			if (p >= 1) {
				if (targetView.useTransformPositionChanges) {
					_target = targetView.target;
				}
				lerping = false;
			}
		}
	}
}