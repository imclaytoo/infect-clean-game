using UnityEngine;
using System.Collections;

namespace Opertoon.Panoply {

	public class AbstractLocalizer : MonoBehaviour {

		public virtual void SetLanguage( string language ) {
		}

		// get localized text from the localization solution
		public virtual string GetLocalizedText( string key ) {
			return key;
		}

	}

}