using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEditor;
using Opertoon.Panoply;

/**
 * ArtworkEditor
 * Copyright © Erik Loyer
 * erik@opertoon.com
 * Part of the Panoply engine
 */

namespace Opertoon.Panoply {
	[CustomEditor( typeof( Artwork ) )]

	[System.Serializable]
	public class ArtworkEditor: Editor {

		Artwork artwork;		// Source object
		
		SerializedProperty scriptIndexProp;
		SerializedProperty statesProp;
		SerializedProperty stateScriptProp;
		SerializedProperty artworkState;
		SerializedProperty scriptStateIndicesProp;
		SerializedProperty maintainScaleProp;
		SerializedProperty positionTypeProp;
		SerializedProperty panelProp;
		SerializedProperty scaleFactorProp;
		SerializedProperty stateCounterProp;
			
		int currentIndex;
		float editorScrubberScriptIndex;
		List<string> scriptStepOptions;				// List of available options for each script step
		
		StateType stateType;
		
		SerializedProperty artworkProp;
		
		Rect timelineRect;
		GUISkin keyframeSkin;
		GUISkin keyframeSkinLight;
		GUISkin currentKeyframeSkin;
		Color highlightColor = new Color( .45f, .76f, 1, 1 );
		Color highlightColorLight = new Color( .78f, .87f, .94f, 1 );
		Color currentHighlightColor;
		int stepIndexPriorToMouseDown;
		int mouseDownStepIndex;
		int draggedKeyframeIndex;
		int draggedKeyframePosition;
		bool draggedAwayFromInitialStep;
		Rect gridRect;
		Rect frameRect;
		Rect markerRect;

		public void OnEnable() {
			
			var target_cs = ( Artwork )target;
			scriptIndexProp = serializedObject.FindProperty( "scriptIndex" );
			statesProp = serializedObject.FindProperty( "states" );
			stateScriptProp = serializedObject.FindProperty( "stateScript" );
			scriptStateIndicesProp = serializedObject.FindProperty( "scriptStateIndices" );
			maintainScaleProp = serializedObject.FindProperty( "maintainScale" );
			positionTypeProp = serializedObject.FindProperty( "positionType" );
			panelProp = serializedObject.FindProperty( "panel" );
			scaleFactorProp = serializedObject.FindProperty( "scaleFactor" );
			stateCounterProp = serializedObject.FindProperty( "stateCounter" );

			gridRect = new Rect (0, 0, 0, 22);
			frameRect = new Rect (0, 0, 3, 22);
			markerRect = new Rect (0, 0, 11, 11);

			artwork = target_cs ;
			
			if (PanoplyCore.scene != null) {
				PanoplyCore.interpolatedStep = PanoplyCore.targetStep = Mathf.Clamp(PanoplyCore.targetStep, 0, PanoplyCore.scene.stepCount - 1);
			} else {
				PanoplyCore.interpolatedStep = PanoplyCore.targetStep = 0;
			}

			scriptStepOptions = new List<string>();
			UpdateScriptStepOptions();
			
			keyframeSkin = Resources.Load("Keyframe") as GUISkin;
			keyframeSkinLight = Resources.Load("KeyframeLight") as GUISkin;
			if ( EditorGUIUtility.isProSkin ) {
				currentKeyframeSkin = keyframeSkin;
				currentHighlightColor = highlightColor;
			} else {
				currentKeyframeSkin = keyframeSkinLight;
				currentHighlightColor = highlightColorLight;
			}

			mouseDownStepIndex = draggedKeyframePosition = draggedKeyframeIndex = -1;

		}
		
		void UpdateScriptStepOptions() {
			
			int i = 0;
			int n = statesProp.arraySize;
			
			scriptStepOptions.Clear();
			scriptStepOptions.Add(ArtworkState.HoldCommand);
			for( i = 0; i < n; i++ ) {
				scriptStepOptions.Add("(" + ( i + 1 ) + ") " + statesProp.GetArrayElementAtIndex( i ).FindPropertyRelative( "id" ).stringValue);
			}

		}
		
