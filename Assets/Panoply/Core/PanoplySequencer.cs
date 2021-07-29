using UnityEngine;
using System;
using Opertoon.Panoply;
using System.Collections.Generic;

/**
 * The PanoplySequencer class manages a scene's audio instruments.
 * Copyright © Erik Loyer
 * erik@opertoon.com
 * Part of the Panoply engine
 */

namespace Opertoon.Panoply {
	public class PanoplySequencer: MonoBehaviour {
	    
	    
	    public GlobalTimeline globalTimeline;
	    public Instrument[] instruments;

		[HideInInspector]
		public int instrumentIndex = 0;
		[HideInInspector]
		public int stateCounter = 0;
		[HideInInspector]
		public List<Instrument> keyedInstruments;

		/* -- StateManager properties start -- *

		[HideInInspector]
		public List<InstrumentState> states = new List<InstrumentState>();	// Library of possible states

		[HideInInspector]
		public int startIndex = 0;											// The index at which this item's script starts
		[HideInInspector]
		public List<string> stateScript = new List<string>();				// Array of state ids that describe how it changes over time
		[HideInInspector]
		public int scriptIndex = 0;											// Index of our current position in the state script

		[HideInInspector]
		public float crossfade;												// Range from -1 to 1; 0 is current frame state, -1 is previous, 1 is next

		[HideInInspector]
		public int[] scriptStateIndices;									// The index of each step's state		
		[HideInInspector]
		public float editorScrubberScriptIndex;								// Position of the scrubber being used to edit this component
		
		/* -- StateManager properties end -- */

	    public void Start() {

	    }

		/* -- StateManager methods start -- *
		
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

		public int GetAdjustedScriptIndex( int index ) {
			return index + (instrumentIndex * PanoplyCore.scene.stepCount);
		}

		public int GetMinScriptIndex() {
			return instrumentIndex * PanoplyCore.scene.stepCount;
		}

		public int GetMaxScriptIndex() {
			return ((instrumentIndex + 1) * PanoplyCore.scene.stepCount) - 1;
		}
		
		public InstrumentState GetStateAtScriptIndex( int index ) {

			int indexAdj = GetAdjustedScriptIndex(index);
			int min = GetMinScriptIndex();
			int max = GetMaxScriptIndex();

			if (( indexAdj >= min ) && ( indexAdj <= max )) {
				string id = stateScript[ indexAdj ];
				if ( id == InstrumentState.HoldCommand ) {
					
					int i = 0;
					InstrumentState instrumentState = null;
					
					for( i = ( indexAdj - 1 ); i >= min; i-- ) {
						instrumentState = GetStateForId( stateScript[ i ] );
						if ( instrumentState != null ) {
							return instrumentState;
						}
					}
					
				} else {
					return GetStateForId( stateScript[ indexAdj ] );
				}
			}
			
			return null;
			
		}		

		public InstrumentState PreviousState() {

			int indexAdj = GetAdjustedScriptIndex(scriptIndex);
			int min = GetMinScriptIndex();
			int max = GetMaxScriptIndex();

			if ( (( max - min ) == 0 ) || ( indexAdj < min ) || ( indexAdj > max ) ) {
				return null;
			} else if ( indexAdj == min ) {
				return GetStateAtScriptIndex( scriptIndex );
			} else {
				return GetStateAtScriptIndex( scriptIndex - 1 );
			}
			
		}
		
		public InstrumentState CurrentState() {
			
			int indexAdj = GetAdjustedScriptIndex(scriptIndex);
			int min = GetMinScriptIndex();
			int max = GetMaxScriptIndex();
			
			if ( (( max - min ) == 0 ) || ( indexAdj < min ) || ( indexAdj > max ) ) {
				return null;
			} else {
				return GetStateAtScriptIndex( scriptIndex );
			}
			
		}
		
		public InstrumentState NextState() {
			
			int indexAdj = GetAdjustedScriptIndex(scriptIndex);
			int min = GetMinScriptIndex();
			int max = GetMaxScriptIndex();
			
			if ( (( max - min ) == 0 ) || ( indexAdj < min ) || ( indexAdj > max ) ) {
				return null;
			} else if ( indexAdj == max ) {
				return GetStateAtScriptIndex( scriptIndex );
			} else {
				return GetStateAtScriptIndex( scriptIndex + 1 );
			}
			
		}
		
		/* -- StateManager methods end -- */
	    
