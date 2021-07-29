using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEditor;
using Opertoon.Panoply;

/**
 * CaptionEditor
 * Copyright © Erik Loyer
 * erik@opertoon.com
 * Part of the Panoply engine
 */

namespace Opertoon.Panoply {
	[CustomEditor( typeof( Caption ) )]

	[System.Serializable]
	public class CaptionEditor: Editor {

		Caption caption;		// Source object
		
		SerializedProperty scriptIndexProp;
		SerializedProperty statesProp;
		SerializedProperty stateScriptProp;
		SerializedProperty captionState;
		SerializedProperty scriptStateIndicesProp;
		SerializedProperty stateCounterProp;
        SerializedProperty renderingMethodProp;
		
		float editorScrubberScriptIndex;
		int currentIndex;
		int stateCount;
		List<string> scriptStepOptions;				// List of available options for each script step
		StateType stateType;

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
			
			var target_cs = ( Caption)target;
			scriptIndexProp = serializedObject.FindProperty( "scriptIndex" );
			statesProp = serializedObject.FindProperty( "states" );
			stateScriptProp = serializedObject.FindProperty( "stateScript" );
			scriptStateIndicesProp = serializedObject.FindProperty( "scriptStateIndices" );
			stateCounterProp = serializedObject.FindProperty( "stateCounter" );
            renderingMethodProp = serializedObject.FindProperty("renderingMethod");

			gridRect = new Rect (0, 0, 0, 22);
			frameRect = new Rect (0, 0, 3, 22);
			markerRect = new Rect (0, 0, 11, 11);

			caption = target_cs ;

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
			scriptStepOptions.Add (CaptionState.HoldCommand);
			for( i = 0; i < n; i++ ) {
				scriptStepOptions.Add("(" + ( i + 1 ) + ") " + statesProp.GetArrayElementAtIndex( i ).FindPropertyRelative( "id" ).stringValue);
			}

		}
		
		void UpdateCaptionState( bool getLastKeyIfNull ) {
			int index = 0;
			CaptionState cs = null;
			serializedObject.ApplyModifiedProperties();
			captionState = null;
			cs = caption.CurrentState();
			if ( getLastKeyIfNull && ( cs == null ) ) {
				cs = caption.GetStateAtScriptIndex( stateScriptProp.arraySize - 1 );
			}
			if ( cs != null ) {
				index = caption.GetStateIndexForId( cs.id );
				captionState = statesProp.GetArrayElementAtIndex( index );
			}	
			/*if ( captionState != null ) {
				Debug.Log( 'caption state: ' + captionState.FindPropertyRelative( 'id' ).stringValue );
			} else {
				Debug.Log( 'null caption state' );
			}*/
			serializedObject.Update();
		}
		
		public void LogStates() {
			
			int i = 0;
			int n = 0;
			string content;

			Debug.Log ("----------");
			
			content = "CAPTION STATES\n";
			n = statesProp.arraySize;
			for( i = 0; i < n; i++ ) {
				content += i + ": " + statesProp.GetArrayElementAtIndex(i).FindPropertyRelative("id").stringValue + "\n";
			}
			Debug.Log (content);
			
			Debug.Log ( "--" );
			
			content = "CAPTION STATE SCRIPT INDICES\n";
			n = scriptStateIndicesProp.arraySize;
			for( i = 0; i < n; i++ ) {
				content += i + ": " + scriptStateIndicesProp.GetArrayElementAtIndex(i).intValue + "\n";
			}
			Debug.Log (content);
			
			Debug.Log ( "--" );

			n = stateScriptProp.arraySize;
			content = "CAPTION STATE SCRIPT\n"; 
			for( i = 0; i < n; i++ ) {
				content += i + ": " + stateScriptProp.GetArrayElementAtIndex(i).stringValue + "\n";
			}
			Debug.Log (content);

		}

		private void InsertState() {
			SerializedKeyframeTimeline timeline = new SerializedKeyframeTimeline(caption);
			timeline.InsertState();
			UpdateCaptionState( false );
		}

