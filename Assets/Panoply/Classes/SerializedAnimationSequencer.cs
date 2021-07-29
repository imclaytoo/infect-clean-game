using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Opertoon.Panoply
{
	public class SerializedAnimationSequencer
	{
#if UNITY_EDITOR

		public SerializedObject target;
		public List<SerializedProperty> triggersPropArray;

		public SerializedAnimationSequencer (UnityEngine.Object obj)
		{
			target = new SerializedObject (obj);
			SetProperties ();
		}

		public SerializedAnimationSequencer (SerializedObject obj)
		{
			target = obj;
			SetProperties ();
		}

		private void SetProperties ()
		{
			triggersPropArray = new List<SerializedProperty> ();
			SerializedProperty triggersProp = target.FindProperty ("triggers");
			int n = triggersProp.arraySize;
			for (int i = 0; i < n; i++) {
				triggersPropArray.Add (triggersProp.GetArrayElementAtIndex (i));
			}
		}

		public void InsertStep ()
		{
			target.Update ();
			int n = triggersPropArray.Count;
			SerializedProperty timelineStepProp;
			for (int i = 0; i < n; i++) {
				timelineStepProp = triggersPropArray[i].FindPropertyRelative ("timelineStep");
				if (timelineStepProp.intValue > PanoplyCore.targetStep) {
					timelineStepProp.intValue++;
				}
			}
			target.ApplyModifiedProperties ();
		}

		public void DeleteStep ()
		{
			target.Update ();
			int n = triggersPropArray.Count;
			SerializedProperty timelineStepProp;
			for (int i = 0; i < n; i++) {
				timelineStepProp = triggersPropArray [i].FindPropertyRelative ("timelineStep");
				if (timelineStepProp.intValue > PanoplyCore.targetStep) {
					timelineStepProp.intValue--;
				}
			}
			target.ApplyModifiedProperties ();
		}

#endif
	}
}
