using UnityEngine;

public class DamageShoot : MonoBehaviour {
    void Update() {
        transform.position = Input.mousePosition;
    }
}