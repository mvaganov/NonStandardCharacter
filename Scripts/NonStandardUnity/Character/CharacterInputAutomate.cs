using NonStandard.Character;
using NonStandard.Process;
using NonStandard.Utility.UnityEditor;
using System.Collections;
using UnityEngine;

#if !ENABLE_INPUT_SYSTEM
public class CharacterInputAutomate : CharacterInputLegacy { }
#else
using UnityEngine.InputSystem;
public class CharacterInputAutomate : MonoBehaviour {
    [Tooltip("The Character's Controls")]
    public InputActionAsset inputActionAsset;
    [Tooltip("if not null, this is where input related scripts will be added and modified.")]
    public Transform playerInputObject;
    GameObject internalIinputObject = null;
    [Tooltip("Where the player should get input from. If null (expected), will generate PlayerInput script here.")]
    public PlayerInput playerInput;
    const string MouseLookActionMapName = "MouseLook";
    InputActionMap mouselookActionMap;
    void Start() {
        if (playerInputObject == null) {
            playerInputObject = transform;
        }
        if (playerInput == null) {
            playerInput = GetComponent<PlayerInput>();
            if (playerInput == null) {
                internalIinputObject = new GameObject("player input");
                internalIinputObject.transform.SetParent(playerInputObject);
                playerInput = internalIinputObject.AddComponent<PlayerInput>();
            }
            playerInput.notificationBehavior = PlayerNotifications.InvokeUnityEvents;
        } else {
            if (inputActionAsset == null) {
                inputActionAsset = playerInput.actions;
            }
        }
        if (playerInput.actions == null && playerInput.notificationBehavior == PlayerNotifications.InvokeUnityEvents) {
            //playerInput.onControlsChanged -= MakeSureCharacterMoveBindsAreSet;
            //playerInput.onControlsChanged += MakeSureCharacterMoveBindsAreSet;
            playerInput.actions = inputActionAsset;
            //playerInput.DeactivateInput();
            //playerInput.ActivateInput();
        //    Proc.Delay(10000, () => MakeSureCharacterMoveBindsAreSet(playerInput));
        //} else {
        //    MakeSureCharacterMoveBindsAreSet(playerInput);
            if(internalIinputObject != null) {
                GameObject tryItAgainBecausePlayerInputWontRefreshActionEvents = Instantiate(internalIinputObject);
                Destroy(internalIinputObject);
                internalIinputObject = tryItAgainBecausePlayerInputWontRefreshActionEvents;
                internalIinputObject.transform.SetParent(playerInputObject);
                playerInput = internalIinputObject.GetComponent<PlayerInput>();
            }
        }
        StartCoroutine(MakeSureCharacterMoveBindsAreSet(playerInput));
    }
    IEnumerator MakeSureCharacterMoveBindsAreSet(PlayerInput pi) {
        CharacterMove cm = GetComponent<CharacterMove>();
        CharacterCamera cc = GetComponentInChildren<CharacterCamera>();
        if (cc != null && !cc.IsTargettingChildOf(cm.transform)) { cc = null; }
        if (cc == null) { cc = CharacterCamera.FindCameraTargettingChildOf(cm.transform); }
        Inpt[] inputs = new Inpt[] {
            new Inpt("Move", "Vector2", new EventBind(cm, nameof(cm.SetMove))),
            new Inpt("Jump", "", new EventBind(cm, nameof(cm.SetJump))),
            new Inpt("Toggle MouseLook", "", new EventBind(this, nameof(BindMouselookInputMapToButton))),
            new Inpt("MouseLook", "Vector2", new EventBind(cc, nameof(cc.ProcessLookRotation)))
        };
        mouselookActionMap = playerInput.actions.FindActionMap(MouseLookActionMapName);
        if (mouselookActionMap == null) { throw new System.Exception("character controls need a `MouseLook` action map"); }
        for (int i = 0; i < inputs.Length; ++i) {
            if (!inputs[i].BindAction(playerInput)) {
                attempts++;
                if(attempts > 10000) {
                    Debug.Log("ok, that's too long.");
                    yield break;
                }
                Debug.Log("try again soon "+attempts);
                --i;
                internalIinputObject.gameObject.SetActive(!internalIinputObject.gameObject.activeSelf);
                yield return null;
            }
        }
        playerInput.enabled = false;
        yield return null;
        playerInput.enabled = true;
        //InputActionMap playerMap = null, iam;
        //for (int i = 0; i < playerInput.actions.actionMaps.Count; ++i) {
        //    iam = playerInput.actions.actionMaps[i];
        //    Debug.Log("|"+iam.name+"|");
        //    if(iam.name == "Player") { playerMap = iam; break; }
        //}
        //iam = playerInput.actions.FindActionMap("Player");
        //Debug.Log("enabling "+playerMap+" "+iam);
        //playerMap.Enable();
    }
    private static int attempts = 0;

