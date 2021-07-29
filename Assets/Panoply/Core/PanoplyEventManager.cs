using UnityEngine;
using System.Collections;

/**
 * The PanoplyEventManager class triggers events as needed by the engine.
 * Copyright © Erik Loyer
 * erik@opertoon.com
 * Part of the Panoply engine
 */

namespace Opertoon.Panoply {
	public class PanoplyEventManager : MonoBehaviour {

		public delegate void TargetStepChanged( int oldStep, int newStep );
		public static event TargetStepChanged OnTargetStepChanged;

		public void HandleTargetStepChanged( int oldStep, int newStep ) {
			if ( OnTargetStepChanged != null ) {
				OnTargetStepChanged( oldStep, newStep );
			}
		}

	}
}