using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/**
 * The ThumbnailIndex class defines a thumbnail image which
 * serves as an index to specific point on the timeline.
 * Copyright © Erik Loyer
 * erik@opertoon.com
 * Part of the Panoply engine
 */

namespace Opertoon.Panoply {
	[System.Serializable]
	public class ThumbnailMenuItem : System.Object {
		public Texture2D thumbnail;
		public int step;
		public bool instantTransition;
	}
}
