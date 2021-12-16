using NonStandard.Character;
using NonStandard.Utility.UnityEditor;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

#if !ENABLE_INPUT_SYSTEM
public class CharacterInputAutomate : CharacterInputLegacy { }
#else
using UnityEngine.InputSystem;
public class UserInput : MonoBehaviour {
    [Tooltip("The Character's Controls")]
    public InputActionAsset inputActionAsset;
    [Tooltip("if not null, this is where input related scripts will be added and modified.")]
    public Transform playerInputObject;
    [Tooltip("Where the player should get input from. If null (expected), will generate PlayerInput script here.")]
    public PlayerInput playerInput;
    const string MouseLookActionMapName = "MouseLook";
    InputActionMap mouselookActionMap;

    public InputActionBindings[] inputBindings;
    bool initialized = false;

#if UNITY_EDITOR
    private void Reset() {
        GenerateDefaultMovesForCharacter();
    }
    void GenerateDefaultMovesForCharacter() {
        CharacterMove cm = GetComponent<CharacterMove>();
        if (cm == null) {
            Transform t = transform;
            do {
                cm = t.GetComponent<CharacterMove>();
                t = t.parent;
            } while (cm == null && t != null);
        }
        CharacterCamera cc = GetComponentInChildren<CharacterCamera>();
        if (cm == null) {
            cm = cc.target.GetComponentInParent<CharacterMove>();
        }
        if (cc != null && !cc.IsTargettingChildOf(cm.transform)) { cc = null; }
        if (cc == null) { cc = CharacterCamera.FindCameraTargettingChildOf(cm.transform); }
        inputBindings = new InputActionBindings[] {
            new InputActionBindings("Move", "Vector2", new EventBind(cm, nameof(cm.SetMove))),
            new InputActionBindings("Jump", "Button", new EventBind(cm, nameof(cm.SetJump))),
            new InputActionBindings("Toggle MouseLook", "Button", new EventBind(this, nameof(BindMouselookInputMapToButton))),
            new InputActionBindings("MouseLook", "Vector2", new EventBind(cc, nameof(cc.ProcessLookRotation)))
        };
    }
#endif

    void Start() {
        if (playerInputObject == null) {
            playerInputObject = transform;
        }
        if (playerInput == null) {
            playerInput = GetComponent<PlayerInput>();
            if (playerInput == null) {
                // INTENTIONALLY NOT CREATING A PlayerInput OBJECT. PlayerInput is buggy, and won't update it's action
                // events dynamically, except in edit mode when the UI is focused on the PlayerInput object.
                //playerInput = playerInputObject.gameObject.AddComponent<PlayerInput>();
            }
            if (playerInput != null) {
                playerInput.notificationBehavior = PlayerNotifications.InvokeUnityEvents;
            }
        } else {
            if (inputActionAsset == null) {
                inputActionAsset = playerInput.actions;
            }
        }
        mouselookActionMap = inputActionAsset.FindActionMap(MouseLookActionMapName);
        if (mouselookActionMap == null) {
            throw new Exception($"character controls need a `{MouseLookActionMapName}` action map");
        }
        if (playerInput == null) {
            BindDirectlyWithoutPlayerInput(inputBindings, true);
        } else {
            StartCoroutine(MakeSureCharacterMoveBindsAreSet(playerInput));
        }
    }
    /// <summary>
    /// the <see cref="PlayerInput"/> class updates it's actionEvents during Editor time, so it can't be used dynamically.
    /// </summary>
    /// <param name="pi"></param>
    /// <returns></returns>
    IEnumerator MakeSureCharacterMoveBindsAreSet(PlayerInput pi) {
        int attempts = 0;
        for (int i = 0; i < inputBindings.Length; ++i) {
            if (!inputBindings[i].BindAction(playerInput)) {
                attempts++;
                if(attempts > 10000) {
                    Debug.Log("ok, that's too long.");
                    yield break;
                }
                //Debug.Log("try again soon "+attempts+" "+playerInput.actionEvents.Count);
                --i;
                yield return null;
            }
        }
        playerInput.enabled = false;
        yield return null;
        playerInput.enabled = true;
        initialized = true;
    }
    void BindDirectlyWithoutPlayerInput(InputActionBindings[] inputs, bool enable) {
        for (int i = 0; i < inputs.Length; ++i) {
            InputActionBindings inp = inputs[i];
            InputAction ia = FindAction(inputActionAsset, inp.name, inp.type);
            if (ia == null) {
                Debug.Log("Missing " + inp.name + " (" + inp.type + ")");
            } else {
                if (enable) {
                    inp.BindAction(ia);
                } else {
                    inp.UnbindAction(ia);
                }
            }
        }
        inputActionAsset.FindActionMap("Player").Enable();
        initialized = true;
    }
    private void OnEnable() {
        if (!initialized) return;
        BindDirectlyWithoutPlayerInput(inputBindings, true);
    }
    private void OnDisable() {
        if (!initialized) return;
        BindDirectlyWithoutPlayerInput(inputBindings, false);
    }

