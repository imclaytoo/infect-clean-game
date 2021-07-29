using UnityEngine;
using System;
using UnityEditor;
using Opertoon.Panoply;

/**
 * InstrumentDrawer
 * Copyright © Erik Loyer
 * erik@opertoon.com
 * Part of the Panoply engine
 */

namespace Opertoon.Panoply {
	[CustomPropertyDrawer (typeof (Instrument))]
	[System.Serializable]
	public class InstrumentDrawer: PropertyDrawer {

		PanoplyScene scene;
		int rowHeight = 16;
		int rowSpacing = 2;
		int toggleWidth = 80;
		int clipWidth = 150;

		public override float GetPropertyHeight( SerializedProperty property, GUIContent label ) {
			if (property.FindPropertyRelative( "name" ).isExpanded) {
				return ( float )( ( 3 * rowHeight ) + (( 3 + 1 ) * rowSpacing ) );
			} else {
				return ( float )( ( 1 * rowHeight ) + (( 1 + 1 ) * rowSpacing ) );
			}
		}
		
		private int GetYForRowNum( int row ) {
			return ( row * rowHeight ) + (( row + 1 ) * rowSpacing );
		}
		
		public override void OnGUI( Rect position, SerializedProperty property, GUIContent label ) {
		 
			EditorGUI.BeginProperty(position, label, property);
			
			if ( scene == null ) {
				var foundPanoplyScenes = UnityEngine.Object.FindObjectsOfType<PanoplyScene> ();
				if (foundPanoplyScenes.Length > 0) {
					scene = foundPanoplyScenes [0];
				}
			}
			
			int rowY = 0;
			
			
			// ROW 0
			rowY = ( int )( position.y + GetYForRowNum( 0 ) );

			SerializedProperty name = property.FindPropertyRelative( "name" );
			name.isExpanded = EditorGUI.Foldout( new Rect(position.x, (float)rowY, position.width, (float)rowHeight), name.isExpanded, name.stringValue);

			if (name.isExpanded) {

				// ROW 1
				rowY = ( int )( position.y + GetYForRowNum( 1 ) );

				// name of the instrument
				Rect nameRect = new Rect( position.x, ( float )rowY, position.width - toggleWidth - clipWidth, ( float )rowHeight );
				property.FindPropertyRelative( "name" ).stringValue = EditorGUI.TextField( nameRect, property.FindPropertyRelative( "name" ).stringValue );

				// audio clip
				Rect clipRect = new Rect( position.width - toggleWidth - clipWidth , ( float )rowY, ( float )clipWidth, ( float )rowHeight );
				property.FindPropertyRelative( "clip" ).objectReferenceValue = EditorGUI.ObjectField( clipRect, property.FindPropertyRelative( "clip" ).objectReferenceValue, typeof( AudioClip ), false );
				
				// does the instrument loop?
				Rect loopRect = new Rect( position.width - toggleWidth, ( float )rowY, ( float )toggleWidth, ( float )rowHeight );
				property.FindPropertyRelative( "doesLoop" ).boolValue = EditorGUI.ToggleLeft( loopRect, " Loop", property.FindPropertyRelative( "doesLoop" ).boolValue );
				
				// ROW 2
				rowY = ( int )( position.y + GetYForRowNum( 2 ) );
				
				// get the current state
				SerializedProperty arrayProp = property.FindPropertyRelative( "states" );
				arrayProp.arraySize = scene.stepCount;
				SerializedProperty stateProp = arrayProp.GetArrayElementAtIndex( Mathf.Min (arrayProp.arraySize - 1, PanoplyCore.targetStep) );

				// current volume of the instrument
				Rect volumeRect = new Rect( position.x, ( float )rowY, position.width, ( float )rowHeight );
				stateProp.FindPropertyRelative( "volume" ).floatValue = EditorGUI.Slider( volumeRect, stateProp.FindPropertyRelative( "volume" ).floatValue, 0.0f, 1.0f );

			}

			EditorGUI.EndProperty();       
		 	        
		}

	}
}
