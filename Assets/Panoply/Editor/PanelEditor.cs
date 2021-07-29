using UnityEngine;
using System;
using UnityEditor;
using Opertoon.Panoply;
using System.Collections.Generic;
using System.Collections;

/**
 * PanelEditor
 * Copyright © Erik Loyer
 * erik@opertoon.com
 * Part of the Panoply engine
 */

namespace Opertoon.Panoply {
	[CustomEditor( typeof( Panel ) )]
	
	[System.Serializable]
	public class PanelEditor: Editor {
		
		Panel panel;		// Source object
		
		SerializedProperty scriptIndexProp;
		SerializedProperty rowsProp;
		SerializedProperty columnsProp;
		
		SerializedProperty frameStatesProp;
		SerializedProperty frameStateScriptProp;
		SerializedProperty frameScriptStateIndicesProp;
		SerializedProperty frameState;
		List<string> frameScriptStepOptions;								// List of available options for each script step
		SerializedProperty interceptInteractionProp;
		SerializedProperty preserveFramingProp;
		SerializedProperty framingDistanceProp;
		SerializedProperty disableCameraControlProp;
		
		SerializedProperty cameraStatesProp;
		SerializedProperty cameraStateScriptProp;
		SerializedProperty cameraState;
		SerializedProperty cameraScriptStateIndicesProp;
		List<string> cameraScriptStepOptions;								// List of available options for each script step
		
		SerializedProperty passiveMotionStatesHProp;
		SerializedProperty passiveMotionStatesVProp;
		SerializedProperty passiveMotionStateScriptProp;
		SerializedProperty passiveMotionStateH;
		SerializedProperty passiveMotionStateV;
		SerializedProperty passiveMotionScriptStateIndicesProp;
		List<string> passiveMotionScriptStepOptions;                        // List of available options for each script step

		SerializedProperty isEditingAdvancedProp;
		SerializedProperty isEditingFrameProp;
		SerializedProperty isEditingCameraProp;
		SerializedProperty isEditingPassiveMotionProp;
		
		SerializedProperty topProp;
		SerializedProperty leftProp;
		SerializedProperty widthProp;
		SerializedProperty heightProp;
		SerializedProperty lookAtEnabledProp;
		
		int currentIndex;
		float editorScrubberScriptIndex;
		
		SerializedProperty frameStateCounterProp;
		SerializedProperty cameraStateCounterProp;
		SerializedProperty passiveMotionHStateCounterProp;
		SerializedProperty passiveMotionVStateCounterProp;
		
		StateType stateType;
		
		GUISkin gridSkin;
		GUISkin gridSkinLight;
		GUISkin currentGridSkin;
		GUISkin keyframeSkin;
		GUISkin keyframeSkinLight;
		GUISkin currentKeyframeSkin;
		Color highlightColor = new Color( .45f, .76f, 1, 1 );
		Color highlightColorLight = new Color( .78f, .87f, .94f, 1 );
		Color currentHighlightColor;
		Panel[] panels;
		Vector2 startDragPos;
		Vector2 startDragGridPos;
		float gridMargin = 25.0f;
		int stepIndexPriorToMouseDown;
		int mouseDownStepIndex;
		int draggedKeyframeIndex;
		int draggedKeyframePosition;
		bool draggedAwayFromInitialStep;
		PanelComponent draggedKeyframeComponent;
		Rect frameTimelineRect;
		Rect cameraTimelineRect;
		Rect passiveMotionTimelineRect;
		Rect gridRect;
		Rect frameRect;
		Rect markerRect;
		Rect activeKeyframeRect;
		Rect panelBaseRect;
		Rect marginRect;
		Rect activeRect;

		SerializedProperty statesProp;
		SerializedProperty stateScriptProp;
		SerializedProperty scriptStateIndicesProp;
		SerializedProperty stateCounterProp;
		string holdCommand;
		List<string> scriptStepOptions;

		public void OnEnable() {
			
			var target_cs = ( Panel )target;
			scriptIndexProp = serializedObject.FindProperty( "scriptIndex" );
			rowsProp = serializedObject.FindProperty( "rows" );
			columnsProp = serializedObject.FindProperty( "columns" );
			lookAtEnabledProp = serializedObject.FindProperty( "lookAtEnabled" );
			
			panel = target_cs ;
			GetStarted();
			
			panels = FindObjectsOfType( typeof( Panel )) as Panel[];
			
		}
		
		void GetStarted() {
			
			frameStatesProp = serializedObject.FindProperty( "frameStates" );
			frameStateScriptProp = serializedObject.FindProperty( "frameStateScript" );
			frameScriptStateIndicesProp = serializedObject.FindProperty( "frameScriptStateIndices" );
			interceptInteractionProp = serializedObject.FindProperty( "interceptInteraction" );
			preserveFramingProp = serializedObject.FindProperty ("preserveFraming");
			framingDistanceProp = serializedObject.FindProperty ("framingDistance");
			disableCameraControlProp = serializedObject.FindProperty( "disableCameraControl" );
			frameScriptStepOptions = new List<string>();

			cameraStatesProp = serializedObject.FindProperty( "cameraStates" );	
			cameraStateScriptProp = serializedObject.FindProperty( "cameraStateScript" );
			cameraScriptStateIndicesProp = serializedObject.FindProperty( "cameraScriptStateIndices" );
			cameraScriptStepOptions = new List<string>();

			passiveMotionStatesHProp = serializedObject.FindProperty( "passiveMotionStatesH" );
			passiveMotionStatesVProp = serializedObject.FindProperty( "passiveMotionStatesV" );
			passiveMotionStateScriptProp = serializedObject.FindProperty( "passiveMotionStateScript" );
			passiveMotionScriptStateIndicesProp = serializedObject.FindProperty( "passiveMotionScriptStateIndices" );
			passiveMotionScriptStepOptions = new List<string>();

			isEditingAdvancedProp = serializedObject.FindProperty ("isEditingAdvanced");
			isEditingFrameProp = serializedObject.FindProperty( "isEditingFrame" );
			isEditingCameraProp = serializedObject.FindProperty( "isEditingCamera" );
			isEditingPassiveMotionProp = serializedObject.FindProperty( "isEditingPassiveMotion" );
			
			frameStateCounterProp = serializedObject.FindProperty( "frameStateCounter" );
			cameraStateCounterProp = serializedObject.FindProperty( "cameraStateCounter" );
			passiveMotionHStateCounterProp = serializedObject.FindProperty( "passiveMotionHStateCounter" );
			passiveMotionVStateCounterProp = serializedObject.FindProperty( "passiveMotionVStateCounter" );
			
			topProp = serializedObject.FindProperty( "top" );
			leftProp = serializedObject.FindProperty( "left" );
			widthProp = serializedObject.FindProperty( "width" );
			heightProp = serializedObject.FindProperty( "height" );

			gridRect = new Rect (0, 0, 0, 22);
			frameRect = new Rect (0, 0, 3, 22);
			markerRect = new Rect (0, 0, 11, 11);
			activeKeyframeRect = new Rect ();
			panelBaseRect = new Rect ();
			activeRect = new Rect ();
			marginRect = new Rect ();

			if (PanoplyCore.scene != null) {
				PanoplyCore.interpolatedStep = PanoplyCore.targetStep = Mathf.Clamp(PanoplyCore.targetStep, 0, PanoplyCore.scene.stepCount - 1);
			} else {
				PanoplyCore.interpolatedStep = PanoplyCore.targetStep = 0;
			}

			UpdateScriptStepOptions( PanelComponent.PanelFrame );
			UpdateScriptStepOptions( PanelComponent.PanelCamera );
			UpdateScriptStepOptions( PanelComponent.PanelPassiveMotion );
			
			gridSkin = Resources.Load("PanelGrid") as GUISkin;
			gridSkinLight = Resources.Load("PanelGridLight") as GUISkin;
			keyframeSkin = Resources.Load("Keyframe") as GUISkin;
			keyframeSkinLight = Resources.Load("KeyframeLight") as GUISkin;

			mouseDownStepIndex = draggedKeyframePosition = draggedKeyframeIndex = -1;
			
			if ( EditorGUIUtility.isProSkin ) {
				currentGridSkin = gridSkin;
				currentKeyframeSkin = keyframeSkin;
				currentHighlightColor = highlightColor;
			} else {
				currentGridSkin = gridSkinLight;
				currentKeyframeSkin = keyframeSkinLight;
				currentHighlightColor = highlightColorLight;
			}

		}
		
