using UnityEngine;
using System;
using Opertoon.Panoply;

/**
 * The PanoplyController class handles user input globally.
 * Copyright © Erik Loyer
 * erik@opertoon.com
 * Part of the Panoply engine
 */

namespace Opertoon.Panoply {

	public enum AxisGesturePhase
	{
		None,
		Began,
		InProgress,
		Ended
	}

	public enum PanoplyGesture {
		SwipeUp,
		SwipeRight,
		SwipeDown,
		SwipeLeft
	}

	public class PanoplyController: MonoBehaviour {
	    
	    public int swipeTouchCount = 1;						// How many touches are required to trigger a swipe
	    public float gestureTriggerDist = 25.0f;			// Distance a gesture must traverse before triggering a new step
	    public float gestureRate = 5.0f;					// How quickly gestures are executed
	    public bool passiveInput = true;					// Whether passive input is accepted
		public bool keyboardInput = true;					// Whether keyboard input is accepted
		public bool ignoreStepCount = false;
		public DeviceType deviceType;

	    [HideInInspector]
	    public float horizontalTilt;
	    
	    [HideInInspector]
	    public float verticalTilt;
	    
	    [HideInInspector]
	    public PanoplyGesture lastGesture;
	    
	    Vector2 gestureStart;
	    bool ignoreCurrentGesture = false;

		public Vector2 gyroCenter = new Vector2 (0, 25);
		public Vector2 gyroRange = new Vector2 (30, 10);
		public Vector2 accelerometerRange = new Vector2 (.2f, .5f);
		public float accelerometerSmoothing = 4f;
	    
	    Vector2 acceleration = new Vector2(0.0f,0.0f);
		Vector2 lastAxisPosition;
	    
	    Panel[] panels;
	    
	    PanoplyEventManager eventManager;

		private Vector2 _accelerometerMin = new Vector2 (-.3f, -.25f);

	    public void Start() {
	    
	    	panels = FindObjectsOfType( typeof( Panel ) ) as Panel[];
	    	
	    	eventManager = GetComponent<PanoplyEventManager>();

			deviceType = SystemInfo.deviceType;

	    }
	       
	    /**
	     * Returns the current value of the specified input.
	     *
	     * @param	input		The input type to query.
	     */
	    public void UpdatePassiveInput() {

			if (passiveInput) {
				switch (PanoplyCore.passiveInputType) {

				case PassiveInputType.Gyroscope:
					float angleDiff = (Input.gyro.attitude.eulerAngles.y - gyroCenter.x + 180) % 360 - 180;
					horizontalTilt = -Mathf.InverseLerp (-gyroRange.x, gyroRange.x, angleDiff);
					angleDiff = (Input.gyro.attitude.eulerAngles.x - gyroCenter.y + 180) % 360 - 180;
					verticalTilt = -Mathf.InverseLerp (-gyroRange.y, gyroRange.y, angleDiff);
					break;

				case PassiveInputType.Accelerometer:
					if (Input.acceleration.x < _accelerometerMin.x) {
						_accelerometerMin.x = Mathf.Min (_accelerometerMin.x, Input.acceleration.x);
					} else if (Input.acceleration.x > (_accelerometerMin.x + accelerometerRange.x)) {
						_accelerometerMin.x = Input.acceleration.x - accelerometerRange.x;
					}
					if (Input.acceleration.y < _accelerometerMin.y) {
						_accelerometerMin.y = Mathf.Min (_accelerometerMin.y, Input.acceleration.y);
					} else if (Input.acceleration.y > (_accelerometerMin.y + accelerometerRange.y)) {
						_accelerometerMin.y = Input.acceleration.y - accelerometerRange.y;
					}
					acceleration.x = (Mathf.InverseLerp (_accelerometerMin.x + accelerometerRange.x, _accelerometerMin.x, Input.acceleration.x) * 2.0f) - 1.0f;
					acceleration.y = (Mathf.InverseLerp (_accelerometerMin.y, _accelerometerMin.y + accelerometerRange.y, Input.acceleration.y) * 2.0f) - 1.0f;
					if (Input.acceleration.z < 0) {
						acceleration *= -1.0f;
					}
					verticalTilt = Mathf.Lerp (verticalTilt, acceleration.y, Time.deltaTime * (1.0f / accelerometerSmoothing));
					horizontalTilt = Mathf.Lerp (horizontalTilt, acceleration.x, Time.deltaTime * (1.0f / accelerometerSmoothing));
					break;

				case PassiveInputType.Mouse:
					horizontalTilt = (Mathf.Clamp ((Input.mousePosition.x / Screen.width), 0.0f, 1.0f) - 0.5f) * 2.0f;
					verticalTilt = (Mathf.Clamp ((Input.mousePosition.y / Screen.height), 0.0f, 1.0f) - 0.5f) * 2.0f;
					break;

				}
	    	} else {
	    		horizontalTilt = 0.0f;
	    		verticalTilt = 0.0f;
	    	}
	    	
	    }  
	    