		private void DeleteCurrentState() {
			SerializedKeyframeTimeline timeline = new SerializedKeyframeTimeline(caption);
			timeline.DeleteCurrentState();
			UpdateCaptionState( false );
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
					if ((stateScriptProp.GetArrayElementAtIndex( i ).stringValue != CaptionState.HoldCommand) && (i != draggedKeyframeIndex)) {
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
				if ( stateScriptProp.GetArrayElementAtIndex( scriptIndexProp.intValue ).stringValue != CaptionState.HoldCommand ) {
					GUI.color = new Color( .78f, .87f, .94f, 1 );
				}
				if ( GUILayout.Button( "Key", GUILayout.Width(35), GUILayout.Height(22) )) {
					if ( stateScriptProp.GetArrayElementAtIndex( scriptIndexProp.intValue ).stringValue == CaptionState.HoldCommand ) {
						SetKeyframeStateType(StateType.Key);
					} else {
						SetKeyframeStateType(StateType.Hold);
					}
				}
				GUI.color = Color.white;
				
				GUI.enabled = !isOnLastStep;
				if ( GUILayout.Button( "Ins", GUILayout.Width(35), GUILayout.Height(22) )) {
					int option = EditorUtility.DisplayDialogComplex("Insert step locally or globally?", "Do you want to insert a step in the caption's timeline only, or across the entire project?", "Caption timeline only", "Cancel", "Entire project");
					if (option == 0) {
						InsertState();
					} else if (option == 2) {
						PanoplyCore.InsertStateGlobally();
					}
				}
				
				GUI.enabled = !isOnFirstStep;
				if ( GUILayout.Button( "Del", GUILayout.Width(35), GUILayout.Height(22) ) ) {
					int option = EditorUtility.DisplayDialogComplex("Delete step locally or globally?", "Do you want to delete a step in the caption's timeline only, or across the entire project?", "Caption timeline only", "Cancel", "Entire project");
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
								if (stateScriptProp.GetArrayElementAtIndex(keyframeIndex).stringValue != CaptionState.HoldCommand) {
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
							} else {
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
									stateScriptProp.GetArrayElementAtIndex(draggedKeyframeIndex).stringValue = CaptionState.HoldCommand;
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
			UpdateCaptionState( true );
		}
		
		private void SetKey() {

			AddState();
			SerializedProperty stateProp = statesProp.GetArrayElementAtIndex( statesProp.arraySize - 1 );
			stateProp.FindPropertyRelative( "index" ).intValue = captionState.FindPropertyRelative( "index" ).intValue;
			stateProp.FindPropertyRelative( "skin" ).objectReferenceValue = captionState.FindPropertyRelative( "skin" ).objectReferenceValue;
			stateProp.FindPropertyRelative( "layout" ).rectValue = captionState.FindPropertyRelative( "layout" ).rectValue;
			stateProp.FindPropertyRelative( "text" ).stringValue = captionState.FindPropertyRelative( "text" ).stringValue;
			stateProp.FindPropertyRelative( "dropShadow" ).vector2Value = captionState.FindPropertyRelative( "dropShadow" ).vector2Value;
			stateProp.FindPropertyRelative( "tail" ).objectReferenceValue = captionState.FindPropertyRelative( "tail" ).objectReferenceValue;
			stateProp.FindPropertyRelative( "tailWidth" ).floatValue = captionState.FindPropertyRelative( "tailWidth" ).floatValue;
			stateProp.FindPropertyRelative( "tailHeight" ).floatValue = captionState.FindPropertyRelative( "tailHeight" ).floatValue;
			stateProp.FindPropertyRelative( "tailRotation" ).floatValue = captionState.FindPropertyRelative( "tailRotation" ).floatValue;
			stateProp.FindPropertyRelative( "color" ).colorValue = captionState.FindPropertyRelative( "color" ).colorValue;
			stateProp.FindPropertyRelative( "drawTail" ).boolValue = captionState.FindPropertyRelative( "drawTail" ).boolValue;
			stateProp.FindPropertyRelative( "vertAlign" ).enumValueIndex = captionState.FindPropertyRelative( "vertAlign" ).enumValueIndex;
			stateProp.FindPropertyRelative( "horzAlign" ).enumValueIndex = captionState.FindPropertyRelative( "horzAlign" ).enumValueIndex;
			stateProp.FindPropertyRelative( "skin" ).objectReferenceValue = captionState.FindPropertyRelative( "skin" ).objectReferenceValue;
			stateProp.FindPropertyRelative( "tail" ).objectReferenceValue = captionState.FindPropertyRelative( "tail" ).objectReferenceValue;
			stateProp.FindPropertyRelative( "dropShadow" ).vector2Value = captionState.FindPropertyRelative( "dropShadow" ).vector2Value;
			stateProp.FindPropertyRelative( "offsetUnitsX" ).enumValueIndex = captionState.FindPropertyRelative( "offsetUnitsX" ).enumValueIndex;
			stateProp.FindPropertyRelative( "offsetUnitsY" ).enumValueIndex = captionState.FindPropertyRelative( "offsetUnitsY" ).enumValueIndex;

			UpdateScriptStepOptions();
			while ( scriptIndexProp.intValue >= stateScriptProp.arraySize ) {
				stateScriptProp.arraySize++;
				scriptStateIndicesProp.arraySize++;
				scriptStateIndicesProp.GetArrayElementAtIndex( scriptStateIndicesProp.arraySize - 1 ).intValue = 0;
				stateScriptProp.GetArrayElementAtIndex( stateScriptProp.arraySize - 1 ).stringValue = scriptStepOptions[ scriptStateIndicesProp.GetArrayElementAtIndex( scriptStateIndicesProp.arraySize - 1 ).intValue ];
			}
			scriptStateIndicesProp.GetArrayElementAtIndex( scriptIndexProp.intValue ).intValue = scriptStepOptions.Count - 1;
			stateScriptProp.GetArrayElementAtIndex( scriptIndexProp.intValue ).stringValue = scriptStepOptions[ scriptStateIndicesProp.GetArrayElementAtIndex( scriptIndexProp.intValue ).intValue ];
			UpdateCaptionState( false );
			//LogStates();
		}
		
		private void SetHold() {
			bool okToSetHold = false;
			
			if ( scriptIndexProp.intValue == 0 ) {
				EditorUtility.DisplayDialog( "Can’t delete first key", "The first step must be a key.", "OK" );
			} else {
				okToSetHold = true;
				if ( scriptIndexProp.intValue < stateScriptProp.arraySize ) {
					if ( stateScriptProp.GetArrayElementAtIndex( scriptIndexProp.intValue ).stringValue != CaptionState.HoldCommand ) {
						SerializedKeyframeTimeline timeline = new SerializedKeyframeTimeline(caption);
						timeline.DeleteStateAtIndex(scriptStateIndicesProp.GetArrayElementAtIndex( scriptIndexProp.intValue ).intValue - 1); // - 1 because 'hold' is at index 0
						UpdateCaptionState( false );
						scriptStateIndicesProp.GetArrayElementAtIndex( scriptIndexProp.intValue ).intValue = 0;
						stateScriptProp.GetArrayElementAtIndex( scriptIndexProp.intValue ).stringValue = CaptionState.HoldCommand;
						UpdateScriptStepOptions();
					}
				}
				if ( okToSetHold ) {
					while ( scriptIndexProp.intValue >= stateScriptProp.arraySize ) {
						stateScriptProp.arraySize++;
						scriptStateIndicesProp.arraySize++;
						scriptStateIndicesProp.GetArrayElementAtIndex( scriptStateIndicesProp.arraySize - 1 ).intValue = 0;
						stateScriptProp.GetArrayElementAtIndex( stateScriptProp.arraySize - 1 ).stringValue = CaptionState.HoldCommand;
					}
					UpdateCaptionState( false );
				}
			}
			//LogStates();
		}
		
		private void SetEnd() {
			/*if ( scriptIndexProp.intValue == 0 ) {
				EditorUtility.DisplayDialog( 'Can’t delete first key', 'The first step must be a key.', 'OK' );
			} else {
				if ( EditorUtility.DisplayDialog( 'Remove keys and holds?', 'Setting this step to "End" will delete any of its keys and holds from this step forward. Are you sure you want to do this?', 'OK', 'Cancel' ) ) {
					for ( i = ( stateScriptProp.arraySize - 1 ); i >= scriptIndexProp.intValue; i-- ) {
						if ( stateScriptProp.GetArrayElementAtIndex( i ).stringValue != CaptionState.HoldCommand ) {
							deleteState( component, acriptStateIndicesProp.GetArrayElementAtIndex( i ).intValue - 1 ); // - 1 because 'hold' is at index 0
							updateScriptStepOptions();
						}
						stateScriptProp.DeleteArrayElementAtIndex( i );
					}
					UpdateCaptionState( false );
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
			
			EditorGUI.BeginChangeCheck();
			if (PanoplyCore.scene != null) {
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

            EditorGUILayout.PropertyField(renderingMethodProp, new GUIContent("Rendering method"));

                EditorGUILayout.Space();
			
			if ( statesProp.arraySize == 0 ) {
				statesProp.arraySize++;
				stateCounterProp.intValue++;
				captionState = statesProp.GetArrayElementAtIndex( 0 );
				captionState.FindPropertyRelative( "id" ).stringValue = "State " + stateCounterProp.intValue;
				captionState.FindPropertyRelative( "index" ).intValue = statesProp.arraySize - 1;
				captionState.FindPropertyRelative( "layout" ).rectValue = new Rect( 0.0f, 0.0f, 200.0f, 70.0f );
				captionState.FindPropertyRelative( "text" ).stringValue = "Hello world.";
				captionState.FindPropertyRelative( "tailWidth" ).floatValue = 20.0f;
				captionState.FindPropertyRelative( "tailHeight" ).floatValue = 100.0f;
				captionState.FindPropertyRelative( "color" ).colorValue = Color.white;
				captionState.FindPropertyRelative( "vertAlign" ).enumValueIndex = ( int )CaptionVertAlign.Top;
				captionState.FindPropertyRelative( "horzAlign" ).enumValueIndex = ( int )CaptionHorzAlign.Left;
				captionState.FindPropertyRelative( "skin" ).objectReferenceValue = Resources.Load ("CenteredRoundRectCaption") as GUISkin;
				captionState.FindPropertyRelative( "tail" ).objectReferenceValue = Resources.Load ("balloon_stem") as Texture2D;
				captionState.FindPropertyRelative( "dropShadow" ).vector2Value = new Vector2(2, 2);
				captionState.FindPropertyRelative( "offsetUnitsX" ).enumValueIndex = 1;
				captionState.FindPropertyRelative( "offsetUnitsY" ).enumValueIndex = 1;
				captionState.FindPropertyRelative ("cornerRounding").floatValue = .11f;

				UpdateScriptStepOptions ();
			}
			
			if ( stateScriptProp.arraySize == 0 ) {
				stateScriptProp.arraySize++;
				scriptStateIndicesProp.arraySize++;
				scriptStateIndicesProp.GetArrayElementAtIndex( 0 ).intValue = 1;
				stateScriptProp.GetArrayElementAtIndex( 0 ).stringValue = scriptStepOptions[ scriptStateIndicesProp.GetArrayElementAtIndex( 0 ).intValue ];
			}

			// fill empty steps with Hold commands
			if (stateScriptProp != null && PanoplyCore.scene != null) {
				while (stateScriptProp.arraySize < PanoplyCore.scene.stepCount) {
					stateScriptProp.arraySize++;
					scriptStateIndicesProp.arraySize++;
					scriptStateIndicesProp.GetArrayElementAtIndex (stateScriptProp.arraySize - 1).intValue = stateScriptProp.arraySize;
					stateScriptProp.GetArrayElementAtIndex (stateScriptProp.arraySize - 1).stringValue = CaptionState.HoldCommand;
				}
			}

			UpdateCaptionState ( false );

			if ( captionState != null ) {
				
				EditorStyles.textField.wordWrap = true;
				
				EditorGUI.BeginChangeCheck();
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel( "Text" );
				captionState.FindPropertyRelative( "text" ).stringValue = EditorGUILayout.TextArea( captionState.FindPropertyRelative( "text" ).stringValue, GUILayout.Height( 60.0f ) );
				EditorGUILayout.EndHorizontal();
				
				// propagate a change in one state's text to all the other states
				if ( EditorGUI.EndChangeCheck() ) {
					int i, n;
					SerializedProperty cs;
					n = statesProp.arraySize;
					for( i = 0; i < n; i++ ) {
						cs = statesProp.GetArrayElementAtIndex( i );
						if ( cs != captionState ) {
							cs.FindPropertyRelative( "text" ).stringValue = captionState.FindPropertyRelative( "text" ).stringValue;
						}
					}
				}
				
				EditorGUILayout.Space();
			}

			KeyframeEditor();

			EditorGUILayout.Space();
			
			if ( captionState != null ) {

				captionState.FindPropertyRelative ("skin").objectReferenceValue = EditorGUILayout.ObjectField ("GUI Skin", captionState.FindPropertyRelative ("skin").objectReferenceValue, typeof (GUISkin), true);
				captionState.FindPropertyRelative( "color" ).colorValue = EditorGUILayout.ColorField( "Color", captionState.FindPropertyRelative( "color" ).colorValue );
				
				EditorGUILayout.Space();
				
				var tmp_cs1 = captionState.FindPropertyRelative( "layout" ).rectValue;
	            tmp_cs1.width = EditorGUILayout.FloatField( "Width", captionState.FindPropertyRelative( "layout" ).rectValue.width );
	            captionState.FindPropertyRelative( "layout" ).rectValue = tmp_cs1;
                if (renderingMethodProp.enumValueIndex == (int)RenderingMethod.LegacyGUI)
                {
                    var tmp_cs2 = captionState.FindPropertyRelative("layout").rectValue;
                    tmp_cs2.height = EditorGUILayout.FloatField("Height", captionState.FindPropertyRelative("layout").rectValue.height);
                    captionState.FindPropertyRelative("layout").rectValue = tmp_cs2;
                }
				
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel("Drop Shadow Offset");
				EditorGUILayout.LabelField( "X", GUILayout.Width(40.0f) );
				var tmp_cs3 = captionState.FindPropertyRelative( "dropShadow" ).vector2Value;
	            tmp_cs3.x = EditorGUILayout.FloatField( captionState.FindPropertyRelative( "dropShadow" ).vector2Value.x );
	            captionState.FindPropertyRelative( "dropShadow" ).vector2Value = tmp_cs3;
				EditorGUILayout.LabelField( "Y", GUILayout.Width(40.0f) );
				var tmp_cs4 = captionState.FindPropertyRelative( "dropShadow" ).vector2Value;
	            tmp_cs4.y = EditorGUILayout.FloatField( captionState.FindPropertyRelative( "dropShadow" ).vector2Value.y );
	            captionState.FindPropertyRelative( "dropShadow" ).vector2Value = tmp_cs4;
				EditorGUILayout.EndHorizontal();
				
				EditorGUILayout.Space();
				
				EditorGUI.BeginChangeCheck();
				captionState.FindPropertyRelative( "drawTail" ).boolValue = EditorGUILayout.Toggle( "Is Dialogue Balloon", captionState.FindPropertyRelative( "drawTail" ).boolValue );
				if ( EditorGUI.EndChangeCheck() ) {
					if ( captionState.FindPropertyRelative( "drawTail" ).boolValue ) {
						var tmp_cs5 = captionState.FindPropertyRelative( "layout" ).rectValue;
	                    tmp_cs5.x = 0.5f;
	                    captionState.FindPropertyRelative( "layout" ).rectValue = tmp_cs5;
						var tmp_cs6 = captionState.FindPropertyRelative( "layout" ).rectValue;
	                    tmp_cs6.y = 0.5f;
	                    captionState.FindPropertyRelative( "layout" ).rectValue = tmp_cs6;
					} else {
						var tmp_cs7 = captionState.FindPropertyRelative( "layout" ).rectValue;
	                    tmp_cs7.x = 10.0f;
	                    captionState.FindPropertyRelative( "layout" ).rectValue = tmp_cs7;
						var tmp_cs8 = captionState.FindPropertyRelative( "layout" ).rectValue;
	                    tmp_cs8.y = 10.0f;
	                    captionState.FindPropertyRelative( "layout" ).rectValue = tmp_cs8;
					}
				}
				
				EditorGUILayout.Space();
				
				if ( captionState.FindPropertyRelative( "drawTail" ).boolValue ) {
					var tmp_cs9 = captionState.FindPropertyRelative( "layout" ).rectValue;
					tmp_cs9.x = EditorGUILayout.Slider( "X", captionState.FindPropertyRelative( "layout" ).rectValue.x, -2, 3 );
	                captionState.FindPropertyRelative( "layout" ).rectValue = tmp_cs9;
					var tmp_cs10 = captionState.FindPropertyRelative( "layout" ).rectValue;
					tmp_cs10.y = EditorGUILayout.Slider( "Y", captionState.FindPropertyRelative( "layout" ).rectValue.y, -2, 3 );
					captionState.FindPropertyRelative( "layout" ).rectValue = tmp_cs10;
                    if (renderingMethodProp.enumValueIndex == (int)RenderingMethod.LegacyGUI)
                    {
                        captionState.FindPropertyRelative("tail").objectReferenceValue = EditorGUILayout.ObjectField("Tail Texture", captionState.FindPropertyRelative("tail").objectReferenceValue, typeof(Texture2D), true);
                    }
                    captionState.FindPropertyRelative( "tailWidth" ).floatValue = EditorGUILayout.FloatField( "Tail Width", captionState.FindPropertyRelative( "tailWidth" ).floatValue );
					captionState.FindPropertyRelative( "tailHeight" ).floatValue = EditorGUILayout.FloatField( "Tail Length", captionState.FindPropertyRelative( "tailHeight" ).floatValue );
					captionState.FindPropertyRelative( "tailRotation" ).floatValue = EditorGUILayout.Slider( "Angle", captionState.FindPropertyRelative( "tailRotation" ).floatValue, -360, 360 ); 
                    if (renderingMethodProp.enumValueIndex == (int)RenderingMethod.Canvas)
                    {
                        captionState.FindPropertyRelative("cornerRounding").floatValue = EditorGUILayout.Slider("Corner Rounding", captionState.FindPropertyRelative("cornerRounding").floatValue, 0, 1);
                    }
                } else {
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.PrefixLabel("Alignment");
					EditorGUILayout.PropertyField( captionState.FindPropertyRelative( "vertAlign" ), new GUIContent( "" ), GUILayout.ExpandWidth(false) );
					EditorGUILayout.PropertyField( captionState.FindPropertyRelative( "horzAlign" ), new GUIContent( "" ), GUILayout.ExpandWidth(false) );
					EditorGUILayout.EndHorizontal();
					EditorGUILayout.BeginHorizontal();
					var tmp_cs11 = captionState.FindPropertyRelative( "layout" ).rectValue;
					if (captionState.FindPropertyRelative( "offsetUnitsX" ).enumValueIndex == 0) {
						tmp_cs11.x = EditorGUILayout.FloatField( "Offset X", captionState.FindPropertyRelative( "layout" ).rectValue.x );
					} else {
						tmp_cs11.x = EditorGUILayout.Slider( "Offset X", captionState.FindPropertyRelative( "layout" ).rectValue.x, -50, 150 );
					}
					captionState.FindPropertyRelative( "layout" ).rectValue = tmp_cs11;
					captionState.FindPropertyRelative( "offsetUnitsX" ).enumValueIndex = EditorGUILayout.Popup( captionState.FindPropertyRelative( "offsetUnitsX" ).enumValueIndex, new string[] {"px", "%"}, GUILayout.Width(60) );
					EditorGUILayout.EndHorizontal();
					EditorGUILayout.BeginHorizontal();
					var tmp_cs12 = captionState.FindPropertyRelative( "layout" ).rectValue;
					if (captionState.FindPropertyRelative( "offsetUnitsY" ).enumValueIndex == 0) {
						tmp_cs12.y = EditorGUILayout.FloatField( "Offset Y", captionState.FindPropertyRelative( "layout" ).rectValue.y );
					} else {
						tmp_cs12.y = EditorGUILayout.Slider( "Offset Y", captionState.FindPropertyRelative( "layout" ).rectValue.y, -50, 150 );
					}
			        captionState.FindPropertyRelative( "layout" ).rectValue = tmp_cs12;
					captionState.FindPropertyRelative( "offsetUnitsY" ).enumValueIndex = EditorGUILayout.Popup( captionState.FindPropertyRelative( "offsetUnitsY" ).enumValueIndex, new string[] {"px", "%"}, GUILayout.Width(60) );
					EditorGUILayout.EndHorizontal();
                    if (renderingMethodProp.enumValueIndex == (int)RenderingMethod.Canvas)
                    {
                        captionState.FindPropertyRelative("cornerRounding").floatValue = EditorGUILayout.Slider("Corner Rounding", captionState.FindPropertyRelative("cornerRounding").floatValue, 0, 1);
                    }
                }

            }

			DrawKeyframeTimeline();
			
			serializedObject.ApplyModifiedProperties();
			
		}
		
	}
}