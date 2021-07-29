using UnityEngine;
using System;
using System.Globalization;
using Opertoon.Panoply;

/**
 * The FrameState class defines the position and size of a frame at
 * a particular point in time.
 * Copyright © Erik Loyer
 * erik@opertoon.com
 * Part of the Panoply engine
 */

namespace Opertoon.Panoply {

	// Defines the way in which a frame's visibility is assessed.
	public enum VisibilityState {
		Start,		// visibility starts in the current state
		On,	 		// visibility continues throught the current state
		End,		// visibility ends in the current state
		Off
	}

	// Defines the way in which a frame's position can be specified.
	public enum PositionType {
		Edge,		// by the position of one of its edges
		Center		// by the position of its horizontal or vertical center
	}

	// Defines units in which a frame's position can be measured.
	public enum PositionUnits { 
		Pixels, 	// pixels
		Percent 	// percentage of screen width or height
	}

	// Defines the horizontal edges which can be used to specify a frame's position.
	public enum PositionEdgeHorz {
		Left,
		Right
	}

	// Defines the vertical edges which can be used to specify a frame's position.
	public enum PositionEdgeVert {
		Top,
		Bottom
	}

	// Defines the horizontal alignment options for a frame.
	public enum PositionAlignHorz {
		Left,
		Center,
		Right
	}

	// Defines the vertical alignment options for a frame.
	public enum PositionAlignVert {
		Top,
		Middle,
		Bottom
	}

	// Defines units in which a frame's width can be measured.
	public enum WidthUnits {
		Pixels,					// pixels
		PercentParentWidth,		// ratio of the parent's current width
		PercentPanelHeight		// ratio of the panel's current height
	}

	// Defines units in which a frame's height can be measured.
	public enum HeightUnits {
		Pixels,					// pixels
		PercentParentHeight,	// ratio of the parent's current height
		PercentPanelWidth		// ratio of the panel's current width
	}

	// Defines methods for calculating the width of the frame.
	public enum WidthCalcMethod {
		NoDimensions,
		Center,
		CenterAndWidth,
		Left,
		LeftAndWidth,
		Right,
		RightAndWidth,
		LeftAndRight
	}

	// Defines methods for calculating the height of the frame.
	public enum HeightCalcMethod {
		NoDimensions,
		Center,
		CenterAndHeight,
		Top,
		TopAndHeight,
		Bottom,
		BottomAndHeight,
		TopAndBottom
	}

	public enum PanelPosition {
		OffscreenLeft,
		Left,
		OffscreenRight,
		Right,
		OffscreenBottom,
		Bottom,
		OffscreenTop,
		Top,
		Center
	}

	[System.Serializable]
	public class FrameState {
		
		public string id = "New state";
		public static string HoldCommand = "Hold";
		public static string EndCommand = "End";

		[HideInInspector]
		public int index;

		[HideInInspector]
		public VisibilityState visibilityState = VisibilityState.On;
		
		public float leftVal = 0.0f;
		public PositionUnits leftUnits = PositionUnits.Pixels;
		public float rightVal = 0.0f;
		public PositionUnits rightUnits = PositionUnits.Pixels;
		public PositionType hPositionType = PositionType.Center;
		public PositionEdgeHorz hEdge = PositionEdgeHorz.Left;
		public float h = 0.0f;
		public PositionUnits hUnits = PositionUnits.Pixels;
		public float topVal = 0.0f;
		public PositionUnits topUnits = PositionUnits.Pixels;
		public float bottomVal = 0.0f;
		public PositionUnits bottomUnits = PositionUnits.Pixels;
		public PositionType vPositionType = PositionType.Center;
		public PositionEdgeVert vEdge = PositionEdgeVert.Top;
		public float v = 0.0f;
		public PositionUnits vUnits = PositionUnits.Pixels;
		public PositionAlignHorz hAlignType = PositionAlignHorz.Center;
		public PositionAlignVert vAlignType = PositionAlignVert.Middle;
		public float widthVal = 100.0f;
		public WidthUnits widthUnits = WidthUnits.PercentParentWidth;
		public float heightVal = 100.0f;
		public HeightUnits heightUnits = HeightUnits.PercentParentHeight;
		public float minAspectRatioVal = 0.0f;
		public float maxAspectRatioVal = 0.0f;
		public float marginTopVal = 0.0f;
		public float marginRightVal = 0.0f;
		public float marginBottomVal = 0.0f;
		public float marginLeftVal = 0.0f;
		public Color matteColor;
		public int borderSize;
		public Color borderColor = Color.black;
		
