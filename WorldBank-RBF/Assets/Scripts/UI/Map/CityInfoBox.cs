﻿using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class CityInfoBox : MB {

	public GameObject panel;
	public Text header;
	public Text body;
	public CityInfoBoxButton button1;
	public CityInfoBoxButton button2;
	public MapManager2 mapManager;

	string Header {
		get { return header.text; }
		set { header.text = value; }
	}

	string Body {
		get { return body.text; }
		set { body.text = value; }
	}

	public void Open (CityButton button) {
		
		CityItem city = button.CityItem;
		bool currentCity = button.CityItem.Symbol == PlayerData.CityGroup.CurrentCity;

		if (city.Visited) {
			if (city.StayedExtraDay) {
				Body = "You've already visited this city but you can pass through it.";
				SetButtons ("Cancel", Close, "Visit", () => TravelTo (city, button.ActiveRoute));
			} else {
				Body = "You've already visited this city but you can pass through it or spend an extra day talking to the rest of the nice people :)";
				if (currentCity) {
					SetButtons ("Cancel", Close, "Extra Day", () => StayExtraDay (city));
				} else {
					SetButtons ("Cancel", Close, "Visit", () => TravelTo (city, button.ActiveRoute));	
				}
			}
		} else {
			Body = city.Model.description;
			SetButtons ("Cancel", Close, "Visit", () => Visit (city, button.ActiveRoute));
		}

		Header = city.Model.display_name;
		panel.SetActive (true);
	}

	public void OpenRouteBlocked () {
		Header = "Route DESTROYED";
		Body = "Oopsie shippy dip! Can't go this dang way, but it's looking good for a hop/skip/and-a-kick to ~~ kooky ~~ Kibari! ;)";
		SetButtons ("Ok", UnlockRoute);
		panel.SetActive (true);
	}

	void UnlockRoute () {
		PlayerData.LockRoute ("mile_to_zima");
		PlayerData.UnlockImplementation("unlockable_route_kibari_to_mile");
		Close ();
	}

	void Close () {
		panel.SetActive (false);
	}

	void TravelTo (CityItem city, RouteItem route) {
		CitiesManager.Instance.TravelToCity (city, route);
		Close ();
	}

	void Visit (CityItem city, RouteItem route) {
		CitiesManager.Instance.VisitCity (city, route);
		Close ();
	}

	void StayExtraDay (CityItem city) {
		CitiesManager.Instance.StayExtraDay (city);
		Close ();
	}

	void SetButtons (string label1, UnityAction onButton1, string label2="", UnityAction onButton2=null) {
		button1.gameObject.SetActive (true);
		button1.Label = label1;
		button1.Button.onClick.RemoveAllListeners ();
		button1.Button.onClick.AddListener (onButton1);
		if (label2 != "") {
			button2.gameObject.SetActive (true);
			button2.Label = label2;
			button2.Button.onClick.RemoveAllListeners ();
			button2.Button.onClick.AddListener (onButton2);
		} else {
			button2.gameObject.SetActive (false);
		}
	}
}
