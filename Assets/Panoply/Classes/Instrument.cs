using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Opertoon.Panoply;

/**
 * The Instrument class defines an audio instrument
 * whose volume can vary across the global timeline.
 * Copyright © Erik Loyer
 * erik@opertoon.com
 * Part of the Panoply engine
 */

namespace Opertoon.Panoply {
	[System.Serializable]
	public class Instrument: System.Object {
		
		public string name = "New instrument";
		public bool doesLoop;
		public AudioClip clip;
		public InstrumentState[] states;
		public AudioSource source;
		public int lastPlayedStep;

		public InstrumentState GetCurrentState() {
			if (( states.Length > PanoplyCore.targetStep ) && ( PanoplyCore.targetStep >= 0 )) {
				return states[ PanoplyCore.targetStep ];
			}
			return null;
		}

		public void InsertState() {
			InstrumentState state = new InstrumentState();
			state.volume = states[Mathf.Max(0, PanoplyCore.targetStep - 1)].volume;
			ArrayList arrayList = new ArrayList(states);
			arrayList.Insert(PanoplyCore.targetStep, state);
			states = new InstrumentState[arrayList.Count];
			arrayList.CopyTo(states);
		}

		public void DeleteCurrentState() {
			DeleteState(PanoplyCore.targetStep);
		}

		public void DeleteState(int index) {
			if (index < states.Length) {
				ArrayList arrayList = new ArrayList(states);
				arrayList.RemoveAt(index);
				states = new InstrumentState[arrayList.Count];
				arrayList.CopyTo(states);
			}
		}

	}
}