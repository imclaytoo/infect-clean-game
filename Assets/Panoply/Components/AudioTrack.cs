using UnityEngine;
using System;
using System.Collections.Generic;
using Opertoon.Panoply;

/**
 * AudioTrack
 * Manages a single channel of audio.
 * Copyright © Erik Loyer
 * erik@opertoon.com
 * Part of the Panoply engine
 */

namespace Opertoon.Panoply {
	public class AudioTrack: MonoBehaviour {  
		
		/* -- StateManager properties start -- */
		
		public List<InstrumentState> states = new List<InstrumentState>();	// Library of possible states
		
		public int startIndex = 0;											// The index at which this item's script starts
		public List<string> stateScript = new List<string>();				// Array of state ids that describe how it changes over time
		public int scriptIndex = 0;											// Index of our current position in the state script
		
		public float crossfade;												// Range from -1 to 1; 0 is current state, -1 is previous, 1 is next
		
		public int[] scriptStateIndices;									// The index of each step's state							
		
		public bool isEditingStates = false;								// Are we currently editing the states?
		public bool isEditingScript = false;								// Are we currently editing the script?
		
		/* -- StateManager properties end -- */
		
		public float progress;
		public int stateCounter = 0;
		public float fadeTime = 1;
		
		public string trackName = "Untitled";
		public bool doesLoop;
		public AudioClip clip;
		public AudioSource source;
		public int lastPlayedStep;

		private float fadeFactor;

		public void Start() {

		}
		
		/* -- StateManager methods start -- */
		
		public InstrumentState GetStateForId( string id ) {
			
			int i = 0;
			int n = states.Count;
			InstrumentState state = null;
			id = GetCleanIdFromId( id );
			
			for( i = 0; i < n; i++ ) {
				state = states[ i ];
				if ( state.id == id ) {
					return state;
				}
			}
			
			return null;
		}
		
		public int GetStateIndexForId( string id ) {
			
			int i = 0;
			int n = states.Count;
			InstrumentState state = null;
			id = GetCleanIdFromId( id );
			
			for( i = 0; i < n; i++ ) {
				state = states[ i ];
				if ( state.id == id ) {
					return i;
				}
			}
			
			return -1;
		}
		
		public string GetCleanIdFromId( string id ) {
			
			string cleanId = null;
			
			string[] temp = id.Split( new string[] {") "}, StringSplitOptions.None );
			if ( temp.Length > 1 ) {
				cleanId = String.Join( ") ", temp, 1, temp.Length - 1 );
			} else {
				cleanId = id;
			}
			
			return cleanId;
		}
		
		public InstrumentState GetStateAtScriptIndex( int index ) {
			
			if (( index >= 0 ) && ( index < stateScript.Count )) {
				string id = stateScript[ index ];
				if ( id == InstrumentState.HoldCommand ) {
					
					int i = 0;
					InstrumentState instrumentState = null;
					
					for( i = ( index - 1 ); i >= 0; i-- ) {
						instrumentState = GetStateForId( stateScript[ i ] );
						if ( instrumentState != null ) {
							return instrumentState;
						}
					}
					
				} else {
					return GetStateForId( stateScript[ index ] );
				}
			}
			
			return null;
			
		}
		
		public InstrumentState PreviousState() {
			
			if (PanoplyCore.scene != null) {
				int scriptIndexProxy = Mathf.Clamp( scriptIndex - 1, 0, PanoplyCore.scene.stepCount - 1 );

				if ( ( stateScript.Count == 0 ) || ( scriptIndex < 0 ) || ( scriptIndex > ( stateScript.Count - 1 ) ) ) {
					return null;
				} else {
					return GetStateAtScriptIndex( scriptIndexProxy );
				}
			} else {
				return null;
			}

		}
		
		public InstrumentState CurrentState() {
			
			if (PanoplyCore.scene != null) {
				int scriptIndexProxy = Mathf.Clamp( scriptIndex, 0, PanoplyCore.scene.stepCount - 1 );

				if ( ( stateScript.Count == 0 ) || ( scriptIndex < 0 ) || ( scriptIndex > ( stateScript.Count - 1 ) ) ) {
					return null;
				} else {
					return GetStateAtScriptIndex( scriptIndexProxy );
				}
			} else {
				return null;
			}

		}
		