		void UpdateScriptStepOptions( PanelComponent component ) {
			
			int i = 0;
			int n = 0;
			
			switch ( component ) {

			case PanelComponent.PanelFrame:				
				n = frameStatesProp.arraySize;
				frameScriptStepOptions.Clear();
				frameScriptStepOptions.Add(FrameState.HoldCommand);
				for( i = 0; i < n; i++ ) {
					frameScriptStepOptions.Add("(" + ( i + 1 ) + ") " + frameStatesProp.GetArrayElementAtIndex( i ).FindPropertyRelative( "id" ).stringValue);
				}
				break;
				
			case PanelComponent.PanelCamera:
				n = cameraStatesProp.arraySize;
				cameraScriptStepOptions.Clear();
				cameraScriptStepOptions.Add(CameraState.HoldCommand);
				for( i = 0; i < n; i++ ) {
					cameraScriptStepOptions.Add("(" + ( i + 1 ) + ") " + cameraStatesProp.GetArrayElementAtIndex( i ).FindPropertyRelative( "id" ).stringValue);
				}
				break;
				
			case PanelComponent.PanelPassiveMotion:
				n = passiveMotionStatesHProp.arraySize;
				passiveMotionScriptStepOptions.Clear();
				passiveMotionScriptStepOptions.Add(PassiveMotionState.HoldCommand);
				for( i = 0; i < n; i++ ) {
					passiveMotionScriptStepOptions.Add("(" + ( i + 1 ) + ") " + passiveMotionStatesHProp.GetArrayElementAtIndex( i ).FindPropertyRelative( "id" ).stringValue);
				}
				break;
				
			}
			
		}
		
		void UpdateState( PanelComponent component, bool getLastKeyIfNull ) {
			
			int index = 0;
			
			serializedObject.ApplyModifiedProperties();
			
			switch ( component ) {
				
			case PanelComponent.PanelFrame:
				FrameState fs = null;
				frameState = null;
				fs = panel.CurrentFrameState();
				if ( getLastKeyIfNull && ( fs == null ) ) {
					fs = panel.GetStateAtScriptIndex( PanelComponent.PanelFrame, frameStateScriptProp.arraySize - 1 ) as FrameState;
				}
				if ( fs != null ) {
					index = panel.GetStateIndexForId( PanelComponent.PanelFrame, fs.id );
					frameState = frameStatesProp.GetArrayElementAtIndex( index );
				}
				
				/*if ( frameState != null ) {
					Debug.Log( 'frame state: ' + frameState.FindPropertyRelative( 'id' ).stringValue );
				} else {
					Debug.Log( 'null frame state' );
				}*/
				break;
				
			case PanelComponent.PanelCamera:
				CameraState cs = null;
				cameraState = null;
				cs = panel.CurrentCameraState();
				if ( getLastKeyIfNull && ( cs == null ) ) {
					cs = panel.GetStateAtScriptIndex( PanelComponent.PanelCamera, cameraStateScriptProp.arraySize - 1 ) as CameraState;
				}
				if ( cs != null ) {
					index = panel.GetStateIndexForId( PanelComponent.PanelCamera, cs.id );
					cameraState = cameraStatesProp.GetArrayElementAtIndex( index );
				}	
				break;
				
			case PanelComponent.PanelPassiveMotion:
				PassiveMotionState pms = null;
				passiveMotionStateH = null;
				passiveMotionStateV = null;
				pms = panel.CurrentPassiveMotionState( PanelComponent.PanelPassiveMotionH );
				if ( getLastKeyIfNull && ( pms == null ) ) {
					pms = panel.GetStateAtScriptIndex( PanelComponent.PanelPassiveMotion, passiveMotionStateScriptProp.arraySize - 1 ) as PassiveMotionState;
				}
				if ( pms != null ) {
					index = panel.GetStateIndexForId( PanelComponent.PanelPassiveMotion, pms.id );
					//Debug.Log( 'script: ' + scriptIndexProp.intValue + ' index: ' + index + ' id: ' + pms.id + ' ' + passiveMotionStatesHProp.arraySize );
					passiveMotionStateH = passiveMotionStatesHProp.GetArrayElementAtIndex( index );
					passiveMotionStateV = passiveMotionStatesVProp.GetArrayElementAtIndex( index );
				}	
				
				/*if ( passiveMotionStateH != null ) {
					Debug.Log( 'passiveMotion states: ' + passiveMotionStateH.FindPropertyRelative( 'id' ).stringValue + ' ' + passiveMotionStateV.FindPropertyRelative( 'id' ).stringValue );
				} else {
					Debug.Log( 'null passiveMotion state' );
				}*/
				break;
				
			}
			
			serializedObject.Update();
		}
		
		public void PopulateFrameState( SerializedProperty fs ) {
			
			fs.FindPropertyRelative( "vPositionType" ).enumValueIndex = ( int )PositionType.Edge;
			fs.FindPropertyRelative( "vEdge" ).enumValueIndex = ( int )PositionEdgeVert.Top;
			fs.FindPropertyRelative( "topUnits" ).enumValueIndex = ( int )PositionUnits.Percent;
			fs.FindPropertyRelative( "v" ).floatValue = frameState.FindPropertyRelative( "topVal" ).floatValue = ( topProp.intValue * ( 1.0f / rowsProp.intValue )) * 100;
			
			fs.FindPropertyRelative( "hPositionType" ).enumValueIndex = ( int )PositionType.Edge;
			fs.FindPropertyRelative( "hEdge" ).enumValueIndex = ( int )PositionEdgeHorz.Left;
			fs.FindPropertyRelative( "leftUnits" ).enumValueIndex = ( int )PositionUnits.Percent;
			fs.FindPropertyRelative( "h" ).floatValue = frameState.FindPropertyRelative( "leftVal" ).floatValue = ( leftProp.intValue * ( 1.0f / columnsProp.intValue )) * 100;
			
			fs.FindPropertyRelative( "widthUnits" ).enumValueIndex = ( int )WidthUnits.PercentParentWidth;
			fs.FindPropertyRelative( "widthVal" ).floatValue = ( widthProp.intValue * ( 1.0f / columnsProp.intValue )) * 100;
			
			fs.FindPropertyRelative( "heightUnits" ).enumValueIndex = ( int )HeightUnits.PercentParentHeight;
			fs.FindPropertyRelative( "heightVal" ).floatValue = ( heightProp.intValue * ( 1.0f / rowsProp.intValue )) * 100;
			
		}

		public void LogStates(PanelComponent component) {

			int i = 0;
			int n = 0;
			string content;

			SetPropertiesForComponent(component);

			string componentName = null;
			switch (component) {

			case PanelComponent.PanelFrame:
				componentName = "FRAME";
				break;

			case PanelComponent.PanelCamera:
				componentName = "CAMERA";
				break;

			case PanelComponent.PanelPassiveMotionH:
				componentName = "PASSIVE MOTION H";
				break;
				
			case PanelComponent.PanelPassiveMotionV:
				componentName = "PASSIVE MOTION V";
				break;

			}

			Debug.Log ("----------");

			content = componentName + " STATES\n";
			n = statesProp.arraySize;
			for( i = 0; i < n; i++ ) {
				content += i + ": " + statesProp.GetArrayElementAtIndex(i).FindPropertyRelative("id").stringValue + "\n";
			}
			Debug.Log (content);

			Debug.Log ( "--" );
			
			content = componentName + " STATE SCRIPT INDICES\n";
			n = scriptStateIndicesProp.arraySize;
			for( i = 0; i < n; i++ ) {
				content += i + ": " + scriptStateIndicesProp.GetArrayElementAtIndex(i).intValue + "\n";
			}
			Debug.Log (content);
			
			Debug.Log ( "--" );

			content = componentName + " STATE SCRIPT\n"; 
			n = stateScriptProp.arraySize;
			for( i = 0; i < n; i++ ) {
				content += i + ": " + stateScriptProp.GetArrayElementAtIndex(i).stringValue + "\n";
			}
			Debug.Log (content);

		}

		private void InsertStateForComponent(PanelComponent component) {
			SerializedKeyframeTimeline timeline = new SerializedKeyframeTimeline(panel, component);
			timeline.InsertState();
			UpdateState( component, false );
		}

		private void DeleteCurrentStateForComponent(PanelComponent component) {
			SerializedKeyframeTimeline timeline = new SerializedKeyframeTimeline(panel, component);
			timeline.DeleteCurrentState();
			UpdateState( component, false );
		}
		
		public Rect ConstrainPanelRect( Rect panelRect, Rect gridRect ) {
			float diff = panelRect.x - gridRect.x;
			if ( diff < 0 ) {
				panelRect.x = gridRect.x;
				panelRect.width += diff;
			}
			diff = panelRect.y - gridRect.y;
			if ( diff < 0 ) {
				panelRect.y = gridRect.y;
				panelRect.height += diff;
			}
			panelRect.width = Mathf.Min( panelRect.x + panelRect.width, gridRect.x + gridRect.width + 1 ) - panelRect.x;
			panelRect.height = Mathf.Min( panelRect.y + panelRect.height, gridRect.y + gridRect.height + 1 ) - panelRect.y;
			if (( panelRect.x < ( gridRect.x + gridRect.width )) && ( panelRect.y < ( gridRect.y + gridRect.height )) && (( panelRect.x + panelRect.width ) > gridRect.x ) && (( panelRect.y + panelRect.height ) > gridRect.y )) {
				return panelRect;
			} else {
				return Rect.zero;
			}
		}

