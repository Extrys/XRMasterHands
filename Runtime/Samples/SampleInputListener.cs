using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class SampleInputListener : MonoBehaviour
{
	[SerializeField] InputAction testingAction;
	[SerializeField] UnityEvent onTestActionPerformed;
	void Start()
	{
		testingAction.Enable();
		testingAction.performed += OnTestActionPerformed;
	}

	void OnDestroy() => testingAction.performed -= OnTestActionPerformed;
	void OnTestActionPerformed(InputAction.CallbackContext context) => onTestActionPerformed.Invoke();
}