	    void PlayInstrumentWithState( Instrument instrument, InstrumentState state ) {
	    	
	    	// is it supposed to loop?
	    	if ( instrument.doesLoop ) {
	    	
	    		// loop it
	    		instrument.source = PanoplyAudio.PlayLoopingClip( instrument.clip, transform, state.volume );
	    		instrument.lastPlayedStep = PanoplyCore.targetStep;
	    		
	    	// is it not supposed to loop?
	    	} else {
	    	
	    		// play it w/o looping
	    		instrument.source = PanoplyAudio.PlayClip( instrument.clip, transform, 1.0f, state.volume, 0, 0 );
	    		instrument.lastPlayedStep = PanoplyCore.targetStep;
	    		
	    	}
	    	
	    }

		public void InsertStateForAllInstruments() {
			int i = 0;
			int n = instruments.Length;
			for (i=0; i<n; i++) {
				instruments[i].InsertState();
			}
		}

		public void DeleteCurrentStateOfAllInstruments() {
			int i = 0;
			int n = instruments.Length;
			for (i=0; i<n; i++) {
				instruments[i].DeleteCurrentState();
			}
		}
	    
	    public void Update() {
	    
	    	int i = 0;
	    	int n = instruments.Length;
	    	Instrument instrument = null;
	    	InstrumentState state = null;
	    	
	    	globalTimeline.step = ( float )PanoplyCore.targetStep;
	    	
	    	for( i = 0; i < n; i++ ) {
	    	
	    		instrument = instruments[ i ];
	    		state = instrument.GetCurrentState();
	    		
	    		if ( PanoplyCore.targetStep != instrument.lastPlayedStep ) {
	    			instrument.lastPlayedStep = -1;
	    		}

				if (state != null) {
	    		
		    		// has an audio clip been assigned?
		    		if ( instrument.clip != null ) {
		    		
		    			// is this a looping instrument?
		    			if ( instrument.doesLoop ) {
		    				
		    				// should we be hearing the instrument?
		    				if ( state.volume > 0 ) {
		    				
		    					// could it already be playing?
		    					if ( instrument.source != null ) {
		    					
		    						// is it not playing?
		    						if ( !instrument.source.isPlaying ) {
		    							PlayInstrumentWithState( instrument, state );
		    							
		    						// is it playing?
		    						} else {
		    						
		    							// is it at the right volume?
		    							if ( instrument.source.volume != state.volume ) {
		    							
		    								// adjust the volume
		    								instrument.source.volume = Mathf.Lerp( instrument.source.volume, state.volume, Time.deltaTime * 2 );
		    								
		    							}
		    							
		    						}
		    						
		    					} else {
		    						PlayInstrumentWithState( instrument, state );
		    					}
		    					
		    				// should we not be hearing the instrument?
		    				} else {
		    				
		    					// could it already be playing?
		    					if ( instrument.source != null ) {
		    					
		    						// is it playing?
		    						if ( instrument.source.isPlaying ) {
		    						
		    							// is it quiet enough to turn off?
		    							if ( instrument.source.volume < .001f ) {
		    							
		    								instrument.source.Stop();
		    								
		    							} else {
		    							
		    								// adjust the volume
		    								instrument.source.volume = Mathf.Lerp( instrument.source.volume, state.volume, Time.deltaTime * 2 );
		    								
		    							}	
		    						}
		    					}
		    					
		    				}
		    				
		    			} else {	
		    				
		    				// should we be hearing the instrument?
		    				if ( state.volume > 0 ) {
		    				
		    					// could it already be playing?
		    					if ( instrument.source != null ) {
		    					
		    						// is it not playing?
		    						if ( !instrument.source.isPlaying ) {
		    							
		    							// if it hasn't already been played during this step, then
		    							if ( instrument.lastPlayedStep != PanoplyCore.targetStep ) {
		    								PlayInstrumentWithState( instrument, state );
		    							}
		    						}
		    						
		    					} else {
		    						
		    						// if it hasn't already been played during this step, then
		    						if ( instrument.lastPlayedStep != PanoplyCore.targetStep ) {
		    							PlayInstrumentWithState( instrument, state );
		    						}
		    					}
		    					
		    				} else {
		    				
		    				}
		    			
		    			}
		    		
		    		}
				}
	    		
	    	}
	    	
	    }
	}
}