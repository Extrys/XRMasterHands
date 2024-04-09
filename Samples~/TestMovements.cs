using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TestMovements : MonoBehaviour
{
    public InputAction testMovement;
    public InputAction testRot;
	private void Awake() => testMovement.Enable();
	void Update() => transform.position += new Vector3(testMovement.ReadValue<Vector2>().x, 0, testMovement.ReadValue<Vector2>().y) * Time.deltaTime;
}