		[HideInInspector]
		public WidthCalcMethod widthCalcMethod;
		
		[HideInInspector]
		public HeightCalcMethod heightCalcMethod;
		
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
		
		[HideInInspector]
		public bool isEditing = false;
		
		public FrameState() {
		}
		
		public FrameState(int i) {
			index = i;
		}
		
		public FrameState(string name) {
			id = name;
		}
		
		public override string ToString() {
			return "Frame State \"" + id + "\"";
		}
		
		/**
		 * Sets the horizontal position of the frame.
		 *
		 * @param horz		String representing horizontal position of the frame ('50', '50%')
		 * @param type		The way in which position will be specified (edge or center)
		 * @param edge		If specified by edge, which edge (left or right)
		 * @param align		Alignment position of the frame
		 */
		public void SetHorizontalPosition(string horz,
	                                      PositionType type,
	                                      PositionEdgeHorz edge,
	                                      PositionAlignHorz align) {
		
			//Debug.Log('set horz pos: '+horz+' '+type+' '+edge+' '+align);
		
			h = 0.0f;
			leftVal = 0.0f;
			rightVal = 0.0f;

			float horzVal = 0.0f;
			PositionUnits units = 0;
			
			if (horz.EndsWith("%")) {
				units = PositionUnits.Percent;
				horzVal = float.Parse(horz.Substring(0, horz.Length - 1));
			} else {
				units = PositionUnits.Pixels;
				horzVal = float.Parse(horz);
			}
			
			hPositionType = type;
			hEdge = edge;
			hAlignType = align;
		
			switch (type) {
			
				case PositionType.Edge:
				switch (edge) {
				
					case PositionEdgeHorz.Left:
					leftVal = horzVal;
					leftUnits = units;
					break;
					
					case PositionEdgeHorz.Right:
					rightVal = horzVal;
					rightUnits = units;
					break;
					
				}
				break;
				
				case PositionType.Center:
				h = horzVal;
				hUnits = units;
				break;
				
			}
			
		}
		
		/**
		 * Sets the vertical position of the frame.
		 *
		 * @param vert		String representing vertical position of the frame ('50', '50%')
		 * @param type		The way in which position will be specified (edge or center)
		 * @param edge		If specified by edge, which edge (top or bottom)
		 * @param align		Alignment position of the frame
		 */
		public void SetVerticalPosition(string vert,
	                                    PositionType type,
	                                    PositionEdgeVert edge,
	                                    PositionAlignVert align) {
		
			//Debug.Log('set vert pos: '+vert+' '+type+' '+edge+' '+align);
		
			v = 0.0f;
			topVal = 0.0f;
			bottomVal = 0.0f;

			float vertVal = 0.0f;
			PositionUnits units = 0;
			
			if (vert.EndsWith("%")) {
				units = PositionUnits.Percent;
				vertVal = float.Parse(vert.Substring(0, vert.Length - 1));
			} else {
				units = PositionUnits.Pixels;
				vertVal = float.Parse(vert);
			}
			
			vPositionType = type;
			vEdge = edge;
			vAlignType = align;
		
			// TODO: figure out why phrasing this as a switch statement (as in the horz version above) caused errors
			
			if (type == PositionType.Edge) {
			
				if (edge == PositionEdgeVert.Top) {
					topVal = vertVal;
					topUnits = units;
					
				} else {
					bottomVal = vertVal;
					bottomUnits = units;
				}
				
			} else {
				v = vertVal;
				vUnits = units;
			}
			
		}
		