		void UpdateArtworkState( bool getLastKeyIfNull ) {
			int index = 0;
			ArtworkState aws = null;
			serializedObject.ApplyModifiedProperties();
			artworkState = null;
			aws = artwork.CurrentState();
			if ( getLastKeyIfNull && ( aws == null ) ) {
				aws = artwork.GetStateAtScriptIndex( stateScriptProp.arraySize - 1 );
			}
			if ( aws != null ) {
				index = artwork.GetStateIndexForId( aws.id );
				//Debug.Log( 'script: ' + scriptIndexProp.intValue + ' index: ' + index + ' id: ' + aws.id + ' ' + statesProp.arraySize );
				artworkState = statesProp.GetArrayElementAtIndex( index );
				//Debug.Log( artworkState.FindPropertyRelative( 'position' ).vector3Value );
			}	
			/*if ( artworkState != null ) {
				Debug.Log( 'artwork state: ' + artworkState.FindPropertyRelative( 'id' ).stringValue );
			} else {
				Debug.Log( 'null artwork state' );
			}*/
			serializedObject.Update();
		}

		public void LogStates() {
			
			int i = 0;
			int n = 0;
			string content;
			
			n = stateScriptProp.arraySize;
			content = "ARTWORK STATE SCRIPT\n"; 
			for( i = 0; i < n; i++ ) {
				content += i + ": " + stateScriptProp.GetArrayElementAtIndex(i).stringValue + "\n";
			}
			Debug.Log (content);
			
		}

		public void InsertState() {
			SerializedKeyframeTimeline timeline = new SerializedKeyframeTimeline(artwork);
			timeline.InsertState();
			UpdateArtworkState( false );
		}

		private void DeleteCurrentState() {
			SerializedKeyframeTimeline timeline = new SerializedKeyframeTimeline(artwork);
			timeline.DeleteCurrentState();
			UpdateArtworkState( false );
		}
		
		private void DrawKeyframeTimeline() {
			
			if (PanoplyCore.scene != null) {

				GUI.skin = currentKeyframeSkin;
				
				float unitWidth = (timelineRect.width - 5.5f - 5) / (PanoplyCore.scene.stepCount - 1);
				
				int n = Mathf.Min (stateScriptProp.arraySize, PanoplyCore.scene.stepCount);
				int i;
				for (i=0; i<n; i += 5) {
					
					// background grid
					GUI.skin.label = currentKeyframeSkin.horizontalSlider;
					if (i < (n - 1)) {
						gridRect.width = unitWidth * (Math.Min (i + 5, n - 1) - i) + 1;
						gridRect.x = timelineRect.x + (unitWidth * i) + 5.5f;
						gridRect.y = timelineRect.y;
						GUI.Label(gridRect, "" );
					}
					
				}
				
				// current step marker
				GUI.skin.label = currentKeyframeSkin.window;
				frameRect.x = timelineRect.x - 1 + (unitWidth * scriptIndexProp.intValue) + 5.5f;
				frameRect.y = timelineRect.y;
				GUI.Label(frameRect, "" );
				
				for (i=0; i<n; i++) {
					
					if ( PanoplyCore.targetStep == i ) {
						GUI.color = currentHighlightColor;
					}

					// inactive keyframes
					GUI.skin.label = currentKeyframeSkin.textField;
					if ((stateScriptProp.GetArrayElementAtIndex( i ).stringValue != ArtworkState.HoldCommand) && (i != draggedKeyframeIndex)) {
						markerRect.x = timelineRect.x + 1 + (unitWidth * i);
						markerRect.y = timelineRect.y + 5.5f;
						GUI.Label(markerRect, "" );
					}
					
					GUI.color = Color.white;
				}
				
				// active keyframe
				Rect r = new Rect();
				if (draggedKeyframeIndex != -1) {
					r = timelineRect;
					GUI.skin.label = currentKeyframeSkin.textArea;
					markerRect.x = r.x + 1 + (unitWidth * draggedKeyframePosition);
					markerRect.y = r.y + 5.5f;
					GUI.Label(markerRect, "" );
				}
				
				GUI.skin = null;

			}
			
		}
		
		private void SetCurrentStep(int step) {
			editorScrubberScriptIndex = PanoplyCore.interpolatedStep = scriptIndexProp.intValue = PanoplyCore.targetStep = step;
		}
		
