using NonStandard.Character;
using UnityEngine;

public class CharacterInputLegacy : MonoBehaviour {
    CharacterMove cm;
    void Start() {
        cm = GetComponent<CharacterMove>();
        if (cm == null) { enabled = false; Debug.LogWarning("Missing "+nameof(CharacterMove)+" on "+name); }
    }
    void Update() {
        Update(cm);
    }

    public static void Update(CharacterMove cm) {
        Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        bool jump = Input.GetButton("Jump");
        cm.MoveInput = input;
        cm.JumpInput = jump ? 1 : 0;
    }
}
