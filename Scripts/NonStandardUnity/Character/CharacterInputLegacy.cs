using NonStandard.Character;
using UnityEngine;

public class CharacterInputLegacy : MonoBehaviour {
    CharacterMove cm;
    CharacterCamera cc;
    void Start() {
        cm = GetComponent<CharacterMove>();
        cc = CharacterCamera.FindCameraTargettingChildOf(cm.transform);
        if (cm == null) { enabled = false; Debug.LogWarning("Missing "+nameof(CharacterMove)+" on "+name); }
    }
    void Update() {
        UpdateCharacterMove(cm);
        if (cc != null && Input.GetMouseButton(1)) {
            cc.ProcessLookRotation(new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")));
        }
    }
    public static void UpdateCharacterMove(CharacterMove cm) {
        Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        bool jump = Input.GetButton("Jump");
        cm.MoveInput = input;
        cm.JumpInput = jump ? 1 : 0;
    }
}