	    /**
	     * Returns true if the specified point falls within the bounds of any
	     * frame that is set to intercept interactions.
	     *
	     * @param point		The point to test.
	     * @return			True if the point is contained by an intercepting frame.
	     */
	    bool PointInAnyInterceptingFrame( Vector2 point ) {
	    
	    	bool result = false;
	    	int i = 0;
	    	int n = 0;
	    	
	    	n = panels.Length;
	    	Panel panel = null;
	    	for( i = 0; i < n; i++ ) {
	    		panel = panels[ i ];
				if ((panel.interceptInteraction && panel.camera.pixelRect.Contains( point )) || PanoplyCore.scene.disableNavigation) {
	    			result = true;
	    			break;
	    		}
	    	}	
	    	
	    	return result;
	    }
	    
	    public void Update() {
	    
	    	Touch touch = new Touch();
	    	Vector2 gestureDelta = Vector2.zero;
	    	float gestureDeltaMultiplier = 1.0f;
	    	bool gotInput = false;
	    	Vector2 averagedPosition = Vector2.zero;
			Vector2 axisPosition = Vector2.zero;
	    	int i = 0;
			int priorTargetStep;
	    	
	    	UpdatePassiveInput();

			switch (deviceType) {

			case DeviceType.Console:
			case DeviceType.Unknown:
				axisPosition.x = Input.GetAxis ("Horizontal");
				axisPosition.y = Input.GetAxis ("Vertical");
				AxisGesturePhase gesturePhase = AxisGesturePhase.None;

				if (axisPosition.x != 0 || axisPosition.y != 0) {
					if (lastAxisPosition.x == 0 && lastAxisPosition.y == 0) {
						gesturePhase = AxisGesturePhase.Began;
					} else {
						gesturePhase = AxisGesturePhase.InProgress;
					}
				} else {
					if (lastAxisPosition.x != 0 || lastAxisPosition.y != 0) {
						gesturePhase = AxisGesturePhase.Ended;
					}
				}

				switch (gesturePhase) {

				case AxisGesturePhase.Began:
					gotInput = true;
					gestureStart = axisPosition;
					ignoreCurrentGesture = false;
					break;

				case AxisGesturePhase.InProgress:
					if (!ignoreCurrentGesture) {

						gotInput = true;
						gestureDelta = axisPosition - gestureStart;

						if (Mathf.Abs (gestureDelta.x) > Mathf.Abs (gestureDelta.y)) {
							if (gestureDelta.x > 0) {
								gestureDeltaMultiplier = -1.0f;
								lastGesture = PanoplyGesture.SwipeRight;
							} else {
								lastGesture = PanoplyGesture.SwipeLeft;
							}
						} else {
							if (gestureDelta.y < 0) {
								gestureDeltaMultiplier = -1.0f;
								lastGesture = PanoplyGesture.SwipeDown;
							} else {
								lastGesture = PanoplyGesture.SwipeUp;
							}
						}

						PanoplyCore.interpolatedStep = Mathf.Lerp (PanoplyCore.interpolatedStep, PanoplyCore.targetStep + Mathf.Clamp (gestureDelta.magnitude * gestureDeltaMultiplier, -.5f, .5f) / .5f, Time.deltaTime * gestureRate);
					}
					break;

				case AxisGesturePhase.Ended:
					if (!ignoreCurrentGesture) {
						gotInput = true;
						priorTargetStep = PanoplyCore.targetStep;
						if ((PanoplyCore.interpolatedStep - PanoplyCore.targetStep) > .07f) {
							if (ignoreStepCount) {
								PanoplyCore.targetStep++;
							} else {
								PanoplyCore.targetStep = Mathf.Min (PanoplyCore.targetStep + 1, PanoplyCore.scene.stepCount - 1);
							}
							if (PanoplyCore.targetStep != priorTargetStep) {
								eventManager.HandleTargetStepChanged (priorTargetStep, PanoplyCore.targetStep);
							}

						} else if ((PanoplyCore.targetStep - PanoplyCore.interpolatedStep) > .07f) {
							if (ignoreStepCount) {
								PanoplyCore.targetStep--;
							} else {
								PanoplyCore.targetStep = Mathf.Max (PanoplyCore.targetStep - 1, 0);
							}
							if (PanoplyCore.targetStep != priorTargetStep) {
								eventManager.HandleTargetStepChanged (priorTargetStep, PanoplyCore.targetStep);
							}
						}

						gestureStart = new Vector2 (-1.0f, -1.0f);
					}
					break;
				}

				lastAxisPosition = axisPosition;
				break;

			case DeviceType.Desktop:
				// if the mouse button was clicked, then record its location
				if (Input.GetMouseButtonDown (0)) {

					// if the click is contained by an frame set to intercept interactions, or if the menu is open, then ignore this gesture
					if (!PointInAnyInterceptingFrame ((Vector2)Input.mousePosition) && !PanoplyCore.menu.isOpen) {
						gotInput = true;
						gestureStart = (Vector2)Input.mousePosition;
						ignoreCurrentGesture = false;

					} else {
						if (!PanoplyCore.menu.ScreenPointIsInsideMenu((Vector2)Input.mousePosition)) {
							PanoplyCore.menu.SetMenuOpen (false);
						}
						ignoreCurrentGesture = true;
					}

					// if the mouse button was released, then calculate our new step index
				} else if (Input.GetMouseButtonUp (0)) {

					if (!ignoreCurrentGesture) {

						gotInput = true;

						priorTargetStep = PanoplyCore.targetStep;
						if ((PanoplyCore.interpolatedStep - PanoplyCore.targetStep) > .07f) {
							if (ignoreStepCount) {
								PanoplyCore.targetStep++;
							} else {
								PanoplyCore.targetStep = Mathf.Min (PanoplyCore.targetStep + 1, PanoplyCore.scene.stepCount - 1);
							}
							if (PanoplyCore.targetStep != priorTargetStep) {
								eventManager.HandleTargetStepChanged (priorTargetStep, PanoplyCore.targetStep);
							}

						} else if ((PanoplyCore.targetStep - PanoplyCore.interpolatedStep) > .07f) {
							if (ignoreStepCount) {
								PanoplyCore.targetStep--;
							} else {
								PanoplyCore.targetStep = Mathf.Max (PanoplyCore.targetStep - 1, 0);
							}
							if (PanoplyCore.targetStep != priorTargetStep) {
								eventManager.HandleTargetStepChanged (priorTargetStep, PanoplyCore.targetStep);
							}
						}

						gestureStart = new Vector2 (-1.0f, -1.0f);

					}

					// if the mouse is currently being dragged, recalculate the current gesture progress based on the distance the mouse has moved
				} else if (Input.GetMouseButton (0)) {

					if (!ignoreCurrentGesture) {
						gotInput = true;

						// in case the down phase gets skipped
						if (gestureStart.x == -1) {
							gestureStart = (Vector2)Input.mousePosition;
						}

						gestureDelta = (Vector2)(Input.mousePosition - (Vector3)gestureStart);

						if (Mathf.Abs (gestureDelta.x) > Mathf.Abs (gestureDelta.y)) {
							if (gestureDelta.x > 0) {
								gestureDeltaMultiplier = -1.0f;
								lastGesture = PanoplyGesture.SwipeRight;
							} else {
								lastGesture = PanoplyGesture.SwipeLeft;
							}
						} else {
							if (gestureDelta.y < 0) {
								gestureDeltaMultiplier = -1.0f;
								lastGesture = PanoplyGesture.SwipeDown;
							} else {
								lastGesture = PanoplyGesture.SwipeUp;
							}
						}

						PanoplyCore.interpolatedStep = Mathf.Lerp (PanoplyCore.interpolatedStep, PanoplyCore.targetStep + Mathf.Clamp (gestureDelta.magnitude * gestureDeltaMultiplier, -gestureTriggerDist, gestureTriggerDist) / gestureTriggerDist, Time.deltaTime * gestureRate);
					}
				}
				if (keyboardInput) {
					if (Input.GetKeyDown ("left")) {
						PanoplyCore.DecrementStep (ignoreStepCount);
					} else if (Input.GetKeyDown ("right")) {
						PanoplyCore.IncrementStep (ignoreStepCount);
					} else if (Input.GetKeyDown ("up")) {
						if (Application.isEditor) {
							PanoplyCore.GoToFirstStep ();
						}
					} else if (Input.GetKeyDown ("down")) {
						if (Application.isEditor) {
							PanoplyCore.GoToLastStep ();
						}
					}
				}
				break;

			case DeviceType.Handheld:
				if (Input.touchCount == swipeTouchCount) {

					TouchPhase dominantPhase = TouchPhase.Canceled;

					for (i = 0; i < swipeTouchCount; i++) {
						touch = Input.touches [i];
						averagedPosition += Input.touches [i].position;
						if (dominantPhase == TouchPhase.Canceled) {
							if (touch.phase == TouchPhase.Began) {
								dominantPhase = touch.phase;
							} else if (touch.phase == TouchPhase.Ended) {
								dominantPhase = touch.phase;
							}
						} else if (dominantPhase == TouchPhase.Ended) {
							if (touch.phase == TouchPhase.Began) {
								dominantPhase = touch.phase;
							}
						}
					}
					averagedPosition /= swipeTouchCount;

					// if the touch is starting, then record its location
					if (dominantPhase == TouchPhase.Began) {

						// if the touch is contained by an frame set to intercept interactions, or the menu is open, then ignore this gesture
						if (!PointInAnyInterceptingFrame (averagedPosition) && !PanoplyCore.menu.isOpen) {
							gotInput = true;
							gestureStart = averagedPosition;
							ignoreCurrentGesture = false;

						} else {
							if (!PanoplyCore.menu.ScreenPointIsInsideMenu(averagedPosition)) {
								PanoplyCore.menu.SetMenuOpen (false);
							}
							ignoreCurrentGesture = true;
						}

						// if the touch is ending, then calculate our new step index
					} else if (dominantPhase == TouchPhase.Ended) {

						if (!ignoreCurrentGesture) {
							gotInput = true;
							priorTargetStep = PanoplyCore.targetStep;
							if ((PanoplyCore.interpolatedStep - PanoplyCore.targetStep) > .07f) {
								if (ignoreStepCount) {
									PanoplyCore.targetStep++;
								} else {
									PanoplyCore.targetStep = Mathf.Min (PanoplyCore.targetStep + 1, PanoplyCore.scene.stepCount - 1);
								}
								if (PanoplyCore.targetStep != priorTargetStep) {
									eventManager.HandleTargetStepChanged (priorTargetStep, PanoplyCore.targetStep);
								}

							} else if ((PanoplyCore.targetStep - PanoplyCore.interpolatedStep) > .07f) {
								if (ignoreStepCount) {
									PanoplyCore.targetStep--;
								} else {
									PanoplyCore.targetStep = Mathf.Max (PanoplyCore.targetStep - 1, 0);
								}
								if (PanoplyCore.targetStep != priorTargetStep) {
									eventManager.HandleTargetStepChanged (priorTargetStep, PanoplyCore.targetStep);
								}
							}

							gestureStart = new Vector2 (-1.0f, -1.0f);
						}

						// otherwise, recalculate the current gesture progress based on distance the touch has moved
					} else {

						if (!ignoreCurrentGesture) {

							gotInput = true;

							// in case the began phase gets skipped
							if (gestureStart.x == -1) {
								gestureStart = averagedPosition;
							}

							gestureDelta = averagedPosition - gestureStart;

							if (Mathf.Abs (gestureDelta.x) > Mathf.Abs (gestureDelta.y)) {
								if (gestureDelta.x > 0) {
									gestureDeltaMultiplier = -1.0f;
									lastGesture = PanoplyGesture.SwipeRight;
								} else {
									lastGesture = PanoplyGesture.SwipeLeft;
								}
							} else {
								if (gestureDelta.y < 0) {
									gestureDeltaMultiplier = -1.0f;
									lastGesture = PanoplyGesture.SwipeDown;
								} else {
									lastGesture = PanoplyGesture.SwipeUp;
								}
							}

							PanoplyCore.interpolatedStep = Mathf.Lerp (PanoplyCore.interpolatedStep, PanoplyCore.targetStep + Mathf.Clamp (gestureDelta.magnitude * gestureDeltaMultiplier, -gestureTriggerDist, gestureTriggerDist) / gestureTriggerDist, Time.deltaTime * gestureRate);
						}
					}
				}
				break;

			}

			/*
	    	switch ( PanoplyCore.passiveInputType ) {
	    		
	    		/* -- TOUCH INPUT -- *
	    		
	    		case PassiveInputType.Accelerometer:
	    		
	    		if ( Input.touchCount == swipeTouchCount ) {
	    			
	    			TouchPhase dominantPhase = TouchPhase.Canceled;
	    			
	    			for( i = 0; i < swipeTouchCount; i++ ) {
	    				touch = Input.touches[ i ];
	    				averagedPosition += Input.touches[ i ].position;
	    				if ( dominantPhase == TouchPhase.Canceled ) {
	    					if ( touch.phase == TouchPhase.Began ) {
	    						dominantPhase = touch.phase;
	    					} else if ( touch.phase == TouchPhase.Ended ) {
	    						dominantPhase = touch.phase;
	    					}
	    				} else if ( dominantPhase == TouchPhase.Ended ) {
	    					if ( touch.phase == TouchPhase.Began ) {
	    						dominantPhase = touch.phase;
	    					}
	    				}
	    			}
	    			averagedPosition /= swipeTouchCount;
	    			
	    			// if the touch is starting, then record its location
	    			if (dominantPhase == TouchPhase.Began) {
	    			
	    				// if the touch is contained by an frame set to intercept interactions, then ignore this gesture
	    				if ( !PointInAnyInterceptingFrame( averagedPosition ) ) {
	    					gotInput = true;
	    					gestureStart = averagedPosition;
	    					ignoreCurrentGesture = false;
	    					
	    				} else {
	    					ignoreCurrentGesture = true;
	    				}
	    				
	    			// if the touch is ending, then calculate our new step index
	    			} else if (dominantPhase == TouchPhase.Ended) {
	    			
	    				if ( !ignoreCurrentGesture ) {
	    					gotInput = true;
							priorTargetStep = PanoplyCore.targetStep;
	    					if ( ( PanoplyCore.interpolatedStep - PanoplyCore.targetStep ) > .07f ) {
								if ( ignoreStepCount ) {
									PanoplyCore.targetStep++;
								} else {
									PanoplyCore.targetStep = Mathf.Min( PanoplyCore.targetStep + 1, PanoplyCore.scene.stepCount - 1 );
								}
								if (PanoplyCore.targetStep != priorTargetStep) {
									eventManager.HandleTargetStepChanged( priorTargetStep, PanoplyCore.targetStep );
								}
	    						
	    					} else if ( ( PanoplyCore.targetStep - PanoplyCore.interpolatedStep ) > .07f ) {
								if ( ignoreStepCount ) {
									PanoplyCore.targetStep--;
								} else {
									PanoplyCore.targetStep = Mathf.Max( PanoplyCore.targetStep - 1, 0 );
								}
								if (PanoplyCore.targetStep != priorTargetStep) {
									eventManager.HandleTargetStepChanged( priorTargetStep, PanoplyCore.targetStep );
								}
	    					}
	    					
	    					gestureStart = new Vector2( -1.0f, -1.0f );
	    				}
	    				
	    			// otherwise, recalculate the current gesture progress based on distance the touch has moved
	    			} else {
	    			
	    				if ( !ignoreCurrentGesture ) {
	    				
	    					gotInput = true;
	    
	    					// in case the began phase gets skipped
	    					if ( gestureStart.x == -1 ) {
	    						gestureStart = averagedPosition;
	    					}
	    					
	    					gestureDelta = averagedPosition - gestureStart;
	    				
	    					if ( Mathf.Abs( gestureDelta.x ) > Mathf.Abs( gestureDelta.y ) ) {
	    						if ( gestureDelta.x > 0 ) {
	    							gestureDeltaMultiplier = -1.0f;
	    							lastGesture = PanoplyGesture.SwipeRight;
	    						} else {
	    							lastGesture = PanoplyGesture.SwipeLeft;
	    						}
	    					} else {
	    						if ( gestureDelta.y < 0 ) {
	    							gestureDeltaMultiplier = -1.0f;
	    							lastGesture = PanoplyGesture.SwipeDown;
	    						} else {
	    							lastGesture = PanoplyGesture.SwipeUp;
	    						}
	    					}
	    					
	    					PanoplyCore.interpolatedStep = Mathf.Lerp( PanoplyCore.interpolatedStep, PanoplyCore.targetStep + Mathf.Clamp( gestureDelta.magnitude * gestureDeltaMultiplier, -gestureTriggerDist, gestureTriggerDist ) / gestureTriggerDist, Time.deltaTime * gestureRate );
	    				}
	    			}
	    		}
	    		
	    		break;
	    		
	    			
	    		/* -- MOUSE INPUT -- *
	    		
	    		case PassiveInputType.Mouse:
	    		
	    		// if the mouse button was clicked, then record its location
	    		if ( Input.GetMouseButtonDown( 0 ) ) {
	    		
	    			// if the click is contained by an frame set to intercept interactions, then ignore this gesture
	    			if ( !PointInAnyInterceptingFrame( ( Vector2 )Input.mousePosition ) ) {
	    				gotInput = true;
	    				gestureStart = ( Vector2 )Input.mousePosition;
	    				ignoreCurrentGesture = false;
	    			
	    			} else {
	    				ignoreCurrentGesture = true;
	    			}
	    		
	    		// if the mouse button was released, then calculate our new step index
	    		} else if ( Input.GetMouseButtonUp( 0 ) ) {
	    		
	    			if ( !ignoreCurrentGesture ) {
	    			
	    				gotInput = true;
	    			
						priorTargetStep = PanoplyCore.targetStep;
	    				if ( ( PanoplyCore.interpolatedStep - PanoplyCore.targetStep ) > .07f ) {
							if ( ignoreStepCount ) {
								PanoplyCore.targetStep++;
							} else {
								PanoplyCore.targetStep = Mathf.Min( PanoplyCore.targetStep + 1, PanoplyCore.scene.stepCount - 1 );
							}
							if (PanoplyCore.targetStep != priorTargetStep) {
								eventManager.HandleTargetStepChanged( priorTargetStep, PanoplyCore.targetStep );
							}
	    					
	    				} else if ( ( PanoplyCore.targetStep - PanoplyCore.interpolatedStep ) > .07f ) {
							if ( ignoreStepCount ) {
								PanoplyCore.targetStep--;
							} else {
								PanoplyCore.targetStep = Mathf.Max( PanoplyCore.targetStep - 1, 0 );
							}
							if (PanoplyCore.targetStep != priorTargetStep) {
								eventManager.HandleTargetStepChanged( priorTargetStep, PanoplyCore.targetStep );
							}
	    				}
	    				
	    				gestureStart = new Vector2( -1.0f, -1.0f );
	    			
	    			}
	    			
	    		// if the mouse is currently being dragged, recalculate the current gesture progress based on the distance the mouse has moved
	    		} else if ( Input.GetMouseButton( 0 ) ) {

	    			if ( !ignoreCurrentGesture ) {
	    				gotInput = true;
	    			
	    				// in case the down phase gets skipped
	    				if ( gestureStart.x == -1 ) {
	    					gestureStart = ( Vector2 )Input.mousePosition;
	    				}
	    				
	    				gestureDelta = ( Vector2 )( Input.mousePosition - ( Vector3 )gestureStart );
	    				
	    				if ( Mathf.Abs( gestureDelta.x ) > Mathf.Abs( gestureDelta.y ) ) {
	    					if ( gestureDelta.x > 0 ) {
	    						gestureDeltaMultiplier = -1.0f;
	    						lastGesture = PanoplyGesture.SwipeRight;
	    					} else {
	    						lastGesture = PanoplyGesture.SwipeLeft;
	    					}
	    				} else {
	    					if ( gestureDelta.y < 0 ) {
	    						gestureDeltaMultiplier = -1.0f;
	    						lastGesture = PanoplyGesture.SwipeDown;
	    					} else {
	    						lastGesture = PanoplyGesture.SwipeUp;
	    					}
	    				}
	    				
	    				PanoplyCore.interpolatedStep = Mathf.Lerp( PanoplyCore.interpolatedStep, PanoplyCore.targetStep + Mathf.Clamp( gestureDelta.magnitude * gestureDeltaMultiplier, -gestureTriggerDist, gestureTriggerDist ) / gestureTriggerDist, Time.deltaTime * gestureRate );
	    			}
	    		}
	    		
	    		break;
	    	
	    	}*/

			if (!ignoreStepCount && PanoplyCore.scene != null) {
				PanoplyCore.interpolatedStep = Mathf.Clamp(PanoplyCore.interpolatedStep, 0, PanoplyCore.scene.stepCount - 1);
			}
	    	
			/*if (keyboardInput) {
		    	if ( Input.GetKeyDown( "left" )) {
		    		PanoplyCore.DecrementStep( ignoreStepCount );
		    	} else if ( Input.GetKeyDown( "right" )) {
					PanoplyCore.IncrementStep( ignoreStepCount );
		    	} else if ( Input.GetKeyDown( "up" )) {
		    		if ( Application.isEditor ) {
		    			PanoplyCore.GoToFirstStep();
		    		}
		    	} else if ( Input.GetKeyDown( "down" )) {
		    		if ( Application.isEditor ) {
		    			PanoplyCore.GoToLastStep();
		    		}
		    	}
			}*/
	    	
	    	// No input; head for target step
	    	if ( !gotInput ) {
	    		PanoplyCore.interpolatedStep = Mathf.Lerp( PanoplyCore.interpolatedStep,  ( float )PanoplyCore.targetStep, Time.deltaTime * gestureRate);
	    	}
	    
	    }
	}
}