		private void KeyframeEditor() {
			
			if (PanoplyCore.scene != null) {

				GUI.skin = currentKeyframeSkin;
				
				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();
				
				EditorGUILayout.Space();
				Rect r = EditorGUILayout.BeginHorizontal();
				r.width -= 117;
				GUILayout.Box("", GUILayout.Height( 22 ), GUILayout.ExpandWidth( true ) );
				
				GUI.skin = null;
				
				timelineRect = r;
				
				bool isOnFirstStep = ( scriptIndexProp.intValue == 0 );
				bool isOnLastStep = ( scriptIndexProp.intValue == (PanoplyCore.scene.stepCount - 1));
				
				GUI.enabled = !isOnFirstStep;
				if ( stateScriptProp.GetArrayElementAtIndex( scriptIndexProp.intValue ).stringValue != ArtworkState.HoldCommand ) {
					GUI.color = new Color( .78f, .87f, .94f, 1 );
				}
				if ( GUILayout.Button( "Key", GUILayout.Width(35), GUILayout.Height(22) )) {
					if ( stateScriptProp.GetArrayElementAtIndex( scriptIndexProp.intValue ).stringValue == ArtworkState.HoldCommand ) {
						SetKeyframeStateType(StateType.Key);
					} else {
						SetKeyframeStateType(StateType.Hold);
					}
				}
				GUI.color = Color.white;
				
				GUI.enabled = !isOnLastStep;
				if ( GUILayout.Button( "Ins", GUILayout.Width(35), GUILayout.Height(22) )) {
					int option = EditorUtility.DisplayDialogComplex("Insert step locally or globally?", "Do you want to insert a step in the artwork's timeline only, or across the entire project?", "Artwork timeline only", "Cancel", "Entire project");
					if (option == 0) {
						InsertState();
					} else if (option == 2) {
						PanoplyCore.InsertStateGlobally();
					}
				}
				
				GUI.enabled = !isOnFirstStep;
				if ( GUILayout.Button( "Del", GUILayout.Width(35), GUILayout.Height(22) ) ) {
					int option = EditorUtility.DisplayDialogComplex("Delete step locally or globally?", "Do you want to delete a step in the artwork's timeline only, or across the entire project?", "Artwork timeline only", "Cancel", "Entire project");
					if (option == 0) {
						DeleteCurrentState();
						//LogStates();
					} else if (option == 2) {
						PanoplyCore.DeleteAllCurrentStatesGlobally();
					}
				}
				GUI.enabled = true;
				
				GUI.skin = currentKeyframeSkin;
				
				EditorGUILayout.EndHorizontal();
				
				float unitWidth = (r.width - 5.5f - 5) / (PanoplyCore.scene.stepCount - 1);
				
				// active keyframe
				if (draggedKeyframeIndex != -1) {
					GUI.skin.label = currentKeyframeSkin.textArea;
					GUI.Label( new Rect( r.x + 1 + (unitWidth * draggedKeyframePosition), r.y + 5.5f, 11, 11 ), "" );
				}
				
				EditorGUILayout.Space();
				
				if ( Event.current.isMouse ) {
					Vector2 pos = Event.current.mousePosition;
					if (r.Contains(pos)) {
						
						// get the step that was clicked
						float adjX = Mathf.Clamp(pos.x - r.x - 5.5f - (unitWidth * .0f), 0, r.width);
						int stepIndex = (int)Mathf.Clamp(Mathf.Round(adjX / unitWidth), 0, PanoplyCore.scene.stepCount - 1);
						
						// keyframe clicks are only registered if they are within the dimensions
						// of the keyframe icon on the timeline
						int keyframeIndex = -1;
						if (Mathf.Abs(adjX - (stepIndex * unitWidth)) <= 11) {
							keyframeIndex = stepIndex;
						}
						
						switch ( Event.current.type ) {
							
						case EventType.MouseDown:
							stepIndexPriorToMouseDown = PanoplyCore.targetStep;
							mouseDownStepIndex = stepIndex;
							if (keyframeIndex > 0) {
								if (stateScriptProp.GetArrayElementAtIndex(keyframeIndex).stringValue != ArtworkState.HoldCommand) {
									draggedKeyframePosition = draggedKeyframeIndex = keyframeIndex;
								}
							}
							draggedAwayFromInitialStep = false;
							break;
							
						case EventType.MouseDrag:
							if (draggedKeyframeIndex != -1) {
								draggedKeyframePosition = stepIndex;
								if (!draggedAwayFromInitialStep && (draggedKeyframePosition != draggedKeyframeIndex)) {
									draggedAwayFromInitialStep = true;
								}
							} else if (mouseDownStepIndex != -1) {
								SetCurrentStep(stepIndex);
								if (!draggedAwayFromInitialStep && (mouseDownStepIndex != stepIndex)) {
									draggedAwayFromInitialStep = true;
								}
							}
							Repaint();
							break;
							
						case EventType.MouseUp:
							if (draggedKeyframeIndex != -1) {
								if (draggedKeyframePosition != draggedKeyframeIndex) {
									scriptStateIndicesProp.GetArrayElementAtIndex(draggedKeyframePosition).intValue = scriptStateIndicesProp.GetArrayElementAtIndex(draggedKeyframeIndex).intValue;
									stateScriptProp.GetArrayElementAtIndex(draggedKeyframePosition).stringValue = scriptStepOptions[ scriptStateIndicesProp.GetArrayElementAtIndex(draggedKeyframeIndex).intValue ];
									scriptStateIndicesProp.GetArrayElementAtIndex(draggedKeyframeIndex).intValue = 0;
									stateScriptProp.GetArrayElementAtIndex(draggedKeyframeIndex).stringValue = ArtworkState.HoldCommand;
									UpdateScriptStepOptions();
								} else {
									SetCurrentStep(stepIndex);
									if (!draggedAwayFromInitialStep && (stepIndexPriorToMouseDown == mouseDownStepIndex) && (mouseDownStepIndex != 0)) {
										SetKeyframeStateType(StateType.Hold);
									}
								}
							} else {
								if ((stepIndexPriorToMouseDown == mouseDownStepIndex) && (mouseDownStepIndex != 0) && !draggedAwayFromInitialStep) {
									SetCurrentStep(stepIndex);
									SetKeyframeStateType(StateType.Key);
								} else {
									SetCurrentStep(stepIndex);
								}
							}
							//LogStates();
							mouseDownStepIndex = draggedKeyframePosition = draggedKeyframeIndex = -1;
							Repaint();
							break;
							
						}
					} else {
						switch ( Event.current.type ) {
						case EventType.MouseUp:
							mouseDownStepIndex = draggedKeyframePosition = draggedKeyframeIndex = -1;
							Repaint();
							break;
						}
					}
				}
				
				GUI.skin = null;

			}
		}
		
