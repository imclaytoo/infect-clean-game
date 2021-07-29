using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

/**
 * The SerializedKeyframeTimeline class handles manipulation of keyframes
 * at the timeline level.
 * Copyright © Erik Loyer
 * erik@opertoon.com
 * Part of the Panoply engine
 */

namespace Opertoon.Panoply {
	public class SerializedKeyframeTimeline {

#if UNITY_EDITOR

		public SerializedObject target;
		public PanelComponent component;
		public List<SerializedProperty> stateArraysProp;
		public SerializedProperty stateScriptProp;
		public SerializedProperty scriptStateIndicesProp;
		public SerializedProperty scriptIndexProp;
		public string holdCommand;
		
		public SerializedKeyframeTimeline(UnityEngine.Object obj) {
			target = new SerializedObject(obj);	
			SetProperties();
		}
		
		public SerializedKeyframeTimeline(SerializedObject obj) {
			target = obj;	
			SetProperties();
		}

		public SerializedKeyframeTimeline(UnityEngine.Object obj, PanelComponent pc) {
			target = new SerializedObject(obj);
			SetPropertiesFromComponent(pc);
		}
		
		public SerializedKeyframeTimeline(SerializedObject obj, PanelComponent pc) {
			target = obj;
			SetPropertiesFromComponent(pc);
		}

		private void SetProperties() {
			stateArraysProp = new List<SerializedProperty>();
			stateArraysProp.Add(target.FindProperty( "states" ));
			stateScriptProp = target.FindProperty( "stateScript" );
			scriptStateIndicesProp = target.FindProperty( "scriptStateIndices" );
			scriptIndexProp = target.FindProperty( "scriptIndex" );
			holdCommand = "Hold";
		}

		private void SetPropertiesFromComponent(PanelComponent pc) {
			component = pc;
			string prefix = "";
			string[] suffixes = new string[1];
			suffixes[0] = "";
			switch (component) {
			case PanelComponent.PanelFrame:
				prefix = "frame";
				break;
			case PanelComponent.PanelCamera:
				prefix = "camera";
				break;
			case PanelComponent.PanelPassiveMotion:
				prefix = "passiveMotion";
				suffixes = new string[2];
				suffixes[0] = "H";
				suffixes[1] = "V";
				break;
			}
			
			stateArraysProp = new List<SerializedProperty>();
			int i;
			int n = suffixes.Length;
			for (i=0; i<n; i++) {
				stateArraysProp.Add(target.FindProperty( prefix + "States" + suffixes[i] ));
			}
			stateScriptProp = target.FindProperty( prefix + "StateScript" );
			scriptStateIndicesProp = target.FindProperty( prefix + "ScriptStateIndices" );
			scriptIndexProp = target.FindProperty( "scriptIndex" );
			holdCommand = "Hold";
		}

		public void InsertState() {
			target.Update ();
			scriptIndexProp.intValue = PanoplyCore.targetStep;
			if (scriptIndexProp.intValue < (stateScriptProp.arraySize - 1)) {
				while (scriptIndexProp.intValue >= stateScriptProp.arraySize) {
					scriptStateIndicesProp.InsertArrayElementAtIndex(scriptStateIndicesProp.arraySize);
					scriptStateIndicesProp.GetArrayElementAtIndex(scriptStateIndicesProp.arraySize - 1).intValue = 0;
					stateScriptProp.InsertArrayElementAtIndex(scriptStateIndicesProp.arraySize);
					stateScriptProp.GetArrayElementAtIndex(stateScriptProp.arraySize - 1).stringValue = holdCommand;
				}
				stateScriptProp.InsertArrayElementAtIndex(scriptIndexProp.intValue + 1);
				stateScriptProp.GetArrayElementAtIndex(scriptIndexProp.intValue + 1).stringValue = holdCommand;
				scriptStateIndicesProp.InsertArrayElementAtIndex(scriptIndexProp.intValue + 1);
				scriptStateIndicesProp.GetArrayElementAtIndex(scriptIndexProp.intValue + 1).intValue = 0;
				EditorUtility.SetDirty(target.targetObject);
			}
			target.ApplyModifiedProperties ();
		}

		// deletes the state referenced at the current step of the script arrays,
		// and deletes the current step of the script arrays
		public void DeleteCurrentState() {
			target.Update ();
			scriptIndexProp.intValue = PanoplyCore.targetStep;
			if (scriptIndexProp.intValue < stateScriptProp.arraySize) {
				if ( stateScriptProp.GetArrayElementAtIndex( scriptIndexProp.intValue ).stringValue != holdCommand ) {
					DeleteStateAtIndex( scriptStateIndicesProp.GetArrayElementAtIndex( scriptIndexProp.intValue ).intValue - 1 ); // - 1 because 'hold' is at index 0
				}
				stateScriptProp.DeleteArrayElementAtIndex( scriptIndexProp.intValue );
				scriptStateIndicesProp.DeleteArrayElementAtIndex( scriptIndexProp.intValue );
				target.ApplyModifiedProperties();
			} else {
				Debug.Log ("Alert: Script index out of range for state script.");
			}
			EditorUtility.SetDirty (target.targetObject);
		}

		// deletes the state at the given index from all state arrays and
		// reindexes all subsequent state references in the script arrays
		public void DeleteStateAtIndex(int index) {
			int i = 0;
			int n = stateArraysProp.Count;
			int v = 0;
			for (i=0; i<n; i++) {
				stateArraysProp[i].DeleteArrayElementAtIndex( index );
			}
			n = scriptStateIndicesProp.arraySize;
			for( i = 0; i < n; i++ ) {
				v = scriptStateIndicesProp.GetArrayElementAtIndex( i ).intValue;
				if ( v > ( index + 1 ) ) {
					scriptStateIndicesProp.GetArrayElementAtIndex( i ).intValue = v - 1;
				}
			}
			EditorUtility.SetDirty(target.targetObject);
			target.ApplyModifiedProperties();
		}

#endif

	}
}