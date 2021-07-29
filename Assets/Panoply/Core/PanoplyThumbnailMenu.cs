using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/**
 * The PanoplyThumbnailMenu class manages a visual index to steps in the scene.
 * Copyright © Erik Loyer
 * erik@opertoon.com
 * Part of the Panoply engine
 */

namespace Opertoon.Panoply {
	public class PanoplyThumbnailMenu : MonoBehaviour {

		[HideInInspector]
		public EventSystem eventSystemPrefab;

		public bool useThumbnailMenu;
		public bool useMenuToggleButton = true;
		public Sprite openMenuIcon;
		public Sprite closeMenuIcon;
		public float menuHeight = 100;
		public int menuPadding = 10;
		public ThumbnailMenuItem[] thumbnails;

		[HideInInspector]
		public bool isOpen;

		[HideInInspector]
		public Button buttonPrefab;

		private Transform _menuRoot;
		private RectTransform _menu;
		private Button _menuToggle;
		private Image _menuToggleImage;
		private Vector2 _menuPositionTarget;
		private Transform _menuContent;
		private Camera _foregroundCamera;

		// Use this for initialization
		void Start () {
		}

		public void ToggleMenu() {
			isOpen = !isOpen;
			UpdateMenuToggleSprite();
		}

		public void SetMenuOpen(bool menuIsOpen) {
			isOpen = menuIsOpen;
			UpdateMenuToggleSprite();
		}

		public bool ScreenPointIsInsideMenu(Vector2 point) {
			return RectTransformUtility.RectangleContainsScreenPoint(_menu, point, _foregroundCamera) || RectTransformUtility.RectangleContainsScreenPoint(_menuToggle.GetComponent<RectTransform>(), point, _foregroundCamera);
		}

		public void SetMenuHeightAndPadding(float h, float p) {
			Vector2 sizeDelta = _menu.sizeDelta;
			sizeDelta.y = h;
			_menu.sizeDelta = sizeDelta;
			Button thumbnailButton;
			ThumbnailMenuItem thumbnailIndex;
			float ar;
			int n = _menuContent.childCount;
			for (int i = 0; i < n; i++) {
				thumbnailIndex = thumbnails[i];
				thumbnailButton = _menuContent.GetChild(i).GetComponent<Button>();
				ar = thumbnailIndex.thumbnail.width / (float)thumbnailIndex.thumbnail.height;
				thumbnailButton.GetComponent<RectTransform>().sizeDelta = new Vector2((menuHeight - (menuPadding * 2)) * ar, menuHeight - (menuPadding * 2));
			}
			_menuContent.GetComponent<HorizontalLayoutGroup>().padding = new RectOffset(menuPadding, menuPadding, menuPadding, menuPadding);
		}

		public void HandleThumbnailButton() {
			int index;
			index = EventSystem.current.currentSelectedGameObject.transform.GetSiblingIndex();
			ThumbnailMenuItem thumbnailMenuItem = thumbnails[index];
			PanoplyCore.SetTargetStep(thumbnailMenuItem.step);
			if (thumbnailMenuItem.instantTransition) {
				PanoplyCore.SetInterpolatedStep(thumbnailMenuItem.step);
			}
		}

		private void InitializeMenu()
		{
			if (GameObject.Find ("EventSystem") == null) {
				Instantiate (eventSystemPrefab);
			}

			_menuRoot = PanoplyCore.panoplyRenderer.targetCanvas.transform.Find ("Menu");
			_menu = PanoplyCore.panoplyRenderer.targetCanvas.transform.Find ("Menu/ThumbnailMenu").GetComponent<RectTransform> ();
			_menuPositionTarget.y = -menuHeight;
			_menu.anchoredPosition = _menuPositionTarget;
			_foregroundCamera = _menuRoot.parent.parent.Find ("Foreground Camera").GetComponent<Camera> ();

			_menuToggle = PanoplyCore.panoplyRenderer.targetCanvas.transform.Find ("Menu/MenuButton").GetComponent<Button> ();
			_menuToggle.onClick.AddListener (delegate { ToggleMenu (); });
			_menuToggleImage = _menuToggle.GetComponent<Image> ();
			if (useMenuToggleButton) {
				UpdateMenuToggleSprite ();
			} else {
				_menuToggle.gameObject.SetActive (false);
			}

			_menuContent = PanoplyCore.panoplyRenderer.targetCanvas.transform.Find ("Menu/ThumbnailMenu/Viewport/Content");
			Button [] buttons = _menuContent.GetComponentsInChildren<Button> ();
			int n = buttons.Length;
			for (int i = 0; i < n; i++) {
				GameObject.DestroyImmediate (buttons [i].gameObject);
			}

			Button thumbnailButton;
			ThumbnailMenuItem thumbnailIndex;
			float ar;
			n = thumbnails.Length;
			for (int i = 0; i < n; i++) {
				thumbnailButton = Instantiate (buttonPrefab, _menuContent).GetComponent<Button> ();
				thumbnailIndex = thumbnails [i];
				thumbnailButton.GetComponent<RawImage> ().texture = thumbnailIndex.thumbnail;
				ar = thumbnailIndex.thumbnail.width / (float)thumbnailIndex.thumbnail.height;
				thumbnailButton.GetComponent<RectTransform> ().sizeDelta = new Vector2 ((menuHeight - (menuPadding * 2)) * ar, menuHeight - (menuPadding * 2));
				thumbnailButton.onClick.AddListener (delegate { HandleThumbnailButton (); });
			}

			SetMenuHeightAndPadding (menuHeight, menuPadding);
		}

		private void UpdateMenuToggleSprite() {
			if (isOpen) {
				_menuToggleImage.sprite = closeMenuIcon;
			} else {
				_menuToggleImage.sprite = openMenuIcon;
			}
			_menuToggleImage.color = Color.white;
			_menuToggleImage.enabled = true;
		}
		
		// Update is called once per frame
		void Update () {
			if (_menuRoot == null && PanoplyCore.panoplyRenderer != null) {
				InitializeMenu ();
			}
			_menuRoot.gameObject.SetActive (useThumbnailMenu);
			if (isOpen) {
				_menuPositionTarget.y = 0;
			} else {
				_menuPositionTarget.y = -menuHeight;
			}
			if (useThumbnailMenu && !_menuToggle.gameObject.activeSelf && useMenuToggleButton) {
				_menuToggle.gameObject.SetActive(true);
			} else if (_menuToggle.gameObject.activeSelf && (!useThumbnailMenu || !useMenuToggleButton)) {
				_menuToggle.gameObject.SetActive(false);
			}
			_menu.anchoredPosition = Vector2.Lerp(_menu.anchoredPosition, _menuPositionTarget, Time.deltaTime * 8);
			_menuRoot.SetSiblingIndex(9999);
		}
	}
}
