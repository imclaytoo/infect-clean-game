using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Opertoon.Panoply;

/**
 * ThumbnailIndexDrawer
 * Copyright © Erik Loyer
 * erik@opertoon.com
 * Part of the Panoply engine
 */

namespace Opertoon.Panoply {
	[CustomPropertyDrawer (typeof (ThumbnailMenuItem))]
	[System.Serializable]
	public class ThumbnailMenuItemDrawer : PropertyDrawer {

		PanoplyScene scene;
		int rowHeight = 16;
		int rowSpacing = 2;

		public override float GetPropertyHeight( SerializedProperty property, GUIContent label ) {
			return ( float )( ( 1 * rowHeight ) + (( 1 + 1 ) * rowSpacing ) );
		}

		public override void OnGUI( Rect position, SerializedProperty property, GUIContent label ) {

			EditorGUI.BeginProperty(position, label, property);

			if ( scene == null ) {
				var foundPanoplyScenes = UnityEngine.Object.FindObjectsOfType<PanoplyScene> ();
				if (foundPanoplyScenes.Length > 0) {
					scene = foundPanoplyScenes [0];
				}
			}

			Rect fieldRect = new Rect(position.x, position.y, position.width * .33f, EditorGUIUtility.singleLineHeight);
			property.FindPropertyRelative("thumbnail").objectReferenceValue = EditorGUI.ObjectField(fieldRect, property.FindPropertyRelative("thumbnail").objectReferenceValue, typeof(Texture2D), false);
			fieldRect.x += position.width * .3f;
			EditorGUIUtility.labelWidth = 100;
			property.FindPropertyRelative("step").intValue = EditorGUI.IntField(fieldRect, "Goes to step", property.FindPropertyRelative("step").intValue);
			fieldRect.x += position.width * .33f;
			fieldRect.width = position.width * .2f;
			EditorGUIUtility.labelWidth = 80;
			property.FindPropertyRelative("instantTransition").boolValue = EditorGUI.ToggleLeft(fieldRect, "Instantly", property.FindPropertyRelative("instantTransition").boolValue);
			fieldRect.x += fieldRect.width + 10;
			fieldRect.width = position.width * .16f - 10;
			if (GUI.Button(fieldRect, "Current")) {
				property.FindPropertyRelative("step").intValue = PanoplyCore.targetStep;
			}

			EditorGUI.EndProperty();       

		}

	}
}