		private void SetPropertiesForComponent(PanelComponent component) {
			switch (component) {
				
			case PanelComponent.PanelFrame:
				statesProp = frameStatesProp;
				stateScriptProp = frameStateScriptProp;
				scriptStateIndicesProp = frameScriptStateIndicesProp;
				stateCounterProp = frameStateCounterProp;
				holdCommand = FrameState.HoldCommand;
				scriptStepOptions = frameScriptStepOptions;
				break;
				
			case PanelComponent.PanelCamera:
				statesProp = cameraStatesProp;
				stateScriptProp = cameraStateScriptProp;
				scriptStateIndicesProp = cameraScriptStateIndicesProp;
				stateCounterProp = cameraStateCounterProp;
				holdCommand = CameraState.HoldCommand;
				scriptStepOptions = cameraScriptStepOptions;
				break;
				
			case PanelComponent.PanelPassiveMotion:
				stateScriptProp = passiveMotionStateScriptProp;
				scriptStateIndicesProp = passiveMotionScriptStateIndicesProp;
				holdCommand = PassiveMotionState.HoldCommand;
				scriptStepOptions = passiveMotionScriptStepOptions;
				break;
				
			case PanelComponent.PanelPassiveMotionH:
				statesProp = passiveMotionStatesHProp;
				stateScriptProp = passiveMotionStateScriptProp;
				scriptStateIndicesProp = passiveMotionScriptStateIndicesProp;
				stateCounterProp = passiveMotionHStateCounterProp;
				holdCommand = PassiveMotionState.HoldCommand;
				scriptStepOptions = passiveMotionScriptStepOptions;
				break;
				
			case PanelComponent.PanelPassiveMotionV:
				statesProp = passiveMotionStatesVProp;
				stateScriptProp = passiveMotionStateScriptProp;
				scriptStateIndicesProp = passiveMotionScriptStateIndicesProp;
				stateCounterProp = passiveMotionVStateCounterProp;
				holdCommand = PassiveMotionState.HoldCommand;
				scriptStepOptions = passiveMotionScriptStepOptions;
				break;

			}

		}

		private void DrawKeyframeTimelines() {

			if (PanoplyCore.scene != null) {

				GUI.skin = currentKeyframeSkin;

				float baseWidth = Mathf.Max (Mathf.Max( frameTimelineRect.width, cameraTimelineRect.width), passiveMotionTimelineRect.width);
				float unitWidth = (baseWidth - 5.5f - 5) / (PanoplyCore.scene.stepCount - 1);

				SetPropertiesForComponent(PanelComponent.PanelFrame);
				
				int n = Mathf.Min (stateScriptProp.arraySize, PanoplyCore.scene.stepCount);
				int i;
				for (i=0; i<n; i += 5) {
					
					// background grid
					GUI.skin.label = currentKeyframeSkin.horizontalSlider;
					if (i < (n - 1)) {
						gridRect.width = unitWidth * (Math.Min(i + 5, n - 1) - i) + 1;
						if (isEditingFrameProp.boolValue) {
							gridRect.x = frameTimelineRect.x + (unitWidth * i) + 5.5f;
							gridRect.y = frameTimelineRect.y;
							GUI.Label( gridRect, "" );
						}
						if (isEditingCameraProp.boolValue) {
							gridRect.x = cameraTimelineRect.x + (unitWidth * i) + 5.5f;
							gridRect.y = cameraTimelineRect.y;
							GUI.Label( gridRect, "" );
						}
						if (isEditingPassiveMotionProp.boolValue) {
							gridRect.x = passiveMotionTimelineRect.x + (unitWidth * i) + 5.5f;
							gridRect.y = passiveMotionTimelineRect.y;
							GUI.Label( gridRect, "" );
						}
					}

				}
			
				// current step marker
				GUI.skin.label = currentKeyframeSkin.window;
				frameRect.x = frameTimelineRect.x - 1 + (unitWidth * scriptIndexProp.intValue) + 5.5f;
				if (isEditingFrameProp.boolValue) {
					frameRect.y = frameTimelineRect.y;
					GUI.Label(frameRect, "" );
				}
				if (isEditingCameraProp.boolValue) {
					frameRect.y = cameraTimelineRect.y;
					GUI.Label(frameRect, "" );
				}
				if (isEditingPassiveMotionProp.boolValue) {
					frameRect.y = passiveMotionTimelineRect.y;
					GUI.Label(frameRect, "" );
				}
					
				for (i=0; i<n; i++) {

					if ( PanoplyCore.targetStep == i ) {
						GUI.color = currentHighlightColor;
					}
					
					// inactive keyframes
					GUI.skin.label = currentKeyframeSkin.textField;
					markerRect.x = frameTimelineRect.x + 1 + (unitWidth * i);
					if (isEditingFrameProp.boolValue && (frameStateScriptProp.GetArrayElementAtIndex( i ).stringValue != FrameState.HoldCommand) && ((i != draggedKeyframeIndex) || (draggedKeyframeComponent != PanelComponent.PanelFrame))) {
						markerRect.y = frameTimelineRect.y + 5.5f;
						GUI.Label(markerRect, "" );
					}
					if (isEditingCameraProp.boolValue && (cameraStateScriptProp.GetArrayElementAtIndex( i ).stringValue != CameraState.HoldCommand) && ((i != draggedKeyframeIndex) || (draggedKeyframeComponent != PanelComponent.PanelCamera))) {
						markerRect.y = cameraTimelineRect.y + 5.5f;
						GUI.Label(markerRect, "" );
					}
					if (isEditingPassiveMotionProp.boolValue && (passiveMotionStateScriptProp.GetArrayElementAtIndex( i ).stringValue != PassiveMotionState.HoldCommand) && ((i != draggedKeyframeIndex) || (draggedKeyframeComponent != PanelComponent.PanelPassiveMotion))) {
						markerRect.y = passiveMotionTimelineRect.y + 5.5f;
						GUI.Label(markerRect, "" );
					}

					GUI.color = Color.white;
				}
				
				// active keyframe
				if (draggedKeyframeIndex != -1) {
					switch (draggedKeyframeComponent) {

					case PanelComponent.PanelFrame:
						activeKeyframeRect = frameTimelineRect;
						break;

					case PanelComponent.PanelCamera:
						activeKeyframeRect = cameraTimelineRect;
						break;

					case PanelComponent.PanelPassiveMotion:
						activeKeyframeRect = passiveMotionTimelineRect;
						break;

					}
					GUI.skin.label = currentKeyframeSkin.textArea;
					markerRect.x = activeKeyframeRect.x + 1 + (unitWidth * draggedKeyframePosition);
					markerRect.y = activeKeyframeRect.y + 5.5f;
					GUI.Label(markerRect, "" );
				}

				GUI.skin = null;
			}

		}

		private void SetCurrentStep(int step) {
			editorScrubberScriptIndex = PanoplyCore.interpolatedStep = scriptIndexProp.intValue = PanoplyCore.targetStep = step;
		}
		
