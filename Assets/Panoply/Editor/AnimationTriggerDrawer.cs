using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Opertoon.Panoply;

/**
 * AnimationTriggerDrawer
 * Copyright © Erik Loyer
 * erik@opertoon.com
 * Part of the Panoply engine
 */

public class AnimationTriggerDrawer : MonoBehaviour
{
	[CustomPropertyDrawer (typeof (AnimationTrigger))]
	[System.Serializable]
	public class ThumbnailMenuItemDrawer : PropertyDrawer
	{

		int rowHeight = 16;
		int rowSpacing = 2;

		public override float GetPropertyHeight (SerializedProperty property, GUIContent label)
		{
			return (float)((1 * rowHeight) + ((1 + 1) * rowSpacing));
		}

		public override void OnGUI (Rect position, SerializedProperty property, GUIContent label)
		{

			EditorGUI.BeginProperty (position, label, property);

			Rect fieldRect = new Rect (position.x, position.y, position.width * .25f, EditorGUIUtility.singleLineHeight);
			EditorGUIUtility.labelWidth = 60;
			property.FindPropertyRelative ("target").objectReferenceValue = EditorGUI.ObjectField (fieldRect, property.FindPropertyRelative ("target").objectReferenceValue, typeof (GameObject), true);

			fieldRect.x += fieldRect.width - 5;
			fieldRect.width = position.width * .3f;
			EditorGUIUtility.labelWidth = 65;
			property.FindPropertyRelative ("triggerName").stringValue = EditorGUI.TextField (fieldRect, "Trigger", property.FindPropertyRelative ("triggerName").stringValue);

			fieldRect.x += fieldRect.width - 5;
			fieldRect.width = position.width * .2f;
			EditorGUIUtility.labelWidth = 50;
			property.FindPropertyRelative ("timelineStep").intValue = EditorGUI.IntField (fieldRect, "Step", property.FindPropertyRelative ("timelineStep").intValue);

			fieldRect.x += fieldRect.width;
			fieldRect.width = position.width * .25f;
			int selectedValue = property.FindPropertyRelative ("direction").intValue;
			string [] enumNames = System.Enum.GetNames (typeof (PanoplyStepDirection));
			int enumValue = EditorGUI.Popup (fieldRect, selectedValue, enumNames);
			property.FindPropertyRelative ("direction").intValue = enumValue;

			EditorGUI.EndProperty ();

		}

	}
}