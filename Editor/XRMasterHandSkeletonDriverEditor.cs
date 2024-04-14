using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomEditor(typeof(XRMasterHandSkeletonDriver), true), CanEditMultipleObjects]
public class XRMasterHandSkeletonDriverEditor : Editor
{
	class JointTransformReferenceReorderableList : ReorderableList
	{
		public JointTransformReferenceReorderableList(SerializedObject serializedObject, SerializedProperty serializedProperty) : base(serializedObject, serializedProperty, true, true, true, true)
		{
			drawElementCallback = OnDrawListElement;
			drawHeaderCallback = OnDrawListHeader;
			elementHeight = EditorGUIUtility.singleLineHeight;
		}

		void OnDrawListHeader(Rect rect) => GUI.Label(rect, Contents.jointTransformReferences);

		void OnDrawListElement(Rect rect, int elementIndex, bool isActive, bool isFocused)
		{
			var element = serializedProperty.GetArrayElementAtIndex(elementIndex);
			rect.yMin += 2;
			var width = rect.width;
			var middle = rect.xMin + width * 0.5f;
			const float padding = 8f;
			rect.xMax = middle - padding;
			EditorGUI.PropertyField(rect, element.FindPropertyRelative("m_XRHandJointID"), GUIContent.none);
			rect.xMax = rect.xMin + width;
			rect.xMin = middle;
			EditorGUI.PropertyField(rect, element.FindPropertyRelative("m_JointTransform"), GUIContent.none);
		}
	}

	const string k_JointsExpandedStateKey = "XRHands." + nameof(XRMasterHandSkeletonDriverEditor) + ".JointsExpanded";
	const string k_FindJointsFromParentUndoMessage = "Find Joints from Parent";

	const string k_ScriptPropertyName = "m_Script";
	const string k_HandednesskingEventsPropertyName = "handedness";
	const string k_handRendererPropertyName = "handRenderer";
	const string k_HideMeshWhenTrackingIsLostName = "hideMeshWhenTrackingIsLost";
	const string k_JointTransformReferencesPropertyName = "m_JointTransformReferences";
	const string k_RootTransformPropertyName = "m_RootTransform";
	static readonly string[] k_KnownProperties =
	{
		k_ScriptPropertyName,
		k_HandednesskingEventsPropertyName,
		k_HideMeshWhenTrackingIsLostName,
		k_handRendererPropertyName,
		k_JointTransformReferencesPropertyName,
		k_RootTransformPropertyName,
	  };

	SerializedProperty m_handedness;
	SerializedProperty m_renderer;
	SerializedProperty m_autoHideRender;
	SerializedProperty m_JointTransformReferences;
	SerializedProperty m_RootTransform;

	JointTransformReferenceReorderableList m_JointTransformReferencesList;
	List<string> m_MissingJointNames = new List<string>();
	bool m_DrawHelpBoxForFindingJoints;
	string m_ErrorFindingJointsMessage;
	bool m_JointsExpanded;

	static class Contents
	{
		public static readonly GUIContent handedness = EditorGUIUtility.TrTextContent("Handedness");
		public static readonly GUIContent renderer = EditorGUIUtility.TrTextContent("Renderer (Optional)");
		public static readonly GUIContent autoHideRender = EditorGUIUtility.TrTextContent("Hide on tracking Lost");
		public static readonly GUIContent jointTransformReferences = EditorGUIUtility.TrTextContent("Joint Transform References");
		public static readonly GUIContent parentTransform = EditorGUIUtility.TrTextContent("Root Transform");
		public static readonly GUIContent findJoints = EditorGUIUtility.TrTextContent("Find Joints", "Search the root transform's children for transforms that start or end with a joint name.");
		public static readonly GUIContent clearJoints = EditorGUIUtility.TrTextContent("Clear");
	}

	void OnEnable()
	{
		m_handedness = serializedObject.FindProperty(k_HandednesskingEventsPropertyName);
		m_autoHideRender = serializedObject.FindProperty(k_HideMeshWhenTrackingIsLostName);
		m_renderer = serializedObject.FindProperty(k_handRendererPropertyName);
		m_JointTransformReferences = serializedObject.FindProperty(k_JointTransformReferencesPropertyName);
		m_RootTransform = serializedObject.FindProperty(k_RootTransformPropertyName);
		m_JointTransformReferencesList = new JointTransformReferenceReorderableList(serializedObject, m_JointTransformReferences);
		m_JointsExpanded = SessionState.GetBool(k_JointsExpandedStateKey, false);
	}

	void OnDisable() => SessionState.SetBool(k_JointsExpandedStateKey, m_JointsExpanded);

	void OnFindJointsButtonPressed()
	{
		foreach (var targetObject in targets)
		{
			var skeletonDriver = (XRMasterHandSkeletonDriver)targetObject;
			Undo.RecordObject(skeletonDriver, k_FindJointsFromParentUndoMessage);
			skeletonDriver.FindJointsFromRoot(m_MissingJointNames);
			EditorUtility.SetDirty(skeletonDriver);
			if (m_MissingJointNames.Count > 0)
			{
				m_DrawHelpBoxForFindingJoints = true;
				m_ErrorFindingJointsMessage = $"Could not find joints: {string.Join(", ", m_MissingJointNames)}";
			}
			else
			{
				m_DrawHelpBoxForFindingJoints = false;
			}
		}

		serializedObject.SetIsDifferentCacheDirty();
	}

	public override void OnInspectorGUI()
	{
		serializedObject.Update();

		DrawInspector();
		DrawPropertiesExcluding(serializedObject, k_KnownProperties);

		serializedObject.ApplyModifiedProperties();
	}

	void DrawInspector()
	{
		EditorGUILayout.PropertyField(m_handedness, Contents.handedness, true);
		EditorGUILayout.PropertyField(m_renderer, Contents.renderer, true);
		if (m_renderer.objectReferenceValue != null)
			EditorGUILayout.PropertyField(m_autoHideRender, Contents.autoHideRender, true);

		EditorGUILayout.Space();

		m_JointsExpanded = EditorGUILayout.Foldout(m_JointsExpanded, Contents.jointTransformReferences, true);
		if (m_JointsExpanded)
		{
			using (new EditorGUI.IndentLevelScope())
			{
				DrawJointTransformsFoldout();
			}
		}

		EditorGUILayout.Space();
	}

	void DrawJointTransformsFoldout()
	{
		EditorGUILayout.PropertyField(m_RootTransform, Contents.parentTransform);
		using (new EditorGUILayout.HorizontalScope())
		{
			using (new EditorGUI.DisabledScope(m_RootTransform.objectReferenceValue == null))
			{
				if (GUILayout.Button(Contents.findJoints))
					OnFindJointsButtonPressed();
			}

			using (new EditorGUI.DisabledScope(m_JointTransformReferences.arraySize == 0))
			{
				if (GUILayout.Button(Contents.clearJoints))
					m_JointTransformReferences.ClearArray();
			}
		}

		if (m_DrawHelpBoxForFindingJoints)
			EditorGUILayout.HelpBox(m_ErrorFindingJointsMessage, MessageType.Warning, true);

		m_JointTransformReferencesList.DoLayoutList();
	}
}