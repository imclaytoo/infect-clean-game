using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using System.Collections.Generic;

/**
 * The UICaption class draws a caption to the UI system.
 * Adapted from the Unity UI Extensions project: https://bitbucket.org/UnityUIExtensions/unity-ui-extensions
 */

namespace Opertoon.Panoply
{
	public class CaptionPoint {
		public Vector2 point;
		public float angle;
		public bool isStem;

		public CaptionPoint(Vector2 p, float a) {
			point = p;
			angle = a;
			isStem = false;
		}

		public CaptionPoint(Vector2 p, float a, bool s) {
			point = p;
			angle = a;
			isStem = s;
		}
	}

    public class UIText : UIPrimitiveBase {
        [Tooltip("The number of segments to draw the primitive, more segments = smoother primitive")]
        [Range(0, 360)]
        [SerializeField]
        private int m_segments = 360;

		private RectTransform _rectTransform;
		private RectTransform _boxRectTransform;
		private List<CaptionPoint> _captionPoints;
		private Shadow _shadow;
		private RectTransform _textRectTransform;
		private Text _textField;
		private CaptionState _captionState;
		private HorizontalLayoutGroup _boxLayoutGroup;
		private Vector2 _anchoredPosition;
		private Vector2 _sizeDelta;
		private Color _textColor;
		private Vector2 _rectCenter;
		private Vector2 _half = new Vector2 (.5f, .5f);

		protected override void Start ()
		{
			_boxRectTransform = transform.Find("Box").GetComponent<RectTransform>();
			_captionPoints = new List<CaptionPoint>();
		}

		void Update() {
			if (_boxRectTransform == null) {
				_boxRectTransform = transform.Find ("Box").GetComponent<RectTransform> ();
			}
		}

		public void UpdateForCaptionState(CaptionState captionState, Panel panel, int fontSize) {
			float guiScale = PanoplyCore.resolutionScale / .5f;
			_captionState = captionState;
			_captionState.tailRotation *= -1;
			_captionState.tailRotation += 90;
			if (_rectTransform == null) {
				_rectTransform = GetComponent<RectTransform>();
			}

			if (_captionState.drawTail) {
				_anchoredPosition.x = panel.frameRect.x - PanoplyCore.panoplyRenderer.screenRect.x;
				_anchoredPosition.y = panel.frameRect.y - PanoplyCore.panoplyRenderer.screenRect.y;
				_rectTransform.anchoredPosition = _anchoredPosition;
			} else {
				_rectTransform.anchoredPosition = Vector2.zero;
			}

			_sizeDelta.x = panel.frameRect.width;
			_sizeDelta.y = panel.frameRect.height;
			_rectTransform.sizeDelta = _sizeDelta;
			if (_shadow == null) {
				_shadow = GetComponent<Shadow>();
			}
			Vector2 effectDist = new Vector2(captionState.dropShadow.x, -captionState.dropShadow.y);
			_shadow.effectDistance = effectDist;
			_shadow.effectColor = Color.black;
			if (_boxLayoutGroup == null) {
				_boxLayoutGroup = transform.Find("Box").GetComponent<HorizontalLayoutGroup>();
			}
			if (_textRectTransform == null) {
				_textRectTransform = transform.Find("Box/Text").GetComponent<RectTransform>();
			}
			_boxLayoutGroup.padding.left = (int)Mathf.Round(captionState.skin.label.padding.left * guiScale);
			_boxLayoutGroup.padding.right = (int)Mathf.Round (captionState.skin.label.padding.right * guiScale);
			_boxLayoutGroup.padding.top = (int)Mathf.Round (captionState.skin.label.padding.top * guiScale);
			_boxLayoutGroup.padding.bottom = (int)Mathf.Round (captionState.skin.label.padding.bottom * guiScale);
			Vector2 sizeDelta = _textRectTransform.sizeDelta;
			sizeDelta.x = captionState.layout.width - _boxLayoutGroup.padding.horizontal;
			_textRectTransform.sizeDelta = sizeDelta;
			_textRectTransform.anchoredPosition = new Vector2((_boxLayoutGroup.padding.left - _boxLayoutGroup.padding.right) * .5f, -(_boxLayoutGroup.padding.top - _boxLayoutGroup.padding.bottom) * .5f);
			_textRectTransform.anchorMin = _half;
			_textRectTransform.anchorMax = _half;
			if (_textField == null) {
				_textField = transform.Find("Box/Text").GetComponent<Text>();
			}
			_textField.font = captionState.skin.label.font;
			_textField.fontSize = fontSize / 2;
			_textField.fontStyle = captionState.skin.label.fontStyle;
			_textField.text = captionState.text;
			_textColor = captionState.skin.label.normal.textColor;
			_textColor.a = captionState.color.a;
			_textField.color = _textColor;
			_textField.alignment = captionState.skin.label.alignment;
			color = captionState.color;
			LayoutRebuilder.ForceRebuildLayoutImmediate(_rectTransform);
			SetAllDirty();
		}