		private void AddState() {
			SerializedProperty stateProp;
			statesProp.arraySize++;
			do {
				stateCounterProp.intValue++;
			} while ( stateCounterProp.intValue <= 1 );
			stateProp = statesProp.GetArrayElementAtIndex( statesProp.arraySize - 1 );
			stateProp.FindPropertyRelative( "id" ).stringValue = "State " + stateCounterProp.intValue;
			UpdateArtworkState( true );
		}
		
		private void SetKey() {

			AddState();
			SerializedProperty stateProp = statesProp.GetArrayElementAtIndex( statesProp.arraySize - 1 );
			stateProp.FindPropertyRelative( "position" ).vector3Value = artworkState.FindPropertyRelative( "position" ).vector3Value;
			stateProp.FindPropertyRelative( "rotation" ).vector3Value = artworkState.FindPropertyRelative( "rotation" ).vector3Value;
			stateProp.FindPropertyRelative( "scale" ).vector3Value = artworkState.FindPropertyRelative( "scale" ).vector3Value;
			stateProp.FindPropertyRelative( "color" ).colorValue = artworkState.FindPropertyRelative( "color" ).colorValue;

			UpdateScriptStepOptions();
			while ( scriptIndexProp.intValue >= stateScriptProp.arraySize ) {
				stateScriptProp.arraySize++;
				scriptStateIndicesProp.arraySize++;
				scriptStateIndicesProp.GetArrayElementAtIndex( scriptStateIndicesProp.arraySize - 1 ).intValue = 0;
				stateScriptProp.GetArrayElementAtIndex( stateScriptProp.arraySize - 1 ).stringValue = scriptStepOptions[ scriptStateIndicesProp.GetArrayElementAtIndex( scriptStateIndicesProp.arraySize - 1 ).intValue ];
			}
			scriptStateIndicesProp.GetArrayElementAtIndex( scriptIndexProp.intValue ).intValue = scriptStepOptions.Count - 1;
			stateScriptProp.GetArrayElementAtIndex( scriptIndexProp.intValue ).stringValue = scriptStepOptions[ scriptStateIndicesProp.GetArrayElementAtIndex( scriptIndexProp.intValue ).intValue ];
			UpdateArtworkState( false );
		}
		
