using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/**
 * The AnimationTrigger class defines conditions under which
 * an animation should be triggered by the Panoply timeline.
 * Copyright © Erik Loyer
 * erik@opertoon.com
 * Part of the Panoply engine
 */

namespace Opertoon.Panoply
{
	[System.Serializable]
	public class AnimationTrigger
	{
		public GameObject target;
		public string triggerName;
		public int timelineStep;
		public PanoplyStepDirection direction;
	}
}