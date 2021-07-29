using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Opertoon.Panoply;

/**
 * AnimationSequencer
 * Manages a set of animation triggers.
 * Copyright © Erik Loyer
 * erik@opertoon.com
 * Part of the Panoply engine
 */

namespace Opertoon.Panoply
{
	public class AnimationSequencer : MonoBehaviour
	{
		public AnimationTrigger [] triggers;

		// Use this for initialization
		void Start ()
		{
			PanoplyEventManager.OnTargetStepChanged += HandleTargetStepChanged;
		}

		public void InsertVirtualKeyframe(int step)
		{
			int n = triggers.Length;
			AnimationTrigger trigger;
			for (int i = 0; i < n; i++) {
				trigger = triggers [i];
				if (trigger.timelineStep > step) {
					trigger.timelineStep++;
				}
			}
		}

		public void DeleteVirtualKeyframe (int step)
		{
			int n = triggers.Length;
			AnimationTrigger trigger;
			for (int i = 0; i < n; i++) {
				trigger = triggers [i];
				if (trigger.timelineStep > step) {
					trigger.timelineStep--;
				}
			}
		}

			public void HandleTargetStepChanged (int oldStep, int newStep)
		{
			bool isForwardTransition = oldStep < newStep;
			int n = triggers.Length;
			AnimationTrigger trigger;
			for (int i = 0; i < n; i++) {
				trigger = triggers [i];
				if (trigger.target != null) {
					if (newStep == trigger.timelineStep) {
						switch (trigger.direction) {
						case PanoplyStepDirection.BackwardOnly:
							if (!isForwardTransition) {
								FireAnimationTrigger (trigger);
							}
							break;
						case PanoplyStepDirection.ForwardOnly:
							if (isForwardTransition) {
								FireAnimationTrigger (trigger);
							}
							break;
						default:
							FireAnimationTrigger (trigger);
							break;
						}
					}
				}
			}
		}

		private void FireAnimationTrigger (AnimationTrigger trigger)
		{
			Animator animator = trigger.target.GetComponent<Animator> ();
			if (animator != null) {
				animator.SetTrigger (trigger.triggerName);
			}
		}

		// Update is called once per frame
		void Update ()
		{

		}
	}
}