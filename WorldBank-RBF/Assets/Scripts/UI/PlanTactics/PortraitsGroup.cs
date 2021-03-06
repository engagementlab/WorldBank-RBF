﻿using UnityEngine;
using UnityEngine.EventSystems;

public class PortraitsGroup : MonoBehaviour {

	CanvasGroup canvasGroup = null;
	CanvasGroup CanvasGroup {
		get {
			if (canvasGroup == null) {
				canvasGroup = GetComponent<CanvasGroup> ();
			}
			return canvasGroup;
		}
	}

	public bool BlockRaycasts {
		get { return CanvasGroup.blocksRaycasts; }
		set { CanvasGroup.blocksRaycasts = value; }
	}
}
