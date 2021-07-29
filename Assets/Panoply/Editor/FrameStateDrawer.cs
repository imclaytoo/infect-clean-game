using UnityEngine;
using System;
using UnityEditor;
using Opertoon.Panoply;

/**
 * FrameStateDrawer
 * Copyright © Erik Loyer
 * erik@opertoon.com
 * Part of the Panoply engine
 */

namespace Opertoon.Panoply {
	[CustomPropertyDrawer (typeof (FrameState))]
	[System.Serializable]
	public class FrameStateDrawer: PropertyDrawer {

		int rowCount = 5;
		int rowHeight = 16;
		int rowSpacing = 2;

		public override float GetPropertyHeight( SerializedProperty property,
	                                                      GUIContent label ) {
		
			rowCount = 1;
			
			if ( !property.FindPropertyRelative( "isEditing" ).boolValue ) {
			
				return ( float )( ( rowCount * rowHeight ) + (( rowCount - 1 ) * rowSpacing ) );
				
			} else {
			
				// name
				rowCount++;
			
				// h position
				rowCount++;
				switch ( property.FindPropertyRelative( "hPositionType" ).enumValueIndex ) {
				
					case ( int )PositionType.Edge:
					rowCount += 2;
					break;
				
					case ( int )PositionType.Center:
					rowCount++;
					break;

				}
				
				// v position
				rowCount++;
				switch ( property.FindPropertyRelative( "vPositionType" ).enumValueIndex ) {
				
					case ( int )PositionType.Edge:
					rowCount += 2;
					break;
				
					case ( int )PositionType.Center:
					rowCount++;
					break;

				}
				
				// size
				rowCount += 2;
				
				// aspect ratio
				rowCount += 4;
				
				// margins
				rowCount += 4;
				
				// matte
				rowCount++;
				
				return ( float )( ( rowCount * rowHeight ) + (( rowCount - 1 ) * rowSpacing + 10 ) );
			
			}
			
		}
		
		/*private function getYForRowNum( row : int ) {
			return ( row * rowHeight ) + (( row + 1 ) * rowSpacing );
		}*/
		
