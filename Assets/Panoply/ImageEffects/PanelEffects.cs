using UnityEngine;
using System.Collections;

namespace Opertoon.Panoply
{
    [ExecuteInEditMode]
    public class PanelEffects : MonoBehaviour
    {
        public Color matteColor;
        public Color borderColor;
        public Border border;

        private Material _material;
        private Camera _camera;
		private Camera _backgroundCamera;

        // Creates a private material used to the effect
        void Awake()
        {
            _material = new Material(Shader.Find("Hidden/PanelShader"));
            _camera = GetComponent<Camera>();
			_backgroundCamera = FindObjectOfType<PanoplyCore> ().transform.Find ("Background Camera").GetComponent<Camera> ();
        }

        // Postprocess the image
        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (matteColor == Color.black && borderColor == Color.black)
            {
                Graphics.Blit(source, destination);
                return;
            }
            _material.SetColor("_TintColor", matteColor);
			if (_backgroundCamera != null) {
				_material.SetColor ("_BackgroundColor", _backgroundCamera.backgroundColor);
			}
			_material.SetColor("_BorderColor", borderColor);
            _material.SetFloat("_BorderTop", border.top);
            _material.SetFloat("_BorderRight", border.right);
            _material.SetFloat("_BorderBottom", border.bottom);
            _material.SetFloat("_BorderLeft", border.left);
            _material.SetFloat("_OffsetLeft", _camera.pixelRect.xMin);
            _material.SetFloat("_OffsetBottomInverted", Screen.height - _camera.pixelRect.yMin);
            _material.SetFloat("_OffsetTopInverted", Screen.height - _camera.pixelRect.yMax);
            _material.SetFloat("_OffsetBottom", _camera.pixelRect.yMin);
            _material.SetFloat("_OffsetTop", _camera.pixelRect.yMax);
            if (PanoplyCore.panoplyRenderer != null)
            {
                _material.SetFloat("_ScreenLeft", PanoplyCore.panoplyRenderer.screenRect.xMin);
                _material.SetFloat("_ScreenRight", PanoplyCore.panoplyRenderer.screenRect.xMax);
                _material.SetFloat("_ScreenBottomInverted", Screen.height - PanoplyCore.panoplyRenderer.screenRect.yMin);
                _material.SetFloat("_ScreenTopInverted", Screen.height - PanoplyCore.panoplyRenderer.screenRect.yMax);
                _material.SetFloat("_ScreenBottom", PanoplyCore.panoplyRenderer.screenRect.yMin);
                _material.SetFloat("_ScreenTop", PanoplyCore.panoplyRenderer.screenRect.yMax);
            }
            Graphics.Blit(source, destination, _material);
        }
    }
}