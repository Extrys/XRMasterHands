using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Sample_HandJoystick : MonoBehaviour
{
    public InputAction testMovement;
    public InputAction testRot;
	private void Awake()
	{
		testMovement.Enable();
		testRot.Enable();
	}

	void Update()
	{
		transform.position += Quaternion.AngleAxis(transform.localEulerAngles.y, Vector3.up) * (new Vector3(testMovement.ReadValue<Vector2>().x, 0, testMovement.ReadValue<Vector2>().y) * Time.deltaTime);
		transform.rotation *= (Quaternion.AngleAxis(testRot.ReadValue<Vector2>().x * 90 * Time.deltaTime, Vector3.up));
	}
}
