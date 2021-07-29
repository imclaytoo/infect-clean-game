using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Opertoon.Panoply;
using UnityEngine.UI;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using UnityEditor;
#endif

/**
 * Panel
 * Manages all elements that go into creating a panel.
 * Copyright © Erik Loyer
 * erik@opertoon.com
 * Part of the Panoply engine
 */

namespace Opertoon.Panoply {

	[ExecuteInEditMode()]
	public class SimplePanel: MonoBehaviour
	{

		public Vector3 homePosition;

		public Vector2 gridSize = new Vector2 (3, 2);
		public Rect gridPosition = new Rect(0, 0, 3, 2);
		public Border margins = new Border (10, 10, 10, 10);
		public float borderSize;
		public Color borderColor;
		public Color matteColor;
		public bool interceptInteraction = false;                           // Does this frame prevent global controller interactions?
		public bool disableCameraControl = false;                           // Should the panel control its camera?

		[HideInInspector]
		public CameraState cameraState;

		[HideInInspector]
		public PassiveMotionState passiveMotionHState;

		[HideInInspector]
		public PassiveMotionState passiveMotionVState;

		[HideInInspector]
		public bool preserveFraming = false;                                // Should the panel attempt to maintain framing of objects at a certain distance from the camera?

		[HideInInspector]
		public float framingDistance = 5;									// At what distance from the camera should the panel attempt to maintain framing?

		[HideInInspector]
		public Rect frameRect;

		[HideInInspector]
		new public Camera camera;

		[HideInInspector]
		public bool lookAtEnabled = false;
	    
	    Vector3 lastCameraPosition;
	    Quaternion lastCameraRotation;
	    
	    private PanoplyController controller;
		private Rect screenRect = new Rect( 0, 0, Screen.width, Screen.height );
        private PanelEffects _panelEffect;
		private Vector3 _centeringOffset;
	    
	    [HideInInspector]
	    public Vector3 passiveMotionTranslationH;
	    
	    [HideInInspector]
	    public Vector3 passiveMotionRotationH;
	    
	    [HideInInspector]
	    public Vector3 passiveMotionTranslationV;
	    
	    [HideInInspector]
	    public Vector3 passiveMotionRotationV;

		[HideInInspector]
		public float pxTop = -1.0f;

		[HideInInspector]
		public float pxRight = -1.0f;

		[HideInInspector]
		public float pxBottom = -1.0f;

		[HideInInspector]
		public float pxLeft = -1.0f;

		[HideInInspector]
		public float pxWidth = 0.0f;

		[HideInInspector]
		public float pxHeight = 0.0f;

		[HideInInspector]
		public float pxCenterH = -1.0f;

		[HideInInspector]
		public float pxCenterV = -1.0f;

		public void Awake() {

			_centeringOffset = new Vector3 ();

		}

		public void Start() {

			//StartCoroutine(SetupRenderTexture());

			camera = GetComponent<Camera> ();

			/* -- PASSIVE MOTION -- */

			if (homePosition == Vector3.zero) {
				float maxHomePositionX = 0;
				Panel[] panels = FindObjectsOfType(typeof(Panel)) as Panel[];
				foreach (Panel panel in panels) {
					maxHomePositionX = Mathf.Max(maxHomePositionX, panel.homePosition.x);
				}
				SimplePanel [] simplePanels = FindObjectsOfType (typeof (SimplePanel)) as SimplePanel [];
				foreach (SimplePanel panel in simplePanels) {
					maxHomePositionX = Mathf.Max (maxHomePositionX, panel.homePosition.x);
				}
				PanoplyScene scene = null;
				PanoplyScene [] scenes = FindObjectsOfType (typeof (PanoplyScene)) as PanoplyScene [];
				if (scenes.Length > 0) {
					scene = scenes [0];
				}
				homePosition = new Vector3(maxHomePositionX + scene.homePositionXSpacing, 0, 0);
            }

			if (GraphicsSettings.renderPipelineAsset == null) {
				_panelEffect = gameObject.GetComponent<PanelEffects> ();
				if (_panelEffect == null) {
					_panelEffect = gameObject.AddComponent<PanelEffects> ();
					_panelEffect.border = new Border ();
				}
			}

        }