		private void KeyframeEditor(PanelComponent component) {

			if (PanoplyCore.scene != null) {

				GUI.skin = currentKeyframeSkin;
				
				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();
				
				EditorGUILayout.Space();
				Rect r = EditorGUILayout.BeginHorizontal();
				r.width -= 117;
				GUILayout.Box("", GUILayout.Height( 22 ), GUILayout.ExpandWidth( true ) );

				// jank
				if (r.x == 0) {
					r.width -= 14;
					r.x = 14;
				}

				GUI.skin = null;

				string subComponentName = null;
				switch (component) {

				case PanelComponent.PanelFrame:
					subComponentName = "Frame";
					frameTimelineRect = r;
					break;

				case PanelComponent.PanelCamera:
					subComponentName = "Camera";
					cameraTimelineRect = r;
					break;

				case PanelComponent.PanelPassiveMotion:
					subComponentName = "Passive motion";
					passiveMotionTimelineRect = r;
					break;

				}
				
				SetPropertiesForComponent(component);

				bool isOnFirstStep = ( scriptIndexProp.intValue == 0 );
				bool isOnLastStep = ( scriptIndexProp.intValue == (PanoplyCore.scene.stepCount - 1));

				GUI.enabled = !isOnFirstStep;
				if ( stateScriptProp.GetArrayElementAtIndex( scriptIndexProp.intValue ).stringValue != holdCommand ) {
					GUI.color = new Color( .78f, .87f, .94f, 1 );
				}
				if ( GUILayout.Button( "Key", GUILayout.Width(35), GUILayout.Height(22) )) {
					if ( stateScriptProp.GetArrayElementAtIndex( scriptIndexProp.intValue ).stringValue == holdCommand ) {
						SetKeyframeStateTypeForComponent(StateType.Key, component);
					} else {
						SetKeyframeStateTypeForComponent(StateType.Hold, component);
					}
				}
				GUI.color = Color.white;

				GUI.enabled = !isOnLastStep;
				if ( GUILayout.Button( "Ins", GUILayout.Width(35), GUILayout.Height(22) )) {
					int option = EditorUtility.DisplayDialogComplex("Insert step locally or globally?", "Do you want to insert a step in the panel's " + subComponentName.ToLower() + " timeline only, or across the entire project?", subComponentName + " timeline only", "Cancel", "Entire project");
					if (option == 0) {
						InsertStateForComponent(component);
					} else if (option == 2) {
						PanoplyCore.InsertStateGlobally();
					}
				}
				
				GUI.enabled = !isOnFirstStep;
				if ( GUILayout.Button( "Del", GUILayout.Width(35), GUILayout.Height(22) ) ) {
					int option = EditorUtility.DisplayDialogComplex("Delete step locally or globally?", "Do you want to delete a step in the panel's " + subComponentName.ToLower() + " timeline only, or across the entire project?", subComponentName + " timeline only", "Cancel", "Entire project");
					if (option == 0) {
						DeleteCurrentStateForComponent(component);
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
				if ((draggedKeyframeIndex != -1) && (draggedKeyframeComponent == component)) {
					GUI.skin.label = currentKeyframeSkin.textArea;
					markerRect.x = r.x + 1 + (unitWidth * draggedKeyframePosition);
					markerRect.y = r.y + 5.5f;
					GUI.Label(markerRect, "" );
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
								if (stateScriptProp.GetArrayElementAtIndex(keyframeIndex).stringValue != holdCommand) {
									draggedKeyframePosition = draggedKeyframeIndex = keyframeIndex;
									draggedKeyframeComponent = component;
								}
							}
							draggedAwayFromInitialStep = false;
							break;
							
						case EventType.MouseDrag:
							if ((draggedKeyframeIndex != -1) && (draggedKeyframeComponent == component)) {
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
								if (draggedKeyframeComponent == component) {
									if (draggedKeyframePosition != draggedKeyframeIndex) {
										scriptStateIndicesProp.GetArrayElementAtIndex(draggedKeyframePosition).intValue = scriptStateIndicesProp.GetArrayElementAtIndex(draggedKeyframeIndex).intValue;
										stateScriptProp.GetArrayElementAtIndex(draggedKeyframePosition).stringValue = scriptStepOptions[ scriptStateIndicesProp.GetArrayElementAtIndex(draggedKeyframeIndex).intValue ];
										scriptStateIndicesProp.GetArrayElementAtIndex(draggedKeyframeIndex).intValue = 0;
										stateScriptProp.GetArrayElementAtIndex(draggedKeyframeIndex).stringValue = holdCommand;
										UpdateScriptStepOptions( component );
									} else {
										SetCurrentStep(stepIndex);
										if (!draggedAwayFromInitialStep && (stepIndexPriorToMouseDown == mouseDownStepIndex) && (mouseDownStepIndex != 0)) {
											SetKeyframeStateTypeForComponent(StateType.Hold, component);
										}
									}
								}
							} else {
								if ((stepIndexPriorToMouseDown == mouseDownStepIndex) && (mouseDownStepIndex != 0) && !draggedAwayFromInitialStep) {
									SetCurrentStep(stepIndex);
									SetKeyframeStateTypeForComponent(StateType.Key, component);
								} else {
									SetCurrentStep(stepIndex);
								}
							}
							//LogStates(component);
							mouseDownStepIndex = draggedKeyframePosition = draggedKeyframeIndex = -1;
							Repaint();
							break;
							
						}
					} else {
						switch ( Event.current.type ) {
						case EventType.MouseUp:
							if ((draggedKeyframeComponent == component) && (draggedKeyframeIndex != -1)) {
								mouseDownStepIndex = draggedKeyframePosition = draggedKeyframeIndex = -1;
								Repaint();
							}
							break;
						}
					}
				}
				
				GUI.skin = null;

			}
		}

		private void AddStateForComponent(PanelComponent component) {
			SetPropertiesForComponent(component);
			SerializedProperty stateProp;
			statesProp.arraySize++;
			do {
				stateCounterProp.intValue++;
			} while ( stateCounterProp.intValue <= 1 );
			stateProp = statesProp.GetArrayElementAtIndex( statesProp.arraySize - 1 );
			stateProp.FindPropertyRelative( "id" ).stringValue = "State " + stateCounterProp.intValue;
			UpdateState( component, true );
		}

		private void SetKeyForComponent(PanelComponent component) {

			SerializedProperty stateProp;

			switch (component) {

			case PanelComponent.PanelFrame:
				AddStateForComponent(component);
				stateProp = statesProp.GetArrayElementAtIndex( statesProp.arraySize - 1 );
				stateProp.FindPropertyRelative( "hPositionType" ).enumValueIndex = frameState.FindPropertyRelative( "hPositionType" ).enumValueIndex;
				stateProp.FindPropertyRelative( "vPositionType" ).enumValueIndex = frameState.FindPropertyRelative( "vPositionType" ).enumValueIndex;
				stateProp.FindPropertyRelative( "hEdge" ).enumValueIndex = frameState.FindPropertyRelative( "hEdge" ).enumValueIndex;
				stateProp.FindPropertyRelative( "h" ).floatValue = frameState.FindPropertyRelative( "h" ).floatValue;
				stateProp.FindPropertyRelative( "leftVal" ).floatValue = frameState.FindPropertyRelative( "leftVal" ).floatValue;
				stateProp.FindPropertyRelative( "rightVal" ).floatValue = frameState.FindPropertyRelative( "rightVal" ).floatValue;
				stateProp.FindPropertyRelative( "v" ).floatValue = frameState.FindPropertyRelative( "v" ).floatValue;
				stateProp.FindPropertyRelative( "topVal" ).floatValue = frameState.FindPropertyRelative( "topVal" ).floatValue;
				stateProp.FindPropertyRelative( "bottomVal" ).floatValue = frameState.FindPropertyRelative( "bottomVal" ).floatValue;
				stateProp.FindPropertyRelative( "widthVal" ).floatValue = frameState.FindPropertyRelative( "widthVal" ).floatValue;
				stateProp.FindPropertyRelative( "heightVal" ).floatValue = frameState.FindPropertyRelative( "heightVal" ).floatValue;
				stateProp.FindPropertyRelative( "widthUnits" ).enumValueIndex = frameState.FindPropertyRelative( "widthUnits" ).enumValueIndex;
				stateProp.FindPropertyRelative( "heightUnits" ).enumValueIndex = frameState.FindPropertyRelative( "heightUnits" ).enumValueIndex;
				stateProp.FindPropertyRelative( "minAspectRatioVal" ).floatValue = frameState.FindPropertyRelative( "minAspectRatioVal" ).floatValue;
				stateProp.FindPropertyRelative( "maxAspectRatioVal" ).floatValue = frameState.FindPropertyRelative( "maxAspectRatioVal" ).floatValue;
				stateProp.FindPropertyRelative( "marginTopVal" ).floatValue = frameState.FindPropertyRelative( "marginTopVal" ).floatValue;
				stateProp.FindPropertyRelative( "marginBottomVal" ).floatValue = frameState.FindPropertyRelative( "marginBottomVal" ).floatValue;
				stateProp.FindPropertyRelative( "marginLeftVal" ).floatValue = frameState.FindPropertyRelative( "marginLeftVal" ).floatValue;
				stateProp.FindPropertyRelative( "marginRightVal" ).floatValue = frameState.FindPropertyRelative( "marginRightVal" ).floatValue;
				stateProp.FindPropertyRelative( "hAlignType" ).enumValueIndex = frameState.FindPropertyRelative( "hAlignType" ).enumValueIndex;
				stateProp.FindPropertyRelative( "vAlignType" ).enumValueIndex = frameState.FindPropertyRelative( "vAlignType" ).enumValueIndex;
				stateProp.FindPropertyRelative( "matteColor" ).colorValue = frameState.FindPropertyRelative( "matteColor" ).colorValue;
				break;

			case PanelComponent.PanelCamera:
				AddStateForComponent(component);
				stateProp = statesProp.GetArrayElementAtIndex( statesProp.arraySize - 1 );
				stateProp.FindPropertyRelative( "position" ).vector3Value = cameraState.FindPropertyRelative( "position" ).vector3Value;
				stateProp.FindPropertyRelative( "rotation" ).vector3Value = cameraState.FindPropertyRelative( "rotation" ).vector3Value;
				stateProp.FindPropertyRelative( "fieldOfViewVal" ).floatValue = cameraState.FindPropertyRelative( "fieldOfViewVal" ).floatValue;
				stateProp.FindPropertyRelative( "lookAt" ).vector3Value = cameraState.FindPropertyRelative( "lookAt" ).vector3Value;
				stateProp.FindPropertyRelative( "orientationType" ).enumValueIndex = cameraState.FindPropertyRelative( "orientationType" ).enumValueIndex;
				break;

			case PanelComponent.PanelPassiveMotion:
				AddStateForComponent(PanelComponent.PanelPassiveMotionH);
				AddStateForComponent(PanelComponent.PanelPassiveMotionV);

				SetPropertiesForComponent(PanelComponent.PanelPassiveMotionH);
				stateProp = statesProp.GetArrayElementAtIndex( statesProp.arraySize - 1 );
				stateProp.FindPropertyRelative( "input" ).enumValueIndex = passiveMotionStateH.FindPropertyRelative( "input" ).enumValueIndex;
				stateProp.FindPropertyRelative( "inputMinimumVal" ).floatValue = passiveMotionStateH.FindPropertyRelative( "inputMinimumVal" ).floatValue;
				stateProp.FindPropertyRelative( "inputMaximumVal" ).floatValue = passiveMotionStateH.FindPropertyRelative( "inputMaximumVal" ).floatValue;
				stateProp.FindPropertyRelative( "outputMinimumVal" ).floatValue = passiveMotionStateH.FindPropertyRelative( "outputMinimumVal" ).floatValue;
				stateProp.FindPropertyRelative( "outputMaximumVal" ).floatValue = passiveMotionStateH.FindPropertyRelative( "outputMaximumVal" ).floatValue;
				
				SetPropertiesForComponent(PanelComponent.PanelPassiveMotionV);
				stateProp = statesProp.GetArrayElementAtIndex( statesProp.arraySize - 1 );
				stateProp.FindPropertyRelative( "input" ).enumValueIndex = passiveMotionStateV.FindPropertyRelative( "input" ).enumValueIndex;
				stateProp.FindPropertyRelative( "inputMinimumVal" ).floatValue = passiveMotionStateV.FindPropertyRelative( "inputMinimumVal" ).floatValue;
				stateProp.FindPropertyRelative( "inputMaximumVal" ).floatValue = passiveMotionStateV.FindPropertyRelative( "inputMaximumVal" ).floatValue;
				stateProp.FindPropertyRelative( "outputMinimumVal" ).floatValue = passiveMotionStateV.FindPropertyRelative( "outputMinimumVal" ).floatValue;
				stateProp.FindPropertyRelative( "outputMaximumVal" ).floatValue = passiveMotionStateV.FindPropertyRelative( "outputMaximumVal" ).floatValue;
				break;

			}

			UpdateScriptStepOptions( component );
			while ( scriptIndexProp.intValue >= stateScriptProp.arraySize ) {
				stateScriptProp.arraySize++;
				scriptStateIndicesProp.arraySize++;
				scriptStateIndicesProp.GetArrayElementAtIndex( scriptStateIndicesProp.arraySize - 1 ).intValue = 0;
				stateScriptProp.GetArrayElementAtIndex( stateScriptProp.arraySize - 1 ).stringValue = scriptStepOptions[ scriptStateIndicesProp.GetArrayElementAtIndex( scriptStateIndicesProp.arraySize - 1 ).intValue ];
			}
			scriptStateIndicesProp.GetArrayElementAtIndex( scriptIndexProp.intValue ).intValue = scriptStepOptions.Count - 1;
			stateScriptProp.GetArrayElementAtIndex( scriptIndexProp.intValue ).stringValue = scriptStepOptions[ scriptStateIndicesProp.GetArrayElementAtIndex( scriptIndexProp.intValue ).intValue ];
			UpdateState( component, false );
		}

