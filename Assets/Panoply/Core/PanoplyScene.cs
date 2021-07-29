using UnityEngine;
using System;
using System.Collections;
using Opertoon.Panoply;
#if UNITY_5_3_OR_NEWER
using UnityEngine.SceneManagement;
#endif

/**
 * The PanoplyScene class manages scene level metadata.
 * Copyright © Erik Loyer
 * erik@opertoon.com
 * Part of the Panoply engine
 */

namespace Opertoon.Panoply {
	public class PanoplyScene: MonoBehaviour {
	    
	    public int stepCount = 30;
		public int homePositionXSpacing = 50;
		public string previousSceneName = "";
		public bool previousSceneIsURL;
	    public string nextSceneName = "";
		public bool nextSceneIsURL;
		public bool advanceOnStart = true;
	    public float delayBeforeAdvance = 1.0f;
		public bool disableNavigation = false;
		public bool useLocalization = false;
		public AbstractLocalizer localizer;
	    
	    int initialTargetStep = 0;
	    bool hasLeftInitialTargetStep = false;

		private PanoplyController controller;
		private bool nextSceneCalled = false;
	    
	    public void Awake() {
	    
	    	PanoplyCore.targetStep = 0;
	        PanoplyCore.interpolatedStep = 0.0f;
	    	
	    }
	    
	    public IEnumerator Start() {

			// set the language to the system language if we're not running in the editor
			if ( useLocalization && ( localizer != null ) && !Application.isEditor ) {
				localizer.SetLanguage( Application.systemLanguage.ToString() );
			}

	    	int firstStep = 0;
	    	
	    	if ( advanceOnStart ) {
	    	
	    		yield return new WaitForSeconds ( delayBeforeAdvance );
	    		
	    		firstStep = 1;
	    		
	    	}
	    	
	    	string direction = PlayerPrefs.GetString( "SceneChangeDirection", "Forward" );
	    	
	    	if ( direction == "Forward" ) {
	    		PanoplyCore.interpolatedStep = 0.0f;
	    		PanoplyCore.targetStep = firstStep;
	    		
	    	} else if ( direction == "Backward" ) {
	    		PanoplyCore.interpolatedStep = ( float )( stepCount - 1 );
	    		PanoplyCore.targetStep = stepCount - 2;
	    	}
	    	
	    	initialTargetStep = PanoplyCore.targetStep;
	    	hasLeftInitialTargetStep = false;
			nextSceneCalled = false;

			controller = GetComponent<PanoplyController>();
	    
	    }
	    
	    public void OnApplicationQuit() {
	    	PlayerPrefs.SetString( "SceneChangeDirection", "Forward" );
	    }
	    
	    public void OnApplicationPause() {
	    	PlayerPrefs.SetString( "SceneChangeDirection", "Forward" );
	    	PlayerPrefs.Save();
	    }
	    
	    /*public FrameState getStateForId( PanelComponent pc, string id ) {
	    
	    	int i = 0;
	    	int n = 0;
	    	//var passiveMotionState : PassiveMotionState;
	    	
	    	switch ( pc ) {
	    	
	    		case PanelComponent.PanelFrame:
	    		FrameState frameState = null;
	    		n = frameStates.Length;
	    		for( i = 0; i < n; i++ ) {
	    			frameState = frameStates[ i ];
	    			if ( frameState.id == id ) {
	    				return frameState;
	    			}
	    		}
	    		break;
	    	
	    		/*case PanelComponent.PanelCamera:
	    		var cameraState : CameraState;
	    		n = cameraStates.Count;
	    		id = getCleanIdFromId( id );
	    		for ( i = 0; i < n; i++ ) {
	    			cameraState = cameraStates[ i ];
	    			if ( cameraState.id == id ) {
	    				return cameraState;
	    			}
	    		}
	    		break;
	    	
	    		case PanelComponent.PanelPassiveMotion:
	    		case PanelComponent.PanelPassiveMotionH:
	    		n = passiveMotionStatesH.Count;
	    		id = getCleanIdFromId( id );
	    		for ( i = 0; i < n; i++ ) {
	    			passiveMotionState = passiveMotionStatesH[ i ];
	    			if ( passiveMotionState.id == id ) {
	    				return passiveMotionState;
	    			}
	    		}
	    		break;
	    	
	    		case PanelComponent.PanelPassiveMotionV:
	    		n = passiveMotionStatesV.Count;
	    		id = getCleanIdFromId( id );
	    		for ( i = 0; i < n; i++ ) {
	    			passiveMotionState = passiveMotionStatesV[ i ];
	    			if ( passiveMotionState.id == id ) {
	    				return passiveMotionState;
	    			}
	    		}
	    		break;*
	    	
	    	}
	    	
	    	return null;
	    }*/
	    
	    public void Update() {
	    
	    	if ( !hasLeftInitialTargetStep ) {
	    		if ( PanoplyCore.targetStep != initialTargetStep ) {
	    			hasLeftInitialTargetStep = true;
	    		}
	    	}

			if (controller != null) {
				if ((Time.timeSinceLevelLoad > 3) && hasLeftInitialTargetStep && !controller.ignoreStepCount) {

					if (previousSceneName != "") {
						if (PanoplyCore.targetStep == 0) {
							if (((PanoplyCore.interpolatedStep - PanoplyCore.targetStep) < .01f) && !nextSceneCalled) {
								PlayerPrefs.SetString ("SceneChangeDirection", "Backward");
								nextSceneCalled = true;
								if (previousSceneIsURL) {
									Application.OpenURL (previousSceneName);
								} else {
#if UNITY_5_3_OR_NEWER
									SceneManager.LoadScene (previousSceneName);
#else
								Application.LoadLevel( previousSceneName );
#endif
								}
							}
						}
					}

					if (nextSceneName != "") {
						if (PanoplyCore.targetStep == (stepCount - 1)) {
							if (((PanoplyCore.targetStep - PanoplyCore.interpolatedStep) < .01f) && !nextSceneCalled) {
								PlayerPrefs.SetString ("SceneChangeDirection", "Forward");
								nextSceneCalled = true;
								if (nextSceneIsURL) {
									Application.OpenURL (nextSceneName);
								} else {
#if UNITY_5_3_OR_NEWER
									SceneManager.LoadScene (nextSceneName);
#else
								Application.LoadLevel( nextSceneName );
#endif
								}
							}
						}
					}
				}
			}

		}
	}
}