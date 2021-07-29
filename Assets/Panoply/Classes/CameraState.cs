using UnityEngine;
using System;

/**
 * The CameraState class defines the position and orientation of a panel
 * camera at a particular point in time.
 * Copyright © Erik Loyer
 * erik@opertoon.com
 * Part of the Panoply engine
 */

namespace Opertoon.Panoply {
	/**
	 * Defines how the camera's orientation is calculated.
	 */
	public enum CameraOrientationType {
		Standard,
		LookAt,
		Inherited
	}                       

	/**
	 * Coarse animation curves the define when a transition should occur
	 * relative to its default position in the state sequence.
	 */
	public enum AnimationCurveType {
		Standard,
		Early,
		Late
	}  
	[System.Serializable]
	public class CameraState {
		
		public string id = "New state";
		public static string HoldCommand = "Hold";
		public int index;
		public float fieldOfViewVal = 60.0f;
		public Vector3 position;
		public Vector3 positionOffset;
		public Vector3 rotationOffset;
		public CameraOrientationType orientationType;
		public bool orientationTypeSpecified;
		public Vector3 rotation = Vector3.zero;
		public bool rotationSpecified;
		public Vector3 lookAt = Vector3.forward;
		public bool lookAtSpecified;
		public AnimationCurveType outCurveVal = AnimationCurveType.Standard;
		
		[HideInInspector]
		public bool isEditing = false;
		
		public CameraState() {
		}
		
		public CameraState( string name ) {
			id = name;
		}
		
		public override string ToString() {
			return "Camera State \"" + id + "\"";
		}
			
		/**
		 * Performs initialization actions.
		 */
		public void setup() {
		
		}
		
	}
}