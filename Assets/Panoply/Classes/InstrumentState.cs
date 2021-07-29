using UnityEngine;
using System;

/**
 * The InstrumentState class defines the volune of an audio
 * instrument at a particular point in on the global timeline.
 * Copyright © Erik Loyer
 * erik@opertoon.com
 * Part of the Panoply engine
 */

namespace Opertoon.Panoply {
	[System.Serializable]
	public class InstrumentState: System.Object {

		public string id = "New state";
		public static string HoldCommand = "Hold";
		[Range(0.0f,1.0f)]
		public float volume;

		// stuff

	}
}