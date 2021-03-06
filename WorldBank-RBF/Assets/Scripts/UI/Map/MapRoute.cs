﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// A representation of the route on the map.
/// </summary>
public class MapRoute : MB {

	RectTransform travelDays = null;
	RectTransform TravelDays {
		get {
			if (travelDays == null) {
				travelDays = Transform.GetChild (1).GetComponent<RectTransform> ();
			}
			return travelDays;
		}
	}

	Transform routeImage = null;
	Transform RouteImage {
		get {
			if (routeImage == null) {
				routeImage = Transform.GetChild (0);
			}
			return routeImage;
		}
	}

	Image image = null;
	Image Image {
		get {
			if (image == null) {
				image = RouteImage.GetComponent<Image> ();
			}
			return image;
		}
	}

	/// <summary>
	/// Sets the unlocked state of the route. Hides the route image and text if the route is not unlocked.
	/// </summary>
	bool Unlocked {
		set {
			RouteImage.gameObject.SetActive (value);
			TravelDays.gameObject.SetActive (value);
		}
	}

	/// <summary>
	/// Gets the two cities that the route connects.
	/// </summary>
	public Terminals Terminals {
		get { return new Terminals (city1, city2); }
	}

	/// <summary>
	/// Sets the text that represents the cost to travel along the route.
	/// </summary>
	int Cost {
		set { costText.text = value.ToString (); }
	}

	RouteItem routeItem = null;

	/// <summary>
	/// Gets/sets the RouteItem associated with this MapRoute. Setting also updates the cost and 
	/// unlocked state. This can only be set once.
	/// </summary>
	public RouteItem RouteItem { 
		get { return routeItem; }
		set {
			if(value == null)
				throw new System.Exception("Route item is null!");
				
			if (routeItem == null) {
				routeItem = value;
				routeItem.onUpdateUnlocked += OnUpdateUnlocked;
				Unlocked = routeItem.Unlocked;
				Cost = routeItem.Cost;
			}
		}
	}
	
	public string city1;
	public string city2;
	public Text costText;
	bool newUnlock = false;

	public void OnUpdateUnlocked () {
		Unlocked = routeItem.Unlocked;
		newUnlock = true;
	}

	void OnEnable () {
		if (newUnlock) {
			StartCoroutine (CoBlink ());
			newUnlock = false;
		} else {
			Image.color = Color.white;
		}
	}

	IEnumerator CoBlink () {
		
		float time = 5f;
		float speed = 1.5f;
		float eTime = 0f;
	
		while (eTime < time) {
			eTime += Time.deltaTime * speed;
			Image.color = new Color (1, 1, 1, Mathf.PingPong (eTime, 1f));
			yield return null;
		}
	}
}