		private void SetHoldForComponent(PanelComponent component) {
			bool okToSetHold = false;

			if ( scriptIndexProp.intValue == 0 ) {
				EditorUtility.DisplayDialog( "Can’t delete first key", "The first step must be a key.", "OK" );
			} else {
				okToSetHold = true;
				if ( scriptIndexProp.intValue < stateScriptProp.arraySize ) {
					if ( stateScriptProp.GetArrayElementAtIndex( scriptIndexProp.intValue ).stringValue != holdCommand ) {
						SerializedKeyframeTimeline timeline = new SerializedKeyframeTimeline(panel, component);
						timeline.DeleteStateAtIndex(scriptStateIndicesProp.GetArrayElementAtIndex( scriptIndexProp.intValue ).intValue - 1); // - 1 because 'hold' is at index 0
						UpdateState( component, false );
						scriptStateIndicesProp.GetArrayElementAtIndex( scriptIndexProp.intValue ).intValue = 0;
						stateScriptProp.GetArrayElementAtIndex( scriptIndexProp.intValue ).stringValue = holdCommand;
						UpdateScriptStepOptions( component );
					}
				}
				if ( okToSetHold ) {
					while ( scriptIndexProp.intValue >= stateScriptProp.arraySize ) {
						stateScriptProp.arraySize++;
						scriptStateIndicesProp.arraySize++;
						scriptStateIndicesProp.GetArrayElementAtIndex( scriptStateIndicesProp.arraySize - 1 ).intValue = 0;
						stateScriptProp.GetArrayElementAtIndex( stateScriptProp.arraySize - 1 ).stringValue = holdCommand;
					}
					UpdateState( component, false );
				}
			}
		}

		private void SetEndForComponent(PanelComponent component) {
			/*if ( scriptIndexProp.intValue == 0 ) {
				EditorUtility.DisplayDialog( 'Can’t delete first key', 'The first step must be a key.', 'OK' );
			} else {
				if ( EditorUtility.DisplayDialog( 'Remove keys and holds?', 'Setting this step to "End" will delete any of its keys and holds from this step forward. Are you sure you want to do this?', 'OK', 'Cancel' ) ) {
					for ( i = ( stateScriptProp.arraySize - 1 ); i >= scriptIndexProp.intValue; i-- ) {
						if ( stateScriptProp.GetArrayElementAtIndex( i ).stringValue != holdCommand ) {
							deleteState( component, acriptStateIndicesProp.GetArrayElementAtIndex( i ).intValue - 1 ); // - 1 because 'hold' is at index 0
							updateScriptStepOptions( component );
						}
						stateScriptProp.DeleteArrayElementAtIndex( i );
					}
					updateState( component, false );
				}
			}*/
		}

		private void SetKeyframeStateTypeForComponent(StateType stateType, PanelComponent component) {

			SetPropertiesForComponent(component);

			switch ( stateType ) {
				
			case StateType.Key:
				SetKeyForComponent(component);
				break;
				
			case StateType.Hold:
				SetHoldForComponent(component);
				break;
				
			case StateType.End:
				SetEndForComponent(component);
				break;
				
			}
			
			//LogStates(component);

		}
		
