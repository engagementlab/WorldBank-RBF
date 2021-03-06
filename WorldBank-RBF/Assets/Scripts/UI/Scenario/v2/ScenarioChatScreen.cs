﻿using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class ScenarioChatScreen : ChatScreen {

	Models.ScenarioCard _data;

    /// <summary>
    /// Set the data for this card
    /// </summary>
    public Models.ScenarioCard Data {
        set {

        	_data = value;

        	Initialize();

        }
    }

    public Transform advisorsContainer;
    public Transform actionsContainer;
    public Text contactsTitleText;
    public Text debugText;

    public Animator supervisorChatTabAnimator;

    List<string> previousAdvisorOptions;
    List<string> currentAdvisorOptions;
	List<string> currentCardOptions;

	List<string> allCardOptions;
	List<string> allCardAffects;

	List<AdvisorButton> btnListAdvisors = new List<AdvisorButton>();

	// Player may not use more than 3 advisors per card
	int advisorsUseLimit = 3;
	int advisorsUsed = 0;

	bool cardQueued;

	void Start() {

		Events.instance.AddListener<ScenarioEvent> (OnScenarioEvent);

	}

 	void OnEnable() {

 		if(cardQueued) {
	 		Initialize();
	 		cardQueued = false;
 		}

 	}

    void Initialize () {

    	if(!gameObject.activeSelf) {

			// Show LED if this tab not selected
			if(tabAnimator.GetComponent<Button>().interactable)
				tabAnimator.Play("ScenarioTabAlert");
				
    		cardQueued = true;
    		return;
    	}

    	// Get initial character info
		Models.Character charRef = DataManager.GetDataForCharacter(_data.initiating_npc);

		// Generate advisors		
		currentAdvisorOptions = _data.characters.Select(x => x.Key).ToList();
		btnListAdvisors.Clear();

		// Render advisors
		ObjectPool.DestroyChildren<AdvisorButton>(advisorsContainer);
		AddAdvisors();
		
		// Generate starting options
		currentCardOptions = new List<string>(_data.starting_options);
		allCardOptions = currentCardOptions.Concat(new List<string>(_data.final_options)).ToList();
		allCardAffects = new List<string>(_data.starting_options_affects).Concat(new List<string>(_data.final_options_affects)).ToList();

		actionsContainer.gameObject.SetActive(true);

		// Create buttons for all options if not speaking to advisor
		AddOptions(currentCardOptions, null, true);

		if (gameObject.activeSelf)
			AddResponseSpeech(_data.initiating_dialogue, charRef, true);

		// Reset of advisors used and make advisors container interactable
		advisorsUsed = 0;
		advisorsContainer.GetComponent<CanvasGroup>().interactable = true;
		advisorsContainer.GetComponent<CanvasGroup>().alpha = 1;

		contactsTitleText.text = "Contacts (" + advisorsUsed + "/3)";

    }

    void RemoveAdvisor(string npcSymbol) {
    		
		Models.Character charRef = DataManager.GetDataForCharacter(npcSymbol);

		if(btnListAdvisors.FirstOrDefault (x => x.NPCName == charRef.display_name) == null)
			return;
		
		AdvisorButton btnChoice = btnListAdvisors.FirstOrDefault (x => x.NPCName == charRef.display_name);

		btnListAdvisors.Remove (btnChoice);

		currentAdvisorOptions.Remove(npcSymbol);

		btnChoice.Hide ();

    }

    public void AddAdvisors() {

    	// Remove initiator from advisors if npc does not have dialogue
    	if(!_data.characters.Keys.Contains(_data.initiating_npc) || !_data.characters[_data.initiating_npc].hasDialogue)
	    	currentAdvisorOptions.Remove(_data.initiating_npc);

    	foreach (string characterSymbol in currentAdvisorOptions) {
    		
    		// Show an advisor option only if they have dialogue (not for feedback only)
			if(!_data.characters[characterSymbol].hasDialogue)
				continue;

			string npcDialogue = _data.characters[characterSymbol].dialogue;

			AdvisorButton btnChoice = ObjectPool.Instantiate<AdvisorButton>("Scenario");
			btnChoice.Show ();

			Models.Character charRef = DataManager.GetDataForCharacter(characterSymbol);
			
			btnChoice.NPCName = charRef.display_name;
			btnChoice.NPCSymbol = charRef.symbol;

			btnChoice.Button.onClick.RemoveAllListeners();
			btnChoice.Button.onClick.AddListener (() => StartCoroutine("AdvisorSelected", charRef.symbol));

			btnChoice.gameObject.SetActive(true);
			btnListAdvisors.Add(btnChoice);

			AddButton<AdvisorButton> (btnChoice, advisorsContainer);

    	}

	}

	public override void Clear () {

		base.Clear();

    	// Disable advisors
		advisorsContainer.GetComponent<CanvasGroup>().interactable = false;
		advisorsContainer.GetComponent<CanvasGroup>().alpha = .4f;

	}

	public void NoMessages() {
		
		Clear();

		AddSystemMessage("No messages.");

	}

	public void NoActionsTaken() {
		
		Clear();

		AddSystemMessage(DataManager.GetUIText("copy_no_action_taken_problem"));
	
		ChatAction nextCardAction = new ChatAction();

		UnityAction nextCard = (() => Events.instance.Raise(new ScenarioEvent(ScenarioEvent.NEXT, null)));
		nextCardAction.action = nextCard;

		RemoveOptions();
		AddOptions (
			new List<string> { "Confirm Feedback" },
			new List<ChatAction> { nextCardAction }
		);

	}

	IEnumerator AdvisorSelected(string strAdvisorSymbol) {

		// Disable supervisor tab
		supervisorChatTabAnimator.Play("SupervisorTabOff");
		supervisorChatTabAnimator.gameObject.GetComponent<Button>().enabled = false;

		DisableAdvisors();
	    		
		AddSystemMessage("...");

		yield return new WaitForSeconds(1);

		ObjectPool.Destroy<SystemMessage>(messagesContainer.transform.GetChild(messagesContainer.transform.childCount-1));

		previousAdvisorOptions = currentAdvisorOptions.ToList ();

		Models.Advisor advisor = _data.characters[strAdvisorSymbol];
		if(advisor.narrowsNpcs)
		{
			foreach(string npc_symbol in advisor.narrows)
				currentAdvisorOptions.Remove(npc_symbol);

		}

		if(advisor.unlocks != null)
		{
			foreach(string option in advisor.unlocks)
			{
				if(!currentCardOptions.Contains(option))
					currentCardOptions.Add(option);
			}
		}

		if(advisor.dialogue != null && gameObject.activeSelf) {
			AddResponseSpeech(advisor.dialogue, DataManager.GetDataForCharacter(strAdvisorSymbol));

			// SFX
			AudioManager.Sfx.Play ("recievemessage", "Phase2");
		}

		RemoveAdvisor(strAdvisorSymbol);

		// Create buttons for all options if not speaking to advisor
		AddOptions(currentCardOptions);

		advisorsUsed++;

		contactsTitleText.text = "Contacts (" + advisorsUsed + "/3)";

		// Disable advisor container if player used their limit for this card
		if(advisorsUsed == advisorsUseLimit) 
			DisableAdvisors();
		else
			EnableAdvisors();

		// SFX
		AudioManager.Sfx.Play ("addtodiscussion", "Phase2");

		// Tutorial
		DialogManager.instance.CreateTutorialScreen("phase_2_conference_action");

		// Disable supervisor tab
		supervisorChatTabAnimator.Play("SupervisorTabOn");
		supervisorChatTabAnimator.gameObject.GetComponent<Button>().enabled = true;
	}

	IEnumerator ShowFeedback(string eventSymbol)
	{
			
		Clear();

		// Disable supervisor tab
		supervisorChatTabAnimator.Play("SupervisorTabOff");
		supervisorChatTabAnimator.gameObject.GetComponent<Button>().enabled = false;

	    DialogManager.instance.RemoveTutorialScreen();
	    
		yield return new WaitForSeconds(1f);

		AddSystemMessage(DataManager.GetUIText("copy_waiting_for_feedback"));

		yield return new WaitForSeconds(3f);
			
		Clear();

		KeyValuePair<string, Models.Advisor> npc = _data.characters.Where(d => d.Value.hasFeedback && d.Value.feedback.ContainsKey(eventSymbol)).
							 ToDictionary(d => d.Key, d => d.Value).FirstOrDefault();

		// Do we have an NPC with this feedback?
		if(!npc.Equals(default(KeyValuePair<string, Models.Advisor>))) {

			ChatAction nextCardAction = new ChatAction();

			UnityAction nextCard = (() => Events.instance.Raise(new ScenarioEvent(ScenarioEvent.NEXT, eventSymbol)));
			nextCardAction.action = nextCard;

			RemoveOptions();
			AddOptions (
				new List<string> { "Confirm Feedback" },
				new List<ChatAction> { nextCardAction }
			);

			Dictionary<string, int> dictAffect = DataManager.GetIndicatorBySymbol(eventSymbol);

			string feedback = npc.Value.feedback[eventSymbol].ToString();
			
			AddResponseSpeech(feedback, DataManager.GetDataForCharacter(npc.Key), false, true, dictAffect);

			IndicatorsCanvas.SelectedOption = new KeyValuePair<string, int[]>(DataManager.GetUnlockableBySymbol(eventSymbol).title, dictAffect.Values.ToArray());
			
			// SFX
			AudioManager.Sfx.Play ("planconfirm", "UI");

			// Tutorial
			DialogManager.instance.CreateTutorialScreen("phase_2_feedback");

		}
		// Error
		else 
			throw new Exception("No feedback found for '" + eventSymbol + "' in '" + _data.symbol + "'!!");

	}

	void DisableAdvisors() {

		advisorsContainer.GetComponent<CanvasGroup>().interactable = false;
		advisorsContainer.GetComponent<CanvasGroup>().alpha = .4f;

	}

	void EnableAdvisors() {

		advisorsContainer.GetComponent<CanvasGroup>().interactable = true;
		advisorsContainer.GetComponent<CanvasGroup>().alpha = 1;
		
	}

	void OnScenarioEvent (ScenarioEvent e) {

		if(e.eventType == "feedback") {

			StartCoroutine("ShowFeedback", e.eventSymbol);

		}
		else if (e.eventType == "next_year" && !panelOpen) {

			advisorsPanel.Play ("Opened");
			panelOpen = true;
		
		}

	}
}