        public int Segments {
            get { return m_segments; }
            set { m_segments = value; SetAllDirty(); }
        }

		//Calculate the intersection point of two lines. Returns true if lines intersect, otherwise false.
		//Note that in 3d, two lines do not intersect most of the time. So if the two lines are not in the 
		//same plane, use ClosestPointsOnTwoLines() instead.
		// Source: http://wiki.unity3d.com/index.php/3d_Math_functions
		private bool LineLineIntersection(out Vector3 intersection, Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2) {

			Vector3 lineVec3 = linePoint2 - linePoint1;
			Vector3 crossVec1and2 = Vector3.Cross(lineVec1, lineVec2);
			Vector3 crossVec3and2 = Vector3.Cross(lineVec3, lineVec2);

			float planarFactor = Vector3.Dot(lineVec3, crossVec1and2);

			//is coplanar, and not parrallel
			if(Mathf.Abs(planarFactor) < 0.0001f && crossVec1and2.sqrMagnitude > 0.0001f)
			{
				float s = Vector3.Dot(crossVec3and2, crossVec1and2) / crossVec1and2.sqrMagnitude;
				intersection = linePoint1 + (lineVec1 * s);
				return true;
			}
			else
			{
				intersection = Vector3.zero;
				return false;
			}
		}

        protected override void OnPopulateMesh(VertexHelper vh) {

			if (_captionState != null) {
	     
	            vh.Clear();
	     
	            Vector2 prevX = Vector2.zero;
	            Vector2 prevY = Vector2.zero;
	            Vector2 uv0 = new Vector2(0, 0);
	            Vector2 uv1 = new Vector2(0, 1);
	            Vector2 uv2 = new Vector2(1, 1);
	            Vector2 uv3 = new Vector2(1, 0);
	            Vector2 pos0;
	            Vector2 pos1;
	            Vector2 pos2;
	            Vector2 pos3;

				if (_captionPoints == null) {
					_captionPoints = new List<CaptionPoint>();
				}

				float tw = _boxRectTransform.rect.width;
				float th = _boxRectTransform.rect.height;

				float stemAngleRadians = 0;
				if (_captionState.drawTail) {
					stemAngleRadians = Mathf.Deg2Rad * _captionState.tailRotation;
					_rectCenter.x = (_captionState.layout.x * _rectTransform.sizeDelta.x) + Mathf.Cos(stemAngleRadians) * (_captionState.tailHeight * 1.33f);
					_rectCenter.y = ((1.0f - _captionState.layout.y) * _rectTransform.sizeDelta.y) + Mathf.Sin(stemAngleRadians) * (_captionState.tailHeight * 1.33f);
				} else {
					_rectCenter.x = _captionState.layout.x + (_boxRectTransform.sizeDelta.x * .5f) - PanoplyCore.panoplyRenderer.screenRect.x;

					// coordinates come in global, with the y-axis origin at the top, so we need to
					// invert the y coordinate and subtract the y position of the enclosing box,
					// since we need the y expressed in coordinates local to the box
					_rectCenter.y = Screen.height - _captionState.layout.y - PanoplyCore.panoplyRenderer.screenRect.y;

					// correct for the fact that the captionState layout rect is calculated based on an explicitly specified height,
					// which we need to ignore here since the height is determined by the text iself
					switch (_captionState.vertAlign) {
					case CaptionVertAlign.Top:
						_rectCenter.y -= _boxRectTransform.sizeDelta.y * .5f;
						break;
					case CaptionVertAlign.Center:
						_rectCenter.y -=  _captionState.layout.height * .5f;
						break;
					case CaptionVertAlign.Bottom:
						_rectCenter.y -=  _captionState.layout.height;
						_rectCenter.y += _boxRectTransform.sizeDelta.y * .5f;
						break;
					}
				}

				prevX = _rectCenter;
				prevY = _rectCenter;

				_boxRectTransform.anchoredPosition = _rectCenter;

				float angleByStep = (Mathf.PI * 2f) / m_segments;
	            float currentAngle = 0f;
				int quadrant;
				Vector2 cornerCenter;
				float radiusH;
				float radiusV;
				if (tw > th) {
					radiusH = (tw * _boxRectTransform.pivot.x) * _captionState.cornerRounding;
					radiusV = Mathf.Min(th * _boxRectTransform.pivot.y, radiusH);
				} else {
					radiusV = (th * _boxRectTransform.pivot.y) * _captionState.cornerRounding;
					radiusH = Mathf.Min(tw * _boxRectTransform.pivot.x, radiusV);
				}

				_captionPoints.Clear();

				float c;
				float s;
				Vector2 edge;
				float localAngle;

	            for (int i = 0; i < m_segments + 1; i++) {
					quadrant = (int)Mathf.Repeat(Mathf.Floor((float)i / ((float)m_segments / 4f)), 4);
					switch (quadrant) {
					case 0:
						// top right
						cornerCenter.x = tw * _boxRectTransform.pivot.x - radiusH;
						cornerCenter.y = th * _boxRectTransform.pivot.y - radiusV;
						break;
					case 1:
						// top left
						cornerCenter.x = -tw * _boxRectTransform.pivot.x + radiusH;
						cornerCenter.y = th * _boxRectTransform.pivot.y - radiusV;
						break;
					case 2:
						// bottom left
						cornerCenter.x = -tw * _boxRectTransform.pivot.x + radiusH;
						cornerCenter.y = -th * _boxRectTransform.pivot.y + radiusV;
						break;
					default:
						// bottom right
						cornerCenter.x = tw * _boxRectTransform.pivot.x - radiusH;
						cornerCenter.y = -th * _boxRectTransform.pivot.y + radiusV;
						break;
					}

					cornerCenter += _rectCenter;

	                c = Mathf.Cos(currentAngle);
	                s = Mathf.Sin(currentAngle);

					edge = new Vector2(cornerCenter.x + (radiusH * c), cornerCenter.y + (radiusV * s));
					localAngle = Mathf.Repeat(Mathf.Atan2(edge.y - _rectCenter.y, edge.x - _rectCenter.x), Mathf.PI * 2);

					_captionPoints.Add(new CaptionPoint(edge, localAngle));

	                currentAngle += angleByStep;
	            }

				CaptionPoint cp;
				int n;

				if (_captionState.drawTail) {

					float stemAngleRadiansProxy = Mathf.Repeat(stemAngleRadians + Mathf.PI, Mathf.PI * 2);
					CaptionPoint stemTip = new CaptionPoint(new Vector2(_rectTransform.sizeDelta.x * _captionState.layout.x, _rectTransform.sizeDelta.y * (1.0f - _captionState.layout.y)), stemAngleRadiansProxy, true);
					_captionPoints.Add(stemTip);

					_captionPoints.Sort((a,b) => a.angle.CompareTo(b.angle));

					List<int> indexesToRemove = new List<int>();
					int index = _captionPoints.IndexOf(stemTip);
					int targetIndexLow;
					int targetIndexHigh;
					CaptionPoint targetLow;
					CaptionPoint targetHigh;
					float diffLow;
					float diffHigh;
					Vector2 diffLowHigh;
					Vector2 stemEdge;
					float stemEdgeAngle;
					float lowRatio;
					float highRatio;
					float ratio;
					float stemWidthRadians = Mathf.Max(.01f, _captionState.tailWidth * .25f) * Mathf.Deg2Rad;
					targetIndexLow = index - 1;
					int iterations = 0;
					bool gotIt = false;

					/*
					Loop around all the points of the bubble, trying to find pairs of points that bracket
					where a new point described by the intersection of the stem with the bubble needs to go.
					Interpolate between the two points to find the location of the new point.
					*/

					do {
						// first iteration; uses the points on either side of the stem's tip to calculate the intersection
						// of that line with a line extending from the center of the bubble to the stem's tip
						if (iterations == 0) {
							targetIndexLow = (int)Mathf.Repeat(index - 1, _captionPoints.Count - 1);
							targetIndexHigh = (int)Mathf.Repeat(index + 1, _captionPoints.Count - 1);
							Vector3 intersection;
							targetLow = _captionPoints[targetIndexLow];
							targetHigh = _captionPoints[targetIndexHigh];
							LineLineIntersection(out intersection, targetLow.point, targetHigh.point - targetLow.point, _rectCenter, stemTip.point - _rectCenter);
							targetLow = new CaptionPoint(intersection, stemTip.angle);
							// second iteration; get the tip and point prior to it
						} else if (iterations == 1) {
							targetIndexLow = (int)Mathf.Repeat(index + 1, _captionPoints.Count - 1);
							targetIndexHigh = (int)Mathf.Repeat(targetIndexLow + 1, _captionPoints.Count - 1);
							targetLow = _captionPoints[targetIndexLow];
							targetHigh = _captionPoints[targetIndexHigh];
							// all other iterations; get the two points prior to the points from the last iteration
						} else {
							targetIndexLow = (int)Mathf.Repeat(targetIndexLow + 1, _captionPoints.Count - 1);
							targetIndexHigh = (int)Mathf.Repeat(targetIndexLow + 1, _captionPoints.Count - 1);
							targetLow = _captionPoints[targetIndexLow];
							targetHigh = _captionPoints[targetIndexHigh];
						}
						diffLow = Mathf.Abs(targetLow.angle - stemAngleRadiansProxy);
						diffHigh = Mathf.Abs(targetHigh.angle - stemAngleRadiansProxy);
						if (diffLow <= stemWidthRadians && diffHigh >= stemWidthRadians) {
							diffLowHigh = targetHigh.point - targetLow.point;
							lowRatio = diffLow / stemWidthRadians;
							highRatio = diffHigh / stemWidthRadians;
							ratio = ((1 - lowRatio) / (highRatio - lowRatio));
							stemEdge = targetLow.point + (diffLowHigh * ratio);
							stemEdgeAngle = Mathf.Atan2(stemEdge.y - _rectCenter.y, stemEdge.x - _rectCenter.x);
							stemEdgeAngle = Mathf.Repeat(stemEdgeAngle, Mathf.PI * 2);
							cp = new CaptionPoint(stemEdge, stemEdgeAngle);
							_captionPoints.Add(cp);
							gotIt = true;
						}
						if (targetHigh != stemTip) {
							indexesToRemove.Add(targetIndexHigh);
						}
						iterations++;
					} while (!gotIt && iterations < m_segments);

					iterations = 0;
					gotIt = false;
					do {
						if (iterations == 0) {
							targetIndexLow = (int)Mathf.Repeat(index - 1, _captionPoints.Count - 1);
							targetIndexHigh = (int)Mathf.Repeat(index + 1, _captionPoints.Count - 1);
							Vector3 intersection;
							targetLow = _captionPoints[targetIndexLow];
							targetHigh = _captionPoints[targetIndexHigh];
							LineLineIntersection(out intersection, targetLow.point, targetHigh.point - targetLow.point, _rectCenter, stemTip.point - _rectCenter);
							targetHigh = new CaptionPoint(intersection, stemTip.angle);
						} else if (iterations == 1) {
							targetIndexLow = (int)Mathf.Repeat(index - 1, _captionPoints.Count - 1);
							targetIndexHigh = (int)Mathf.Repeat(targetIndexLow + 1, _captionPoints.Count - 1);
							targetLow = _captionPoints[targetIndexLow];
							targetHigh = _captionPoints[targetIndexHigh];
						} else {
							targetIndexLow = (int)Mathf.Repeat(targetIndexLow - 1, _captionPoints.Count - 1);
							targetIndexHigh = (int)Mathf.Repeat(targetIndexLow + 1, _captionPoints.Count - 1);
							targetLow = _captionPoints[targetIndexLow];
							targetHigh = _captionPoints[targetIndexHigh];
						}
						diffHigh = Mathf.Abs(targetLow.angle - stemAngleRadiansProxy);
						diffLow = Mathf.Abs(targetHigh.angle - stemAngleRadiansProxy);
						if (diffLow <= stemWidthRadians && diffHigh >= stemWidthRadians) {
							diffLowHigh = targetHigh.point - targetLow.point;
							lowRatio = diffLow / stemWidthRadians;
							highRatio = diffHigh / stemWidthRadians;
							ratio = 1 - ((1 - lowRatio) / (highRatio - lowRatio));
							stemEdge = targetLow.point + (diffLowHigh * ratio);
							stemEdgeAngle = Mathf.Atan2(stemEdge.y - _rectCenter.y, stemEdge.x - _rectCenter.x);
							stemEdgeAngle = Mathf.Repeat(stemEdgeAngle, Mathf.PI * 2);
							cp = new CaptionPoint(stemEdge, stemEdgeAngle);
							_captionPoints.Add(cp);
							gotIt = true;
						}
						if (targetHigh != stemTip) {
							indexesToRemove.Add(targetIndexHigh);
						}
						iterations++;
					} while (!gotIt && iterations < m_segments);

					indexesToRemove.Sort();
					n = indexesToRemove.Count;
					for (int i = n - 1; i >= 0; i--) {
						_captionPoints.RemoveAt(indexesToRemove[i]);
					}

				}

				_captionPoints.Sort((a,b) => a.angle.CompareTo(b.angle));

				n = _captionPoints.Count;
				for (int i = 0; i < n; i++) {
					cp = _captionPoints[i];
					StepThroughPointsAndFill(_rectCenter, cp.point, ref prevX, ref prevY, out pos0, out pos1, out pos2, out pos3);
					uv0 = new Vector2(pos0.x / tw + 0.5f, pos0.y / th + 0.5f);
					uv1 = new Vector2(pos1.x / tw + 0.5f, pos1.y / th + 0.5f);
					uv2 = new Vector2(pos2.x / tw + 0.5f, pos2.y / th + 0.5f);
					uv3 = new Vector2(pos3.x / tw + 0.5f, pos3.y / th + 0.5f);
					vh.AddUIVertexQuad(SetVbo(new[] { pos0, pos1, pos2, pos3 }, new[] { uv0, uv1, uv2, uv3 }));
				}
				cp = _captionPoints[0];
				StepThroughPointsAndFill(_rectCenter, cp.point, ref prevX, ref prevY, out pos0, out pos1, out pos2, out pos3);
				uv0 = new Vector2(pos0.x / tw + 0.5f, pos0.y / th + 0.5f);
				uv1 = new Vector2(pos1.x / tw + 0.5f, pos1.y / th + 0.5f);
				uv2 = new Vector2(pos2.x / tw + 0.5f, pos2.y / th + 0.5f);
				uv3 = new Vector2(pos3.x / tw + 0.5f, pos3.y / th + 0.5f);
				vh.AddUIVertexQuad(SetVbo(new[] { pos0, pos1, pos2, pos3 }, new[] { uv0, uv1, uv2, uv3 }));

			}
         }

		private void StepThroughPointsAndFill(Vector2 center, Vector2 edge, ref Vector2 prevX, ref Vector2 prevY, out Vector2 pos0, out Vector2 pos1, out Vector2 pos2, out Vector2 pos3) {
            pos0 = prevX;
			pos1 = edge;

			pos2 = center;
			pos3 = center;

            prevX = pos1;
            prevY = pos2;
        }

	}
}