		private void SetHold() {
			bool okToSetHold = false;
			
			if ( scriptIndexProp.intValue == 0 ) {
				EditorUtility.DisplayDialog( "Can’t delete first key", "The first step must be a key.", "OK" );
			} else {
				okToSetHold = true;
				if ( scriptIndexProp.intValue < stateScriptProp.arraySize ) {
					if ( stateScriptProp.GetArrayElementAtIndex( scriptIndexProp.intValue ).stringValue != ArtworkState.HoldCommand ) {
						SerializedKeyframeTimeline timeline = new SerializedKeyframeTimeline(artwork);
						timeline.DeleteStateAtIndex(scriptStateIndicesProp.GetArrayElementAtIndex( scriptIndexProp.intValue ).intValue - 1); // - 1 because 'hold' is at index 0
						UpdateArtworkState( false );
						scriptStateIndicesProp.GetArrayElementAtIndex( scriptIndexProp.intValue ).intValue = 0;
						stateScriptProp.GetArrayElementAtIndex( scriptIndexProp.intValue ).stringValue = ArtworkState.HoldCommand;
						UpdateScriptStepOptions();
					}
				}
				if ( okToSetHold ) {
					while ( scriptIndexProp.intValue >= stateScriptProp.arraySize ) {
						stateScriptProp.arraySize++;
						scriptStateIndicesProp.arraySize++;
						scriptStateIndicesProp.GetArrayElementAtIndex( scriptStateIndicesProp.arraySize - 1 ).intValue = 0;
						stateScriptProp.GetArrayElementAtIndex( stateScriptProp.arraySize - 1 ).stringValue = ArtworkState.HoldCommand;
					}
					UpdateArtworkState( false );
				}
			}
		}
		
		private void SetEnd() {
			/*if ( scriptIndexProp.intValue == 0 ) {
				EditorUtility.DisplayDialog( 'Can’t delete first key', 'The first step must be a key.', 'OK' );
			} else {
				if ( EditorUtility.DisplayDialog( 'Remove keys and holds?', 'Setting this step to "End" will delete any of its keys and holds from this step forward. Are you sure you want to do this?', 'OK', 'Cancel' ) ) {
					for ( i = ( stateScriptProp.arraySize - 1 ); i >= scriptIndexProp.intValue; i-- ) {
						if ( stateScriptProp.GetArrayElementAtIndex( i ).stringValue != ArtworkState.HoldCommand ) {
							deleteState( component, acriptStateIndicesProp.GetArrayElementAtIndex( i ).intValue - 1 ); // - 1 because 'hold' is at index 0
							updateScriptStepOptions();
						}
						stateScriptProp.DeleteArrayElementAtIndex( i );
					}
					UpdateArtworkState( false );
				}
			}*/
		}
		
		private void SetKeyframeStateType(StateType stateType) {

			switch ( stateType ) {
				
			case StateType.Key:
				SetKey();
				break;
				
			case StateType.Hold:
				SetHold();
				break;
				
			case StateType.End:
				SetEnd();
				break;
				
			}
			
			//LogStates();
			
		}	

