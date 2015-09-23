﻿/* 
World Bank RBF
Created by Engagement Lab, 2015
==============
 SceneManager.cs
 Unity scene management. Mostly handles data to/from static DataManager, but applying it only to this scene. Should likely be inside of any scene.

 Created by Johnny Richardson on 4/13/15.
==============
*/
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Parse;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class SceneManager : MonoBehaviour {

	/*static SceneManager instance = null;
	static public SceneManager Instance {
		get {
			if (instance == null) {
				instance = Object.FindObjectOfType (typeof (SceneManager)) as SceneManager;
				if (instance == null) {
					GameObject go = new GameObject ("SceneManager");
					DontDestroyOnLoad (go);
					instance = go.AddComponent<SceneManager>();
				}
			}
			return instance;
		}
	}*/

	public string sceneName;

	[HideInInspector]
	public int environmentIndex = 0;
	[HideInInspector]
	public string environment;

	[HideInInspector]
	public bool tutorialEnabled;

	public delegate void AuthCallbackDelegate();

	private PlayerLoginRegisterUI loginUI;

	void Awake () {

		// We need our game config data before calling any remote endpoints
		LoadGameConfig();

		// Authenticate to API
		NetworkManager.Instance.Authenticate(ClientAuthenticated);
		
		// Set global game data if needed
		SetGameData();
      
	}

    #if UNITY_EDITOR
	void OnGUI() {
		GUIStyle style = new GUIStyle();
		
		style.fontSize = 13;
		style.fontStyle = FontStyle.BoldAndItalic;

	    GUI.contentColor = Color.white;

        GUI.Label(new Rect(4, 4, 100, 20), "ENVIRONMENT: " + environment, style);
    }
    #endif

	/// <summary>
	/// Client was authenticated to API; we can now get game data and ask player to log in
	/// </summary>
    /// <param name="response">Dictionary containing "authed" key telling us if API auth </param>
    public void ClientAuthenticated(Dictionary<string, object> response) {

		Debug.Log("Client API auth successful? " + response["authed"]);

		if(!System.Convert.ToBoolean(response["authed"]))
			return;

		NetworkManager.Instance.Cookie = response["session_cookie"].ToString();

		// Authenticate player -- user/pass is hard-coded if in editor
		if(!PlayerManager.Instance.Authenticated)
		{

			#if UNITY_EDITOR
				if (EditorApplication.currentScene != "Assets/Scenes/Menus.unity") {
					PlayerManager.Instance.Authenticate("tester@elab.emerson.edu", "password");
				} /*else {
					loginUI = ObjectPool.Instantiate<PlayerLoginRegisterUI>();
					loginUI.Callback = UserAuthenticateResponse;	
				}*/
			#else
				// loginUI = ObjectPool.Instantiate<PlayerLoginRegisterUI>();
				// loginUI.Callback = UserAuthenticateResponse;
			#endif
			
		}

		DataManager.SceneContext = sceneName;

    }
	
	/// <summary>
	/// User attempted authentication; return/show error if failed
	/// </summary>
    /// <param name="success">Was authentication successful?.</param>
	public void UserAuthenticateResponse(bool success) {

		if(!success)
			return;

		// Open map; this may be something different later
		// if(phaseOne)
		// 	NotebookManager.Instance.OpenMap();

		Debug.Log("Player auth successful? " + success);

	}

	/// <summary>
	/// Obtains game config data and passes it to global data manager
	/// </summary>
	private void LoadGameConfig()
	{
		// Open stream to API JSON config file
		TextAsset apiJson = (TextAsset)Resources.Load("api", typeof(TextAsset));
		StringReader strConfigData = new StringReader(apiJson.text);

		// Set in data manager class with chosen environment config
		DataManager.SetGameConfig(strConfigData.ReadToEnd(), environment);


	    #if UNITY_EDITOR
			DataManager.tutorialEnabled = tutorialEnabled;
		#else
			DataManager.tutorialEnabled = true;
		#endif

		strConfigData.Close();
	}

	/// <summary>
	/// Obtains and sets global game data
	/// </summary>
	private void SetGameData() {

		string gameData = null;

		// This should live in a static global dictionary somewhere
		// Try to get data from API remote
		try {

			gameData = NetworkManager.Instance.DownloadDataFromURL("/gameData");

		}
		// Fallback: load game data from local config
		catch(System.Exception e) {

			// If in editor, always throw so we catch issues
			#if UNITY_EDITOR
				throw new System.Exception("Unable to obtain game data due to error '" + e + "'");
			#endif
 
	        TextAsset dataJson = (TextAsset)Resources.Load("data", typeof(TextAsset));
			StringReader strData = new StringReader(dataJson.text);
	        
			gameData = strData.ReadToEnd();

			strData.Close();
		
		}

		// Set global game data
		if(gameData != null && gameData.Length > 0)	
			DataManager.SetGameData(gameData);

	}
}
