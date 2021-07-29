using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Opertoon.Panoply;

/**
 * Artwork
 * Manages a piece of artwork.
 * Copyright © Erik Loyer
 * erik@opertoon.com
 * Part of the Panoply engine
 */

namespace Opertoon.Panoply {
	[ExecuteInEditMode()]
	public class Artwork: MonoBehaviour {  
	    
	    /* -- StateManager properties start -- */
	    
	    public List<ArtworkState> states = new List<ArtworkState>();		// Library of possible states
	    
	    public int startIndex = 0;											// The index at which this item's script starts
	    public List<string> stateScript = new List<string>();				// Array of state ids that describe how it changes over time
	    public int scriptIndex = 0;											// Index of our current position in the state script
	    
	    public float crossfade;												// Range from -1 to 1; 0 is current state, -1 is previous, 1 is next
	    
	    public int[] scriptStateIndices;									// The index of each step's state	

		public int priorHoldStateIndex = -1;								// Index of the hold step prior to the current step (-1 if unknown)
		public int priorNonHoldStateIndex = -1;								// Index of the non-hold step prior to the current step (-1 if unknown)
		public ArtworkState priorNonHoldState;								// The non-hold state prior to the current step

	    public bool isEditingStates = false;								// Are we currently editing the states?
	    public bool isEditingScript = false;								// Are we currently editing the script?
	    
	    /* -- StateManager properties end -- */
	    
	    public Panel panel;
	    
	    public float progress;
	    
	    public bool maintainScale = false;
	    public float scaleFactor = 5;
	    public int stateCounter = 0;

		public ArtworkPositionType positionType = ArtworkPositionType.Panel;
	    
	    SpriteRenderer spriteRenderer;
	    Material tempMaterial;
		Renderer artworkRenderer;

		private ArtworkState lastScaledState;
	    
	    public void Start() {

            artworkRenderer = transform.GetComponent<Renderer>();
            if (artworkRenderer != null)
            {
                spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
                tempMaterial = new Material(artworkRenderer.sharedMaterial);
                artworkRenderer.sharedMaterial = tempMaterial;
            }

            if (maintainScale) {
				MaintainScaleForZ();
			}
	    
	    }
	    
	    /* -- StateManager methods start -- */
	    