    void BindMouselookInputMapToButton(InputAction.CallbackContext context) {
        switch (context.phase) {
            case InputActionPhase.Started: mouselookActionMap.Enable(); break;
            case InputActionPhase.Canceled: mouselookActionMap.Disable(); break;
        }
    }

    struct Inpt {
        public string name, type;
        public EventBind evnt;
        public Inpt(string n, string t, EventBind e) {
            name = n; type = t; evnt = e;
        }
        public bool BindAction(PlayerInput playerInput) {
            if (playerInput.actions != null) {
                //Debug.Log("!!!!! player assign " + name + " " + playerInput.actionEvents.Count);
                string n = "/" + name;
                foreach (var e in playerInput.actionEvents) {
                    //Debug.Log("~~~~ " + e.actionName + " vs " + name);
                    if (e.actionName.Contains(n)) {
                        Debug.Log("binding " + evnt.target + "." + evnt.methodName + " to " + e);
                        evnt.Bind(e);
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
#endif


/*
#if !ENABLE_INPUT_SYSTEM
public class CharacterInputAutomate : CharacterInputLegacy { }
#else
using UnityEngine.InputSystem;
public class CharacterInputAutomate : MonoBehaviour {

    [Tooltip("Where the player should get input from. If null (expected), will generate PlayerInput script here.")]
    public PlayerInput playerInput;
    [Tooltip("The Character's Controls")]
    public InputActionAsset inputActionAsset;
    CharacterMove cm;
    void Start() {
        cm = GetComponent<CharacterMove>();
        RequiredMoves(playerInput, true, true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    struct Inpt {
        public string name, type;
        public EventBind evnt;
        public System.Action<InputAction.CallbackContext> startAct, cancelAct;
        public string[] possibleBindings;
        public Inpt(string n, string t, EventBind e,
            System.Action<InputAction.CallbackContext> sAct, System.Action<InputAction.CallbackContext> cAct, string[] b) {
            name = n;type = t;evnt = e;startAct = sAct;cancelAct = cAct;possibleBindings = b;
        }
        public void SetupAction(InputAction ia, PlayerInput playerInput) {
            if (playerInput == null) {
                if (ia == null) { return; }
                if (startAct != null) {
                    ia.started += startAct;
                }
                if (cancelAct != null) {
                    ia.canceled += cancelAct;
                }
            } else {
                if (playerInput.actions != null) {
                    Debug.Log("!!!!! player assign " + name + " " + playerInput.actionEvents.Count);
                    string n = "/" + name;
                    foreach (var e in playerInput.actionEvents) {
                        Debug.Log("~~~~ " + e.actionName + " vs " + name);
                        if (e.actionName.Contains(n)) {
                            Debug.Log("binding " + evnt.target + "." + evnt.methodName + " to " + e);
                            evnt.Bind(e);
                            break;
                        }
                    }
                }
            }
        }
    }
    void SetMove(InputAction.CallbackContext context) {
        Debug.Log("    "+context);
        cm.SetMove(context);
    }
    public bool RequiredMoves(PlayerInput playerInput, bool alsoAssign, bool generateIfMissing) {
        InputActionAsset iaa;
        if (playerInput.actions == null) {
            Debug.Log("creating new InputActionAsset");
            iaa = InputActionAsset.CreateInstance<InputActionAsset>();
            //playerInput.actions = iaa;
            playerInput.notificationBehavior = PlayerNotifications.InvokeUnityEvents;
        } else {
            iaa = playerInput.actions;
        }
        Inpt[] inputs = new Inpt[] {
            new Inpt("Move", "Vector2", new EventBind(cm, nameof(cm.SetMove)), cm.SetMove, cm.SetMove,new string[]{ "<Gamepad>/leftStick", "<XRController>/{Primary2DAxis}", "<Joystick>/stick" }),//cm.SetMove, cm.SetMove),
  //          new Inpt("Jump", "", new EventBind(cm, nameof(cm.SetJump)), cm.SetJump, cm.SetJump)
        };
        Debug.Log(inputs.Length);
        InputActionMap generated = null;
        // generate actions in action map
        for (int i = 0; i < inputs.Length; ++i) {
            Inpt inp = inputs[i];
            InputAction ia = FindAction(iaa, inp.name, inp.type);
            if (ia == null) {
                Debug.LogWarning("missing " + inp.name);
                if (generateIfMissing) {
                    if (generated == null) {
                        generated = iaa.AddActionMap("generated");
                        Debug.Log("generated " + generated.name);
                    }
                    ia = generated.AddAction(inp.name, string.IsNullOrEmpty(inp.type) ? InputActionType.Button : InputActionType.Value, inp.type);
                    if (inp.possibleBindings != null) {
                        for (int b = 0; b < inp.possibleBindings.Length; ++b) {
                            Debug.Log("binding " + inp.name + " " + inp.possibleBindings[b]);
                            ia.AddBinding(inp.possibleBindings[b]);
                        }
                    }
                }
            }
        }
        //if (generateIfMissing && alsoAssign && iaa != playerInput.actions) {
            playerInput.actions = iaa;
        //}
        Debug.Log("####" + playerInput.actionEvents.Count);
        // bind actions in PlayerInput
        for (int i = 0; i < inputs.Length; ++i) {
            Inpt inp = inputs[i];
            InputAction ia = FindAction(iaa, inp.name, inp.type);
            if (ia != null) {
                if (alsoAssign) {
                    inp.SetupAction(ia, playerInput);
                }
            }
        }
        //GenerateAndAssignActions(playerInput, inputs, ref generated, alsoAssign, generateIfMissing, iaa);
        //if (generateIfMissing && alsoAssign && iaa != playerInput.actions) {
        //    playerInput.actions = iaa;
        //    GenerateAndAssignActions(playerInput, inputs, ref generated, alsoAssign, generateIfMissing, iaa);
        //}

        return true;
    }

    bool GenerateAndAssignActions(PlayerInput playerInput, Inpt[] inputs, ref InputActionMap generated, bool alsoAssign, bool generateIfMissing, InputActionAsset iaa) {
        for (int i = 0; i < inputs.Length; ++i) {
            Inpt inp = inputs[i];
            InputAction ia = FindAction(iaa, inp.name, inp.type);
            if (ia != null) {
                if (alsoAssign) {
                    inp.SetupAction(ia, playerInput);
                }
            } else {
                Debug.LogWarning("missing " + inp.name);
                if (generateIfMissing) {
                    if (generated == null) {
                        generated = iaa.AddActionMap("generated");
                        Debug.Log("generated " + generated.name);
                    }
                    ia = generated.AddAction(inp.name, string.IsNullOrEmpty(inp.type) ? InputActionType.Button : InputActionType.Value, inp.type);
                    if (inp.possibleBindings != null) {
                        for (int b = 0; b < inp.possibleBindings.Length; ++b) {
                            Debug.Log("binding " + inp.name + " " + inp.possibleBindings[b]);
                            ia.AddBinding(inp.possibleBindings[b]);
                        }
                    }
                    if (alsoAssign) {
                        inp.SetupAction(ia, playerInput);
                    }
                } else {
                    return false;
                }
            }
        }
        return true;
    }


    public static InputAction FindAction(InputActionAsset actionAsset, string actionName, string actionInputType) {
        foreach(var actionMap in actionAsset.actionMaps) {
            foreach(var action in actionMap.actions) {
                if(action.name == actionName) {
                    if (action.expectedControlType != actionInputType) {
                        Debug.Log("found " + actionName + ", but Input type is " + action.expectedControlType + ", not " + actionInputType);
                    } else {
                        return action;
                    }
                }
            }
        }
        return null;
    }
}
#endif
*/