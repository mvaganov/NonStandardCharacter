using NonStandard.Character;
using NonStandard.Process;
using NonStandard.Utility.UnityEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterInputAutomate : MonoBehaviour {
    public PlayerInput playerInput;
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