		public void MoveTo(string destStr) {
			PanelPosition dest = (PanelPosition)System.Enum.Parse( typeof( PanelPosition ), destStr );
			MoveTo( dest ); 
		}

		public void MoveTo(PanelPosition dest) {
			MoveTo(dest, "0%");
		}
		
		public void MoveTo(PanelPosition dest, string offsetStr) {

			string offsetStringH, offsetStringV;
			PositionUnits positionUnitsH = PositionUnits.Percent;
			PositionUnits positionUnitsV = PositionUnits.Percent;
			float offsetValH = 0.0f;
			float offsetValV = 0.0f;

			string[] temp;
			temp = offsetStr.Split( ","[ 0 ] );

			if (temp[0].EndsWith("%")) {
				offsetStringH = temp[0].Substring(0, temp[0].Length - 1);
				positionUnitsH = PositionUnits.Percent;
			} else {
				offsetStringH = temp[0];
				positionUnitsH = PositionUnits.Pixels;
			}
			offsetValH = float.Parse(offsetStringH);

			if (temp.Length > 1) {
				if (temp[1].EndsWith("%")) {
					offsetStringV = temp[1].Substring(0, temp[1].Length - 1);
					positionUnitsV = PositionUnits.Percent;
				} else {
					offsetStringV = temp[1];
					positionUnitsV = PositionUnits.Pixels;
				}
				offsetValV = float.Parse(offsetStringV);
			}

			switch ( dest ) {
			
				case PanelPosition.OffscreenLeft:
				hPositionType = PositionType.Edge;
				hEdge = PositionEdgeHorz.Right;
				rightVal = 100.0f + offsetValH;
				rightUnits = positionUnitsH;
				break;
			
				case PanelPosition.Left:
				hPositionType = PositionType.Edge;
				hEdge = PositionEdgeHorz.Left;
				leftVal = 0.0f + offsetValH;
				leftUnits = positionUnitsH;
				break;
			
				case PanelPosition.OffscreenRight:
				hPositionType = PositionType.Edge;
				hEdge = PositionEdgeHorz.Left;
				leftVal = 100.0f + offsetValH;
				leftUnits = positionUnitsH;
				break;
			
				case PanelPosition.Right:
				hPositionType = PositionType.Edge;
				hEdge = PositionEdgeHorz.Right;
				rightVal = 0.0f + offsetValH;
				rightUnits = positionUnitsH;
				break;
			
				case PanelPosition.OffscreenBottom:
				vPositionType = PositionType.Edge;
				vEdge = PositionEdgeVert.Top;
				topVal = 100.0f + offsetValH;
				topUnits = positionUnitsH;
				break;
			
				case PanelPosition.Bottom:
				vPositionType = PositionType.Edge;
				vEdge = PositionEdgeVert.Bottom;
				bottomVal = 0.0f + offsetValH;
				bottomUnits = positionUnitsH;
				break;
			
				case PanelPosition.OffscreenTop:
				vPositionType = PositionType.Edge;
				vEdge = PositionEdgeVert.Bottom;
				bottomVal = 100.0f + offsetValH;
				bottomUnits = positionUnitsH;
				break;
			
				case PanelPosition.Top:
				vPositionType = PositionType.Edge;
				vEdge = PositionEdgeVert.Top;
				topVal = 0.0f + offsetValH;
				topUnits = positionUnitsH;
				break;
			
				case PanelPosition.Center:
				hPositionType = PositionType.Center;
				h = 0.0f + offsetValH;
				hUnits = positionUnitsH;
				vPositionType = PositionType.Center;
				v = 0.0f + offsetValV;
				vUnits = positionUnitsV;
				break;
		
			}
		}

		public void SetSize( string wStr, string hStr ) {
			SetSize ( wStr, hStr, "", "" );
		}

