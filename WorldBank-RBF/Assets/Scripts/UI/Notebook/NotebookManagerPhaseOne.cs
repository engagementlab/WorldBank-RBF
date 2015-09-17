﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NotebookManagerPhaseOne : MonoBehaviour {

	static NotebookManagerPhaseOne instance = null;
	static public NotebookManagerPhaseOne Instance {
		get {
			if (instance == null) {
				instance = Object.FindObjectOfType (typeof (NotebookManagerPhaseOne)) as NotebookManagerPhaseOne;
			}
			return instance;
		}
	}

	bool isOpen = false;
	public bool IsOpen { 
		get { return isOpen; }
		set {
			isOpen = value;
			MainCamera.Instance.Positioner.Drag.Enabled = !isOpen;
		}
	}

	public bool CanCloseNotebook {
		get {
			string currentCity = PlayerData.CityGroup.CurrentCity;
			Debug.Log (currentCity + ", " + DataManager.SceneContext);
			return (
				(currentCity == DataManager.SceneContext
				&& !NotebookManager.Instance.MakingPlan)
			);
		}
	}

	public List<CanvasToggle> toggles;

	public void CloseCanvases () {
		foreach (CanvasToggle toggle in toggles) {
			toggle.Close ();
		}
	}
}
