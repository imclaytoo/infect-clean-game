using UnityEngine;
using System;
using UnityEditor;
using Opertoon.Panoply;

/**
 * GlobalTimelineDrawer
 * Copyright © Erik Loyer
 * erik@opertoon.com
 * Part of the Panoply engine
 */

namespace Opertoon.Panoply {
	[CustomPropertyDrawer (typeof (GlobalTimeline))]
	[System.Serializable]
	public class GlobalTimelineDrawer: PropertyDrawer {

		public override float GetPropertyHeight( SerializedProperty property,
	                                                      GUIContent label ) {
			return 20.0f;
		}

		public override void OnGUI( Rect position, SerializedProperty property, GUIContent label ) {
		
			int intStep = 0;
			Event e = Event.current;
		 
			EditorGUI.BeginProperty(position, label, property);
			
			SerializedProperty stepProp = property.FindPropertyRelative( "step" );

			if (PanoplyCore.scene != null) {
				EditorGUI.BeginChangeCheck();
				stepProp.floatValue = EditorGUI.Slider( position, "Global timeline", stepProp.floatValue, 0.0f, ( float )( PanoplyCore.scene.stepCount - 1 ) );
				if ( EditorGUI.EndChangeCheck() ) {
					intStep = ( int )Mathf.Round( stepProp.floatValue );
					if ( !e.shift ) {
						stepProp.floatValue = ( float )intStep;
					}
					PanoplyCore.interpolatedStep = stepProp.floatValue;
					PanoplyCore.targetStep = intStep;
				}
			}

			EditorGUI.EndProperty();       
		 	        
		}

	}
}