		/**
		 * Sets the dimensions of the frame.
		 *
		 * @param wStr		String representing width of the frame (50, 50%, 50AR)		
		 * @param hStr		String representing height of the frame (50, 50%, 50AR)
		 * @param minAR		Minimum aspect ratio of the frame
		 * @param maxAR		Maximum aspect ratio of the frame
		 */
		public void SetSize(string wStr,string hStr,string minAR,string maxAR) {
		
			widthVal = 0.0f;
			if (wStr.EndsWith("AR")) {
				wStr = wStr.Substring(0, wStr.Length - 2);
				widthUnits = WidthUnits.PercentPanelHeight;

			} else if (wStr.EndsWith("%")) {
				wStr = wStr.Substring(0, wStr.Length - 1);
				widthUnits = WidthUnits.PercentParentWidth;
								
			} else {
				widthUnits = WidthUnits.Pixels;
			}
			widthVal = float.Parse(wStr);
		
			heightVal = 0.0f;
			if (hStr != null) {
				if (hStr.EndsWith("AR")) {
					hStr = hStr.Substring(0, hStr.Length - 2);
					heightUnits = HeightUnits.PercentPanelWidth;
		
				} else if (hStr.EndsWith("%")) {
					hStr = hStr.Substring(0, hStr.Length - 1);
					heightUnits = HeightUnits.PercentParentHeight;
									
				} else {
					heightUnits = HeightUnits.Pixels;
				}
				heightVal = float.Parse(hStr);
			}
			
			minAspectRatioVal = 0.0f;
			maxAspectRatioVal = 0.0f;
			if (minAR != "") {
				minAspectRatioVal = float.Parse(minAR);
			}
			if (maxAR != "") {
				maxAspectRatioVal = float.Parse(maxAR);
			}
			
		}
		
		public void SetWidth(string wStr) {
		
			widthVal = 0.0f;
			if (wStr.EndsWith("AR")) {
				wStr = wStr.Substring(0, wStr.Length - 2);
				widthUnits = WidthUnits.PercentPanelHeight;

			} else if (wStr.EndsWith("%")) {
				wStr = wStr.Substring(0, wStr.Length - 1);
				widthUnits = WidthUnits.PercentParentWidth;
								
			} else {
				widthUnits = WidthUnits.Pixels;
			}
			widthVal = float.Parse(wStr);
		
		}
		
		public void SetHeight(string hStr) {
		
			heightVal = 0.0f;
			if (hStr != null) {
				if (hStr.EndsWith("AR")) {
					hStr = hStr.Substring(0, hStr.Length - 2);
					heightUnits = HeightUnits.PercentPanelWidth;
		
				} else if (hStr.EndsWith("%")) {
					hStr = hStr.Substring(0, hStr.Length - 1);
					heightUnits = HeightUnits.PercentParentHeight;
									
				} else {
					heightUnits = HeightUnits.Pixels;
				}
				heightVal = float.Parse(hStr);
			}
		
		}
		
		public void SetAllMargins(float m) {
			string mStr = m.ToString();
			SetMargins( mStr, mStr, mStr, mStr );
		}
		
		public void SetMargins(string m) {
			SetMargins( m, m, m, m );
		}
		
		/**
		 * Set the margins of the frame.
		 *
		 * @param mTop			Top margin
		 * @param mRight		Right margin
		 * @param mBottom		Bottom margin
		 * @param mLeft			Left margin
		 */
		public void SetMargins(string mTop,string mRight,string mBottom,string mLeft) {
		
			marginTopVal = 0.0f;
			marginRightVal = 0.0f;
			marginBottomVal = 0.0f;
			marginLeftVal = 0.0f;
			if (mTop != "") {
				marginTopVal = float.Parse(mTop);
			}
			if (mRight != "") {
				marginRightVal = float.Parse(mRight);
			}
			if (mBottom != "") {
				marginBottomVal = float.Parse(mBottom);
			}
			if (mLeft != "") {
				marginLeftVal = float.Parse(mLeft);
			}
			
		}
		