    public static InputAction FindAction(InputActionAsset actionAsset, string actionName, string actionInputType) {
        foreach (var actionMap in actionAsset.actionMaps) {
            string n = "/" + actionName;
            foreach (var action in actionMap.actions) {
                if (action.name == actionName || action.name.Contains(n)) {
                    if (action.expectedControlType != actionInputType) {
                        Debug.Log("found " + actionName + ", but Input type is " + action.expectedControlType + ", not " + actionInputType);
                    } else {
                        return action;
                    }
                }
            }
            //foreach (var action in actionMap.actions) { Debug.Log(actionName+"???\n"+ actionMap.actions.JoinToString("\n",a=>a.name)); }
        }
        return null;
    }

    void BindMouselookInputMapToButton(InputAction.CallbackContext context) {
        switch (context.phase) {
            case InputActionPhase.Started: mouselookActionMap.Enable(); break;
            case InputActionPhase.Canceled: mouselookActionMap.Disable(); break;
        }
    }

    [Serializable]
    public class UnityInputEvent : UnityEvent<InputAction.CallbackContext> { }

    [Serializable]
    public class InputActionBindings {
        public string name, type;
        public EventBind evnt;
        public UnityInputEvent startPerformCancel = new UnityInputEvent();
        public InputActionBindings(string n, string t, EventBind e) {
            name = n; type = t; evnt = e;
            e.Bind(startPerformCancel);
        }
        public bool BindAction(PlayerInput playerInput) {
            if (playerInput.actions != null) {
                //Debug.Log("!!!!! player assign " + name + " " + playerInput.actionEvents.Count);
                string n = "/" + name;
                foreach (var e in playerInput.actionEvents) {
                    //Debug.Log("~~~~ " + e.actionName + " vs " + name);
                    if (e.actionName.Contains(n)) {
                        Debug.Log(name+" binding {" + evnt + "()} to " + e);
                        //evnt.Bind(e);
                        e.AddListener(startPerformCancel.Invoke);
                        return true;
                    }
                }
            }
            return false;
        }
        public void BindAction(InputAction ia) {
            if (startPerformCancel != null) {
                ia.started  -= startPerformCancel.Invoke;
                ia.performed-= startPerformCancel.Invoke;
                ia.canceled -= startPerformCancel.Invoke;
                ia.started  += startPerformCancel.Invoke;
                ia.performed+= startPerformCancel.Invoke;
                ia.canceled += startPerformCancel.Invoke;
            }
        }
        public void UnbindAction(InputAction ia) {
            if (startPerformCancel != null) {
                ia.started  -= startPerformCancel.Invoke;
                ia.performed-= startPerformCancel.Invoke;
                ia.canceled -= startPerformCancel.Invoke;
            }
        }
    }
}
#endif
