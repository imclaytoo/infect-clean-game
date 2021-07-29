using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEditor;
using Opertoon.Panoply;

/**
 * AudioTrackEditor
 * Copyright © Erik Loyer
 * erik@opertoon.com
 * Part of the Panoply engine
 */

namespace Opertoon.Panoply {
	[CustomEditor( typeof( AudioTrack ) )]
	
	[System.Serializable]
	public class AudioTrackEditor: Editor {
		
		AudioTrack audioTrack;		// Source object
		
		SerializedProperty scriptIndexProp;
		SerializedProperty statesProp;
		SerializedProperty stateScriptProp;
		SerializedProperty instrumentState;
		SerializedProperty scriptStateIndicesProp;
		SerializedProperty nameProp;
		SerializedProperty doesLoopProp;
		SerializedProperty clipProp;
		SerializedProperty stateCounterProp;
		SerializedProperty fadeTimeProp;

		int currentIndex;
		float editorScrubberScriptIndex;
		List<string> scriptStepOptions;				// List of available options for each script step
		
		StateType stateType;
		
		SerializedProperty audioTrackProp;
		
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
			
			var target_cs = ( AudioTrack )target;
			scriptIndexProp = serializedObject.FindProperty( "scriptIndex" );
			statesProp = serializedObject.FindProperty( "states" );
			stateScriptProp = serializedObject.FindProperty( "stateScript" );
			scriptStateIndicesProp = serializedObject.FindProperty( "scriptStateIndices" );
			nameProp = serializedObject.FindProperty( "trackName" );
			doesLoopProp = serializedObject.FindProperty( "doesLoop" );
			clipProp = serializedObject.FindProperty( "clip" );
			stateCounterProp = serializedObject.FindProperty( "stateCounter" );
			fadeTimeProp = serializedObject.FindProperty( "fadeTime" );

			gridRect = new Rect (0, 0, 0, 22);
			frameRect = new Rect (0, 0, 3, 22);
			markerRect = new Rect (0, 0, 11, 11);

			audioTrack = target_cs ;
			
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

			name = "Hello";
			
		}
		
		void UpdateScriptStepOptions() {
			
			int i = 0;
			int n = statesProp.arraySize;
			
			scriptStepOptions.Clear();
			scriptStepOptions.Add(InstrumentState.HoldCommand);
			for( i = 0; i < n; i++ ) {
				scriptStepOptions.Add("(" + ( i + 1 ) + ") " + statesProp.GetArrayElementAtIndex( i ).FindPropertyRelative( "id" ).stringValue);
			}
			
		}
		
		void UpdateInstrumentState( bool getLastKeyIfNull ) {
			int index = 0;
			InstrumentState ins = null;
			serializedObject.ApplyModifiedProperties();
			instrumentState = null;
			ins = audioTrack.CurrentState();
			if ( getLastKeyIfNull && ( ins == null ) ) {
				ins = audioTrack.GetStateAtScriptIndex( stateScriptProp.arraySize - 1 );
			}
			if ( ins != null ) {
				index = audioTrack.GetStateIndexForId( ins.id );
				//Debug.Log( 'script: ' + scriptIndexProp.intValue + ' index: ' + index + ' id: ' + aws.id + ' ' + statesProp.arraySize );
				instrumentState = statesProp.GetArrayElementAtIndex( index );
				//Debug.Log( instrumentState.FindPropertyRelative( 'position' ).vector3Value );
			}	
			/*if ( instrumentState != null ) {
				Debug.Log( 'instrument state: ' + instrumentState.FindPropertyRelative( 'id' ).stringValue );
			} else {
				Debug.Log( 'null instrument state' );
			}*/
			serializedObject.Update();
		}
		
		public void LogStates() {
			
			int i = 0;
			int n = 0;
			string content;
			
			n = stateScriptProp.arraySize;
			content = "AUDIO TRACK STATE SCRIPT\n"; 
			for( i = 0; i < n; i++ ) {
				content += i + ": " + stateScriptProp.GetArrayElementAtIndex(i).stringValue + "\n";
			}
			Debug.Log (content);
			
		}
		
		public void InsertState() {
			SerializedKeyframeTimeline timeline = new SerializedKeyframeTimeline(audioTrack);
			timeline.InsertState();
			UpdateInstrumentState( false );
		}
		
