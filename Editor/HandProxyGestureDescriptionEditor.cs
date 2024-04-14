using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR.Hands.Gestures;

[CustomEditor(typeof(HandProxyGestureDescription))]
public class HandProxyGestureDescriptionEditor : Editor
{
	HandGestureData[] gestureDatas;
	SerializedProperty gestureDatasProp;
	XRHandPose tempPose;
	string messageText;
	MessageType messageType;
	float lastTimeSinceAppear;
	float duration = 5f;
	void OnEnable()
	{
		gestureDatasProp = serializedObject.FindProperty(nameof(HandProxyGestureDescription.gestureDatas));
	}

	public override void OnInspectorGUI()
	{
		gestureDatas = new HandGestureData[gestureDatasProp.arraySize];
		serializedObject.Update();

		bool refreshProxyClassButtonPressed = GUILayout.Button("Refresh Proxy Device", new GUILayoutOption[] { GUILayout.Height(30) });
		bool shouldRefreshProxyClass = false;
		if (refreshProxyClassButtonPressed)
		{
			bool hasErrors = ((HandProxyGestureDescription)serializedObject.targetObject).CheckForErrors(out messageText);
			if (hasErrors)
				messageType = MessageType.Error;
			else
			{
				messageType = MessageType.Info;
				messageText = "No errors found, invoking class regeneration";
				shouldRefreshProxyClass = true;
			}
			lastTimeSinceAppear = DateTime.Now.Second;
		}
		if (shouldRefreshProxyClass)
		{
			HandProxyDevice_CodeUpdater.RegenerateClass(((HandProxyGestureDescription)serializedObject.targetObject));
			messageType = MessageType.Info;
			messageText = "Regeneration Complete";
		}

		if (DateTime.Now.Second - lastTimeSinceAppear < duration)
		{
			EditorGUILayout.HelpBox(messageText, messageType);
		}

		GUILayout.Space(20);
		var serializedUseOptimizedControlsElement = serializedObject.FindProperty(nameof(HandProxyGestureDescription.useOptimizedControls));
		EditorGUILayout.PropertyField(serializedUseOptimizedControlsElement, true);
		GUILayout.Space(20);
		EditorGUILayout.LabelField("Joystick Pose (Optional)");
		var serializedJoystickStarterElement = serializedObject.FindProperty(nameof(HandProxyGestureDescription.joystickStarter));
		EditorGUILayout.PropertyField(serializedJoystickStarterElement, true);
		if (serializedJoystickStarterElement.FindPropertyRelative(nameof(HandGestureData.handPose)).objectReferenceValue != null)
		{
			var serializedJoystickSimulatorElement = serializedObject.FindProperty(nameof(HandProxyGestureDescription.scriptableSimulator));
			if (serializedJoystickSimulatorElement.objectReferenceValue == null)
				GUI.backgroundColor = Color.red;
			EditorGUILayout.PropertyField(serializedJoystickSimulatorElement, true);
			GUI.backgroundColor = Color.white;
		}
		GUILayout.Space(40);


		tempPose = (XRHandPose)EditorGUILayout.ObjectField("Pose to add", tempPose, typeof(XRHandPose), false);

		GUI.enabled = tempPose != null;
		bool pressed = GUILayout.Button("Add");
		GUI.enabled = true;
		GUILayout.Space(20);

		EditorGUILayout.LabelField("Poses");

		for (int i = 0; i < gestureDatas.Length; i++)
		{
			var serializedElement = gestureDatasProp.GetArrayElementAtIndex(i);
			EditorGUILayout.PropertyField(serializedElement, true);
			Rect lastRect = GUILayoutUtility.GetLastRect();
			bool remove = GUI.Button(new Rect(lastRect.width - 10, lastRect.y, 20, EditorGUIUtility.singleLineHeight), "x");
			if (remove)
			{
				gestureDatasProp.DeleteArrayElementAtIndex(i);
				serializedObject.ApplyModifiedProperties();
				return;
			}
		}

		if (pressed)
		{
			// checks if the tempPose is null or if the gestureDatas array already contains the tempPose
			if (tempPose == null)
				Debug.LogWarning("No se puede agregar un elemento nulo");
			else if (gestureDatasProp.arraySize > 0)
			{
				for (int i = 0; i < gestureDatasProp.arraySize; i++)
				{
					if (gestureDatasProp.GetArrayElementAtIndex(i).FindPropertyRelative(nameof(HandGestureData.handPose)).objectReferenceValue == tempPose)
					{
						Debug.LogWarning("No se puede agregar un elemento que ya existe");
						tempPose = null;
						return;
					}
				}
			}
			gestureDatasProp.arraySize++;
			gestureDatasProp.GetArrayElementAtIndex(gestureDatasProp.arraySize - 1).FindPropertyRelative(nameof(HandGestureData.handPose)).objectReferenceValue = tempPose;
			serializedObject.ApplyModifiedProperties();
			tempPose = null;
		}
		// Asegúrate de llamar a este método para que los cambios se reflejen en la UI y se guarde el estado modificado
		serializedObject.ApplyModifiedProperties();
	}
}