		public override void OnInspectorGUI() {
		
			Event e = Event.current;
			
			serializedObject.Update();
			
			EditorGUILayout.Space();
			
			if (PanoplyCore.scene != null) {
				EditorGUI.BeginChangeCheck();
				editorScrubberScriptIndex = EditorGUILayout.Slider( "Global timeline", editorScrubberScriptIndex, 0.0f, ( float )( PanoplyCore.scene.stepCount - 1 ) );
				if ( EditorGUI.EndChangeCheck() ) {
					scriptIndexProp.intValue = ( int )Mathf.Round( editorScrubberScriptIndex );
					if ( e.shift ) {
						PanoplyCore.interpolatedStep = editorScrubberScriptIndex;
					} else {
						PanoplyCore.interpolatedStep = ( float )scriptIndexProp.intValue;
					}
					PanoplyCore.targetStep = scriptIndexProp.intValue;
				} else {
					editorScrubberScriptIndex = PanoplyCore.interpolatedStep;
					scriptIndexProp.intValue = ( int )Mathf.Round( editorScrubberScriptIndex );
				}
			}

			EditorGUILayout.Space();

			EditorGUILayout.PropertyField( positionTypeProp );
			if ( positionTypeProp.enumValueIndex == ( int ) ArtworkPositionType.Panel ) {
				if ( panelProp.objectReferenceValue == null ) {
					EditorGUILayout.HelpBox( "The panel position type requires a base panel. Please specify one below.", MessageType.Warning );
				}
				panelProp.objectReferenceValue = EditorGUILayout.ObjectField( "Base panel", panelProp.objectReferenceValue, typeof( Panel ), true );
			}

			maintainScaleProp.boolValue = EditorGUILayout.Toggle( "Maintain scale", maintainScaleProp.boolValue );
			
			if ( maintainScaleProp.boolValue && ( panelProp.objectReferenceValue == null )) {
				EditorGUILayout.HelpBox( "The maintain scale feature requires a target panel. Please specify one below.", MessageType.Warning );
			}
			
			if ( maintainScaleProp.boolValue ) {
				panelProp.objectReferenceValue = EditorGUILayout.ObjectField( "Target panel", panelProp.objectReferenceValue, typeof( Panel ), true );
				scaleFactorProp.floatValue = EditorGUILayout.FloatField( "Scale factor", scaleFactorProp.floatValue );
			}


			EditorGUILayout.Space();
			
			if ( statesProp.arraySize == 0 ) {
				statesProp.arraySize++;
				stateCounterProp.intValue++;
				artworkState = statesProp.GetArrayElementAtIndex( 0 );
				artworkState.FindPropertyRelative( "id" ).stringValue = "State " + stateCounterProp.intValue;
				artworkState.FindPropertyRelative( "scale" ).vector3Value = artwork.transform.localScale;
				Vector3 modifiedPosition = artwork.transform.position;
				if (modifiedPosition == Vector3.zero) {
					modifiedPosition.z += 10;
				}
				artworkState.FindPropertyRelative( "position" ).vector3Value = modifiedPosition;
				artwork.transform.position = Vector3.zero;
				artworkState.FindPropertyRelative( "rotation" ).vector3Value = artwork.transform.rotation.eulerAngles;
				artworkState.FindPropertyRelative( "color" ).colorValue = Color.white;
				UpdateScriptStepOptions();
			}
			
			if ( stateScriptProp.arraySize == 0 ) {
				stateScriptProp.arraySize++;
				scriptStateIndicesProp.arraySize++;
				scriptStateIndicesProp.GetArrayElementAtIndex( 0 ).intValue = 1;
				stateScriptProp.GetArrayElementAtIndex( 0 ).stringValue = scriptStepOptions[ scriptStateIndicesProp.GetArrayElementAtIndex( 0 ).intValue ];
			}

			// fill empty steps with Hold commands
			if (PanoplyCore.scene != null) {
				while ( stateScriptProp.arraySize < PanoplyCore.scene.stepCount ) {
					stateScriptProp.arraySize++;
					scriptStateIndicesProp.arraySize++;
					scriptStateIndicesProp.GetArrayElementAtIndex( stateScriptProp.arraySize - 1 ).intValue = stateScriptProp.arraySize;
					stateScriptProp.GetArrayElementAtIndex( stateScriptProp.arraySize - 1 ).stringValue = ArtworkState.HoldCommand;
				}
			}
					
			UpdateArtworkState( false );

			KeyframeEditor();

			EditorGUILayout.Space();
		
			if ( artworkState != null ) {
			
				EditorGUI.BeginChangeCheck();
				artworkState.FindPropertyRelative( "position" ).vector3Value = EditorGUILayout.Vector3Field( "Position", artworkState.FindPropertyRelative( "position" ).vector3Value );
				artworkState.FindPropertyRelative( "rotation" ).vector3Value = EditorGUILayout.Vector3Field( "Rotation", artworkState.FindPropertyRelative( "rotation" ).vector3Value );
				artworkState.FindPropertyRelative( "scale" ).vector3Value = EditorGUILayout.Vector3Field( "Scale", artworkState.FindPropertyRelative( "scale" ).vector3Value );
				artworkState.FindPropertyRelative( "color" ).colorValue = EditorGUILayout.ColorField( "Color", artworkState.FindPropertyRelative( "color" ).colorValue );
			}

			DrawKeyframeTimeline();
			
			serializedObject.ApplyModifiedProperties();

			if ( EditorGUI.EndChangeCheck() ) {
				if (maintainScaleProp.boolValue) {
					artwork.MaintainScaleForZ();
				}
				artwork.Update();
			}

		}
		
	}
}