	    void CalculatePassiveMotion( PanelComponent pc ) {
	    
	    	if ( controller == null ) {
	    		controller = ( PanoplyController )FindObjectOfType( typeof( PanoplyController ) );
	    	}
	    
	    	PassiveMotionState state = null;
	    
	    	if ( pc == PanelComponent.PanelPassiveMotionH ) {
	    		state = passiveMotionHState;
	    	} else if ( pc == PanelComponent.PanelPassiveMotionV ) {
	    		state = passiveMotionVState;
	    	}
	    
	    	Vector3 translation = Vector3.zero;
	    	Vector3 rotation = Vector3.zero;
	    	float passiveMotionOutput = 0.0f;

	    	if (state != null) {
	    		
	    		// INTERPOLATE INPUT VARIANCES
	    		
	    		// loop through each variance and apply its affects to the specified properties
	    		switch ( state.input ) {
	    		
	    			case PassiveMotionControllerInput.HorizontalTilt:
	    			passiveMotionOutput = state.GetOutput( controller.horizontalTilt );
	    			break;
	    			
	    			case PassiveMotionControllerInput.VerticalTilt:
					passiveMotionOutput = state.GetOutput( controller.verticalTilt );
					break;
	    		
	    		}
	    		switch ( state.output ) {
	    		
	    			case PassiveMotionOutputType.Position:
	    			if (state.outputProperty == PassiveMotionOutputProperty.X) {
	    				translation.x = passiveMotionOutput;
	    			} else if (state.outputProperty == PassiveMotionOutputProperty.Y) {
	    				translation.y = passiveMotionOutput;
	    			} else if (state.outputProperty == PassiveMotionOutputProperty.Z) {
	    				translation.z = passiveMotionOutput;
	    			}
	    			break;
	    			
	    			case PassiveMotionOutputType.Rotation:
	    			if (state.outputProperty == PassiveMotionOutputProperty.X) {
	    				rotation.x = passiveMotionOutput;
	    			} else if (state.outputProperty == PassiveMotionOutputProperty.Y) {
	    				rotation.y = passiveMotionOutput;
	    			} else if (state.outputProperty == PassiveMotionOutputProperty.Z) {
	    				rotation.z = passiveMotionOutput;
	    			}
	    			break;
	    			
	    		}
	    
	    		if ( pc == PanelComponent.PanelPassiveMotionH ) {
	    			passiveMotionTranslationH = Vector3.Lerp( passiveMotionTranslationH, translation, Time.deltaTime * 10);
	    			passiveMotionRotationH = Vector3.Lerp( passiveMotionRotationH, rotation, Time.deltaTime * 10);
	    		} else if ( pc == PanelComponent.PanelPassiveMotionV ) {
	    			passiveMotionTranslationV = Vector3.Lerp( passiveMotionTranslationV, translation, Time.deltaTime * 10);
	    			passiveMotionRotationV = Vector3.Lerp( passiveMotionRotationV, rotation, Time.deltaTime * 10);
	    		}
	    
	    	}
	    
	    }

		public void UpdateFrameRect (bool includeMargins = true)
		{

			pxTop = -1.0f;
			pxRight = -1.0f;
			pxBottom = -1.0f;
			pxLeft = -1.0f;
			pxWidth = 0.0f;
			pxHeight = 0.0f;
			pxCenterH = -1.0f;
			pxCenterV = -1.0f;

			if (PanoplyCore.panoplyRenderer != null) {

				screenRect = PanoplyCore.panoplyRenderer.screenRect;

				float unitWidth = screenRect.width / (float)gridSize.x;
				float unitHeight = screenRect.height / (float)gridSize.y;

				pxWidth = unitWidth * gridPosition.width;
				pxHeight = unitHeight * gridPosition.height;
				pxLeft = unitWidth * gridPosition.x;
				pxRight = pxLeft + pxWidth;
				pxTop = screenRect.height - (unitHeight * gridPosition.y);
				pxBottom = pxTop - pxHeight;

				float marginLeftProxy = 0.0f;
				float marginRightProxy = 0.0f;
				float marginTopProxy = 0.0f;
				float marginBottomProxy = 0.0f;

				if (includeMargins) {
					marginLeftProxy = (margins.left * 2) * PanoplyCore.resolutionScale;
					marginRightProxy = (margins.right * 2) * PanoplyCore.resolutionScale;
					marginTopProxy = (margins.top * 2) * PanoplyCore.resolutionScale;
					marginBottomProxy = (margins.bottom * 2) * PanoplyCore.resolutionScale;
				}

				// make sure width and height aren't less than 1 and that margins aren't larger than width or height
				pxWidth = Mathf.Max (1.0f, pxRight - marginRightProxy - (pxLeft + marginLeftProxy));
				pxHeight = Mathf.Max (1.0f, pxTop - marginTopProxy - (pxBottom + marginBottomProxy));

				frameRect.x = pxLeft + Mathf.Min (marginLeftProxy, pxWidth) + screenRect.x;
				frameRect.y = pxBottom + Mathf.Min (marginBottomProxy, pxHeight) + screenRect.y;
				frameRect.width = pxWidth;
				frameRect.height = pxHeight;

			} else {
				frameRect = Rect.zero;
			}

		}

