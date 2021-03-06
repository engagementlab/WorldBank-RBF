﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class CurrentCityIndicator : MB {

	public Sprite bus;
	public Sprite train;
	public Sprite truck;
	public Sprite airship;

	Image travelImage = null;
	Image TravelImage {
		get {
			if (travelImage == null) {
				travelImage = Transform.GetChild (0).GetComponent<Image> ();
			}
			return travelImage;
		}
	}

	static bool moving = false;
	public static bool Moving {
		get { return moving; }
	}

	public void Move (Transform parent, RouteItem route, System.Action onEnd=null) {

		SetTravelImage (route.TransportationMode);
		
		List<Vector3> positions;
		try {
			positions = route.Positions.ConvertAll (x => x); // Clone the list
		} catch {
			throw new System.Exception ("The route between " + route.Terminals.city1 + " and " + route.Terminals.city2 + " does not have a list of positions.");
		}
		
		bool reverse = route.Terminals.Reverse (PlayerData.CityGroup.CurrentCity);
		float speed = route.Speed;

		Parent = parent;
		Parent.SetSiblingLast ();
		if (reverse)
			positions.Reverse ();

		if (moving) return;
		moving = true;
		StartCoroutine (CoMoveToRoute (positions[0], 
			() => StartCoroutine (CoMove (positions, speed, onEnd))
		));
	}

	void SetTravelImage (string mode) {
		switch (mode) {
			case "train": TravelImage.sprite = train; break;
			case "truck": TravelImage.sprite = truck; break;
			case "bus": TravelImage.sprite = bus; break;
			case "airship": TravelImage.sprite = airship; break;
		}
	}

	IEnumerator CoMoveToRoute (Vector3 endPosition, System.Action onEnd) {

		float eTime =  0f;
		float time = 0.5f;
		Vector3 startPosition = LocalPosition;

		while (eTime < time) {
			eTime += Time.deltaTime;
			float p = Mathf.SmoothStep (0, 1, eTime / time);
			LocalPosition = Vector3.Lerp (startPosition, endPosition, p);
			yield return null;
		}

		onEnd ();
	}

	IEnumerator CoMove (List<Vector3> positions, float speed, System.Action onEnd) {
		
		float positionCount = (float)positions.Count-1;
		int index = 0;

		while (index < positionCount) {
			yield return StartCoroutine (CoMove (positions[index], positions[index+1], speed));
			index ++;
			yield return null;
		}

		moving = false;
		if (onEnd != null) onEnd ();
	}

	IEnumerator CoMove (Vector3 fromPoint, Vector3 toPoint, float speed=25f) {
		
		float time = Vector3.Distance (fromPoint, toPoint) / speed;
		float eTime = 0f;
	
		while (eTime < time) {
			eTime += Time.deltaTime;
			float progress = eTime / time;
			LocalPosition = Vector3.Lerp (fromPoint, toPoint, progress);
			yield return null;
		}
	}
}
