﻿using UnityEngine;
using UnityEngine.UI;

public class TitleScreen : MonoBehaviour {

	public MenusManager menus;
	public Text email;
	public Text txtError;

	bool returningPlayer = true;

	void Awake () {
		Events.instance.AddListener<PlayerLoginEvent>(OnFormEvent);
		ClearError ();
	}

	public void OnLogin () {
		string e = email.text;
		if (e == "") {
			ShowError ("Please enter an email address.");
		} else {
			PlayerManager.Instance.Authenticate(e.Replace ("\n", ""));
		}
	}

	public void OnRegister () {
		menus.SetScreen ("register");
	}

	public void OnEmailInput () {
		ClearError ();
	}

	void OnFormEvent (PlayerLoginEvent e) {

    	if (!e.success) {
	    	// txtError.text = e.error;
	    	// txtError.gameObject.SetActive(true);
	    	Debug.Log ("no success");
	    } else {
	    	Debug.Log ("success");
	    	OnAuthenticate ();
	    }
    }

    void OnAuthenticate () {
    	if (returningPlayer) {
    		menus.SetScreen ("phase");
		} else {
			menus.SetScreen ("loading");
		}
    }

    void ShowError (string error) {
    	txtError.text = error;
    }

    void ClearError () {
    	txtError.text = "";
    }
}
