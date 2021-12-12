using NonStandard.Character;
using NonStandard.Utility.UnityEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterInputAutomate : MonoBehaviour {
    public InputActionAsset inputActionAsset;
    public PlayerInput playerInput;
    CharacterMove cm;
    void Start() {
        cm = GetComponent<CharacterMove>();
        RequiredMoves(inputActionAsset, true, true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    struct Inpt {
        public string name, type;
        public EventBind evnt;
        public System.Action<InputAction.CallbackContext> startAct, cancelAct;
        public Inpt(string n, string t, EventBind e,
            System.Action<InputAction.CallbackContext> sAct, System.Action<InputAction.CallbackContext> cAct) {
            name = n;type = t;evnt = e;startAct = sAct;cancelAct = cAct;
        }
        public void SetupAction(InputAction ia, PlayerInput playerInput) {
            if (playerInput == null) {
                if (startAct != null) {
                    ia.started += startAct;
                }
                if (cancelAct != null) {
                    ia.canceled += cancelAct;
                }
            } else {
                Debug.Log("player assign " + name);
                string n = "/" + name;
                foreach (var e in playerInput.actionEvents) {
                    Debug.Log(e.actionName+" vs "+name);
                    if (e.actionName.Contains(n)) {
                        Debug.Log("binding "+evnt.target+"."+evnt.methodName+" to " + e);
                        evnt.Bind(e);
                        break;
                    }
                }
            }
        }
    }
    void SetMove(InputAction.CallbackContext context) {
        Debug.Log("    "+context);
        cm.SetMove(context);
    }
    public bool RequiredMoves(InputActionAsset actionAsset, bool alsoAssign, bool generateIfMissing) {
        InputAction ia;
        Inpt[] inputs = new Inpt[] {
            new Inpt("Move", "Vector2", new EventBind(cm, nameof(cm.SetMove)), cm.SetMove, cm.SetMove),//cm.SetMove, cm.SetMove),
  //          new Inpt("Jump", "", new EventBind(cm, nameof(cm.SetJump)), cm.SetJump, cm.SetJump)
        };
        Debug.Log(inputs.Length);
        InputActionMap generated = null;
        for(int i = 0; i < inputs.Length; ++i) {
            Inpt inp = inputs[i];
            ia = FindAction(actionAsset, inp.name, inp.type);
            if (ia != null) {
                if (alsoAssign) {
                    inp.SetupAction(ia, playerInput);
                }
            } else {
                Debug.LogWarning("missing "+inp.name);
                if (generateIfMissing) {
                    if (generated == null) {
                        generated = actionAsset.AddActionMap("generated");
                    }
                    ia = generated.AddAction(inp.name, string.IsNullOrEmpty(inp.type) ? InputActionType.Button : InputActionType.Value, inp.type);
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