		/**
		 * Set the matte color of the frame.
		 *
		 * @param color		The matte color
		 */
		public void SetMatteColor(Color color) {
			matteColor = color;
		}
		
		public Rect GetRect() {
			return GetRect( true );
		}
		
		/**
		 * Returns a Rect calculated based on the properties of the frame
		 * and the current screen dimensions.
		 *
		 * @return	The calculated rectangle.
		 */
		public Rect GetRect(bool includeMargins) {
		
			pxTop = -1.0f;
			pxRight = -1.0f;
			pxBottom = -1.0f;
			pxLeft = -1.0f;
			pxWidth = 0.0f;
			pxHeight = 0.0f;
			pxCenterH = -1.0f;
			pxCenterV = -1.0f;
			widthCalcMethod = WidthCalcMethod.NoDimensions;
			heightCalcMethod = HeightCalcMethod.NoDimensions;

			if ( PanoplyCore.panoplyRenderer != null ) {

				Rect screenRect = PanoplyCore.panoplyRenderer.screenRect;
			
				// -- WIDTH AND HEIGHT -- //
				
				//if (widthVal > 0) {
					switch (widthUnits) {
					
						// width in pixels
						case WidthUnits.Pixels:
							pxWidth = widthVal;
							break;
							
						// width as percent of screen width
						case WidthUnits.PercentParentWidth:
							pxWidth = screenRect.width * (widthVal * .01f);
							break;
												
					}
				//}
				
				//if (heightVal > 0) {
					switch (heightUnits) {
					
						// height in pixels
						case HeightUnits.Pixels:
							pxHeight = heightVal;
							break;
							
						// height as percent of screen height
						case HeightUnits.PercentParentHeight:
							pxHeight = screenRect.height * (heightVal * .01f);
							break;
							
						// height as percent of width (aspect ratio)
						case HeightUnits.PercentPanelWidth:
							pxHeight = pxWidth * heightVal;
							break;
							
					}
				//}
				
				// width as percent of height (aspect ratio)
				// need to do this here instead of with the other width calculations above, otherwise
				// we don't know what the height is yet
				if (/*(widthVal > 0) &&*/ (widthUnits == WidthUnits.PercentPanelHeight)) {
					pxWidth = pxHeight * widthVal;
				}
						
				// -- LEFT AND RIGHT -- //
				
				// if a horizontal center offset has been specified, then
				if (hPositionType == PositionType.Center) {
					
					switch (hUnits) {
					
						case PositionUnits.Pixels:
							pxCenterH = ((screenRect.width * .5f) + h);
							break;
							
						case PositionUnits.Percent:
							pxCenterH = ((screenRect.width * .5f) + (screenRect.width * (h * .01f)));
							break;
							
					}
					
					// if width has been set, then calculate left and right edges based on it
					//if (pxWidth > 0) {
						pxLeft = pxCenterH - (pxWidth * .5f);
						pxRight = pxCenterH + (pxWidth * .5f);
						widthCalcMethod = WidthCalcMethod.CenterAndWidth;
						
					// otherwise, set left and right edges to the edges of the screen
					/*} else {
						pxLeft = 0;
						pxRight = screenRect.width;
						widthCalcMethod = WidthCalcMethod.Center;
					}*/
				
				// otherwise, if no horizontal center offset has been specified, then
				} else if (hPositionType == PositionType.Edge) {
				
					// if the left edge has been specified, then calculate it
					if (hEdge == PositionEdgeHorz.Left) {
						switch (leftUnits) {
						
							case PositionUnits.Pixels:
								pxLeft = leftVal;
								break;
								
							case PositionUnits.Percent:
								pxLeft = screenRect.width * (leftVal * .01f);
								break;
								
						}
						widthCalcMethod = WidthCalcMethod.Left;
					
					// if the right edge has been specified, then calculate it
					} else if (hEdge == PositionEdgeHorz.Right) {
						switch (rightUnits) {
						
							case PositionUnits.Pixels:
								pxRight = screenRect.width - rightVal;
								break;
								
							case PositionUnits.Percent:
							pxRight = screenRect.width - (screenRect.width * (rightVal * .01f));
								break;
								
						}
						widthCalcMethod = WidthCalcMethod.Right;
					}
				}
				
				// -- TOP AND BOTTOM -- //
				
				// if a vertical center offset has been specified, then
				if (vPositionType == PositionType.Center) {
				
					switch (vUnits) {
					
						case PositionUnits.Pixels:
							pxCenterV = ((screenRect.height * .5f) + v);
							break;
							
						case PositionUnits.Percent:
							pxCenterV = ((screenRect.height * .5f) + (screenRect.height * (v * .01f)));
							break;
							
					}
					
					// if a height has been set, then calculate top and bottom edges based on it
					//if (pxHeight > 0) {
						pxBottom = pxCenterV - (pxHeight * .5f);
						pxTop = pxCenterV + (pxHeight * .5f);
						heightCalcMethod = HeightCalcMethod.CenterAndHeight;
						
					// otherwise, set top and bottom edges to the edges of the screen
					/*} else {
						pxBottom = 0;
						pxTop = screenRect.height;
						heightCalcMethod = HeightCalcMethod.Center;
					}*/
				
				// otherwise, if no vertical center offset has been specified, then
				} else if (vPositionType == PositionType.Edge) {
				
					// if the top edge has been specified, then calculate it
					if (vEdge == PositionEdgeVert.Top) {
						switch (topUnits) {
						
							case PositionUnits.Pixels:
								pxTop = screenRect.height - topVal;
								break;
								
							case PositionUnits.Percent:
								pxTop = screenRect.height - (screenRect.height * (topVal * .01f));
								break;
								
						}
						heightCalcMethod = HeightCalcMethod.Top;

					// if the botttom edge has been specified, then calculate it
					} else if (vEdge == PositionEdgeVert.Bottom) {
						switch (bottomUnits) {
						
							case PositionUnits.Pixels:
								pxBottom = bottomVal;
								break;
								
							case PositionUnits.Percent:
								pxBottom = screenRect.height * (bottomVal * .01f);
								break;
								
						}
						heightCalcMethod = HeightCalcMethod.Bottom;
					}
					
				}
				
				// if the top edge has not been set, then
				if (pxTop == -1) {
				
					// if a height has been set, then calculate the top edge from the bottom edge and the height
					//if (pxHeight > 0) {
						pxTop = pxBottom + pxHeight;
						heightCalcMethod = HeightCalcMethod.BottomAndHeight;
						
					// otherwise, set the top to the top of the screen
					/*} else {
						pxTop = screenRect.height;
					}*/
				}
				
				// if no bottom edge has been specified and the height has been set, then
				// calculate the bottom edge based on the top edge and the height
				if ((pxBottom == -1) /*&& (pxHeight > 0)*/) {
					pxBottom = pxTop - pxHeight;
					heightCalcMethod = HeightCalcMethod.TopAndHeight;
				}
				
				// if the right edge has not been set, then
				if (pxRight == -1) {
				
					// if a width has been set, then calculate the right edge from the left edge and the width
					//if (pxWidth > 0) {
						pxRight = pxLeft + pxWidth;
						widthCalcMethod = WidthCalcMethod.LeftAndWidth;
						
					// otherwise, set the right edge to the right edge of the screen
					/*} else {
						pxRight = screenRect.width;
					}*/
				}
				
				// if no left edge has been specified and the width has been set, then
				// calculate the left edge based on the right edge and the width
				if ((pxLeft == -1)/* && (pxWidth > 0)*/) {
					pxLeft = pxRight - pxWidth;
					widthCalcMethod = WidthCalcMethod.RightAndWidth;
				}
				
				// correct for aspect ratio limits
				float ar = 0.0f;
				float diff = 0.0f;
				ar = (pxRight - pxLeft) / (pxTop - pxBottom);
				if ((maxAspectRatioVal > 0) && (ar > maxAspectRatioVal)) {
					pxHeight = pxTop - pxBottom;
					pxWidth = pxHeight * maxAspectRatioVal;
					diff = (pxRight - pxLeft) - pxWidth;
					switch (hAlignType) {
					
						case PositionAlignHorz.Left:
							pxRight -= diff;
							break;
							
						case PositionAlignHorz.Center:
							pxRight -= (diff * .5f);
							pxLeft += (diff * .5f);
							break;
						
						case PositionAlignHorz.Right:
							pxLeft += diff;
							break;
							
					}
				}
				if ((minAspectRatioVal > 0) && (ar < minAspectRatioVal)) {
					pxWidth = pxRight - pxLeft;
					pxHeight = pxWidth / minAspectRatioVal;
					diff = (pxTop - pxBottom) - pxHeight;
					switch (vAlignType) {
					
						case PositionAlignVert.Top:
							pxBottom += diff;
							break;
							
						case PositionAlignVert.Middle:
							pxTop -= (diff * .5f);
							pxBottom += (diff * .5f);
							break;
						
						case PositionAlignVert.Bottom:
							pxTop -= diff;
							break;
							
					}
				}
				
				float marginLeftProxy = 0.0f;
				float marginRightProxy = 0.0f;
				float marginTopProxy = 0.0f;
				float marginBottomProxy = 0.0f;
				
				if ( includeMargins ) {
					marginLeftProxy = ( marginLeftVal * 2 ) * PanoplyCore.resolutionScale;
					marginRightProxy = ( marginRightVal * 2 ) * PanoplyCore.resolutionScale;
					marginTopProxy = ( marginTopVal * 2 ) * PanoplyCore.resolutionScale;
					marginBottomProxy = ( marginBottomVal * 2 ) * PanoplyCore.resolutionScale;
				}
				
				// make sure width and height aren't less than 1 and that margins aren't larger than width or height
				pxWidth = Mathf.Max(1.0f, pxRight - marginRightProxy - (pxLeft + marginLeftProxy));
				pxHeight = Mathf.Max(1.0f, pxTop - marginTopProxy - (pxBottom + marginBottomProxy));
				
				return new Rect(pxLeft + Mathf.Min(marginLeftProxy, pxWidth) + screenRect.x, pxBottom + Mathf.Min(marginBottomProxy, pxHeight) + screenRect.y, pxWidth, pxHeight);
			} else {
				return new Rect();
			}
		
		}
				
