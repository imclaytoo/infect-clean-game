using UnityEngine;
using System;

/**
 * The PassiveMotionState class defines the way in which a property can vary
 * in response to user input.
 * Copyright © Erik Loyer
 * erik@opertoon.com
 * Part of the Panoply engine
 */

namespace Opertoon.Panoply {
	/**
	 * Identifies a controller input.
	 */
	public enum PassiveMotionControllerInput {
		HorizontalTilt,
		VerticalTilt
	}

	/**
	 * Identifies a kind of passive motion output.
	 */
	public enum PassiveMotionOutputType {
		Position,
		Rotation
	}

	/**
	 * Identifies the passive motion output property to affect.
	 */
	public enum PassiveMotionOutputProperty {
		X,
		Y,
		Z	
	}

	[System.Serializable]
	public class PassiveMotionState {
		
		public string id = "New state";
		public static string HoldCommand = "Hold";
		public int index;
		public PassiveMotionControllerInput input;
		public float inputMinimumVal = -1.0f;
		public float inputMaximumVal = 1.0f;
		public PassiveMotionOutputType output;
		public PassiveMotionOutputProperty outputProperty;
		public float outputMinimumVal = -1.0f;
		public float outputMaximumVal = 1.0f;
		public float multiplierVal = 10.0f;
		
		
		[HideInInspector]
		public bool isEditing = false;
		
		public override string ToString() {
			return "PassiveMotionState \"" + id + "\"";
		}
		
		/**
		 * Given an input value, returns an output value according to the settings
		 * of the passiveMotionState.
		 *
		 * @param	i			The input value.
		 * @return				An output value.
		 */
		public float GetOutput(float i) {
		
			float o = Mathf.Lerp(outputMinimumVal, outputMaximumVal, Mathf.InverseLerp(inputMinimumVal, inputMaximumVal, i));
			//Debug.Log( input + ' ' + inputMinimumVal + '-' + inputMaximumVal + ' input: ' + i + ' ' + output + ' ' + outputMinimumVal + '-' + outputMaximumVal + ' output: ' + o );
			return o;
		
		}
		
	}
}