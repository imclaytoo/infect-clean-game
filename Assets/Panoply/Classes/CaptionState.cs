using UnityEngine;
using System;
using Opertoon.Panoply;

/**
 * The CaptionState class defines the position, dimensions,
 * and content of a caption at a particular moment in time.
 * Copyright © Erik Loyer
 * erik@opertoon.com
 * Part of the Panoply engine
 */

namespace Opertoon.Panoply {
	public enum CaptionHorzAlign {
		Left,
		Center,
		Right
	}

	public enum CaptionVertAlign {
		Top,
		Center,
		Bottom
	}

	[System.Serializable]
	public class CaptionState {
		
		public string id = "New state";
		public static string HoldCommand = "Hold";
		public int index;
		public GUISkin skin;
		public Rect layout = new Rect( 10.0f, 10.0f, 200.0f, 70.0f );
		public PositionUnits offsetUnitsX = PositionUnits.Pixels;
		public PositionUnits offsetUnitsY = PositionUnits.Pixels;
		public string text = "Hello world.";
		public Vector2 dropShadow = new Vector2( 0.0f, 0.0f );
		public Color dropShadowColor = Color.black;
		public Texture2D tail;
		public float tailWidth = 40.0f;
		public float tailHeight = 200.0f;
		public float tailRotation;
		public Color color = Color.white;
		public bool drawTail = false;
		public CaptionVertAlign vertAlign = CaptionVertAlign.Top;
		public CaptionHorzAlign horzAlign = CaptionHorzAlign.Left;
        public float cornerRounding = .11f;

        public CaptionState() {
		
		}
		
		public CaptionState( string name ) {
		
			id = name;
			
		}
		
	}
}