		public override void OnInspectorGUI() {
			
			Event e = Event.current;
			
			serializedObject.Update();
			
			EditorGUILayout.Space();
			
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField( "Global timeline", GUILayout.Width(132.0f) );
			if (PanoplyCore.scene != null) {
				editorScrubberScriptIndex = EditorGUILayout.Slider( "", editorScrubberScriptIndex, 0.0f, ( float )( PanoplyCore.scene.stepCount - 1 ) );
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
			EditorGUILayout.EndHorizontal();
			
			EditorGUILayout.Space();
			
			if ( frameStatesProp.arraySize == 0 ) {
				frameStatesProp.arraySize++;
				frameStateCounterProp.intValue++;
				frameState = frameStatesProp.GetArrayElementAtIndex( 0 );
				frameState.FindPropertyRelative( "hPositionType" ).enumValueIndex = ( int )PositionType.Center;
				frameState.FindPropertyRelative( "vPositionType" ).enumValueIndex = ( int )PositionType.Center;
				frameState.FindPropertyRelative( "hEdge" ).enumValueIndex = ( int )PositionEdgeHorz.Left;
				frameState.FindPropertyRelative( "h" ).floatValue = frameState.FindPropertyRelative( "leftVal" ).floatValue = frameState.FindPropertyRelative( "rightVal" ).floatValue = 0.0f;
				frameState.FindPropertyRelative( "v" ).floatValue = frameState.FindPropertyRelative( "topVal" ).floatValue = frameState.FindPropertyRelative( "bottomVal" ).floatValue = 0.0f;
				frameState.FindPropertyRelative( "id" ).stringValue = "State " + frameStateCounterProp.intValue;
				frameState.FindPropertyRelative( "widthVal" ).floatValue = 100.0f;
				frameState.FindPropertyRelative( "heightVal" ).floatValue = 100.0f;
				frameState.FindPropertyRelative( "widthUnits" ).enumValueIndex = 1;
				frameState.FindPropertyRelative( "heightUnits" ).enumValueIndex = 1;
				UpdateScriptStepOptions( PanelComponent.PanelFrame );
			}
			
			if ( frameStateScriptProp.arraySize == 0 ) {
				frameStateScriptProp.arraySize++;
				frameScriptStateIndicesProp.arraySize++;
				frameScriptStateIndicesProp.GetArrayElementAtIndex( 0 ).intValue = 1;
				frameStateScriptProp.GetArrayElementAtIndex( 0 ).stringValue = frameScriptStepOptions[ frameScriptStateIndicesProp.GetArrayElementAtIndex( 0 ).intValue ];
			}
			
			// fill empty steps with Hold commands
			if (PanoplyCore.scene != null) {
				while ( frameStateScriptProp.arraySize < PanoplyCore.scene.stepCount ) {
					frameStateScriptProp.arraySize++;
					frameScriptStateIndicesProp.arraySize++;
					frameScriptStateIndicesProp.GetArrayElementAtIndex( frameStateScriptProp.arraySize - 1 ).intValue = frameStateScriptProp.arraySize;
					frameStateScriptProp.GetArrayElementAtIndex( frameStateScriptProp.arraySize - 1 ).stringValue = FrameState.HoldCommand;
				}
			}
			
			if ( cameraStatesProp.arraySize == 0 ) {
				cameraStatesProp.arraySize++;
				cameraStateCounterProp.intValue++;
				cameraState = cameraStatesProp.GetArrayElementAtIndex( 0 );
				cameraState.FindPropertyRelative( "id" ).stringValue = "State " + cameraStateCounterProp.intValue;
				cameraState.FindPropertyRelative( "fieldOfViewVal" ).floatValue = 60.0f;
				UpdateScriptStepOptions( PanelComponent.PanelCamera );
			}
			
			if ( cameraStateScriptProp.arraySize == 0 ) {
				cameraStateScriptProp.arraySize++;
				cameraScriptStateIndicesProp.arraySize++;
				cameraScriptStateIndicesProp.GetArrayElementAtIndex( 0 ).intValue = 1;
				cameraStateScriptProp.GetArrayElementAtIndex( 0 ).stringValue = cameraScriptStepOptions[ cameraScriptStateIndicesProp.GetArrayElementAtIndex( 0 ).intValue ];
			}
			
			// fill empty steps with Hold commands
			if (PanoplyCore.scene != null) {
				while ( cameraStateScriptProp.arraySize < PanoplyCore.scene.stepCount ) {
					cameraStateScriptProp.arraySize++;
					cameraScriptStateIndicesProp.arraySize++;
					cameraScriptStateIndicesProp.GetArrayElementAtIndex( cameraStateScriptProp.arraySize - 1 ).intValue = cameraStateScriptProp.arraySize;
					cameraStateScriptProp.GetArrayElementAtIndex( cameraStateScriptProp.arraySize - 1 ).stringValue = CameraState.HoldCommand;
				}
			}
			
			if ( passiveMotionStatesHProp.arraySize == 0 ) {
				
				passiveMotionStatesHProp.arraySize++;
				passiveMotionHStateCounterProp.intValue++;
				passiveMotionStateH = passiveMotionStatesHProp.GetArrayElementAtIndex( 0 );
				passiveMotionStateH.FindPropertyRelative( "id" ).stringValue = "State " + passiveMotionHStateCounterProp.intValue;
				passiveMotionStateH.FindPropertyRelative( "input" ).enumValueIndex = ( int )PassiveMotionControllerInput.HorizontalTilt;
				passiveMotionStateH.FindPropertyRelative( "outputProperty" ).enumValueIndex = ( int )PassiveMotionOutputProperty.X;
				passiveMotionStateH.FindPropertyRelative( "inputMinimumVal" ).floatValue = -1.0f;
				passiveMotionStateH.FindPropertyRelative( "inputMaximumVal" ).floatValue = 1.0f;
				passiveMotionStateH.FindPropertyRelative( "outputMinimumVal" ).floatValue = -1.0f;
				passiveMotionStateH.FindPropertyRelative( "outputMaximumVal" ).floatValue = 1.0f;
				
				passiveMotionStatesVProp.arraySize++;
				passiveMotionVStateCounterProp.intValue++;
				passiveMotionStateV = passiveMotionStatesVProp.GetArrayElementAtIndex( 0 );
				passiveMotionStateV.FindPropertyRelative( "id" ).stringValue = "State " + passiveMotionVStateCounterProp.intValue;
				passiveMotionStateV.FindPropertyRelative( "input" ).enumValueIndex = ( int )PassiveMotionControllerInput.VerticalTilt;
				passiveMotionStateV.FindPropertyRelative( "outputProperty" ).enumValueIndex = ( int )PassiveMotionOutputProperty.Y;
				passiveMotionStateV.FindPropertyRelative( "inputMinimumVal" ).floatValue = -1.0f;
				passiveMotionStateV.FindPropertyRelative( "inputMaximumVal" ).floatValue = 1.0f;
				passiveMotionStateV.FindPropertyRelative( "outputMinimumVal" ).floatValue = -1.0f;
				passiveMotionStateV.FindPropertyRelative( "outputMaximumVal" ).floatValue = 1.0f;
				
				UpdateScriptStepOptions( PanelComponent.PanelPassiveMotion );
			}
			
			if ( passiveMotionStateScriptProp.arraySize == 0 ) {
				passiveMotionStateScriptProp.arraySize++;
				passiveMotionScriptStateIndicesProp.arraySize++;
				passiveMotionScriptStateIndicesProp.GetArrayElementAtIndex( 0 ).intValue = 1;
				passiveMotionStateScriptProp.GetArrayElementAtIndex( 0 ).stringValue = passiveMotionScriptStepOptions[ passiveMotionScriptStateIndicesProp.GetArrayElementAtIndex( 0 ).intValue ];
			}
			
			// fill empty steps with Hold commands
			if (PanoplyCore.scene != null) {
				while ( passiveMotionStateScriptProp.arraySize < PanoplyCore.scene.stepCount ) {
					passiveMotionStateScriptProp.arraySize++;
					passiveMotionScriptStateIndicesProp.arraySize++;
					passiveMotionScriptStateIndicesProp.GetArrayElementAtIndex( passiveMotionStateScriptProp.arraySize - 1 ).intValue = passiveMotionStateScriptProp.arraySize;
					passiveMotionStateScriptProp.GetArrayElementAtIndex( passiveMotionStateScriptProp.arraySize - 1 ).stringValue = FrameState.HoldCommand;
				}
			}
			
			// returns the currently active frame state, whether this is a hold or key
			UpdateState( PanelComponent.PanelFrame, false );
			
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField( "Home position", GUILayout.Width(130.0f) );
			serializedObject.FindProperty( "homePosition" ).vector3Value = EditorGUILayout.Vector3Field( "", serializedObject.FindProperty( "homePosition" ).vector3Value );
			EditorGUILayout.EndHorizontal();
			
			/* -- ROWS & COLUMNS -- */
			
			EditorGUI.BeginChangeCheck();
			
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField( "Layout grid", GUILayout.Width(130.0f) );
			EditorGUILayout.LabelField( "Rows", GUILayout.Width(35.0f) );
			rowsProp.intValue = EditorGUILayout.IntField( rowsProp.intValue );
			EditorGUILayout.LabelField( "Cols", GUILayout.Width(35.0f) );
			columnsProp.intValue = EditorGUILayout.IntField( columnsProp.intValue );
			EditorGUILayout.EndHorizontal();
			
			if ( EditorGUI.EndChangeCheck() ) {
				PopulateFrameState( frameState );
			}

			if ( frameState != null ) {
				if (PanoplyCore.panoplyRenderer != null ) {
					if ( PanoplyCore.panoplyRenderer.screenRect.width > 0 ) {
						//float hGridUnit = PanoplyCore.screenRect.width / columnsProp.intValue;
						//float vGridUnit = PanoplyCore.screenRect.height / rowsProp.intValue;
						if (rowsProp.intValue > 0) {
							topProp.intValue = ( int )Mathf.Round( ( frameState.FindPropertyRelative( "v" ).floatValue * .01f ) / ( 1.0f / rowsProp.intValue ) );
							heightProp.intValue = ( int )Mathf.Round( ( frameState.FindPropertyRelative( "heightVal" ).floatValue * .01f ) / ( 1.0f / rowsProp.intValue ) );
						}
						if (columnsProp.intValue > 0) {
							leftProp.intValue = ( int )Mathf.Round( ( frameState.FindPropertyRelative( "h" ).floatValue * .01f ) / ( 1.0f / columnsProp.intValue ) );
							widthProp.intValue = ( int )Mathf.Round( ( frameState.FindPropertyRelative( "widthVal" ).floatValue * .01f ) / ( 1.0f / columnsProp.intValue ) );
						}
					}
				}
			}

			EditorGUILayout.Space ();

			EditorStyles.foldout.fontStyle = FontStyle.Bold;
			isEditingFrameProp.boolValue = EditorGUILayout.Foldout( isEditingFrameProp.boolValue, "Frame" );
			EditorStyles.foldout.fontStyle = FontStyle.Normal;
			if ( isEditingFrameProp.boolValue && (PanoplyCore.panoplyRenderer != null)) {

				KeyframeEditor(PanelComponent.PanelFrame);

				/* -- VISUAL EDITOR -- */

				GUI.skin = currentGridSkin;
				
				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();

				float screenAR = PanoplyCore.panoplyRenderer.aspectRatio;
				
				EditorGUILayout.Space();
				Rect r = EditorGUILayout.BeginHorizontal();
				GUILayout.Box("", GUILayout.Height( 200 ), GUILayout.ExpandWidth( true ) );
				EditorGUILayout.EndHorizontal();
				
				float editorAR = r.width / 200.0f;

				float newValue;
				if (screenAR > 0) {
					if ( screenAR > editorAR ) {
						newValue = r.width / screenAR;
						r.y += ( r.height - newValue ) * .5f;
						r.height = newValue;
					} else {
						newValue = r.height * screenAR;
						r.x += ( r.width - newValue ) * .5f;
						r.width = newValue;
					}
				}
				float unitWidth = ( r.width - ( gridMargin * 2 )) / (float)columnsProp.intValue;
				float unitHeight = ( r.height - ( gridMargin * 2 )) / (float)rowsProp.intValue;

				panelBaseRect.x = r.x + gridMargin;
				panelBaseRect.y = r.y + gridMargin;
				panelBaseRect.width = r.width - (gridMargin * 2);
				panelBaseRect.height = r.height - (gridMargin * 2);

				GUI.skin.label = currentGridSkin.textArea;

				// first and last columns
				marginRect.x = r.x;
				marginRect.y = r.y + gridMargin;
				marginRect.width = gridMargin + 1.0f;
				marginRect.height = panelBaseRect.height + 1;
				GUI.Label (marginRect, "");
				marginRect.x = r.x + gridMargin + (unitWidth * columnsProp.intValue);
				GUI.Label (marginRect, "");

				// first and last rows
				marginRect.x = r.x + gridMargin;
				marginRect.y = r.y;
				marginRect.width = panelBaseRect.width + 1;
				marginRect.height = gridMargin + 1.0f;
				GUI.Label (marginRect, "");
				marginRect.y = r.y + gridMargin + (unitHeight * rowsProp.intValue);
				GUI.Label (marginRect, "");

				// background
				marginRect.x = panelBaseRect.x;
				marginRect.y = panelBaseRect.y;
				marginRect.width = panelBaseRect.width + 1;
				marginRect.height = panelBaseRect.height + 1;
				GUI.Label (marginRect, "");
				EditorGUILayout.Space();
				
				GUI.skin = null;
				
				// grid = 3,3 / top = 4, height = 2 / left = 4, width = 1
				
				/* -- END VISUAL EDITOR -- */
				
				if ( frameState != null ) {
					
					/* -- ACTIVE PANEL IN VISUAL EDITOR -- */
					
					FrameState ofs = null;
					GUI.skin = currentGridSkin;
					GUI.skin.label = currentGridSkin.toggle;
					foreach ( Panel otherPanel in panels ) {
						if ( otherPanel != panel ) {
							ofs = otherPanel.CurrentFrameState();
							if ( ofs != null ) {
								otherPanel.top = ( int )Mathf.Round( ( ofs.v * .01f ) / ( 1.0f / otherPanel.rows) );
								otherPanel.left = ( int )Mathf.Round( ( ofs.h * .01f ) / ( 1.0f / otherPanel.columns ) );
								otherPanel.width = ( int )Mathf.Round( ( ofs.widthVal * .01f ) / ( 1.0f / otherPanel.columns ) );
								otherPanel.height = ( int )Mathf.Round( ( ofs.heightVal * .01f ) / ( 1.0f / otherPanel.rows ) );
								activeRect.x = panelBaseRect.x + (panelBaseRect.width * (otherPanel.left * (1.0f / otherPanel.columns)));
								activeRect.y = panelBaseRect.y + (panelBaseRect.height * (otherPanel.top * (1.0f / otherPanel.rows)));
								activeRect.width = panelBaseRect.width * (otherPanel.width * (1.0f / otherPanel.columns)) + 1;
								activeRect.height = panelBaseRect.height * (otherPanel.height * (1.0f / otherPanel.rows)) + 1;
								activeRect = ConstrainPanelRect( activeRect, r );
								GUI.Label( activeRect, otherPanel.gameObject.name );
							}
						}
					}
					
					if ( frameStateScriptProp.GetArrayElementAtIndex( scriptIndexProp.intValue ).stringValue == FrameState.HoldCommand ) {
						GUI.skin.label = currentGridSkin.textField;
					} else {
						GUI.skin.label = currentGridSkin.window;
					}
					activeRect.x = panelBaseRect.x + (panelBaseRect.width * (leftProp.intValue * (1.0f / columnsProp.intValue)));
					activeRect.y = panelBaseRect.y + (panelBaseRect.height * (topProp.intValue * (1.0f / rowsProp.intValue)));
					activeRect.width = panelBaseRect.width * (widthProp.intValue * (1.0f / columnsProp.intValue)) + 1;
					activeRect.height = panelBaseRect.height * (heightProp.intValue * (1.0f / rowsProp.intValue)) + 1;
					activeRect = ConstrainPanelRect( activeRect, r );
					GUI.Label( activeRect, panel.gameObject.name );

					GUI.skin = null;
					
					if ( Event.current.isMouse ) {
						Vector2 pos = Event.current.mousePosition;
						if (( pos.x >= r.x ) && ( pos.x <= ( r.x + r.width )) && ( pos.y >= r.y ) && ( pos.y <= ( r.y + r.height ))) {
							Vector2 unitPos = new Vector2( Mathf.Floor(( pos.x - panelBaseRect.x ) / (float)unitWidth ), Mathf.Floor(( pos.y - panelBaseRect.y ) / (float)unitHeight ) );
							switch ( Event.current.type ) {
								
							case EventType.MouseDown:
								startDragPos = pos;
								startDragGridPos = unitPos;
								widthProp.intValue = 1;
								heightProp.intValue = 1;
								leftProp.intValue = ( int )unitPos.x;
								topProp.intValue = ( int )unitPos.y;
								PopulateFrameState( frameState );
								break;
								
							case EventType.MouseDrag:
							case EventType.MouseUp:
								if ( startDragPos != Vector2.zero ) {
								leftProp.intValue = ( int )Mathf.Min( startDragGridPos.x, unitPos.x );
								topProp.intValue = ( int )Mathf.Min( startDragGridPos.y, unitPos.y );
								widthProp.intValue = ( int )Mathf.Abs( unitPos.x - startDragGridPos.x ) + 1;
								heightProp.intValue = ( int )Mathf.Abs( unitPos.y - startDragGridPos.y ) + 1;
								widthProp.intValue = Mathf.Max( 1, widthProp.intValue );
								heightProp.intValue = Mathf.Max( 1, heightProp.intValue );
								PopulateFrameState( frameState );
									/*}*/
								}
								break;
								
							}
						} else {
							startDragPos = Vector2.zero;
						}
					}
					
					/* -- END ACTIVE PANEL IN VISUAL EDITOR -- */
					
					/* -- DIMENSIONS & LAYOUT -- */
					
					EditorGUILayout.Space();
					
					EditorGUI.BeginChangeCheck();
					
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.LabelField( "Top", GUILayout.Width(60.0f) );
					topProp.intValue = EditorGUILayout.IntField( topProp.intValue );
					
					EditorGUILayout.LabelField( "Width", GUILayout.Width(60.0f) );
					widthProp.intValue = EditorGUILayout.IntField( widthProp.intValue );
					EditorGUILayout.EndHorizontal();
					
					EditorGUILayout.BeginHorizontal();		
					EditorGUILayout.LabelField( "Left", GUILayout.Width(60.0f) );
					leftProp.intValue = EditorGUILayout.IntField( leftProp.intValue );
					
					EditorGUILayout.LabelField( "Height", GUILayout.Width(60.0f) );
					heightProp.intValue = EditorGUILayout.IntField( heightProp.intValue );
					EditorGUILayout.EndHorizontal();
					
					if ( EditorGUI.EndChangeCheck() ) {
						PopulateFrameState( frameState );
					}
					
					EditorGUILayout.Space();

					EditorGUILayout.PrefixLabel("Margins");
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.LabelField( "Top", GUILayout.Width(50.0f) );
					frameState.FindPropertyRelative( "marginTopVal" ).floatValue = EditorGUILayout.FloatField(frameState.FindPropertyRelative( "marginTopVal" ).floatValue );
					EditorGUILayout.LabelField( "Bottom", GUILayout.Width(50.0f) );
					frameState.FindPropertyRelative( "marginBottomVal" ).floatValue = EditorGUILayout.FloatField(frameState.FindPropertyRelative( "marginBottomVal" ).floatValue );
					EditorGUILayout.EndHorizontal();
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.LabelField( "Left", GUILayout.Width(50.0f) );
					frameState.FindPropertyRelative( "marginLeftVal" ).floatValue = EditorGUILayout.FloatField(frameState.FindPropertyRelative( "marginLeftVal" ).floatValue );
					EditorGUILayout.LabelField( "Right", GUILayout.Width(50.0f) );
					frameState.FindPropertyRelative( "marginRightVal" ).floatValue = EditorGUILayout.FloatField(frameState.FindPropertyRelative( "marginRightVal" ).floatValue );
					EditorGUILayout.EndHorizontal();
					
					EditorGUILayout.Space();

					/* -- ASPECT RATIO -- */
					
					EditorGUILayout.Space();

					frameState.FindPropertyRelative( "borderSize" ).intValue = EditorGUILayout.IntField("Border size", frameState.FindPropertyRelative( "borderSize" ).intValue );
					frameState.FindPropertyRelative( "borderColor" ).colorValue = EditorGUILayout.ColorField("Border color", frameState.FindPropertyRelative( "borderColor" ).colorValue );

					EditorGUILayout.Space();

					EditorGUILayout.PrefixLabel("Aspect ratio");
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.LabelField( "Min", GUILayout.Width(40.0f) );
					frameState.FindPropertyRelative( "minAspectRatioVal" ).floatValue = EditorGUILayout.FloatField( frameState.FindPropertyRelative( "minAspectRatioVal" ).floatValue );
					EditorGUILayout.LabelField( "Max", GUILayout.Width(40.0f) );
					frameState.FindPropertyRelative( "maxAspectRatioVal" ).floatValue = EditorGUILayout.FloatField( frameState.FindPropertyRelative( "maxAspectRatioVal" ).floatValue );
					EditorGUILayout.EndHorizontal();
					
					/* -- ALIGNMENT -- */
					
					if (( frameState.FindPropertyRelative( "minAspectRatioVal" ).floatValue != 0.0f ) || ( frameState.FindPropertyRelative( "maxAspectRatioVal" ).floatValue != 0.0f )) {
						EditorGUILayout.Space();
						
						EditorGUI.indentLevel = 1;
						EditorGUILayout.PropertyField( frameState.FindPropertyRelative( "hAlignType" ), new GUIContent( "H Align" ), GUILayout.ExpandWidth(false));
						EditorGUILayout.PropertyField( frameState.FindPropertyRelative( "vAlignType" ), new GUIContent( "V Align" ), GUILayout.ExpandWidth(false));
						EditorGUI.indentLevel = 0;
					}
					
					EditorGUILayout.Space();
					
					frameState.FindPropertyRelative( "matteColor" ).colorValue = EditorGUILayout.ColorField("Matte color", frameState.FindPropertyRelative( "matteColor" ).colorValue );

					EditorGUILayout.Space();
					
				}
				
			}

			/* -- ADVANCED SETTINGS -- */

			EditorStyles.foldout.fontStyle = FontStyle.Bold;
			isEditingAdvancedProp.boolValue = EditorGUILayout.Foldout (isEditingAdvancedProp.boolValue, "Settings");
			EditorStyles.foldout.fontStyle = FontStyle.Normal;
			if (isEditingAdvancedProp.boolValue && (PanoplyCore.panoplyRenderer != null)) {
				EditorGUILayout.BeginHorizontal ();
				EditorGUILayout.LabelField ("Intercept interactions", GUILayout.Width (130.0f));
				interceptInteractionProp.boolValue = EditorGUILayout.Toggle ("", interceptInteractionProp.boolValue);
				EditorGUILayout.EndHorizontal ();

				EditorGUILayout.BeginHorizontal ();
				EditorGUILayout.LabelField ("Disable camera control", GUILayout.Width (130.0f));
				disableCameraControlProp.boolValue = EditorGUILayout.Toggle ("", disableCameraControlProp.boolValue);
				EditorGUILayout.EndHorizontal ();

				GUI.enabled = !disableCameraControlProp.boolValue;
				EditorGUILayout.BeginHorizontal ();
				EditorGUILayout.LabelField ("Preserve framing", GUILayout.Width (130.0f));
				preserveFramingProp.boolValue = EditorGUILayout.Toggle ("", preserveFramingProp.boolValue, GUILayout.Width (20.0f));
				EditorGUILayout.LabelField ("Distance", GUILayout.Width (50.0f));
				framingDistanceProp.floatValue = EditorGUILayout.FloatField (framingDistanceProp.floatValue, GUILayout.Width (40.0f));
				EditorGUILayout.EndHorizontal ();
				GUI.enabled = true;

				EditorGUILayout.Space ();
			}

			if ( !disableCameraControlProp.boolValue ) {

				UpdateState( PanelComponent.PanelCamera, false );

				EditorStyles.foldout.fontStyle = FontStyle.Bold;
				isEditingCameraProp.boolValue = EditorGUILayout.Foldout( isEditingCameraProp.boolValue, "Camera" );
				EditorStyles.foldout.fontStyle = FontStyle.Normal;
				if ( isEditingCameraProp.boolValue ) {
					
					/* -- CAMERA STATE MANAGEMENT -- */

					KeyframeEditor(PanelComponent.PanelCamera);
					
					EditorGUILayout.Space();
					
					if ( cameraState != null ) {
						
						/* -- CAMERA -- */
						
						cameraState.FindPropertyRelative( "fieldOfViewVal" ).floatValue = EditorGUILayout.FloatField( "Field of view", cameraState.FindPropertyRelative( "fieldOfViewVal" ).floatValue );
						
						cameraState.FindPropertyRelative( "position" ).vector3Value = EditorGUILayout.Vector3Field( "Position", cameraState.FindPropertyRelative( "position" ).vector3Value );
						cameraState.FindPropertyRelative( "rotation" ).vector3Value = EditorGUILayout.Vector3Field( "Rotation", cameraState.FindPropertyRelative( "rotation" ).vector3Value );
						
						EditorGUILayout.Space();
						
						EditorGUI.BeginChangeCheck();
						lookAtEnabledProp.boolValue = EditorGUILayout.Toggle( "Enable look at", lookAtEnabledProp.boolValue );
						if ( EditorGUI.EndChangeCheck() ) {
							if ( lookAtEnabledProp.boolValue ) {
								cameraState.FindPropertyRelative( "orientationType" ).enumValueIndex = ( int )CameraOrientationType.LookAt;
							} else {
								cameraState.FindPropertyRelative( "orientationType" ).enumValueIndex = ( int )CameraOrientationType.Standard;
							}
						}
						if ( lookAtEnabledProp.boolValue ) {
							cameraState.FindPropertyRelative( "lookAt" ).vector3Value = EditorGUILayout.Vector3Field( "Look at", cameraState.FindPropertyRelative( "lookAt" ).vector3Value );
						}
						
						EditorGUILayout.Space();
						
					}
					
				}
			
				UpdateState( PanelComponent.PanelPassiveMotion, false );
				
				EditorStyles.foldout.fontStyle = FontStyle.Bold;
				isEditingPassiveMotionProp.boolValue = EditorGUILayout.Foldout( isEditingPassiveMotionProp.boolValue, "Passive Motion" );
				EditorStyles.foldout.fontStyle = FontStyle.Normal;
				if ( isEditingPassiveMotionProp.boolValue ) {
					
					/* -- PASSIVE MOTION STATE MANAGEMENT -- */

					KeyframeEditor(PanelComponent.PanelPassiveMotion);
					
					EditorGUILayout.Space();
					
					if ( passiveMotionStateH != null ) {
						
						EditorGUI.BeginChangeCheck();
						passiveMotionStateH.FindPropertyRelative( "outputMaximumVal" ).floatValue = EditorGUILayout.Slider( "Horizontal tilt", passiveMotionStateH.FindPropertyRelative( "outputMaximumVal" ).floatValue, 0.0f, 5.0f );
						if ( EditorGUI.EndChangeCheck() ) {
							passiveMotionStateH.FindPropertyRelative( "outputMinimumVal" ).floatValue = -passiveMotionStateH.FindPropertyRelative( "outputMaximumVal" ).floatValue;
						}
						EditorGUI.BeginChangeCheck();
						passiveMotionStateV.FindPropertyRelative( "outputMaximumVal" ).floatValue = EditorGUILayout.Slider( "Vertical tilt", passiveMotionStateV.FindPropertyRelative( "outputMaximumVal" ).floatValue, 0.0f, 5.0f );
						if ( EditorGUI.EndChangeCheck() ) {
							passiveMotionStateV.FindPropertyRelative( "outputMinimumVal" ).floatValue = -passiveMotionStateV.FindPropertyRelative( "outputMaximumVal" ).floatValue;
						}
						
						EditorGUILayout.Space();
						
					}
					
				}
				
			}

			DrawKeyframeTimelines();
			
			
			/*EditorGUILayout.BeginHorizontal();
			if ( GUILayout.Button( 'Remove step' ) ) {
				if ( EditorUtility.DisplayDialog( 'Remove Step?', 'Are you sure you want to remove this step?', 'OK', 'Cancel' ) ) {
					panel.stateScript.RemoveAt( scriptIndexProp.intValue );
				}
			}
			if ( GUILayout.Button( 'Add step' ) ) {
				panel.stateScript.Add( FrameState.HoldCommand );
			}
			EditorGUILayout.EndHorizontal();
			
			EditorGUILayout.Space();
			
			tabIndex = GUILayout.Toolbar( tabIndex, tabOptions );
			
			EditorGUILayout.Space();
			
			switch ( tabIndex ) {
				
				case 0:
				// Build array of frame state ids
				n = panel.states.Count;
				frameStateIds = new String[ n + 1 ];
				for ( i = 0; i < n; i++ ) {
					frameState = panel.states[ i ] as FrameState;
					if ( frameState ) {
						frameStateIds[ i ] = frameState.id;
					}
				}
				frameStateIds[ n ] = FrameState.HoldCommand;
				
				// Build array of frame state script ids
				n = panel.stepDuration;
				frameStateScriptIndices = new int[ n ];
				for ( i = 0; i < n; i++ ) {
					if ( i >= panel.oFrame.stateScript.Count ) {
						panel.oFrame.stateScript.Add( FrameState.HoldCommand );
					}
					frameStateIndex = panel.oFrame.getStateIndexForId( panel.oFrame.stateScript[ i ] );
					if ( frameStateIndex != -1 ) {
						frameStateScriptIndices[ i ] = frameStateIndex;
					} else {
						frameStateScriptIndices[ i ] = panel.oFrame.states.Count;
					}
				}
			
				for ( i = 0; i < panel.stepDuration; i++ ) {
				
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.PrefixLabel( 'Step ' + ( panel.startIndex + i ) );
					frameStateScriptIndices[ i ] = EditorGUILayout.Popup( frameStateScriptIndices[ i ], frameStateIds );
					EditorGUILayout.EndHorizontal();

					/*if ( i == scriptIndexProp.intValue ) {
						GUI.color = Color( 100, 100, 100, 1 );
					}
					EditorGUI.BeginChangeCheck();
					frameStateScriptIndices[ i ] = EditorGUILayout.Popup( 'Step ' + i, frameStateScriptIndices[ i ], frameStateIds );
					if ( EditorGUI.EndChangeCheck() ) {
						PanoplyCore.interpolatedStep = PanoplyCore.targetStep = startIndexProp.intValue + i;
					}
					GUI.color = Color.white;
					Frame.stateScript[ i ] = frameStateIds[ frameStateScriptIndices[ i ] ];
				}
				break;
			
			}*/
			
			serializedObject.ApplyModifiedProperties();
			
		}
		
	}
}
