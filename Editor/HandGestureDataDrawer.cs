using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(HandGestureData))]
public class HandGestureDataDrawer : PropertyDrawer
{
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		EditorGUI.BeginProperty(position, label, property);
		EditorGUI.BeginChangeCheck();

		Rect left = new Rect(position.x, position.y, position.width / 2, position.height);
		Rect right = new Rect(position.x+ 20 + position.width / 2, position.y, (position.width / 2) - 60, position.height);
		float lineHeigh = EditorGUIUtility.singleLineHeight;

		Rect poseRect = new Rect(left.x, left.y/*(left.height/2f) - (lineHeigh/2)*/, left.width, lineHeigh);
		SerializedProperty handPoseProp = property.FindPropertyRelative(nameof(HandGestureData.handPose));

		if (handPoseProp.objectReferenceValue == null)
			GUI.backgroundColor = Color.red;
		EditorGUI.ObjectField(poseRect, handPoseProp, GUIContent.none);
		GUI.backgroundColor = Color.white;

		property.isExpanded = EditorGUI.Foldout(new Rect(right.x, right.y, right.width, lineHeigh), property.isExpanded, "Extra", true);
		if (property.isExpanded)
		{
			EditorGUI.indentLevel += 1;

			Rect rightL = new Rect(right.x, right.y + EditorGUIUtility.singleLineHeight, right.width / 2, lineHeigh);
			Rect rightR = new Rect(right.x + right.width / 2, right.y + EditorGUIUtility.singleLineHeight, right.width / 2, lineHeigh);
			
			EditorGUI.LabelField(rightL, "Minimum Hold Time");
			EditorGUI.PropertyField(rightR, property.FindPropertyRelative(nameof(HandGestureData.minimumHoldTime)),GUIContent.none);

			EditorGUI.LabelField(new Rect(rightL.x, rightL.y + EditorGUIUtility.singleLineHeight, rightL.width, lineHeigh), "Detection Interval");
			EditorGUI.PropertyField(new Rect(rightR.x, rightR.y + EditorGUIUtility.singleLineHeight, rightR.width, lineHeigh), property.FindPropertyRelative(nameof(HandGestureData.detectionInterval)),GUIContent.none);
			EditorGUI.indentLevel -= 1;
		}

		if (EditorGUI.EndChangeCheck())
			property.serializedObject.ApplyModifiedProperties();

		EditorGUI.EndProperty();
	}

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		// Ajustamos la altura en base a si el foldout está abierto o no.
		if (property.isExpanded)
		{
			// Altura para 3 líneas si el foldout está abierto.
			return EditorGUIUtility.singleLineHeight * 3.2f; // Ajusta este valor según sea necesario.
		}
		// Altura para 1 línea si el foldout está cerrado.
		return EditorGUIUtility.singleLineHeight;
	}
}