	    public ArtworkState GetStateForId( string id ) {
	    
	    	int i = 0;
	    	int n = states.Count;
	    	ArtworkState state = null;
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
	    	ArtworkState state = null;
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
	    
	    public void PushState( string id, bool deleteFuture ) {
	    
	    	if ( deleteFuture && ( scriptIndex < ( stateScript.Count - 1 ) ) ) {
	    		stateScript.RemoveRange( scriptIndex + 1, stateScript.Count - scriptIndex );
	    	}
	    
	    	ArtworkState state = GetStateForId( id );
	    	if ( state != null ) {
	    		stateScript.Add( state.id );
	    	}
	    	
	    }
	    
	    public void SetCurrentState( string id ) {
	    
	    	ArtworkState state = GetStateForId( id );
	    	if ( state != null ) {
	    		if ( stateScript.Count == 0 ) {
	    			PushState( state.id, false );
	    		} else {
	    			stateScript[ scriptIndex ] = state.id;
	    		}
	    	}
	    
	    }
	    
	    public void DecrementState() {
	    
	    	if ( scriptIndex > 1 ) {
	    		scriptIndex--;
	    	}
	    
	    }
	    
	    public void IncrementState() {
	    	
	    	if ( scriptIndex < ( stateScript.Count - 1 ) ) {
	    		scriptIndex++;
	    	}
	    	
	    }
	    
	    public ArtworkState GetKeyStateAtScriptIndex( int index ) {
	    
	    	if (( index >= 0 ) && ( index < stateScript.Count )) {
	    		string id = stateScript[ index ];
	    		
	    		if ( id != ArtworkState.HoldCommand ) {
	    			return GetStateForId( stateScript[ index ] );
	    		}
	    	}
	    	
	    	return null;
		}
		
		public ArtworkState GetPriorNonHoldState(int index) {
			int i = 0;
			ArtworkState artworkState = null;
			for( i = ( index - 1 ); i >= 0; i-- ) {
				artworkState = GetStateForId( stateScript[ i ] ) as ArtworkState;
				if ( artworkState != null ) {
					if (stateScript[i] != ArtworkState.HoldCommand) {
						priorNonHoldStateIndex = i;
						priorNonHoldState = artworkState;
						return artworkState;
					}
				}
			}
			return null;
		}
	    
	    public ArtworkState GetStateAtScriptIndex( int index ) {
			ArtworkState state = null;
	    	if (( index >= 0 ) && ( index < stateScript.Count )) {
	    		string id = stateScript[ index ];
	    		if ( id == ArtworkState.HoldCommand ) {
					// if editing, ignore prior state cache since it doesn't play nice with editor scripts
					// when we make changes to the timeline
					if (Application.isEditor && !Application.isPlaying) {
		    			int i = 0;
		    			for( i = ( index - 1 ); i >= 0; i-- ) {
		    				state = GetStateForId( stateScript[ i ] );
		    				if ( state != null ) {
		    					return state;
		    				}
		    			}
					// if not editing, use cached prior state indices
					} else {
						if (index > priorHoldStateIndex + 1) {
							state = GetPriorNonHoldState(index);
						} else if (index >= priorNonHoldStateIndex) {
							state = priorNonHoldState;
						} else {
							state = GetPriorNonHoldState(index);
						}
						priorHoldStateIndex = index;
					}
	    		} else {
	    			return GetStateForId( stateScript[ index ] );
	    		}
	    	}
	    	return state;	
	    }
	    
	    public ArtworkState PreviousState() {

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
	    
	    public ArtworkState CurrentState() {
			
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
	    
	    public ArtworkState NextState() {
			
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
		
		public void MaintainScaleForZ() {
			
			// Maintain apparent scale of the artwork no matter its distance from the camera.
			// Sources: http://forums.adobe.com/message/4850091?tstart=30
			if (( panel != null ) && (PanoplyCore.panoplyRenderer != null)) {
				float d = Mathf.Abs( transform.position.z - panel.transform.position.z );
				float r = d / ( panel.camera.fieldOfView * ( PanoplyCore.panoplyRenderer.screenRect.height / panel.camera.pixelRect.height ));
				if (( r != Mathf.Infinity ) && !float.IsNaN(r)) {
					r *= scaleFactor;
					transform.localScale = new Vector3( r, r, r );
					ArtworkState cstate = CurrentState();
					if ( cstate != null ) {
						cstate.scale = transform.localScale;
					}
				}
			}
			
		}
	    
	    public void Update() {
			
			this.enabled = !(Application.isPlaying && (states.Count == 1));

	    	// scale management
			if (((priorNonHoldState != lastScaledState) || !Application.isPlaying) && maintainScale) {
				MaintainScaleForZ();
				lastScaledState = priorNonHoldState;
			}
	    
	    	/* -- StateManager update start --*/
			
			int targetStepProxy = ( int ) Mathf.Max ( Mathf.Floor( PanoplyCore.interpolatedStep ), Mathf.Min ( Mathf.Ceil ( PanoplyCore.interpolatedStep ), PanoplyCore.targetStep ));
			scriptIndex = targetStepProxy - startIndex;
			crossfade = PanoplyCore.interpolatedStep - targetStepProxy;

	    	ArtworkState stateA = null;
	    	ArtworkState stateB = null;
	    	
	    	if ( crossfade < 0 ) {
	    		stateA = PreviousState();
	    		stateB = CurrentState();
	    		progress = crossfade + 1;
	    		
	    	} else {
	    		stateA = CurrentState();
	    		stateB = NextState();
	    		progress = crossfade;
	    	}
	    	
	    	/* -- StateManager update end --*/

			bool okToCalc = true;
			if (Application.isPlaying) {
				if ((scriptIndex > 0) && (scriptIndex < (stateScript.Count-1))) {
					if ((stateScript[scriptIndex-1] == ArtworkState.HoldCommand) && (stateScript[scriptIndex] == ArtworkState.HoldCommand) && (stateScript[scriptIndex+1] == ArtworkState.HoldCommand)) {
						okToCalc = false;
					}
				}
			}

	    	if ( ( stateA != null ) && ( stateB != null ) && okToCalc ) {
	    		
	    		// interpolate color, position, rotation and scale
    			if ( spriteRenderer != null ) {
    				spriteRenderer.color = Color.Lerp(stateA.color, stateB.color, progress);
    			} else if (tempMaterial != null) {
    				artworkRenderer.sharedMaterial.color = Color.Lerp(stateA.color, stateB.color, progress);
    			}

				Vector3 pos = Vector3.Lerp(stateA.position, stateB.position, progress);
				switch ( positionType ) {

				case ArtworkPositionType.Panel:
					if ( panel != null ) {
						pos += panel.homePosition;
					}
					transform.position = pos;
					break;

				case ArtworkPositionType.Local:
					transform.localPosition = pos;
					break;

				case ArtworkPositionType.Global:
					transform.position = pos;
					break;

				}

    			var tmp_cs1 = transform.rotation;
                tmp_cs1.eulerAngles = Vector3.Lerp(stateA.rotation, stateB.rotation, progress);
                transform.rotation = tmp_cs1;
    			
				if ( !maintainScale ) {
    				transform.localScale = Vector3.Lerp(stateA.scale, stateB.scale, progress);
    			}

	    	}
	    	
	    }
	}
}