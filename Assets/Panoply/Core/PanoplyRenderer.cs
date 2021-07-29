using UnityEngine;
using System;
using Opertoon.Panoply;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

/**
 * The PanoplyRenderer class executes global rendering tasks.
 * Copyright © Erik Loyer
 * erik@opertoon.com
 * Part of the Panoply engine
 */

namespace Opertoon.Panoply
{

	[ExecuteInEditMode ()]
	public class PanoplyRenderer : MonoBehaviour
	{

		public Vector2 referenceScreenSize = new Vector2 (1024.0f, 768.0f);

		[Range (0.0f, 1.0f)]
		public float matchWidthHeight = 0.5f;

		public bool enforceAspectRatio = false;

		public UIText panelCaptionPrefab;

		[HideInInspector]
		public float aspectRatio;

		[HideInInspector]
		public Rect screenRect = new Rect ();           // size of the addressable screen area after aspect ratio enforcement (if any)

		[HideInInspector]
		public Rect scaledScreenRect = new Rect ();     // the screenRect, scaled according to the current resolutionScale

		Texture2D matteTexture;
		Caption [] captions;
		//ScriptablePanel[] scriptablePanels;

		[HideInInspector]
		public Canvas targetCanvas;

		[HideInInspector]
		public RectTransform maskRectTransform;

		private Vector2 _maskSizeDelta;

		public void Start ()
		{

			targetCanvas = FindObjectOfType (typeof (Canvas)) as Canvas;
			maskRectTransform = targetCanvas.transform.Find ("Mask").GetComponent<RectTransform> ();
			_maskSizeDelta = new Vector2 ();

			matteTexture = new Texture2D (1, 1);
			matteTexture.SetPixel (0, 0, Color.white);
			matteTexture.Apply ();

			UpdateInventory ();
			ResetCanvas ();

			CalculateScreenRect ();

		}

		public void UpdateInventory ()
		{
			captions = FindObjectsOfType (typeof (Caption)) as Caption [];
			//scriptablePanels = FindObjectsOfType( typeof( ScriptablePanel ) ) as ScriptablePanel[];
		}

		public void HandleCaptionStart (Caption caption)
		{
			UpdateInventory ();
		}

		public void RenderFrame (FrameState stateA,
								FrameState stateB,
								Rect drawRect,
								float progress)
		{

			Color color = Color.clear;

			if ((stateA != null) && (stateB != null)) {
				color = Color.Lerp (stateA.matteColor, stateB.matteColor, progress);
				if (color != Color.clear) {
					RenderFrame (drawRect, color);
				}
			}

		}

		public void RenderFrame (Rect drawRect, Color color)
		{
			if (color.a > 0) {
				GUI.color = color;
				drawRect.y = Screen.height - drawRect.y - drawRect.height;
				GUI.DrawTexture (new Rect (drawRect), matteTexture);
			}
		}

		public void RenderSelection (Rect drawRect)
		{

			//GUI.color = Color( .3, .49, .84, 1.0 );
			GUI.color = Color.gray;
			GUI.DrawTexture (new Rect (drawRect.x, drawRect.y, drawRect.width, 1.0f), matteTexture);
			GUI.DrawTexture (new Rect (drawRect.x + drawRect.width - 1, drawRect.y, 1.0f, drawRect.height), matteTexture);
			GUI.DrawTexture (new Rect (drawRect.x, drawRect.y + drawRect.height - 1, drawRect.width, 1.0f), matteTexture);
			GUI.DrawTexture (new Rect (drawRect.x, drawRect.y, 1.0f, drawRect.height), matteTexture);
			GUI.color = Color.white;

		}

		private void ResetCanvas ()
		{
			int i;
			int n;
			if (targetCanvas != null) {
				UIText [] captions = targetCanvas.GetComponentsInChildren<UIText> (true);
				n = captions.Length;
				for (i = 0; i < n; i++) {
					GameObject.DestroyImmediate (captions [i].gameObject);
				}
			}
		}

		public void CalculateScreenRect ()
		{
			aspectRatio = referenceScreenSize.x / referenceScreenSize.y;
			if (enforceAspectRatio) {
				float screenAR = Screen.width / (float)Screen.height;
				if (aspectRatio > screenAR) {
					screenRect.width = Screen.width;
					screenRect.height = Screen.width / aspectRatio;
				} else {
					screenRect.width = Screen.height * aspectRatio;
					screenRect.height = Screen.height;
				}
				screenRect.x = (Screen.width - screenRect.width) * .5f;
				screenRect.y = (Screen.height - screenRect.height) * .5f;
			} else {
				screenRect.x = 0.0f;
				screenRect.y = 0.0f;
				screenRect.width = Screen.width;
				screenRect.height = Screen.height;
			}
			scaledScreenRect.x = screenRect.x / PanoplyCore.resolutionScale;
			scaledScreenRect.y = screenRect.y / PanoplyCore.resolutionScale;
			scaledScreenRect.width = screenRect.width / PanoplyCore.resolutionScale;
			scaledScreenRect.height = screenRect.height / PanoplyCore.resolutionScale;
			_maskSizeDelta.x = screenRect.width - Screen.width;
			_maskSizeDelta.y = screenRect.height - Screen.height;
			if (targetCanvas != null) {
				if (maskRectTransform == null) {
					maskRectTransform = targetCanvas.transform.Find ("Mask").GetComponent<RectTransform> ();
				}
				maskRectTransform.sizeDelta = _maskSizeDelta;
			}
		}

		public void OnGUI ()
		{

			GUI.depth = 10;

			int i = 0;
			int n = 0;
			Caption caption;
			//ScriptablePanel scriptablePanel = null;

			if (!Application.isPlaying || (Time.timeSinceLevelLoad > .1)) {
				n = captions.Length;
				for (i = 0; i < n; i++) {
					caption = captions [i];
					if ((caption != null) && caption.enabled && caption.renderingMethod == RenderingMethod.LegacyGUI) {
						caption.Render ();
					}
				}
			}

		}

		public void LateUpdate ()
		{

			CalculateScreenRect ();

		}
	}
}