		private void DeleteCurrentState() {
			SerializedKeyframeTimeline timeline = new SerializedKeyframeTimeline(audioTrack);
			timeline.DeleteCurrentState();
			UpdateInstrumentState( false );
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
						GUI.Label (gridRect, "");
					}

				}
				
				// current step marker
				GUI.skin.label = currentKeyframeSkin.window;
				frameRect.x = timelineRect.x - 1 + (unitWidth * scriptIndexProp.intValue) + 5.5f;
				frameRect.y = timelineRect.y;
				GUI.Label (frameRect, "");

				for (i=0; i<n; i++) {
					
					if ( PanoplyCore.targetStep == i ) {
						GUI.color = currentHighlightColor;
					}
					
					// inactive keyframes
					GUI.skin.label = currentKeyframeSkin.textField;
					if ((stateScriptProp.GetArrayElementAtIndex( i ).stringValue != InstrumentState.HoldCommand) && (i != draggedKeyframeIndex)) {
						markerRect.x = timelineRect.x + 1 + (unitWidth * i);
						markerRect.y = timelineRect.y + 5.5f;
						GUI.Label (markerRect, "");
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
					GUI.Label (markerRect, "");
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
				if ( stateScriptProp.GetArrayElementAtIndex( scriptIndexProp.intValue ).stringValue != InstrumentState.HoldCommand ) {
					GUI.color = new Color( .78f, .87f, .94f, 1 );
				}
				if ( GUILayout.Button( "Key", GUILayout.Width(35), GUILayout.Height(22) )) {
					if ( stateScriptProp.GetArrayElementAtIndex( scriptIndexProp.intValue ).stringValue == InstrumentState.HoldCommand ) {
						SetKeyframeStateType(StateType.Key);
					} else {
						SetKeyframeStateType(StateType.Hold);
					}
				}
				GUI.color = Color.white;
				
				GUI.enabled = !isOnLastStep;
				if ( GUILayout.Button( "Ins", GUILayout.Width(35), GUILayout.Height(22) )) {
					int option = EditorUtility.DisplayDialogComplex("Insert step locally or globally?", "Do you want to insert a step in the audio track's timeline only, or across the entire project?", "Audio timeline only", "Cancel", "Entire project");
					if (option == 0) {
						InsertState();
					} else if (option == 2) {
						PanoplyCore.InsertStateGlobally();
					}
				}
				
				GUI.enabled = !isOnFirstStep;
				if ( GUILayout.Button( "Del", GUILayout.Width(35), GUILayout.Height(22) ) ) {
					int option = EditorUtility.DisplayDialogComplex("Delete step locally or globally?", "Do you want to delete a step in the audio track's timeline only, or across the entire project?", "Audio timeline only", "Cancel", "Entire project");
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
								if (stateScriptProp.GetArrayElementAtIndex(keyframeIndex).stringValue != InstrumentState.HoldCommand) {
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
									stateScriptProp.GetArrayElementAtIndex(draggedKeyframeIndex).stringValue = InstrumentState.HoldCommand;
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
			UpdateInstrumentState( true );
		}
		
		private void SetKey() {

			AddState();
			SerializedProperty stateProp = statesProp.GetArrayElementAtIndex( statesProp.arraySize - 1 );
			stateProp.FindPropertyRelative( "volume" ).floatValue = instrumentState.FindPropertyRelative( "volume" ).floatValue;
			UpdateScriptStepOptions();

			while ( scriptIndexProp.intValue >= stateScriptProp.arraySize ) {
				stateScriptProp.arraySize++;
				scriptStateIndicesProp.arraySize++;
				scriptStateIndicesProp.GetArrayElementAtIndex( scriptStateIndicesProp.arraySize - 1 ).intValue = 0;
				stateScriptProp.GetArrayElementAtIndex( stateScriptProp.arraySize - 1 ).stringValue = scriptStepOptions[ scriptStateIndicesProp.GetArrayElementAtIndex( scriptStateIndicesProp.arraySize - 1 ).intValue ];
			}
			scriptStateIndicesProp.GetArrayElementAtIndex( scriptIndexProp.intValue ).intValue = scriptStepOptions.Count - 1;
			stateScriptProp.GetArrayElementAtIndex( scriptIndexProp.intValue ).stringValue = scriptStepOptions[ scriptStateIndicesProp.GetArrayElementAtIndex( scriptIndexProp.intValue ).intValue ];
			UpdateInstrumentState( false );
		}
		
		private void SetHold() {
			bool okToSetHold = false;
			
			if ( scriptIndexProp.intValue == 0 ) {
				EditorUtility.DisplayDialog( "Can’t delete first key", "The first step must be a key.", "OK" );
			} else {
				okToSetHold = true;
				if ( scriptIndexProp.intValue < stateScriptProp.arraySize ) {
					if ( stateScriptProp.GetArrayElementAtIndex( scriptIndexProp.intValue ).stringValue != InstrumentState.HoldCommand ) {
						SerializedKeyframeTimeline timeline = new SerializedKeyframeTimeline(audioTrack);
						timeline.DeleteStateAtIndex(scriptStateIndicesProp.GetArrayElementAtIndex( scriptIndexProp.intValue ).intValue - 1); // - 1 because 'hold' is at index 0
						UpdateInstrumentState( false );
						scriptStateIndicesProp.GetArrayElementAtIndex( scriptIndexProp.intValue ).intValue = 0;
						stateScriptProp.GetArrayElementAtIndex( scriptIndexProp.intValue ).stringValue = InstrumentState.HoldCommand;
						UpdateScriptStepOptions();
					}
				}
				if ( okToSetHold ) {
					while ( scriptIndexProp.intValue >= stateScriptProp.arraySize ) {
						stateScriptProp.arraySize++;
						scriptStateIndicesProp.arraySize++;
						scriptStateIndicesProp.GetArrayElementAtIndex( scriptStateIndicesProp.arraySize - 1 ).intValue = 0;
						stateScriptProp.GetArrayElementAtIndex( stateScriptProp.arraySize - 1 ).stringValue = InstrumentState.HoldCommand;
					}
					UpdateInstrumentState( false );
				}
			}
		}
		
		private void SetEnd() {
			/*if ( scriptIndexProp.intValue == 0 ) {
				EditorUtility.DisplayDialog( 'Can’t delete first key', 'The first step must be a key.', 'OK' );
			} else {
				if ( EditorUtility.DisplayDialog( 'Remove keys and holds?', 'Setting this step to "End" will delete any of its keys and holds from this step forward. Are you sure you want to do this?', 'OK', 'Cancel' ) ) {
					for ( i = ( stateScriptProp.arraySize - 1 ); i >= scriptIndexProp.intValue; i-- ) {
						if ( stateScriptProp.GetArrayElementAtIndex( i ).stringValue != InstrumentState.HoldCommand ) {
							deleteState( component, acriptStateIndicesProp.GetArrayElementAtIndex( i ).intValue - 1 ); // - 1 because 'hold' is at index 0
							updateScriptStepOptions();
						}
						stateScriptProp.DeleteArrayElementAtIndex( i );
					}
					UpdateInstrumentState( false );
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
			
			nameProp.stringValue = EditorGUILayout.TextField( "Name", nameProp.stringValue );
			clipProp.objectReferenceValue = EditorGUILayout.ObjectField( "Audio Clip", clipProp.objectReferenceValue, typeof( AudioClip ), false );
			fadeTimeProp.floatValue = EditorGUILayout.Slider( "Fade time", fadeTimeProp.floatValue, 0.0f, 10.0f );
			doesLoopProp.boolValue = EditorGUILayout.Toggle( "Loop", doesLoopProp.boolValue );

			EditorGUILayout.Space();
			
			if ( statesProp.arraySize == 0 ) {
				statesProp.arraySize++;
				stateCounterProp.intValue++;
				instrumentState = statesProp.GetArrayElementAtIndex( 0 );
				instrumentState.FindPropertyRelative( "volume" ).floatValue = 0;
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
					stateScriptProp.GetArrayElementAtIndex( stateScriptProp.arraySize - 1 ).stringValue = InstrumentState.HoldCommand;
				}
			}
			
			UpdateInstrumentState( false );
			
			KeyframeEditor();
			
			EditorGUILayout.Space();
			
			if ( instrumentState != null ) {
				EditorGUI.BeginChangeCheck();
				instrumentState.FindPropertyRelative( "volume" ).floatValue = EditorGUILayout.Slider( "Volume", instrumentState.FindPropertyRelative( "volume" ).floatValue, 0, 1 );
				EditorGUILayout.Space();
			}
			
			DrawKeyframeTimeline();
			
			serializedObject.ApplyModifiedProperties();

		}
		
	}
}
