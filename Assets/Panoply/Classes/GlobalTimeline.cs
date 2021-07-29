using UnityEngine;
using System;

/**
 * The GlobalTimeline class encapsulates the current
 * target step so that a slider drawer can be used to
 * globally navigate the timeline across inspectors.
 * Copyright © Erik Loyer
 * erik@opertoon.com
 * Part of the Panoply engine
 */

namespace Opertoon.Panoply {
	[System.Serializable]
	public class GlobalTimeline: System.Object {
		
		public float step;
		
	}
}