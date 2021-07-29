using UnityEngine;
using System;
using Opertoon.Panoply;

/**
 * The ArtworkState class defines the position, orientation, and/or size of
 * a single layer of content in a panel at a particular point in time.
 * Copyright © Erik Loyer
 * erik@opertoon.com
 * Part of the Panoply engine
 */

namespace Opertoon.Panoply {

	public enum ArtworkPositionType {
		Panel,
		Local,
		Global
	}

	[System.Serializable]
	public class ArtworkState {

		public string id;

		public static string HoldCommand = "Hold";

		public string name;

		public Vector3 position;
		public bool positionSpecified;

		public Vector3 rotation;
		public bool rotationSpecified;

		public Vector3 scale;
		public bool scaleSpecified;

		public string overlayTarget;
		public int overlayTargetVal = 0;
		
		public float xVal = 0.0f;
		public PositionAlignHorz hPositionType = PositionAlignHorz.Left;
		public PositionUnits hUnits = PositionUnits.Pixels;

		public float yVal = 0.0f;
		public PositionAlignVert vPositionType = PositionAlignVert.Top;
		public PositionUnits vUnits = PositionUnits.Pixels;

		public float xInsetVal = -1.0f;
		public float xInsetCalc;

		public float yInsetVal = -1.0f;
		public float yInsetCalc;
		
		public Color color = Color.white;
		public bool colorSpecified;
		
		public Rect dimensions;
		
		public override string ToString() {
			return "ArtworkState \"" + id + "\"";
		}
		
		
		/**
		 * Calculates the position of the overlay within the given rectangle.
		 *
		 * @param	rect		Rectangle defining the panel's frame.
		 * @param	scale		Scale value to apply to the coordinates.
		 * @return				A vector defining the position of the overlay.
		 */
		public Vector2 getPositionForRect(Rect rect,float scale) {
		
			Vector2 position = new Vector2(0.0f, 0.0f);
			
			switch (hPositionType) {
			
				case PositionAlignHorz.Left:
				switch (hUnits) {
				
					case PositionUnits.Pixels:
					position.x = xVal * scale;
					break;
					
					case PositionUnits.Percent:
					position.x = (xVal * .01f) * rect.width;
					break;
				
				}
				break;
			
				case PositionAlignHorz.Center:
				switch (hUnits) {
				
					case PositionUnits.Pixels:
					position.x = (rect.width * .5f) + (xVal * scale);
					break;
					
					case PositionUnits.Percent:
					position.x = (rect.width * .5f) + ((xVal * .01f) * rect.width);
					break;
				
				}
				break;
			
				case PositionAlignHorz.Right:
				switch (hUnits) {
				
					case PositionUnits.Pixels:
					position.x = rect.width - (xVal * scale);
					break;
					
					case PositionUnits.Percent:
					position.x = rect.width - ((xVal * .01f) * rect.width);
					break;
				
				}
				break;

			}
			
			switch (vPositionType) {
			
				case PositionAlignVert.Bottom:
				switch (vUnits) {
				
					case PositionUnits.Pixels:
					position.y = yVal * scale;
					break;
					
					case PositionUnits.Percent:
					position.y = (yVal * .01f) * rect.height;
					break;
				
				}
				break;
			
				case PositionAlignVert.Middle:
				switch (vUnits) {
				
					case PositionUnits.Pixels:
					position.y = (rect.height * .5f) + (yVal * scale);
					break;
					
					case PositionUnits.Percent:
					position.y = (rect.height * .5f) + ((yVal * .01f) * rect.height);
					break;
				
				}
				break;
			
				case PositionAlignVert.Top:
				switch (vUnits) {
				
					case PositionUnits.Pixels:
					position.y = rect.height - (yVal * scale);
					break;
					
					case PositionUnits.Percent:
					position.y = rect.height - ((yVal * .01f) * rect.height);
					break;
				
				}
				break;

			}
			
			return position;	
		}

	} 
}