		public override void OnGUI( Rect position, SerializedProperty property, GUIContent label ) {
		 
		 	Rect itemRect = new Rect();
		 	
			EditorGUI.BeginProperty(position, label, property);
			
			itemRect = new Rect( position.x, position.y, position.width, ( float )rowHeight );		
			property.FindPropertyRelative( "isEditing" ).boolValue = EditorGUI.Foldout( itemRect, property.FindPropertyRelative( "isEditing" ).boolValue, property.FindPropertyRelative( "id" ).stringValue, true );
			
			if ( property.FindPropertyRelative( "isEditing" ).boolValue ) {
				
				/* -- HORIZONTAL POSITION -- */
				
				itemRect.y += ( float )( rowSpacing + rowHeight ); 
				property.FindPropertyRelative( "id" ).stringValue = EditorGUI.TextField( itemRect, "Name", property.FindPropertyRelative( "id" ).stringValue );
				
				itemRect.y += ( float )( rowSpacing + rowHeight ); 
				property.FindPropertyRelative( "hPositionType" ).enumValueIndex = EditorGUI.Popup( itemRect, "H Position", property.FindPropertyRelative( "hPositionType" ).enumValueIndex, new string[] {"Edge", "Center"} );
				
				itemRect.y += ( float )( rowSpacing + rowHeight ); 
				switch ( property.FindPropertyRelative( "hPositionType" ).enumValueIndex ) {
				
					case ( int )PositionType.Edge:		
					property.FindPropertyRelative( "hEdge" ).enumValueIndex = EditorGUI.Popup( itemRect, "H Edge", property.FindPropertyRelative( "hEdge" ).enumValueIndex, new string[] {"Left", "Right"} );
		
					if ( property.FindPropertyRelative( "hEdge" ).enumValueIndex == ( int )PositionEdgeHorz.Left ) {
						itemRect.y += ( float )( rowSpacing + rowHeight ); 
						itemRect.width = position.width - 80;
						property.FindPropertyRelative( "leftVal" ).floatValue = EditorGUI.FloatField( itemRect, "Left", property.FindPropertyRelative( "leftVal" ).floatValue );
						
						itemRect.x = position.x + position.width - 80;
						itemRect.width = 80.0f;
						property.FindPropertyRelative( "leftUnits" ).enumValueIndex = EditorGUI.Popup( itemRect, property.FindPropertyRelative( "leftUnits" ).enumValueIndex, new string[] {"px", "%"} );

					} else {	
						itemRect.x = position.x;
						itemRect.y += ( float )( rowSpacing + rowHeight ); 
						itemRect.width = position.width - 80;
						property.FindPropertyRelative( "rightVal" ).floatValue = EditorGUI.FloatField( itemRect, "Right", property.FindPropertyRelative( "rightVal" ).floatValue );
						
						itemRect.x = position.x + position.width - 80;
						itemRect.width = 80.0f;
						property.FindPropertyRelative( "rightUnits" ).enumValueIndex = EditorGUI.Popup( itemRect, property.FindPropertyRelative( "rightUnits" ).enumValueIndex, new string[] {"px", "%"} );
					}
					break;
				
					case ( int )PositionType.Center:
					itemRect.width = position.width - 80;
					property.FindPropertyRelative( "h" ).floatValue = EditorGUI.FloatField( itemRect, "Offset", property.FindPropertyRelative( "h" ).floatValue );

					itemRect.x = position.x + position.width - 80;
					itemRect.width = 80.0f;
					property.FindPropertyRelative( "hUnits" ).enumValueIndex = EditorGUI.Popup( itemRect, property.FindPropertyRelative( "hUnits" ).enumValueIndex, new string[] {"px", "%"} );
					break;

				}
				
				/* -- VERTICAL POSITION -- */
				
				itemRect.x = position.x;
				itemRect.width = position.width;
				itemRect.y += ( float )( rowSpacing + rowHeight ); 
				property.FindPropertyRelative( "vPositionType" ).enumValueIndex = EditorGUI.Popup( itemRect, "V Position", property.FindPropertyRelative( "vPositionType" ).enumValueIndex, new string[] {"Edge", "Center"} );
				
				itemRect.y += ( float )( rowSpacing + rowHeight ); 
				switch ( property.FindPropertyRelative( "vPositionType" ).enumValueIndex ) {
				
					case ( int )PositionType.Edge:
					property.FindPropertyRelative( "vEdge" ).enumValueIndex = EditorGUI.Popup( itemRect, "V Edge", property.FindPropertyRelative( "vEdge" ).enumValueIndex, new string[] {"Top", "Bottom"} );

					if ( property.FindPropertyRelative( "vEdge" ).enumValueIndex == ( int )PositionEdgeVert.Top ) {
						itemRect.y += ( float )( rowSpacing + rowHeight ); 
						itemRect.width = position.width - 80;
						property.FindPropertyRelative( "topVal" ).floatValue = EditorGUI.FloatField( itemRect, "Top", property.FindPropertyRelative( "topVal" ).floatValue );
						
						itemRect.x = position.x + position.width - 80;
						itemRect.width = 80.0f;
						property.FindPropertyRelative( "topUnits" ).enumValueIndex = EditorGUI.Popup( itemRect, property.FindPropertyRelative( "topUnits" ).enumValueIndex, new string[] {"px", "%"} );
					
					} else {
						itemRect.x = position.x;
						itemRect.y += ( float )( rowSpacing + rowHeight ); 
						itemRect.width = position.width - 80;
						property.FindPropertyRelative( "bottomVal" ).floatValue = EditorGUI.FloatField( itemRect, "Bottom", property.FindPropertyRelative( "bottomVal" ).floatValue );
						
						itemRect.x = position.x + position.width - 80;
						itemRect.width = 80.0f;
						property.FindPropertyRelative( "bottomUnits" ).enumValueIndex = EditorGUI.Popup( itemRect, property.FindPropertyRelative( "bottomUnits" ).enumValueIndex, new string[] {"px", "%"} );
					}
					break;
				
					case ( int )PositionType.Center:
					itemRect.width = position.width - 80;
					property.FindPropertyRelative( "v" ).floatValue = EditorGUI.FloatField( itemRect, "Offset", property.FindPropertyRelative( "v" ).floatValue );

					itemRect.x = position.x + position.width - 80;
					itemRect.width = 80.0f;
					property.FindPropertyRelative( "vUnits" ).enumValueIndex = EditorGUI.Popup( itemRect, property.FindPropertyRelative( "vUnits" ).enumValueIndex, new string[] {"px", "%"} );
					break;

				}
				
				/* -- SIZE -- */
				
				itemRect.x = position.x;
				itemRect.width = position.width - 80;
				itemRect.y += ( float )( rowSpacing + rowHeight ); 		
				property.FindPropertyRelative( "widthVal" ).floatValue = EditorGUI.FloatField( itemRect, "Width", property.FindPropertyRelative( "widthVal" ).floatValue );
				
				itemRect.x = position.x + position.width - 80;
				itemRect.width = 80.0f;
				property.FindPropertyRelative( "widthUnits" ).enumValueIndex = EditorGUI.Popup( itemRect, property.FindPropertyRelative( "widthUnits" ).enumValueIndex, new string[] {"pixels", "% screen width", "% panel height"} );
				
				itemRect.x = position.x;
				itemRect.width = position.width - 80;
				itemRect.y += ( float )( rowSpacing + rowHeight ); 		
				property.FindPropertyRelative( "heightVal" ).floatValue = EditorGUI.FloatField( itemRect, "Height", property.FindPropertyRelative( "heightVal" ).floatValue );
				
				itemRect.x = position.x + position.width - 80;
				itemRect.width = 80.0f;
				property.FindPropertyRelative( "heightUnits" ).enumValueIndex = EditorGUI.Popup( itemRect, property.FindPropertyRelative( "heightUnits" ).enumValueIndex, new string[] {"pixels", "% screen height", "% panel width"} );
				
				/* -- ASPECT RATIO -- */
				
				itemRect.x = position.x;
				itemRect.width = position.width;
				itemRect.y += ( float )( rowSpacing + rowHeight ); 		
				property.FindPropertyRelative( "minAspectRatioVal" ).floatValue = EditorGUI.FloatField( itemRect, "Min Aspect Ratio", property.FindPropertyRelative( "minAspectRatioVal" ).floatValue );

				itemRect.y += ( float )( rowSpacing + rowHeight ); 		
				property.FindPropertyRelative( "maxAspectRatioVal" ).floatValue = EditorGUI.FloatField( itemRect, "Max Aspect Ratio", property.FindPropertyRelative( "maxAspectRatioVal" ).floatValue );

				itemRect.y += ( float )( rowSpacing + rowHeight ); 		
				property.FindPropertyRelative( "hAlignType" ).enumValueIndex = EditorGUI.Popup( itemRect, "H Alignment", property.FindPropertyRelative( "hAlignType" ).enumValueIndex, new string[] {"Left", "Center", "Right"} );

				itemRect.y += ( float )( rowSpacing + rowHeight ); 		
				property.FindPropertyRelative( "vAlignType" ).enumValueIndex = EditorGUI.Popup( itemRect, "V Alignment", property.FindPropertyRelative( "vAlignType" ).enumValueIndex, new string[] {"Top", "Middle", "Bottom"} );
						
				/* -- MARGINS -- */
				
				itemRect.y += ( float )( rowSpacing + rowHeight ); 		
				property.FindPropertyRelative( "marginTopVal" ).floatValue = EditorGUI.FloatField( itemRect, "Margin Top", property.FindPropertyRelative( "marginTopVal" ).floatValue );
				
				itemRect.y += ( float )( rowSpacing + rowHeight ); 		
				property.FindPropertyRelative( "marginRightVal" ).floatValue = EditorGUI.FloatField( itemRect, "Margin Right", property.FindPropertyRelative( "marginRightVal" ).floatValue );
				
				itemRect.y += ( float )( rowSpacing + rowHeight ); 		
				property.FindPropertyRelative( "marginBottomVal" ).floatValue = EditorGUI.FloatField( itemRect, "Margin Bottom", property.FindPropertyRelative( "marginBottomVal" ).floatValue );
				
				itemRect.y += ( float )( rowSpacing + rowHeight ); 		
				property.FindPropertyRelative( "marginLeftVal" ).floatValue = EditorGUI.FloatField( itemRect, "Margin Left", property.FindPropertyRelative( "marginLeftVal" ).floatValue );
				
				/* -- MATTE -- */
				
				itemRect.y += ( float )( rowSpacing + rowHeight ); 		
				property.FindPropertyRelative( "matteColor" ).colorValue = EditorGUI.ColorField( itemRect, "Matte Color", property.FindPropertyRelative( "matteColor" ).colorValue );

			}		
			
			EditorGUI.EndProperty();       
		 	        
		}

	}
}