		/**
		 * Clones the panel layer and returns the clone.
		 *
		 * @return	A clone of the panel layer.
		 */
		public FrameState Clone() {
		
			FrameState frame = new FrameState();
			frame.visibilityState = visibilityState;
			frame.leftVal = leftVal;
			frame.leftUnits = leftUnits;
			frame.rightVal = rightVal;
			frame.rightUnits = rightUnits;
			frame.hPositionType = hPositionType;
			frame.hEdge = hEdge;
			frame.h = h;
			frame.hUnits = hUnits;
			frame.topVal = topVal;
			frame.topUnits = topUnits;
			frame.bottomVal = bottomVal;
			frame.bottomUnits = bottomUnits;
			frame.vPositionType = vPositionType;
			frame.vEdge = vEdge;
			frame.v = v;
			frame.vUnits = vUnits;
			frame.hAlignType = hAlignType;
			frame.vAlignType = vAlignType;
			frame.widthVal = widthVal;
			frame.widthUnits = widthUnits;
			frame.heightVal = heightVal;
			frame.heightUnits = heightUnits;
			frame.minAspectRatioVal = minAspectRatioVal;
			frame.maxAspectRatioVal = maxAspectRatioVal;
			frame.marginTopVal = marginTopVal;
			frame.marginRightVal = marginRightVal;
			frame.marginBottomVal = marginBottomVal;
			frame.marginLeftVal = marginLeftVal;
			frame.matteColor = matteColor;
		
			return frame;
		}
		
	} 
}

