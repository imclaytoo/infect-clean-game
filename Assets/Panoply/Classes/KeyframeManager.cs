using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

/**
 * The KeyframeManager class handles manipulation of keyframes
 * at the component level.
 * Copyright © Erik Loyer
 * erik@opertoon.com
 * Part of the Panoply engine
 */

namespace Opertoon.Panoply {
	public class KeyframeManager {


		public KeyframeManager() {
		}

#if UNITY_EDITOR
		public static void InsertStateForTimelineObject(UnityEngine.Object obj) {
			SerializedKeyframeTimeline timeline;
			Debug.Log (obj.GetType ());
			if (obj.GetType() == typeof(Panel)) {
				timeline = new SerializedKeyframeTimeline(obj, PanelComponent.PanelFrame);
				timeline.InsertState();
				timeline = new SerializedKeyframeTimeline(obj, PanelComponent.PanelCamera);
				timeline.InsertState();
				timeline = new SerializedKeyframeTimeline(obj, PanelComponent.PanelPassiveMotion);
				timeline.InsertState();

			} else if ((obj.GetType() == typeof(Artwork)) || (obj.GetType() == typeof(Caption)) || (obj.GetType() == typeof(AudioTrack))) {
				timeline = new SerializedKeyframeTimeline(obj);
				timeline.InsertState();

			} else if (obj.GetType () == typeof (AnimationSequencer)) {
				SerializedAnimationSequencer sequencer = new SerializedAnimationSequencer (obj);
				sequencer.InsertStep ();
			}
		}

		public static void DeleteCurrentStateForTimelineObject(UnityEngine.Object obj) {
			SerializedKeyframeTimeline timeline;
			
			if (obj.GetType() == typeof(Panel)) {
				timeline = new SerializedKeyframeTimeline(obj, PanelComponent.PanelFrame);
				timeline.DeleteCurrentState();
				timeline = new SerializedKeyframeTimeline(obj, PanelComponent.PanelCamera);
				timeline.DeleteCurrentState();
				timeline = new SerializedKeyframeTimeline(obj, PanelComponent.PanelPassiveMotion);
				timeline.DeleteCurrentState();
				
			} else if ((obj.GetType() == typeof(Artwork)) || (obj.GetType() == typeof(Caption)) || (obj.GetType() == typeof(AudioTrack))) {
				timeline = new SerializedKeyframeTimeline(obj);
				timeline.DeleteCurrentState();

			} else if (obj.GetType() == typeof(AnimationSequencer)) {
				SerializedAnimationSequencer sequencer = new SerializedAnimationSequencer (obj);
				sequencer.DeleteStep ();
			}
		}
#endif

	}
}