		public void Update() {

			UpdateFrameRect ();

			if ((screenRect).Overlaps (frameRect) && (frameRect.width >= 1) && (frameRect.height >= 1) && (((frameRect.x - screenRect.x) + frameRect.width) >= 1) && (((frameRect.y - screenRect.y) + frameRect.height) >= 1) && ((screenRect.height - (frameRect.y - screenRect.y)) >= 1) && ((screenRect.width - (frameRect.x - screenRect.x)) >= 1)) {
				camera.pixelRect = frameRect;
				camera.enabled = true;
			} else {
				camera.enabled = false;
			}

			// for some reason if the matte is fully black it won't be drawn at all
			if (matteColor == Color.black) matteColor.r += .001f;
			if (GraphicsSettings.renderPipelineAsset == null) {
				_panelEffect.matteColor = matteColor;
				_panelEffect.border.SetSize (borderSize);
				_panelEffect.borderColor = borderColor;
			}
	    	
	    	/* -- PASSIVE MOTION -- */
	    	
	    	CalculatePassiveMotion( PanelComponent.PanelPassiveMotionH );
	    	CalculatePassiveMotion( PanelComponent.PanelPassiveMotionV );
	    	
	    	/* -- CAMERA -- */
	    	
			if (!disableCameraControl) {
				if (cameraState != null) {
		    		
		    		float currentFOV = cameraState.fieldOfViewVal;
		    		float targetVerticalFOV = 0.0f;
		    		
		    		// default: crop horizontal, scale vertical (constant fov)
		    		targetVerticalFOV = currentFOV;
		    		
		    		// correct for vertical scale, now crop horizontal and vertical
					targetVerticalFOV *= ( Screen.height / PanoplyCore.panoplyRenderer.referenceScreenSize.y );
		    		
		    		// apply scale based on reference dimensions
		    		targetVerticalFOV /= ( PanoplyCore.resolutionScale * 2 );
					var referenceVerticalFOV = targetVerticalFOV;
		    		
		    		// make sure we crop vertically even in cases where the panel is not full height
		    		targetVerticalFOV *= ( camera.pixelRect.height / Screen.height );
		    		
		    		camera.fieldOfView = Mathf.Min( 180.0f, targetVerticalFOV );
		    		
		    		//GetComponent.<Camera>().fieldOfView = Mathf.Lerp( stateA.fieldOfViewVal, stateB.fieldOfViewVal, progress );
		    		lastCameraPosition = camera.transform.position = homePosition + cameraState.position;
		    	
		    		camera.transform.Translate( passiveMotionTranslationH );
		    		camera.transform.Translate( passiveMotionTranslationV );
		    	
		    		camera.transform.Rotate( passiveMotionRotationV );
		    		camera.transform.Rotate( passiveMotionRotationV );

					// move the camera to prevent cropping as panels are offscreen
					if (preserveFraming) {
						float heightDiff = Mathf.Abs (frameRect.height - camera.pixelRect.height);
						float vOffset = framingDistance * Mathf.Tan ((Mathf.Deg2Rad * referenceVerticalFOV) * .5f);
						if (frameRect.y > 0) {
							vOffset *= -1;
						}
						_centeringOffset.y = heightDiff / PanoplyCore.panoplyRenderer.screenRect.height * vOffset;
						float widthDiff = Mathf.Abs (frameRect.width - camera.pixelRect.width);
						float hOffset = framingDistance * Mathf.Tan ((Mathf.Deg2Rad * currentFOV) * .5f);
						if (frameRect.x > 0) {
							hOffset *= -1;
						}
						_centeringOffset.x = widthDiff / PanoplyCore.panoplyRenderer.screenRect.width * hOffset;
						camera.transform.Translate (_centeringOffset);
					}

					Vector3 cameraRotation = cameraState.rotation;
		    		switch ( cameraState.orientationType ) {
		    		
		    			case CameraOrientationType.Standard:
		    			var tmp_cs2 = camera.transform.rotation;
		                tmp_cs2.eulerAngles = cameraRotation;
		                camera.transform.rotation = tmp_cs2;
		    			break;
		    			
		    			case CameraOrientationType.LookAt:
						camera.transform.LookAt( cameraState.lookAt + homePosition);
		    			camera.transform.Rotate( cameraRotation );
		    			break;
		    			
		    		}
		    		
		    		lastCameraRotation = camera.transform.rotation;
		    
		    	} else {
		    		camera.transform.position = lastCameraPosition;
		    		camera.transform.rotation = lastCameraRotation;
		    	}
			}

			if (screenRect.Overlaps(frameRect)) {
				camera.enabled = ((frameRect.width > 1.5f) && (frameRect.height > 1.5f));
			}

	    }
	}
}
