using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class Sample_InputListener : MonoBehaviour
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