		public InstrumentState NextState() {
			
			if (PanoplyCore.scene != null) {
				int scriptIndexProxy = Mathf.Clamp( scriptIndex + 1, 0, PanoplyCore.scene.stepCount - 1 );

				if ( ( stateScript.Count == 0 ) || ( scriptIndex < 0 ) || ( scriptIndex > ( stateScript.Count - 1 ) ) ) {
					return null;
				} else {
					return GetStateAtScriptIndex( scriptIndexProxy );
				}
			} else {
				return null;
			}

		}
		
		/* -- StateManager methods end -- */
		
		void PlayAudioWithState( InstrumentState state ) {
			
			// is it supposed to loop?
			if ( doesLoop ) {
				
				// loop it
				source = PanoplyAudio.PlayLoopingClip( clip, transform, 0 );
				lastPlayedStep = PanoplyCore.targetStep;
				
				// is it not supposed to loop?
			} else {
				
				// play it w/o looping
				source = PanoplyAudio.PlayClip( clip, transform, 1.0f, state.volume, 0, 0 );
				lastPlayedStep = PanoplyCore.targetStep;
				
			}
			
		}

		public void Update() {
			
			/* -- StateManager update start --*/
			
			int targetStepProxy = ( int ) Mathf.Max ( Mathf.Floor( PanoplyCore.interpolatedStep ), Mathf.Min ( Mathf.Ceil ( PanoplyCore.interpolatedStep ), PanoplyCore.targetStep ));
			scriptIndex = targetStepProxy - startIndex;
			/*crossfade = PanoplyCore.interpolatedStep - targetStepProxy;
			
			InstrumentState stateA = null;
			InstrumentState stateB = null;
			
			if ( crossfade < 0 ) {
				stateA = PreviousState();
				stateB = CurrentState();
				progress = crossfade + 1;
				
			} else {
				stateA = CurrentState();
				stateB = NextState();
				progress = crossfade;
			}*/
			
			/* -- StateManager update end --*/

			InstrumentState state = CurrentState();
			
			if ( PanoplyCore.targetStep != lastPlayedStep ) {
				lastPlayedStep = -1;
			}

			fadeFactor = 1.0f / Mathf.Max(.0001f, (fadeTime * .5f));
			
			if (state != null) {
				
				// has an audio clip been assigned?
				if ( clip != null ) {
					
					// is this a looping instrument?
					if ( doesLoop ) {
						
						// should we be hearing the instrument?
						if ( state.volume > 0 ) {
							
							// could it already be playing?
							if ( source != null ) {
								
								// is it not playing?
								if ( !source.isPlaying ) {
									PlayAudioWithState( state );
									
									// is it playing?
								} else {
									
									// is it at the right volume?
									if ( source.volume != state.volume ) {
										
										// adjust the volume
										source.volume = Mathf.Lerp( source.volume, state.volume, Time.deltaTime * fadeFactor );
										
									}
									
								}
								
							} else {
								PlayAudioWithState( state );
							}
							
							// should we not be hearing the instrument?
						} else {
							
							// could it already be playing?
							if ( source != null ) {
								
								// is it playing?
								if ( source.isPlaying ) {
									
									// is it quiet enough to turn off?
									if ( source.volume < .001f ) {
										
										source.Stop();
										
									} else {
										
										// adjust the volume
										source.volume = Mathf.Lerp( source.volume, state.volume, Time.deltaTime * fadeFactor );
										
									}	
								}
							}
							
						}
						
					} else {	
							
						// could it already be playing?
						if ( source != null ) {
							
							// is it not playing?
							if ( !source.isPlaying ) {
									
								// should we be hearing the instrument?
								if ( state.volume > 0 ) {

									// if it hasn't already been played during this step, then
									if ( lastPlayedStep != PanoplyCore.targetStep ) {
										PlayAudioWithState( state );
									}
								}

							} else {
								
								// is it quiet enough to turn off?
								if ( source.volume < .001f ) {
									
									source.Stop();
									
								} else {
									
									// adjust the volume
									source.volume = Mathf.Lerp( source.volume, state.volume, Time.deltaTime * fadeFactor );
									
								}	
							}
							
						} else {
							
							// should we be hearing the instrument?
							if ( state.volume > 0 ) {

								// if it hasn't already been played during this step, then
								if ( lastPlayedStep != PanoplyCore.targetStep ) {
									PlayAudioWithState( state );
								}
							}
						}
						
					}
					
				}
				
			}			
		}
	}
}